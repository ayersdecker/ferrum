# Ferrum Architecture

## Overview

Ferrum is a domain-agnostic native-compute framework for .NET MAUI. It provides
the scaffolding that lets any C/C++ static library be called from a MAUI app
with zero-copy buffer passing, NativeAOT-compatible P/Invoke, and a
code-generation tool that automates the binding layer.

Ferrum does **not** ship a native core of its own. It provides the plumbing;
the consuming application provides the native library.

---

## Component Map

```
┌───────────────────────────────────────────────────────────────────────┐
│  Consumer MAUI App                                                    │
│  ┌─────────────────────────────────────────────────────────────────┐  │
│  │  Application code (C#)                                          │  │
│  │  Uses NativeBuffer<T> for zero-copy buffer passing              │  │
│  │  Calls generated [LibraryImport] bindings                       │  │
│  └────────────────────────────┬────────────────────────────────────┘  │
│                               │ P/Invoke (source-generated, AOT-safe) │
│  ┌────────────────────────────▼────────────────────────────────────┐  │
│  │  Ferrum.Framework  (src/Framework/)                             │  │
│  │  • NativeBuffer<T>        — pinned Span<T>/Memory<T> bridge     │  │
│  │  • InteropConstants       — platform-aware library name         │  │
│  └────────────────────────────┬────────────────────────────────────┘  │
│                               │ [LibraryImport] / __Internal (iOS)    │
│  ┌────────────────────────────▼────────────────────────────────────┐  │
│  │  Consumer Native Library  (any C/C++17 static lib)              │  │
│  │  Built by CMake templates in  native/                           │  │
│  └─────────────────────────────────────────────────────────────────┘  │
└───────────────────────────────────────────────────────────────────────┘

  ┌────────────────────────────────────────────────────────────────────┐
  │  Offline tools                                                     │
  │  ferrum-codegen  (tools/codegen/)                                  │
  │  Reads:  consumer's .h file                                        │
  │  Writes: [LibraryImport] .cs bindings + blittable struct defs      │
  └────────────────────────────────────────────────────────────────────┘
```

---

## Native Build Layer (`native/`)

### CMake structure

| File / directory | Purpose |
|---|---|
| `native/CMakeLists.txt` | Top-level project; includes helpers, optionally builds the test fixtures |
| `native/cmake/ferrum_helpers.cmake` | `ferrum_add_static_library()` macro that sets common flags |
| `native/cmake/ios.toolchain.cmake` | Minimal iOS cross-compilation toolchain (no external deps) |
| `native/test_stub/` | Trivial `ferrum_add(int, int)` C library used to validate the pipeline |
| `native/dsp_fixture/` | `ferrum_dsp_scale(float*, int32_t, float)` + `ferrum_dsp_stats(const float*, int32_t, FerrumDspStats*)` — validates the float-buffer and struct-out-param patterns |
| `native/scripts/build_ios.sh` | Builds all three iOS slices and packages them as an XCFramework |
| `native/scripts/build_android.sh` | Builds `.so` for all four Android ABIs using the NDK toolchain |

### iOS

iOS builds use CMake's built-in iOS support (`CMAKE_SYSTEM_NAME=iOS`) with
our thin toolchain wrapper. Three separate CMake configurations are produced:

- `OS64` — arm64 device library
- `SIMULATORARM64` — arm64 simulator library (Apple Silicon Macs)
- `SIMULATOR64` — x86_64 simulator library (Intel Macs)

The two simulator slices are fat-lipo'd together, then both the device slice
and the fat simulator slice are assembled into an XCFramework using
`xcodebuild -create-xcframework`.

The resulting `.xcframework` is added as a `<NativeReference>` in the MAUI
`.csproj` file.

### Android

Android builds use the NDK-bundled `android.toolchain.cmake`. We target
`android-24` (API 24, Android 7.0) across four ABIs:
`arm64-v8a`, `armeabi-v7a`, `x86_64`, `x86`.

The resulting `.so` files are placed under `artifacts/android/jniLibs/<abi>/`
and referenced as `<AndroidNativeLibrary>` in the MAUI `.csproj`.

---

## Interop Layer (`src/Framework/`)

### `NativeBuffer<T>`

A zero-copy, pinned buffer abstraction. The generic constraint `where T : unmanaged`
is enforced at compile time to guarantee blittability. No reflection is involved.

```csharp
// Typical usage
float[] audio = new float[4096];
using var buf = new NativeBuffer<float>(audio.AsMemory());
MyNativeLib.ProcessAudio(buf.Pointer, buf.Length);
// Pin is released on Dispose — GC can move the array again
```

Key design points:
- `MemoryHandle.Pin()` keeps the underlying memory at a fixed address.
- `Pointer` returns `void*`; `TypedPointer` returns `T*` for typed access.
- `Dispose()` releases the pin; using the buffer after disposal throws `ObjectDisposedException`.
- Allocation is explicit (`new NativeBuffer<T>(length)`) or wrap-existing
  (`new NativeBuffer<T>(Memory<T>)`).

### `InteropConstants`

Provides the `FerrumLibrary` constant used as the first argument to `[LibraryImport]`:

- iOS / Mac Catalyst: `"__Internal"` (NativeAOT static linkage — the symbol is
  resolved inside the current process, no dynamic loading)
- Android + all other platforms: `"ferrum_test_stub"` (resolved via the
  platform's native library loader at runtime)

---

## Codegen Tool (`tools/codegen/`)

`ferrum-codegen` parses a C header file and emits ready-to-use
`[LibraryImport]` bindings and `[StructLayout(LayoutKind.Sequential)]` struct
definitions.

### Design principles

1. **Fail loudly, never silently.** Any construct that cannot be safely bound
   (function pointers, `char*`, platform-ambiguous `long`, array declarators)
   causes an immediate error with a human-readable message pointing to the
   source line. No incorrect bindings are ever emitted.

2. **NativeAOT-first.** The generated code uses `[LibraryImport]`
   (source-generated at build time) instead of `[DllImport]` (which uses
   reflection at runtime). No `MarshalAs` attributes are emitted.

3. **Simple, dependency-free parser.** The parser is a strict tokenizer —
   not a full C frontend. It handles the subset of C headers needed for
   P/Invoke: function prototypes and POD structs. Anything it does not
   recognise is rejected immediately (see constraint 1). A full libclang
   integration was considered and rejected: it would add ~40–50 MB of native
   binary dependencies to the dotnet tool package without materially improving
   correctness within the blittable-only constraint. See
   [open-questions.md](open-questions.md) item 5 for the full rationale.

### Supported C types

| C type | C# type | Notes |
|---|---|---|
| `void` | `void` | |
| `bool` / `_Bool` | `byte` | 1 = true, 0 = false |
| `char` | `byte` | Raw byte, not a string |
| `int8_t` / `signed char` | `sbyte` | |
| `uint8_t` / `unsigned char` | `byte` | |
| `int16_t` / `short` | `short` | |
| `uint16_t` / `unsigned short` | `ushort` | |
| `int32_t` / `int` | `int` | |
| `uint32_t` / `unsigned int` | `uint` | |
| `int64_t` / `long long` | `long` | |
| `uint64_t` / `unsigned long long` | `ulong` | |
| `float` | `float` | |
| `double` | `double` | |
| `void*` | `void*` | Requires `unsafe` context |
| `T*` (blittable) | `T*` | Pointer to any supported primitive |
| `size_t` | `nuint` | |
| `ptrdiff_t` / `intptr_t` | `nint` | |
| `long` | **ERROR** | Platform-dependent size — use `int32_t`/`int64_t` |
| `char*` | **ERROR** | Non-blittable — use `byte*` with explicit encoding |
| Function pointers | **ERROR** | Bind manually with `[UnmanagedFunctionPointer]` |

---

## NativeAOT Compatibility

Every component is designed to work with iOS NativeAOT:

- `[LibraryImport]` is resolved at build time by the Roslyn source generator;
  no reflection is needed at runtime.
- `NativeBuffer<T>` uses only value-type generics and `System.Runtime.InteropServices`
  APIs that are NativeAOT-safe.
- The codegen tool generates code that contains no `MarshalAs`, no `Type.GetType()`,
  no dynamic invocation.
- The `Ferrum.Framework.csproj` sets `<UseInterpreter>false</UseInterpreter>` on
  iOS to catch any inadvertent use of the interpreter during CI.

---

## Scope Rules

Ferrum is a **framework**, not an application. Contributions must be
domain-agnostic. If a change would only be useful to one specific native
library or application, it belongs in the consuming project, not here.

Indicators of scope creep (flag these in code review):
- A function or type name references a specific application domain
  (audio, video, ML model name, etc.)
- The codegen tool learns about specific library ABIs by name
- Any native code beyond the trivial test fixtures is added to `native/`

The in-tree test fixtures (`test_stub/`, `dsp_fixture/`) are explicitly exempt
from the scope rule — they exist solely to prove the framework plumbing works
with different parameter patterns, not to implement any application logic.

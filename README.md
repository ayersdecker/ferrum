# Ferrum

> **C/C++ to C# — from UI to the bare metal, get it done.**

Ferrum is a reusable native-compute framework for .NET MAUI. It provides the
scaffolding that lets any MAUI app call into a C/C++ static library with:

- **Zero-copy buffers** — `NativeBuffer<T>` pins a `Span<T>`/`Memory<T>` region
  so you can pass a pointer directly to native code without copying.
- **NativeAOT-compatible P/Invoke** — every binding uses `[LibraryImport]`
  (source-generated), not `[DllImport]`. iOS NativeAOT is a first-class target.
- **Code-generation tool** — `ferrum-codegen` parses a C header and emits
  ready-to-use `[LibraryImport]` bindings. It fails loudly (never silently) on
  constructs it cannot safely bind.
- **Cross-platform CMake templates** — build an XCFramework for iOS or `.so`
  per ABI for Android from any C/C++17 source tree.

Ferrum is **domain-agnostic** — it does not know or care what your native core
does. Audio, ML inference, signal processing, computer vision — all equally
welcome.

---

## Repository Layout

```
/native/                CMake project: iOS/Android toolchain files, test stub
/tools/codegen/         ferrum-codegen — header-to-[LibraryImport] generator
/src/Framework/         Ferrum.Framework C# library (NativeBuffer<T>, etc.)
/src/Framework.Tests/   xUnit tests for the framework and codegen
/samples/MinimalDemo/   End-to-end MAUI sample calling ferrum_add(int, int)
/docs/                  Architecture notes, getting-started guide, open questions
.github/workflows/      CI: native build (iOS/Android), codegen tests, MAUI build
```

---

## Quick Start

### 1. Build the native test stub

```bash
# iOS (requires macOS + Xcode)
./native/scripts/build_ios.sh

# Android (requires NDK — set ANDROID_NDK_HOME)
./native/scripts/build_android.sh
```

### 2. Generate bindings for your header

```bash
dotnet tool install --global Ferrum.Codegen

ferrum-codegen \
  --input  mylib/include/mylib.h \
  --output src/MyApp/Interop/MylibBindings.cs \
  --lib    __Internal
```

### 3. Call native code with zero-copy buffers

```csharp
using Ferrum.Framework.Buffers;
using MyApp.Interop;

using var buf = new NativeBuffer<float>(1024);
for (int i = 0; i < buf.Length; i++)
    buf.Span[i] = (float)i;

unsafe { MylibBindings.process(buf.TypedPointer, buf.Length); }
```

See [docs/getting-started.md](docs/getting-started.md) for the full walkthrough.

---

## Design Constraints

| Constraint | Detail |
|---|---|
| No domain logic | This repo is framework-only. Application-specific code is scope creep. |
| NativeAOT on iOS | No runtime codegen, no reflection-based marshalling. |
| Loud failures | The codegen tool exits non-zero rather than emitting an incorrect binding. |
| Blittable only | Only types with a 1:1 memory layout between C and C# are supported. |

---

## Open Questions

Several architectural decisions are not yet finalised. See
[docs/open-questions.md](docs/open-questions.md) for the full list, including:

- NuGet package vs. source template (or both)
- Windows / macOS desktop MAUI targets in v1
- libclang-based vs. regex-based codegen parser

---

## License

[MIT](LICENSE) © 2026 Decker Ayers

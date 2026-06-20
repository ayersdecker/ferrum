# Ferrum

[![NuGet](https://img.shields.io/nuget/v/Ferrum.Framework?label=Ferrum.Framework)](https://www.nuget.org/packages/Ferrum.Framework/)
[![NuGet](https://img.shields.io/nuget/v/Ferrum.Codegen?label=Ferrum.Codegen)](https://www.nuget.org/packages/Ferrum.Codegen/)
[![Build Status](https://github.com/ayersdecker/ferrum/actions/workflows/codegen-tests.yml/badge.svg)](https://github.com/ayersdecker/ferrum/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

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
/native/                CMake project: iOS/Android toolchain files, test fixtures
/tools/codegen/         ferrum-codegen — header-to-[LibraryImport] generator
/src/Framework/         Ferrum.Framework C# library (NativeBuffer<T>, etc.)
/src/Framework.Tests/   xUnit tests for the framework and codegen
/samples/MinimalDemo/   End-to-end MAUI sample calling native test functions
/templates/maui-ferrum/ dotnet new template for scaffolding new Ferrum apps
/docs/                  Architecture notes, getting-started guide, open questions
.github/workflows/      CI: native build (iOS/Android), codegen tests, MAUI build
```

---

## Quick Start

### Option 1: Use the Project Template (Fastest Way to Start)

Create a new Ferrum-enabled MAUI app in seconds:

```bash
# Install the template (from local repo)
dotnet new install templates/maui-ferrum

# Create a new project
dotnet new maui-ferrum -n MyApp
cd MyApp

# See README.md in the generated project for next steps
```

The template creates a complete MAUI project with Ferrum.Framework already configured and example `NativeBuffer<T>` usage.

### Option 2: Use the Sample (Recommended for First-Time Users)

Clone this repository and run the MinimalDemo sample:

```bash
git clone https://github.com/ayersdecker/ferrum.git
cd ferrum

# Build native test fixtures for your target platform
# iOS (requires macOS + Xcode):
./native/scripts/build_ios.sh

# Android (requires NDK — set ANDROID_NDK_HOME):
export ANDROID_NDK_HOME=/path/to/ndk
./native/scripts/build_android.sh

# Open and run the sample
cd samples/MinimalDemo
dotnet build -f net9.0-android  # or net9.0-ios
```

See [samples/MinimalDemo/README.md](samples/MinimalDemo/README.md) for details.

### Option 3: Add to Your Existing MAUI App

#### 1. Install the NuGet package

```bash
dotnet add package Ferrum.Framework
dotnet tool install --global Ferrum.Codegen
```

#### 2. Build your native library

Create your C/C++ library with a plain-C API header:

```c
// mylib/include/mylib.h
#ifdef __cplusplus
extern "C" {
#endif

void process_samples(float* data, int count);

#ifdef __cplusplus
}
#endif
```

Build for iOS and Android using the provided CMake scripts (see [docs/getting-started.md](docs/getting-started.md)).

#### 3. Generate C# bindings

```bash
ferrum-codegen \
  --input  mylib/include/mylib.h \
  --output Interop/MylibBindings.cs \
  --ns     MyApp.Interop \
  --lib    __Internal
```

#### 4. Call native code with zero-copy buffers

```csharp
using Ferrum.Framework.Buffers;
using MyApp.Interop;

using var buffer = new NativeBuffer<float>(1024);
for (int i = 0; i < buffer.Length; i++)
    buffer.Span[i] = (float)i * 0.5f;

unsafe 
{ 
    MylibBindings.process_samples(buffer.TypedPointer, buffer.Length); 
}

// Read results back from buffer.Span
```

📖 **Full Guide:** [docs/getting-started.md](docs/getting-started.md)  
🏗️ **Architecture:** [docs/architecture.md](docs/architecture.md)

---

## Design Constraints

| Constraint | Detail |
|---|---|
| No domain logic | This repo is framework-only. Application-specific code is scope creep. |
| NativeAOT on iOS | No runtime codegen, no reflection-based marshalling. |
| Loud failures | The codegen tool exits non-zero rather than emitting an incorrect binding. |
| Blittable only | Only types with a 1:1 memory layout between C and C# are supported. |

---

## Troubleshooting

### iOS: `dllimport not found` or `__Internal` issues

**Problem:** The app crashes with `DllNotFoundException` when calling native code on iOS.

**Solution:** Ensure your native library is built as an XCFramework and properly referenced:

```bash
./native/scripts/build_ios.sh
```

Then add to your `.csproj`:

```xml
<ItemGroup Condition="$([MSBuild]::GetTargetFrameworkIdentifier('$(TargetFramework)')) == 'iOS'">
  <NativeReference Include="path/to/libmylib.xcframework">
    <Kind>Framework</Kind>
    <SmartLink>true</SmartLink>
  </NativeReference>
</ItemGroup>
```

Use `--lib __Internal` in ferrum-codegen for iOS.

### Android: Native library not loaded

**Problem:** `DllNotFoundException: libmylib` on Android.

**Solution:** Build for all required ABIs and include them in your project:

```bash
export ANDROID_NDK_HOME=/path/to/ndk
./native/scripts/build_android.sh
```

Add to `.csproj`:

```xml
<ItemGroup Condition="$([MSBuild]::GetTargetFrameworkIdentifier('$(TargetFramework)')) == 'Android'">
  <AndroidNativeLibrary Include="artifacts/android/jniLibs/arm64-v8a/libmylib.so" />
  <AndroidNativeLibrary Include="artifacts/android/jniLibs/armeabi-v7a/libmylib.so" />
  <AndroidNativeLibrary Include="artifacts/android/jniLibs/x86_64/libmylib.so" />
</ItemGroup>
```

Use `--lib libmylib` (without `.so`) in ferrum-codegen for Android.

### Codegen fails with "Unsupported type"

**Problem:** `ferrum-codegen` exits with an error about unsupported types.

**Solution:** Check that your header only uses [blittable types](docs/architecture.md):
- ✅ `int32_t`, `int64_t`, `float`, `double`, pointers to structs
- ❌ `char*`, `long`, function pointers, unions

### NativeBuffer AccessViolation

**Problem:** Crash when accessing `NativeBuffer.Span` after disposal.

**Solution:** Keep the `NativeBuffer` alive while native code is using it:

```csharp
using var buffer = new NativeBuffer<float>(1024);
unsafe { MyFunc(buffer.TypedPointer, buffer.Length); }
// Native call completes before 'using' disposes buffer
```

For more issues, see [GitHub Issues](https://github.com/ayersdecker/ferrum/issues) or [docs/getting-started.md](docs/getting-started.md).

---

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for:
- How to build and test the framework
- Code style guidelines
- PR submission process

---

## Roadmap

- [x] NuGet package publication (`Ferrum.Framework` and `Ferrum.Codegen` dotnet tool)
- [x] `dotnet new` project template for quick scaffolding (`maui-ferrum`)
- [x] Codegen parser decision (tokenizer-based, fails loudly on unsupported constructs)
- [ ] Windows and macOS desktop MAUI support
- [ ] Community samples (audio processing, ML inference, computer vision)

See [docs/open-questions.md](docs/open-questions.md) for detailed discussion on pending decisions.

---

## License

[MIT](LICENSE) © 2026 Decker Ayers

# Ferrum MAUI App

This project was created from the **Ferrum MAUI App** template. It's preconfigured with the Ferrum.Framework for native C/C++ interop on iOS and Android.

## What's Included

- ✅ **Ferrum.Framework** NuGet package reference
- ✅ NativeAOT enabled for iOS
- ✅ AllowUnsafeBlocks for P/Invoke
- ✅ Example `NativeBuffer<T>` usage in `MainPage.xaml.cs`
- ✅ Placeholder for native library references in `.csproj`

## Next Steps

### 1. Create Your Native Library

Write your C/C++ library with a plain-C API. Example:

```c
// mylib/include/mylib.h
#ifdef __cplusplus
extern "C" {
#endif

void process_data(float* samples, int count);

#ifdef __cplusplus
}
#endif
```

See the [Ferrum documentation](https://github.com/ayersdecker/ferrum/blob/main/docs/getting-started.md) for CMake build scripts and platform setup.

### 2. Build Your Native Library

**iOS (macOS + Xcode):**
```bash
# Use Ferrum's CMake iOS toolchain or your own build system
# Output: libmylib.xcframework
```

**Android (NDK):**
```bash
export ANDROID_NDK_HOME=/path/to/ndk
# Use Ferrum's CMake Android toolchain or your own build system
# Output: libmylib.so for each ABI (arm64-v8a, armeabi-v7a, x86_64)
```

### 3. Generate C# Bindings

Install the Ferrum code generator:

```bash
dotnet tool install --global Ferrum.Codegen
```

Generate bindings from your header:

```bash
ferrum-codegen \
  --input mylib/include/mylib.h \
  --output Interop/MylibBindings.cs \
  --ns FerrumApp.Interop \
  --lib __Internal  # or libmylib for Android
```

### 4. Add Native Library to Project

Edit `FerrumApp.csproj` and uncomment/adjust the native library references:

**iOS:**
```xml
<ItemGroup Condition="$([MSBuild]::GetTargetFrameworkIdentifier('$(TargetFramework)')) == 'iOS'">
  <NativeReference Include="native/ios/libmylib.xcframework">
	<Kind>Framework</Kind>
	<SmartLink>true</SmartLink>
  </NativeReference>
</ItemGroup>
```

**Android:**
```xml
<ItemGroup Condition="$([MSBuild]::GetTargetFrameworkIdentifier('$(TargetFramework)')) == 'Android'">
  <AndroidNativeLibrary Include="native/android/jniLibs/arm64-v8a/libmylib.so" />
  <AndroidNativeLibrary Include="native/android/jniLibs/armeabi-v7a/libmylib.so" />
  <AndroidNativeLibrary Include="native/android/jniLibs/x86_64/libmylib.so" />
</ItemGroup>
```

### 5. Call Your Native Code

In `MainPage.xaml.cs` (or anywhere in your app):

```csharp
using Ferrum.Framework.Buffers;
using FerrumApp.Interop;

using var buffer = new NativeBuffer<float>(1024);

// Fill with data
for (int i = 0; i < buffer.Length; i++)
	buffer.Span[i] = (float)i;

// Call native code (zero-copy)
unsafe 
{ 
	MylibBindings.process_data(buffer.TypedPointer, buffer.Length); 
}

// Read results from buffer.Span
```

## Building and Running

```bash
# Android
dotnet build -f net9.0-android
dotnet build -f net9.0-android -t:Run

# iOS (macOS only)
dotnet build -f net9.0-ios
dotnet build -f net9.0-ios -t:Run
```

## Learn More

- 📚 [Ferrum Documentation](https://github.com/ayersdecker/ferrum/blob/main/README.md)
- 🏗️ [Architecture Guide](https://github.com/ayersdecker/ferrum/blob/main/docs/architecture.md)
- 🚀 [Getting Started Tutorial](https://github.com/ayersdecker/ferrum/blob/main/docs/getting-started.md)
- 💬 [GitHub Discussions](https://github.com/ayersdecker/ferrum/discussions)
- 🐛 [Report Issues](https://github.com/ayersdecker/ferrum/issues)

## Troubleshooting

See the main [Ferrum README troubleshooting section](https://github.com/ayersdecker/ferrum/blob/main/README.md#troubleshooting) for common issues with iOS/Android native library loading.

---

**Happy coding with Ferrum!** 🚀

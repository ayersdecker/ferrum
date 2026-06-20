# Quick Fix: Native Library Not Found

## What I Just Fixed

The `AndroidNativeLibrary` items in `MinimalDemo.csproj` were missing the `<Abi>` metadata that tells the Android build system which architecture each `.so` file is for.

### Before:
```xml
<AndroidNativeLibrary Include="...arm64-v8a\libferrum_test_stub.so" />
```

### After:
```xml
<AndroidNativeLibrary Include="...arm64-v8a\libferrum_test_stub.so">
  <Abi>arm64-v8a</Abi>
</AndroidNativeLibrary>
```

This ensures the correct `.so` file is packaged for each device architecture.

---

## How to Rebuild and Test

### Option 1: In Visual Studio (Recommended)

1. **Stop debugging** if you're currently debugging (Shift+F5)
2. **Close Visual Studio** completely
3. **Reopen Visual Studio** and open `Ferrum.sln`
4. **Build** > **Rebuild Solution** (or just rebuild MinimalDemo)
5. **Select an Android emulator** or device
6. **Press F5** to debug
7. **Click "Call Native"** button
8. **Expected**: `ferrum_add(21, 21) = 42`

### Option 2: Using PowerShell Script

I've created a rebuild script for you:

```powershell
# Make sure Visual Studio is closed or debugging is stopped
.\rebuild-minimaldemo.ps1
```

This script will:
- Check for file locks
- Verify native libraries exist
- Clean build directories
- Rebuild for Android
- Verify .so files are included

---

## What Should Happen Now

When you rebuild and deploy:

1. **Build process** should include the `.so` files
2. **APK packaging** should place them in `lib/{abi}/libferrum_test_stub.so`
3. **Runtime** should find and load the library
4. **ferrum_add()** call should succeed
5. **Result** should show: `ferrum_add(21, 21) = 42`

---

## If It Still Doesn't Work

### Check 1: Verify .so files are in APK

After building, check if the libraries are included:

```powershell
# Find the APK
$apk = Get-ChildItem -Path "samples\MinimalDemo\bin\Debug\net9.0-android" -Filter "*.apk" -Recurse | Select-Object -First 1

# List contents (requires Java/Android SDK)
& "C:\Program Files (x86)\Android\android-sdk\build-tools\<version>\aapt" list $apk.FullName | Select-String "libferrum"
```

Should show:
```
lib/arm64-v8a/libferrum_test_stub.so
lib/armeabi-v7a/libferrum_test_stub.so
lib/x86_64/libferrum_test_stub.so
```

### Check 2: Device Architecture Match

Make sure your emulator/device architecture matches one of the built ABIs:

- ✅ arm64-v8a (most modern Android devices/emulators)
- ✅ armeabi-v7a (older ARM devices)
- ✅ x86_64 (some emulators)

In Visual Studio:
- Check the emulator name - it usually indicates the architecture
- Example: "Pixel 5 - API 34 (x86_64)" → uses x86_64

### Check 3: Build Output

Look for these messages in the Build Output window:

```
AndroidAsset: artifacts\android\jniLibs\arm64-v8a\libferrum_test_stub.so
```

If you don't see this, the files aren't being included.

---

## Quick Test

After rebuilding, try this in Visual Studio:

1. **Solution Explorer** → Right-click **MinimalDemo** → **Properties**
2. **Android** → **Advanced** → Check **Supported architectures**
3. Should include: arm64-v8a, armeabi-v7a, x86_64

---

## Summary of Changes

| File | Change | Why |
|------|--------|-----|
| `MinimalDemo.csproj` | Added `<Abi>` metadata to `AndroidNativeLibrary` | Tells Android build which .so goes to which ABI folder |
| `rebuild-minimaldemo.ps1` | Created rebuild script | Easy clean rebuild process |

---

## Next Steps

1. **Close Visual Studio** completely (to release file locks)
2. **Reopen** Visual Studio
3. **Rebuild** MinimalDemo
4. **Deploy** to Android
5. **Test** - Click "Call Native" button

**Expected Result**: `ferrum_add(21, 21) = 42` ✅

If you still see "native library not found", run `.\rebuild-minimaldemo.ps1` and share the output!

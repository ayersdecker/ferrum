# Quick Fix Summary for DllNotFoundException

## Current Situation
- **Exception**: `DllNotFoundException: ferrum_test_stub`
- **Location**: `MainPage.xaml.cs`, line 18
- **Cause**: Native Android libraries (`.so` files) are not built
- **Missing files**:
  - `artifacts/android/jniLibs/arm64-v8a/libferrum_test_stub.so`
  - `artifacts/android/jniLibs/armeabi-v7a/libferrum_test_stub.so`  
  - `artifacts/android/jniLibs/x86_64/libferrum_test_stub.so`

## System Status
- ✅ CMake 4.3.1 installed
- ✅ Build script exists: `native/scripts/build_android.sh`
- ✅ Native source code exists: `native/test_stub/src/add.c`
- ❌ Android NDK not installed
- ❌ Git Bash/WSL not installed (needed to run build script)

## Immediate Solutions (Choose One)

### Option A: Install Prerequisites and Build (Best for long-term)

1. **Install Android NDK**:
   - Open **Visual Studio Installer**
   - Modify your Visual Studio installation
   - Under **Mobile development with .NET** workload:
	 - Check **Android SDK setup (API level 34)**
	 - Check **Android NDK (Side by side)**  
   - Click **Modify** to install

   OR use Android Studio SDK Manager as described in `ANDROID_BUILD_INSTRUCTIONS.md`

2. **Install Git for Windows** (to get bash):
   - Download: https://git-scm.com/download/win
   - Install with default options

3. **Set NDK environment variable** (PowerShell as Administrator):
   ```powershell
   # After NDK is installed via VS/Android Studio
   $ndkPath = "$env:LOCALAPPDATA\Android\Sdk\ndk"
   $latestNdk = Get-ChildItem $ndkPath | Sort-Object Name -Descending | Select-Object -First 1
   [System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $latestNdk.FullName, 'User')
   ```

4. **Build native libraries**:
   ```powershell
   # Restart PowerShell to get new environment variables
   & "C:\Program Files\Git\bin\bash.exe" ./native/scripts/build_android.sh
   ```

5. **Rebuild and run** MinimalDemo

---

### Option B: Quick Workaround (Skip native code temporarily)

Modify `samples/MinimalDemo/MainPage.xaml.cs` to bypass the native call:

```csharp
private void OnCallNativeClicked(object sender, EventArgs e)
{
	// TODO: Build native libraries with Android NDK first
	// See ANDROID_BUILD_INSTRUCTIONS.md for setup guide

	#if ANDROID
	ResultLabel.Text = "Native library not built yet.\n" +
					   "Install Android NDK and run:\n" +
					   "./native/scripts/build_android.sh";
	#else
	int result = AddInterop.ferrum_add(21, 21);
	ResultLabel.Text = $"ferrum_add(21, 21) = {result}";
	#endif
}
```

This allows you to run the app on Android without the native library, while iOS will continue to work (if built).

---

### Option C: Get Pre-built Libraries

If someone else has built the libraries or they exist in CI/CD:

1. Obtain the `.so` files from another developer or build server
2. Create directory: `artifacts/android/jniLibs/arm64-v8a/`
3. Copy `libferrum_test_stub.so` into each ABI folder
4. Rebuild MinimalDemo

---

## Recommended Path Forward

For a complete working solution:

1. ✅ **Read** `ANDROID_BUILD_INSTRUCTIONS.md` (created in your repo root)
2. ⏳ **Install** Android NDK via Visual Studio Installer or Android Studio
3. ⏳ **Install** Git for Windows (for bash shell)
4. ⏳ **Build** native libraries with the build script
5. ✅ **Test** the app - the exception should be resolved

---

## Files Created

- ✅ `ANDROID_BUILD_INSTRUCTIONS.md` - Detailed setup and troubleshooting guide
- ✅ `QUICK_FIX_SUMMARY.md` - This file

---

## Need Help?

If you encounter issues during setup, check the "Troubleshooting" section in `ANDROID_BUILD_INSTRUCTIONS.md`.

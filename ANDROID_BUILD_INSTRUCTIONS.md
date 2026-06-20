# Android Native Library Build Instructions

## Problem
The MinimalDemo Android app is failing with `DllNotFoundException: ferrum_test_stub` because the native shared libraries (`.so` files) have not been built yet.

## Solution

### Option 1: Install Android NDK and Build (Recommended)

#### Step 1: Install Android NDK

1. **Install Android Studio** (if not already installed):
   - Download from: https://developer.android.com/studio
   - Run the installer

2. **Install Android NDK via Android Studio**:
   - Open Android Studio
   - Go to **Tools** > **SDK Manager**
   - Click the **SDK Tools** tab
   - Check **NDK (Side by side)**
   - Check **CMake** (if not already installed)
   - Click **Apply** to download and install

   The NDK will typically be installed to:
   - Windows: `%LOCALAPPDATA%\Android\Sdk\ndk\<version>`
   - macOS: `~/Library/Android/sdk/ndk/<version>`
   - Linux: `~/Android/Sdk/ndk/<version>`

3. **Set Environment Variable** (Windows PowerShell):
   ```powershell
   # Find your NDK version
   $ndkPath = "$env:LOCALAPPDATA\Android\Sdk\ndk"
   $latestNdk = Get-ChildItem $ndkPath | Sort-Object Name -Descending | Select-Object -First 1

   # Set for current session
   $env:ANDROID_NDK_HOME = $latestNdk.FullName

   # Set permanently (requires admin)
   [System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $latestNdk.FullName, 'User')
   ```

#### Step 2: Build Native Libraries

Run the build script from the repository root:

**Windows (PowerShell):**
```powershell
# Ensure NDK is set
$env:ANDROID_NDK_HOME = "C:\Users\<YourUser>\AppData\Local\Android\Sdk\ndk\<version>"

# Run the build script via WSL or Git Bash
wsl bash ./native/scripts/build_android.sh
# OR
"C:\Program Files\Git\bin\bash.exe" ./native/scripts/build_android.sh
```

**macOS/Linux:**
```bash
./native/scripts/build_android.sh
```

This will build the native libraries for all Android ABIs (arm64-v8a, armeabi-v7a, x86_64, x86) and place them in:
```
artifacts/android/jniLibs/arm64-v8a/libferrum_test_stub.so
artifacts/android/jniLibs/armeabi-v7a/libferrum_test_stub.so
artifacts/android/jniLibs/x86_64/libferrum_test_stub.so
artifacts/android/jniLibs/x86/libferrum_test_stub.so
```

#### Step 3: Rebuild and Run

After the native libraries are built:

1. **Clean and rebuild** the MinimalDemo project in Visual Studio
2. **Deploy** to your Android device/emulator
3. The `ferrum_add()` call should now work correctly

---

### Option 2: Pre-built Libraries (Quick Test)

If you have pre-built `.so` files from another machine or CI/CD, you can manually place them in:

```
artifacts/android/jniLibs/<abi>/libferrum_test_stub.so
```

Where `<abi>` is one of: `arm64-v8a`, `armeabi-v7a`, `x86_64`, `x86`

Then rebuild the MinimalDemo project.

---

### Option 3: Skip Native Code (Temporary Workaround)

To temporarily bypass this error for testing .NET MAUI functionality:

1. Comment out the native call in `samples/MinimalDemo/MainPage.xaml.cs`:
   ```csharp
   private void OnCallNativeClicked(object sender, EventArgs e)
   {
	   // int result = AddInterop.ferrum_add(21, 21);
	   // ResultLabel.Text = $"ferrum_add(21, 21) = {result}";
	   ResultLabel.Text = "Native call disabled - install Android NDK to enable";
   }
   ```

This is only for testing the MAUI app structure and should not be used as a permanent solution.

---

## Verification

After building, verify the libraries exist:

```powershell
Get-ChildItem -Path .\artifacts\android\jniLibs -Recurse -Filter *.so
```

You should see 4 `.so` files (one per ABI).

---

## Troubleshooting

### "bash: command not found" on Windows

You need a bash shell. Options:
1. Install Git for Windows (includes Git Bash): https://git-scm.com/download/win
2. Install WSL (Windows Subsystem for Linux)
3. Use the Android NDK's `ndk-build` command directly (advanced)

### "Android NDK not found" error from build script

Ensure `ANDROID_NDK_HOME` or `NDK_HOME` environment variable points to your NDK installation:

```powershell
# Check current value
$env:ANDROID_NDK_HOME

# Set it if empty
$env:ANDROID_NDK_HOME = "C:\Users\<YourUser>\AppData\Local\Android\Sdk\ndk\<version>"
```

### CMake errors

Ensure CMake 3.21+ is installed:
```powershell
cmake --version
```

If not installed, download from: https://cmake.org/download/

---

## Next Steps

After fixing this issue, you should:

1. ✅ Build native libraries (one-time setup)
2. ✅ Verify `.so` files exist in `artifacts/android/jniLibs/`
3. ✅ Rebuild MinimalDemo
4. ✅ Test the app on Android device/emulator
5. ✅ Verify `ferrum_add(21, 21)` returns `42`

---

## Additional Information

- **Native source code**: `native/test_stub/src/add.c`
- **C header**: `native/test_stub/include/add.h`
- **Build script**: `native/scripts/build_android.sh`
- **CMake config**: `native/test_stub/CMakeLists.txt`
- **Managed interop**: `samples/MinimalDemo/Interop/AddInterop.cs`

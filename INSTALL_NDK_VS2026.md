# Android NDK Installation for Visual Studio 2026 Preview

## Issue
Visual Studio 2026 (Preview) doesn't have Android NDK available in the installer yet.

## Solution: Install via Android Command Line Tools (Standalone)

### Option 1: Android Command Line Tools (Fastest - No Android Studio needed)

#### Step 1: Download Command Line Tools

1. Visit: https://developer.android.com/studio#command-line-tools-only
2. Download **"Command line tools only"** for Windows
3. Extract the ZIP file to a folder like: `C:\Android`

#### Step 2: Setup Android SDK Directory

```powershell
# Create Android SDK directory
$androidSdk = "$env:LOCALAPPDATA\Android\Sdk"
New-Item -ItemType Directory -Force -Path $androidSdk

# Extract command line tools to:
# $androidSdk\cmdline-tools\latest\
```

After extraction, your directory structure should look like:
```
C:\Users\draye\AppData\Local\Android\Sdk\
  └── cmdline-tools\
	  └── latest\
		  ├── bin\
		  ├── lib\
		  └── ...
```

#### Step 3: Install NDK Using sdkmanager

```powershell
# Set Android SDK path
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
$sdkmanager = "$env:ANDROID_HOME\cmdline-tools\latest\bin\sdkmanager.bat"

# Accept licenses
& $sdkmanager --licenses

# Install Android NDK
& $sdkmanager "ndk;26.1.10909125"

# Install CMake (if needed)
& $sdkmanager "cmake;3.22.1"

# Verify installation
& $sdkmanager --list_installed
```

#### Step 4: Set Environment Variables

```powershell
# Set Android SDK
[System.Environment]::SetEnvironmentVariable('ANDROID_HOME', "$env:LOCALAPPDATA\Android\Sdk", 'User')

# Set NDK path
$ndkPath = "$env:LOCALAPPDATA\Android\Sdk\ndk\26.1.10909125"
[System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $ndkPath, 'User')

# Set for current session
$env:ANDROID_NDK_HOME = $ndkPath
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"

Write-Host "✓ Environment variables set"
Write-Host "  ANDROID_HOME: $env:ANDROID_HOME"
Write-Host "  ANDROID_NDK_HOME: $env:ANDROID_NDK_HOME"
```

---

### Option 2: Android Studio (More User-Friendly)

If you prefer a GUI:

#### Step 1: Download and Install Android Studio

1. Download from: https://developer.android.com/studio
2. Run installer (default options)
3. Launch Android Studio

#### Step 2: Install NDK via SDK Manager

1. In Android Studio welcome screen, click **More Actions** → **SDK Manager**
   (or **Tools** → **SDK Manager** if project is open)

2. Click **SDK Tools** tab

3. Check these items:
   - ☑ **Android SDK Command-line Tools**
   - ☑ **Android SDK Build-Tools**
   - ☑ **NDK (Side by side)**
   - ☑ **CMake**

4. Click **Apply** to download and install

5. Note the SDK location shown at the top (usually: `C:\Users\draye\AppData\Local\Android\Sdk`)

#### Step 3: Set Environment Variables

```powershell
# After Android Studio installation
$androidSdk = "$env:LOCALAPPDATA\Android\Sdk"
$ndkPath = Get-ChildItem "$androidSdk\ndk" -Directory | Sort-Object Name -Descending | Select-Object -First 1

# Set environment variables
[System.Environment]::SetEnvironmentVariable('ANDROID_HOME', $androidSdk, 'User')
[System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $ndkPath.FullName, 'User')

# Set for current session
$env:ANDROID_HOME = $androidSdk
$env:ANDROID_NDK_HOME = $ndkPath.FullName

Write-Host "✓ NDK installed at: $($ndkPath.FullName)"
```

---

### Option 3: Direct NDK Download (Manual)

For complete control:

1. **Download NDK directly**: https://developer.android.com/ndk/downloads
   - Choose: **android-ndk-r26c-windows.zip** (or latest)

2. **Extract to a folder**:
   ```powershell
   $ndkPath = "C:\Android\ndk\26.1.10909125"
   # Extract ZIP contents to this folder
   ```

3. **Set environment variable**:
   ```powershell
   [System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $ndkPath, 'User')
   $env:ANDROID_NDK_HOME = $ndkPath
   ```

---

## Quick Setup Script for Command Line Tools

Save this as `install-ndk-cmdline.ps1`:

```powershell
# Install Android NDK using command line tools

Write-Host "=== Android NDK Installation (Command Line) ===" -ForegroundColor Cyan
Write-Host ""

# Setup directories
$androidSdk = "$env:LOCALAPPDATA\Android\Sdk"
$cmdlineTools = "$androidSdk\cmdline-tools\latest"

if (-not (Test-Path "$cmdlineTools\bin\sdkmanager.bat")) {
	Write-Host "❌ Command line tools not found!" -ForegroundColor Red
	Write-Host ""
	Write-Host "Please download and extract command line tools first:" -ForegroundColor Yellow
	Write-Host "  1. Visit: https://developer.android.com/studio#command-line-tools-only"
	Write-Host "  2. Download 'Command line tools only' for Windows"
	Write-Host "  3. Extract to: $cmdlineTools"
	Write-Host ""
	exit 1
}

Write-Host "✓ Found command line tools" -ForegroundColor Green

# Set ANDROID_HOME
$env:ANDROID_HOME = $androidSdk
[System.Environment]::SetEnvironmentVariable('ANDROID_HOME', $androidSdk, 'User')

$sdkmanager = "$cmdlineTools\bin\sdkmanager.bat"

Write-Host "Accepting licenses..." -ForegroundColor Yellow
& $sdkmanager --licenses

Write-Host ""
Write-Host "Installing NDK (this may take a few minutes)..." -ForegroundColor Yellow
& $sdkmanager "ndk;26.1.10909125"

Write-Host ""
Write-Host "Installing CMake..." -ForegroundColor Yellow
& $sdkmanager "cmake;3.22.1"

# Set NDK environment variable
$ndkPath = "$androidSdk\ndk\26.1.10909125"
if (Test-Path $ndkPath) {
	$env:ANDROID_NDK_HOME = $ndkPath
	[System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $ndkPath, 'User')

	Write-Host ""
	Write-Host "✓ Installation complete!" -ForegroundColor Green
	Write-Host ""
	Write-Host "Environment variables set:" -ForegroundColor Cyan
	Write-Host "  ANDROID_HOME: $env:ANDROID_HOME"
	Write-Host "  ANDROID_NDK_HOME: $env:ANDROID_NDK_HOME"
	Write-Host ""
	Write-Host "Next steps:" -ForegroundColor Yellow
	Write-Host "  1. Restart PowerShell"
	Write-Host "  2. Run: & `"C:\Program Files\Git\bin\bash.exe`" ./native/scripts/build_android.sh"
	Write-Host ""
} else {
	Write-Host "❌ NDK installation failed" -ForegroundColor Red
}
```

---

## After Installation

Once NDK is installed via ANY method above:

1. **Restart PowerShell** (to pick up environment variables)

2. **Verify installation**:
   ```powershell
   Write-Host "ANDROID_NDK_HOME: $env:ANDROID_NDK_HOME"
   Test-Path "$env:ANDROID_NDK_HOME\build\cmake\android.toolchain.cmake"
   ```

3. **Build native libraries**:
   ```powershell
   & "C:\Program Files\Git\bin\bash.exe" ./native/scripts/build_android.sh
   ```

4. **Rebuild MinimalDemo** in Visual Studio

5. **Deploy and test** on Android device/emulator

---

## Which Option Should You Choose?

| Method | When to Use |
|--------|------------|
| **Command Line Tools** | You only need NDK, minimal installation |
| **Android Studio** | You plan to do Android development, want GUI |
| **Direct Download** | You want manual control, offline installation |

**Recommended for your case**: **Command Line Tools** (Option 1) - fastest and lightest weight.

---

## Time Required

- Command Line Tools: ~5-10 minutes
- Android Studio: ~15-20 minutes
- Direct Download: ~5 minutes (manual setup)

---

## Support for VS 2026

When Visual Studio 2026 reaches release (not preview), the Android NDK component should become available. For now, use one of the standalone installation methods above.

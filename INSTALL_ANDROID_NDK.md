# Step-by-Step: Install Android NDK via Visual Studio

## Method 1: Visual Studio Installer (Easiest)

1. **Open Visual Studio Installer**
   - Close Visual Studio if it's running
   - Search for "Visual Studio Installer" in Windows Start menu
   - Or run: `& "${env:ProgramFiles}\Microsoft Visual Studio\Installer\setup.exe"`

2. **Modify Your Installation**
   - Find "Visual Studio Community 2022" (or your edition)
   - Click the **"Modify"** button

3. **Select Mobile Development Workload**
   - Go to the **"Workloads"** tab
   - Check **"Mobile development with .NET"**
   - This installs .NET MAUI and Android SDK

4. **Add Android NDK**
   - Click the **"Individual components"** tab
   - Search for "Android"
   - Check these items:
	 - ✓ **Android SDK setup (API level 34)**
	 - ✓ **Android NDK (Side by side)**
   - Or under "Mobile development with .NET" > Installation details:
	 - Check **"Android NDK (Side by side)"**

5. **Install**
   - Click **"Modify"** button (bottom right)
   - This will download ~2-3 GB
   - Takes 10-20 minutes depending on your connection

6. **Verify Installation**
   - After installation completes, the NDK will be at:
	 `%LOCALAPPDATA%\Android\Sdk\ndk\<version>`
   - Example: `C:\Users\draye\AppData\Local\Android\Sdk\ndk\26.1.10909125`

---

## Method 2: Android Studio (Alternative)

If you prefer Android Studio or VS Installer isn't working:

1. **Download Android Studio**
   - https://developer.android.com/studio
   - Install with default options

2. **Open SDK Manager**
   - Launch Android Studio
   - **Tools** > **SDK Manager** (or Welcome Screen > More Actions > SDK Manager)

3. **Install NDK**
   - Click **"SDK Tools"** tab
   - Check **"NDK (Side by side)"**
   - Check **"CMake"** (if not already installed)
   - Click **"Apply"** to download

4. **Note the SDK Location**
   - The SDK path is shown at the top of SDK Manager
   - Usually: `C:\Users\<username>\AppData\Local\Android\Sdk`

---

## After Installation: Set Environment Variable

Once the NDK is installed, set the environment variable:

### PowerShell Script:

```powershell
# Find the NDK installation
$sdkPath = "$env:LOCALAPPDATA\Android\Sdk"
$ndkPath = Join-Path $sdkPath "ndk"

if (Test-Path $ndkPath) {
	# Get the latest NDK version
	$latestNdk = Get-ChildItem $ndkPath -Directory | Sort-Object Name -Descending | Select-Object -First 1

	Write-Host "Found NDK: $($latestNdk.FullName)"

	# Set for current session
	$env:ANDROID_NDK_HOME = $latestNdk.FullName
	Write-Host "Set ANDROID_NDK_HOME for current session"

	# Set permanently for user (requires no admin)
	[System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $latestNdk.FullName, 'User')
	Write-Host "Set ANDROID_NDK_HOME permanently for user account"

	# Verify
	Write-Host ""
	Write-Host "Environment variable set to:"
	Write-Host "  $env:ANDROID_NDK_HOME"
	Write-Host ""
	Write-Host "Restart your terminal/Visual Studio to use the new environment variable."
} else {
	Write-Host "ERROR: NDK not found at $ndkPath"
	Write-Host "Make sure Android NDK is installed via Visual Studio Installer or Android Studio."
}
```

### Save and run this script:

1. Save the above as `set-ndk-path.ps1` in your repo root
2. Run: `.\set-ndk-path.ps1`
3. **Restart PowerShell** (close and reopen) to pick up the environment variable

---

## Then Build Native Libraries

After setting ANDROID_NDK_HOME and restarting PowerShell:

```powershell
# Verify the variable is set
$env:ANDROID_NDK_HOME

# Should output something like:
# C:\Users\draye\AppData\Local\Android\Sdk\ndk\26.1.10909125

# Now run the build script
& "C:\Program Files\Git\bin\bash.exe" ./native/scripts/build_android.sh
```

---

## Quick Check Commands

Run these to verify your setup:

```powershell
# Check if NDK path is set
Write-Host "ANDROID_NDK_HOME: $env:ANDROID_NDK_HOME"

# Check if NDK exists
if ($env:ANDROID_NDK_HOME -and (Test-Path $env:ANDROID_NDK_HOME)) {
	Write-Host "✓ NDK found at: $env:ANDROID_NDK_HOME"
	Test-Path (Join-Path $env:ANDROID_NDK_HOME "build\cmake\android.toolchain.cmake")
} else {
	Write-Host "✗ NDK not found or ANDROID_NDK_HOME not set"
}

# Check CMake
cmake --version

# Check Git Bash
Test-Path "C:\Program Files\Git\bin\bash.exe"
```

---

## Time Required

- **Visual Studio Installer**: 15-20 minutes (download + install)
- **Android Studio**: 10-15 minutes (if you already have VS)
- **Environment variable setup**: 1 minute
- **Building native libraries**: 2-5 minutes

---

## Need Help?

If installation fails or you encounter issues, check:
- Disk space (need ~3-5 GB free)
- Internet connection (for downloads)
- Windows Defender/antivirus (may slow download)

Run the quick check commands above to diagnose issues.

# PowerShell script to find and set ANDROID_NDK_HOME environment variable
# Run this AFTER installing Android NDK via Visual Studio Installer or Android Studio

Write-Host "=== Android NDK Configuration ===" -ForegroundColor Cyan
Write-Host ""

# Search for Android SDK in common locations
$sdkLocations = @(
	"$env:LOCALAPPDATA\Android\Sdk",
	"$env:APPDATA\Android\Sdk",
	"$env:ProgramFiles\Android\Sdk",
	"$env:ProgramFiles(x86)\Android\Sdk",
	"$env:USERPROFILE\Android\Sdk"
)

$sdkPath = $null
foreach ($location in $sdkLocations) {
	if (Test-Path $location) {
		$sdkPath = $location
		Write-Host "✓ Found Android SDK at: $sdkPath" -ForegroundColor Green
		break
	}
}

if (-not $sdkPath) {
	Write-Host "✗ Android SDK not found in standard locations" -ForegroundColor Red
	Write-Host ""
	Write-Host "Please install Android NDK first:" -ForegroundColor Yellow
	Write-Host "  1. Open Visual Studio Installer"
	Write-Host "  2. Modify your installation"
	Write-Host "  3. Check 'Mobile development with .NET' workload"
	Write-Host "  4. Under Individual Components, check 'Android NDK (Side by side)'"
	Write-Host "  5. Click Modify to install"
	Write-Host ""
	Write-Host "Or install via Android Studio SDK Manager"
	Write-Host ""
	Write-Host "See INSTALL_ANDROID_NDK.md for detailed instructions"
	exit 1
}

# Look for NDK directory
$ndkPath = Join-Path $sdkPath "ndk"

if (-not (Test-Path $ndkPath)) {
	Write-Host "✗ NDK directory not found at: $ndkPath" -ForegroundColor Red
	Write-Host ""
	Write-Host "Android SDK is installed but NDK is missing." -ForegroundColor Yellow
	Write-Host "Install Android NDK via Visual Studio Installer or Android Studio SDK Manager."
	Write-Host ""
	exit 1
}

# Find installed NDK versions
$ndkVersions = Get-ChildItem $ndkPath -Directory -ErrorAction SilentlyContinue | 
			   Sort-Object Name -Descending

if ($ndkVersions.Count -eq 0) {
	Write-Host "✗ No NDK versions found in: $ndkPath" -ForegroundColor Red
	Write-Host "Install Android NDK via Visual Studio Installer or Android Studio SDK Manager."
	exit 1
}

# Select the latest version
$latestNdk = $ndkVersions[0]
Write-Host "✓ Found NDK version: $($latestNdk.Name)" -ForegroundColor Green
Write-Host "  Path: $($latestNdk.FullName)" -ForegroundColor Gray
Write-Host ""

# Verify toolchain file exists
$toolchainFile = Join-Path $latestNdk.FullName "build\cmake\android.toolchain.cmake"
if (Test-Path $toolchainFile) {
	Write-Host "✓ NDK toolchain verified" -ForegroundColor Green
} else {
	Write-Host "⚠ Warning: Toolchain file not found at expected location" -ForegroundColor Yellow
	Write-Host "  Expected: $toolchainFile"
}

Write-Host ""
Write-Host "Setting ANDROID_NDK_HOME environment variable..." -ForegroundColor Cyan

# Set for current PowerShell session
$env:ANDROID_NDK_HOME = $latestNdk.FullName
Write-Host "✓ Set for current PowerShell session" -ForegroundColor Green

# Set permanently for user (no admin required)
try {
	[System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $latestNdk.FullName, 'User')
	Write-Host "✓ Set permanently for user account" -ForegroundColor Green
} catch {
	Write-Host "⚠ Failed to set permanent environment variable: $_" -ForegroundColor Yellow
	Write-Host "  You may need to run PowerShell as Administrator" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== Configuration Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "ANDROID_NDK_HOME = $env:ANDROID_NDK_HOME" -ForegroundColor White
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Close and reopen PowerShell (or restart Visual Studio)"
Write-Host "     to pick up the new environment variable"
Write-Host ""
Write-Host "  2. Verify the variable is set:"
Write-Host "     `$env:ANDROID_NDK_HOME" -ForegroundColor Cyan
Write-Host ""
Write-Host "  3. Build the native libraries:"
Write-Host "     & `"C:\Program Files\Git\bin\bash.exe`" ./native/scripts/build_android.sh" -ForegroundColor Cyan
Write-Host ""
Write-Host "  4. Rebuild MinimalDemo in Visual Studio"
Write-Host ""
Write-Host "  5. Deploy to Android device/emulator"
Write-Host ""

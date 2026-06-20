# PowerShell Script to Set Up Android NDK for Ferrum Native Build
# Run this script as Administrator after installing Android NDK

Write-Host "=== Ferrum Android NDK Setup ===" -ForegroundColor Cyan
Write-Host ""

# Function to find NDK installation
function Find-AndroidNDK {
	$possiblePaths = @(
		"$env:LOCALAPPDATA\Android\Sdk\ndk",
		"$env:ANDROID_NDK_HOME",
		"$env:NDK_HOME",
		"$env:PROGRAMFILES\Android\android-sdk\ndk",
		"$env:PROGRAMFILES(x86)\Android\android-sdk\ndk"
	)

	foreach ($path in $possiblePaths) {
		if ($path -and (Test-Path $path)) {
			$ndkVersions = Get-ChildItem -Path $path -Directory -ErrorAction SilentlyContinue | 
						   Sort-Object Name -Descending
			if ($ndkVersions) {
				return $ndkVersions[0].FullName
			}
		}
	}
	return $null
}

# Check current NDK status
Write-Host "1. Checking for Android NDK..." -ForegroundColor Yellow
$currentNdk = $env:ANDROID_NDK_HOME
if ($currentNdk) {
	Write-Host "   ✓ ANDROID_NDK_HOME is set: $currentNdk" -ForegroundColor Green
} else {
	Write-Host "   ✗ ANDROID_NDK_HOME not set" -ForegroundColor Red
}

# Try to find NDK
Write-Host ""
Write-Host "2. Searching for NDK installation..." -ForegroundColor Yellow
$foundNdk = Find-AndroidNDK

if ($foundNdk) {
	Write-Host "   ✓ Found NDK at: $foundNdk" -ForegroundColor Green

	if ($currentNdk -ne $foundNdk) {
		Write-Host ""
		Write-Host "3. Setting ANDROID_NDK_HOME environment variable..." -ForegroundColor Yellow

		# Set for current session
		$env:ANDROID_NDK_HOME = $foundNdk
		Write-Host "   ✓ Set for current PowerShell session" -ForegroundColor Green

		# Set permanently for user
		try {
			[System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $foundNdk, 'User')
			Write-Host "   ✓ Set permanently for user account" -ForegroundColor Green
			Write-Host "   (You may need to restart Visual Studio to pick up changes)" -ForegroundColor Gray
		} catch {
			Write-Host "   ⚠ Failed to set permanent variable: $_" -ForegroundColor Red
			Write-Host "   Run this script as Administrator to set permanently" -ForegroundColor Yellow
		}
	} else {
		Write-Host "   ✓ ANDROID_NDK_HOME already points to latest NDK" -ForegroundColor Green
	}
} else {
	Write-Host "   ✗ NDK not found in standard locations" -ForegroundColor Red
	Write-Host ""
	Write-Host "To install Android NDK:" -ForegroundColor Yellow
	Write-Host "  Option 1: Visual Studio Installer" -ForegroundColor Cyan
	Write-Host "    - Open Visual Studio Installer"
	Write-Host "    - Modify your installation"
	Write-Host "    - Under 'Mobile development with .NET' workload:"
	Write-Host "      ✓ Check 'Android SDK setup'"
	Write-Host "      ✓ Check 'Android NDK (Side by side)'"
	Write-Host ""
	Write-Host "  Option 2: Android Studio" -ForegroundColor Cyan
	Write-Host "    - Download from: https://developer.android.com/studio"
	Write-Host "    - Open SDK Manager (Tools > SDK Manager)"
	Write-Host "    - SDK Tools tab > Check 'NDK (Side by side)' > Apply"
	Write-Host ""
	exit 1
}

# Check for Git Bash
Write-Host ""
Write-Host "4. Checking for bash shell (needed to run build script)..." -ForegroundColor Yellow
$gitBashPaths = @(
	"${env:ProgramFiles}\Git\bin\bash.exe",
	"${env:ProgramFiles(x86)}\Git\bin\bash.exe"
)

$bashPath = $null
foreach ($path in $gitBashPaths) {
	if (Test-Path $path) {
		$bashPath = $path
		break
	}
}

if ($bashPath) {
	Write-Host "   ✓ Git Bash found at: $bashPath" -ForegroundColor Green
} else {
	Write-Host "   ✗ Git Bash not found" -ForegroundColor Red
	Write-Host "   Install Git for Windows: https://git-scm.com/download/win" -ForegroundColor Yellow
	$bashPath = "bash"  # Try system PATH
}

# Check CMake
Write-Host ""
Write-Host "5. Checking for CMake..." -ForegroundColor Yellow
try {
	$cmakeVersion = & cmake --version 2>&1 | Select-Object -First 1
	Write-Host "   ✓ $cmakeVersion" -ForegroundColor Green
} catch {
	Write-Host "   ✗ CMake not found" -ForegroundColor Red
	Write-Host "   Download from: https://cmake.org/download/" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "=== Summary ===" -ForegroundColor Cyan
Write-Host "ANDROID_NDK_HOME: $env:ANDROID_NDK_HOME" -ForegroundColor Gray
Write-Host ""

if ($foundNdk -and $bashPath) {
	Write-Host "✓ All prerequisites installed!" -ForegroundColor Green
	Write-Host ""
	Write-Host "Next steps:" -ForegroundColor Yellow
	Write-Host "  1. Close and reopen PowerShell/Visual Studio (to pick up env vars)"
	Write-Host "  2. Navigate to repository root"
	Write-Host "  3. Run the build script:"
	Write-Host ""
	Write-Host "     & `"$bashPath`" ./native/scripts/build_android.sh" -ForegroundColor Cyan
	Write-Host ""
	Write-Host "  4. Rebuild MinimalDemo project in Visual Studio"
	Write-Host "  5. Deploy to Android device/emulator"
	Write-Host ""
} else {
	Write-Host "⚠ Some prerequisites are missing. Install them first." -ForegroundColor Red
}

Write-Host "For detailed instructions, see: ANDROID_BUILD_INSTRUCTIONS.md" -ForegroundColor Gray
Write-Host ""

# Quick Android NDK Setup for VS 2026 Preview
# This script helps you install Android NDK via command line tools

param(
	[switch]$DownloadCmdlineTools,
	[switch]$InstallNDK
)

$ErrorActionPreference = "Stop"

Write-Host "=== Android NDK Setup for VS 2026 Preview ===" -ForegroundColor Cyan
Write-Host ""

$androidSdk = "$env:LOCALAPPDATA\Android\Sdk"
$cmdlineTools = "$androidSdk\cmdline-tools\latest"
$sdkmanager = "$cmdlineTools\bin\sdkmanager.bat"

# Function to download command line tools
function Download-CommandLineTools {
	Write-Host "Downloading Android Command Line Tools..." -ForegroundColor Yellow

	$downloadUrl = "https://dl.google.com/android/repository/commandlinetools-win-11076708_latest.zip"
	$zipFile = "$env:TEMP\android-cmdline-tools.zip"
	$extractPath = "$env:TEMP\android-cmdline-tools"

	try {
		# Download
		Write-Host "  Downloading from: $downloadUrl"
		Invoke-WebRequest -Uri $downloadUrl -OutFile $zipFile -UseBasicParsing
		Write-Host "  ✓ Download complete" -ForegroundColor Green

		# Extract
		Write-Host "  Extracting..."
		Expand-Archive -Path $zipFile -DestinationPath $extractPath -Force

		# Move to correct location
		Write-Host "  Installing to: $cmdlineTools"
		New-Item -ItemType Directory -Force -Path $cmdlineTools | Out-Null

		# The zip contains a 'cmdline-tools' folder, we need to move its contents
		$sourcePath = "$extractPath\cmdline-tools"
		Get-ChildItem -Path $sourcePath | Move-Item -Destination $cmdlineTools -Force

		Write-Host "  ✓ Command Line Tools installed" -ForegroundColor Green

		# Cleanup
		Remove-Item $zipFile -Force
		Remove-Item $extractPath -Recurse -Force

		return $true
	} catch {
		Write-Host "  ✗ Download failed: $_" -ForegroundColor Red
		return $false
	}
}

# Check if command line tools exist
if (-not (Test-Path $sdkmanager)) {
	Write-Host "Command Line Tools not found" -ForegroundColor Yellow
	Write-Host ""

	if ($DownloadCmdlineTools) {
		if (Download-CommandLineTools) {
			Write-Host ""
		} else {
			Write-Host ""
			Write-Host "Please download manually from:" -ForegroundColor Yellow
			Write-Host "  https://developer.android.com/studio#command-line-tools-only"
			Write-Host ""
			Write-Host "Extract the ZIP and place contents in:" -ForegroundColor Yellow
			Write-Host "  $cmdlineTools"
			exit 1
		}
	} else {
		Write-Host "Run this script with -DownloadCmdlineTools to download automatically," -ForegroundColor Cyan
		Write-Host "or download manually from:" -ForegroundColor Cyan
		Write-Host "  https://developer.android.com/studio#command-line-tools-only"
		Write-Host ""
		Write-Host "Extract to: $cmdlineTools" -ForegroundColor Gray
		Write-Host ""
		Write-Host "Then run: .\install-ndk-standalone.ps1 -InstallNDK"
		exit 1
	}
}

Write-Host "✓ Command Line Tools found" -ForegroundColor Green
Write-Host ""

# Set ANDROID_HOME
$env:ANDROID_HOME = $androidSdk
[System.Environment]::SetEnvironmentVariable('ANDROID_HOME', $androidSdk, 'User')
Write-Host "✓ ANDROID_HOME set to: $androidSdk" -ForegroundColor Green

# Install NDK
if ($InstallNDK -or $DownloadCmdlineTools) {
	Write-Host ""
	Write-Host "Accepting Android SDK licenses..." -ForegroundColor Yellow

	# Accept licenses (echo 'y' multiple times)
	"y`ny`ny`ny`ny`ny`ny`ny`ny`n" | & $sdkmanager --licenses 2>&1 | Out-Null

	Write-Host "✓ Licenses accepted" -ForegroundColor Green
	Write-Host ""
	Write-Host "Installing Android NDK (this takes 5-10 minutes)..." -ForegroundColor Yellow
	Write-Host "  Downloading and extracting NDK package..." -ForegroundColor Gray

	& $sdkmanager "ndk;26.1.10909125" 2>&1 | ForEach-Object {
		if ($_ -match "Installing|Downloading") {
			Write-Host "  $_" -ForegroundColor Gray
		}
	}

	Write-Host ""
	Write-Host "Installing CMake..." -ForegroundColor Yellow
	& $sdkmanager "cmake;3.22.1" 2>&1 | Out-Null

	Write-Host "✓ NDK and CMake installed" -ForegroundColor Green
}

# Check if NDK is installed
$ndkPath = "$androidSdk\ndk\26.1.10909125"
if (Test-Path $ndkPath) {
	Write-Host ""
	Write-Host "✓ NDK found at: $ndkPath" -ForegroundColor Green

	# Set environment variable
	$env:ANDROID_NDK_HOME = $ndkPath
	[System.Environment]::SetEnvironmentVariable('ANDROID_NDK_HOME', $ndkPath, 'User')

	Write-Host "✓ ANDROID_NDK_HOME set" -ForegroundColor Green

	# Verify toolchain
	$toolchain = "$ndkPath\build\cmake\android.toolchain.cmake"
	if (Test-Path $toolchain) {
		Write-Host "✓ NDK toolchain verified" -ForegroundColor Green
	}

	Write-Host ""
	Write-Host "=== Installation Complete! ===" -ForegroundColor Green
	Write-Host ""
	Write-Host "Environment variables:" -ForegroundColor Cyan
	Write-Host "  ANDROID_HOME      = $env:ANDROID_HOME" -ForegroundColor White
	Write-Host "  ANDROID_NDK_HOME  = $env:ANDROID_NDK_HOME" -ForegroundColor White
	Write-Host ""
	Write-Host "Next steps:" -ForegroundColor Yellow
	Write-Host "  1. Restart PowerShell (close and reopen)" -ForegroundColor White
	Write-Host "  2. Verify with: `$env:ANDROID_NDK_HOME" -ForegroundColor Cyan
	Write-Host "  3. Build native libraries:" -ForegroundColor White
	Write-Host "     & `"C:\Program Files\Git\bin\bash.exe`" ./native/scripts/build_android.sh" -ForegroundColor Cyan
	Write-Host "  4. Rebuild MinimalDemo in Visual Studio" -ForegroundColor White
	Write-Host ""

} else {
	Write-Host ""
	Write-Host "NDK not found. Run with -InstallNDK flag:" -ForegroundColor Yellow
	Write-Host "  .\install-ndk-standalone.ps1 -InstallNDK"
	Write-Host ""
}

# Rebuild MinimalDemo with Native Libraries
# Run this script to properly rebuild after native library changes

Write-Host "=== MinimalDemo Rebuild Script ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check if Visual Studio is using files
Write-Host "Step 1: Checking for file locks..." -ForegroundColor Yellow
$vsProcesses = Get-Process -Name "devenv" -ErrorAction SilentlyContinue
if ($vsProcesses) {
	Write-Host "⚠️  Visual Studio is running" -ForegroundColor Yellow
	Write-Host ""
	Write-Host "Please do the following in Visual Studio:" -ForegroundColor Cyan
	Write-Host "  1. Stop debugging (if debugging)" -ForegroundColor White
	Write-Host "  2. Close the solution" -ForegroundColor White
	Write-Host "  3. Close Visual Studio (optional but recommended)" -ForegroundColor Gray
	Write-Host ""
	$response = Read-Host "Have you stopped debugging and closed the solution? (y/n)"
	if ($response -ne 'y') {
		Write-Host "Exiting. Please stop debugging and try again." -ForegroundColor Red
		exit 1
	}
}

# Step 2: Verify native libraries exist
Write-Host ""
Write-Host "Step 2: Verifying native libraries..." -ForegroundColor Yellow
$libsExist = $true
$requiredLibs = @(
	"artifacts\android\jniLibs\arm64-v8a\libferrum_test_stub.so",
	"artifacts\android\jniLibs\armeabi-v7a\libferrum_test_stub.so",
	"artifacts\android\jniLibs\x86_64\libferrum_test_stub.so"
)

foreach ($lib in $requiredLibs) {
	if (Test-Path $lib) {
		$size = (Get-Item $lib).Length
		Write-Host "  ✓ $lib ($size bytes)" -ForegroundColor Green
	} else {
		Write-Host "  ✗ $lib MISSING!" -ForegroundColor Red
		$libsExist = $false
	}
}

if (-not $libsExist) {
	Write-Host ""
	Write-Host "Native libraries are missing. Build them first:" -ForegroundColor Red
	Write-Host "  & `"C:\Program Files\Git\bin\bash.exe`" ./native/scripts/build_android_ninja.sh" -ForegroundColor Cyan
	exit 1
}

# Step 3: Clean obj and bin directories
Write-Host ""
Write-Host "Step 3: Cleaning build outputs..." -ForegroundColor Yellow
$dirsToClean = @(
	"samples\MinimalDemo\bin",
	"samples\MinimalDemo\obj"
)

foreach ($dir in $dirsToClean) {
	if (Test-Path $dir) {
		try {
			Remove-Item -Path $dir -Recurse -Force -ErrorAction Stop
			Write-Host "  ✓ Cleaned $dir" -ForegroundColor Green
		} catch {
			Write-Host "  ⚠️  Could not clean $dir (files may be locked)" -ForegroundColor Yellow
		}
	}
}

# Step 4: Build for Android
Write-Host ""
Write-Host "Step 4: Building MinimalDemo for Android..." -ForegroundColor Yellow
Write-Host ""

$buildOutput = dotnet build samples/MinimalDemo/MinimalDemo.csproj `
	-c Debug `
	-f net9.0-android `
	--no-incremental `
	2>&1

if ($LASTEXITCODE -eq 0) {
	Write-Host "✓ Build succeeded!" -ForegroundColor Green

	# Check if .so files are in the output
	Write-Host ""
	Write-Host "Step 5: Verifying native libraries in build output..." -ForegroundColor Yellow

	$outputPaths = @(
		"samples\MinimalDemo\obj\Debug\net9.0-android\android\assets",
		"samples\MinimalDemo\bin\Debug\net9.0-android"
	)

	foreach ($path in $outputPaths) {
		if (Test-Path $path) {
			$soFiles = Get-ChildItem -Path $path -Recurse -Filter "*.so" -ErrorAction SilentlyContinue
			if ($soFiles) {
				Write-Host "  Found .so files in $path" -ForegroundColor Green
				$soFiles | ForEach-Object {
					Write-Host "    - $($_.Name) in $($_.Directory.Name)" -ForegroundColor Gray
				}
			}
		}
	}

	Write-Host ""
	Write-Host "=== Build Complete! ===" -ForegroundColor Green
	Write-Host ""
	Write-Host "Next steps:" -ForegroundColor Yellow
	Write-Host "  1. Open Visual Studio" -ForegroundColor White
	Write-Host "  2. Open Ferrum.sln" -ForegroundColor White
	Write-Host "  3. Set MinimalDemo as startup project" -ForegroundColor White
	Write-Host "  4. Select Android emulator or device" -ForegroundColor White
	Write-Host "  5. Press F5 to debug" -ForegroundColor White
	Write-Host "  6. Click 'Call Native' button" -ForegroundColor White
	Write-Host "  7. Should see: ferrum_add(21, 21) = 42" -ForegroundColor Cyan
	Write-Host ""

} else {
	Write-Host "✗ Build failed!" -ForegroundColor Red
	Write-Host ""
	Write-Host "Build output:" -ForegroundColor Yellow
	Write-Host $buildOutput
	Write-Host ""
	Write-Host "Common issues:" -ForegroundColor Yellow
	Write-Host "  - Visual Studio still has files locked (close VS completely)" -ForegroundColor White
	Write-Host "  - Try running this script again" -ForegroundColor White
	Write-Host "  - Check that native libraries exist in artifacts/ directory" -ForegroundColor White
}

@echo off
REM Quick Android NDK status check for Ferrum project
REM Double-click this file to run

echo === Ferrum Android NDK Status Check ===
echo.

REM Check for NDK
echo 1. Checking for Android NDK...
if defined ANDROID_NDK_HOME (
	echo    [OK] ANDROID_NDK_HOME: %ANDROID_NDK_HOME%
) else if defined NDK_HOME (
	echo    [OK] NDK_HOME: %NDK_HOME%
) else (
	echo    [ERROR] ANDROID_NDK_HOME not set
	echo.
	echo    To install Android NDK:
	echo      - Open Visual Studio Installer
	echo      - Modify installation
	echo      - Check "Mobile development with .NET"
	echo      - Check "Android NDK (Side by side)"
	echo      - Click Modify
	echo.
)

echo.
echo 2. Checking for Git Bash...
if exist "C:\Program Files\Git\bin\bash.exe" (
	echo    [OK] Git Bash found
	set BASH_PATH="C:\Program Files\Git\bin\bash.exe"
) else if exist "C:\Program Files (x86)\Git\bin\bash.exe" (
	echo    [OK] Git Bash found
	set BASH_PATH="C:\Program Files (x86)\Git\bin\bash.exe"
) else (
	echo    [ERROR] Git Bash not found
	echo    Install from: https://git-scm.com/download/win
	set BASH_PATH=
)

echo.
echo 3. Checking for CMake...
cmake --version >nul 2>&1
if %errorlevel% == 0 (
	cmake --version | findstr /i "version"
) else (
	echo    [ERROR] CMake not found
	echo    Download from: https://cmake.org/download/
)

echo.
echo === Next Steps ===
echo.

if defined ANDROID_NDK_HOME (
	if defined BASH_PATH (
		echo All prerequisites are installed!
		echo.
		echo To build native libraries, run:
		echo   %BASH_PATH% ./native/scripts/build_android.sh
		echo.
		echo Then rebuild MinimalDemo in Visual Studio.
	) else (
		echo Install Git for Windows, then run the build script.
	)
) else (
	echo Install Android NDK first (see instructions above).
)

echo.
echo See ANDROID_BUILD_INSTRUCTIONS.md for detailed setup guide.
echo.
pause

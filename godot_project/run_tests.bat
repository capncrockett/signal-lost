@echo off
REM This script runs all tests for the Signal Lost Godot project
REM It tries to find Godot in common installation locations

echo Running tests for Signal Lost Godot project...

REM Set the Godot executable path
set "GODOT_EXECUTABLE=C:\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64_console.exe"

REM Check if Godot executable exists
if not exist "%GODOT_EXECUTABLE%" (
    echo Error: Godot executable not found at %GODOT_EXECUTABLE%
    echo Please install Godot or update the script with the correct path.
    exit /b 1
)
echo Using Godot executable: %GODOT_EXECUTABLE%

REM Get the directory of this script
set "DIR=%~dp0"

echo Project path: %DIR%

REM Check if GUT is installed
if not exist "%DIR%addons\gut" (
    echo GUT is not installed. Installing...
    call "%DIR%install_gut.bat"
)

REM Run the test runner script
cd "%DIR%"
"%GODOT_EXECUTABLE%" --script "tests/test_runner.gd"

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE% equ 0 (
    echo All tests passed!
) else (
    echo Some tests failed. See above for details.
)

exit /b %EXIT_CODE%

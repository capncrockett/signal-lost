@echo off
REM This script runs custom tests for the Signal Lost Godot project on Windows

echo Running custom tests for Signal Lost Godot project...

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

REM Run the comprehensive test runner scene
echo Running comprehensive test runner scene...
"%GODOT_EXECUTABLE%" --path "%DIR%" tests/ComprehensiveTestRunnerScene.tscn

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE% EQU 0 (
    echo Custom tests completed successfully!
) else (
    echo Custom tests failed with exit code %EXIT_CODE%
)

exit /b %EXIT_CODE%

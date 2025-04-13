@echo off
REM This script runs all tests for the Signal Lost Godot project
REM It requires Godot to be installed and available in the PATH

echo Running tests for Signal Lost Godot project...

REM Check if Godot is installed
where godot >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo Error: Godot is not installed or not in PATH
    echo Please install Godot from https://godotengine.org/download
    exit /b 1
)

REM Get the directory of this script
set "DIR=%~dp0"

echo Project path: %DIR%

REM Run the test runner script
godot --path "%DIR%" --script Tests/TestRunner.cs

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE% equ 0 (
    echo All tests passed!
) else (
    echo Some tests failed. See above for details.
)

exit /b %EXIT_CODE%

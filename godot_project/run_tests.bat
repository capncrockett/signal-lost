@echo off
REM This script runs all tests for the Signal Lost Godot project
REM It tries to find Godot in common installation locations

echo Running tests for Signal Lost Godot project...

REM Try to find Godot executable
set "GODOT_EXECUTABLE="

REM Check if Godot is in PATH
where godot >nul 2>nul
if %ERRORLEVEL% equ 0 (
    set "GODOT_EXECUTABLE=godot"
    goto :found_godot
)

REM Check common installation locations
if exist "%ProgramFiles%\Godot\Godot.exe" (
    set "GODOT_EXECUTABLE=%ProgramFiles%\Godot\Godot.exe"
    goto :found_godot
)

if exist "%ProgramFiles%\Godot_4.4.1\Godot.exe" (
    set "GODOT_EXECUTABLE=%ProgramFiles%\Godot_4.4.1\Godot.exe"
    goto :found_godot
)

if exist "%ProgramFiles(x86)%\Godot\Godot.exe" (
    set "GODOT_EXECUTABLE=%ProgramFiles(x86)%\Godot\Godot.exe"
    goto :found_godot
)

if exist "%ProgramFiles(x86)%\Godot_4.4.1\Godot.exe" (
    set "GODOT_EXECUTABLE=%ProgramFiles(x86)%\Godot_4.4.1\Godot.exe"
    goto :found_godot
)

REM Check for Godot in the current directory
if exist "Godot.exe" (
    set "GODOT_EXECUTABLE=Godot.exe"
    goto :found_godot
)

REM Check for Godot in the parent directory
if exist "..\Godot.exe" (
    set "GODOT_EXECUTABLE=..\Godot.exe"
    goto :found_godot
)

REM Godot not found
echo Error: Godot executable not found
echo Please install Godot from https://godotengine.org/download
echo or specify the path to the Godot executable in the GODOT_EXECUTABLE variable
exit /b 1

:found_godot
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
"%GODOT_EXECUTABLE%" --path "%DIR%" --script Tests/TestRunner.cs

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE% equ 0 (
    echo All tests passed!
) else (
    echo Some tests failed. See above for details.
)

exit /b %EXIT_CODE%

@echo off
REM This script runs the Pixel Inventory Test scene

REM Find the Godot executable
set "GODOT_EXECUTABLE=C:\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64_console.exe"

REM Check if Godot executable exists
if not exist "%GODOT_EXECUTABLE%" (
    echo Godot executable not found at %GODOT_EXECUTABLE%
    echo Please install Godot or update the script with the correct path.
    exit /b 1
)

echo Using Godot executable: %GODOT_EXECUTABLE%

REM Get the directory of this script
set "DIR=%~dp0"

echo Running Pixel Inventory Test scene...
echo Project path: %DIR%

REM Run the scene
cd "%DIR%"
"%GODOT_EXECUTABLE%" --path . --scene scenes/test/PixelInventoryTest.tscn

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE% equ 0 (
    echo Scene ran successfully!
) else (
    echo Scene failed to run. See above for details.
)

exit /b %EXIT_CODE%

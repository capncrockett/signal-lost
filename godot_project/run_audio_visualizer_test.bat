@echo off
REM Run Audio Visualizer tests for the Signal Lost project

REM Get the directory of this script
set DIR=%~dp0

REM Set the path to the Godot executable
set GODOT_EXECUTABLE=C:\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64_console.exe

REM Create a timestamp for the log file
set TIMESTAMP=%DATE:~-4%-%DATE:~4,2%-%DATE:~7,2%_%TIME:~0,2%-%TIME:~3,2%-%TIME:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%

REM Create logs directory if it doesn't exist
if not exist "%DIR%\logs" mkdir "%DIR%\logs"

REM Run the Audio Visualizer test scene
echo Running Audio Visualizer tests...
cd "%DIR%"
"%GODOT_EXECUTABLE%" --path "%DIR%" tests/audio_visualizer/SimpleAudioVisualizerTestScene.tscn > "logs/audio_visualizer_tests_%TIMESTAMP%.log" 2>&1

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE% EQU 0 (
    echo All Audio Visualizer tests passed!
) else (
    echo Some Audio Visualizer tests failed. See logs for details.
)

exit /b %EXIT_CODE%

@echo off
REM This script runs the Godot project and captures logs to a file

echo Running Signal Lost Godot project with log capture...

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

REM Create logs directory if it doesn't exist
if not exist "%DIR%logs" mkdir "%DIR%logs"

REM Generate timestamp for log file
set "TIMESTAMP=%DATE:~-4,4%%DATE:~-7,2%%DATE:~-10,2%-%TIME:~0,2%%TIME:~3,2%%TIME:~6,2%"
set "TIMESTAMP=%TIMESTAMP: =0%"

REM Set log file path
set "LOG_FILE=%DIR%logs\godot_log_%TIMESTAMP%.log"

echo Log will be saved to: %LOG_FILE%

REM Run the project with log file capture
"%GODOT_EXECUTABLE%" --path "%DIR%" --log-file "%LOG_FILE%" %*

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

REM Filter the log file to show only errors and warnings
echo.
echo ===== ERRORS AND WARNINGS =====
findstr /i /C:"ERROR" /C:"WARNING" /C:"SCRIPT ERROR" "%LOG_FILE%"
echo ================================

echo Full log saved to: %LOG_FILE%

exit /b %EXIT_CODE%

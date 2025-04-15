@echo off
REM This script reads and displays the latest Godot log file

echo Reading latest Godot log file...

REM Get the directory of this script
set "DIR=%~dp0"

REM Check if logs directory exists
if not exist "%DIR%logs" (
    echo Error: Logs directory not found.
    echo Please run the project with run_with_logs.bat first.
    exit /b 1
)

REM Find the latest log file
for /f "delims=" %%a in ('dir /b /od "%DIR%logs\godot_log_*.log"') do set "LATEST_LOG=%%a"

if not defined LATEST_LOG (
    echo Error: No log files found.
    echo Please run the project with run_with_logs.bat first.
    exit /b 1
)

set "LOG_FILE=%DIR%logs\%LATEST_LOG%"

echo Latest log file: %LOG_FILE%

REM Display options
echo.
echo Choose an option:
echo 1. Show full log
echo 2. Show only errors and warnings
echo 3. Show only errors
echo 4. Show only warnings
echo 5. Search for specific text
echo.

set /p OPTION="Enter option (1-5): "

if "%OPTION%"=="1" (
    type "%LOG_FILE%"
) else if "%OPTION%"=="2" (
    findstr /i /C:"ERROR" /C:"WARNING" /C:"SCRIPT ERROR" "%LOG_FILE%"
) else if "%OPTION%"=="3" (
    findstr /i /C:"ERROR" /C:"SCRIPT ERROR" "%LOG_FILE%"
) else if "%OPTION%"=="4" (
    findstr /i /C:"WARNING" "%LOG_FILE%"
) else if "%OPTION%"=="5" (
    set /p SEARCH_TEXT="Enter text to search for: "
    findstr /i /C:"%SEARCH_TEXT%" "%LOG_FILE%"
) else (
    echo Invalid option.
    exit /b 1
)

exit /b 0

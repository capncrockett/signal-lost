@echo off
REM This script runs the Godot project and generates an error report

echo Running Signal Lost Godot project with error reporting...

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
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set "TIMESTAMP=%dt:~0,8%-%dt:~8,6%"

REM Set log file path
set "LOG_FILE=%DIR%logs\godot_log_%TIMESTAMP%.log"
set "ERROR_REPORT=%DIR%logs\error_report_%TIMESTAMP%.md"

echo Log will be saved to: %LOG_FILE%
echo Error report will be saved to: %ERROR_REPORT%

REM Run the project with log file capture
"%GODOT_EXECUTABLE%" --path "%DIR%" --log-file "%LOG_FILE%" %*

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

REM Generate error report
echo # Signal Lost Error Report > "%ERROR_REPORT%"
echo. >> "%ERROR_REPORT%"
echo Generated: %TIMESTAMP% >> "%ERROR_REPORT%"
echo. >> "%ERROR_REPORT%"
echo ## Errors >> "%ERROR_REPORT%"
echo. >> "%ERROR_REPORT%"
findstr /i /C:"ERROR" /C:"SCRIPT ERROR" "%LOG_FILE%" >> "%ERROR_REPORT%"
echo. >> "%ERROR_REPORT%"
echo ## Warnings >> "%ERROR_REPORT%"
echo. >> "%ERROR_REPORT%"
findstr /i /C:"WARNING" "%LOG_FILE%" >> "%ERROR_REPORT%"

REM Display summary
echo.
echo ===== ERROR REPORT SUMMARY =====
echo Errors:
findstr /i /C:"ERROR" /C:"SCRIPT ERROR" "%LOG_FILE%" | find /c /v ""
echo Warnings:
findstr /i /C:"WARNING" "%LOG_FILE%" | find /c /v ""
echo ================================

echo Full log saved to: %LOG_FILE%
echo Error report saved to: %ERROR_REPORT%

exit /b %EXIT_CODE%

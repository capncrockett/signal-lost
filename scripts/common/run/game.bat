@echo off
REM Unified game runner script for Signal Lost
REM Usage: scripts\common\run\game.bat [options]
REM Options:
REM   --scene=NAME     Run a specific scene (e.g., main, pixel, radio)
REM   --verbose        Run with verbose output
REM   --logs           Run with log capture
REM   --error-report   Run with error reporting

REM Default values
set "SCENE=MainGameScene"
set "VERBOSE=false"
set "LOGS=false"
set "ERROR_REPORT=false"

REM Parse command line arguments
:parse_args
if "%~1"=="" goto :done_parsing
set "ARG=%~1"

if "%ARG:~0,8%"=="--scene=" (
    set "SCENE_ARG=%ARG:~8%"
    if "%SCENE_ARG%"=="main" (
        set "SCENE=MainGameScene"
    ) else if "%SCENE_ARG%"=="pixel" (
        set "SCENE=PixelDrawingDemo"
    ) else if "%SCENE_ARG%"=="radio" (
        set "SCENE=RadioTunerScene"
    ) else (
        set "SCENE=%SCENE_ARG%"
    )
) else if "%ARG%"=="--verbose" (
    set "VERBOSE=true"
) else if "%ARG%"=="--logs" (
    set "LOGS=true"
) else if "%ARG%"=="--error-report" (
    set "ERROR_REPORT=true"
) else (
    echo Unknown option: %ARG%
    echo Usage: scripts\common\run\game.bat [--scene=NAME] [--verbose] [--logs] [--error-report]
    exit /b 1
)

shift
goto :parse_args
:done_parsing

REM Set the Godot executable path
REM Check if GODOT_PATH environment variable is set
if defined GODOT_PATH (
    set "GODOT_EXECUTABLE=%GODOT_PATH%"
) else (
    REM Default to a common Windows path
    set "GODOT_EXECUTABLE=C:\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64_console.exe"
)

REM Check if Godot executable exists
if not exist "%GODOT_EXECUTABLE%" (
    echo Error: Godot executable not found at %GODOT_EXECUTABLE%
    echo Please install Godot or set the GODOT_PATH environment variable.
    exit /b 1
)

echo Using Godot executable: %GODOT_EXECUTABLE%

REM Get the project directory
set "SCRIPT_DIR=%~dp0"
set "PROJECT_DIR=%SCRIPT_DIR%..\..\..\godot_project"
echo Project path: %PROJECT_DIR%

REM Create command arguments
set "CMD_ARGS=--path "%PROJECT_DIR%""

if "%VERBOSE%"=="true" (
    set "CMD_ARGS=%CMD_ARGS% --verbose"
)

if "%LOGS%"=="true" (
    REM Create logs directory if it doesn't exist
    if not exist "%PROJECT_DIR%\logs" mkdir "%PROJECT_DIR%\logs"
    
    REM Generate timestamp for log file
    for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "DATETIME=%%a"
    set "TIMESTAMP=%DATETIME:~0,8%-%DATETIME:~8,6%"
    
    REM Set log file path
    set "LOG_FILE=%PROJECT_DIR%\logs\godot_log_%TIMESTAMP%.log"
    
    echo Log will be saved to: %LOG_FILE%
    set "CMD_ARGS=%CMD_ARGS% --log-file "%LOG_FILE%""
)

if "%ERROR_REPORT%"=="true" (
    REM Create logs directory if it doesn't exist
    if not exist "%PROJECT_DIR%\logs" mkdir "%PROJECT_DIR%\logs"
    
    REM Generate timestamp for log file
    for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "DATETIME=%%a"
    set "TIMESTAMP=%DATETIME:~0,8%-%DATETIME:~8,6%"
    
    REM Set log file path
    set "LOG_FILE=%PROJECT_DIR%\logs\godot_log_%TIMESTAMP%.log"
    set "ERROR_REPORT_FILE=%PROJECT_DIR%\logs\error_report_%TIMESTAMP%.md"
    
    echo Log will be saved to: %LOG_FILE%
    echo Error report will be saved to: %ERROR_REPORT_FILE%
    set "CMD_ARGS=%CMD_ARGS% --log-file "%LOG_FILE%""
)

REM Run the scene
cd "%PROJECT_DIR%"
echo Running scene: %SCENE%
"%GODOT_EXECUTABLE%" %CMD_ARGS% --scene scenes\%SCENE%.tscn

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE% equ 0 (
    echo Game ran successfully!
) else (
    echo Game failed to run. See above for details.
)

REM Generate error report if requested
if "%ERROR_REPORT%"=="true" if exist "%LOG_FILE%" (
    echo Generating error report...
    echo # Signal Lost Error Report > "%ERROR_REPORT_FILE%"
    echo. >> "%ERROR_REPORT_FILE%"
    echo Generated: %DATE% %TIME% >> "%ERROR_REPORT_FILE%"
    echo. >> "%ERROR_REPORT_FILE%"
    echo ## Errors >> "%ERROR_REPORT_FILE%"
    echo. >> "%ERROR_REPORT_FILE%"
    echo ``` >> "%ERROR_REPORT_FILE%"
    findstr /i "ERROR SCRIPT_ERROR" "%LOG_FILE%" >> "%ERROR_REPORT_FILE%"
    echo ``` >> "%ERROR_REPORT_FILE%"
    echo. >> "%ERROR_REPORT_FILE%"
    echo ## Warnings >> "%ERROR_REPORT_FILE%"
    echo. >> "%ERROR_REPORT_FILE%"
    echo ``` >> "%ERROR_REPORT_FILE%"
    findstr /i "WARNING" "%LOG_FILE%" >> "%ERROR_REPORT_FILE%"
    echo ``` >> "%ERROR_REPORT_FILE%"
    echo. >> "%ERROR_REPORT_FILE%"
    echo ## Full Log >> "%ERROR_REPORT_FILE%"
    echo. >> "%ERROR_REPORT_FILE%"
    echo See: %LOG_FILE% >> "%ERROR_REPORT_FILE%"
)

exit /b %EXIT_CODE%

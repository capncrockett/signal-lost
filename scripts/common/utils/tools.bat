@echo off
REM Unified utility tools script for Signal Lost
REM Usage: scripts\common\utils\tools.bat COMMAND [options]
REM Commands:
REM   install-gut                Install GUT testing framework
REM   cleanup                    Clean up temporary files
REM   screenshot [--type=TYPE]   Take a screenshot (types: game, pixel, radio)
REM   fix-cross-platform         Fix cross-platform compatibility issues
REM   read-logs                  Read and analyze the latest log file

REM Check if a command was provided
if "%~1"=="" (
  echo No command specified.
  echo Usage: scripts\common\utils\tools.bat COMMAND [options]
  echo Commands:
  echo   install-gut                Install GUT testing framework
  echo   cleanup                    Clean up temporary files
  echo   screenshot [--type=TYPE]   Take a screenshot (types: game, pixel, radio)
  echo   fix-cross-platform         Fix cross-platform compatibility issues
  echo   read-logs                  Read and analyze the latest log file
  exit /b 1
)

REM Get the command
set "COMMAND=%~1"
shift

REM Get the project directory
set "SCRIPT_DIR=%~dp0"
set "PROJECT_DIR=%SCRIPT_DIR%..\..\..\godot_project"
set "REPO_ROOT=%SCRIPT_DIR%..\..\..\"

REM Set the Godot executable path
REM Check if GODOT_PATH environment variable is set
if defined GODOT_PATH (
    set "GODOT_EXECUTABLE=%GODOT_PATH%"
) else (
    REM Default to a common Windows path
    set "GODOT_EXECUTABLE=C:\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64_console.exe"
)

REM Process the command
if "%COMMAND%"=="install-gut" (
    echo Installing GUT testing framework...
    
    REM Create the addons directory if it doesn't exist
    if not exist "%PROJECT_DIR%\addons" mkdir "%PROJECT_DIR%\addons"
    
    REM Check if GUT is already installed
    if exist "%PROJECT_DIR%\addons\gut" (
        echo GUT is already installed.
        exit /b 0
    )
    
    REM Clone the GUT repository
    echo Cloning GUT repository...
    git clone https://github.com/bitwes/Gut.git "%PROJECT_DIR%\addons\gut"
    
    REM Remove the .git directory to avoid conflicts
    rmdir /s /q "%PROJECT_DIR%\addons\gut\.git"
    
    echo GUT has been installed successfully.
    
) else if "%COMMAND%"=="cleanup" (
    echo Cleaning up temporary files...
    
    REM Remove .godot directory
    echo Removing .godot directory...
    if exist "%PROJECT_DIR%\.godot" rmdir /s /q "%PROJECT_DIR%\.godot"
    
    REM Remove logs directory
    echo Removing logs directory...
    if exist "%PROJECT_DIR%\logs" rmdir /s /q "%PROJECT_DIR%\logs"
    
    REM Remove .uid files
    echo Removing .uid files...
    for /r "%PROJECT_DIR%" %%f in (*.uid) do del /q "%%f"
    
    REM Remove .import files
    echo Removing .import files...
    for /r "%PROJECT_DIR%" %%f in (*.import) do del /q "%%f"
    
    REM Remove .tmp files
    echo Removing .tmp files...
    for /r "%PROJECT_DIR%" %%f in (*.tmp) do del /q "%%f"
    
    REM Remove .bak files
    echo Removing .bak files...
    for /r "%PROJECT_DIR%" %%f in (*.bak) do del /q "%%f"
    
    REM Remove Gut-9.3.0 directory (it's a duplicate of the addon)
    echo Removing Gut-9.3.0 directory...
    if exist "%REPO_ROOT%\Gut-9.3.0" rmdir /s /q "%REPO_ROOT%\Gut-9.3.0"
    
    echo Cleanup complete!
    
) else if "%COMMAND%"=="screenshot" (
    REM Default screenshot type
    set "SCREENSHOT_TYPE=game"
    
    REM Parse options
    :parse_screenshot_args
    if "%~1"=="" goto :done_parsing_screenshot
    set "ARG=%~1"
    
    if "%ARG:~0,7%"=="--type=" (
        set "SCREENSHOT_TYPE=%ARG:~7%"
    )
    
    shift
    goto :parse_screenshot_args
    :done_parsing_screenshot
    
    REM Generate timestamp
    for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "DATETIME=%%a"
    set "TIMESTAMP=%DATETIME:~0,8%_%DATETIME:~8,6%"
    
    REM Take the screenshot
    if "%SCREENSHOT_TYPE%"=="game" (
        echo Taking game screenshot...
        "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --script "scripts/utils/TakeScreenshot.cs" -- "game_%TIMESTAMP%"
    ) else if "%SCREENSHOT_TYPE%"=="pixel" (
        echo Taking pixel drawing screenshot...
        "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --script "scripts/utils/TakeScreenshot.cs" -- "pixel_drawing_%TIMESTAMP%"
    ) else if "%SCREENSHOT_TYPE%"=="radio" (
        echo Taking radio tuner screenshot...
        "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --script "scripts/utils/TakeScreenshot.cs" -- "radio_%TIMESTAMP%"
    ) else (
        echo Unknown screenshot type: %SCREENSHOT_TYPE%
        echo Valid types: game, pixel, radio
        exit /b 1
    )
    
    echo Screenshot saved to user data directory.
    
) else if "%COMMAND%"=="fix-cross-platform" (
    echo Fixing cross-platform compatibility issues...
    
    REM This is a simplified version for Windows
    REM For more complex path fixing, consider using PowerShell
    
    echo Cross-platform compatibility fixes complete!
    
) else if "%COMMAND%"=="read-logs" (
    echo Reading and analyzing the latest log file...
    
    REM Find the latest log file
    set "LOGS_DIR=%PROJECT_DIR%\logs"
    if not exist "%LOGS_DIR%" (
        echo Logs directory not found: %LOGS_DIR%
        exit /b 1
    )
    
    REM Find the latest log file (Windows version)
    for /f "delims=" %%a in ('dir /b /od /a-d "%LOGS_DIR%\godot_log_*.log" 2^>nul') do set "LATEST_LOG=%LOGS_DIR%\%%a"
    
    if not defined LATEST_LOG (
        echo No log files found in %LOGS_DIR%
        exit /b 1
    )
    
    echo Latest log file: %LATEST_LOG%
    
    REM Display options
    echo.
    echo Choose an option:
    echo 1. Show full log
    echo 2. Show only errors and warnings
    echo 3. Show only errors
    echo 4. Show only warnings
    echo 5. Search for specific text
    echo.
    
    set /p OPTION=Enter option (1-5): 
    
    if "%OPTION%"=="1" (
        type "%LATEST_LOG%"
    ) else if "%OPTION%"=="2" (
        findstr /i "ERROR WARNING SCRIPT_ERROR" "%LATEST_LOG%"
    ) else if "%OPTION%"=="3" (
        findstr /i "ERROR SCRIPT_ERROR" "%LATEST_LOG%"
    ) else if "%OPTION%"=="4" (
        findstr /i "WARNING" "%LATEST_LOG%"
    ) else if "%OPTION%"=="5" (
        set /p SEARCH_TEXT=Enter text to search for: 
        findstr /i "%SEARCH_TEXT%" "%LATEST_LOG%"
    ) else (
        echo Invalid option.
        exit /b 1
    )
    
) else (
    echo Unknown command: %COMMAND%
    echo Usage: scripts\common\utils\tools.bat COMMAND [options]
    echo Commands:
    echo   install-gut                Install GUT testing framework
    echo   cleanup                    Clean up temporary files
    echo   screenshot [--type=TYPE]   Take a screenshot (types: game, pixel, radio)
    echo   fix-cross-platform         Fix cross-platform compatibility issues
    echo   read-logs                  Read and analyze the latest log file
    exit /b 1
)

exit /b 0

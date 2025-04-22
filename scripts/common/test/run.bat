@echo off
REM Unified test runner script for Signal Lost
REM Usage: scripts\common\test\run.bat [options]
REM Options:
REM   --gut             Run GUT tests
REM   --radio           Run radio tests
REM   --pixel           Run pixel UI tests
REM   --integration     Run integration tests
REM   --class=NAME      Run specific test class
REM   --skip=CLASSES    Skip specific test classes (comma-separated)

REM Default values
set "TEST_TYPE=all"
set "TEST_CLASS="
set "SKIP_CLASSES="

REM Parse command line arguments
:parse_args
if "%~1"=="" goto :done_parsing
set "ARG=%~1"

if "%ARG%"=="--gut" (
    set "TEST_TYPE=gut"
) else if "%ARG%"=="--radio" (
    set "TEST_TYPE=radio"
) else if "%ARG%"=="--pixel" (
    set "TEST_TYPE=pixel"
) else if "%ARG%"=="--integration" (
    set "TEST_TYPE=integration"
) else if "%ARG:~0,8%"=="--class=" (
    set "TEST_CLASS=%ARG:~8%"
) else if "%ARG:~0,7%"=="--skip=" (
    set "SKIP_CLASSES=%ARG:~7%"
) else (
    echo Unknown option: %ARG%
    echo Usage: scripts\common\test\run.bat [--gut] [--radio] [--pixel] [--integration] [--class=NAME] [--skip=CLASSES]
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

REM Check if GUT is installed
if not exist "%PROJECT_DIR%\addons\gut" (
    echo GUT is not installed. Installing...
    
    REM Create the addons directory if it doesn't exist
    if not exist "%PROJECT_DIR%\addons" mkdir "%PROJECT_DIR%\addons"
    
    REM Clone the GUT repository
    echo Cloning GUT repository...
    git clone https://github.com/bitwes/Gut.git "%PROJECT_DIR%\addons\gut"
    
    REM Remove the .git directory to avoid conflicts
    rmdir /s /q "%PROJECT_DIR%\addons\gut\.git"
    
    echo GUT has been installed successfully.
)

REM Run the appropriate tests
cd "%PROJECT_DIR%"

if "%TEST_TYPE%"=="gut" (
    echo Running GUT tests...
    "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --headless tests/GutTestRunner.tscn
) else if "%TEST_TYPE%"=="radio" (
    echo Running Radio Tuner tests...
    "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --headless tests/FullIntegrationTestScene.tscn
) else if "%TEST_TYPE%"=="pixel" (
    echo Running Pixel UI tests...
    "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --headless --script "tests/TestRunner.cs" -- --class="PixelInventoryUITests,PixelMapInterfaceTests"
) else if "%TEST_TYPE%"=="integration" (
    echo Running integration tests...
    "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --headless --script "tests/TestRunner.cs" -- --class="IntegrationTests"
) else (
    if defined TEST_CLASS (
        echo Running specific test class: %TEST_CLASS%
        "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --headless --script "tests/TestRunner.cs" -- --class="%TEST_CLASS%"
    ) else if defined SKIP_CLASSES (
        echo Running all tests except: %SKIP_CLASSES%
        "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --headless --script "tests/TestRunner.cs" -- --skip-classes="%SKIP_CLASSES%"
    ) else (
        echo Running all tests...
        "%GODOT_EXECUTABLE%" --path "%PROJECT_DIR%" --headless --script "tests/TestRunner.cs"
    )
)

REM Get the exit code
set EXIT_CODE=%ERRORLEVEL%

if %EXIT_CODE% equ 0 (
    echo All tests passed!
) else (
    echo Some tests failed. See above for details.
)

exit /b %EXIT_CODE%

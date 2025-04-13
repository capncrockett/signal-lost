@echo off
REM This script installs the GUT (Godot Unit Testing) addon

REM Get the directory of the script
set "DIR=%~dp0"

REM Create the addons directory if it doesn't exist
if not exist "%DIR%addons" mkdir "%DIR%addons"

REM Check if GUT is already installed
if exist "%DIR%addons\gut" (
    echo GUT is already installed.
    exit /b 0
)

REM Clone the GUT repository
echo Cloning GUT repository...
git clone https://github.com/bitwes/Gut.git "%DIR%addons\gut"

REM Remove the .git directory to avoid conflicts
rmdir /s /q "%DIR%addons\gut\.git"

echo GUT has been installed successfully.

@echo off
REM Windows wrapper for the unified game runner script

REM Get the directory of this script
set "SCRIPT_DIR=%~dp0"

REM Call the common script with all arguments
call "%SCRIPT_DIR%..\common\run\game.bat" %*

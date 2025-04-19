@echo off
REM Script to take a screenshot using the ScreenshotTaker utility
REM Usage: take_screenshot.bat [filename]

REM Default filename
SET FILENAME=screenshot

REM Check if a filename was provided
IF NOT "%~1"=="" SET FILENAME=%~1

echo Taking screenshot with filename: %FILENAME%

REM Run Godot with the screenshot tool
godot --path godot_project --script tools/TakeScreenshot.cs %FILENAME%

echo Screenshot taken.

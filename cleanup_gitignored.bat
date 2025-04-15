@echo off
REM Cleanup script for Signal Lost project
REM Removes all gitignored files and directories that are still in the repository

echo Starting cleanup of gitignored files...

REM Remove .godot directory
echo Removing .godot directory...
if exist godot_project\.godot rmdir /s /q godot_project\.godot

REM Remove logs directory
echo Removing logs directory...
if exist godot_project\logs rmdir /s /q godot_project\logs

REM Remove .uid files
echo Removing .uid files...
for /r godot_project %%f in (*.uid) do del /q "%%f"

REM Remove .import files
echo Removing .import files...
for /r godot_project %%f in (*.import) do del /q "%%f"

REM Remove .tmp files
echo Removing .tmp files...
for /r godot_project %%f in (*.tmp) do del /q "%%f"

REM Remove .bak files
echo Removing .bak files...
for /r godot_project %%f in (*.bak) do del /q "%%f"

REM Remove Gut-9.3.0 directory (it's a duplicate of the addon)
echo Removing Gut-9.3.0 directory...
if exist Gut-9.3.0 rmdir /s /q Gut-9.3.0

REM Remove remaining GDScript files (except in addons)
echo Removing remaining GDScript files...
for /r godot_project %%f in (*.gd) do (
    echo %%f | findstr /i /c:"addons" > nul
    if errorlevel 1 del /q "%%f"
)

echo Cleanup complete!

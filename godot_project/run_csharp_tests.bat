@echo off
REM Run C# tests for the Signal Lost project

REM Get the directory of this script
set DIR=%~dp0

REM Set the path to the Godot executable
set GODOT_EXECUTABLE=C:\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64_console.exe

REM Create a timestamp for the log file
set TIMESTAMP=%DATE:~-4%-%DATE:~4,2%-%DATE:~7,2%_%TIME:~0,2%-%TIME:~3,2%-%TIME:~6,2%
set TIMESTAMP=%TIMESTAMP: =0%

REM Create logs directory if it doesn't exist
if not exist "%DIR%\logs" mkdir "%DIR%\logs"

REM Run the C# test runner
echo Running C# tests...
cd "%DIR%"
"%GODOT_EXECUTABLE%" --script "tests/TestRunner.cs" > "logs/csharp_tests_%TIMESTAMP%.log" 2>&1

REM Check the exit code
if %ERRORLEVEL% NEQ 0 (
    echo Tests failed. See logs/csharp_tests_%TIMESTAMP%.log for details.
    exit /b 1
) else (
    echo All tests passed.
    exit /b 0
)

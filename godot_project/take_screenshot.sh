#!/bin/bash

# Script to take a screenshot using the ScreenshotTaker utility

# Path to Godot executable
GODOT_PATH="/Applications/Godot_mono.app/Contents/MacOS/Godot"

# Run the game with the screenshot command
$GODOT_PATH --path godot_project --script-execute "scripts/utils/TakeScreenshot.cs"

echo "Screenshot taken and saved to user://Screenshots directory"

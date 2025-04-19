#!/bin/bash

# Script to take a screenshot using the ScreenshotTaker utility
# Usage: ./take_screenshot.sh [filename]

# Default filename
FILENAME="screenshot"

# Check if a filename was provided
if [ $# -gt 0 ]; then
    FILENAME="$1"
fi

echo "Taking screenshot with filename: $FILENAME"

# Run Godot with the screenshot tool
/Applications/Godot_mono.app/Contents/MacOS/Godot --path godot_project --script tools/TakeScreenshot.cs "$FILENAME"

echo "Screenshot taken."

#!/bin/bash
#
# AI Agent Screenshot Tool
# 
# This script runs the game scene screenshot tool, which:
# 1. Loads the main game scene (PixelMainScene.tscn)
# 2. Waits 5 seconds for the scene to fully render
# 3. Takes a screenshot and saves it to the user data directory
# 4. Automatically exits after taking the screenshot
#
# The screenshots are saved to:
# ~/Library/Application Support/Godot/app_userdata/Signal Lost/Screenshots/
# with timestamped filenames like: game_screenshot_YYYYMMDD_HHMMSS.png

# Path to Godot executable
GODOT_PATH="/Applications/Godot_mono.app/Contents/MacOS/Godot"

# Path to the project and screenshot scene
PROJECT_PATH="/Users/josephdavey/Codin/signal-lost/godot_project"
SCREENSHOT_SCENE="game_scene_screenshot.tscn"

echo "AI Agent Screenshot Tool - Starting..."
echo "Taking screenshot of the main game scene..."

# Run the screenshot scene
"$GODOT_PATH" --path "$PROJECT_PATH" "$SCREENSHOT_SCENE"

echo "Screenshot process complete."
echo "Screenshot saved to: ~/Library/Application Support/Godot/app_userdata/Signal Lost/Screenshots/"
echo "You can view the most recent screenshot with: open ~/Library/Application\ Support/Godot/app_userdata/Signal\ Lost/Screenshots/game_screenshot_*.png"

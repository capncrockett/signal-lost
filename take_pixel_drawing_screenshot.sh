#!/bin/bash

# Take a screenshot of the pixel drawing demo
/Applications/Godot_mono.app/Contents/MacOS/Godot --path . --script scripts/utils/TakeScreenshot.cs -- "pixel_drawing_$(date +%Y%m%d_%H%M%S)"

echo "Screenshot saved to ~/Library/Application Support/Godot/app_userdata/Signal Lost/Screenshots/"
echo "You can view the most recent screenshot with: open ~/Library/Application\ Support/Godot/app_userdata/Signal\ Lost/Screenshots/pixel_drawing_*.png"

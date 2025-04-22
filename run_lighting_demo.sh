#!/bin/bash

# Run the Lighting Demo scene
# This script launches the Godot engine with the Lighting Demo scene

# Get the directory where this script is located
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"

# Change to the godot_project directory
cd "$SCRIPT_DIR/godot_project"

# Check if we're on macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS path
    GODOT_PATH="/Applications/Godot_mono.app/Contents/MacOS/Godot"
    
    # Check if Godot exists at the expected path
    if [ ! -f "$GODOT_PATH" ]; then
        echo "Godot not found at $GODOT_PATH"
        echo "Please install Godot with Mono support or update the path in this script."
        exit 1
    fi
else
    # Windows/Linux path (assuming 'godot' is in the PATH)
    GODOT_PATH="godot"
fi

# Run Godot with the Lighting Demo scene
echo "Starting Lighting Demo..."
"$GODOT_PATH" --path . --scene scenes/LightingDemo.tscn

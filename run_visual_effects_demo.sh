#!/bin/bash

# Run the Visual Effects Demo scene
# This script launches the Godot engine with the Visual Effects Demo scene

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

# Run Godot with the Visual Effects Demo scene
echo "Starting Visual Effects Demo..."
"$GODOT_PATH" --path . --scene Scenes/VisualEffectsDemo.tscn

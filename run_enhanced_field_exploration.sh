#!/bin/bash

# This script runs the Enhanced Field Exploration Demo scene

# Function to find Godot executable
find_godot() {
    # Check if GODOT_EXECUTABLE environment variable is set
    if [ ! -z "$GODOT_EXECUTABLE" ]; then
        if [ -f "$GODOT_EXECUTABLE" ]; then
            echo "$GODOT_EXECUTABLE"
            return 0
        else
            echo "Warning: GODOT_EXECUTABLE is set but the file does not exist: $GODOT_EXECUTABLE"
        fi
    fi

    # Check if Godot is in PATH
    if command -v godot &> /dev/null; then
        echo "godot"
        return 0
    fi

    # Check common installation locations on macOS
    if [ -d "/Applications/Godot_mono.app" ]; then
        echo "/Applications/Godot_mono.app/Contents/MacOS/Godot"
        return 0
    fi

    if [ -d "/Applications/Godot.app" ]; then
        echo "/Applications/Godot.app/Contents/MacOS/Godot"
        return 0
    fi

    # Not found
    echo ""
    return 1
}

# Find Godot executable
GODOT=$(find_godot)

if [ -z "$GODOT" ]; then
    echo "Error: Could not find Godot executable. Please install Godot or set the GODOT_EXECUTABLE environment variable."
    exit 1
fi

echo "Using Godot executable: $GODOT"

# Run the demo scene
echo "Running Enhanced Field Exploration Demo..."
"$GODOT" --path godot_project --scene Scenes/field/EnhancedFieldExplorationScene.tscn

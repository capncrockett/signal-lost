#!/bin/bash

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Find the Godot executable
GODOT_EXECUTABLE="/Applications/Godot_mono.app/Contents/MacOS/Godot"

# Check if Godot executable exists
if [ ! -f "$GODOT_EXECUTABLE" ]; then
    echo "Godot executable not found at $GODOT_EXECUTABLE"
    echo "Please install Godot or update the script with the correct path."
    exit 1
fi

echo "Using Godot executable: $GODOT_EXECUTABLE"
echo "Running Signal Lost Godot project..."
echo "Project path: $DIR"

# Run the project
"$GODOT_EXECUTABLE" --path "$DIR"

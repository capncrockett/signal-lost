#!/bin/bash

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Find the Godot executable
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    GODOT_EXECUTABLE="/Applications/Godot_mono.app/Contents/MacOS/Godot"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Linux
    GODOT_EXECUTABLE="godot"
elif [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" || "$OSTYPE" == "win32" ]]; then
    # Windows
    GODOT_EXECUTABLE="godot.exe"
else
    echo "Unsupported OS: $OSTYPE"
    exit 1
fi

# Check if Godot executable exists
if [ ! -f "$GODOT_EXECUTABLE" ] && ! command -v "$GODOT_EXECUTABLE" &> /dev/null; then
    echo "Godot executable not found at $GODOT_EXECUTABLE"
    echo "Please install Godot or update the script with the correct path."
    exit 1
fi

echo "Using Godot executable: $GODOT_EXECUTABLE"
echo "Running integration tests for Signal Lost Godot project..."
echo "Project path: $DIR"

# Run the integration test scene
echo "Running full integration test scene..."
"$GODOT_EXECUTABLE" --path "$DIR" --headless --script "res://tests/FullIntegrationTest.gd"

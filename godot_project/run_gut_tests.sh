#!/bin/bash

# This script runs all GUT tests for the Signal Lost Godot project

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
echo "Running GUT tests for Signal Lost Godot project..."
echo "Project path: $DIR"

# Run the GUT test runner scene
"$GODOT_EXECUTABLE" --path "$DIR" --headless tests/GutTestRunner.tscn

# Get the exit code
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo "All GUT tests passed!"
else
    echo "Some GUT tests failed. See above for details."
fi

exit $EXIT_CODE

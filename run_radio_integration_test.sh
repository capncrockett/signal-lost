#!/bin/bash

# This script runs the radio integration test

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

    # Check for Godot in the current directory
    if [ -f "./Godot" ]; then
        echo "./Godot"
        return 0
    fi

    # Godot not found
    return 1
}

# Find Godot executable
GODOT_EXECUTABLE=$(find_godot)

# Check if Godot was found
if [ -z "$GODOT_EXECUTABLE" ]; then
    echo "Error: Godot executable not found"
    echo "Please install Godot from https://godotengine.org/download"
    echo "or specify the path to the Godot executable in the GODOT_EXECUTABLE environment variable"
    echo "Example: GODOT_EXECUTABLE=/path/to/godot ./run_radio_integration_test.sh"
    exit 1
fi

echo "Using Godot executable: $GODOT_EXECUTABLE"

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Running Radio Integration Test..."
echo "Project path: $DIR"

# Create a test scene file
TEST_SCENE_PATH="$DIR/tests/RadioIntegrationTestScene.tscn"

cat > "$TEST_SCENE_PATH" << EOL
[gd_scene load_steps=2 format=3]

[ext_resource type="Script" path="res://tests/RadioIntegrationTest.cs" id="1_test"]

[node name="RadioIntegrationTestScene" type="Node"]
script = ExtResource("1_test")
EOL

echo "Created test scene at $TEST_SCENE_PATH"

# Run the test scene
cd "$DIR"
echo "Running test scene with verbose output..."
"$GODOT_EXECUTABLE" --path . --headless --verbose tests/RadioIntegrationTestScene.tscn

# Get the exit code
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo "All tests passed!"
else
    echo "Some tests failed. See above for details."
fi

# Clean up the test scene file
rm "$TEST_SCENE_PATH"
echo "Cleaned up test scene"

exit $EXIT_CODE

#!/bin/bash

# This script runs all tests for the Signal Lost Godot project
# It tries to find Godot in common installation locations

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
    if [ -d "/Applications/Godot.app" ]; then
        echo "/Applications/Godot.app/Contents/MacOS/Godot"
        return 0
    fi

    if [ -d "/Applications/Godot_4.4.1.app" ]; then
        echo "/Applications/Godot_4.4.1.app/Contents/MacOS/Godot"
        return 0
    fi

    # Check for Godot in the current directory
    if [ -f "./Godot" ]; then
        echo "./Godot"
        return 0
    fi

    # Check for Godot in the parent directory
    if [ -f "../Godot" ]; then
        echo "../Godot"
        return 0
    fi

    # Check common installation locations on Linux
    if [ -f "/usr/bin/godot" ]; then
        echo "/usr/bin/godot"
        return 0
    fi

    if [ -f "/usr/local/bin/godot" ]; then
        echo "/usr/local/bin/godot"
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
    echo "Example: GODOT_EXECUTABLE=/path/to/godot ./run_tests.sh"
    exit 1
fi

echo "Using Godot executable: $GODOT_EXECUTABLE"

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Running tests for Signal Lost Godot project..."
echo "Project path: $DIR"

# Check if GUT is installed
if [ ! -d "$DIR/addons/gut" ]; then
    echo "GUT is not installed. Installing..."
    "$DIR/install_gut.sh"
fi

# Run the test runner script
"$GODOT_EXECUTABLE" --path "$DIR" --script tests/test_runner.gd

# Get the exit code
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo "All tests passed!"
else
    echo "Some tests failed. See above for details."
fi

exit $EXIT_CODE

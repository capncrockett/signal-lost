#!/bin/bash

# This script runs all tests for the Signal Lost Godot project
# It requires Godot to be installed and available in the PATH

# Check if Godot is installed
if ! command -v godot &> /dev/null; then
    echo "Error: Godot is not installed or not in PATH"
    echo "Please install Godot from https://godotengine.org/download"
    exit 1
fi

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Running tests for Signal Lost Godot project..."
echo "Project path: $DIR"

# Run the test runner script
godot --path "$DIR" --script Tests/TestRunner.cs

# Get the exit code
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo "All tests passed!"
else
    echo "Some tests failed. See above for details."
fi

exit $EXIT_CODE

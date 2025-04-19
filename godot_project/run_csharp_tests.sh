#!/bin/bash

# Skip building the project during transition
# echo "Building project..."
# dotnet build

# Run the tests using Godot
echo "Running tests..."
/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path . --script tests/TestRunner.cs

echo "Tests completed."

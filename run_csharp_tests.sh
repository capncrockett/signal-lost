#!/bin/bash

# Build the project
echo "Building project..."
cd godot_project && dotnet build SignalLost.csproj

# Run the tests
echo "Running tests..."
cd .. && /Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path godot_project tests/CSharpTestScene.tscn

echo "Tests completed."

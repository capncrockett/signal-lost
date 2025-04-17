#!/bin/bash

# Build the project
echo "Building project..."
cd godot_project && dotnet build SignalLost.csproj

# Run the tests
echo "Running C# tests..."
cd .. && /Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path godot_project tests/MacTestScene.tscn

echo "Tests completed."

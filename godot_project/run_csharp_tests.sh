#!/bin/bash

# Build the test project
echo "Building test project..."
dotnet build SignalLostTests.csproj

# Run the tests using Godot
echo "Running tests..."
/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path . tests/CSharpTestScene.tscn

echo "Tests completed."

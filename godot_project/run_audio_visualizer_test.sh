#!/bin/bash

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Find the Godot executable
GODOT_EXECUTABLE="C:\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64_console.exe"

# Check if Godot executable exists
if [ ! -f "$GODOT_EXECUTABLE" ]; then
    echo "Godot executable not found at $GODOT_EXECUTABLE"
    echo "Please install Godot or update the script with the correct path."
    exit 1
fi

echo "Using Godot executable: $GODOT_EXECUTABLE"
echo "Running Audio Visualizer Test..."
echo "Project path: $DIR"

# Run the audio visualizer test scene
"$GODOT_EXECUTABLE" --path "$DIR" --headless tests/audio_visualizer/SimpleAudioVisualizerTestScene.tscn

# Get the exit code
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo "Audio Visualizer Test completed successfully!"
else
    echo "Audio Visualizer Test failed with exit code $EXIT_CODE"
fi

exit $EXIT_CODE

#!/bin/bash

# Cleanup script for Signal Lost project
# Removes all gitignored files and directories that are still in the repository

echo "Starting cleanup of gitignored files..."

# Remove .godot directory
echo "Removing .godot directory..."
rm -rf ./godot_project/.godot

# Remove logs directory
echo "Removing logs directory..."
rm -rf ./godot_project/logs

# Remove .uid files
echo "Removing .uid files..."
find ./godot_project -name "*.uid" -delete

# Remove .import files
echo "Removing .import files..."
find ./godot_project -name "*.import" -delete

# Remove .tmp files
echo "Removing .tmp files..."
find ./godot_project -name "*.tmp" -delete

# Remove .bak files
echo "Removing .bak files..."
find ./godot_project -name "*.bak" -delete

# Remove Gut-9.3.0 directory (it's a duplicate of the addon)
echo "Removing Gut-9.3.0 directory..."
rm -rf ./Gut-9.3.0

# Remove remaining GDScript files (except in addons)
echo "Removing remaining GDScript files..."
find ./godot_project -name "*.gd" | grep -v "addons" | xargs rm -f

# Update scene files to use C# scripts
echo "Updating scene files to use C# scripts..."

# Update RadioTuner.tscn
sed -i 's|path="res://Scenes/Radio/RadioTuner.gd"|path="res://scripts/RadioTuner.cs"|g' ./godot_project/scenes/radio/RadioTuner.tscn
sed -i 's|path="res://Scenes/Radio/AudioVisualizer.gd"|path="res://scripts/AudioVisualizer.cs"|g' ./godot_project/scenes/radio/RadioTuner.tscn

# Update test scenes
sed -i 's|path="res://tests/ComprehensiveTestRunner.gd"|path="res://tests/CSharpTestRunner.cs"|g' ./godot_project/tests/ComprehensiveTestRunnerScene.tscn
sed -i 's|path="res://tests/force_exit.gd"|path="res://tests/TestRunner.cs"|g' ./godot_project/tests/ComprehensiveTestRunnerScene.tscn

sed -i 's|path="res://tests/SimpleTest.gd"|path="res://tests/SimpleTest.cs"|g' ./godot_project/tests/SimpleTestScene.tscn
sed -i 's|path="res://tests/AudioVisualizerTest.gd"|path="res://tests/SimpleTest.cs"|g' ./godot_project/tests/AudioVisualizerTestScene.tscn
sed -i 's|path="res://tests/FullIntegrationTest.gd"|path="res://tests/IntegrationTests.cs"|g' ./godot_project/tests/FullIntegrationTestScene.tscn
sed -i 's|path="res://tests/agent_beta_test.gd"|path="res://tests/SimpleTest.cs"|g' ./godot_project/tests/AgentBetaTestScene.tscn
sed -i 's|path="res://tests/agent_beta_modification_test.gd"|path="res://tests/SimpleTest.cs"|g' ./godot_project/tests/AgentBetaModificationTestScene.tscn
sed -i 's|path="res://tests/audio_visualizer/SimpleAudioVisualizerTest.gd"|path="res://tests/SimpleTest.cs"|g' ./godot_project/tests/audio_visualizer/SimpleAudioVisualizerTestScene.tscn

# Update GutTestRunner.tscn
sed -i 's|_select_script = "res://tests/unit/test_game_state.gd"|_select_script = "res://tests/GameStateTests.cs"|g' ./godot_project/tests/GutTestRunner.tscn
sed -i 's|_directory1 = "res://tests/unit"|_directory1 = "res://tests"|g' ./godot_project/tests/GutTestRunner.tscn
sed -i 's|_directory2 = "res://tests/integration"|_directory2 = ""|g' ./godot_project/tests/GutTestRunner.tscn
sed -i 's|_post_run_script = "res://tests/scripts/post_run.gd"|_post_run_script = ""|g' ./godot_project/tests/GutTestRunner.tscn

echo "Cleanup complete!"

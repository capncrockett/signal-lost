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

echo "Cleanup complete!"

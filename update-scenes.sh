#!/bin/bash

# Script to update scene files to use pixel-based implementations
# This script updates scene files to reference the correct C# scripts

echo "Updating scene files to use pixel-based implementations..."

# List of scene files to update
find godot_project -name "*.tscn" > /tmp/scene_files.txt

# Count files
FILE_COUNT=$(wc -l < /tmp/scene_files.txt)
echo "Found $FILE_COUNT scene files to update"

# Update scene files
while IFS= read -r file; do
  echo "Updating: $file"
  
  # Replace old script references with pixel-based implementations
  sed -i.bak 's|path="res://scripts/RadioTuner.cs"|path="res://scripts/PixelRadioInterface.cs"|g' "$file"
  sed -i.bak 's|path="res://scripts/InventoryUI.cs"|path="res://scripts/PixelInventoryUI.cs"|g' "$file"
  sed -i.bak 's|path="res://scripts/MapInterface.cs"|path="res://scripts/PixelMapInterface.cs"|g' "$file"
  sed -i.bak 's|path="res://scripts/MessageDisplay.cs"|path="res://scripts/PixelMessageDisplay.cs"|g' "$file"
  sed -i.bak 's|path="res://scripts/QuestUI.cs"|path="res://scripts/PixelQuestUI.cs"|g' "$file"
  
  # Remove backup files
  rm "${file}.bak"
done < /tmp/scene_files.txt

# Clean up temporary file
rm /tmp/scene_files.txt

echo "Scene file updates complete."

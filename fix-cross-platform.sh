#!/bin/bash

# Script to fix cross-platform compatibility issues
# This script updates paths and ensures compatibility between Windows and Mac

echo "Fixing cross-platform compatibility issues..."

# Fix path case sensitivity issues
echo "Fixing path case sensitivity issues..."

# List of directories to check for case sensitivity issues
DIRS_TO_CHECK=(
  "godot_project/Scenes"
  "godot_project/scripts"
  "godot_project/tests"
)

for dir in "${DIRS_TO_CHECK[@]}"; do
  echo "Checking directory: $dir"
  
  # Find all scene files
  find "$dir" -name "*.tscn" -type f | while read -r scene_file; do
    echo "  Checking scene file: $scene_file"
    
    # Extract script paths from scene file
    grep -o 'path="res://[^"]*"' "$scene_file" | sed 's/path="//;s/"$//' | while read -r script_path; do
      # Remove the res:// prefix
      rel_path="${script_path#res://}"
      
      # Check if the path exists with exact case
      if [ ! -e "godot_project/$rel_path" ]; then
        echo "    Warning: Path not found with exact case: $rel_path"
        
        # Try to find the correct case
        correct_path=$(find "godot_project" -ipath "godot_project/$rel_path" -type f | head -n 1)
        
        if [ -n "$correct_path" ]; then
          # Extract the correct path relative to godot_project
          correct_rel_path="${correct_path#godot_project/}"
          echo "    Found correct path: $correct_rel_path"
          
          # Replace the path in the scene file
          sed -i.bak "s|path=\"$script_path\"|path=\"res://$correct_rel_path\"|g" "$scene_file"
          rm "${scene_file}.bak"
        fi
      fi
    done
  done
done

# Fix TestRunner.cs to ensure it works on both platforms
echo "Updating TestRunner.cs for cross-platform compatibility..."

TEST_RUNNER="godot_project/tests/TestRunner.cs"

if [ -f "$TEST_RUNNER" ]; then
  # Make a backup
  cp "$TEST_RUNNER" "${TEST_RUNNER}.bak"
  
  # Update the file to handle platform differences
  sed -i '' 's/if (OS.GetName() == "Windows")/if (OS.GetName() == "Windows" || OS.GetName() == "macOS")/' "$TEST_RUNNER"
  
  echo "TestRunner.cs updated for cross-platform compatibility"
else
  echo "TestRunner.cs not found"
fi

echo "Cross-platform compatibility fixes complete."

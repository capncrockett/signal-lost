#!/bin/bash

# Script to clean up remaining GDScript files
# This script removes all GDScript files except those in addons

echo "Cleaning up remaining GDScript files..."

# Create a backup directory
BACKUP_DIR="gdscript_backup/$(date +%Y%m%d)"
mkdir -p "$BACKUP_DIR"
echo "Created backup directory: $BACKUP_DIR"

# Find all GDScript files except those in addons
find godot_project -name "*.gd" | grep -v "addons" > /tmp/gdscript_files.txt

# Count files
FILE_COUNT=$(wc -l < /tmp/gdscript_files.txt)
echo "Found $FILE_COUNT GDScript files to clean up"

# Move files to backup directory
while IFS= read -r file; do
  # Create directory structure in backup
  backup_path="$BACKUP_DIR/$(dirname "$file" | sed 's|godot_project/||')"
  mkdir -p "$backup_path"
  
  # Copy file to backup
  cp "$file" "$backup_path/$(basename "$file")"
  
  # Remove original file
  rm "$file"
  
  echo "Removed: $file"
done < /tmp/gdscript_files.txt

# Clean up temporary file
rm /tmp/gdscript_files.txt

echo "Cleanup complete. All GDScript files (except in addons) have been removed."
echo "Backups are stored in $BACKUP_DIR"

#!/bin/bash

# Script to clean up outdated documentation files
# This script removes migration-related documentation that is no longer needed

echo "Cleaning up outdated documentation files..."

# List of files to remove
FILES_TO_REMOVE=(
  "docs/godot-migration.md"
  "docs/csharp-migration.md"
  "docs/csharp-migration-tasks.md"
  "docs/files-to-remove.md"
  "docs/godot-cleanup-plan.md"
  "docs/sprint-godot-migration.md"
)

# Create a backup directory
BACKUP_DIR="docs/archive/$(date +%Y%m%d)"
mkdir -p "$BACKUP_DIR"
echo "Created backup directory: $BACKUP_DIR"

# Move files to backup directory
for file in "${FILES_TO_REMOVE[@]}"; do
  if [ -f "$file" ]; then
    echo "Moving $file to backup directory..."
    cp "$file" "$BACKUP_DIR/$(basename "$file")"
    rm "$file"
  else
    echo "File $file not found, skipping..."
  fi
done

echo "Cleanup complete. Outdated files have been moved to $BACKUP_DIR"
echo "You can safely delete the backup directory if you no longer need these files."

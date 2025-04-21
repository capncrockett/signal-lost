#!/bin/bash

# Enhanced script to clean up outdated documentation files
# This script handles redundant and outdated documentation

echo "Cleaning up outdated documentation files..."

# Today's date for backup directory
TODAY=$(date +%Y%m%d)
BACKUP_DIR="docs/archive/$TODAY"
mkdir -p "$BACKUP_DIR"
echo "Created backup directory: $BACKUP_DIR"

# List of potentially redundant or outdated files to archive
FILES_TO_ARCHIVE=(
  # Files that might be redundant or outdated
  "docs/memory-leak-detection.md"
  "docs/radio-dial-fix-plan.md"
  "docs/component_integration.md"
  "docs/audio-system-implementation-plan.md"
  "docs/game-content-and-progression-plan.md"
  "docs/user_documentation.md"
  # Consolidated pixel UI documentation
  "docs/pixel-ui-system.md"
  "docs/pixel-ui-development.md"
  "docs/README-pixel-ui.md"
)

# Move files to backup directory
for file in "${FILES_TO_ARCHIVE[@]}"; do
  if [ -f "$file" ]; then
    echo "Moving $file to backup directory..."
    cp "$file" "$BACKUP_DIR/$(basename "$file")"
    rm "$file"
  else
    echo "File $file not found, skipping..."
  fi
done

# Clean up redundant archive folders
echo "Checking for redundant archive folders..."

# Get a list of archive folders older than 30 days
OLD_ARCHIVES=$(find docs/archive -type d -name "2025*" -mtime +30 2>/dev/null)

if [ -n "$OLD_ARCHIVES" ]; then
  echo "Found old archive folders to clean up:"
  echo "$OLD_ARCHIVES"

  # Ask for confirmation
  read -p "Do you want to remove these old archive folders? (y/n): " CONFIRM
  if [ "$CONFIRM" = "y" ]; then
    echo "$OLD_ARCHIVES" | xargs rm -rf
    echo "Old archive folders removed."
  else
    echo "Skipping removal of old archive folders."
  fi
else
  echo "No old archive folders found."
fi

# Update docs/README.md to reflect current state
echo "Updating documentation index..."

# Create a list of current documentation files
DOC_LIST=$(find docs -maxdepth 1 -type f -name "*.md" | sort | grep -v "README.md")

echo "Documentation cleanup complete."
echo "Outdated files have been moved to $BACKUP_DIR"
echo "You may want to update docs/README.md to reflect the current documentation state."


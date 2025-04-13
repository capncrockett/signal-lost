#!/bin/bash

# Cleanup script for Godot migration
# This script removes all unnecessary files for the Godot implementation

echo "Starting cleanup for Godot migration..."

# Files and directories to keep
KEEP=(
  ".git"
  ".github"
  ".gitignore"
  "assets"
  "docs"
  "godot_project"
  "README.md"
  "CONTRIBUTING.md"
  "cleanup.sh"
)

# Function to check if a file/directory should be kept
should_keep() {
  local path="$1"
  for keep in "${KEEP[@]}"; do
    if [[ "$path" == "$keep" || "$path" == "$keep/"* ]]; then
      return 0
    fi
  done
  return 1
}

# List all files and directories in the current directory
for item in *; do
  if should_keep "$item"; then
    echo "Keeping: $item"
  else
    echo "Removing: $item"
    if [ -d "$item" ]; then
      rm -rf "$item"
    else
      rm -f "$item"
    fi
  fi
done

# Remove specific files within kept directories
echo "Cleaning up specific files within kept directories..."

# Remove React-specific documentation
find docs -name "*react*" -type f -delete
find docs -name "*typescript*" -type f -delete
find docs -name "*eslint*" -type f -delete
find docs -name "*playwright*" -type f -delete
find docs -name "*jest*" -type f -delete

# Remove React-specific configuration files
echo "Removing React-specific configuration files..."
rm -f .eslintrc.json
rm -f .prettierrc
rm -f .lintstagedrc.json
rm -rf .husky

echo "Cleanup complete!"
echo "The repository is now ready for Godot development."

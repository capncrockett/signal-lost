#!/bin/bash

# This script installs the GUT (Godot Unit Testing) addon

# Get the directory of the script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Create the addons directory if it doesn't exist
mkdir -p "$DIR/addons"

# Check if GUT is already installed
if [ -d "$DIR/addons/gut" ]; then
    echo "GUT is already installed."
    exit 0
fi

# Clone the GUT repository
echo "Cloning GUT repository..."
git clone https://github.com/bitwes/Gut.git "$DIR/addons/gut"

# Remove the .git directory to avoid conflicts
rm -rf "$DIR/addons/gut/.git"

echo "GUT has been installed successfully."

#!/bin/bash
# Unified utility tools script for Signal Lost
# Usage: ./scripts/common/utils/tools.sh COMMAND [options]
# Commands:
#   install-gut                Install GUT testing framework
#   cleanup                    Clean up temporary files
#   screenshot [--type=TYPE]   Take a screenshot (types: game, pixel, radio)
#   fix-cross-platform         Fix cross-platform compatibility issues
#   read-logs                  Read and analyze the latest log file

# Check if a command was provided
if [ $# -eq 0 ]; then
  echo "No command specified."
  echo "Usage: ./scripts/common/utils/tools.sh COMMAND [options]"
  echo "Commands:"
  echo "  install-gut                Install GUT testing framework"
  echo "  cleanup                    Clean up temporary files"
  echo "  screenshot [--type=TYPE]   Take a screenshot (types: game, pixel, radio)"
  echo "  fix-cross-platform         Fix cross-platform compatibility issues"
  echo "  read-logs                  Read and analyze the latest log file"
  exit 1
fi

# Get the command
COMMAND=$1
shift

# Get the project directory
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../godot_project" && pwd)"
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)"

# Find the Godot executable
if [[ "$OSTYPE" == "darwin"* ]]; then
  # macOS
  GODOT_EXECUTABLE="/Applications/Godot_mono.app/Contents/MacOS/Godot"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
  # Linux
  GODOT_EXECUTABLE="godot"
else
  # Windows with WSL or Git Bash
  GODOT_EXECUTABLE="godot"
fi

# Check if GODOT_PATH environment variable is set
if [ -n "$GODOT_PATH" ]; then
  GODOT_EXECUTABLE="$GODOT_PATH"
fi

# Process the command
case $COMMAND in
  "install-gut")
    echo "Installing GUT testing framework..."
    
    # Create the addons directory if it doesn't exist
    mkdir -p "$PROJECT_DIR/addons"
    
    # Remove existing GUT installation if it exists
    if [ -d "$PROJECT_DIR/addons/gut" ]; then
      echo "Removing existing GUT installation..."
      rm -rf "$PROJECT_DIR/addons/gut"
    fi
    
    # Clone the GUT repository
    echo "Cloning GUT repository..."
    git clone https://github.com/bitwes/Gut.git "$PROJECT_DIR/addons/gut"
    
    # Remove the .git directory to avoid conflicts
    rm -rf "$PROJECT_DIR/addons/gut/.git"
    
    echo "GUT has been installed successfully."
    ;;
    
  "cleanup")
    echo "Cleaning up temporary files..."
    
    # Remove .godot directory
    echo "Removing .godot directory..."
    rm -rf "$PROJECT_DIR/.godot"
    
    # Remove logs directory
    echo "Removing logs directory..."
    rm -rf "$PROJECT_DIR/logs"
    
    # Remove .uid files
    echo "Removing .uid files..."
    find "$PROJECT_DIR" -name "*.uid" -delete
    
    # Remove .import files
    echo "Removing .import files..."
    find "$PROJECT_DIR" -name "*.import" -delete
    
    # Remove .tmp files
    echo "Removing .tmp files..."
    find "$PROJECT_DIR" -name "*.tmp" -delete
    
    # Remove .bak files
    echo "Removing .bak files..."
    find "$PROJECT_DIR" -name "*.bak" -delete
    
    # Remove Gut-9.3.0 directory (it's a duplicate of the addon)
    echo "Removing Gut-9.3.0 directory..."
    rm -rf "$REPO_ROOT/Gut-9.3.0"
    
    echo "Cleanup complete!"
    ;;
    
  "screenshot")
    # Default screenshot type
    SCREENSHOT_TYPE="game"
    
    # Parse options
    for arg in "$@"; do
      case $arg in
        --type=*)
          SCREENSHOT_TYPE="${arg#*=}"
          shift
          ;;
      esac
    done
    
    # Generate timestamp
    TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
    
    # Take the screenshot
    case $SCREENSHOT_TYPE in
      "game")
        echo "Taking game screenshot..."
        "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --script "scripts/utils/TakeScreenshot.cs" -- "game_$TIMESTAMP"
        ;;
      "pixel")
        echo "Taking pixel drawing screenshot..."
        "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --script "scripts/utils/TakeScreenshot.cs" -- "pixel_drawing_$TIMESTAMP"
        ;;
      "radio")
        echo "Taking radio tuner screenshot..."
        "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --script "scripts/utils/TakeScreenshot.cs" -- "radio_$TIMESTAMP"
        ;;
      *)
        echo "Unknown screenshot type: $SCREENSHOT_TYPE"
        echo "Valid types: game, pixel, radio"
        exit 1
        ;;
    esac
    
    echo "Screenshot saved to ~/Library/Application Support/Godot/app_userdata/Signal Lost/Screenshots/"
    echo "You can view the most recent screenshot with: open ~/Library/Application\ Support/Godot/app_userdata/Signal\ Lost/Screenshots/${SCREENSHOT_TYPE}_*.png"
    ;;
    
  "fix-cross-platform")
    echo "Fixing cross-platform compatibility issues..."
    
    # Fix path case sensitivity issues
    echo "Fixing path case sensitivity issues..."
    
    # List of directories to check for case sensitivity issues
    DIRS_TO_CHECK=(
      "$PROJECT_DIR/Scenes"
      "$PROJECT_DIR/scripts"
      "$PROJECT_DIR/tests"
    )
    
    for dir in "${DIRS_TO_CHECK[@]}"; do
      echo "Checking directory: $dir"
      
      # Find all scene files
      find "$dir" -name "*.tscn" -type f 2>/dev/null | while read -r scene_file; do
        echo "  Checking scene file: $scene_file"
        
        # Check for case sensitivity issues in paths
        grep -o 'path="res://[^"]*"' "$scene_file" | while read -r path_match; do
          # Extract the path
          res_path="${path_match#path=\"res://}"
          res_path="${res_path%\"}"
          
          # Convert to filesystem path
          fs_path="$PROJECT_DIR/$res_path"
          
          # Check if the file exists with the exact case
          if [ ! -f "$fs_path" ] && [ ! -d "$fs_path" ]; then
            echo "    Case sensitivity issue detected: $res_path"
            
            # Try to find the file with a case-insensitive search
            found_path=$(find "$PROJECT_DIR" -ipath "$fs_path" -type f 2>/dev/null | head -n 1)
            
            if [ -n "$found_path" ]; then
              # Extract the correct case path relative to the project
              correct_path="${found_path#$PROJECT_DIR/}"
              echo "    Found correct path: $correct_path"
              
              # Replace the path in the scene file
              sed -i "s|path=\"res://$res_path\"|path=\"res://$correct_path\"|g" "$scene_file"
              echo "    Fixed path in $scene_file"
            fi
          fi
        done
      done
    done
    
    echo "Cross-platform compatibility fixes complete!"
    ;;
    
  "read-logs")
    echo "Reading and analyzing the latest log file..."
    
    # Find the latest log file
    LOGS_DIR="$PROJECT_DIR/logs"
    if [ ! -d "$LOGS_DIR" ]; then
      echo "Logs directory not found: $LOGS_DIR"
      exit 1
    fi
    
    LATEST_LOG=$(find "$LOGS_DIR" -name "godot_log_*.log" -type f -print0 | xargs -0 ls -t | head -n 1)
    
    if [ -z "$LATEST_LOG" ]; then
      echo "No log files found in $LOGS_DIR"
      exit 1
    fi
    
    echo "Latest log file: $LATEST_LOG"
    
    # Display options
    echo
    echo "Choose an option:"
    echo "1. Show full log"
    echo "2. Show only errors and warnings"
    echo "3. Show only errors"
    echo "4. Show only warnings"
    echo "5. Search for specific text"
    echo
    
    read -p "Enter option (1-5): " OPTION
    
    case $OPTION in
      1)
        cat "$LATEST_LOG"
        ;;
      2)
        grep -E "ERROR|WARNING|SCRIPT ERROR" "$LATEST_LOG"
        ;;
      3)
        grep -E "ERROR|SCRIPT ERROR" "$LATEST_LOG"
        ;;
      4)
        grep -E "WARNING" "$LATEST_LOG"
        ;;
      5)
        read -p "Enter text to search for: " SEARCH_TEXT
        grep -i "$SEARCH_TEXT" "$LATEST_LOG"
        ;;
      *)
        echo "Invalid option."
        exit 1
        ;;
    esac
    ;;
    
  *)
    echo "Unknown command: $COMMAND"
    echo "Usage: ./scripts/common/utils/tools.sh COMMAND [options]"
    echo "Commands:"
    echo "  install-gut                Install GUT testing framework"
    echo "  cleanup                    Clean up temporary files"
    echo "  screenshot [--type=TYPE]   Take a screenshot (types: game, pixel, radio)"
    echo "  fix-cross-platform         Fix cross-platform compatibility issues"
    echo "  read-logs                  Read and analyze the latest log file"
    exit 1
    ;;
esac

exit 0

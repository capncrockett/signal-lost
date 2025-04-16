#!/bin/bash
# This script runs the Godot project and captures logs to a file

echo "Running Signal Lost Godot project with log capture..."

# Set the Godot executable path based on OS
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    GODOT_EXECUTABLE="/Applications/Godot.app/Contents/MacOS/Godot"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Linux
    GODOT_EXECUTABLE="godot"
else
    # Windows with WSL or Git Bash
    GODOT_EXECUTABLE="C:/Godot_v4.4.1-stable_mono_win64/Godot_v4.4.1-stable_mono_win64/Godot_v4.4.1-stable_mono_win64_console.exe"
fi

# Check if Godot executable exists
if [ ! -f "$GODOT_EXECUTABLE" ]; then
    echo "Error: Godot executable not found at $GODOT_EXECUTABLE"
    echo "Please install Godot or update the script with the correct path."
    exit 1
fi

echo "Using Godot executable: $GODOT_EXECUTABLE"

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

echo "Project path: $DIR"

# Create logs directory if it doesn't exist
mkdir -p "$DIR/logs"

# Generate timestamp for log file
TIMESTAMP=$(date +"%Y%m%d-%H%M%S")

# Set log file path
LOG_FILE="$DIR/logs/godot_log_$TIMESTAMP.log"

echo "Log will be saved to: $LOG_FILE"

# Run the project with log file capture
"$GODOT_EXECUTABLE" --path "$DIR" --log-file "$LOG_FILE" "$@"

# Get the exit code
EXIT_CODE=$?

# Filter the log file to show only errors and warnings
echo
echo "===== ERRORS AND WARNINGS ====="
grep -E "ERROR|WARNING|SCRIPT ERROR" "$LOG_FILE"
echo "================================"

echo "Full log saved to: $LOG_FILE"

exit $EXIT_CODE

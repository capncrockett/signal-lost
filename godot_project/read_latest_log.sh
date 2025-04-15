#!/bin/bash
# This script reads and displays the latest Godot log file

echo "Reading latest Godot log file..."

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Check if logs directory exists
if [ ! -d "$DIR/logs" ]; then
    echo "Error: Logs directory not found."
    echo "Please run the project with run_with_logs.sh first."
    exit 1
fi

# Find the latest log file
LATEST_LOG=$(ls -t "$DIR/logs"/godot_log_*.log 2>/dev/null | head -n 1)

if [ -z "$LATEST_LOG" ]; then
    echo "Error: No log files found."
    echo "Please run the project with run_with_logs.sh first."
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

exit 0

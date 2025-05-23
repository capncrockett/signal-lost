#!/bin/bash
# This script runs the Godot project and generates an error report

echo "Running Signal Lost Godot project with error reporting..."

# Set the Godot executable path
# Check if GODOT_PATH environment variable is set
if [ -n "$GODOT_PATH" ]; then
    GODOT_EXECUTABLE="$GODOT_PATH"
else
    # Default based on OS
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        GODOT_EXECUTABLE="/Applications/Godot.app/Contents/MacOS/Godot"
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        # Linux
        GODOT_EXECUTABLE="godot"
    else
        # Windows with WSL or Git Bash
        GODOT_EXECUTABLE="godot"
    fi
fi

# Check if Godot executable exists or is in PATH
if ! command -v "$GODOT_EXECUTABLE" &> /dev/null; then
    echo "Error: Godot executable not found at $GODOT_EXECUTABLE or in PATH"
    echo "Please install Godot, add it to PATH, or set the GODOT_PATH environment variable."
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
ERROR_REPORT="$DIR/logs/error_report_$TIMESTAMP.md"

echo "Log will be saved to: $LOG_FILE"
echo "Error report will be saved to: $ERROR_REPORT"

# Run the project with log file capture
"$GODOT_EXECUTABLE" --path "$DIR" --log-file "$LOG_FILE" "$@"

# Get the exit code
EXIT_CODE=$?

# Generate error report
echo "# Signal Lost Error Report" > "$ERROR_REPORT"
echo "" >> "$ERROR_REPORT"
echo "Generated: $TIMESTAMP" >> "$ERROR_REPORT"
echo "" >> "$ERROR_REPORT"
echo "## Errors" >> "$ERROR_REPORT"
echo "" >> "$ERROR_REPORT"
grep -E "ERROR|SCRIPT ERROR" "$LOG_FILE" >> "$ERROR_REPORT"
echo "" >> "$ERROR_REPORT"
echo "## Warnings" >> "$ERROR_REPORT"
echo "" >> "$ERROR_REPORT"
grep -E "WARNING" "$LOG_FILE" >> "$ERROR_REPORT"

# Display summary
echo
echo "===== ERROR REPORT SUMMARY ====="
echo "Errors: $(grep -E "ERROR|SCRIPT ERROR" "$LOG_FILE" | wc -l)"
echo "Warnings: $(grep -E "WARNING" "$LOG_FILE" | wc -l)"
echo "================================"

echo "Full log saved to: $LOG_FILE"
echo "Error report saved to: $ERROR_REPORT"

exit $EXIT_CODE

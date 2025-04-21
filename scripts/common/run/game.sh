#!/bin/bash
# Unified game runner script for Signal Lost
# Usage: ./scripts/common/run/game.sh [options]
# Options:
#   --scene=NAME     Run a specific scene (e.g., main, pixel, radio)
#   --verbose        Run with verbose output
#   --logs           Run with log capture
#   --error-report   Run with error reporting

# Default values
SCENE="MainGameScene"
VERBOSE=false
LOGS=false
ERROR_REPORT=false

# Parse command line arguments
for arg in "$@"; do
  case $arg in
    --scene=*)
      SCENE_ARG="${arg#*=}"
      case $SCENE_ARG in
        main)
          SCENE="MainGameScene"
          ;;
        pixel)
          SCENE="PixelDrawingDemo"
          ;;
        radio)
          SCENE="RadioTunerScene"
          ;;
        *)
          SCENE=$SCENE_ARG
          ;;
      esac
      shift
      ;;
    --verbose)
      VERBOSE=true
      shift
      ;;
    --logs)
      LOGS=true
      shift
      ;;
    --error-report)
      ERROR_REPORT=true
      shift
      ;;
    *)
      # Unknown option
      echo "Unknown option: $arg"
      echo "Usage: ./scripts/common/run/game.sh [--scene=NAME] [--verbose] [--logs] [--error-report]"
      exit 1
      ;;
  esac
done

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

# Check if Godot executable exists
if [ ! -f "$GODOT_EXECUTABLE" ]; then
  echo "Godot executable not found at $GODOT_EXECUTABLE"
  echo "Please install Godot or set the GODOT_PATH environment variable."
  exit 1
fi

echo "Using Godot executable: $GODOT_EXECUTABLE"

# Get the project directory
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../../godot_project" && pwd)"
echo "Project path: $PROJECT_DIR"

# Create command arguments
CMD_ARGS="--path \"$PROJECT_DIR\""

if [ "$VERBOSE" = true ]; then
  CMD_ARGS="$CMD_ARGS --verbose"
fi

if [ "$LOGS" = true ]; then
  # Create logs directory if it doesn't exist
  mkdir -p "$PROJECT_DIR/logs"
  
  # Generate timestamp for log file
  TIMESTAMP=$(date +"%Y%m%d-%H%M%S")
  
  # Set log file path
  LOG_FILE="$PROJECT_DIR/logs/godot_log_$TIMESTAMP.log"
  
  echo "Log will be saved to: $LOG_FILE"
  CMD_ARGS="$CMD_ARGS --log-file \"$LOG_FILE\""
fi

if [ "$ERROR_REPORT" = true ]; then
  # Create logs directory if it doesn't exist
  mkdir -p "$PROJECT_DIR/logs"
  
  # Generate timestamp for log file
  TIMESTAMP=$(date +"%Y%m%d-%H%M%S")
  
  # Set log file path
  LOG_FILE="$PROJECT_DIR/logs/godot_log_$TIMESTAMP.log"
  ERROR_REPORT_FILE="$PROJECT_DIR/logs/error_report_$TIMESTAMP.md"
  
  echo "Log will be saved to: $LOG_FILE"
  echo "Error report will be saved to: $ERROR_REPORT_FILE"
  CMD_ARGS="$CMD_ARGS --log-file \"$LOG_FILE\""
fi

# Run the scene
cd "$PROJECT_DIR"
COMMAND="$GODOT_EXECUTABLE $CMD_ARGS --scene scenes/$SCENE.tscn"
echo "Running command: $COMMAND"
eval $COMMAND

# Get the exit code
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
  echo "Game ran successfully!"
else
  echo "Game failed to run. See above for details."
fi

# Generate error report if requested
if [ "$ERROR_REPORT" = true ] && [ -f "$LOG_FILE" ]; then
  echo "Generating error report..."
  echo "# Signal Lost Error Report" > "$ERROR_REPORT_FILE"
  echo "" >> "$ERROR_REPORT_FILE"
  echo "Generated: $(date)" >> "$ERROR_REPORT_FILE"
  echo "" >> "$ERROR_REPORT_FILE"
  echo "## Errors" >> "$ERROR_REPORT_FILE"
  echo "" >> "$ERROR_REPORT_FILE"
  echo '```' >> "$ERROR_REPORT_FILE"
  grep -E "ERROR|SCRIPT ERROR" "$LOG_FILE" >> "$ERROR_REPORT_FILE"
  echo '```' >> "$ERROR_REPORT_FILE"
  echo "" >> "$ERROR_REPORT_FILE"
  echo "## Warnings" >> "$ERROR_REPORT_FILE"
  echo "" >> "$ERROR_REPORT_FILE"
  echo '```' >> "$ERROR_REPORT_FILE"
  grep -E "WARNING" "$LOG_FILE" >> "$ERROR_REPORT_FILE"
  echo '```' >> "$ERROR_REPORT_FILE"
  echo "" >> "$ERROR_REPORT_FILE"
  echo "## Full Log" >> "$ERROR_REPORT_FILE"
  echo "" >> "$ERROR_REPORT_FILE"
  echo "See: $LOG_FILE" >> "$ERROR_REPORT_FILE"
fi

exit $EXIT_CODE

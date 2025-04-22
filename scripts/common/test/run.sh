#!/bin/bash
# Unified test runner script for Signal Lost
# Usage: ./scripts/common/test/run.sh [options]
# Options:
#   --gut             Run GUT tests
#   --radio           Run radio tests
#   --pixel           Run pixel UI tests
#   --integration     Run integration tests
#   --class=NAME      Run specific test class
#   --skip=CLASSES    Skip specific test classes (comma-separated)

# Default values
TEST_TYPE="all"
TEST_CLASS=""
SKIP_CLASSES=""

# Parse command line arguments
for arg in "$@"; do
  case $arg in
    --gut)
      TEST_TYPE="gut"
      shift
      ;;
    --radio)
      TEST_TYPE="radio"
      shift
      ;;
    --pixel)
      TEST_TYPE="pixel"
      shift
      ;;
    --integration)
      TEST_TYPE="integration"
      shift
      ;;
    --class=*)
      TEST_CLASS="${arg#*=}"
      shift
      ;;
    --skip=*)
      SKIP_CLASSES="${arg#*=}"
      shift
      ;;
    *)
      # Unknown option
      echo "Unknown option: $arg"
      echo "Usage: ./scripts/common/test/run.sh [--gut] [--radio] [--pixel] [--integration] [--class=NAME] [--skip=CLASSES]"
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

# Check if GUT is installed
if [ ! -d "$PROJECT_DIR/addons/gut" ]; then
  echo "GUT is not installed. Installing..."
  
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
fi

# Run the appropriate tests
cd "$PROJECT_DIR"

case $TEST_TYPE in
  "gut")
    echo "Running GUT tests..."
    "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --headless tests/GutTestRunner.tscn
    ;;
  "radio")
    echo "Running Radio Tuner tests..."
    "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --headless tests/FullIntegrationTestScene.tscn
    ;;
  "pixel")
    echo "Running Pixel UI tests..."
    "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --headless --script "tests/TestRunner.cs" -- --class="PixelInventoryUITests,PixelMapInterfaceTests"
    ;;
  "integration")
    echo "Running integration tests..."
    "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --headless --script "tests/TestRunner.cs" -- --class="IntegrationTests"
    ;;
  "all")
    if [ -n "$TEST_CLASS" ]; then
      echo "Running specific test class: $TEST_CLASS"
      "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --headless --script "tests/TestRunner.cs" -- --class="$TEST_CLASS"
    elif [ -n "$SKIP_CLASSES" ]; then
      echo "Running all tests except: $SKIP_CLASSES"
      "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --headless --script "tests/TestRunner.cs" -- --skip-classes="$SKIP_CLASSES"
    else
      echo "Running all tests..."
      "$GODOT_EXECUTABLE" --path "$PROJECT_DIR" --headless --script "tests/TestRunner.cs"
    fi
    ;;
esac

# Get the exit code
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
  echo "All tests passed!"
else
  echo "Some tests failed. See above for details."
fi

exit $EXIT_CODE

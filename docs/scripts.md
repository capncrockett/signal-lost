# Signal Lost Scripts

This document outlines all available scripts for running, testing, and maintaining the Signal Lost project.

## Directory Structure

```
/scripts
  /windows          # Windows-specific scripts (.bat)
  /mac              # Mac-specific scripts (.sh)
  /common           # Cross-platform scripts (with both .sh and .bat versions)
    /run            # Scripts for running the game
    /test           # Scripts for testing
    /utils          # Utility scripts
```

## Running the Game

### Basic Usage

```bash
# Mac
./scripts/mac/run_game.sh

# Windows
scripts\windows\run_game.bat
```

### Options

- `--scene=NAME` - Run a specific scene
  - `main` - Main game scene
  - `pixel` - Pixel drawing demo
  - `radio` - Radio tuner scene
  - Or specify a custom scene name

- `--verbose` - Run with verbose output
- `--logs` - Run with log capture
- `--error-report` - Run with error reporting

### Examples

```bash
# Run the main game scene
./scripts/mac/run_game.sh

# Run the pixel drawing demo
./scripts/mac/run_game.sh --scene=pixel

# Run the radio tuner scene with verbose output
./scripts/mac/run_game.sh --scene=radio --verbose

# Run with error reporting
./scripts/mac/run_game.sh --error-report
```

## Running Tests

### Basic Usage

```bash
# Mac
./scripts/mac/run_tests.sh

# Windows
scripts\windows\run_tests.bat
```

### Options

- `--gut` - Run GUT tests
- `--radio` - Run radio tests
- `--pixel` - Run pixel UI tests
- `--integration` - Run integration tests
- `--class=NAME` - Run specific test class
- `--skip=CLASSES` - Skip specific test classes (comma-separated)

### Examples

```bash
# Run all tests
./scripts/mac/run_tests.sh

# Run GUT tests
./scripts/mac/run_tests.sh --gut

# Run radio tests
./scripts/mac/run_tests.sh --radio

# Run a specific test class
./scripts/mac/run_tests.sh --class=RadioTunerTests

# Run all tests except specific classes
./scripts/mac/run_tests.sh --skip=IntegrationTests,RadioTunerTests
```

## Utility Tools

### Basic Usage

```bash
# Mac
./scripts/mac/tools.sh COMMAND [options]

# Windows
scripts\windows\tools.bat COMMAND [options]
```

### Commands

- `install-gut` - Install GUT testing framework
- `cleanup` - Clean up temporary files
- `screenshot` - Take a screenshot
- `fix-cross-platform` - Fix cross-platform compatibility issues
- `read-logs` - Read and analyze the latest log file

### Options

For the `screenshot` command:
- `--type=TYPE` - Specify the type of screenshot (game, pixel, radio)

### Examples

```bash
# Install GUT testing framework
./scripts/mac/tools.sh install-gut

# Clean up temporary files
./scripts/mac/tools.sh cleanup

# Take a game screenshot
./scripts/mac/tools.sh screenshot

# Take a pixel drawing screenshot
./scripts/mac/tools.sh screenshot --type=pixel

# Fix cross-platform compatibility issues
./scripts/mac/tools.sh fix-cross-platform

# Read and analyze the latest log file
./scripts/mac/tools.sh read-logs
```

## Migrating from Old Scripts

If you were using the old scripts, here's how to use the new ones:

| Old Script | New Script |
|------------|------------|
| `run_project.sh` | `./scripts/mac/run_game.sh` |
| `run_project_windows.bat` | `scripts\windows\run_game.bat` |
| `run_game.sh` | `./scripts/mac/run_game.sh --verbose` |
| `run_game.bat` | `scripts\windows\run_game.bat --verbose` |
| `run_pixel_drawing_demo.sh` | `./scripts/mac/run_game.sh --scene=pixel` |
| `run_pixel_drawing_demo.bat` | `scripts\windows\run_game.bat --scene=pixel` |
| `run_tests.sh` | `./scripts/mac/run_tests.sh` |
| `run_tests.bat` | `scripts\windows\run_tests.bat` |
| `run_gut_tests.sh` | `./scripts/mac/run_tests.sh --gut` |
| `run_radio_test_windows.bat` | `scripts\windows\run_tests.bat --radio` |
| `install_gut.sh` | `./scripts/mac/tools.sh install-gut` |
| `install_gut.bat` | `scripts\windows\tools.bat install-gut` |
| `run_with_logs.sh` | `./scripts/mac/run_game.sh --logs` |
| `run_with_logs.bat` | `scripts\windows\run_game.bat --logs` |
| `run_with_error_report.sh` | `./scripts/mac/run_game.sh --error-report` |
| `run_with_error_report.bat` | `scripts\windows\run_game.bat --error-report` |
| `take_game_screenshot.sh` | `./scripts/mac/tools.sh screenshot` |
| `cleanup_gitignored.sh` | `./scripts/mac/tools.sh cleanup` |
| `cleanup_gitignored.bat` | `scripts\windows\tools.bat cleanup` |
| `fix-cross-platform.sh` | `./scripts/mac/tools.sh fix-cross-platform` |

## Making Scripts Executable (Mac/Linux)

If you're on Mac or Linux, you may need to make the scripts executable:

```bash
# Make all scripts executable
chmod +x scripts/mac/*.sh
chmod +x scripts/common/run/*.sh
chmod +x scripts/common/test/*.sh
chmod +x scripts/common/utils/*.sh
```

## Setting Up Godot Path

The scripts will try to find the Godot executable in common locations. If your Godot installation is in a different location, you can set the `GODOT_PATH` environment variable:

```bash
# Mac/Linux
export GODOT_PATH=/path/to/godot

# Windows
set GODOT_PATH=C:\path\to\Godot_v4.4.1-stable_mono_win64_console.exe
```

## Troubleshooting

### Script Not Found

If you get a "script not found" error, make sure you're running the script from the root directory of the repository.

### Godot Not Found

If you get a "Godot executable not found" error, make sure Godot is installed and either:
1. It's in one of the default locations the scripts check
2. You've set the `GODOT_PATH` environment variable

### Permission Denied

If you get a "permission denied" error on Mac/Linux, make sure the scripts are executable:

```bash
chmod +x scripts/mac/*.sh
```

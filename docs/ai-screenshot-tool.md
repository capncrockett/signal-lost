# AI Screenshot Tool

## Overview

The AI Screenshot Tool is a utility designed specifically for AI agents to visually verify the state of the game during development. It provides a reliable way to capture screenshots of the game and save them to a consistent location across platforms.

## Purpose

Unlike traditional screenshot features intended for end users, this tool serves as a development aid that allows AI agents to:

1. Visually verify changes made to the game
2. Understand the current state of the UI and game elements
3. Document visual progress and changes over time
4. Debug visual issues by capturing the current state

## Usage

To take a screenshot of the main game scene:

```bash
# From the godot_project directory
./take_game_screenshot.sh
```

This will:
1. Load the main game scene (PixelMainScene.tscn)
2. Wait 5 seconds for the scene to fully render
3. Take a screenshot and save it to the user data directory
4. Automatically exit after taking the screenshot

## Screenshot Location

Screenshots are saved to Godot's user data directory, which provides cross-platform compatibility:

- On macOS: `~/Library/Application Support/Godot/app_userdata/Signal Lost/Screenshots/`
- On Windows: `%APPDATA%\Godot\app_userdata\Signal Lost\Screenshots\`
- On Linux: `~/.local/share/godot/app_userdata/[Project Name]/`

The filenames include timestamps for easy identification: `game_screenshot_YYYYMMDD_HHMMSS.png`

## Implementation Details

The screenshot functionality is implemented using:

1. **game_scene_screenshot.gd**: A GDScript that loads the main game scene, waits for it to render, and takes a screenshot
2. **game_scene_screenshot.tscn**: A scene that uses the script
3. **take_game_screenshot.sh**: A shell script that runs the scene

The implementation uses Godot's `user://` directory for cross-platform compatibility, ensuring that screenshots can be taken and accessed without special permissions on any platform.

## Benefits for AI Development

This tool is essential for AI-assisted development because:

1. It provides visual feedback that text descriptions alone cannot convey
2. It allows AI agents to verify that visual changes have been implemented correctly
3. It helps identify UI issues that might not be apparent from code inspection
4. It creates a visual record of development progress over time

## Cross-Platform Considerations

The screenshot tool is designed to work consistently across different platforms:

- Uses Godot's `user://` directory instead of platform-specific paths
- Doesn't require special permissions to save files
- Creates directories if they don't exist
- Uses consistent naming conventions across platforms

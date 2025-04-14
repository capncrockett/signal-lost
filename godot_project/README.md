# Signal Lost - Godot C# Implementation

This is the Godot C# implementation of the Signal Lost game, migrated from the original browser-based React application.

## Project Structure

- `scenes/` - Contains all game scenes
  - `radio/` - Radio tuner and related scenes
- `Scripts/` - Contains all C# script files
- `assets/` - Contains all game assets
  - `audio/` - Audio files
  - `images/` - Image files
- `Tests/` - Contains all C# test scripts

## Key Components

### GameState

The `GameState` script is an autoloaded singleton that manages the game's state, including:

- Current radio frequency
- Radio power state
- Discovered frequencies
- Game progress
- Signal and message data

### AudioManager

The `AudioManager` script is an autoloaded singleton that handles all audio-related functionality:

- Playing static noise
- Playing signal tones
- Managing audio effects
- Controlling volume and mute state

### RadioTuner

The `RadioTuner` scene is the main gameplay element, allowing the player to:

- Tune to different frequencies
- Detect signals
- Display messages
- Scan the radio spectrum

## Testing

The project uses a custom testing framework for automated testing. Tests can be run from the terminal using:

```bash
./run_custom_tests.sh
```

Or for integration tests:

```bash
./run_integration_tests.sh
```

## Development

1. Install Godot 4.4.1 with Mono/C# support from [godotengine.org](https://godotengine.org/download)
2. Clone this repository
3. Open the project in Godot
4. Run the project using the provided script: `./run_project.sh`

## Running the Project

The easiest way to run the project is to use the provided script:

```bash
./run_project.sh
```

This will automatically find the Godot executable and run the project.

Alternatively, you can open the project in the Godot editor:

1. Open Godot
2. Click on "Import" or "Open"
3. Navigate to the `godot_project` directory
4. Select the `project.godot` file
5. Click "Open"
6. Once the editor is open, click the "Play" button in the top-right corner

## Building

To build the game for different platforms:

```bash
# Export for Windows
godot --path /path/to/project --export "Windows Desktop" /path/to/output.exe

# Export for macOS
godot --path /path/to/project --export "Mac OSX" /path/to/output.zip

# Export for Linux
godot --path /path/to/project --export "Linux/X11" /path/to/output.x86_64

# Export for Web
godot --path /path/to/project --export "HTML5" /path/to/output/index.html
```

## Troubleshooting

If you encounter any issues:

1. Make sure you have Godot 4.4.1 with Mono/C# support installed
2. Check that all the required assets are in the correct locations
3. Try running the project from the Godot editor to see any error messages
4. Check the console output for any error messages
5. If you see errors about missing icon.png, make sure the file exists in the assets/images directory
6. If you see errors about missing audio files, you may need to add your own audio files to the assets/audio directory

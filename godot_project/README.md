# Signal Lost - Godot Implementation

This is the Godot implementation of the Signal Lost game, migrated from the original browser-based React application.

## Project Structure

- `scenes/` - Contains all game scenes
  - `radio/` - Radio tuner and related scenes
- `scripts/` - Contains all GDScript files
- `assets/` - Contains all game assets
  - `audio/` - Audio files
  - `images/` - Image files
- `tests/` - Contains all test scripts

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

The project uses GUT (Godot Unit Testing) for automated testing. Tests can be run from the terminal using:

```bash
godot --path /path/to/project --script tests/test_runner.gd
```

Or from within the Godot editor using the GUT panel.

## Development

1. Install Godot 4.x from [godotengine.org](https://godotengine.org/download)
2. Clone this repository
3. Open the project in Godot
4. Install the GUT addon from the Asset Library

## Terminal Testing

To run tests from the terminal:

```bash
# Run all tests
godot --path /path/to/project --script tests/test_runner.gd

# Run a specific test
godot --path /path/to/project -s addons/gut/gut_cmdln.gd -gtest=res://tests/test_radio_tuner.gd
```

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

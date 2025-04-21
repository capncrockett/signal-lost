# Radio Interface Documentation

## Overview

The radio interface is a core component of Signal Lost, providing players with a way to tune into different frequencies, detect signals, and receive messages. The interface is designed with a pixel-art aesthetic and provides both visual and audio feedback.

## Components

### Visual Components

1. **Radio Interface**
   - A pixel-based visual representation of a radio
   - Includes a tuning dial, frequency display, signal strength meter, and control buttons
   - Provides visual feedback for user interactions

2. **Tuning Dial**
   - Allows players to tune to different frequencies
   - Provides visual feedback when rotating
   - Frequency range: 88.0 MHz to 108.0 MHz

3. **Frequency Display**
   - Shows the current frequency
   - Updates in real-time as the dial is turned
   - Displays "-- . - MHz" when the radio is powered off

4. **Signal Strength Meter**
   - Indicates the strength of the current signal
   - Fills from left to right as signal strength increases
   - Provides visual feedback for signal detection

5. **Control Buttons**
   - **Power Button**: Turns the radio on/off
   - **Scan Button**: Automatically scans for available signals
   - **Message Button**: Retrieves messages when available

### Audio Components

1. **Radio Audio Manager**
   - Handles all radio-specific audio effects
   - Provides UI sound effects for buttons and controls
   - Generates tuning noise when adjusting frequency
   - Integrates with the game's audio system

2. **Sound Effects**
   - **Button Clicks**: When pressing power, scan, or message buttons
   - **Dial Turning**: When adjusting frequency
   - **Static Noise**: Varies with signal strength
   - **Signal Tones**: When tuned to a frequency
   - **Squelch Effects**: When turning the radio on/off

### Integration Components

1. **Radio Interface Manager**
   - Connects the radio interface with the game's systems
   - Handles bidirectional communication between UI and game state
   - Updates the interface based on game state changes
   - Processes user interactions and updates the game state

## Usage

### Player Interaction

1. **Turning the Radio On/Off**
   - Click the power button to toggle the radio's power state
   - When off, the frequency display shows "-- . - MHz" and all audio is muted
   - When on, the frequency display shows the current frequency and audio is active

2. **Tuning to a Frequency**
   - Click and drag the tuning dial to change frequency
   - The frequency display updates in real-time
   - The signal strength meter indicates if a signal is present
   - Audio feedback changes based on signal strength

3. **Scanning for Signals**
   - Click the scan button to automatically find the next available signal
   - The dial will rotate and stop when a signal is found
   - If no signal is found, it returns to the original frequency

4. **Retrieving Messages**
   - When the message button is lit, a message is available at the current frequency
   - Click the message button to view the message content
   - Messages provide narrative content and gameplay information

### Developer Integration

1. **Adding the Radio Interface to a Scene**
   - Add the PixelRadioInterface control to your scene
   - Add the RadioInterfaceManager and RadioAudioManager nodes
   - Connect the RadioInterfaceManager to the PixelRadioInterface

2. **Connecting to Game Systems**
   - Ensure GameState and RadioSystem are available
   - The RadioInterfaceManager will automatically connect to these systems
   - Use the provided signals to respond to user interactions

3. **Customizing the Radio Interface**
   - Adjust the visual appearance by modifying the PixelRadioInterface class
   - Customize audio effects by modifying the RadioAudioManager class
   - Add new functionality by extending the RadioInterfaceManager class

## Technical Details

### Classes

1. **PixelRadioInterface**
   - Inherits from Godot's Control class
   - Handles drawing the radio interface
   - Processes user input for interaction
   - Emits signals for frequency changes, power toggles, etc.

2. **RadioInterfaceManager**
   - Inherits from Godot's Node class
   - Connects the UI with game systems
   - Handles signal detection and message availability
   - Updates the UI based on game state changes

3. **RadioAudioManager**
   - Inherits from Godot's Node class
   - Manages audio effects for the radio
   - Provides sound effects for user interactions
   - Generates dynamic audio based on signal strength

### Signals

1. **From PixelRadioInterface**
   - `FrequencyChanged(float frequency)`: Emitted when the frequency changes
   - `PowerToggle(bool isPoweredOn)`: Emitted when the power state changes
   - `ScanRequested()`: Emitted when the scan button is pressed
   - `MessageRequested()`: Emitted when the message button is pressed

2. **From RadioInterfaceManager**
   - `SignalDetected(string signalId, float frequency)`: Emitted when a signal is detected
   - `SignalLost(string signalId)`: Emitted when a signal is lost
   - `MessageReceived(string messageId)`: Emitted when a message is received

### Integration with Game Systems

1. **GameState**
   - Stores the current frequency and power state
   - Manages signals and messages
   - Provides methods for setting frequency and toggling power

2. **RadioSystem**
   - Handles signal detection and strength calculation
   - Manages signal types and properties
   - Provides methods for getting signal information

## Testing

1. **Integration Tests**
   - Verify that the radio interface works with game systems
   - Test frequency changes, power toggles, and message retrieval
   - Ensure signal detection works correctly

2. **Simple Tests**
   - Verify that the radio interface components can be created
   - Test basic functionality in isolation
   - Ensure visual and audio components work correctly

## Future Enhancements

1. **Visual Effects**
   - Add static/noise visual effects when tuning between stations
   - Implement visual feedback for signal type (Morse, voice, data)
   - Add animations for button presses and dial rotation

2. **Audio Enhancements**
   - Add more realistic static and tuning sounds
   - Implement different audio effects for different signal types
   - Add spatial audio for signal sources

3. **Gameplay Integration**
   - Connect the radio to the game's progression system
   - Add radio puzzles and challenges
   - Implement radio-based navigation and exploration

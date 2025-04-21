# Radio System

This document provides an overview of the radio system in Signal Lost, including its functionality, implementation, and how to use it in gameplay. The radio system has been significantly enhanced with the recent implementation of the PixelRadioInterface and related components.

## Table of Contents

1. [Overview](#overview)
2. [Radio Functionality](#radio-functionality)
3. [Signal Types](#signal-types)
4. [Implementation Details](#implementation-details)
5. [Integration with Gameplay](#integration-with-gameplay)
6. [Future Enhancements](#future-enhancements)

## Overview

The radio system is a core gameplay mechanic in Signal Lost. Players use the radio to:

- Discover story elements and clues
- Locate important areas on the map
- Receive guidance on objectives
- Solve puzzles through signal interpretation

The radio system features realistic audio with continuous white noise, squelch effects, and signal fading based on strength. The visual interface provides feedback on signal quality and helps players tune to the correct frequencies.

## Radio Functionality

### Basic Controls

- **Power**: Toggle the radio on/off
- **Tuning**: Adjust the frequency using the dial or keyboard
- **Scanning**: Automatically scan for signals
- **Volume**: Adjust the audio volume (if implemented)

### Signal Reception

Signal reception is affected by:

- **Distance** from the signal source
- **Obstacles** between the player and source
- **Weather conditions** in the game world
- **Time of day** for certain signals

### Visual Feedback

The radio interface provides several forms of visual feedback:

- **Signal Strength Meter**: Shows the strength of the current signal
- **Static Visualization**: Displays visual noise that correlates with audio
- **Morse Code Display**: Visualizes Morse code signals
- **Frequency Scanner**: Shows a spectrum of available signals

## Signal Types

The game features several types of radio signals:

### Morse Code

Morse code signals transmit encoded messages using dots and dashes. Players need to decode these messages to uncover important information.

Example Morse code patterns:

- SOS: `... --- ...`
- TEST: `- . ... -`

### Voice Transmissions

Voice transmissions contain spoken messages from NPCs. These may be:

- Clear transmissions with important instructions
- Distorted transmissions that need to be interpreted
- Looping emergency broadcasts

### Data Signals

Data signals represent digital information being transmitted. These appear as:

- Distinctive patterns in the static
- Regular pulses or beeps
- Unique visual patterns on the frequency scanner

### Environmental Signals

Environmental signals come from the game world itself:

- Electrical interference from power sources
- Weather-related static
- Signals from hidden locations

## Implementation Details

The radio system is implemented using several key components:

### RadioTuner

The `RadioTuner` class manages the core radio functionality:

```csharp
public class RadioTuner : Node
{
    // Frequency range
    public float MinFrequency = 88.0f;
    public float MaxFrequency = 108.0f;

    // Get/set the current frequency
    public float GetCurrentFrequency();
    public void SetFrequency(float frequency);

    // Control radio state
    public void TogglePower();
    public void ToggleScanning();
    public bool IsPowerOn();
    public bool IsScanning();

    // Get signal information
    public float GetSignalStrength();
}
```

### RadioSystem

The `RadioSystem` class manages the overall radio functionality and integrates with the game state:

```csharp
public class RadioSystem : Node
{
    // Signal management
    public void RegisterSignal(RadioSignal signal);
    public void UnregisterSignal(RadioSignal signal);
    public RadioSignal FindSignalAtFrequency(float frequency, float tolerance = 0.1f);

    // Signal detection
    public float GetSignalStrength(float frequency);
    public bool IsSignalDetected(float frequency);

    // Events
    [Signal] public delegate void SignalDetectedEventHandler(string signalId, float frequency);
    [Signal] public delegate void SignalLostEventHandler(string signalId);
}
```

### RadioAudioManager

The `RadioAudioManager` class handles all audio aspects of the radio system:

```csharp
public class RadioAudioManager : Node
{
    // Audio control
    public void StartTuning();
    public void StopTuning();
    public void SetFrequencyAndSignal(float frequency, float signalStrength);
    public void SetRadioPower(bool isOn);

    // Sound effects
    public void PlayButtonClick();
    public void PlayDialTurn();
    public void PlaySliderSound();
}
```

### Visual Interface

The visual interface is implemented using:

#### PixelRadioInterface

The `PixelRadioInterface` class provides a pixel-art style radio interface:

```csharp
public class PixelRadioInterface : Control
{
    // Radio state
    [Export] public float MinFrequency { get; set; } = 88.0f;
    [Export] public float MaxFrequency { get; set; } = 108.0f;
    [Export] public float CurrentFrequency { get; set; } = 98.0f;
    [Export] public float SignalStrength { get; set; } = 0.0f;
    [Export] public bool IsPoweredOn { get; set; } = false;

    // Interface methods
    public void SetSignalStrength(float strength);
    public void SetMessageAvailable(bool available);
    public void SetFrequency(float frequency);

    // Signals
    [Signal] public delegate void FrequencyChangedEventHandler(float frequency);
    [Signal] public delegate void PowerToggleEventHandler(bool isPoweredOn);
    [Signal] public delegate void ScanRequestedEventHandler();
    [Signal] public delegate void MessageRequestedEventHandler();
}
```

#### RadioInterfaceManager

The `RadioInterfaceManager` class connects the radio interface with the game systems:

```csharp
public class RadioInterfaceManager : Node
{
    // References
    [Export] public NodePath RadioInterfacePath { get; set; }

    // Methods
    private void SyncWithGameState();
    private void CheckMessageAvailability();

    // Event handlers
    private void OnFrequencyChanged(float frequency);
    private void OnPowerToggle(bool isPoweredOn);
    private void OnScanRequested();
    private void OnMessageRequested();
}
```

Other visual components include:

- `EnhancedRadioDial`: Advanced tuning dial with visual feedback
- `RadioSignalDisplay`: Visual representation of detected signals
- `RadioSignalsDemo`: Demo scene for testing radio signals

## Integration with Gameplay

The radio system integrates with other gameplay systems:

### Narrative Integration

- Radio messages advance the story
- Discovering new frequencies unlocks new narrative elements
- Some story paths require interpreting specific signals

### Puzzle Integration

- Puzzles may require finding specific frequencies
- Some puzzles involve decoding Morse code messages
- Environmental puzzles may use radio signals as clues

### Exploration Integration

- Radio signals can guide players to new locations
- Signal strength increases as players approach sources
- Some areas may block or distort radio signals

## Running the Radio System

Several scripts have been created to run different aspects of the radio system:

### Main Game with Radio

```bash
./run_game_with_radio.sh
```

This script runs the main game scene with the radio interface visible.

### Radio Demo

```bash
./run_radio_demo.sh
```

This script runs the RadioSignalsDemo scene, which provides a comprehensive interface for testing radio signals.

### Radio Dial Test

```bash
./run_radio_dial.sh
```

This script runs the EnhancedRadioDial scene for testing the radio dial interface.

### Radio System Integration Test

```bash
./run_radio_test.sh
```

This script runs the RadioSystemIntegrationTest scene for testing the integration of radio components.

## Future Enhancements

Planned enhancements for the radio system include:

### Technical Improvements

- 8-bit/16-bit sound generation for authentic radio sounds
- Advanced signal visualization with spectrum analysis
- Memory optimization for better performance
- Cross-platform compatibility improvements

### Gameplay Enhancements

- More signal types (digital, encrypted, etc.)
- Radio direction finding (triangulation)
- Signal recording and playback
- Radio part upgrades and customization

### UI Enhancements

- More detailed frequency scanner
- Visual signal analyzer
- Morse code decoder tool
- Signal strength map overlay

## Known Issues

- Memory leaks may occur during extended radio usage
- Audio latency issues on some platforms
- UI scaling issues on different screen resolutions
- Signal detection may be inconsistent at edge frequencies

# Radio System

This document provides an overview of the radio system in Signal Lost, including its functionality, implementation, and how to use it in gameplay.

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

### Audio System

The audio system handles:
- Continuous white noise generation
- Signal fading based on strength
- Squelch effects when tuning
- Transitions between static and signals

### Visual Interface

The visual interface is implemented using:
- `PixelRadioInterface`: Basic radio controls
- `EnhancedRadioInterface`: Advanced visual features
- `StaticNoiseVisualizer`: Visual representation of static
- `MorseCodeVisualizer`: Visual representation of Morse code
- `FrequencyScannerVisualizer`: Frequency spectrum display

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

## Future Enhancements

Planned enhancements for the radio system include:

### Technical Improvements

- More realistic audio processing
- Advanced signal visualization
- Improved performance for mobile platforms

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

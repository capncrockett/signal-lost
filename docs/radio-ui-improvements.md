# Radio UI Improvements Guide

This document provides instructions for implementing UI improvements to the radio system in the Signal Lost game.

## Overview

The radio UI improvements include:

1. Better visual feedback for signal strength
2. Visual representation of static/noise
3. Morse code visualization
4. Improved tuning controls
5. More cohesive visual style

## Implementation Steps

### 1. Required Assets

Make sure the following assets are available:

- `radio.png` - Main radio background
- `signalStrengthIndicator.png` - Icon for signal strength
- `staticOverlay.png` - Texture for static visualization
- `radio_knob.svg` - Tuning knob image (in assets/images/radio directory)

### 2. Required Scripts

Ensure these scripts are properly set up:

- `RadioTuner.cs` - Updated to support new UI components
- `AudioVisualizer.cs` - For visualizing audio signals
- `MorseVisualizer.cs` - New script for visualizing Morse code patterns

### 3. Scene Structure in Godot Editor

Open the Godot editor and restructure the RadioTuner scene with the following hierarchy:

```
RadioTuner (Control)
├── RadioBackground (TextureRect)
│   - Texture: radio.png
├── RadioPanel (Panel)
│   ├── FrequencyDisplayPanel (Panel)
│   │   ├── FrequencyDisplay (Label)
│   ├── TuningSection (VBoxContainer)
│   │   ├── FrequencySlider (HSlider)
│   │   │   ├── TuningKnob (TextureRect)
│   │   │       - Texture: radio_knob.svg
│   │   ├── ButtonsRow (HBoxContainer)
│   │       ├── PowerButton (Button)
│   │       ├── ScanButton (Button)
│   │       ├── TuneDownButton (Button)
│   │       ├── TuneUpButton (Button)
│   ├── SignalSection (VBoxContainer)
│   │   ├── SignalLabel (Label)
│   │   ├── SignalStrengthContainer (HBoxContainer)
│   │       ├── SignalIcon (TextureRect)
│   │       │   - Texture: signalStrengthIndicator.png
│   │       ├── SignalStrengthMeter (ProgressBar)
│   ├── VisualizationContainer (VBoxContainer)
│   │   ├── VisualizerLabel (Label)
│   │   ├── StaticVisualization (ColorRect)
│   │       - Script: AudioVisualizer.cs
│   │       ├── StaticOverlay (TextureRect)
│   │           - Texture: staticOverlay.png
│   ├── MorseContainer (VBoxContainer)
│   │   ├── MorseLabel (Label)
│   │   ├── MorseVisualizer (ColorRect)
│   │       - Script: MorseVisualizer.cs
│   ├── MessageContainer (Panel)
│       ├── MessageTitle (Label)
│       ├── MessageButton (Button)
│       ├── MessageDisplay (Label)
```

### 4. Node Configuration

#### RadioTuner (Control)
- Apply the signal_lost_theme.tres theme

#### RadioBackground (TextureRect)
- Set texture to radio.png
- Position centered on screen
- Size: 600x400

#### RadioPanel (Panel)
- Position centered on screen
- Size: 500x350

#### FrequencyDisplayPanel (Panel)
- Position at top of RadioPanel
- Size: 300x50

#### FrequencyDisplay (Label)
- Font size: 24
- Text alignment: Center
- Text: "90.0 MHz"

#### FrequencySlider (HSlider)
- Value range: 0-100 (will be mapped to frequency range)
- Initial value: 10

#### TuningKnob (TextureRect)
- Texture: radio_knob.svg
- Size: 50x50
- Position: Centered on slider

#### ButtonsRow (HBoxContainer)
- Distribute buttons evenly

#### SignalStrengthMeter (ProgressBar)
- Hide percentage display
- Value range: 0-100

#### StaticVisualization (ColorRect)
- Apply AudioVisualizer.cs script
- Set properties:
  - NumBars: 32
  - BarWidth: 4.0
  - BarSpacing: 2.0
  - SignalColor: Green (0, 0.8, 0, 1)
  - StaticColor: Light Gray (0.8, 0.8, 0.8, 1)
  - BackgroundColor: Dark Gray (0.1, 0.1, 0.1, 1)

#### StaticOverlay (TextureRect)
- Texture: staticOverlay.png
- Stretch mode: Scale to fill

#### MorseVisualizer (ColorRect)
- Apply MorseVisualizer.cs script
- Set properties:
  - ActiveColor: Green (0, 0.8, 0, 1)
  - InactiveColor: Dark Gray (0.1, 0.1, 0.1, 1)

### 5. Testing

After setting up the scene, run the game to test the radio UI improvements:

```bash
# Windows
cd godot_project
./run_game.bat

# Mac/Linux
cd godot_project
./run_game.sh
```

## Features

### 1. Improved Radio Tuner UI
- Better visual layout with proper radio background
- Enhanced frequency display
- Rotating tuning knob that provides visual feedback

### 2. Visual Signal Strength Feedback
- Signal strength meter with icon
- Visual indication when tuning close to a signal

### 3. Static/Signal Visualization
- Audio visualizer that shows frequency bars
- Static overlay that changes opacity based on signal strength
- Visual noise effect that correlates with audio static

### 4. Morse Code Visualization
- Visual representation of Morse code patterns
- Shows dots and dashes as they are received
- History of recently received Morse code

### 5. Cohesive Visual Style
- Consistent color scheme (green and dark backgrounds)
- Better readability of text elements
- Proper spacing and alignment

## Troubleshooting

If you encounter issues:

1. Ensure all scripts are properly attached to the correct nodes
2. Check that all assets are in the correct directories
3. Verify that the node paths in RadioTuner.cs match your scene structure
4. Run with verbose logging to see any errors:
   ```
   ./run_game.bat --verbose
   ```

## Cross-Platform Considerations

- On Mac, ensure file paths use forward slashes (/)
- On Windows, backslashes (\) are used in file paths
- The C# scripts should work identically on both platforms

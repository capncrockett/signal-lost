# Pixel-Based UI System

This document provides an overview of the pixel-based UI system implemented in Signal Lost, including the radio interface and visualization tools.

## Table of Contents

1. [Overview](#overview)
2. [UI Visualization System](#ui-visualization-system)
3. [Pixel-Based Radio Interface](#pixel-based-radio-interface)
4. [Enhanced Radio Interface](#enhanced-radio-interface)
5. [Development Guidelines](#development-guidelines)
6. [Testing](#testing)

## Overview

Signal Lost uses a pixel-based UI approach instead of relying on image assets. This approach offers several benefits:

- **Reduced file size**: No need for multiple image assets
- **Better performance**: Drawing primitives are efficient
- **More authentic retro feel**: Pixel-perfect rendering
- **Easier modifications**: Code-based UI is more flexible
- **Consistent visual style**: Unified look across all UI elements

The pixel-based UI system consists of several components:
- UI Visualization System for debugging
- Pixel-Based Radio Interface
- Enhanced Radio Interface with visual features

## UI Visualization System

The UI Visualization System helps debug and improve pixel-based UIs without relying on screenshots. It includes:

### Text-Based ASCII Visualization

The `UIVisualizer` class provides a text-based ASCII representation of UI elements:

```csharp
UIVisualizer.VisualizeUI(control);
```

This outputs a grid-based visualization to the console, showing the layout of UI elements:

```
=== UI VISUALIZATION ===
Window Size: 1152x648

----------------------------------------------------------------------------------
|                                                                                |
|                                                                                |
|                                                                                |
|                          ####################                                  |
|                          #                  #                                  |
|                          #       TEXT       #                                  |
|                          #                  #                                  |
|                          #                  #                                  |
|                          #                  #                                  |
|                          #                  #                                  |
|                          #                  #                                  |
|                          #        B         #                                  |
|                          ####################                                  |
|                                                                                |
|                                                                                |
|                                                                                |
----------------------------------------------------------------------------------

=== END VISUALIZATION ===
```

### Detailed Logging

The system logs detailed information about UI elements:

```
UI State: Showing Message
Show Button: Position(476,324), Size(200,50), Hovered: False
Message Box: Position(115.2,129.6), Size(921.6,388.8)
Close Button: Position(916.8,458.4), Size(100,40), Hovered: False
```

### Screenshot Capability

The `ScreenshotTaker` class captures screenshots for debugging:

```csharp
// Automatically takes a screenshot after 3 seconds
// Saves to user://screenshot.png
```

## Pixel-Based Radio Interface

The Pixel-Based Radio Interface (`PixelRadioInterface`) provides a basic radio UI using drawing primitives:

### Features

- **Tuning Dial**: Interactive dial for frequency adjustment
- **Frequency Display**: Shows the current frequency
- **Signal Meter**: Displays signal strength
- **Power Button**: Toggles the radio on/off
- **Mute Button**: Toggles audio muting

### Implementation

The interface is implemented using Godot's drawing primitives:

```csharp
// Draw tuning dial
DrawCircle(center, radius, color);
DrawArc(center, radius, 0, Mathf.Pi * 2, 32, borderColor, 2);

// Draw frequency display
DrawRect(rect, backgroundColor);
DrawString(font, position, text, alignment, -1, fontSize, textColor);
```

## Enhanced Radio Interface

The Enhanced Radio Interface (`EnhancedRadioInterface`) extends the basic radio interface with advanced visual features:

### Static Noise Visualizer

The `StaticNoiseVisualizer` displays visual static that correlates with audio quality:

- Adjusts noise intensity based on signal strength
- Shows more static for weak signals
- Displays a signal wave for strong signals

### Morse Code Visualizer

The `MorseCodeVisualizer` provides a visual representation of Morse code signals:

- Displays dots and dashes in sequence
- Highlights the current position
- Supports customizable messages

```csharp
// Morse code mapping
{'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, ...
```

### Frequency Scanner Visualizer

The `FrequencyScannerVisualizer` shows a frequency spectrum with signal peaks:

- Displays a scanning line that moves across frequencies
- Shows signal peaks at specific frequencies
- Provides a grid with frequency markers

### Integration

The Enhanced Radio Interface integrates all visualizers into a cohesive retro-style interface:

- Improved tuning knob with visual feedback
- Signal strength meter with segments
- Power and scan buttons with visual states
- Frequency display with digital readout

## Development Guidelines

When extending or modifying the pixel-based UI system:

1. **Use the UI Visualization System** for debugging
2. **Maintain the retro aesthetic** with pixel-perfect rendering
3. **Keep performance in mind** by minimizing draw calls
4. **Ensure accessibility** with clear visual feedback
5. **Test on different screen sizes** to ensure proper scaling

### Adding New UI Elements

To add a new UI element:

1. Create a new class that extends `Control`
2. Implement the `_Draw()` method using drawing primitives
3. Use the UI Visualization System to debug the layout
4. Add interaction handling in `_GuiInput()`
5. Update the element in `_Process()` if needed

## Testing

Test scenes are provided for each component:

- `PixelUITest.tscn`: Tests the basic pixel UI system
- `SimpleTextTest.tscn`: Tests text rendering with built-in controls
- `PixelRadioTest.tscn`: Tests the basic radio interface
- `EnhancedRadioTest.tscn`: Tests the enhanced radio interface

To run a test scene:

```bash
godot --path . scenes/test/EnhancedRadioTest.tscn
```

Each test scene includes the `UIVisualizer` and `ScreenshotTaker` for debugging.

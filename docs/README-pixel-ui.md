# Signal Lost - Pixel-Based UI System

## Overview

Signal Lost has transitioned to a pixel-based UI system that offers better performance, a more authentic retro feel, and easier modifications. This document provides an overview of the pixel-based UI system and how to work with it.

## Key Components

### PixelFont

A custom font rendering system that draws characters pixel-by-pixel, providing a consistent retro look across all UI elements.

### UI Elements

- **PixelRadioInterface**: Interactive radio tuner with knobs and displays
- **PixelInventoryUI**: Inventory grid with item slots and interactions
- **PixelMapInterface**: Map display with locations and navigation
- **PixelMessageDisplay**: Message display with typewriter effects
- **PixelQuestUI**: Quest tracking and management interface

## Getting Started

### Prerequisites

- Godot 4.x with Mono support
- .NET SDK 6.0 or later
- Visual Studio or Visual Studio Code (recommended)

### Running the Project

1. Open the project in Godot
2. Open the PixelMainScene.tscn scene
3. Press F5 or click the Play button

### Running Tests

```bash
# From the godot_project directory
/path/to/godot --path . --headless --script tests/TestRunner.cs
```

## Development Guidelines

### Creating New UI Elements

1. Create a new C# script that inherits from Control
2. Implement the `_Draw()` method to render your UI
3. Add input handling in `_Input()` or `_GuiInput()`
4. Use the PixelFont class for text rendering

### Cross-Platform Considerations

- All drawing is done using platform-independent Godot primitives
- Input handling works with both mouse/keyboard and touch
- Path references should use forward slashes (/) for cross-platform compatibility

## Documentation

For more detailed information, see:

- [Pixel UI System](pixel-ui-system.md): Detailed overview of the pixel-based UI system
- [Extending Pixel UI](extending_pixel_ui.md): Guide to creating new pixel-based UI elements
- [Pixel-Based Approach](pixel-based-approach.md): Design philosophy and benefits

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

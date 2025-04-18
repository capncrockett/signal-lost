# Pixel-Based UI System

## Overview

Signal Lost uses a pixel-based UI approach instead of relying on image assets. This approach offers several benefits:

- **Reduced file size**: No need for multiple image assets
- **Better performance**: Drawing primitives are efficient
- **More authentic retro feel**: Pixel-perfect rendering
- **Easier modifications**: Code-based UI is more flexible
- **Consistent visual style**: Unified look across all UI elements

## Core Components

The pixel-based UI system consists of several key components:

### 1. PixelFont

A custom font rendering system that draws characters pixel-by-pixel:

```csharp
public partial class PixelFont : Resource
{
    // Character patterns stored as boolean arrays
    private Dictionary<char, bool[,]> _characterPatterns = new Dictionary<char, bool[,]>();
    
    // Get the pattern for a character
    public bool[,] GetCharacterPattern(char c)
    {
        // Return the pattern or a default for unknown characters
    }
}
```

### 2. Base UI Elements

All pixel-based UI elements share common functionality:

- Custom drawing using Godot's `_Draw()` method
- Input handling for interactivity
- Visibility toggling
- Consistent styling properties

### 3. Specific UI Implementations

- **PixelRadioInterface**: Interactive radio tuner with knobs and displays
- **PixelInventoryUI**: Inventory grid with item slots and interactions
- **PixelMapInterface**: Map display with locations and navigation
- **PixelMessageDisplay**: Message display with typewriter effects
- **PixelQuestUI**: Quest tracking and management interface

## Implementation Details

### Radio Interface

The PixelRadioInterface provides a radio UI using drawing primitives:

- **Tuning Dial**: Interactive dial for frequency adjustment
- **Frequency Display**: Shows the current frequency
- **Signal Meter**: Displays signal strength
- **Power Button**: Toggles the radio on/off

Implementation example:

```csharp
// Draw tuning dial
DrawCircle(center, radius, color);
DrawArc(center, radius, 0, Mathf.Pi * 2, 32, borderColor, 2);

// Draw frequency display
DrawRect(rect, backgroundColor);
DrawString(font, position, text, alignment, -1, fontSize, textColor);
```

### Message Display

The PixelMessageDisplay provides a typewriter-style text display:

- Renders text character by character
- Supports different text speeds
- Includes cursor blinking effects
- Handles multi-page messages

### Inventory UI

The PixelInventoryUI implements a grid-based inventory system:

- Displays items in a grid layout
- Supports item selection and interaction
- Shows item details on hover
- Handles item dragging and dropping

### Map Interface

The PixelMapInterface provides a navigable map:

- Displays a grid-based map
- Shows player position and points of interest
- Supports panning and zooming
- Highlights discovered locations

## Usage

To use a pixel-based UI element in your scene:

1. Add a Control node to your scene
2. Attach the appropriate script (e.g., PixelInventoryUI.cs)
3. Configure exported properties in the Inspector
4. Connect signals if needed

## Cross-Platform Considerations

The pixel-based UI system is designed to work consistently across platforms:

- All drawing is done using platform-independent Godot primitives
- Input handling works with both mouse/keyboard and touch
- Performance is optimized for all supported platforms

## Testing

Each UI component has corresponding test scenes and unit tests:

- Unit tests verify functionality and behavior
- Test scenes allow visual inspection and interaction testing

Run tests using:

```bash
cd godot_project
./run_tests.bat
```

## Development Guidelines

When extending or modifying the pixel-based UI system:

1. **Maintain the retro aesthetic** with pixel-perfect rendering
2. **Keep performance in mind** by minimizing draw calls
3. **Ensure accessibility** with clear visual feedback
4. **Test on different screen sizes** to ensure proper scaling

## Extending the System

To create a new pixel-based UI element:

1. Create a new C# script that inherits from Control
2. Implement the `_Draw()` method to render your UI
3. Add input handling in `_Input()` or `_GuiInput()`
4. Use the PixelFont class for text rendering
5. Add exported properties for customization

Example:

```csharp
public partial class MyPixelUI : Control
{
    [Export]
    public Color BackgroundColor { get; set; } = Colors.Black;
    
    [Export]
    public Color TextColor { get; set; } = Colors.White;
    
    private PixelFont _font;
    
    public override void _Ready()
    {
        _font = ResourceLoader.Load<PixelFont>("res://resources/pixel_font.tres");
    }
    
    public override void _Draw()
    {
        // Draw background
        DrawRect(new Rect2(Vector2.Zero, Size), BackgroundColor);
        
        // Draw text
        _font.DrawText(this, "Hello World", new Vector2(10, 10), TextColor);
    }
}
```

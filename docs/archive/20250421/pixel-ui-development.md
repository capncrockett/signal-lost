# Pixel-Based UI Development Guide

## Overview

This document provides guidelines for extending the pixel-based UI system in Signal Lost and outlines the development roadmap.

## Creating New UI Components

### Basic Structure

To create a new UI component:

1. Create a new C# class that extends `Control`
2. Implement the required methods
3. Add the component to a scene

```csharp
using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class MyPixelComponent : Control
    {
        // UI properties
        private Color _backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        private Color _textColor = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        
        // UI state
        private bool _isActive = false;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Initialization code
        }
        
        // Process function called every frame
        public override void _Process(double delta)
        {
            // Update code
            QueueRedraw();
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            // Draw background
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), _backgroundColor);
            
            // Draw other elements
            if (_isActive)
            {
                DrawCircle(new Vector2(Size.X / 2, Size.Y / 2), 20, _textColor);
            }
        }
        
        // Handle input events
        public override void _GuiInput(InputEvent @event)
        {
            // Handle mouse button events
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
                {
                    _isActive = !_isActive;
                    QueueRedraw();
                }
            }
        }
    }
}
```

### Required Methods

At minimum, implement these methods:

- `_Ready()`: Initialize the component
- `_Draw()`: Draw the component using primitives
- `_GuiInput()`: Handle user input

For dynamic components, also implement:

- `_Process()`: Update the component state
- `_Notification()`: Handle resize and other notifications

## Best Practices

### Performance Optimization

- Minimize calls to `QueueRedraw()`
- Use `_Draw()` efficiently
- Only redraw when necessary
- Cache calculations where possible

```csharp
// Bad: Redrawing every frame
public override void _Process(double delta)
{
    QueueRedraw();
}

// Good: Redraw only when needed
public override void _Process(double delta)
{
    if (_needsRedraw)
    {
        QueueRedraw();
        _needsRedraw = false;
    }
}
```

### Visual Consistency

- Use a consistent color palette
- Maintain uniform spacing and alignment
- Follow the same interaction patterns
- Use similar visual feedback for similar actions

### Accessibility

- Ensure sufficient contrast
- Provide multiple feedback methods (visual + audio)
- Make interactive elements clearly distinguishable
- Test with different color settings

## Common UI Patterns

### Button Implementation

```csharp
private void DrawButton(Rect2 rect, string text, bool isHovered, bool isPressed)
{
    // Determine button color based on state
    Color bgColor;
    if (isPressed)
    {
        bgColor = new Color(0.0f, 0.5f, 0.0f, 1.0f);
    }
    else if (isHovered)
    {
        bgColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    }
    else
    {
        bgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
    }
    
    // Draw button background
    DrawRect(rect, bgColor);
    
    // Draw button border
    DrawRect(rect, _borderColor, false, 1);
    
    // Draw button text
    DrawString(ThemeDB.FallbackFont, 
        new Vector2(rect.Position.X + rect.Size.X / 2, rect.Position.Y + rect.Size.Y / 2 + 6),
        text, 
        HorizontalAlignment.Center, -1, 16, _textColor);
}
```

### Slider Implementation

```csharp
private void DrawSlider(Rect2 rect, float value, bool isHovered)
{
    // Draw slider track
    DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1.0f));
    
    // Draw slider border
    DrawRect(rect, _borderColor, false, 1);
    
    // Calculate handle position
    float handleWidth = 10;
    float handleHeight = rect.Size.Y;
    float handleX = rect.Position.X + (rect.Size.X - handleWidth) * value;
    
    // Draw slider handle
    Color handleColor = isHovered ? _highlightColor : _handleColor;
    DrawRect(new Rect2(handleX, rect.Position.Y, handleWidth, handleHeight), handleColor);
}
```

## Development Roadmap

### Completed Features

- ✅ Basic pixel-based UI framework
- ✅ Pixel-based radio interface
- ✅ Pixel-based inventory UI
- ✅ Pixel-based message display
- ✅ Pixel-based map interface

### Current Development

- 🔄 Field exploration system
- 🔄 Game progression mechanics
- 🔄 Save/load system

### Upcoming Features

- ⬜ Advanced radio features
  - ⬜ Signal analysis tools
  - ⬜ Frequency scanner visualization
  - ⬜ Morse code visualization
- ⬜ World integration
  - ⬜ Link radio signals to world locations
  - ⬜ Implement signal strength based on proximity
  - ⬜ Add environmental effects on radio reception
- ⬜ Visual polish
  - ⬜ Refine all UI animations
  - ⬜ Improve color schemes and contrast
  - ⬜ Add subtle visual effects (glow, scan lines)

## Testing

Each UI component should have corresponding test scenes and unit tests:

- Unit tests verify functionality and behavior
- Test scenes allow visual inspection and interaction testing

Run tests using:

```bash
cd godot_project
./run_tests.bat
```

## Example Components

### Toggle Switch

```csharp
public partial class PixelToggleSwitch : Control
{
    [Export]
    public bool IsOn { get; set; } = false;
    
    [Export]
    public Color OnColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
    
    [Export]
    public Color OffColor { get; set; } = new Color(0.5f, 0.0f, 0.0f, 1.0f);
    
    [Export]
    public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);
    
    private bool _isHovered = false;
    
    public override void _Ready()
    {
        MinSize = new Vector2(60, 30);
    }
    
    public override void _Draw()
    {
        // Draw background
        DrawRect(new Rect2(0, 0, Size.X, Size.Y), BackgroundColor);
        
        // Draw border
        DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color(0.3f, 0.3f, 0.3f, 1.0f), false, 1);
        
        // Draw switch track
        float trackWidth = Size.X - 10;
        float trackHeight = Size.Y / 2;
        float trackX = 5;
        float trackY = (Size.Y - trackHeight) / 2;
        
        DrawRect(new Rect2(trackX, trackY, trackWidth, trackHeight), 
            new Color(0.2f, 0.2f, 0.2f, 1.0f));
        
        // Draw switch handle
        float handleWidth = trackWidth / 2;
        float handleHeight = Size.Y - 6;
        float handleX = IsOn ? trackX + trackWidth - handleWidth : trackX;
        float handleY = 3;
        
        Color handleColor = IsOn ? OnColor : OffColor;
        if (_isHovered)
        {
            handleColor = new Color(
                handleColor.R * 1.2f,
                handleColor.G * 1.2f,
                handleColor.B * 1.2f,
                handleColor.A
            );
        }
        
        DrawRect(new Rect2(handleX, handleY, handleWidth, handleHeight), handleColor);
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
            {
                IsOn = !IsOn;
                EmitSignal(SignalName.Toggled, IsOn);
                QueueRedraw();
            }
        }
        else if (@event is InputEventMouseMotion)
        {
            bool wasHovered = _isHovered;
            _isHovered = true;
            
            if (wasHovered != _isHovered)
            {
                QueueRedraw();
            }
        }
    }
    
    public override void _Notification(int what)
    {
        if (what == NotificationMouseExit)
        {
            bool wasHovered = _isHovered;
            _isHovered = false;
            
            if (wasHovered != _isHovered)
            {
                QueueRedraw();
            }
        }
    }
    
    [Signal]
    public delegate void ToggledEventHandler(bool isOn);
}
```

### Progress Bar

```csharp
public partial class PixelProgressBar : Control
{
    [Export]
    public float Value { get; set; } = 0.5f;
    
    [Export]
    public Color FillColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
    
    [Export]
    public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);
    
    [Export]
    public bool ShowSegments { get; set; } = true;
    
    [Export]
    public int SegmentCount { get; set; } = 10;
    
    public override void _Ready()
    {
        MinSize = new Vector2(100, 20);
    }
    
    public override void _Draw()
    {
        // Draw background
        DrawRect(new Rect2(0, 0, Size.X, Size.Y), BackgroundColor);
        
        // Draw border
        DrawRect(new Rect2(0, 0, Size.X, Size.Y), new Color(0.3f, 0.3f, 0.3f, 1.0f), false, 1);
        
        // Draw fill
        float fillWidth = Size.X * Mathf.Clamp(Value, 0.0f, 1.0f);
        DrawRect(new Rect2(0, 0, fillWidth, Size.Y), FillColor);
        
        // Draw segments
        if (ShowSegments && SegmentCount > 1)
        {
            float segmentWidth = Size.X / SegmentCount;
            
            for (int i = 1; i < SegmentCount; i++)
            {
                float x = i * segmentWidth;
                DrawLine(
                    new Vector2(x, 0),
                    new Vector2(x, Size.Y),
                    new Color(0.0f, 0.0f, 0.0f, 0.3f),
                    1
                );
            }
        }
    }
    
    public void SetValue(float value)
    {
        Value = Mathf.Clamp(value, 0.0f, 1.0f);
        QueueRedraw();
    }
}
```

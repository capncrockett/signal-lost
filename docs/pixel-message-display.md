# Pixel-Based Message Display

## Overview

The Pixel-Based Message Display is a component for displaying text messages with a retro, pixel-art aesthetic. It features a typewriter effect, visual noise, and various display styles to simulate different types of interfaces (terminal, radio, note, computer).

## Features

- **Pixel Font Rendering**: Text is rendered using a custom pixel font for a retro aesthetic
- **Typewriter Effect**: Text appears character by character with variable speed
- **Visual Effects**:
  - Scanlines for CRT monitor simulation
  - Screen flicker for added atmosphere
  - Interference/noise effects for damaged displays
- **Multiple Display Types**:
  - Terminal (green text on black background)
  - Radio (yellow text with heavy interference)
  - Note (black text on paper background)
  - Computer (blue text on dark blue background)
- **ASCII Art Support**: Display ASCII art alongside text messages
- **Interactive Elements**:
  - Close button
  - Decode button for encrypted messages
- **Sound Effects**:
  - Typewriter sounds for text display
  - Interface sounds for button clicks
- **Keyboard Controls**:
  - Escape to close
  - Space/Enter to skip typing or decode

## Usage

### Basic Usage

```csharp
// Get reference to the message display
var messageDisplay = GetNode<PixelMessageDisplay>("MessageDisplay");

// Set and show a message
messageDisplay.SetMessage(
    "message_id",      // Unique ID for the message
    "Message Title",   // Title to display
    "Message content with pixel-font rendering and typewriter effect.",
    false,             // Is the message already decoded?
    0.2f               // Interference level (0.0 to 1.0)
);
```

### Display Types

```csharp
// Terminal style
messageDisplay.MessageType = "Terminal";
messageDisplay.TextColor = new Color(0.0f, 0.8f, 0.0f, 1.0f); // Green
messageDisplay.BackgroundColor = new Color(0.05f, 0.05f, 0.05f, 1.0f); // Dark gray
messageDisplay.EnableScanlines = true;
messageDisplay.EnableScreenFlicker = true;

// Radio style
messageDisplay.MessageType = "Radio";
messageDisplay.TextColor = new Color(0.9f, 0.9f, 0.2f, 1.0f); // Yellow
messageDisplay.BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f); // Dark gray
messageDisplay.EnableScanlines = false;
messageDisplay.EnableScreenFlicker = true;

// Note style
messageDisplay.MessageType = "Note";
messageDisplay.TextColor = new Color(0.1f, 0.1f, 0.1f, 1.0f); // Black
messageDisplay.BackgroundColor = new Color(0.95f, 0.95f, 0.9f, 1.0f); // Off-white
messageDisplay.EnableScanlines = false;
messageDisplay.EnableScreenFlicker = false;

// Computer style
messageDisplay.MessageType = "Computer";
messageDisplay.TextColor = new Color(0.0f, 0.6f, 0.9f, 1.0f); // Blue
messageDisplay.BackgroundColor = new Color(0.0f, 0.0f, 0.2f, 1.0f); // Dark blue
messageDisplay.EnableScanlines = true;
messageDisplay.EnableScreenFlicker = true;
```

### ASCII Art

```csharp
// Create ASCII art
List<string> asciiArt = new List<string>
{
    "    _____  _                   _   _                 _   ",
    "   / ____|(_)                 | | | |               | |  ",
    "  | (___   _   __ _  _ __   __ _ | | | |     ___   ___| |_ ",
    "   \\___ \\ | | / _` || '_ \\ / _` || | | |    / _ \\ / __| __|",
    "   ____) || || (_| || | | || (_| || | | |___| (_) |\\__ \\ |_ ",
    "  |_____/ |_| \\__, ||_| |_| \\__,_||_| |______\\___/ |___/\\__|",
    "               __/ |                                      ",
    "              |___/                                       "
};

// Display message with ASCII art
messageDisplay.SetMessageWithArt(
    "ascii_art_message",
    "SYSTEM IDENTIFICATION",
    "Welcome to the Signal Lost terminal system.",
    asciiArt,
    false,
    0.1f
);
```

### Signals

Connect to the message display signals to handle user interactions:

```csharp
// Connect signals
messageDisplay.MessageClosed += OnMessageClosed;
messageDisplay.DecodeRequested += OnDecodeRequested;

// Handle message closed event
private void OnMessageClosed()
{
    GD.Print("Message closed");
}

// Handle decode requested event
private void OnDecodeRequested(string messageId)
{
    GD.Print($"Decode requested for message: {messageId}");
    
    // Show decoded version of the message
    messageDisplay.SetMessage(
        messageId,
        "DECODED MESSAGE",
        "This is the decoded version of the message.",
        true,
        0.0f  // No interference in decoded message
    );
}
```

## Customization

The PixelMessageDisplay component has many customizable properties:

```csharp
// Colors
messageDisplay.BackgroundColor = new Color(0.05f, 0.05f, 0.05f, 1.0f);
messageDisplay.BorderColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
messageDisplay.TextColor = new Color(0.0f, 0.8f, 0.0f, 1.0f);
messageDisplay.TitleColor = new Color(0.0f, 0.9f, 0.0f, 1.0f);
messageDisplay.ButtonColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
messageDisplay.ButtonHighlightColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
messageDisplay.ButtonTextColor = new Color(0.0f, 0.8f, 0.0f, 1.0f);
messageDisplay.ScanlineColor = new Color(0.0f, 0.0f, 0.0f, 0.2f);

// Effects
messageDisplay.NoiseIntensity = 0.2f;
messageDisplay.TypewriterSpeed = 0.05f;
messageDisplay.TypewriterSpeedVariation = 0.02f;
messageDisplay.CharacterSize = 8;
messageDisplay.LineSpacing = 2;
messageDisplay.EnableScanlines = true;
messageDisplay.EnableScreenFlicker = true;
messageDisplay.EnableTypewriterSound = true;
messageDisplay.ShowTimestamp = true;
```

## Integration with Game Systems

The PixelMessageDisplay component is designed to integrate with other game systems:

- **Radio System**: Display decoded radio messages
- **Computer Terminals**: Show computer interfaces and data
- **Notes and Documents**: Display found items and clues
- **Story Progression**: Reveal narrative elements through messages

## Test Scene

A test scene is provided to demonstrate the different message display types:

- Location: `godot_project/Scenes/test/EnhancedMessageDisplayTest.tscn`
- Script: `godot_project/scripts/EnhancedMessageDisplayTest.cs`

Run this scene to see examples of different message types and styles.

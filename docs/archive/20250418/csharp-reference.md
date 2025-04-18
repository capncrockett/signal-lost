# C# in Godot Reference Guide

## Overview

Signal Lost is implemented in C# using Godot Engine. This document provides a quick reference for C# usage in the project.

## Project Structure

- `godot_project/scripts/`: C# scripts for game logic
- `godot_project/scenes/`: Godot scene files (.tscn)
- `godot_project/tests/`: C# test files
- `godot_project/resources/`: Game resources and assets

## Key Components

- **GameState**: Manages game state (frequency, signals, etc.)
- **AudioManager**: Handles audio playback and processing
- **RadioTuner**: Main UI component for tuning
- **AudioVisualizer**: Visualizes audio signals
- **ScreenshotTaker**: Captures screenshots for debugging and documentation

## C# Essentials

### Classes

```csharp
// All Godot-derived classes must be partial and have the GlobalClass attribute
namespace SignalLost
{
    [GlobalClass]
    public partial class MyComponent : Node
    {
        // Properties, methods, etc.
    }
}
```

### Properties

```csharp
// Export properties for inspector
[Export]
public float MyProperty { get; set; } = 1.0f;

// Private fields
private float _myField = 0.0f;
```

### Signals

```csharp
// Define signal
[Signal]
public delegate void MySignalEventHandler(string parameter);

// Emit signal
EmitSignal(SignalName.MySignal, "parameter");

// Connect to signal
otherNode.MySignal += OnMySignal;

// Signal handler
private void OnMySignal(string parameter)
{
    // Handle signal
}
```

### Node References

```csharp
// Get node reference
private Label _myLabel;

public override void _Ready()
{
    _myLabel = GetNode<Label>("Path/To/Label");
}
```

## Building and Testing

### Building

```bash
# Command line build
dotnet build godot_project/SignalLost.csproj

# Or use Godot Editor: Build > Build Solution
```

### Running Tests

```bash
# Run all tests
cd godot_project
./run_tests.sh

# Run specific test scene
/Applications/Godot_mono.app/Contents/MacOS/Godot --headless --path . tests/MacTestScene.tscn
```

## Common Patterns

### Singleton Access

```csharp
// Get singleton reference
private GameState _gameState;

public override void _Ready()
{
    _gameState = GetNode<GameState>("/root/GameState");
}
```

### Resource Loading

```csharp
// Load a resource
var resource = ResourceLoader.Load<Resource>("res://path/to/resource.tres");
```

### Scene Instantiation

```csharp
// Load and instantiate a scene
var packedScene = ResourceLoader.Load<PackedScene>("res://path/to/scene.tscn");
var instance = packedScene.Instantiate<Node>();
AddChild(instance);
```

## Testing

### Test Class Structure

```csharp
using Godot;
using System;
using GUT;

namespace SignalLost.Tests
{
    [TestClass]
    public class MyComponentTests : Test
    {
        private MyComponent _component;

        public override void Before()
        {
            _component = new MyComponent();
            AddChild(_component);
        }

        public override void After()
        {
            _component.QueueFree();
            _component = null;
        }

        [Test]
        public void TestSomething()
        {
            // Arrange
            _component.SomeProperty = 42;

            // Act
            _component.DoSomething();

            // Assert
            AssertEqual(_component.Result, 84, "Result should be doubled");
        }
    }
}
```

### Platform-Specific Tests

```csharp
[Test]
public void TestPlatformSpecificFeature()
{
    if (OS.GetName() == "macOS")
    {
        GD.Print("Skipping test on Mac");
        Pass("Test skipped on Mac platform");
        return;
    }
    
    // Windows-specific test code here
}
```

## Best Practices

1. **Use C# Naming Conventions**:
   - PascalCase for public members and types
   - _camelCase for private fields
   - Use meaningful names

2. **Organize Code**:
   - Keep classes focused and small
   - Group related functionality
   - Use regions sparingly

3. **Error Handling**:
   - Use null checks
   - Handle exceptions appropriately
   - Log errors with GD.PrintErr()

4. **Documentation**:
   - Add XML comments to public methods
   - Document complex logic
   - Keep comments up-to-date

5. **Testing**:
   - Write tests for all components
   - Test edge cases
   - Use mocks when appropriate

## Debugging

- Use `GD.Print()` for logging
- Use Visual Studio or VS Code for debugging
- Use the Godot debugger for runtime inspection
- Add breakpoints in your IDE

## Resources

- [Godot C# Documentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Godot API Reference](https://docs.godotengine.org/en/stable/classes/index.html)

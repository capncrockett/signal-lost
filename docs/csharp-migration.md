# C# Migration Guide

## Overview

Signal Lost has been migrated from a dual GDScript/C# implementation to a pure C# implementation.

## Why C#?

- **Better IDE Support**: Full Visual Studio/VS Code integration
- **Performance**: Faster for complex operations
- **Type Safety**: Strong typing catches errors at compile time
- **Ecosystem**: Access to the entire .NET ecosystem
- **CLI Integration**: Better command-line tooling

## Project Structure

- `godot_project/scripts/`: C# scripts for game logic
- `godot_project/scenes/`: Godot scene files (.tscn)
- `godot_project/tests/`: C# test files

## Key Components

- **GameState**: Manages game state (frequency, signals, etc.)
- **AudioManager**: Handles audio playback
- **RadioTuner**: Main UI component for tuning
- **AudioVisualizer**: Visualizes audio signals

## C# Essentials

### Classes

```csharp
// All Godot-derived classes must be partial
public partial class MyClass : Node
{
    // ...
}
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
```

### Building

```bash
# Command line build
godot --build-solutions --path godot_project

# Or use Godot Editor: Build > Build Solution
```

### Testing

```bash
# Run all tests
cd godot_project
./run_csharp_tests.bat  # Windows
./run_csharp_tests.sh   # Linux/Mac
```

## Migration Summary

### Migrated Files
- `AudioVisualizer.gd` → `AudioVisualizer.cs`
- `FixedGameState.gd` → `GameState.cs` (merged)

### Removed Files
- `AudioManagerWrapper.gd`
- `GameStateWrapper.gd`
- All GDScript test files

### Next Steps
1. Update scene files to use C# scripts
2. Add more C# tests
3. Fix any remaining issues

### Development Guidelines
- All new code must be in C#
- Use `partial` keyword for Godot-derived classes
- Write tests for all new components
- See `workflow.md` for detailed development process

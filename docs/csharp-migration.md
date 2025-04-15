# C# Migration Guide

## Overview

Signal Lost has been migrated from a dual GDScript/C# implementation to a pure C# implementation. This document explains the migration process and how to work with the C# codebase.

## Why C#?

C# offers several advantages over GDScript for this project:

1. **Better IDE Support**: Full Visual Studio/VS Code integration with IntelliSense
2. **Performance**: C# can be faster for complex operations
3. **Type Safety**: Strong typing helps catch errors at compile time
4. **Ecosystem**: Access to the entire .NET ecosystem
5. **CLI Integration**: Better command-line tooling and build processes
6. **Reusability**: Code can be shared with other .NET projects

## Project Structure

The project is now organized as follows:

- `godot_project/scripts/`: Contains all C# scripts for the game logic
- `godot_project/scenes/`: Contains Godot scene files (.tscn)
- `godot_project/tests/`: Contains C# test files

## Key Components

### Core Systems

- **GameState**: Manages the game state, including radio frequency, discovered signals, etc.
- **AudioManager**: Handles audio playback, including static noise and signal tones
- **RadioTuner**: The main UI component for tuning the radio

### UI Components

- **AudioVisualizer**: Visualizes audio signals and static

## Working with C# in Godot

### Important Notes

1. All C# classes that derive from Godot classes must include the `partial` keyword:

```csharp
public partial class MyClass : Node
{
    // ...
}
```

2. Signals in C# are defined using delegates:

```csharp
[Signal]
public delegate void MySignalEventHandler(string parameter);
```

3. To emit signals in C#:

```csharp
EmitSignal(SignalName.MySignal, "parameter");
```

4. To connect to signals in C#:

```csharp
otherNode.MySignal += OnMySignal;
```

### Building and Running

To build and run the C# project:

1. Open the project in Godot
2. Click on "Build" or press F5
3. Alternatively, use the command line:

```bash
godot --build-solutions --path /path/to/project
```

## Testing

Tests are written in C# using the MSTest framework. To run tests:

1. Use the `run_tests.bat` script (Windows) or `run_tests.sh` script (Linux/Mac)
2. Tests can also be run from the Godot editor using the GUT plugin

## Migrated Files

The following files have been migrated from GDScript to C#:

- `AudioVisualizer.gd` → `AudioVisualizer.cs`
- `FixedGameState.gd` → (functionality merged into) `GameState.cs`

## Removed Files

The following files have been removed as they are no longer needed:

- `AudioManagerWrapper.gd`
- `GameStateWrapper.gd`
- All GDScript test files

## Future Development

All new development should be done in C#. If you need to add a new component:

1. Create a new C# script in the `godot_project/scripts/` directory
2. Make sure to include the `partial` keyword for classes that derive from Godot classes
3. Add appropriate tests in the `godot_project/tests/` directory

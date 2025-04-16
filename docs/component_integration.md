# Signal Lost Component Integration Guide

This document explains how the various components in the Signal Lost game integrate with each other.

## Architecture Overview

Signal Lost uses a singleton-based architecture for global state management. The main singletons are:

- **GameState**: Manages the core game state including radio state, frequencies, signals, and messages
- **AudioManager**: Handles all audio playback including static noise, signals, and effects
- **MapSystem**: Manages the game map, locations, and navigation
- **InventorySystem**: Handles the player's inventory
- **QuestSystem**: Manages quests and objectives

These singletons are automatically loaded when the game starts (defined in the `project.godot` file under `[autoload]`).

## Component Interactions

### Radio Tuner

The RadioTuner component is the main interface for the player to interact with radio signals. It:

1. Gets and sets the current frequency through the GameState singleton
2. Toggles the radio power state through GameState
3. Requests audio playback through the AudioManager singleton
4. Updates its visual display based on the current state

### Map UI

The MapUI component displays the game world map and allows navigation. It:

1. Gets the current location from the MapSystem singleton
2. Shows connected locations that can be traveled to
3. Allows the player to change location through the MapSystem
4. Updates its display based on discovered locations

### Inventory UI

The InventoryUI component displays and manages the player's inventory. It:

1. Gets the inventory items from the InventorySystem singleton
2. Allows the player to use, combine, or examine items
3. Updates when items are added or removed

### Quest UI

The QuestUI component displays active and completed quests. It:

1. Gets quest information from the QuestSystem singleton
2. Shows quest objectives and progress
3. Updates when quest status changes

## Data Flow Example

When a player tunes the radio to a specific frequency:

1. The RadioTuner UI updates the frequency display
2. The RadioTuner calls `GameState.SetFrequency(newFrequency)`
3. GameState checks if there's a signal at that frequency
4. If a signal is found, GameState calculates the signal strength
5. GameState notifies AudioManager to play the appropriate audio
6. The AudioVisualizer component reads the signal strength and updates its display
7. If the signal contains a message, it can be decoded and displayed

## Testing Component Integration

To test how components interact with each other:

1. Run the `ComponentIntegrationDemo.tscn` scene
2. This scene shows the RadioTuner alongside other UI components
3. The state panel displays real-time information from the GameState
4. Interact with the RadioTuner and observe how other components react

## Analyzing Scene References

To understand the relationships between scenes:

1. Run the `SceneAnalyzerRunner.tscn` scene in the tools folder
2. This will generate a report at `res://tools/scene_analysis.md`
3. The report includes:
   - Detection of duplicate UIDs
   - Scene hierarchy visualization
   - Detailed scene references

## Common Integration Issues

### Duplicate UIDs

Scene files in Godot have unique identifiers (UIDs). Duplicate UIDs can cause:
- Scenes loading incorrectly
- References pointing to the wrong scene
- Warnings in the editor

If you see warnings about duplicate UIDs, run the Scene Analyzer tool to identify and fix them.

### Signal Connection Issues

When connecting signals between components:
- Ensure the signal exists on the source object
- Verify the method signature matches the expected parameters
- Check that the target object is properly referenced

### Singleton Access

When accessing singletons:
- Use `GetNode<T>("/root/SingletonName")` to get a reference
- Check for null before using the reference
- Consider caching the reference for performance

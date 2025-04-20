# Signal Lost: Codebase Documentation

This document provides an overview of the Signal Lost codebase, including its architecture, key components, and how they interact. It's intended for developers who need to understand, maintain, or extend the game.

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Core Systems](#core-systems)
3. [Game Mechanics](#game-mechanics)
4. [User Interface](#user-interface)
5. [Audio System](#audio-system)
6. [Testing Framework](#testing-framework)
7. [Save/Load System](#saveload-system)
8. [Build and Deployment](#build-and-deployment)
9. [Best Practices](#best-practices)
10. [Common Issues and Solutions](#common-issues-and-solutions)

## Architecture Overview

Signal Lost is built using the Godot Engine (version 4.x) with C# as the scripting language. The game follows a component-based architecture with the following key design principles:

### Singleton Pattern

Core game systems are implemented as singletons (autoloaded nodes) to provide global access to their functionality. These include:

- `GameState`: Manages the overall game state
- `AudioManager`: Handles all audio playback
- `QuestSystem`: Manages quests and objectives
- `InventorySystem`: Handles inventory and items
- `MapSystem`: Manages locations and navigation
- `GameProgressionManager`: Controls game progression
- `SaveManager`: Handles saving and loading game data
- `MessageManager`: Manages in-game messages

### Scene-Based Components

Game UI and interactive elements are implemented as scenes that can be instantiated and added to the scene tree. These include:

- `RadioTuner`: The interactive radio interface
- `InventoryUI`: The inventory display and interaction
- `MapInterface`: The map display and navigation
- `MessageDisplay`: Displays decoded messages
- `PixelMapInterface`: Pixel-based map visualization

### Signal-Based Communication

Components communicate using Godot's built-in signal system, which provides a decoupled way for objects to interact. For example:

- `GameState` emits signals when the radio is toggled or frequency changes
- `QuestSystem` emits signals when quests are discovered or completed
- `InventorySystem` emits signals when items are added or used

## Core Systems

### GameState

The `GameState` class is the central repository for game state information. It manages:

- Radio state (on/off, frequency, discovered frequencies)
- Player state (health, hunger, thirst)
- Game progress
- Messages and signals
- Current location

Key methods:
- `SetFrequency(float frequency)`: Sets the radio frequency
- `ToggleRadio()`: Toggles the radio on/off
- `FindSignalAtFrequency(float frequency)`: Finds a signal at the given frequency
- `DecodeMessage(string messageId)`: Decodes a message
- `SetCurrentLocation(string locationId)`: Sets the current location

### QuestSystem

The `QuestSystem` class manages quests and objectives. It handles:

- Quest discovery and activation
- Quest objectives and completion
- Quest prerequisites
- Quest rewards

Key methods:
- `DiscoverQuest(string questId)`: Discovers a quest
- `ActivateQuest(string questId)`: Activates a quest
- `UpdateQuestObjective(string questId, string objectiveId, int progress)`: Updates quest objective progress
- `IsQuestCompleted(string questId)`: Checks if a quest is completed

### InventorySystem

The `InventorySystem` class manages the player's inventory. It handles:

- Item acquisition and removal
- Item usage
- Item combining
- Item database

Key methods:
- `AddItemToInventory(string itemId)`: Adds an item to the inventory
- `RemoveItemFromInventory(string itemId)`: Removes an item from the inventory
- `UseItem(string itemId)`: Uses an item
- `CombineItems(string itemId1, string itemId2)`: Combines two items
- `HasItem(string itemId)`: Checks if the player has an item

### MapSystem

The `MapSystem` class manages locations and navigation. It handles:

- Location discovery
- Location navigation
- Location connections
- Location search

Key methods:
- `DiscoverLocation(string locationId)`: Discovers a location
- `ChangeLocation(string locationId)`: Changes the current location
- `IsLocationDiscovered(string locationId)`: Checks if a location is discovered
- `IsLocationConnected(string locationId)`: Checks if a location is connected to the current location
- `SearchCurrentLocation()`: Searches the current location for items

### GameProgressionManager

The `GameProgressionManager` class controls game progression. It handles:

- Progression stages
- Progression requirements
- Progression events
- Stage-specific content

Key methods:
- `CheckProgressionRequirements()`: Checks if requirements for advancing are met
- `AdvanceProgression()`: Advances to the next progression stage
- `SetProgression(ProgressionStage stage)`: Sets the progression to a specific stage
- `GetCurrentStageDescription()`: Gets a description of the current stage
- `GetNextObjective()`: Gets the next objective based on the current stage

## Game Mechanics

### Radio Mechanics

The radio is a central gameplay mechanic. It allows the player to:

- Tune to different frequencies
- Detect signals
- Decode messages
- Discover new locations

The `RadioTuner` class implements the interactive radio interface, while the `GameState` class manages the underlying radio state.

### Survival Mechanics

The game includes survival mechanics such as:

- Health: Decreases when the player takes damage
- Hunger: Decreases over time, affects health when very low
- Thirst: Decreases over time, affects health when very low
- Day/night cycle: Affects survival stat depletion rates

These mechanics are managed by the `GameState` class and are updated in the `_Process` method.

### Exploration Mechanics

Exploration is a key gameplay element. It allows the player to:

- Discover new locations
- Find items
- Complete quests
- Advance the story

The `MapSystem` class manages exploration mechanics, while the `PixelMapInterface` class provides the visual representation.

### Quest Mechanics

Quests drive the game's narrative and progression. They include:

- Main quests: Drive the main story
- Side quests: Provide additional content
- Location-based quests: Discovered by visiting locations
- Item-based quests: Require specific items

The `QuestSystem` class manages quest mechanics, while the `GameProgressionManager` class ties quests to progression.

## User Interface

### Pixel-Based UI

The game uses a pixel-based UI approach for a retro aesthetic. This includes:

- Programmatically drawn UI elements
- Pixel-perfect rendering
- Limited color palette
- Grid-based layouts

The pixel-based UI is implemented using Godot's drawing primitives and custom shaders.

### Radio Interface

The `RadioTuner` class implements the interactive radio interface. It includes:

- Frequency display
- Tuning knob
- Power button
- Signal strength meter
- Static visualization
- Message display

### Inventory Interface

The `InventoryUI` class implements the inventory interface. It includes:

- Item grid
- Item details
- Item usage
- Item combining

### Map Interface

The `PixelMapInterface` class implements the map interface. It includes:

- Location display
- Navigation controls
- Location details
- Search controls

### Message Display

The `MessageDisplay` class implements the message display. It includes:

- Message title
- Message content
- Decoded/encoded status
- Navigation controls

## Audio System

The `AudioManager` class handles all audio playback. It includes:

### Static Noise Generation

The `AudioManager` can generate different types of noise:

- White noise: Equal energy per frequency
- Pink noise: 1/f spectrum (more natural sounding)
- Brown noise: 1/f² spectrum (deeper rumble)
- Digital noise: Harsh, bit-crushed sound

These are generated procedurally using the `GenerateWhiteNoise()`, `GeneratePinkNoise()`, `GenerateBrownNoise()`, and `GenerateDigitalNoise()` methods.

### Signal Tone Generation

The `AudioManager` can generate different waveforms for signal tones:

- Sine wave: Smooth, pure tone
- Square wave: Harsh, buzzy tone
- Triangle wave: Softer than square, but still distinct
- Sawtooth wave: Sharp, bright tone

These are generated procedurally in the `FillSignalBuffer()` method.

### Audio Effects

The `AudioManager` applies various audio effects to create a realistic radio sound:

- EQ: Shapes the frequency response
- Distortion: Adds grit and character
- Bandpass filter: Creates a radio-like sound
- Reverb: Adds space and depth

These effects are set up in the `SetupAudioEffects()` method.

### Volume Control

The `AudioManager` provides volume control for different audio sources:

- Master volume: Controls overall volume
- Static volume: Controls static noise volume
- Signal volume: Controls signal tone volume

These are controlled using the `SetVolume()` method and Godot's audio bus system.

## Testing Framework

Signal Lost uses a custom testing framework built on top of the Microsoft.VisualStudio.TestTools.UnitTesting framework. It includes:

### Base Test Classes

- `TestBase`: Base class for all tests
- `UnitTestBase`: Base class for unit tests
- `IntegrationTestBase`: Base class for integration tests
- `E2ETestBase`: Base class for end-to-end tests

These base classes provide common functionality such as setup, teardown, and helper methods.

### Test Data

The `TestData` class provides constants for test data such as:

- Frequencies: Radio frequencies for testing
- Location IDs: Location identifiers for testing
- Quest IDs: Quest identifiers for testing
- Item IDs: Item identifiers for testing
- Message IDs: Message identifiers for testing

### Test Helpers

The `TestHelpers` class provides helper methods for testing such as:

- `WaitSeconds(float seconds)`: Waits for a specified number of seconds
- `WaitForSignal(object source, string signal)`: Waits for a signal to be emitted
- `CreateMockObject(string type)`: Creates a mock object for testing

### Test Runner

The `CSharpTestRunner` class runs the tests and reports results. It:

- Discovers tests using reflection
- Runs tests in the correct order
- Reports test results
- Handles test failures

## Save/Load System

The `SaveManager` class handles saving and loading game data. It:

### Save Data Structure

The save data is structured as a nested dictionary with the following keys:

- `gameState`: Game state data
- `inventory`: Inventory data
- `quests`: Quest data
- `map`: Map data
- `messages`: Message data
- `progression`: Progression data

### Save Methods

- `SaveGame(string saveName)`: Saves the game with the given name
- `SaveGameToFile(string filePath, Dictionary<string, object> saveData)`: Saves the game data to a file
- `CreateSaveData()`: Creates the save data dictionary

### Load Methods

- `LoadGame(string saveName)`: Loads the game with the given name
- `LoadGameFromFile(string filePath)`: Loads the game data from a file
- `ApplySaveData(Dictionary<string, object> saveData)`: Applies the save data to the game

### Auto-Save

The `SaveManager` also handles auto-saving:

- `AutoSave()`: Auto-saves the game
- `IsAutoSaveEnabled()`: Checks if auto-save is enabled
- `SetAutoSaveEnabled(bool enabled)`: Enables or disables auto-save

## Build and Deployment

Signal Lost uses Godot's export system with the .NET build pipeline for building and deploying the game.

### Export Templates

Export templates are configured for the following platforms:

- Windows: 64-bit executable
- macOS: Universal binary
- Linux: 64-bit executable

### Build Process

The build process involves:

1. Compiling the C# code
2. Packaging the game resources
3. Creating the final executable

This is handled by Godot's export system, which can be accessed through the Editor or via the command line.

### Deployment

The game can be deployed to various platforms:

- Steam: Using the Steamworks SDK
- Itch.io: Using the Butler tool
- Direct download: Hosting the executable on a website

## Best Practices

When working with the Signal Lost codebase, follow these best practices:

### Code Style

- Use PascalCase for class names and public methods
- Use camelCase for private fields and local variables
- Use `_` prefix for private fields
- Use XML documentation comments for public methods and classes
- Keep methods short and focused on a single responsibility
- Use meaningful variable and method names

### Architecture

- Keep the singleton classes focused on their specific responsibilities
- Use signals for communication between components
- Avoid direct references between components when possible
- Use dependency injection for testing
- Keep the scene tree structure clean and organized

### Performance

- Use object pooling for frequently created/destroyed objects
- Minimize allocations in performance-critical code
- Use Godot's built-in profiling tools to identify bottlenecks
- Optimize rendering by using appropriate techniques (e.g., static batching)
- Use async/await for long-running operations

### Testing

- Write tests for all new functionality
- Run tests before committing changes
- Use the appropriate test base class for the type of test
- Use test data constants for consistent test data
- Mock external dependencies for unit tests

## Common Issues and Solutions

### Audio Issues

**Issue**: Audio not playing or distorted.

**Solution**:
- Check that the audio buses are set up correctly
- Verify that the audio files are in the correct format
- Check that the volume is not set to 0 or muted
- Restart the audio system using `AudioManager.RestartAudio()`

### Performance Issues

**Issue**: Frame rate drops or stuttering.

**Solution**:
- Use the profiler to identify bottlenecks
- Optimize rendering by reducing draw calls
- Reduce the number of physics objects
- Use object pooling for frequently created/destroyed objects
- Consider using LOD (Level of Detail) for complex scenes

### Save/Load Issues

**Issue**: Save files not loading or corrupted.

**Solution**:
- Check that the save file exists and is not corrupted
- Verify that the save data structure matches the expected format
- Use try/catch blocks to handle exceptions during loading
- Implement a backup save system
- Add validation for save data

### UI Issues

**Issue**: UI elements not displaying correctly or responsive.

**Solution**:
- Check that the UI elements are properly anchored and sized
- Verify that the UI theme is applied correctly
- Use the Godot debugger to inspect the UI hierarchy
- Implement responsive design for different screen sizes
- Test on different resolutions and aspect ratios

### Cross-Platform Issues

**Issue**: Game behaves differently on different platforms.

**Solution**:
- Use platform-specific code only when necessary
- Test on all target platforms
- Use Godot's built-in platform detection
- Implement fallbacks for platform-specific features
- Document platform-specific behavior

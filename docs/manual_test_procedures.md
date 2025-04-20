# Signal Lost: Manual Test Procedures

This document outlines manual test procedures for the Signal Lost game. These procedures are designed to verify that the game functions correctly from a user perspective and to catch issues that automated tests might miss.

## Table of Contents

1. [Game Startup and Initial State](#game-startup-and-initial-state)
2. [Radio Functionality](#radio-functionality)
3. [Inventory System](#inventory-system)
4. [Map and Navigation](#map-and-navigation)
5. [Quest System](#quest-system)
6. [Game Progression](#game-progression)
7. [Save/Load System](#saveload-system)
8. [Audio System](#audio-system)
9. [Performance Testing](#performance-testing)
10. [Cross-Platform Testing](#cross-platform-testing)

## Game Startup and Initial State

### Test 1.1: Game Launch
1. Launch the game executable
2. Verify that the game starts without errors
3. Verify that the main menu appears
4. Verify that all menu options are visible and clickable

**Expected Result**: Game launches successfully and displays the main menu with all options.

### Test 1.2: New Game
1. Launch the game
2. Select "New Game" from the main menu
3. Verify that the game starts in the bunker location
4. Verify that the player has the initial inventory items
5. Verify that the radio is in the initial broken state

**Expected Result**: New game starts with the player in the bunker, with initial inventory items, and a broken radio.

### Test 1.3: Game Settings
1. Launch the game
2. Select "Settings" from the main menu
3. Adjust volume settings
4. Toggle fullscreen mode
5. Return to the main menu
6. Verify that settings are saved

**Expected Result**: Settings can be adjusted and are saved between sessions.

## Radio Functionality

### Test 2.1: Radio Repair
1. Start a new game
2. Complete the radio repair quest
3. Verify that the radio becomes functional
4. Verify that the radio interface appears

**Expected Result**: Radio becomes functional after repair and the interface is displayed.

### Test 2.2: Frequency Tuning
1. Start a game with a repaired radio
2. Turn on the radio
3. Adjust the frequency using the tuning knob
4. Verify that the frequency display updates
5. Verify that static noise changes with frequency
6. Tune to a known signal frequency
7. Verify that the signal is detected

**Expected Result**: Radio frequency can be adjusted, static noise changes, and signals can be detected.

### Test 2.3: Signal Detection
1. Start a game with a repaired radio
2. Turn on the radio
3. Tune to a known signal frequency (e.g., 91.5)
4. Verify that the signal strength meter increases
5. Verify that the static noise decreases
6. Verify that the message button becomes active
7. Press the message button
8. Verify that the message is displayed

**Expected Result**: Signals can be detected, signal strength is displayed, and messages can be viewed.

### Test 2.4: Scanning
1. Start a game with a repaired radio
2. Turn on the radio
3. Press the scan button
4. Verify that the frequency automatically changes
5. Verify that scanning stops when a signal is found
6. Verify that the signal is detected

**Expected Result**: Scanning automatically changes frequency and stops when a signal is found.

## Inventory System

### Test 3.1: Item Acquisition
1. Start a game
2. Navigate to a location with items
3. Search the location
4. Verify that items are added to the inventory
5. Verify that the inventory display updates

**Expected Result**: Items can be found and added to the inventory.

### Test 3.2: Item Usage
1. Start a game with items in the inventory
2. Open the inventory
3. Select an item
4. Use the item
5. Verify that the item is consumed (if consumable)
6. Verify that the item's effect is applied

**Expected Result**: Items can be used and their effects are applied.

### Test 3.3: Item Combining
1. Start a game with combinable items in the inventory
2. Open the inventory
3. Select the first item
4. Select the second item to combine with
5. Verify that the items are combined
6. Verify that the new item appears in the inventory

**Expected Result**: Items can be combined to create new items.

## Map and Navigation

### Test 4.1: Location Discovery
1. Start a game
2. Complete the radio repair quest
3. Find a signal that reveals a new location
4. Verify that the location is added to the map
5. Verify that the location is marked as discovered

**Expected Result**: New locations can be discovered through radio signals.

### Test 4.2: Location Navigation
1. Start a game with discovered locations
2. Open the map
3. Select a discovered location
4. Navigate to the location
5. Verify that the current location changes
6. Verify that the location's description and visuals are displayed

**Expected Result**: Player can navigate between discovered locations.

### Test 4.3: Map Fragments
1. Start a game
2. Find a map fragment
3. Use the map fragment
4. Verify that the map completion percentage increases
5. Verify that new locations are discovered

**Expected Result**: Map fragments can be used to discover new locations and increase map completion.

## Quest System

### Test 5.1: Quest Discovery
1. Start a game
2. Perform actions that trigger quest discovery (e.g., repair radio, find signal)
3. Verify that new quests are added to the quest log
4. Verify that quest descriptions are displayed

**Expected Result**: Quests can be discovered through gameplay actions.

### Test 5.2: Quest Objectives
1. Start a game with active quests
2. View the quest log
3. Check the objectives for a quest
4. Complete an objective
5. Verify that the objective is marked as completed
6. Verify that quest progress is updated

**Expected Result**: Quest objectives can be completed and progress is tracked.

### Test 5.3: Quest Completion
1. Start a game with active quests
2. Complete all objectives for a quest
3. Verify that the quest is marked as completed
4. Verify that any rewards are given
5. Verify that follow-up quests are discovered (if applicable)

**Expected Result**: Quests can be completed, rewards are given, and follow-up quests are discovered.

## Game Progression

### Test 6.1: Progression Stages
1. Start a new game
2. Complete the radio repair quest
3. Verify that progression advances to RadioRepair stage
4. Find a signal
5. Verify that progression advances to FirstSignal stage
6. Continue through the game, completing key quests
7. Verify that progression advances through each stage

**Expected Result**: Game progression advances through stages as key quests are completed.

### Test 6.2: Stage-Specific Content
1. Start a game at different progression stages
2. Verify that stage-specific quests are available
3. Verify that stage-specific locations are discovered
4. Verify that stage-specific messages are received

**Expected Result**: Each progression stage has specific content that becomes available.

### Test 6.3: Endgame
1. Progress through the game to the final stage
2. Complete the final quest
3. Verify that the endgame sequence plays
4. Verify that the game can be restarted or exited

**Expected Result**: The game can be completed and the endgame sequence plays.

## Save/Load System

### Test 7.1: Game Saving
1. Start a new game
2. Progress through the game
3. Save the game
4. Verify that the save file is created
5. Verify that the save file contains the correct timestamp

**Expected Result**: Game can be saved and save files are created.

### Test 7.2: Game Loading
1. Start the game
2. Load a saved game
3. Verify that the game state is restored correctly
4. Verify that the player's location, inventory, and progress are preserved

**Expected Result**: Saved games can be loaded and game state is restored correctly.

### Test 7.3: Auto-Save
1. Start a new game
2. Enable auto-save
3. Progress through the game
4. Verify that the game auto-saves at key points
5. Exit the game without manually saving
6. Restart the game
7. Verify that the auto-save can be loaded

**Expected Result**: Game auto-saves at key points and auto-saves can be loaded.

## Audio System

### Test 8.1: Sound Effects
1. Start a game
2. Perform actions that trigger sound effects (e.g., button clicks, item usage)
3. Verify that sound effects play
4. Adjust the sound effect volume
5. Verify that the volume changes

**Expected Result**: Sound effects play and volume can be adjusted.

### Test 8.2: Radio Audio
1. Start a game with a repaired radio
2. Turn on the radio
3. Verify that static noise plays
4. Tune to a signal
5. Verify that the signal audio plays
6. Verify that the static noise decreases
7. Adjust the radio volume
8. Verify that the volume changes

**Expected Result**: Radio audio plays correctly and volume can be adjusted.

### Test 8.3: Ambient Audio
1. Start a game
2. Navigate to different locations
3. Verify that location-specific ambient audio plays
4. Adjust the ambient audio volume
5. Verify that the volume changes

**Expected Result**: Ambient audio plays for each location and volume can be adjusted.

## Performance Testing

### Test 9.1: Frame Rate
1. Start a game
2. Enable the FPS counter
3. Play through different parts of the game
4. Verify that the frame rate remains stable
5. Verify that there are no significant frame drops

**Expected Result**: Game maintains a stable frame rate throughout gameplay.

### Test 9.2: Memory Usage
1. Start a game
2. Monitor memory usage
3. Play through different parts of the game
4. Verify that memory usage remains stable
5. Verify that there are no memory leaks

**Expected Result**: Game maintains stable memory usage without leaks.

### Test 9.3: Load Times
1. Start the game
2. Measure the time it takes to load the game
3. Load a saved game
4. Measure the time it takes to load the saved game
5. Navigate between locations
6. Measure the time it takes to load each location

**Expected Result**: Game and location load times are reasonable.

## Cross-Platform Testing

### Test 10.1: Windows
1. Install the game on a Windows machine
2. Run through the basic functionality tests
3. Verify that the game works correctly on Windows

**Expected Result**: Game functions correctly on Windows.

### Test 10.2: macOS
1. Install the game on a macOS machine
2. Run through the basic functionality tests
3. Verify that the game works correctly on macOS

**Expected Result**: Game functions correctly on macOS.

### Test 10.3: Linux
1. Install the game on a Linux machine
2. Run through the basic functionality tests
3. Verify that the game works correctly on Linux

**Expected Result**: Game functions correctly on Linux.

## Reporting Issues

When reporting issues found during manual testing, please include the following information:

1. Test ID and name
2. Steps to reproduce the issue
3. Expected result
4. Actual result
5. Screenshots or videos (if applicable)
6. System information (OS, hardware, etc.)
7. Any error messages or logs

This will help the development team identify and fix the issue more quickly.

## Test Tracking

Use the following table to track the results of manual tests:

| Test ID | Test Name | Tester | Date | Result | Notes |
|---------|-----------|--------|------|--------|-------|
| 1.1 | Game Launch | | | | |
| 1.2 | New Game | | | | |
| 1.3 | Game Settings | | | | |
| 2.1 | Radio Repair | | | | |
| 2.2 | Frequency Tuning | | | | |
| 2.3 | Signal Detection | | | | |
| 2.4 | Scanning | | | | |
| 3.1 | Item Acquisition | | | | |
| 3.2 | Item Usage | | | | |
| 3.3 | Item Combining | | | | |
| 4.1 | Location Discovery | | | | |
| 4.2 | Location Navigation | | | | |
| 4.3 | Map Fragments | | | | |
| 5.1 | Quest Discovery | | | | |
| 5.2 | Quest Objectives | | | | |
| 5.3 | Quest Completion | | | | |
| 6.1 | Progression Stages | | | | |
| 6.2 | Stage-Specific Content | | | | |
| 6.3 | Endgame | | | | |
| 7.1 | Game Saving | | | | |
| 7.2 | Game Loading | | | | |
| 7.3 | Auto-Save | | | | |
| 8.1 | Sound Effects | | | | |
| 8.2 | Radio Audio | | | | |
| 8.3 | Ambient Audio | | | | |
| 9.1 | Frame Rate | | | | |
| 9.2 | Memory Usage | | | | |
| 9.3 | Load Times | | | | |
| 10.1 | Windows | | | | |
| 10.2 | macOS | | | | |
| 10.3 | Linux | | | | |

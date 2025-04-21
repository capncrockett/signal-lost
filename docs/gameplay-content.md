# Gameplay Content

## Overview

The gameplay content provides the actual game experience for the player, including interactive objects, quests, and field exploration. It builds upon the gameplay systems to create a cohesive and engaging experience.

## Key Components

### 1. Interactive Objects

Interactive objects are the primary way for the player to interact with the game world. They can:

- Provide items to the player
- Require items to interact with
- Advance quest objectives
- Provide information to the player
- Trigger scene transitions

```csharp
public partial class InteractiveObject : Node2D
{
    // Object properties
    [Export] public string ObjectId { get; set; } = "";
    [Export] public string ObjectName { get; set; } = "";
    [Export] public string ObjectDescription { get; set; } = "";
    [Export] public bool IsInteractable { get; set; } = true;
    [Export] public bool RequiresItem { get; set; } = false;
    [Export] public string RequiredItemId { get; set; } = "";
    [Export] public string GrantsItemId { get; set; } = "";
    [Export] public int GrantsItemQuantity { get; set; } = 1;
    [Export] public bool IsOneTimeInteraction { get; set; } = false;
    [Export] public string InteractionMessage { get; set; } = "";
    [Export] public string QuestId { get; set; } = "";
    [Export] public string QuestObjectiveId { get; set; } = "";
    
    // Methods for handling interactions
    // ...
}
```

### 2. Field Exploration

The field exploration system allows the player to move around the game world and interact with objects. It includes:

- Grid-based movement
- Scene transitions
- Object interactions
- UI for displaying interactions

```csharp
public partial class FieldExplorationManager : Node
{
    // References to game systems and UI elements
    // ...
    
    // Interactive objects
    private Dictionary<Vector2I, InteractiveObject> _interactiveObjectsMap = new Dictionary<Vector2I, InteractiveObject>();
    private InteractiveObject _currentInteractiveObject;
    
    // Scene transitions
    private Dictionary<string, string> _sceneTransitions = new Dictionary<string, string>
    {
        { "bunker_exit", "res://scenes/field/ForestScene.tscn" },
        { "forest_entrance", "res://scenes/field/BunkerScene.tscn" },
        { "forest_cabin", "res://scenes/field/CabinScene.tscn" },
        { "cabin_exit", "res://scenes/field/ForestScene.tscn" }
    };
    
    // Methods for handling field exploration
    // ...
}
```

### 3. Quests

Quests provide goals and direction for the player. They include:

- Objectives to complete
- Rewards for completion
- Progression to the next quest

```
[resource]
script = ExtResource("1_8j3qv")
Id = "quest_radio_repair"
Title = "Repair the Radio"
Description = "Your radio is damaged and needs repair. Find the necessary parts to fix it."
IsActive = true
IsCompleted = false
IsDiscovered = true
Objectives = {
"find_radio_part": {
"description": "Find radio parts (0/2)",
"is_completed": false,
"progress": 0,
"required_progress": 2
},
"repair_radio": {
"description": "Use the parts to repair the radio",
"is_completed": false,
"progress": 0,
"required_progress": 1
}
}
Rewards = {
"radio_functionality": {
"description": "Radio functionality",
"is_granted": false,
"item_id": "",
"quantity": 0
}
}
NextQuestId = "quest_radio_signals"
```

## Game Locations

The game includes several key locations:

### 1. Bunker

The starting location where the player begins the game. It includes:

- Radio parts for repairing the radio
- A flashlight for exploring dark areas
- A locked cabinet that requires a key
- The exit to the forest

### 2. Forest

The main exploration area that connects different locations. It includes:

- The entrance back to the bunker
- A cabin that requires a key to enter
- A radio signal source
- A key to the cabin

### 3. Cabin

A small cabin in the forest that contains important items and information. It includes:

- The exit back to the forest
- Additional radio parts
- A journal with information about the incident and radio signals

## Quest Progression

The game follows a clear quest progression:

1. **Repair the Radio**
   - Find radio parts in the bunker and cabin
   - Use the parts to repair the radio
   - Reward: Radio functionality

2. **Mysterious Signals**
   - Find clues about radio signals in the journal
   - Locate the source of a radio signal in the forest
   - Decode the mysterious signal using the radio
   - Reward: Updated map with new locations

3. **Explore the Forest**
   - Find a map fragment in the locked cabinet
   - Find the key to the cabin in the forest
   - Find and enter the cabin
   - Reward: Access to the cabin and its contents

## Gameplay Loop

The core gameplay loop consists of:

1. **Exploration**: The player explores different locations to find items and clues
2. **Interaction**: The player interacts with objects to advance quests and gain items
3. **Radio Usage**: The player uses the radio to find and decode signals
4. **Quest Completion**: The player completes quests to progress through the game

## Integration with Game Systems

The gameplay content integrates with several other game systems:

### Radio System

- The player must repair the radio to progress
- Radio signals provide clues and advance quests
- Different frequencies reveal different types of signals

### Quest System

- Quests provide goals and direction for the player
- Completing quests advances the game progression
- Some quests are only available after completing others

### Inventory System

- The player collects items to repair the radio and complete quests
- Some items are required to access certain locations
- Using items can trigger quest progression

### Map System

- The player discovers new locations as they progress
- Some locations are only accessible after completing certain quests
- Visiting locations can trigger quest progression

## Future Enhancements

1. **Additional Locations**: Add more locations to explore
2. **More Interactive Objects**: Add more objects to interact with
3. **Complex Puzzles**: Add puzzles that require multiple items or clues
4. **Branching Quests**: Add quests with multiple outcomes
5. **Dynamic Events**: Add events that occur based on player actions
6. **NPC Interactions**: Add NPCs to interact with
7. **Environmental Storytelling**: Use the environment to tell the story
8. **Audio Cues**: Add audio cues to enhance the atmosphere
9. **Visual Effects**: Add visual effects to enhance the experience
10. **Difficulty Settings**: Add difficulty settings to adjust the challenge

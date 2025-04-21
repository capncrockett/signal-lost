# Game Content and Progression Plan

## Overview

This document outlines the plan for implementing game content and a progression system in Signal Lost, transforming it from a technical framework into a playable game with engaging content and mechanics.

## Game Concept

Signal Lost is a lo-fi, pixel-art narrative exploration game where a lone radio operator pieces together a fractured world through distorted frequencies and eerie signals. The player uses a radio to discover signals, decode messages, and explore locations to uncover the mystery of what happened to the world.

## Core Gameplay Loop

1. **Tune Radio**: Find signals at different frequencies
2. **Decode Messages**: Extract information from signals
3. **Explore Locations**: Visit places mentioned in messages
4. **Collect Items**: Find tools and resources
5. **Solve Puzzles**: Use items and information to progress
6. **Advance Story**: Unlock new areas and narrative elements

## Game Content Implementation

### 1. Radio Signals

Create a set of meaningful radio signals at different frequencies:

```csharp
// In RadioSystem.cs, InitializeSignals method
private void InitializeSignals()
{
    // Emergency Broadcast
    AddSignal(new RadioSignal
    {
        Id = "emergency_broadcast",
        Frequency = 91.5f,
        Message = "THIS IS AN EMERGENCY BROADCAST. ALL CITIZENS MUST EVACUATE TO DESIGNATED SHELTERS IMMEDIATELY. THIS IS NOT A DRILL.",
        Type = RadioSignalType.Voice,
        Strength = 0.9f
    });
    
    // Research Facility
    AddSignal(new RadioSignal
    {
        Id = "research_facility",
        Frequency = 94.3f,
        Message = "ATTENTION ALL PERSONNEL. CONTAINMENT BREACH IN SECTOR 7. INITIATE LOCKDOWN PROCEDURES.",
        Type = RadioSignalType.Voice,
        Strength = 0.7f
    });
    
    // Survivor Group
    AddSignal(new RadioSignal
    {
        Id = "survivor_group",
        Frequency = 88.9f,
        Message = "If anyone can hear this, we have established a safe zone at the old mill. We have supplies and shelter. Approach with caution.",
        Type = RadioSignalType.Voice,
        Strength = 0.5f
    });
    
    // Automated Weather Station
    AddSignal(new RadioSignal
    {
        Id = "weather_station",
        Frequency = 102.7f,
        Message = "AUTOMATED WEATHER ALERT: SEVERE ATMOSPHERIC DISTURBANCE DETECTED. SEEK SHELTER IMMEDIATELY.",
        Type = RadioSignalType.Automated,
        Strength = 0.8f
    });
    
    // Military Channel
    AddSignal(new RadioSignal
    {
        Id = "military_channel",
        Frequency = 107.3f,
        Message = "OSCAR TANGO SEVEN NINER. PERIMETER BREACH AT COORDINATES 47.2N 122.5W. REQUESTING IMMEDIATE BACKUP.",
        Type = RadioSignalType.Military,
        Strength = 0.6f
    });
    
    // Mysterious Beacon
    AddSignal(new RadioSignal
    {
        Id = "mysterious_beacon",
        Frequency = 99.1f,
        Message = "3-7-5-2-1-9-8-4-6... 3-7-5-2-1-9-8-4-6...",
        Type = RadioSignalType.Morse,
        Strength = 0.4f
    });
}
```

### 2. Map Locations

Design a game world map with key locations:

```csharp
// In MapSystem.cs, InitializeLocations method
private void InitializeLocations()
{
    // Starting Location - Emergency Bunker
    AddLocation(new MapLocation
    {
        Id = "emergency_bunker",
        Name = "Emergency Bunker",
        Description = "A small underground bunker with basic supplies and a radio.",
        Position = new Vector2(15, 15),
        IsDiscovered = true,
        IsAccessible = true
    });
    
    // Research Facility
    AddLocation(new MapLocation
    {
        Id = "research_facility",
        Name = "Research Facility",
        Description = "A large complex where experimental research was conducted.",
        Position = new Vector2(8, 22),
        IsDiscovered = false,
        IsAccessible = false,
        RequiredItem = "keycard_research"
    });
    
    // Old Mill
    AddLocation(new MapLocation
    {
        Id = "old_mill",
        Name = "Old Mill",
        Description = "An abandoned mill now serving as a survivor camp.",
        Position = new Vector2(25, 10),
        IsDiscovered = false,
        IsAccessible = true
    });
    
    // Weather Station
    AddLocation(new MapLocation
    {
        Id = "weather_station",
        Name = "Weather Station",
        Description = "An automated weather monitoring station on a hilltop.",
        Position = new Vector2(30, 18),
        IsDiscovered = false,
        IsAccessible = true
    });
    
    // Military Outpost
    AddLocation(new MapLocation
    {
        Id = "military_outpost",
        Name = "Military Outpost",
        Description = "A small military installation with communications equipment.",
        Position = new Vector2(12, 5),
        IsDiscovered = false,
        IsAccessible = false,
        RequiredItem = "military_badge"
    });
    
    // Mysterious Tower
    AddLocation(new MapLocation
    {
        Id = "mysterious_tower",
        Name = "Mysterious Tower",
        Description = "A tall tower emitting strange signals.",
        Position = new Vector2(20, 28),
        IsDiscovered = false,
        IsAccessible = false,
        RequiredItem = "tower_key"
    });
}
```

### 3. Inventory Items

Create a set of collectible and usable items:

```csharp
// In InventorySystem.cs, InitializeItems method
private void InitializeItems()
{
    // Basic Tools
    AddItem(new InventoryItem
    {
        Id = "radio",
        Name = "Portable Radio",
        Description = "A battery-powered radio that can receive signals.",
        Type = ItemType.Tool,
        IsConsumable = false,
        IsEquippable = true,
        IconIndex = 0
    });
    
    AddItem(new InventoryItem
    {
        Id = "flashlight",
        Name = "Flashlight",
        Description = "A battery-powered flashlight for dark areas.",
        Type = ItemType.Tool,
        IsConsumable = false,
        IsEquippable = true,
        IconIndex = 1
    });
    
    // Key Items
    AddItem(new InventoryItem
    {
        Id = "keycard_research",
        Name = "Research Facility Keycard",
        Description = "A keycard that grants access to the research facility.",
        Type = ItemType.Key,
        IsConsumable = false,
        IsEquippable = false,
        IconIndex = 2
    });
    
    AddItem(new InventoryItem
    {
        Id = "military_badge",
        Name = "Military ID Badge",
        Description = "An ID badge that grants access to military installations.",
        Type = ItemType.Key,
        IsConsumable = false,
        IsEquippable = false,
        IconIndex = 3
    });
    
    // Consumables
    AddItem(new InventoryItem
    {
        Id = "batteries",
        Name = "Batteries",
        Description = "Used to power electronic devices.",
        Type = ItemType.Consumable,
        IsConsumable = true,
        IsEquippable = false,
        IconIndex = 4
    });
    
    AddItem(new InventoryItem
    {
        Id = "medkit",
        Name = "First Aid Kit",
        Description = "Used to treat injuries.",
        Type = ItemType.Consumable,
        IsConsumable = true,
        IsEquippable = false,
        IconIndex = 5
    });
    
    // Documents
    AddItem(new InventoryItem
    {
        Id = "research_notes",
        Name = "Research Notes",
        Description = "Notes detailing experiments conducted at the research facility.",
        Type = ItemType.Document,
        IsConsumable = false,
        IsEquippable = false,
        IconIndex = 6,
        Content = "Project SIGNAL: Attempt to establish communication across [REDACTED]. Initial tests show promising results but significant side effects including [REDACTED] and temporal anomalies."
    });
}
```

### 4. Quest System

Implement a quest system to guide player progression:

```csharp
// In QuestSystem.cs, InitializeQuests method
private void InitializeQuests()
{
    // Main Storyline Quests
    
    // Starting Quest
    AddQuest(new Quest
    {
        Id = "repair_radio",
        Title = "Repair the Radio",
        Description = "Find the necessary components to repair your damaged radio.",
        Status = QuestStatus.Active,
        Objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                Id = "find_antenna",
                Description = "Find a replacement antenna",
                IsCompleted = false,
                RequiredItemId = "antenna"
            },
            new QuestObjective
            {
                Id = "find_batteries",
                Description = "Find batteries",
                IsCompleted = false,
                RequiredItemId = "batteries"
            }
        },
        Rewards = new List<string> { "radio" }
    });
    
    // Second Quest
    AddQuest(new Quest
    {
        Id = "locate_survivors",
        Title = "Locate Survivors",
        Description = "Use the radio to find other survivors in the area.",
        Status = QuestStatus.Inactive,
        Objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                Id = "tune_to_survivor_frequency",
                Description = "Tune to the survivor frequency (88.9 MHz)",
                IsCompleted = false,
                RequiredSignalId = "survivor_group"
            },
            new QuestObjective
            {
                Id = "visit_old_mill",
                Description = "Visit the old mill",
                IsCompleted = false,
                RequiredLocationId = "old_mill"
            }
        },
        Rewards = new List<string> { "map" }
    });
    
    // Side Quests
    AddQuest(new Quest
    {
        Id = "weather_monitoring",
        Title = "Weather Monitoring",
        Description = "Investigate the automated weather alerts.",
        Status = QuestStatus.Inactive,
        Objectives = new List<QuestObjective>
        {
            new QuestObjective
            {
                Id = "tune_to_weather_frequency",
                Description = "Tune to the weather station frequency (102.7 MHz)",
                IsCompleted = false,
                RequiredSignalId = "weather_station"
            },
            new QuestObjective
            {
                Id = "visit_weather_station",
                Description = "Visit the weather station",
                IsCompleted = false,
                RequiredLocationId = "weather_station"
            },
            new QuestObjective
            {
                Id = "retrieve_weather_data",
                Description = "Retrieve the weather data",
                IsCompleted = false,
                RequiredItemId = "weather_data"
            }
        },
        Rewards = new List<string> { "batteries", "weather_map" }
    });
}
```

## Game Progression System

### 1. Game Stages

Define clear stages of progression:

```csharp
// In GameProgressionSystem.cs
public enum GameStage
{
    Beginning,
    Exploration,
    Discovery,
    Revelation,
    Conclusion
}

public class GameProgressionSystem : Node
{
    // Current game stage
    private GameStage _currentStage = GameStage.Beginning;
    
    // Progress within the current stage (0-100)
    private int _stageProgress = 0;
    
    // Stage descriptions
    private Dictionary<GameStage, string> _stageDescriptions = new Dictionary<GameStage, string>
    {
        { GameStage.Beginning, "You've just arrived at the emergency bunker. Your radio is damaged and needs repair." },
        { GameStage.Exploration, "With your radio working, you can now explore the surrounding area and look for other survivors." },
        { GameStage.Discovery, "You've made contact with other survivors and discovered clues about what happened." },
        { GameStage.Revelation, "The truth about the incident is starting to become clear as you piece together the evidence." },
        { GameStage.Conclusion, "You now know what happened and must make a final decision about what to do next." }
    };
    
    // Stage objectives
    private Dictionary<GameStage, string> _stageObjectives = new Dictionary<GameStage, string>
    {
        { GameStage.Beginning, "Repair your radio by finding the necessary components." },
        { GameStage.Exploration, "Explore the area and make contact with other survivors." },
        { GameStage.Discovery, "Investigate the research facility and military outpost." },
        { GameStage.Revelation, "Decode the mysterious signal and find the source." },
        { GameStage.Conclusion, "Reach the mysterious tower and confront the truth." }
    };
    
    // Get current stage
    public GameStage GetCurrentStage()
    {
        return _currentStage;
    }
    
    // Get stage description
    public string GetStageDescription()
    {
        return _stageDescriptions[_currentStage];
    }
    
    // Get stage objective
    public string GetStageObjective()
    {
        return _stageObjectives[_currentStage];
    }
    
    // Get stage progress
    public int GetStageProgress()
    {
        return _stageProgress;
    }
    
    // Advance to next stage
    public void AdvanceStage()
    {
        if (_currentStage < GameStage.Conclusion)
        {
            _currentStage++;
            _stageProgress = 0;
            EmitSignal(nameof(StageChanged), (int)_currentStage);
        }
    }
    
    // Update stage progress
    public void UpdateProgress(int progress)
    {
        _stageProgress = Mathf.Clamp(progress, 0, 100);
        EmitSignal(nameof(ProgressChanged), _stageProgress);
        
        // Automatically advance stage if progress reaches 100
        if (_stageProgress >= 100)
        {
            AdvanceStage();
        }
    }
    
    // Increment progress by a specific amount
    public void IncrementProgress(int amount)
    {
        UpdateProgress(_stageProgress + amount);
    }
}
```

### 2. Progression Triggers

Implement triggers that advance the game progression:

```csharp
// In GameState.cs
public void CheckProgressionTriggers()
{
    var progressionSystem = GetNode<GameProgressionSystem>("/root/GameProgressionSystem");
    var questSystem = GetNode<QuestSystem>("/root/QuestSystem");
    
    if (progressionSystem == null || questSystem == null)
    {
        return;
    }
    
    // Check current stage
    GameStage currentStage = progressionSystem.GetCurrentStage();
    
    switch (currentStage)
    {
        case GameStage.Beginning:
            // Check if radio repair quest is completed
            var radioQuest = questSystem.GetQuest("repair_radio");
            if (radioQuest != null && radioQuest.Status == QuestStatus.Completed)
            {
                progressionSystem.UpdateProgress(100); // This will advance to the next stage
            }
            break;
            
        case GameStage.Exploration:
            // Check if survivor quest is completed
            var survivorQuest = questSystem.GetQuest("locate_survivors");
            if (survivorQuest != null && survivorQuest.Status == QuestStatus.Completed)
            {
                progressionSystem.IncrementProgress(50);
            }
            
            // Check if weather quest is completed
            var weatherQuest = questSystem.GetQuest("weather_monitoring");
            if (weatherQuest != null && weatherQuest.Status == QuestStatus.Completed)
            {
                progressionSystem.IncrementProgress(50);
            }
            break;
            
        // Additional cases for other stages
    }
}
```

### 3. Unlockable Content

Implement a system for unlocking content as the player progresses:

```csharp
// In GameState.cs
public bool IsContentUnlocked(string contentId)
{
    // Check if content is unlocked based on game progression
    var progressionSystem = GetNode<GameProgressionSystem>("/root/GameProgressionSystem");
    
    if (progressionSystem == null)
    {
        return false;
    }
    
    GameStage currentStage = progressionSystem.GetCurrentStage();
    int stageProgress = progressionSystem.GetStageProgress();
    
    // Define unlock conditions for different content
    switch (contentId)
    {
        case "research_facility":
            return currentStage >= GameStage.Exploration && stageProgress >= 50;
            
        case "military_outpost":
            return currentStage >= GameStage.Discovery && stageProgress >= 25;
            
        case "mysterious_tower":
            return currentStage >= GameStage.Revelation && stageProgress >= 75;
            
        case "advanced_radio":
            return currentStage >= GameStage.Discovery && stageProgress >= 50;
            
        default:
            return false;
    }
}
```

## Integration with Existing Systems

### 1. Connect Radio System to Game Progression

```csharp
// In RadioSystem.cs
public void OnSignalDetected(string signalId, float frequency)
{
    // Get game state and quest system
    var gameState = GetNode<GameState>("/root/GameState");
    var questSystem = GetNode<QuestSystem>("/root/QuestSystem");
    
    if (gameState == null || questSystem == null)
    {
        return;
    }
    
    // Add to discovered signals
    gameState.AddDiscoveredSignal(signalId, frequency);
    
    // Check if this signal completes any quest objectives
    questSystem.CheckSignalObjectives(signalId);
    
    // Check progression triggers
    gameState.CheckProgressionTriggers();
}
```

### 2. Connect Map System to Game Progression

```csharp
// In MapSystem.cs
public void OnLocationDiscovered(string locationId)
{
    // Get game state and quest system
    var gameState = GetNode<GameState>("/root/GameState");
    var questSystem = GetNode<QuestSystem>("/root/QuestSystem");
    
    if (gameState == null || questSystem == null)
    {
        return;
    }
    
    // Add to discovered locations
    gameState.AddDiscoveredLocation(locationId);
    
    // Check if this location completes any quest objectives
    questSystem.CheckLocationObjectives(locationId);
    
    // Check progression triggers
    gameState.CheckProgressionTriggers();
}
```

### 3. Connect Inventory System to Game Progression

```csharp
// In InventorySystem.cs
public void OnItemAdded(string itemId)
{
    // Get game state and quest system
    var gameState = GetNode<GameState>("/root/GameState");
    var questSystem = GetNode<QuestSystem>("/root/QuestSystem");
    
    if (gameState == null || questSystem == null)
    {
        return;
    }
    
    // Check if this item completes any quest objectives
    questSystem.CheckItemObjectives(itemId);
    
    // Check progression triggers
    gameState.CheckProgressionTriggers();
}
```

## Implementation Steps

1. Implement radio signals with meaningful content
2. Create map locations with descriptions and requirements
3. Design inventory items with different types and uses
4. Implement the quest system with main and side quests
5. Create the game progression system with stages
6. Implement progression triggers and unlockable content
7. Connect all systems together
8. Test the complete gameplay loop

## Testing Plan

1. Test each radio signal to ensure it can be detected and decoded
2. Test map locations to verify they can be discovered and accessed
3. Test inventory items to ensure they can be collected and used
4. Test quests to verify objectives can be completed
5. Test game progression to ensure stages advance correctly
6. Test unlockable content to verify it becomes available at the right time
7. Test the complete gameplay loop from start to finish

## Success Criteria

1. Players can tune the radio to find meaningful signals
2. The map shows locations that can be explored
3. The inventory contains useful items that serve a purpose
4. Quests provide clear objectives and rewards
5. Game progression feels natural and rewarding
6. The overall experience tells a cohesive story
7. Players can complete the game from start to finish

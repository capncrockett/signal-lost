# Enhanced Radio Signals System

## Overview

The Enhanced Radio Signals System is a comprehensive upgrade to the existing radio functionality in Signal Lost. It introduces different signal types, signal strength mechanics, and integration with the inventory system to create a more immersive and engaging radio experience.

## Key Features

1. **Multiple Signal Types**
   - Voice signals with human speech content
   - Morse code signals with encoded messages
   - Data signals with technical information

2. **Signal Strength Mechanics**
   - Signal strength varies based on proximity to the exact frequency
   - Signal quality affects content readability (static effects)
   - Equipment can boost signal detection capabilities

3. **Hidden Signals**
   - Some signals are hidden and require special equipment to detect
   - Adds an exploration element to the radio tuning experience

4. **Equipment Integration**
   - Different radio equipment provides different capabilities
   - Special items can enhance signal detection or reveal hidden signals

5. **Visual Feedback**
   - Animations for different signal types
   - Visual representation of signal strength
   - Content formatting based on signal type

## System Architecture

The Enhanced Radio Signals System consists of several key components:

### 1. EnhancedSignalData

A data class that represents a radio signal with enhanced properties:

```csharp
public partial class EnhancedSignalData : Resource
{
    // Basic signal properties
    [Export] public string Id { get; set; } = "";
    [Export] public string Name { get; set; } = "";
    [Export] public string Description { get; set; } = "";
    [Export] public float Frequency { get; set; } = 90.0f;
    [Export] public SignalType Type { get; set; } = SignalType.Voice;
    [Export] public string Content { get; set; } = "";
    [Export] public string LocationId { get; set; } = "";
    [Export] public bool IsStatic { get; set; } = false;
    [Export] public float Bandwidth { get; set; } = 0.3f;
    [Export] public float MinSignalStrength { get; set; } = 0.5f;
    [Export] public bool IsHidden { get; set; } = false;
    [Export] public bool IsDecoded { get; set; } = false;
    [Export] public string RequiredItemToUnlock { get; set; } = "";
    // Related signal IDs (not exported due to Godot C# limitations with List<string>)
    public List<string> RelatedSignalIds { get; set; } = new List<string>();
    [Export] public string NextSignalId { get; set; } = "";
    [Export] public int StoryProgressRequired { get; set; } = 0;
    [Export] public int StoryProgressUnlocked { get; set; } = 0;
    [Export] public string AudioPath { get; set; } = "";
    [Export] public string ImagePath { get; set; } = "";
    
    // Methods for signal strength calculation, content formatting, etc.
}
```

### 2. RadioSignalsManager

A singleton that manages all radio signals in the game:

```csharp
public partial class RadioSignalsManager : Node
{
    // Dictionary of all signals in the game
    private Dictionary<string, EnhancedSignalData> _signalDatabase = new Dictionary<string, EnhancedSignalData>();
    
    // List of discovered signal IDs
    private List<string> _discoveredSignals = new List<string>();
    
    // Currently active signal
    private EnhancedSignalData _currentSignal = null;
    
    // Current signal strength
    private float _currentSignalStrength = 0.0f;
    
    // Signal detection boost from equipment (multiplier)
    private float _signalBoost = 1.0f;
    
    // Can detect hidden signals
    private bool _canDetectHiddenSignals = false;
    
    // Methods for signal management, detection, etc.
}
```

### 3. RadioSignalDisplay

A UI component that displays radio signals and their content:

```csharp
public partial class RadioSignalDisplay : Control
{
    // UI elements
    [Export] private Label _signalNameLabel;
    [Export] private Label _signalTypeLabel;
    [Export] private Label _signalStrengthLabel;
    [Export] private RichTextLabel _signalContentLabel;
    [Export] private TextureRect _signalStrengthIndicator;
    [Export] private Panel _noSignalPanel;
    [Export] private AnimationPlayer _animationPlayer;
    
    // Methods for updating the UI based on signal data
}
```

### 4. RadioEquipmentHandler

A class that integrates the radio signals system with the inventory system:

```csharp
public partial class RadioEquipmentHandler : Node
{
    // Equipment effects
    private Dictionary<string, EquipmentEffect> _equipmentEffects = new Dictionary<string, EquipmentEffect>
    {
        { "radio", new EquipmentEffect { SignalBoost = 1.0f, CanDetectHiddenSignals = false } },
        { "radio_enhanced", new EquipmentEffect { SignalBoost = 2.0f, CanDetectHiddenSignals = false } },
        { "strange_crystal", new EquipmentEffect { SignalBoost = 1.5f, CanDetectHiddenSignals = true } }
    };
    
    // Methods for handling equipment effects
}
```

### 5. RadioSystemIntegration

A bridge between the enhanced radio signals system and the existing radio system:

```csharp
public partial class RadioSystemIntegration : Node
{
    // Signal mapping (maps enhanced signals to legacy signals)
    private Dictionary<string, string> _signalMapping = new Dictionary<string, string>();
    
    // Methods for syncing the two systems
}
```

## Integration with Game Systems

The Enhanced Radio Signals System integrates with several other game systems:

1. **GameState**
   - Stores discovered signals
   - Tracks radio power state and current frequency
   - Provides signals for frequency changes and radio toggling

2. **Inventory System**
   - Provides equipment that affects signal detection
   - Allows for item-based signal unlocking

3. **Audio System**
   - Plays appropriate audio for different signal types
   - Adjusts static noise based on signal strength

4. **Map System**
   - Locations can be associated with signals
   - Discovering signals can reveal map locations

## Usage Examples

### 1. Adding a New Signal

```csharp
// Create a new signal
var signal = new EnhancedSignalData
{
    Id = "emergency_broadcast",
    Name = "Emergency Broadcast",
    Description = "An automated emergency broadcast message.",
    Frequency = 121.5f,
    Type = SignalType.Voice,
    Content = "This is an emergency broadcast. All civilians are advised to evacuate the area immediately. This is not a drill.",
    LocationId = "emergency_center",
    IsStatic = true,
    Bandwidth = 0.3f,
    MinSignalStrength = 0.3f
};

// Add the signal to the database
_radioSignalsManager.AddSignalToDatabase(signal);
```

### 2. Detecting Signals at a Frequency

```csharp
// Set the radio frequency
_gameState.SetFrequency(121.5f);

// The RadioSignalsManager will automatically check for signals at this frequency
// and emit the appropriate signals (SignalDiscovered, SignalStrengthChanged, etc.)
```

### 3. Applying Equipment Effects

```csharp
// Add an enhanced radio to the inventory
_gameState.AddToInventory("radio_enhanced");

// The RadioEquipmentHandler will automatically update the signal boost
// This will make it easier to detect weak signals
```

## Future Enhancements

1. **Signal Interference**
   - Implement interference between nearby signals
   - Add environmental factors that affect signal reception

2. **Dynamic Signals**
   - Create signals that change over time
   - Implement scheduled broadcasts at specific times

3. **Signal Recording**
   - Allow players to record signals for later analysis
   - Implement a signal comparison system

4. **Advanced Decoding**
   - Add puzzles related to signal decoding
   - Implement a cipher system for encrypted signals

## Conclusion

The Enhanced Radio Signals System significantly improves the radio experience in Signal Lost by adding depth, variety, and integration with other game systems. It transforms the radio from a simple tuning mechanic into a core gameplay element that drives exploration, discovery, and narrative progression.

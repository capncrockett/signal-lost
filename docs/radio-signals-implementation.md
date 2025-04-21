# Radio Signals Implementation

## Overview

The radio signals implementation provides a comprehensive system for managing radio signals in the game. It includes signal discovery, decoding, and content display, as well as integration with the game's audio system and UI.

## Key Components

### 1. RadioSignalsManager

The RadioSignalsManager is the core class that manages radio signals in the game. It:

- Maintains a database of all signals in the game
- Tracks discovered signals
- Calculates signal strength based on frequency proximity
- Handles signal discovery and decoding
- Integrates with the game's audio system
- Provides signals for UI updates

```csharp
public partial class RadioSignalsManager : Node
{
    // Signal database
    private Dictionary<string, RadioSignalData> _signalDatabase = new Dictionary<string, RadioSignalData>();
    
    // Discovered signals
    private HashSet<string> _discoveredSignals = new HashSet<string>();
    
    // Current signal
    private RadioSignalData _currentSignal = null;
    private float _currentSignalStrength = 0.0f;
    
    // Signal detection settings
    private float _signalDetectionThreshold = 0.3f;
    private float _signalBoost = 1.0f;
    private bool _canDetectHiddenSignals = false;
    
    // Methods for managing signals
    // ...
}
```

### 2. EnhancedSignalData

The EnhancedSignalData class represents a radio signal with detailed information and content. It:

- Stores basic signal properties (ID, name, frequency, etc.)
- Calculates signal strength based on frequency proximity
- Formats signal content based on signal type and strength
- Applies static effects to content based on signal strength
- Checks if a signal can be detected based on requirements

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
    // Additional properties
    // ...
    
    // Methods for signal processing
    // ...
}
```

### 3. RadioSignalDisplay

The RadioSignalDisplay class is a UI component that displays radio signals and their content. It:

- Shows signal information (name, type, frequency, etc.)
- Displays signal content with appropriate formatting
- Shows signal strength indicator
- Plays animations based on signal type
- Handles UI updates when signals change

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
    
    // Methods for UI updates
    // ...
}
```

### 4. RadioEquipmentHandler

The RadioEquipmentHandler class manages radio equipment that affects signal detection. It:

- Tracks equipped radio items
- Applies signal boost based on equipment
- Enables detection of hidden signals with special equipment
- Integrates with the inventory system

```csharp
public partial class RadioEquipmentHandler : Node
{
    // Equipment effects
    private Dictionary<string, EquipmentEffect> _equipmentEffects = new Dictionary<string, EquipmentEffect>();
    
    // Methods for handling equipment
    // ...
}
```

## Signal Types

The system supports three types of radio signals:

1. **Voice**: Human speech signals with narrative content
2. **Morse**: Morse code signals that need to be decoded
3. **Data**: Binary or encoded data signals

Each signal type has its own formatting, animation, and audio.

## Signal Properties

Radio signals have several key properties:

- **Frequency**: The exact frequency of the signal (e.g., 90.0 MHz)
- **Bandwidth**: How wide the signal is (how close you need to be to detect it)
- **MinSignalStrength**: The minimum signal strength required to decode the content
- **IsHidden**: Whether the signal requires special equipment to detect
- **IsStatic**: Whether the signal is always available or only at certain times/locations
- **LocationId**: The location associated with the signal (for map integration)

## Signal Detection

Signal detection is based on several factors:

1. **Frequency Proximity**: How close the radio is tuned to the exact frequency
2. **Signal Boost**: Equipment can boost signal detection
3. **Hidden Signal Detection**: Special equipment can detect hidden signals
4. **Signal Requirements**: Some signals require story progress or items to detect

The signal strength is calculated as:

```csharp
float CalculateSignalStrength(float frequency, RadioSignalData signal)
{
    float distance = Mathf.Abs(frequency - signal.Frequency);
    float maxDistance = signal.Bandwidth;
    
    // Calculate strength based on how close we are to the exact frequency
    if (distance <= maxDistance)
    {
        return 1.0f - (distance / maxDistance);
    }
    else
    {
        return 0.0f;
    }
}
```

## Signal Content

Signal content is formatted based on the signal type and strength:

1. **Voice**: Plain text with static effects based on signal strength
2. **Morse**: Formatted with color and spacing for readability
3. **Data**: Formatted with monospace font and color for a technical look

Static effects are applied based on signal strength:

```csharp
string ApplyStatic(string content, float signalStrength)
{
    // Calculate static intensity (0.0 = no static, 1.0 = full static)
    float staticIntensity = 1.0f - signalStrength;
    
    // Apply static to content
    string result = "";
    foreach (char c in content)
    {
        if (GD.Randf() < staticIntensity)
        {
            // Replace with static character
            result += GetRandomStaticCharacter();
        }
        else
        {
            // Keep original character
            result += c;
        }
    }
    
    return result;
}
```

## Integration with Game Systems

The radio signals system integrates with several other game systems:

### Audio System

- Plays appropriate audio for different signal types
- Adjusts static volume based on signal strength
- Provides audio cues for signal discovery and decoding

### UI System

- Displays signal information and content
- Shows signal strength indicator
- Provides visual feedback for signal discovery and decoding

### Inventory System

- Equipment can affect signal detection
- Some signals require specific items to detect
- Signal discovery can unlock new items

### Quest System

- Signal discovery can advance quests
- Some quests require finding specific signals
- Signal content can provide clues for quests

### Map System

- Signals can be associated with locations
- Signal discovery can reveal map locations
- Map locations can affect signal availability

## Example Signals

The system includes several example signals:

1. **Emergency Broadcast**: A voice signal at 121.5 MHz with emergency information
2. **Military Communication**: A voice signal at 95.7 MHz with military communications
3. **Survivor Message**: A voice signal at 103.2 MHz with a message from survivors
4. **Morse Code**: A morse code signal at 88.3 MHz with an SOS message
5. **Data Transmission**: A data signal at 107.5 MHz with encoded information
6. **Hidden Signal**: A hidden voice signal at 91.1 MHz with classified information

## Future Enhancements

1. **Dynamic Signals**: Signals that change over time or based on player actions
2. **Signal Interference**: Interference between nearby signals
3. **Signal Recording**: Ability to record signals for later analysis
4. **Advanced Decoding**: Puzzles related to signal decoding
5. **Environmental Effects**: Weather and terrain effects on signal reception
6. **Signal Triangulation**: Using multiple signal sources to locate a target
7. **Two-Way Communication**: Ability to respond to signals
8. **Signal Jamming**: Ability to jam or block signals
9. **Custom Radio Equipment**: More specialized radio equipment with unique effects
10. **Signal Analysis Tools**: Tools for analyzing and comparing signals

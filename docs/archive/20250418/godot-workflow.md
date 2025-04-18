# Development Workflow (Signal Lost with C# in Godot Engine)

> **Note**: This workflow document outlines the current development process for Signal Lost using C# in Godot Engine with Alpha/Beta Agent collaboration.

## Pre-coding Checklist

1. Pull latest changes from the `develop` branch
2. Open the project in Godot Engine
3. Check the sprint document for your agent's responsibilities (Alpha or Beta)
4. Review interface contracts between agents

## Alpha/Beta Agent Development

### Overview

Signal Lost development follows an Alpha/Beta Agent development approach where two AI agents collaborate with distinct roles and responsibilities:

1. **Agent Alpha**: Senior developer responsible for primary code development, including:

   - Writing and implementing new features
   - Unit and integration testing
   - Ensuring C# type safety
   - Fixing bugs and errors
   - Maintaining code quality and documentation

2. **Agent Beta**: QA developer responsible for quality assurance, including:
   - End-to-end testing
   - Code cleanup and optimization
   - Removing unused code and variables
   - Improving code organization
   - Ensuring consistent code style

### Agent Collaboration

The agents collaborate through a structured development process:

- Agent Alpha leads development and implements new features
- Agent Beta focuses on testing and code quality
- Interface contracts are established to ensure smooth integration
- Agents communicate through GitHub issues and PRs
- Agent Beta reviews Agent Alpha's code for quality and test coverage
- Agent Alpha implements fixes based on Agent Beta's feedback

```csharp
// Example of interface contract between agents
// Shared interface used by both Alpha and Beta agents
namespace SignalLost
{
    [GlobalClass]
    public partial class Signal : Resource
    {
        public string Id { get; set; }
        public float Frequency { get; set; }
        public float Strength { get; set; }
        public string Type { get; set; }  // "message", "location", or "event"
        public string Content { get; set; }
        public bool Discovered { get; set; }
        public int Timestamp { get; set; }

        // Agent Alpha implements signal detection
        public static Signal DetectSignal(float frequency)
        {
            // Implementation by Agent Alpha
            return null;
        }

        // Agent Beta consumes signals for narrative progression
        public void ProcessSignal()
        {
            // Implementation by Agent Beta
        }
    }
}
```

## Godot Engine Approach

Signal Lost uses the Godot Engine for game development, providing a robust framework for 2D games with excellent audio processing capabilities.

### Key Principles

1. **Godot Nodes for Game Objects**

   - Use Godot's node system for game objects
   - Leverage the scene system for reusable components
   - Use signals for communication between nodes
   - Follow Godot's design patterns

2. **C# for Game Logic**

   - Use C# for all game logic
   - Leverage strong typing for better code quality
   - Follow C# coding conventions and the Godot C# style guide
   - Keep scripts focused and modular

3. **State Management**
   - Use Godot's built-in state management
   - Implement autoloaded singletons for global state
   - Use signals for state updates
   - Keep state changes predictable and traceable

### Example Implementation

```csharp
using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class RadioTuner : Control
    {
        // Export variables for inspector configuration
        [Export]
        public float MinFrequency { get; set; } = 88.0f;

        [Export]
        public float MaxFrequency { get; set; } = 108.0f;

        [Export]
        public float FrequencyStep { get; set; } = 0.1f;

        // Private member variables
        private Label _frequencyDisplay;
        private Button _powerButton;
        private Slider _frequencySlider;
        private ProgressBar _signalStrengthMeter;
        private Control _staticVisualization;

        // Local state
        private bool _isOn = false;
        private float _currentFrequency = 90.0f;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get UI references
            _frequencyDisplay = GetNode<Label>("FrequencyDisplay");
            _powerButton = GetNode<Button>("PowerButton");
            _frequencySlider = GetNode<Slider>("FrequencySlider");
            _signalStrengthMeter = GetNode<ProgressBar>("SignalStrengthMeter");
            _staticVisualization = GetNode<Control>("StaticVisualization");

            // Initialize UI
            UpdateUI();

            // Connect signals
            _powerButton.Pressed += OnPowerButtonPressed;
            _frequencySlider.ValueChanged += OnFrequencySliderChanged;
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            if (_isOn)
            {
                // Process input
                if (Input.IsActionJustPressed("tune_up"))
                {
                    ChangeFrequency(FrequencyStep);
                }
                else if (Input.IsActionJustPressed("tune_down"))
                {
                    ChangeFrequency(-FrequencyStep);
                }

                // Update visualization
                UpdateStaticVisualization(delta);
            }
        }

        // Change the frequency by a specific amount
        public void ChangeFrequency(float amount)
        {
            _currentFrequency = Mathf.Clamp(_currentFrequency + amount, MinFrequency, MaxFrequency);
            _currentFrequency = Mathf.Round(_currentFrequency * 10) / 10;  // Round to nearest 0.1
            UpdateUI();
        }

        // Toggle the radio power
        public void TogglePower()
        {
            _isOn = !_isOn;
            UpdateUI();
        }

        // Update the UI based on current state
        private void UpdateUI()
        {
            // Update frequency display
            _frequencyDisplay.Text = $"{_currentFrequency:F1} MHz";

            // Update power button
            _powerButton.Text = _isOn ? "ON" : "OFF";

            // Update frequency slider
            float percentage = (_currentFrequency - MinFrequency) / (MaxFrequency - MinFrequency);
            _frequencySlider.Value = percentage * 100;
        }

        // Event handlers
        private void OnPowerButtonPressed()
        {
            TogglePower();
        }

        private void OnFrequencySliderChanged(double value)
        {
            float percentage = (float)value / 100.0f;
            _currentFrequency = MinFrequency + percentage * (MaxFrequency - MinFrequency);
            _currentFrequency = Mathf.Round(_currentFrequency * 10) / 10;  // Round to nearest 0.1
            UpdateUI();
        }

        private void UpdateStaticVisualization(double delta)
        {
            // Implementation for static visualization
        }
    }
}
```

## Component Development Workflow

1. **Plan the component**:

   - Review the current sprint document for requirements
   - Define properties and state requirements
   - Sketch the component structure
   - Identify potential reusable sub-components
   - Determine which Godot nodes to use

2. **Write tests first**:

   - Create test file with the same name as the component
   - Write tests for all expected behaviors
   - Include tests for edge cases
   - Add tests for both UI elements and game logic

3. **Implement the component**:

   - Create the scene file
   - Add necessary nodes
   - Implement the component logic
   - Add visual elements
   - Ensure proper signal connections

4. **Verify**:

   - Run tests: `./godot_project/run_tests.sh`
   - Manually test in the Godot editor
   - Check for errors in the Godot console
   - Verify visual elements

5. **Document**:
   - Add comments to the code
   - Update component documentation
   - Add usage examples if needed
   - Update sprint document with progress

## Testing Approach

### Unit Tests (GUT) - Agent Alpha Responsibility

- Test each script in isolation
- Focus on behavior, not implementation details
- Mock external dependencies
- Maintain at least 80% test coverage

```csharp
// Example unit test
using Godot;
using System;
using GUT;

namespace SignalLost.Tests
{
    [TestClass]
    public class RadioTunerTests : Test
    {
        private PackedScene _radioTunerScene;
        private RadioTuner _radioTuner = null;

        public override void Before()
        {
            _radioTunerScene = GD.Load<PackedScene>("res://scenes/radio/RadioTuner.tscn");
            _radioTuner = _radioTunerScene.Instantiate<RadioTuner>();
            AddChild(_radioTuner);
        }

        public override void After()
        {
            _radioTuner.QueueFree();
            _radioTuner = null;
        }

        [Test]
        public void TestFrequencyChange()
        {
            // Arrange
            _radioTuner.Call("SetFrequency", 90.0f);

            // Act
            _radioTuner.Call("ChangeFrequency", 0.1f);

            // Assert
            AssertEqual((float)_radioTuner.Get("_currentFrequency"), 90.1f,
                "Frequency should be 90.1 after increasing by 0.1");
        }
    }
}
```

### Cross-Agent Integration Tests - Shared Responsibility

- Agent Alpha: Write integration tests for new features
- Agent Beta: Verify tests cover all edge cases
- Verify interface contracts are properly implemented
- Test boundary conditions and edge cases
- Ensure consistent behavior across components

```csharp
// Example Cross-Agent Integration test
using Godot;
using System;
using GUT;

namespace SignalLost.Tests
{
    [TestClass]
    public class CrossAgentIntegrationTests : Test
    {
        private PackedScene _radioTunerScene; // Alpha agent component
        private PackedScene _narrativeDisplayScene; // Beta agent component

        [Test]
        public void TestRadioTunerSignalDetectionTriggersNarrativeDisplayUpdate()
        {
            // Arrange
            _radioTunerScene = GD.Load<PackedScene>("res://scenes/radio/RadioTuner.tscn");
            _narrativeDisplayScene = GD.Load<PackedScene>("res://scenes/narrative/NarrativeDisplay.tscn");

            var radioTuner = _radioTunerScene.Instantiate<RadioTuner>();
            var narrativeDisplay = _narrativeDisplayScene.Instantiate<Control>();
            AddChild(radioTuner);
            AddChild(narrativeDisplay);

            // Act
            radioTuner.Call("SetFrequency", 91.5f); // Known signal frequency
            radioTuner.Call("ProcessFrequency");

            // Assert
            AssertTrue((bool)narrativeDisplay.Get("has_message"),
                "Narrative display should have a message");
            AssertStringContains((string)narrativeDisplay.Get("message_text"),
                "Signal detected");

            // Cleanup
            radioTuner.QueueFree();
            narrativeDisplay.QueueFree();
        }
    }
}
```

### Manual Testing - Agent Beta Responsibility

- Test critical user flows
- Verify interactions between components
- Test on multiple platforms
- Capture screenshots for visual verification
- Document any issues found

## Pre-commit Checklist

```bash
# Run tests
./godot_project/run_tests.sh

# Check for C# compilation errors
dotnet build godot_project/SignalLost.sln

# Run all checks at once
./godot_project/check_all.sh
```

## Debugging Tips

- Use Godot's built-in debugger
- Add print statements for debugging
- Use the Remote tab for runtime inspection
- Leverage Godot's profiler for performance issues
- Monitor cross-agent integration with interface logging:
  ```csharp
  // Enable debug mode to log all cross-agent interactions
  Contracts.EnableDebugMode();
  ```

## State Management

### General Principles

- Use autoloaded singletons for global state
- Keep component state local when possible
- Use signals for state updates
- Follow unidirectional data flow

### Agent-Specific Responsibilities

#### Agent Alpha (Senior Developer)

- Implement new features and components
- Write unit and integration tests
- Fix bugs and errors
- Maintain code quality and documentation
- Ensure type safety
- Create PRs for feature implementation

#### Agent Beta (QA Developer)

- Write and maintain manual tests
- Clean up unused code and variables
- Improve code organization
- Ensure consistent code style
- Review Agent Alpha's PRs for quality
- Report issues and suggest improvements
- Create PRs for code cleanup and optimization

### Agent-Specific State Management

- Alpha agent manages audio and radio tuner state
- Beta agent manages narrative and game progression state
- Shared state is managed through well-defined interfaces
- Use C# strong typing to enforce state boundaries between agents

```csharp
// Example state management with agent-specific singletons
// Alpha Agent Singleton (Radio & Audio)
using Godot;
using System;

namespace SignalLost
{
    public partial class AudioManager : Node
    {
        // Radio state
        public float CurrentFrequency { get; private set; } = 90.0f;
        public bool IsRadioOn { get; private set; } = false;
        public float SignalStrength { get; private set; } = 0.0f;
        public float StaticIntensity { get; private set; } = 0.5f;

        // Audio state
        public float Volume { get; private set; } = 0.8f;
        public bool IsMuted { get; private set; } = false;

        // Signals
        [Signal]
        public delegate void FrequencyChangedEventHandler(float newFrequency);

        [Signal]
        public delegate void RadioToggledEventHandler(bool isOn);

        // Functions
        public void SetFrequency(float freq)
        {
            CurrentFrequency = Mathf.Clamp(freq, 88.0f, 108.0f);
            EmitSignal(SignalName.FrequencyChanged, CurrentFrequency);
        }

        public void ToggleRadio()
        {
            IsRadioOn = !IsRadioOn;
            EmitSignal(SignalName.RadioToggled, IsRadioOn);
        }
    }

    // Beta Agent Singleton (Narrative & Game State)
    public partial class NarrativeManager : Node
    {
        // Narrative state
        public Dictionary<string, object> Messages { get; private set; } = new Dictionary<string, object>();
        public string CurrentMessageId { get; private set; } = null;

        // Game progress state
        public List<string> DiscoveredSignals { get; private set; } = new List<string>();
        public List<string> CompletedObjectives { get; private set; } = new List<string>();
        public string PlayerLocation { get; private set; } = "bunker";
        public int GameTime { get; private set; } = 0;

        // Signals
        [Signal]
        public delegate void MessageReceivedEventHandler(string messageId);

        [Signal]
        public delegate void ObjectiveCompletedEventHandler(string objectiveId);

        // Functions
        public void ReceiveMessage(string messageId)
        {
            CurrentMessageId = messageId;
            EmitSignal(SignalName.MessageReceived, messageId);
        }

        public void CompleteObjective(string objectiveId)
        {
            if (!CompletedObjectives.Contains(objectiveId))
            {
                CompletedObjectives.Add(objectiveId);
                EmitSignal(SignalName.ObjectiveCompleted, objectiveId);
            }
        }
    }
}
```

## Development Priorities

1. **Sprint Goals**: Focus on completing the current sprint objectives
2. **Agent Responsibilities**:
   - Agent Alpha: Feature development, unit testing, and code quality
   - Agent Beta: Manual testing, code cleanup, and quality assurance
3. **Test Coverage**: Maintain at least 80% test coverage
4. **Type Safety**: Use C#'s strong typing effectively to prevent bugs
5. **Component Architecture**: Create a clean, maintainable node structure
6. **Accessibility**: Ensure the game is accessible to all users
7. **Code Quality**: Follow Godot style guide and maintain consistent code style
8. **Performance**: Optimize rendering and state updates
9. **Documentation**: Keep documentation up-to-date with implementation changes

## C# Best Practices

- Use strong typing for all variables and functions
- Keep functions small and focused
- Use clear, descriptive variable and function names
- Add comments for complex logic
- Follow C# coding conventions and the Godot C# style guide
- Use signals for communication between nodes
- Leverage Godot's built-in features
- Create reusable components
- Document public methods and properties with XML comments

```csharp
// Example of good C# usage for components
using Godot;
using System;

namespace SignalLost
{
    public partial class RadioTuner : Control
    {
        // Export variables for inspector configuration
        [Export]
        public float InitialFrequency { get; set; } = 91.5f;

        [Export]
        public float MinFrequency { get; set; } = 88.0f;

        [Export]
        public float MaxFrequency { get; set; } = 108.0f;

        // Signal definitions
        [Signal]
        public delegate void FrequencyChangedEventHandler(float frequency);

        [Signal]
        public delegate void SignalFoundEventHandler(GameState.SignalData signalData);

        // Private variables
        private bool _isOn = false;
        private float _currentFrequency;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            _currentFrequency = InitialFrequency;
            UpdateUi();
        }

        // Public methods
        public void SetFrequency(float frequency)
        {
            _currentFrequency = Mathf.Clamp(frequency, MinFrequency, MaxFrequency);
            UpdateUi();
            EmitSignal(SignalName.FrequencyChanged, _currentFrequency);
            CheckForSignal();
        }

        public void TogglePower()
        {
            _isOn = !_isOn;
            UpdateUi();
        }

        // Private methods
        private void UpdateUi()
        {
            GetNode<Label>("FrequencyDisplay").Text = $"{_currentFrequency:F1} MHz";
            GetNode<Button>("PowerButton").Text = _isOn ? "ON" : "OFF";
        }

        private void CheckForSignal()
        {
            var gameState = GetNode<GameState>("/root/GameState");
            var signalData = gameState.FindSignalAtFrequency(_currentFrequency);
            if (signalData != null)
            {
                EmitSignal(SignalName.SignalFound, signalData);
            }
        }
    }
}
```

## Agent Synchronization Workflow

### Daily Sync Process

1. Start by updating develop:

   ```bash
   git checkout develop
   git pull origin develop
   ```

2. Create feature branch:

   ```bash
   # For Alpha agent
   git checkout -b feature/alpha/your-feature
   # For Beta agent
   git checkout -b feature/beta/your-feature
   ```

3. Before creating PR:
   ```bash
   git checkout your-feature-branch
   git fetch origin
   git rebase origin/develop
   ```

### Conflict Prevention

1. Check other agent's work:

   ```bash
   git fetch origin
   git checkout origin/feature/[alpha|beta]/*
   ```

2. Use interface validation:
   ```bash
   godot --path godot_project --script addons/contract_validator/validate.gd
   ```

### Merge Conflict Resolution

1. Both agents must use the contract validation tool
2. Conflicts in shared interfaces must be resolved via contract branches
3. Create a contract resolution branch:

   ```bash
   git checkout -b feature/contract/resolve-conflict
   ```

4. Both agents must approve contract resolution PRs

### PR Requirements

1. PR title format: `[Alpha|Beta] Feature description`
2. Required checks for Agent Alpha PRs:
   - All unit and integration tests passing
   - C# compilation passing
   - Proper documentation
   - No conflicts with develop
3. Required checks for Agent Beta PRs:
   - All manual tests passing
   - Code cleanup and optimization complete
   - No unused variables or code
   - Consistent code style
   - No conflicts with develop

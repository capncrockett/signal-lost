using Godot;
using System;
using SignalLost;

/// <summary>
/// Initializes the demo environment for the radio interface.
/// </summary>
[GlobalClass]
public partial class DemoInitializer : Control
{
    // Called when the node enters the scene tree
    public override void _Ready()
    {
        // Check if GameState exists
        var gameState = GetNode<GameState>("/root/GameState");
        if (gameState == null)
        {
            // Create GameState
            gameState = new GameState();
            gameState.Name = "GameState";
            GetTree().Root.AddChild(gameState);
            GD.Print("DemoInitializer: Created GameState");
        }
        
        // Check if RadioSystem exists
        var radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
        if (radioSystem == null)
        {
            // Create RadioSystem
            radioSystem = new RadioSystem();
            radioSystem.Name = "RadioSystem";
            GetTree().Root.AddChild(radioSystem);
            GD.Print("DemoInitializer: Created RadioSystem");
        }
        
        // Check if MessageManager exists
        var messageManager = GetNode<MessageManager>("/root/MessageManager");
        if (messageManager == null)
        {
            // Create MessageManager
            messageManager = new MessageManager();
            messageManager.Name = "MessageManager";
            GetTree().Root.AddChild(messageManager);
            GD.Print("DemoInitializer: Created MessageManager");
        }
        
        // Initialize test signals and messages
        InitializeTestData(gameState);
        
        GD.Print("DemoInitializer: Demo environment initialized");
    }
    
    // Initialize test data for the demo
    private void InitializeTestData(GameState gameState)
    {
        // Add test messages if they don't exist
        if (!gameState.Messages.ContainsKey("msg_emergency"))
        {
            gameState.Messages["msg_emergency"] = new GameState.MessageData
            {
                Id = "msg_emergency",
                Title = "EMERGENCY BROADCAST",
                Content = "This is an emergency broadcast. All personnel must evacuate immediately. This is not a drill.",
                Decoded = false
            };
        }
        
        if (!gameState.Messages.ContainsKey("msg_weather"))
        {
            gameState.Messages["msg_weather"] = new GameState.MessageData
            {
                Id = "msg_weather",
                Title = "Weather Report",
                Content = "Severe weather warning for the following areas: Sector 7, Sector 12, and Sector 15. Expect heavy precipitation and reduced visibility.",
                Decoded = false
            };
        }
        
        if (!gameState.Messages.ContainsKey("msg_music"))
        {
            gameState.Messages["msg_music"] = new GameState.MessageData
            {
                Id = "msg_music",
                Title = "Music Station",
                Content = "You're listening to the wasteland's only music station. Up next: 'The End of the World' by Skeeter Davis.",
                Decoded = true
            };
        }
        
        // Add test signals if they don't exist
        bool hasEmergencySignal = false;
        bool hasWeatherSignal = false;
        bool hasMusicSignal = false;
        
        foreach (var signal in gameState.Signals)
        {
            if (signal.MessageId == "msg_emergency") hasEmergencySignal = true;
            if (signal.MessageId == "msg_weather") hasWeatherSignal = true;
            if (signal.MessageId == "msg_music") hasMusicSignal = true;
        }
        
        if (!hasEmergencySignal)
        {
            gameState.Signals.Add(new GameState.SignalData
            {
                Id = "signal_emergency",
                Frequency = 91.5f,
                MessageId = "msg_emergency",
                IsStatic = true,
                Bandwidth = 0.3f
            });
        }
        
        if (!hasWeatherSignal)
        {
            gameState.Signals.Add(new GameState.SignalData
            {
                Id = "signal_weather",
                Frequency = 95.7f,
                MessageId = "msg_weather",
                IsStatic = false,
                Bandwidth = 0.2f
            });
        }
        
        if (!hasMusicSignal)
        {
            gameState.Signals.Add(new GameState.SignalData
            {
                Id = "signal_music",
                Frequency = 103.2f,
                MessageId = "msg_music",
                IsStatic = true,
                Bandwidth = 0.4f
            });
        }
        
        GD.Print("DemoInitializer: Test data initialized");
    }
}

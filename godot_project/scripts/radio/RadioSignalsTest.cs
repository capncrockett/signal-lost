using Godot;

namespace SignalLost.Radio
{
    /// <summary>
    /// Test script for the radio signals system.
    /// </summary>
    [GlobalClass]
    public partial class RadioSignalsTest : Node
    {
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            GD.Print("RadioSignalsTest: Starting test...");
            
            // Get references to systems
            var radioSignalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");
            var gameState = GetNode<GameState>("/root/GameState");
            
            if (radioSignalsManager == null)
            {
                GD.PrintErr("RadioSignalsTest: RadioSignalsManager not found");
                return;
            }
            
            if (gameState == null)
            {
                GD.PrintErr("RadioSignalsTest: GameState not found");
                return;
            }
            
            // Connect to signals
            radioSignalsManager.SignalDiscovered += OnSignalDiscovered;
            radioSignalsManager.SignalLost += OnSignalLost;
            radioSignalsManager.SignalStrengthChanged += OnSignalStrengthChanged;
            
            // Turn on the radio
            gameState.ToggleRadio();
            
            // Add radio to inventory
            gameState.AddToInventory("radio");
            
            // Test different frequencies
            TestFrequency(gameState, 98.5f); // Radio Station
            TestFrequency(gameState, 106.7f); // Distress Signal
            TestFrequency(gameState, 121.5f); // Emergency Broadcast
            TestFrequency(gameState, 140.85f); // Military Communication
            TestFrequency(gameState, 152.25f); // Research Facility
            TestFrequency(gameState, 162.4f); // Weather Station
            TestFrequency(gameState, 135.0f); // Automated Beacon
            
            // Test hidden signal with crystal
            gameState.AddToInventory("strange_crystal");
            TestFrequency(gameState, 87.5f); // Mysterious Signal
            
            GD.Print("RadioSignalsTest: Test completed");
        }
        
        // Test a specific frequency
        private void TestFrequency(GameState gameState, float frequency)
        {
            GD.Print($"RadioSignalsTest: Testing frequency {frequency} MHz");
            gameState.SetFrequency(frequency);
            
            // Wait a moment to let the signal be processed
            // In a real test, we would use a timer or async/await
        }
        
        // Handle signal discovered
        private void OnSignalDiscovered(string signalId)
        {
            GD.Print($"RadioSignalsTest: Signal discovered: {signalId}");
        }
        
        // Handle signal lost
        private void OnSignalLost(string signalId)
        {
            GD.Print($"RadioSignalsTest: Signal lost: {signalId}");
        }
        
        // Handle signal strength changed
        private void OnSignalStrengthChanged(string signalId, float strength)
        {
            GD.Print($"RadioSignalsTest: Signal strength changed: {signalId} = {strength:F2}");
        }
    }
}

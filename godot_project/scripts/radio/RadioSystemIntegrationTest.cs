using Godot;
using SignalLost.Radio;

namespace SignalLost
{
    /// <summary>
    /// Test script for the radio system integration.
    /// </summary>
    [GlobalClass]
    public partial class RadioSystemIntegrationTest : Node
    {
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            GD.Print("RadioSystemIntegrationTest: Starting test...");
            
            // Get references to systems
            var radioSignalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");
            var radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            var gameState = GetNode<GameState>("/root/GameState");
            var radioSystemIntegration = GetNode<RadioSystemIntegration>("/root/RadioSystemIntegration");
            
            if (radioSignalsManager == null)
            {
                GD.PrintErr("RadioSystemIntegrationTest: RadioSignalsManager not found");
                return;
            }
            
            if (radioSystem == null)
            {
                GD.PrintErr("RadioSystemIntegrationTest: RadioSystem not found");
                return;
            }
            
            if (gameState == null)
            {
                GD.PrintErr("RadioSystemIntegrationTest: GameState not found");
                return;
            }
            
            if (radioSystemIntegration == null)
            {
                GD.PrintErr("RadioSystemIntegrationTest: RadioSystemIntegration not found");
                return;
            }
            
            // Connect to signals
            radioSignalsManager.SignalDiscovered += OnSignalDiscovered;
            radioSignalsManager.SignalLost += OnSignalLost;
            radioSignalsManager.SignalStrengthChanged += OnSignalStrengthChanged;
            
            radioSystem.SignalDetected += OnLegacySignalDetected;
            radioSystem.SignalLost += OnLegacySignalLost;
            
            // Turn on the radio
            gameState.ToggleRadio();
            
            // Test different frequencies
            TestFrequency(gameState, 98.5f); // Radio Station
            TestFrequency(gameState, 106.7f); // Distress Signal
            TestFrequency(gameState, 121.5f); // Emergency Broadcast
            
            // Check if signals are properly mapped
            CheckSignalMapping(radioSignalsManager, radioSystem);
            
            GD.Print("RadioSystemIntegrationTest: Test completed");
        }
        
        // Test a specific frequency
        private void TestFrequency(GameState gameState, float frequency)
        {
            GD.Print($"RadioSystemIntegrationTest: Testing frequency {frequency} MHz");
            gameState.SetFrequency(frequency);
            
            // Wait a moment to let the signal be processed
            // In a real test, we would use a timer or async/await
        }
        
        // Check if signals are properly mapped between the enhanced and legacy systems
        private void CheckSignalMapping(RadioSignalsManager radioSignalsManager, RadioSystem radioSystem)
        {
            GD.Print("RadioSystemIntegrationTest: Checking signal mapping...");
            
            // Get all signals from both systems
            var enhancedSignals = radioSignalsManager.GetAllSignals();
            var legacySignals = radioSystem.GetSignals();
            
            // Check if all enhanced signals have corresponding legacy signals
            foreach (var enhancedSignal in enhancedSignals)
            {
                bool found = false;
                foreach (var legacySignal in legacySignals)
                {
                    if (legacySignal.Value.Frequency == enhancedSignal.Value.Frequency)
                    {
                        found = true;
                        GD.Print($"RadioSystemIntegrationTest: Found mapping for {enhancedSignal.Key} -> {legacySignal.Key}");
                        break;
                    }
                }
                
                if (!found)
                {
                    GD.PrintErr($"RadioSystemIntegrationTest: No mapping found for {enhancedSignal.Key}");
                }
            }
        }
        
        // Handle enhanced signal discovered
        private void OnSignalDiscovered(string signalId)
        {
            GD.Print($"RadioSystemIntegrationTest: Enhanced signal discovered: {signalId}");
        }
        
        // Handle enhanced signal lost
        private void OnSignalLost(string signalId)
        {
            GD.Print($"RadioSystemIntegrationTest: Enhanced signal lost: {signalId}");
        }
        
        // Handle enhanced signal strength changed
        private void OnSignalStrengthChanged(string signalId, float strength)
        {
            GD.Print($"RadioSystemIntegrationTest: Enhanced signal strength changed: {signalId} = {strength:F2}");
        }
        
        // Handle legacy signal detected
        private void OnLegacySignalDetected(string signalId, float frequency)
        {
            GD.Print($"RadioSystemIntegrationTest: Legacy signal detected: {signalId} at {frequency} MHz");
        }
        
        // Handle legacy signal lost
        private void OnLegacySignalLost(string signalId)
        {
            GD.Print($"RadioSystemIntegrationTest: Legacy signal lost: {signalId}");
        }
    }
}

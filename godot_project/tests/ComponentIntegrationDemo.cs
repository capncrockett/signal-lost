using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalLost
{
    [GlobalClass]
    public partial class ComponentIntegrationDemo : Node
    {
        private RichTextLabel _stateInfo;
        private TabContainer _tabContainer;
        
        // Timer for updating the state display
        private Timer _updateTimer;
        
        public override void _Ready()
        {
            // Get references to UI elements
            _stateInfo = GetNode<RichTextLabel>("ComponentsContainer/RightPanel/StatePanel/VBoxContainer/StateInfo");
            _tabContainer = GetNode<TabContainer>("ComponentsContainer/RightPanel/TabContainer");
            
            // Create a timer to update the state display
            _updateTimer = new Timer();
            _updateTimer.WaitTime = 0.5f; // Update every half second
            _updateTimer.Timeout += UpdateStateDisplay;
            AddChild(_updateTimer);
            _updateTimer.Start();
            
            // Initial update
            UpdateStateDisplay();
            
            GD.Print("Component Integration Demo initialized");
        }
        
        private void UpdateStateDisplay()
        {
            // Get the GameState singleton
            var gameState = GetNode<GameState>("/root/GameState");
            
            if (gameState == null)
            {
                GD.PrintErr("GameState singleton not found!");
                return;
            }
            
            // Format the discovered frequencies
            string discoveredFreqs = "None";
            if (gameState.DiscoveredFrequencies.Count > 0)
            {
                discoveredFreqs = string.Join(", ", gameState.DiscoveredFrequencies.Select(f => $"{f:F1} MHz"));
            }
            
            // Update the state display
            _stateInfo.Text = $"[b]Radio State:[/b] {(gameState.IsRadioOn ? "ON" : "OFF")}\n" +
                             $"[b]Current Frequency:[/b] {gameState.CurrentFrequency:F1} MHz\n" +
                             $"[b]Signal Strength:[/b] {gameState.CalculateSignalStrength(gameState.CurrentFrequency):F0}%\n" +
                             $"[b]Current Location:[/b] {gameState.CurrentLocation}\n" +
                             $"[b]Discovered Frequencies:[/b] {discoveredFreqs}";
        }
    }
}

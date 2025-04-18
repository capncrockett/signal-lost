using Godot;
using System;

namespace SignalLost
{
    /// <summary>
    /// A simplified RadioTuner class to support tests
    /// </summary>
    [GlobalClass]
    public partial class RadioTuner : Control
    {
        // Constants for frequency limits
        private const float MinFrequency = 88.0f;
        private const float MaxFrequency = 108.0f;

        // Reference to the GameState
        private GameState _gameState;

        // UI state
        private bool _isScanning = false;
        private bool _showMessage = false;
        private string _currentSignalId = "";
        private float _signalStrength = 0.0f;
        private float _staticIntensity = 0.5f;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get the GameState singleton
            _gameState = GetNode<GameState>("/root/GameState");
            
            // Connect to GameState signals
            _gameState.RadioToggled += OnRadioToggled;
            _gameState.FrequencyChanged += OnFrequencyChanged;
        }

        // Called when the radio is toggled
        public void OnRadioToggled(bool isOn)
        {
            // If radio is turned off, stop scanning
            if (!isOn && _isScanning)
            {
                _isScanning = false;
            }
        }

        // Called when the frequency is changed
        public void OnFrequencyChanged(float frequency)
        {
            // Process the new frequency
            ProcessFrequency(frequency);
        }

        // Process the current frequency
        private void ProcessFrequency(float frequency)
        {
            // Find signal at the current frequency
            var signalData = _gameState.FindSignalAtFrequency(frequency);
            
            if (signalData != null)
            {
                // Calculate signal strength
                float strength = _gameState.CalculateSignalStrength(frequency, signalData);
                
                // Update state
                _signalStrength = strength;
                _staticIntensity = 1.0f - strength;
                _currentSignalId = signalData.MessageId;
                
                // Add to discovered frequencies if signal is strong enough
                if (strength > 0.7f)
                {
                    _gameState.AddDiscoveredFrequency(signalData.Frequency);
                }
            }
            else
            {
                // No signal, just static
                _signalStrength = 0.1f;
                _staticIntensity = 0.9f;
                _currentSignalId = "";
            }
        }

        // Toggle the radio power
        public void TogglePower()
        {
            if (_gameState == null) return;

            _gameState.ToggleRadio();

            // For testing purposes, manually call OnRadioToggled
            OnRadioToggled(_gameState.IsRadioOn);
        }

        // Change the frequency
        public void ChangeFrequency(float amount)
        {
            // Calculate new frequency
            float newFrequency = _gameState.CurrentFrequency + amount;
            
            // Clamp to valid range
            newFrequency = Mathf.Clamp(newFrequency, MinFrequency, MaxFrequency);
            
            // Update GameState
            _gameState.SetFrequency(newFrequency);
        }
    }
}

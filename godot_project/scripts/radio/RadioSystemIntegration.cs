using Godot;
using System.Collections.Generic;
using SignalLost.Radio;

namespace SignalLost
{
    /// <summary>
    /// Integrates the enhanced radio signals system with the existing radio system.
    /// </summary>
    [GlobalClass]
    public partial class RadioSystemIntegration : Node
    {
        // References to systems
        private RadioSignalsManager _radioSignalsManager;
        private RadioSystem _radioSystem;
        private GameState _gameState;
        private AudioManager _audioManager;
        
        // Signal mapping (maps enhanced signals to legacy signals)
        private Dictionary<string, string> _signalMapping = new Dictionary<string, string>();
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to systems
            _radioSignalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");
            _radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            _gameState = GetNode<GameState>("/root/GameState");
            _audioManager = GetNode<AudioManager>("/root/AudioManager");
            
            if (_radioSignalsManager == null)
            {
                GD.PrintErr("RadioSystemIntegration: RadioSignalsManager not found");
                return;
            }
            
            if (_radioSystem == null)
            {
                GD.PrintErr("RadioSystemIntegration: RadioSystem not found");
                return;
            }
            
            if (_gameState == null)
            {
                GD.PrintErr("RadioSystemIntegration: GameState not found");
                return;
            }
            
            if (_audioManager == null)
            {
                GD.PrintErr("RadioSystemIntegration: AudioManager not found");
                return;
            }
            
            // Connect signals
            _radioSignalsManager.SignalDiscovered += OnSignalDiscovered;
            _radioSignalsManager.SignalLost += OnSignalLost;
            _radioSignalsManager.SignalStrengthChanged += OnSignalStrengthChanged;
            
            _radioSystem.SignalDetected += OnLegacySignalDetected;
            _radioSystem.SignalLost += OnLegacySignalLost;
            
            _gameState.FrequencyChanged += OnFrequencyChanged;
            _gameState.RadioToggled += OnRadioToggled;
            
            // Initialize
            SyncRadioSystems();
        }
        
        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_radioSignalsManager != null)
            {
                _radioSignalsManager.SignalDiscovered -= OnSignalDiscovered;
                _radioSignalsManager.SignalLost -= OnSignalLost;
                _radioSignalsManager.SignalStrengthChanged -= OnSignalStrengthChanged;
            }
            
            if (_radioSystem != null)
            {
                _radioSystem.SignalDetected -= OnLegacySignalDetected;
                _radioSystem.SignalLost -= OnLegacySignalLost;
            }
            
            if (_gameState != null)
            {
                _gameState.FrequencyChanged -= OnFrequencyChanged;
                _gameState.RadioToggled -= OnRadioToggled;
            }
        }
        
        // Sync the radio systems
        private void SyncRadioSystems()
        {
            // Sync frequency
            _radioSystem.SetFrequency(_gameState.CurrentFrequency);
            
            // Sync radio power
            _radioSystem.SetRadioPower(_gameState.IsRadioOn);
            
            // Sync signals
            SyncSignals();
        }
        
        // Sync signals between the enhanced and legacy systems
        private void SyncSignals()
        {
            // Clear existing signals in the legacy system
            foreach (var signal in _radioSystem.GetSignals())
            {
                _radioSystem.RemoveSignal(signal.Key);
            }
            
            // Add enhanced signals to the legacy system
            foreach (var signal in _radioSignalsManager.GetAllSignals())
            {
                RegisterEnhancedSignal(signal.Value);
            }
        }
        
        // Register an enhanced signal with the legacy system
        private void RegisterEnhancedSignal(EnhancedSignalData enhancedSignal)
        {
            // Create a legacy signal from the enhanced signal
            var legacySignal = new RadioSignal
            {
                Id = enhancedSignal.Id + "_legacy",
                Frequency = enhancedSignal.Frequency,
                Message = enhancedSignal.Content,
                Type = ConvertSignalType(enhancedSignal.Type),
                Strength = enhancedSignal.IsHidden ? 0.0f : 1.0f,
                IsActive = true
            };
            
            // Add the legacy signal to the legacy system
            _radioSystem.AddSignal(legacySignal);
            
            // Map the enhanced signal ID to the legacy signal ID
            _signalMapping[enhancedSignal.Id] = legacySignal.Id;
            
            GD.Print($"Registered enhanced signal {enhancedSignal.Id} as legacy signal {legacySignal.Id}");
        }
        
        // Convert enhanced signal type to legacy signal type
        private RadioSignalType ConvertSignalType(SignalType enhancedType)
        {
            switch (enhancedType)
            {
                case SignalType.Voice:
                    return RadioSignalType.Voice;
                case SignalType.Morse:
                    return RadioSignalType.Morse;
                case SignalType.Data:
                    return RadioSignalType.Data;
                default:
                    return RadioSignalType.Voice;
            }
        }
        
        // Handle enhanced signal discovered
        private void OnSignalDiscovered(string signalId)
        {
            var enhancedSignal = _radioSignalsManager.GetSignal(signalId);
            if (enhancedSignal != null)
            {
                // Register the enhanced signal with the legacy system
                RegisterEnhancedSignal(enhancedSignal);
                
                // Add to GameState's discovered signals
                _gameState.AddDiscoveredSignal(signalId, enhancedSignal.Frequency);
                
                // Play discovery sound
                _audioManager?.PlayEffect("discovery");
            }
        }
        
        // Handle enhanced signal lost
        private void OnSignalLost(string signalId)
        {
            // Check if we have a mapping for this signal
            if (_signalMapping.TryGetValue(signalId, out string legacySignalId))
            {
                // Remove the legacy signal from the legacy system
                _radioSystem.RemoveSignal(legacySignalId);
                
                // Remove the mapping
                _signalMapping.Remove(signalId);
            }
        }
        
        // Handle enhanced signal strength changed
        private void OnSignalStrengthChanged(string signalId, float strength)
        {
            // Check if we have a mapping for this signal
            if (_signalMapping.TryGetValue(signalId, out string legacySignalId))
            {
                // Get the legacy signal
                var legacySignal = _radioSystem.GetSignal(legacySignalId);
                if (legacySignal != null)
                {
                    // Update the legacy signal strength
                    legacySignal.Strength = strength;
                }
            }
            
            // Update audio based on signal strength
            UpdateAudio(signalId, strength);
        }
        
        // Handle legacy signal detected
        private void OnLegacySignalDetected(string signalId, float frequency)
        {
            // This is handled by the enhanced system
        }
        
        // Handle legacy signal lost
        private void OnLegacySignalLost(string signalId)
        {
            // This is handled by the enhanced system
        }
        
        // Handle frequency changed
        private void OnFrequencyChanged(float frequency)
        {
            // Update the legacy system frequency
            _radioSystem.SetFrequency(frequency);
        }
        
        // Handle radio toggled
        private void OnRadioToggled(bool isOn)
        {
            // Update the legacy system radio power
            _radioSystem.SetRadioPower(isOn);
        }
        
        // Update audio based on signal strength
        private void UpdateAudio(string signalId, float strength)
        {
            if (_audioManager == null || !_gameState.IsRadioOn)
                return;
                
            var enhancedSignal = _radioSignalsManager.GetSignal(signalId);
            if (enhancedSignal == null)
                return;
                
            // Calculate static intensity (inverse of signal strength)
            float staticIntensity = 1.0f - strength;
            
            if (strength > 0.1f)
            {
                // Play signal with appropriate volume and type
                string waveType = "sine"; // Default wave type
                
                // Adjust wave type based on signal type
                switch (enhancedSignal.Type)
                {
                    case SignalType.Morse:
                        waveType = "square";
                        break;
                    case SignalType.Data:
                        waveType = "sawtooth";
                        break;
                    case SignalType.Voice:
                        waveType = "sine";
                        break;
                }
                
                // Play signal
                _audioManager.PlaySignal(enhancedSignal.Frequency * 10, strength, waveType, strength > 0.7f);
                
                // Play static noise
                _audioManager.PlayStaticNoise(staticIntensity);
            }
            else
            {
                // Just play static
                _audioManager.StopSignal();
                _audioManager.PlayStaticNoise(1.0f);
            }
        }
    }
}

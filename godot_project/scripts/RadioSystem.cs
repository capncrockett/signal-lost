using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class RadioSystem : Node
    {
        // Signal definitions
        [Signal]
        public delegate void SignalAddedEventHandler(string signalId);

        [Signal]
        public delegate void SignalRemovedEventHandler(string signalId);

        [Signal]
        public delegate void SignalDetectedEventHandler(string signalId, float frequency);

        [Signal]
        public delegate void SignalLostEventHandler(string signalId);

        // Radio state
        private float _currentFrequency = 90.0f;
        private bool _isRadioOn = false;
        private float _signalStrength = 0.0f;
        private float _staticIntensity = 1.0f;
        private bool _isMuted = false;

        // Radio configuration
        public float MinFrequency { get; set; } = 88.0f;
        public float MaxFrequency { get; set; } = 108.0f;
        public float FrequencyStep { get; set; } = 0.1f;
        public float SignalDetectionThreshold { get; set; } = 0.2f;

        // Signal database
        private Dictionary<string, RadioSignal> _signals = new Dictionary<string, RadioSignal>();
        private string _currentSignalId = null;

        // References to other systems
        private GameState _gameState;
        private AudioManager _audioManager;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to other systems
            _gameState = GetNode<GameState>("/root/GameState");
            _audioManager = GetNode<AudioManager>("/root/AudioManager");

            if (_gameState == null || _audioManager == null)
            {
                GD.PrintErr("RadioSystem: Failed to get references to other systems");
                return;
            }

            // Initialize with some test signals
            InitializeTestSignals();
        }

        // Initialize with some test signals
        private void InitializeTestSignals()
        {
            // Add some test signals
            AddSignal(new RadioSignal
            {
                Id = "signal_emergency",
                Frequency = 91.5f,
                Message = "SOS SOS SOS",
                Type = RadioSignalType.Morse,
                Strength = 0.8f
            });

            AddSignal(new RadioSignal
            {
                Id = "signal_test",
                Frequency = 95.7f,
                Message = "TEST TEST TEST",
                Type = RadioSignalType.Morse,
                Strength = 0.6f
            });

            AddSignal(new RadioSignal
            {
                Id = "signal_beacon",
                Frequency = 103.2f,
                Message = "BEACON ACTIVE",
                Type = RadioSignalType.Morse,
                Strength = 0.9f
            });
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            if (_isRadioOn)
            {
                // Process current frequency
                ProcessFrequency();
            }
        }

        // Process the current frequency
        private void ProcessFrequency()
        {
            // Find the strongest signal at the current frequency
            var signalData = FindSignalAtFrequency(_currentFrequency);

            if (signalData != null)
            {
                // Calculate signal strength based on how close we are to the exact frequency
                float frequencyDifference = Math.Abs(signalData.Frequency - _currentFrequency);
                float maxDifference = 0.5f; // Maximum difference to detect a signal
                float normalizedDifference = Mathf.Clamp(frequencyDifference / maxDifference, 0.0f, 1.0f);
                float strength = signalData.Strength * (1.0f - normalizedDifference);

                // Apply threshold
                if (strength < SignalDetectionThreshold)
                {
                    strength = 0.0f;
                }

                // Update state
                _signalStrength = strength;
                _staticIntensity = 1.0f - strength;
                _currentSignalId = strength > 0 ? signalData.Id : null;

                // Emit signal detected/lost events
                if (strength > 0 && _currentSignalId != null)
                {
                    EmitSignal(SignalName.SignalDetected, _currentSignalId, _currentFrequency);
                }
                else if (_currentSignalId != null)
                {
                    EmitSignal(SignalName.SignalLost, _currentSignalId);
                    _currentSignalId = null;
                }

                // Play audio
                if (_audioManager != null)
                {
                    if (strength > 0)
                    {
                        // Play signal with appropriate volume
                        _audioManager.PlaySignal(signalData.Frequency * 10, strength, "sine", true);
                        _audioManager.PlayStaticNoise(_staticIntensity);
                    }
                    else
                    {
                        // Just play static
                        _audioManager.StopSignal();
                        _audioManager.PlayStaticNoise(_staticIntensity);
                    }
                }
            }
            else
            {
                // No signal found
                _signalStrength = 0.0f;
                _staticIntensity = 1.0f;

                if (_currentSignalId != null)
                {
                    EmitSignal(SignalName.SignalLost, _currentSignalId);
                    _currentSignalId = null;
                }

                // Play audio
                if (_audioManager != null)
                {
                    _audioManager.StopSignal();
                    _audioManager.PlayStaticNoise(_staticIntensity);
                }
            }
        }

        // Find a signal at the given frequency
        private RadioSignal FindSignalAtFrequency(float frequency)
        {
            RadioSignal bestSignal = null;
            float closestDistance = float.MaxValue;

            foreach (var signal in _signals.Values)
            {
                float distance = Math.Abs(signal.Frequency - frequency);
                if (distance < closestDistance && distance < 0.5f)
                {
                    closestDistance = distance;
                    bestSignal = signal;
                }
            }

            return bestSignal;
        }

        // Add a signal to the system
        public void AddSignal(RadioSignal signal)
        {
            if (!_signals.ContainsKey(signal.Id))
            {
                _signals[signal.Id] = signal;
                EmitSignal(SignalName.SignalAdded, signal.Id);
            }
        }

        // Remove a signal from the system
        public void RemoveSignal(string signalId)
        {
            if (_signals.ContainsKey(signalId))
            {
                _signals.Remove(signalId);
                EmitSignal(SignalName.SignalRemoved, signalId);
            }
        }

        // Get a signal by ID
        public RadioSignal GetSignal(string signalId)
        {
            return _signals.ContainsKey(signalId) ? _signals[signalId] : null;
        }

        // Get all signals
        public Dictionary<string, RadioSignal> GetSignals()
        {
            return _signals;
        }

        // Set the current frequency
        public void SetFrequency(float frequency)
        {
            _currentFrequency = Mathf.Clamp(frequency, MinFrequency, MaxFrequency);
            _currentFrequency = Mathf.Snapped(_currentFrequency, FrequencyStep);
        }

        // Get the current frequency
        public float GetFrequency()
        {
            return _currentFrequency;
        }

        // Toggle the radio power
        public void ToggleRadio()
        {
            _isRadioOn = !_isRadioOn;

            if (!_isRadioOn)
            {
                // Stop all audio when radio is turned off
                if (_audioManager != null)
                {
                    _audioManager.StopSignal();
                    _audioManager.StopStaticNoise();
                }
            }
            else
            {
                // Start with static noise when turning on
                if (_audioManager != null)
                {
                    _audioManager.PlayStaticNoise(1.0f);
                }
            }
        }

        // Set the radio power state
        public void SetRadioPower(bool isOn)
        {
            if (_isRadioOn != isOn)
            {
                ToggleRadio();
            }
        }

        // Get the radio power state
        public bool IsRadioOn()
        {
            return _isRadioOn;
        }

        // Get the current signal strength
        public float GetSignalStrength()
        {
            return _signalStrength;
        }

        // Get the current static intensity
        public float GetStaticIntensity()
        {
            return _staticIntensity;
        }

        // Set mute state
        public void SetMute(bool isMuted)
        {
            _isMuted = isMuted;
            if (_audioManager != null)
            {
                _audioManager.SetMuted(isMuted);
            }
        }

        // Get mute state
        public bool IsMuted()
        {
            return _isMuted;
        }
    }
}

using Godot;
using System;
using System.Collections.Generic;
using SignalLost.Radio;

namespace SignalLost
{
    /// <summary>
    /// Controller for the enhanced radio signals demo scene.
    /// </summary>
    [GlobalClass]
    public partial class EnhancedRadioDemoController : Control
    {
        // References to UI elements
        private Control _radioInterface;
        private Label _messageDisplay;
        private Label _narrativeInfoLabel;

        // References to managers
        private RadioNarrativeManager _narrativeManager;
        private Dictionary<float, EnhancedSignalData> _demoSignals = new Dictionary<float, EnhancedSignalData>();

        // Current state
        private float _currentFrequency = 91.5f;
        private bool _radioPowered = true;
        private string _currentSignalId = "";
        private float _currentSignalStrength = 0.0f;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _radioInterface = GetNode<Control>("RadioInterface");
            _messageDisplay = GetNode<Label>("MessageDisplay");
            _narrativeInfoLabel = GetNode<Label>("NarrativeInfo");
            _narrativeManager = GetNode<RadioNarrativeManager>("RadioNarrativeManager");

            // Connect signals if needed
            if (_narrativeManager != null)
            {
                _narrativeManager.NarrativeThreadDiscovered += OnNarrativeThreadDiscovered;
                _narrativeManager.NarrativeSignalDecoded += OnNarrativeSignalDecoded;
            }

            // Create demo signals
            CreateDemoSignals();

            // Update UI
            UpdateUI();
        }

        // Called every frame
        public override void _Process(double delta)
        {
            // Simulate signal strength changes
            if (_radioPowered)
            {
                // Check for signals at current frequency
                CheckForSignalsAtFrequency(_currentFrequency);
            }
        }

        // Create demo signals for testing
        private void CreateDemoSignals()
        {
            // Create some demo signals
            CreateDemoSignal(91.5f, "signal_emergency", "Emergency Broadcast", "This is an emergency broadcast. Please remain calm and follow instructions.", 1.0f);
            CreateDemoSignal(95.7f, "signal_weather", "Weather Report", "Expect severe weather conditions in the following areas...", 0.8f);
            CreateDemoSignal(103.2f, "signal_news", "News Bulletin", "Breaking news: Strange phenomena reported in multiple locations.", 0.9f);
            CreateDemoSignal(107.9f, "signal_music", "Music Station", "Now playing: 'Lost in the Static' by The Frequencies", 0.7f);
            CreateDemoSignal(88.3f, "signal_interference", "Unknown Signal", "...bzzt...cannot...bzzt...understand...bzzt...", 0.5f);
        }

        // Create a demo signal
        private void CreateDemoSignal(float frequency, string id, string name, string content, float strength)
        {
            var signal = new EnhancedSignalData
            {
                Id = id,
                Name = name,
                Content = content,
                Frequency = frequency,
                MinSignalStrength = strength,
                Type = SignalType.Voice
            };

            _demoSignals[frequency] = signal;
        }

        // Check for signals at the current frequency
        private void CheckForSignalsAtFrequency(float frequency)
        {
            // Reset current signal
            _currentSignalId = "";
            _currentSignalStrength = 0.0f;

            // Check for signals within range
            foreach (var signal in _demoSignals.Values)
            {
                float distance = Math.Abs(frequency - signal.Frequency);
                if (distance <= 0.3f)
                {
                    // Calculate signal strength based on distance
                    float strength = 1.0f - (distance / 0.3f);
                    strength = Mathf.Clamp(strength, 0.0f, 1.0f) * signal.MinSignalStrength;

                    // If this is the strongest signal so far, use it
                    if (strength > _currentSignalStrength)
                    {
                        _currentSignalId = signal.Id;
                        _currentSignalStrength = strength;
                    }
                }
            }

            // Update UI
            UpdateUI();
        }

        // Update the UI based on current state
        private void UpdateUI()
        {
            // Update UI elements if needed
            if (_messageDisplay != null && !string.IsNullOrEmpty(_currentSignalId))
            {
                EnhancedSignalData signal = null;
                foreach (var s in _demoSignals.Values)
                {
                    if (s.Id == _currentSignalId)
                    {
                        signal = s;
                        break;
                    }
                }

                if (signal != null && _currentSignalStrength > 0.5f)
                {
                    _messageDisplay.Text = $"Signal: {signal.Name} ({_currentSignalStrength:F2})";
                }
                else
                {
                    _messageDisplay.Text = "No signal detected";
                }
            }
        }

        // Called when a narrative thread is discovered
        private void OnNarrativeThreadDiscovered(string threadId)
        {
            if (_narrativeInfoLabel != null)
            {
                _narrativeInfoLabel.Text = $"Narrative Thread: {threadId}";
                GD.Print($"Narrative thread discovered: {threadId}");
            }
        }

        // Called when a narrative signal is decoded
        private void OnNarrativeSignalDecoded(string signalId)
        {
            GD.Print($"Narrative signal decoded: {signalId}");
        }
    }
}

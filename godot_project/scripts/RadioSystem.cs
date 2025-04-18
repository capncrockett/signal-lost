using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class RadioSystem : Node
    {
        // Radio signal properties
        public class RadioSignal
        {
            public string Id { get; set; }
            public float Frequency { get; set; }
            public string Message { get; set; }
            public RadioSignalType Type { get; set; }
            public float Strength { get; set; }
        }
        
        public enum RadioSignalType
        {
            Voice,
            Morse,
            Digital,
            Music,
            Static
        }
        
        // Radio state
        private Dictionary<string, RadioSignal> _signals = new Dictionary<string, RadioSignal>();
        private float _currentFrequency = 90.0f;
        private bool _isOn = false;
        
        // Signals
        [Signal] public delegate void SignalAddedEventHandler(string signalId);
        [Signal] public delegate void SignalRemovedEventHandler(string signalId);
        [Signal] public delegate void FrequencyChangedEventHandler(float frequency);
        [Signal] public delegate void RadioToggledEventHandler(bool isOn);
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("RadioSystem: Initializing...");
        }
        
        // Add a radio signal
        public void AddSignal(RadioSignal signal)
        {
            _signals[signal.Id] = signal;
            EmitSignal(SignalName.SignalAdded, signal.Id);
        }
        
        // Remove a radio signal
        public void RemoveSignal(string signalId)
        {
            if (_signals.ContainsKey(signalId))
            {
                _signals.Remove(signalId);
                EmitSignal(SignalName.SignalRemoved, signalId);
            }
        }
        
        // Get a radio signal by ID
        public RadioSignal GetSignal(string signalId)
        {
            return _signals.ContainsKey(signalId) ? _signals[signalId] : null;
        }
        
        // Get all radio signals
        public Dictionary<string, RadioSignal> GetSignals()
        {
            return _signals;
        }
        
        // Set the current frequency
        public void SetFrequency(float frequency)
        {
            _currentFrequency = Mathf.Clamp(frequency, 88.0f, 108.0f);
            EmitSignal(SignalName.FrequencyChanged, _currentFrequency);
        }
        
        // Get the current frequency
        public float GetFrequency()
        {
            return _currentFrequency;
        }
        
        // Turn the radio on or off
        public void SetRadioOn(bool isOn)
        {
            _isOn = isOn;
            EmitSignal(SignalName.RadioToggled, _isOn);
        }
        
        // Toggle the radio on/off state
        public void ToggleRadio()
        {
            SetRadioOn(!_isOn);
        }
        
        // Check if the radio is on
        public bool IsRadioOn()
        {
            return _isOn;
        }
        
        // Find signals at the current frequency
        public List<RadioSignal> GetSignalsAtCurrentFrequency(float bandwidth = 0.5f)
        {
            return GetSignalsAtFrequency(_currentFrequency, bandwidth);
        }
        
        // Find signals at a specific frequency
        public List<RadioSignal> GetSignalsAtFrequency(float frequency, float bandwidth = 0.5f)
        {
            var result = new List<RadioSignal>();
            
            foreach (var signal in _signals.Values)
            {
                float distance = Math.Abs(frequency - signal.Frequency);
                if (distance <= bandwidth)
                {
                    result.Add(signal);
                }
            }
            
            return result;
        }
    }
}

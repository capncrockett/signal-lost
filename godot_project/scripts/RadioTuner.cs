using Godot;
using System;

namespace SignalLost
{
    public partial class RadioTuner : Control
    {
        // Radio tuner properties
        [Export]
        public float MinFrequency { get; set; } = 88.0f;
        
        [Export]
        public float MaxFrequency { get; set; } = 108.0f;
        
        [Export]
        public float FrequencyStep { get; set; } = 0.1f;

        // UI references (will be set in _Ready)
        private Label _frequencyDisplay;
        private Button _powerButton;
        private Slider _frequencySlider;
        private ProgressBar _signalStrengthMeter;
        private Control _staticVisualization;
        private Control _messageContainer;
        private Button _messageButton;
        private Control _messageDisplay;
        private Button _scanButton;
        private Button _tuneDownButton;
        private Button _tuneUpButton;

        // Local state
        private bool _isScanning = false;
        private bool _showMessage = false;
        private string _currentSignalId = null;
        private float _signalStrength = 0.0f;
        private float _staticIntensity = 0.5f;
        private Timer _scanTimer = null;

        // References to singletons
        private GameState _gameState;
        private AudioManager _audioManager;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to singletons
            _gameState = GetNode<GameState>("/root/GameState");
            _audioManager = GetNode<AudioManager>("/root/AudioManager");

            // Get UI references
            _frequencyDisplay = GetNode<Label>("FrequencyDisplay");
            _powerButton = GetNode<Button>("PowerButton");
            _frequencySlider = GetNode<Slider>("FrequencySlider");
            _signalStrengthMeter = GetNode<ProgressBar>("SignalStrengthMeter");
            _staticVisualization = GetNode<Control>("StaticVisualization");
            _messageContainer = GetNode<Control>("MessageContainer");
            _messageButton = GetNode<Button>("MessageContainer/MessageButton");
            _messageDisplay = GetNode<Control>("MessageContainer/MessageDisplay");
            _scanButton = GetNode<Button>("ScanButton");
            _tuneDownButton = GetNode<Button>("TuneDownButton");
            _tuneUpButton = GetNode<Button>("TuneUpButton");

            // Initialize UI
            UpdateUi();
            
            // Connect signals
            _powerButton.Pressed += OnPowerButtonPressed;
            _frequencySlider.ValueChanged += OnFrequencySliderChanged;
            _messageButton.Pressed += OnMessageButtonPressed;
            _scanButton.Pressed += OnScanButtonPressed;
            _tuneDownButton.Pressed += OnTuneDownButtonPressed;
            _tuneUpButton.Pressed += OnTuneUpButtonPressed;
            
            // Connect to GameState signals
            _gameState.FrequencyChanged += OnFrequencyChanged;
            _gameState.RadioToggled += OnRadioToggled;
            
            // Create scan timer
            _scanTimer = new Timer();
            _scanTimer.WaitTime = 0.3f;  // Scan speed in seconds
            _scanTimer.OneShot = false;
            _scanTimer.Timeout += OnScanTimerTimeout;
            AddChild(_scanTimer);
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            if (_gameState.IsRadioOn)
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
                else if (Input.IsActionJustPressed("toggle_power"))
                {
                    TogglePower();
                }
                
                // Process frequency and update audio/visuals
                ProcessFrequency();
                UpdateStaticVisualization((float)delta);
            }
        }

        // Process the current frequency
        private void ProcessFrequency()
        {
            var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);
            
            if (signalData != null)
            {
                // Calculate signal strength based on how close we are to the exact frequency
                _signalStrength = _gameState.CalculateSignalStrength(_gameState.CurrentFrequency, signalData);
                
                // Calculate static intensity based on signal strength
                _staticIntensity = signalData.IsStatic ? 1.0f - _signalStrength : (1.0f - _signalStrength) * 0.5f;
                
                // Update UI
                _signalStrengthMeter.Value = _signalStrength * 100;
                _currentSignalId = signalData.MessageId;
                
                // If this is a new signal discovery, add it to discovered frequencies
                if (!_gameState.DiscoveredFrequencies.Contains(signalData.Frequency))
                {
                    _gameState.AddDiscoveredFrequency(signalData.Frequency);
                }
                
                // Play appropriate audio
                if (signalData.IsStatic)
                {
                    // Play static with the signal mixed in
                    _audioManager.PlayStaticNoise(_staticIntensity);
                    _audioManager.PlaySignal(signalData.Frequency * 10, _signalStrength * 0.5f);  // Scale up for audible range
                }
                else
                {
                    // Play a clear signal
                    _audioManager.StopStaticNoise();
                    _audioManager.PlaySignal(signalData.Frequency * 10);  // Scale up for audible range
                }
            }
            else
            {
                // No signal found, just play static
                float intensity = _gameState.GetStaticIntensity(_gameState.CurrentFrequency);
                
                // Update state
                _staticIntensity = intensity;
                _signalStrength = 0.1f;  // Low signal strength
                _currentSignalId = null;
                
                // Update UI
                _signalStrengthMeter.Value = _signalStrength * 100;
                
                // Play audio
                _audioManager.StopSignal();
                _audioManager.PlayStaticNoise(intensity);
            }
            
            // Update message button state
            UpdateMessageButton();
        }

        // Update the static visualization
        private void UpdateStaticVisualization(float delta)
        {
            // This would normally update a shader or material
            // For now, we'll just update a property
            var modulate = _staticVisualization.Modulate;
            modulate.A = _staticIntensity;
            _staticVisualization.Modulate = modulate;
        }

        // Change the frequency by a specific amount
        private void ChangeFrequency(float amount)
        {
            float newFreq = _gameState.CurrentFrequency + amount;
            newFreq = Mathf.Clamp(newFreq, MinFrequency, MaxFrequency);
            newFreq = Mathf.Snapped(newFreq, FrequencyStep);  // Round to nearest step
            
            _gameState.SetFrequency(newFreq);
        }

        // Toggle the radio power
        private void TogglePower()
        {
            _gameState.ToggleRadio();
        }

        // Toggle frequency scanning
        private void ToggleScanning()
        {
            _isScanning = !_isScanning;
            
            if (_isScanning && _gameState.IsRadioOn)
            {
                _scanTimer.Start();
            }
            else
            {
                _scanTimer.Stop();
            }
            
            // Update UI
            _scanButton.Text = _isScanning ? "Stop Scan" : "Scan";
        }

        // Toggle message display
        private void ToggleMessage()
        {
            _showMessage = !_showMessage;
            
            // Update UI
            _messageDisplay.Visible = _showMessage;
            _messageButton.Text = _showMessage ? "Hide Message" : "Show Message";
            
            if (_showMessage && _currentSignalId != null)
            {
                var message = _gameState.GetMessage(_currentSignalId);
                if (message != null)
                {
                    // Assuming MessageDisplay has a SetMessage method
                    // _messageDisplay.Call("set_message", message);
                    // In a real implementation, you'd have a proper MessageDisplay class
                }
            }
        }

        // Update the UI based on current state
        private void UpdateUi()
        {
            // Update frequency display
            _frequencyDisplay.Text = $"{_gameState.CurrentFrequency:F1} MHz";
            
            // Update power button
            _powerButton.Text = _gameState.IsRadioOn ? "ON" : "OFF";
            
            // Update frequency slider
            float percentage = (_gameState.CurrentFrequency - MinFrequency) / (MaxFrequency - MinFrequency);
            _frequencySlider.Value = percentage * 100;
            
            // Update message button
            UpdateMessageButton();
            
            // Update scan button
            _scanButton.Text = _isScanning ? "Stop Scan" : "Scan";
            
            // Update tune buttons
            _tuneDownButton.Disabled = !_gameState.IsRadioOn || _isScanning;
            _tuneUpButton.Disabled = !_gameState.IsRadioOn || _isScanning;
        }

        // Update the message button state
        private void UpdateMessageButton()
        {
            bool hasMessage = _currentSignalId != null && _gameState.GetMessage(_currentSignalId) != null;
            _messageButton.Disabled = !_gameState.IsRadioOn || !hasMessage;
            _messageContainer.Visible = hasMessage;
        }

        // Signal handlers
        private void OnPowerButtonPressed()
        {
            TogglePower();
        }

        private void OnFrequencySliderChanged(double value)
        {
            float freq = MinFrequency + ((float)value / 100.0f) * (MaxFrequency - MinFrequency);
            freq = Mathf.Snapped(freq, FrequencyStep);  // Round to nearest step
            _gameState.SetFrequency(freq);
        }

        private void OnMessageButtonPressed()
        {
            ToggleMessage();
        }

        private void OnTuneDownButtonPressed()
        {
            ChangeFrequency(-FrequencyStep);
        }

        private void OnTuneUpButtonPressed()
        {
            ChangeFrequency(FrequencyStep);
        }

        private void OnScanButtonPressed()
        {
            ToggleScanning();
        }

        private void OnScanTimerTimeout()
        {
            if (_isScanning && _gameState.IsRadioOn)
            {
                // Increment frequency by step
                float newFreq = _gameState.CurrentFrequency + FrequencyStep;
                
                // If we reach the max frequency, loop back to min
                if (newFreq > MaxFrequency)
                {
                    newFreq = MinFrequency;
                }
                
                _gameState.SetFrequency(newFreq);
            }
        }

        private void OnFrequencyChanged(float newFrequency)
        {
            UpdateUi();
        }

        private void OnRadioToggled(bool isOn)
        {
            if (!isOn)
            {
                // Stop all audio when radio is turned off
                _audioManager.StopSignal();
                _audioManager.StopStaticNoise();
                
                // Stop scanning if active
                if (_isScanning)
                {
                    ToggleScanning();
                }
            }
            
            UpdateUi();
        }
    }
}

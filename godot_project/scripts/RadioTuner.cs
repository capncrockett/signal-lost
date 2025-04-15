using Godot;

namespace SignalLost
{
    [GlobalClass]
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
            try
            {
                // Get references to singletons
                _gameState = GetNode<GameState>("/root/GameState");
                if (_gameState == null)
                {
                    // Try to find GameState in the parent hierarchy
                    _gameState = GetParent<GameState>();
                    if (_gameState == null)
                    {
                        // Try to find GameState in the scene tree
                        foreach (var node in GetTree().GetNodesInGroup("GameState"))
                        {
                            if (node is GameState gameState)
                            {
                                _gameState = gameState;
                                break;
                            }
                        }

                        // If still null, try to find it as a sibling
                        if (_gameState == null && GetParent() != null)
                        {
                            foreach (var child in GetParent().GetChildren())
                            {
                                if (child is GameState gameState)
                                {
                                    _gameState = gameState;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Get AudioManager reference
                _audioManager = GetNode<AudioManager>("/root/AudioManager");
                if (_audioManager == null)
                {
                    // Try to find AudioManager in the parent hierarchy
                    _audioManager = GetParent<AudioManager>();
                    if (_audioManager == null)
                    {
                        // Try to find AudioManager in the scene tree
                        foreach (var node in GetTree().GetNodesInGroup("AudioManager"))
                        {
                            if (node is AudioManager audioManager)
                            {
                                _audioManager = audioManager;
                                break;
                            }
                        }

                        // If still null, try to find it as a sibling
                        if (_audioManager == null && GetParent() != null)
                        {
                            foreach (var child in GetParent().GetChildren())
                            {
                                if (child is AudioManager audioManager)
                                {
                                    _audioManager = audioManager;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Get UI references
                _frequencyDisplay = GetNodeOrNull<Label>("FrequencyDisplay");
                _powerButton = GetNodeOrNull<Button>("PowerButton");
                _frequencySlider = GetNodeOrNull<Slider>("FrequencySlider");
                _signalStrengthMeter = GetNodeOrNull<ProgressBar>("SignalStrengthMeter");
                _staticVisualization = GetNodeOrNull<Control>("StaticVisualization");
                _messageContainer = GetNodeOrNull<Control>("MessageContainer");
                _messageButton = _messageContainer != null ? _messageContainer.GetNodeOrNull<Button>("MessageButton") : null;
                _messageDisplay = _messageContainer != null ? _messageContainer.GetNodeOrNull<Control>("MessageDisplay") : null;
                _scanButton = GetNodeOrNull<Button>("ScanButton");
                _tuneDownButton = GetNodeOrNull<Button>("TuneDownButton");
                _tuneUpButton = GetNodeOrNull<Button>("TuneUpButton");

                // Initialize UI
                UpdateUi();

                // Connect signals if UI elements exist
                if (_powerButton != null) _powerButton.Pressed += OnPowerButtonPressed;
                if (_frequencySlider != null) _frequencySlider.ValueChanged += OnFrequencySliderChanged;
                if (_messageButton != null) _messageButton.Pressed += OnMessageButtonPressed;
                if (_scanButton != null) _scanButton.Pressed += OnScanButtonPressed;
                if (_tuneDownButton != null) _tuneDownButton.Pressed += OnTuneDownButtonPressed;
                if (_tuneUpButton != null) _tuneUpButton.Pressed += OnTuneUpButtonPressed;

                // Connect to GameState signals if GameState exists
                if (_gameState != null)
                {
                    _gameState.FrequencyChanged += OnFrequencyChanged;
                    _gameState.RadioToggled += OnRadioToggled;
                }

                // Create scan timer
                _scanTimer = new Timer();
                _scanTimer.WaitTime = 0.3f;  // Scan speed in seconds
                _scanTimer.OneShot = false;
                _scanTimer.Timeout += OnScanTimerTimeout;
                AddChild(_scanTimer);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in RadioTuner._Ready: {ex.Message}");
            }
        }

        // Helper method to safely get a node or return null if not found
        private T GetNodeOrNull<T>(string path) where T : class
        {
            try
            {
                return GetNode<T>(path);
            }
            catch
            {
                return null;
            }
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            // Skip processing if GameState is null (for tests)
            if (_gameState == null) return;

            // Process input regardless of radio state
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

            // Only process frequency and update visuals if radio is on
            if (_gameState.IsRadioOn)
            {
                // Process frequency and update audio/visuals
                ProcessFrequency();
                UpdateStaticVisualization((float)delta);
            }
        }

        // Process the current frequency
        private void ProcessFrequency()
        {
            if (_gameState == null || _audioManager == null) return;

            var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);

            if (signalData != null)
            {
                // Calculate signal strength based on how close we are to the exact frequency
                _signalStrength = _gameState.CalculateSignalStrength(_gameState.CurrentFrequency, signalData);

                // Calculate static intensity based on signal strength
                _staticIntensity = signalData.IsStatic ? 1.0f - _signalStrength : (1.0f - _signalStrength) * 0.5f;

                // Update UI
                if (_signalStrengthMeter != null)
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
                if (_signalStrengthMeter != null)
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
        public void ChangeFrequency(float amount)
        {
            if (_gameState == null) return;

            float newFreq = _gameState.CurrentFrequency + amount;
            newFreq = Mathf.Clamp(newFreq, MinFrequency, MaxFrequency);
            newFreq = Mathf.Snapped(newFreq, FrequencyStep);  // Round to nearest step

            _gameState.SetFrequency(newFreq);

            // For testing purposes, manually call OnFrequencyChanged
            OnFrequencyChanged(newFreq);
        }

        // Toggle the radio power
        public void TogglePower()
        {
            if (_gameState == null) return;

            _gameState.ToggleRadio();

            // For testing purposes, manually call OnRadioToggled
            OnRadioToggled(_gameState.IsRadioOn);
        }

        // Toggle frequency scanning
        public void ToggleScanning()
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
        public void ToggleMessage()
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
            // Only update UI elements if they exist and GameState exists
            if (_gameState == null) return;

            // Update frequency display
            if (_frequencyDisplay != null)
                _frequencyDisplay.Text = $"{_gameState.CurrentFrequency:F1} MHz";

            // Update power button
            if (_powerButton != null)
                _powerButton.Text = _gameState.IsRadioOn ? "ON" : "OFF";

            // Update frequency slider
            if (_frequencySlider != null)
            {
                float percentage = (_gameState.CurrentFrequency - MinFrequency) / (MaxFrequency - MinFrequency);
                _frequencySlider.Value = percentage * 100;
            }

            // Update message button
            UpdateMessageButton();

            // Update scan button
            if (_scanButton != null)
                _scanButton.Text = _isScanning ? "Stop Scan" : "Scan";

            // Update tune buttons
            if (_tuneDownButton != null)
                _tuneDownButton.Disabled = !_gameState.IsRadioOn || _isScanning;
            if (_tuneUpButton != null)
                _tuneUpButton.Disabled = !_gameState.IsRadioOn || _isScanning;
        }

        // Update the message button state
        private void UpdateMessageButton()
        {
            if (_messageButton == null || _messageContainer == null || _gameState == null) return;

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

        public void OnRadioToggled(bool isOn)
        {
            if (!isOn)
            {
                // Stop all audio when radio is turned off
                if (_audioManager != null)
                {
                    _audioManager.StopSignal();
                    _audioManager.StopStaticNoise();
                }

                // Stop scanning if active
                if (_isScanning)
                {
                    ToggleScanning();
                }
            }
            else
            {
                // Process frequency to start playing appropriate audio
                ProcessFrequency();
            }

            UpdateUi();
        }
    }
}

using Godot;
using System;

namespace SignalLost.Radio
{
    /// <summary>
    /// UI component to display radio signals and their content.
    /// </summary>
    [GlobalClass]
    public partial class RadioSignalDisplay : Control
    {
        // Reference to the radio signals manager
        private RadioSignalsManager _radioSignalsManager;
        
        // Reference to the game state
        private GameState _gameState;
        
        // UI elements
        [Export] private Label _signalNameLabel;
        [Export] private Label _signalTypeLabel;
        [Export] private Label _signalStrengthLabel;
        [Export] private RichTextLabel _signalContentLabel;
        [Export] private TextureRect _signalStrengthIndicator;
        [Export] private Panel _noSignalPanel;
        [Export] private AnimationPlayer _animationPlayer;
        
        // Currently displayed signal
        private EnhancedSignalData _currentSignal;
        
        // Signal strength of the current signal
        private float _currentSignalStrength = 0.0f;
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to systems
            _radioSignalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");
            _gameState = GetNode<GameState>("/root/GameState");
            
            if (_radioSignalsManager == null)
            {
                GD.PrintErr("RadioSignalDisplay: RadioSignalsManager not found");
                return;
            }
            
            if (_gameState == null)
            {
                GD.PrintErr("RadioSignalDisplay: GameState not found");
                return;
            }
            
            // Connect signals
            _radioSignalsManager.SignalDiscovered += OnSignalDiscovered;
            _radioSignalsManager.SignalLost += OnSignalLost;
            _radioSignalsManager.SignalStrengthChanged += OnSignalStrengthChanged;
            _radioSignalsManager.SignalDecoded += OnSignalDecoded;
            _gameState.FrequencyChanged += OnFrequencyChanged;
            _gameState.RadioToggled += OnRadioToggled;
            
            // Initialize UI
            UpdateUI();
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
                _radioSignalsManager.SignalDecoded -= OnSignalDecoded;
            }
            
            if (_gameState != null)
            {
                _gameState.FrequencyChanged -= OnFrequencyChanged;
                _gameState.RadioToggled -= OnRadioToggled;
            }
        }
        
        // Handle signal discovered
        private void OnSignalDiscovered(string signalId)
        {
            // Get the signal
            var signal = _radioSignalsManager.GetSignal(signalId);
            
            if (signal == null)
                return;
                
            // Check if this is the current signal
            if (_currentSignal != null && _currentSignal.Id == signalId)
            {
                // Update the UI
                UpdateUI();
            }
        }
        
        // Handle signal lost
        private void OnSignalLost(string signalId)
        {
            // Check if this is the current signal
            if (_currentSignal != null && _currentSignal.Id == signalId)
            {
                // Clear the display
                _currentSignal = null;
                _currentSignalStrength = 0.0f;
                UpdateUI();
            }
        }
        
        // Handle signal strength changed
        private void OnSignalStrengthChanged(string signalId, float strength)
        {
            // Get the signal
            var signal = _radioSignalsManager.GetSignal(signalId);
            
            if (signal == null)
                return;
                
            // Update the current signal
            _currentSignal = signal;
            _currentSignalStrength = strength;
            
            // Update the UI
            UpdateUI();
        }
        
        // Handle signal decoded
        private void OnSignalDecoded(string signalId)
        {
            // Check if this is the current signal
            if (_currentSignal != null && _currentSignal.Id == signalId)
            {
                // Update the UI
                UpdateUI();
            }
        }
        
        // Handle frequency changed
        private void OnFrequencyChanged(float frequency)
        {
            // The RadioSignalsManager will handle this and emit SignalStrengthChanged if needed
        }
        
        // Handle radio toggled
        private void OnRadioToggled(bool isOn)
        {
            if (!isOn)
            {
                // Radio turned off, clear the display
                _currentSignal = null;
                _currentSignalStrength = 0.0f;
                UpdateUI();
            }
        }
        
        // Update the UI
        private void UpdateUI()
        {
            if (_currentSignal != null && _gameState.IsRadioOn)
            {
                // Show signal info
                _signalNameLabel.Text = _currentSignal.Name;
                _signalTypeLabel.Text = $"Type: {_currentSignal.Type}";
                
                // Update signal strength
                int strengthPercent = (int)(_currentSignalStrength * 100);
                _signalStrengthLabel.Text = $"Strength: {strengthPercent}%";
                
                // Update signal content
                _signalContentLabel.Text = _currentSignal.GetFormattedContent(_currentSignalStrength);
                
                // Update signal strength indicator
                if (_signalStrengthIndicator != null)
                {
                    // This would update a texture or progress bar
                    // For now, we'll just set the modulate color based on strength
                    Color color = new Color(
                        1.0f,
                        _currentSignalStrength,
                        _currentSignalStrength,
                        1.0f
                    );
                    _signalStrengthIndicator.Modulate = color;
                }
                
                // Hide "No Signal" panel
                if (_noSignalPanel != null)
                {
                    _noSignalPanel.Visible = false;
                }
                
                // Play animation based on signal type
                PlaySignalAnimation(_currentSignal.Type);
            }
            else
            {
                // Show "No Signal" message
                _signalNameLabel.Text = "No Signal";
                _signalTypeLabel.Text = "Type: None";
                _signalStrengthLabel.Text = "Strength: 0%";
                _signalContentLabel.Text = "";
                
                // Show "No Signal" panel
                if (_noSignalPanel != null)
                {
                    _noSignalPanel.Visible = true;
                }
                
                // Stop any animations
                if (_animationPlayer != null && _animationPlayer.IsPlaying())
                {
                    _animationPlayer.Stop();
                }
            }
        }
        
        // Play animation based on signal type
        private void PlaySignalAnimation(SignalType type)
        {
            if (_animationPlayer == null)
                return;
                
            switch (type)
            {
                case SignalType.Voice:
                    if (_animationPlayer.HasAnimation("voice_signal"))
                    {
                        _animationPlayer.Play("voice_signal");
                    }
                    break;
                    
                case SignalType.Morse:
                    if (_animationPlayer.HasAnimation("morse_signal"))
                    {
                        _animationPlayer.Play("morse_signal");
                    }
                    break;
                    
                case SignalType.Data:
                    if (_animationPlayer.HasAnimation("data_signal"))
                    {
                        _animationPlayer.Play("data_signal");
                    }
                    break;
            }
        }
    }
}

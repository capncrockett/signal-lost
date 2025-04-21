using Godot;
using SignalLost.Radio;

namespace SignalLost
{
    /// <summary>
    /// Enhanced radio dial that works with the enhanced radio signals system.
    /// </summary>
    [GlobalClass]
    public partial class EnhancedRadioDial : Control
    {
        // References to systems
        private RadioSignalsManager _radioSignalsManager;
        private GameState _gameState;
        private AudioManager _audioManager;

        // UI elements
        [Export] private PixelRadioInterface _radioInterface;

        // Current signal
        private EnhancedSignalData _currentSignal;
        private float _currentSignalStrength;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to systems
            _radioSignalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");
            _gameState = GetNode<GameState>("/root/GameState");
            _audioManager = GetNode<AudioManager>("/root/AudioManager");

            if (_radioSignalsManager == null)
            {
                GD.PrintErr("EnhancedRadioDial: RadioSignalsManager not found");
                return;
            }

            if (_gameState == null)
            {
                GD.PrintErr("EnhancedRadioDial: GameState not found");
                return;
            }

            if (_audioManager == null)
            {
                GD.PrintErr("EnhancedRadioDial: AudioManager not found");
                return;
            }

            if (_radioInterface == null)
            {
                GD.PrintErr("EnhancedRadioDial: PixelRadioInterface not found");
                return;
            }

            // Connect signals
            _radioSignalsManager.SignalDiscovered += OnSignalDiscovered;
            _radioSignalsManager.SignalLost += OnSignalLost;
            _radioSignalsManager.SignalStrengthChanged += OnSignalStrengthChanged;
            _radioSignalsManager.SignalDecoded += OnSignalDecoded;

            _gameState.FrequencyChanged += OnFrequencyChanged;
            _gameState.RadioToggled += OnRadioToggled;

            // Initialize
            UpdateRadioInterface();
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
            // Update the radio interface
            UpdateRadioInterface();

            // Play discovery sound
            _audioManager?.PlayEffect("discovery");
        }

        // Handle signal lost
        private void OnSignalLost(string signalId)
        {
            // Update the radio interface
            UpdateRadioInterface();
        }

        // Handle signal strength changed
        private void OnSignalStrengthChanged(string signalId, float strength)
        {
            // Update current signal
            _currentSignal = _radioSignalsManager.GetCurrentSignal();
            _currentSignalStrength = strength;

            // Update the radio interface
            UpdateRadioInterface();
        }

        // Handle signal decoded
        private void OnSignalDecoded(string signalId)
        {
            // Update the radio interface
            UpdateRadioInterface();

            // Play decode sound
            _audioManager?.PlayEffect("decode");
        }

        // Handle frequency changed
        private void OnFrequencyChanged(float frequency)
        {
            // Update the radio interface
            UpdateRadioInterface();
        }

        // Handle radio toggled
        private void OnRadioToggled(bool isOn)
        {
            // Update the radio interface
            UpdateRadioInterface();

            // Play power sound
            if (isOn)
                _audioManager?.PlaySquelchOn();
            else
                _audioManager?.PlaySquelchOff();
        }

        // Update the radio interface
        private void UpdateRadioInterface()
        {
            if (_radioInterface == null)
                return;

            // Get the current signal strength
            float signalStrength = _currentSignalStrength;

            // Get the current signal
            _currentSignal = _radioSignalsManager.GetCurrentSignal();

            // Update the radio interface directly
            // Since we can't use our MockRadioSystem directly, we'll update the GameState
            // which the PixelRadioInterface uses

            // The PixelRadioInterface already uses GameState for frequency and radio power
            // We just need to make sure the RadioSystem has the correct signal strength
            var radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            if (radioSystem != null)
            {
                // Update the signal strength in the RadioSystem
                // This is a bit of a hack, but it works
                if (_currentSignal != null)
                {
                    radioSystem.UpdateSignalSourceStrength(_currentSignal.Frequency, signalStrength);
                }
            }

            // Force the PixelRadioInterface to update
            _radioInterface.QueueRedraw();
        }


    }
}

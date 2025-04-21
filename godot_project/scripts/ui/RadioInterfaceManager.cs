using Godot;
using System;
using SignalLost;

namespace SignalLost.UI
{
    /// <summary>
    /// Manages the integration between the PixelRadioInterface and the game's radio systems.
    /// </summary>
    [GlobalClass]
    public partial class RadioInterfaceManager : Node
    {
        // References to game systems
        private GameState _gameState;
        private RadioSystem _radioSystem;
        
        // Reference to the radio interface
        [Export] public NodePath RadioInterfacePath { get; set; }
        private PixelRadioInterface _radioInterface;
        
        // Signal handling
        private string _currentSignalId;
        private bool _messageAvailable = false;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to game systems
            _gameState = GetNode<GameState>("/root/GameState");
            _radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            
            if (_gameState == null)
            {
                GD.PrintErr("RadioInterfaceManager: GameState not found");
                return;
            }
            
            if (_radioSystem == null)
            {
                GD.PrintErr("RadioInterfaceManager: RadioSystem not found");
                return;
            }
            
            // Get reference to radio interface
            if (RadioInterfacePath != null && !RadioInterfacePath.IsEmpty)
            {
                _radioInterface = GetNode<PixelRadioInterface>(RadioInterfacePath);
            }
            else
            {
                // Try to find the radio interface as a child
                foreach (Node child in GetChildren())
                {
                    if (child is PixelRadioInterface radioInterface)
                    {
                        _radioInterface = radioInterface;
                        break;
                    }
                }
            }
            
            if (_radioInterface == null)
            {
                GD.PrintErr("RadioInterfaceManager: PixelRadioInterface not found");
                return;
            }
            
            // Connect signals from radio interface
            _radioInterface.FrequencyChanged += OnFrequencyChanged;
            _radioInterface.PowerToggle += OnPowerToggle;
            _radioInterface.ScanRequested += OnScanRequested;
            _radioInterface.MessageRequested += OnMessageRequested;
            
            // Connect signals from game systems
            _gameState.FrequencyChanged += OnGameStateFrequencyChanged;
            _gameState.RadioToggled += OnGameStateRadioToggled;
            _gameState.SignalDiscovered += OnSignalDiscovered;
            
            if (_radioSystem != null)
            {
                _radioSystem.SignalDetected += OnSignalDetected;
                _radioSystem.SignalLost += OnSignalLost;
            }
            
            // Initialize the radio interface with current game state
            SyncWithGameState();
        }
        
        // Sync the radio interface with the current game state
        private void SyncWithGameState()
        {
            if (_gameState == null || _radioInterface == null) return;
            
            // Set frequency
            _radioInterface.SetFrequency(_gameState.CurrentFrequency);
            
            // Set power state
            _radioInterface.IsPoweredOn = _gameState.IsRadioOn;
            
            // Set signal strength
            float signalStrength = 0.0f;
            if (_radioSystem != null)
            {
                signalStrength = _radioSystem.GetSignalStrength();
            }
            else
            {
                // Fallback to calculating signal strength from GameState
                var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);
                if (signalData != null)
                {
                    signalStrength = GameState.CalculateSignalStrength(_gameState.CurrentFrequency, signalData);
                }
            }
            _radioInterface.SetSignalStrength(signalStrength);
            
            // Check for message availability
            CheckMessageAvailability();
            
            // Redraw the interface
            _radioInterface.QueueRedraw();
        }
        
        // Process function called every frame
        public override void _Process(double delta)
        {
            // Update signal strength in real-time
            if (_gameState != null && _radioInterface != null && _gameState.IsRadioOn)
            {
                float signalStrength = 0.0f;
                if (_radioSystem != null)
                {
                    signalStrength = _radioSystem.GetSignalStrength();
                }
                else
                {
                    // Fallback to calculating signal strength from GameState
                    var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);
                    if (signalData != null)
                    {
                        signalStrength = GameState.CalculateSignalStrength(_gameState.CurrentFrequency, signalData);
                    }
                }
                _radioInterface.SetSignalStrength(signalStrength);
            }
        }
        
        // Check if a message is available at the current frequency
        private void CheckMessageAvailability()
        {
            if (_gameState == null || _radioInterface == null) return;
            
            bool messageAvailable = false;
            
            // Check if there's a signal at the current frequency
            var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);
            if (signalData != null)
            {
                // Check if the signal has a message
                if (!string.IsNullOrEmpty(signalData.MessageId))
                {
                    // Check if the message exists
                    var message = _gameState.GetMessage(signalData.MessageId);
                    if (message != null)
                    {
                        messageAvailable = true;
                    }
                }
            }
            
            // Update the radio interface
            _messageAvailable = messageAvailable;
            _radioInterface.SetMessageAvailable(messageAvailable);
        }
        
        // Handle frequency changed from radio interface
        private void OnFrequencyChanged(float frequency)
        {
            if (_gameState == null) return;
            
            // Update game state
            _gameState.SetFrequency(frequency);
            
            // Check for message availability
            CheckMessageAvailability();
        }
        
        // Handle power toggle from radio interface
        private void OnPowerToggle(bool isPoweredOn)
        {
            if (_gameState == null) return;
            
            // Only toggle if the state is different
            if (_gameState.IsRadioOn != isPoweredOn)
            {
                _gameState.ToggleRadio();
            }
        }
        
        // Handle scan requested from radio interface
        private void OnScanRequested()
        {
            if (_gameState == null) return;
            
            // Implement frequency scanning
            // This is a simple implementation that scans through frequencies
            // until it finds a signal
            
            float startFreq = _gameState.CurrentFrequency;
            float currentFreq = startFreq;
            float step = 0.1f;
            bool signalFound = false;
            
            // Scan up to 10 MHz in both directions
            for (int i = 0; i < 100 && !signalFound; i++)
            {
                currentFreq += step;
                
                // Wrap around if we reach the limits
                if (currentFreq > 108.0f)
                {
                    currentFreq = 88.0f;
                }
                
                // Check if there's a signal at this frequency
                var signalData = _gameState.FindSignalAtFrequency(currentFreq);
                if (signalData != null)
                {
                    // Found a signal, stop scanning
                    signalFound = true;
                    _gameState.SetFrequency(currentFreq);
                    break;
                }
            }
            
            // If no signal was found, return to the original frequency
            if (!signalFound)
            {
                _gameState.SetFrequency(startFreq);
            }
        }
        
        // Handle message requested from radio interface
        private void OnMessageRequested()
        {
            if (_gameState == null || !_messageAvailable) return;
            
            // Get the signal at the current frequency
            var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);
            if (signalData != null && !string.IsNullOrEmpty(signalData.MessageId))
            {
                // Show the message
                var messageManager = GetNode<MessageManager>("/root/MessageManager");
                if (messageManager != null)
                {
                    messageManager.ShowMessage(signalData.MessageId);
                }
                else
                {
                    GD.Print($"Message content: {_gameState.GetMessage(signalData.MessageId)?.Content}");
                }
            }
        }
        
        // Handle frequency changed from game state
        private void OnGameStateFrequencyChanged(float frequency)
        {
            if (_radioInterface == null) return;
            
            // Update radio interface
            _radioInterface.SetFrequency(frequency);
            
            // Check for message availability
            CheckMessageAvailability();
        }
        
        // Handle radio toggled from game state
        private void OnGameStateRadioToggled(bool isOn)
        {
            if (_radioInterface == null) return;
            
            // Update radio interface
            _radioInterface.IsPoweredOn = isOn;
            _radioInterface.QueueRedraw();
            
            // Check for message availability
            CheckMessageAvailability();
        }
        
        // Handle signal detected from radio system
        private void OnSignalDetected(string signalId, float frequency)
        {
            _currentSignalId = signalId;
            CheckMessageAvailability();
        }
        
        // Handle signal lost from radio system
        private void OnSignalLost(string signalId)
        {
            if (_currentSignalId == signalId)
            {
                _currentSignalId = null;
                CheckMessageAvailability();
            }
        }
        
        // Handle signal discovered from game state
        private void OnSignalDiscovered(string signalId, float frequency)
        {
            CheckMessageAvailability();
        }
    }
}

using Godot;
using System.Collections.Generic;
using System.Linq;

namespace SignalLost.Radio
{
    /// <summary>
    /// Manages radio signals and their discovery in the game.
    /// </summary>
    [GlobalClass]
    public partial class RadioSignalsManager : Node
    {
        // Dictionary of all signals in the game
        private Dictionary<string, EnhancedSignalData> _signalDatabase = new Dictionary<string, EnhancedSignalData>();
        
        // List of discovered signal IDs
        private List<string> _discoveredSignals = new List<string>();
        
        // Currently active signal
        private EnhancedSignalData _currentSignal = null;
        
        // Current signal strength
        private float _currentSignalStrength = 0.0f;
        
        // Reference to the GameState
        private GameState _gameState;
        
        // Signal detection boost from equipment (multiplier)
        private float _signalBoost = 1.0f;
        
        // Can detect hidden signals
        private bool _canDetectHiddenSignals = false;
        
        // Signals
        [Signal]
        public delegate void SignalDiscoveredEventHandler(string signalId);
        
        [Signal]
        public delegate void SignalLostEventHandler(string signalId);
        
        [Signal]
        public delegate void SignalStrengthChangedEventHandler(string signalId, float strength);
        
        [Signal]
        public delegate void SignalDecodedEventHandler(string signalId);
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get the GameState
            _gameState = GetNode<GameState>("/root/GameState");
            
            if (_gameState == null)
            {
                GD.PrintErr("RadioSignalsManager: GameState not found");
                return;
            }
            
            // Connect to GameState signals
            _gameState.FrequencyChanged += OnFrequencyChanged;
            _gameState.RadioToggled += OnRadioToggled;
            _gameState.InventoryChanged += OnInventoryChanged;
            
            // Initialize the signal database
            InitializeSignalDatabase();
            
            // Sync with GameState
            SyncWithGameState();
        }
        
        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_gameState != null)
            {
                _gameState.FrequencyChanged -= OnFrequencyChanged;
                _gameState.RadioToggled -= OnRadioToggled;
                _gameState.InventoryChanged -= OnInventoryChanged;
            }
        }
        
        // Initialize the signal database with all signals in the game
        private void InitializeSignalDatabase()
        {
            // Emergency Broadcast
            AddSignalToDatabase(new EnhancedSignalData
            {
                Id = "emergency_broadcast",
                Name = "Emergency Broadcast",
                Description = "An automated emergency broadcast message.",
                Frequency = 121.5f,
                Type = SignalType.Voice,
                Content = "This is an emergency broadcast. All civilians are advised to evacuate the area immediately. This is not a drill.",
                LocationId = "emergency_center",
                IsStatic = true,
                Bandwidth = 0.3f,
                MinSignalStrength = 0.3f
            });
            
            // Military Communication
            AddSignalToDatabase(new EnhancedSignalData
            {
                Id = "military_comm",
                Name = "Military Communication",
                Description = "Encrypted military communication channel.",
                Frequency = 140.85f,
                Type = SignalType.Data,
                Content = "ENCRYPTED: 7A-F3-22-9C-B1-D4-E8-0F-5A-C6-3B-8D-2E-7F-A9-1C",
                LocationId = "military_outpost",
                IsStatic = false,
                Bandwidth = 0.2f,
                MinSignalStrength = 0.6f,
                IsDecoded = false,
                RequiredItemToUnlock = "military_badge"
            });
            
            // Research Facility
            AddSignalToDatabase(new EnhancedSignalData
            {
                Id = "research_data",
                Name = "Research Facility Data",
                Description = "Data transmission from the research facility.",
                Frequency = 152.25f,
                Type = SignalType.Data,
                Content = "PROJECT SIGNAL: Test sequence alpha-3 initiated. Quantum resonance detected at coordinates 47.2N, 122.5W. Temporal anomaly readings increasing.",
                LocationId = "research_facility",
                IsStatic = true,
                Bandwidth = 0.3f,
                MinSignalStrength = 0.5f,
                RequiredItemToUnlock = "keycard_research"
            });
            
            // Distress Signal
            AddSignalToDatabase(new EnhancedSignalData
            {
                Id = "distress_signal",
                Name = "Distress Signal",
                Description = "A distress signal from a survivor.",
                Frequency = 106.7f,
                Type = SignalType.Voice,
                Content = "If anyone can hear this, I'm trapped in the old cabin near the lake. Something is outside. Please send help.",
                LocationId = "cabin",
                IsStatic = false,
                Bandwidth = 0.4f,
                MinSignalStrength = 0.4f
            });
            
            // Weather Station
            AddSignalToDatabase(new EnhancedSignalData
            {
                Id = "weather_data",
                Name = "Weather Station",
                Description = "Automated weather data transmission.",
                Frequency = 162.4f,
                Type = SignalType.Data,
                Content = "ATMOSPHERIC ANOMALY DETECTED: Unusual patterns in the upper atmosphere. Barometric pressure: 28.5 inHg, falling. Temperature: 42°F, falling. Wind: 25 mph, NE.",
                LocationId = "weather_station",
                IsStatic = true,
                Bandwidth = 0.3f,
                MinSignalStrength = 0.3f
            });
            
            // Mysterious Signal
            AddSignalToDatabase(new EnhancedSignalData
            {
                Id = "mysterious_signal",
                Name = "Unknown Signal",
                Description = "A strange signal of unknown origin.",
                Frequency = 87.5f,
                Type = SignalType.Morse,
                Content = "... .. --. -. .- .-.. / .-.. --- ... -", // "SIGNAL LOST" in Morse
                LocationId = "mysterious_tower",
                IsStatic = false,
                Bandwidth = 0.2f,
                MinSignalStrength = 0.7f,
                IsHidden = true,
                StoryProgressRequired = 5
            });
            
            // Automated Beacon
            AddSignalToDatabase(new EnhancedSignalData
            {
                Id = "automated_beacon",
                Name = "Automated Beacon",
                Description = "An automated beacon signal.",
                Frequency = 135.0f,
                Type = SignalType.Morse,
                Content = "... --- ...", // "SOS" in Morse
                LocationId = "crashed_vehicle",
                IsStatic = true,
                Bandwidth = 0.3f,
                MinSignalStrength = 0.4f
            });
            
            // Radio Station
            AddSignalToDatabase(new EnhancedSignalData
            {
                Id = "radio_station",
                Name = "Radio Station",
                Description = "A local radio station broadcasting music and news.",
                Frequency = 98.5f,
                Type = SignalType.Voice,
                Content = "You're listening to 98.5 FM. Our top story tonight: Strange atmospheric phenomena reported across the region. Scientists are baffled by the sudden appearance of...*static*",
                LocationId = "radio_station",
                IsStatic = true,
                Bandwidth = 0.5f,
                MinSignalStrength = 0.2f
            });
        }
        
        // Add a signal to the database
        public void AddSignalToDatabase(EnhancedSignalData signal)
        {
            _signalDatabase[signal.Id] = signal;
        }
        
        // Sync with GameState
        private void SyncWithGameState()
        {
            // Clear current discovered signals
            _discoveredSignals.Clear();
            
            // Add signals from GameState's discovered signals
            foreach (var signalEntry in _gameState.GetDiscoveredSignals())
            {
                string signalId = signalEntry.Key;
                if (_signalDatabase.ContainsKey(signalId))
                {
                    _discoveredSignals.Add(signalId);
                }
            }
            
            // Check for signals at the current frequency
            CheckForSignalsAtFrequency(_gameState.CurrentFrequency);
        }
        
        // Handle frequency changed
        private void OnFrequencyChanged(float frequency)
        {
            // Only process if the radio is on
            if (_gameState.IsRadioOn)
            {
                // Check for signals at this frequency
                CheckForSignalsAtFrequency(frequency);
            }
            else
            {
                // Radio is off, clear current signal
                _currentSignal = null;
                _currentSignalStrength = 0.0f;
            }
        }
        
        // Handle radio toggled
        private void OnRadioToggled(bool isOn)
        {
            if (isOn)
            {
                // Radio turned on, check for signals at current frequency
                CheckForSignalsAtFrequency(_gameState.CurrentFrequency);
            }
            else
            {
                // Radio turned off, clear current signal
                _currentSignal = null;
                _currentSignalStrength = 0.0f;
            }
        }
        
        // Handle inventory changed
        private void OnInventoryChanged()
        {
            // Recheck current frequency in case we now have items that unlock signals
            if (_gameState.IsRadioOn)
            {
                CheckForSignalsAtFrequency(_gameState.CurrentFrequency);
            }
        }
        
        // Check for signals at a specific frequency
        private void CheckForSignalsAtFrequency(float frequency)
        {
            // Clear current signal
            EnhancedSignalData previousSignal = _currentSignal;
            _currentSignal = null;
            _currentSignalStrength = 0.0f;
            
            // Find signals at this frequency
            foreach (var signal in _signalDatabase.Values)
            {
                // Calculate signal strength
                float signalStrength = signal.CalculateSignalStrength(frequency);
                
                // Apply signal boost from equipment
                signalStrength *= _signalBoost;
                
                // Check if signal is detectable
                if (signalStrength >= signal.MinSignalStrength)
                {
                    // Check if signal is hidden and if we can detect hidden signals
                    if (signal.IsHidden && !_canDetectHiddenSignals)
                    {
                        continue;
                    }
                    
                    // Check if signal can be detected based on requirements
                    if (signal.CanBeDetected(signalStrength, _gameState.GameProgress, _gameState.Inventory))
                    {
                        // This is the strongest signal at this frequency
                        if (_currentSignal == null || signalStrength > _currentSignalStrength)
                        {
                            _currentSignal = signal;
                            _currentSignalStrength = signalStrength;
                        }
                        
                        // Discover the signal if not already discovered
                        if (!_discoveredSignals.Contains(signal.Id))
                        {
                            DiscoverSignal(signal.Id);
                        }
                    }
                }
            }
            
            // Emit signal strength changed signal if we have a current signal
            if (_currentSignal != null)
            {
                EmitSignal(SignalName.SignalStrengthChanged, _currentSignal.Id, _currentSignalStrength);
            }
            
            // Emit signal lost signal if we had a signal before but not now
            if (previousSignal != null && _currentSignal == null)
            {
                EmitSignal(SignalName.SignalLost, previousSignal.Id);
            }
        }
        
        // Discover a signal
        private void DiscoverSignal(string signalId)
        {
            if (!_signalDatabase.ContainsKey(signalId))
            {
                GD.PrintErr($"RadioSignalsManager: Signal {signalId} not found in the database");
                return;
            }
            
            _discoveredSignals.Add(signalId);
            
            // Add to GameState's discovered signals
            _gameState.AddDiscoveredSignal(signalId, _signalDatabase[signalId].Frequency);
            
            // Emit signal discovered signal
            EmitSignal(SignalName.SignalDiscovered, signalId);
            
            GD.Print($"Signal discovered: {_signalDatabase[signalId].Name}");
        }
        
        // Decode a signal
        public bool DecodeSignal(string signalId)
        {
            if (!_signalDatabase.ContainsKey(signalId))
            {
                GD.PrintErr($"RadioSignalsManager: Signal {signalId} not found in the database");
                return false;
            }
            
            if (_signalDatabase[signalId].IsDecoded)
            {
                return false;
            }
            
            _signalDatabase[signalId].IsDecoded = true;
            
            // Decode the message in GameState if it exists
            if (!string.IsNullOrEmpty(_signalDatabase[signalId].Content))
            {
                _gameState.DecodeMessage(_signalDatabase[signalId].Content);
            }
            
            // Emit signal decoded signal
            EmitSignal(SignalName.SignalDecoded, signalId);
            
            GD.Print($"Signal decoded: {_signalDatabase[signalId].Name}");
            
            return true;
        }
        
        // Get a signal by ID
        public EnhancedSignalData GetSignal(string signalId)
        {
            if (_signalDatabase.ContainsKey(signalId))
            {
                return _signalDatabase[signalId];
            }
            return null;
        }
        
        // Get the current signal
        public EnhancedSignalData GetCurrentSignal()
        {
            return _currentSignal;
        }
        
        // Get the current signal strength
        public float GetCurrentSignalStrength()
        {
            return _currentSignalStrength;
        }
        
        // Get all signals
        public Dictionary<string, EnhancedSignalData> GetAllSignals()
        {
            return _signalDatabase;
        }
        
        // Get all discovered signals
        public List<EnhancedSignalData> GetDiscoveredSignals()
        {
            return _discoveredSignals
                .Where(signalId => _signalDatabase.ContainsKey(signalId))
                .Select(signalId => _signalDatabase[signalId])
                .ToList();
        }
        
        // Check if a signal is discovered
        public bool IsSignalDiscovered(string signalId)
        {
            return _discoveredSignals.Contains(signalId);
        }
        
        // Set the signal boost from equipment
        public void SetSignalBoost(float boost)
        {
            _signalBoost = boost;
            
            // Recheck current frequency with new boost
            if (_gameState.IsRadioOn)
            {
                CheckForSignalsAtFrequency(_gameState.CurrentFrequency);
            }
            
            GD.Print($"Signal boost set to {_signalBoost}");
        }
        
        // Enable or disable hidden signal detection
        public void EnableHiddenSignalDetection(bool enable)
        {
            _canDetectHiddenSignals = enable;
            
            // Recheck current frequency with new detection capability
            if (_gameState.IsRadioOn)
            {
                CheckForSignalsAtFrequency(_gameState.CurrentFrequency);
            }
            
            GD.Print($"Hidden signal detection {(_canDetectHiddenSignals ? "enabled" : "disabled")}");
        }
    }
}

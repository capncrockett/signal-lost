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
        private PixelRadioInterface _radioInterface;
        private PixelMessageDisplay _messageDisplay;
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
            _radioInterface = GetNode<PixelRadioInterface>("PixelRadioInterface");
            _messageDisplay = GetNode<PixelMessageDisplay>("PixelMessageDisplay");
            _narrativeInfoLabel = GetNode<Label>("NarrativeInfo");
            _narrativeManager = GetNode<RadioNarrativeManager>("RadioNarrativeManager");

            // Connect signals
            _radioInterface.FrequencyChanged += OnFrequencyChanged;
            _radioInterface.PowerToggle += OnPowerToggle;
            _radioInterface.MessageRequested += OnMessageRequested;
            _messageDisplay.MessageClosed += OnMessageClosed;
            _messageDisplay.DecodeRequested += OnDecodeRequested;

            if (_narrativeManager != null)
            {
                _narrativeManager.NarrativeThreadDiscovered += OnNarrativeThreadDiscovered;
                _narrativeManager.NarrativeSignalDecoded += OnNarrativeSignalDecoded;
            }

            // Initialize demo signals
            InitializeDemoSignals();

            // Set initial state
            _radioInterface.SetFrequency(_currentFrequency);
            _radioInterface.IsPoweredOn = _radioPowered;
            CheckForSignalAtFrequency(_currentFrequency);
        }

        // Initialize demo signals
        private void InitializeDemoSignals()
        {
            // Emergency Broadcast
            var emergencySignal = new EnhancedSignalData
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
                MinSignalStrength = 0.3f,
                NarrativeThreadId = "main_story",
                NarrativeSequence = 0,
                CharacterId = "Emergency System",
                InterferenceLevel = 0.2f
            };
            _demoSignals[emergencySignal.Frequency] = emergencySignal;

            // Mysterious Signal
            var mysteriousSignal = new EnhancedSignalData
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
                NarrativeThreadId = "main_story",
                NarrativeSequence = 2,
                InterferenceLevel = 0.5f,
                EncodedContent = "... .. --. -. .- .-.. / .-.. --- ... -",
                DecodedContent = "SIGNAL LOST"
            };
            _demoSignals[mysteriousSignal.Frequency] = mysteriousSignal;

            // Radio Station
            var radioStationSignal = new EnhancedSignalData
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
                MinSignalStrength = 0.2f,
                NarrativeThreadId = "local_radio",
                NarrativeSequence = 0,
                CharacterId = "Radio Host",
                InterferenceLevel = 0.3f
            };
            _demoSignals[radioStationSignal.Frequency] = radioStationSignal;

            // Weather Data
            var weatherSignal = new EnhancedSignalData
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
                MinSignalStrength = 0.3f,
                NarrativeThreadId = "weather_station",
                NarrativeSequence = 0,
                InterferenceLevel = 0.1f,
                EncodedContent = "01000001 01010100 01001101 01001111 01010011 01010000 01001000 01000101 01010010 01001001 01000011",
                DecodedContent = "ATMOSPHERIC ANOMALY DETECTED: Unusual patterns in the upper atmosphere. Barometric pressure: 28.5 inHg, falling. Temperature: 42°F, falling. Wind: 25 mph, NE."
            };
            _demoSignals[weatherSignal.Frequency] = weatherSignal;

            // Research Facility
            var researchSignal = new EnhancedSignalData
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
                NarrativeThreadId = "main_story",
                NarrativeSequence = 3,
                CharacterId = "Research Facility AI",
                InterferenceLevel = 0.4f,
                IsKeyNarrativeSignal = true
            };
            _demoSignals[researchSignal.Frequency] = researchSignal;
        }

        // Handle frequency changed
        private void OnFrequencyChanged(float frequency)
        {
            _currentFrequency = frequency;
            CheckForSignalAtFrequency(frequency);
        }

        // Handle power toggle
        private void OnPowerToggle(bool isPowered)
        {
            _radioPowered = isPowered;

            if (!isPowered)
            {
                // Turn off signal
                _radioInterface.SetSignalStrength(0.0f);
                _radioInterface.SetMessageAvailable(false);
                _currentSignalId = "";
                _currentSignalStrength = 0.0f;
            }
            else
            {
                // Check for signal at current frequency
                CheckForSignalAtFrequency(_currentFrequency);
            }
        }

        // Check for a signal at the specified frequency
        private void CheckForSignalAtFrequency(float frequency)
        {
            if (!_radioPowered)
            {
                return;
            }

            // Reset signal
            _currentSignalId = "";
            _currentSignalStrength = 0.0f;
            _radioInterface.SetSignalStrength(0.0f);
            _radioInterface.SetMessageAvailable(false);

            // Check each signal
            foreach (var signal in _demoSignals.Values)
            {
                float signalStrength = signal.CalculateSignalStrength(frequency);

                // Check if signal is detectable
                if (signalStrength >= signal.MinSignalStrength)
                {
                    // This is a detectable signal
                    _currentSignalId = signal.Id;
                    _currentSignalStrength = signalStrength;
                    _radioInterface.SetSignalStrength(signalStrength);
                    _radioInterface.SetMessageAvailable(true);
                    break;
                }
            }
        }

        // Handle message requested
        private void OnMessageRequested()
        {
            if (string.IsNullOrEmpty(_currentSignalId) || _currentSignalStrength <= 0.0f)
            {
                return;
            }

            // Get the signal
            EnhancedSignalData signal = null;
            foreach (var s in _demoSignals.Values)
            {
                if (s.Id == _currentSignalId)
                {
                    signal = s;
                    break;
                }
            }

            if (signal == null)
            {
                return;
            }

            // Show the message
            _messageDisplay.MessageType = "Radio";
            _messageDisplay.SetMessage(
                signal.Id,
                signal.Name,
                signal.GetFormattedContent(_currentSignalStrength),
                signal.IsDecoded,
                signal.InterferenceLevel
            );
            _messageDisplay.Visible = true;
        }

        // Handle message closed
        private void OnMessageClosed()
        {
            _messageDisplay.Visible = false;
        }

        // Handle decode requested
        private void OnDecodeRequested(string signalId)
        {
            // Find the signal
            EnhancedSignalData signal = null;
            foreach (var s in _demoSignals.Values)
            {
                if (s.Id == signalId)
                {
                    signal = s;
                    break;
                }
            }

            if (signal == null)
            {
                return;
            }

            // Decode the signal
            signal.IsDecoded = true;

            // Notify the narrative manager
            if (_narrativeManager != null)
            {
                // This would normally be handled by the RadioSignalsManager
                // but for the demo we'll call it directly
                _narrativeManager.OnSignalDecoded(signalId);
            }

            // Update the message display
            _messageDisplay.SetMessage(
                signal.Id,
                signal.Name,
                signal.GetFormattedContent(_currentSignalStrength),
                true,
                signal.InterferenceLevel
            );
        }

        // Handle narrative thread discovered
        private void OnNarrativeThreadDiscovered(string threadId)
        {
            UpdateNarrativeInfoLabel();
        }

        // Handle narrative signal decoded
        private void OnNarrativeSignalDecoded(string signalId)
        {
            UpdateNarrativeInfoLabel();
        }

        // Update the narrative info label
        private void UpdateNarrativeInfoLabel()
        {
            if (_narrativeManager == null)
            {
                return;
            }

            string text = "Narrative Threads:\n";
            var discoveredThreads = _narrativeManager.GetDiscoveredThreads();

            if (discoveredThreads.Count == 0)
            {
                text += "- None discovered yet";
            }
            else
            {
                foreach (string threadId in discoveredThreads)
                {
                    var thread = _narrativeManager.GetThreadInfo(threadId);
                    if (thread != null)
                    {
                        float progress = _narrativeManager.GetThreadProgress(threadId);
                        text += $"- {thread.Name} ({progress:P0} complete)\n";
                    }
                }
            }

            _narrativeInfoLabel.Text = text;
        }
    }
}

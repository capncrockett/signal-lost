using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost.Radio
{
    /// <summary>
    /// Manages narrative progression through radio signals, connecting radio content with story elements.
    /// </summary>
    [GlobalClass]
    public partial class RadioNarrativeManager : Node
    {
        // Singleton instance
        private static RadioNarrativeManager _instance;
        public static RadioNarrativeManager Instance => _instance;

        // References to other systems
        private GameState _gameState;
        private RadioSignalsManager _signalsManager;

        // Narrative state
        private Dictionary<string, NarrativeSignalState> _narrativeState = new Dictionary<string, NarrativeSignalState>();
        private List<string> _discoveredNarrativeThreads = new List<string>();
        private string _activeNarrativeThread = "";

        // Narrative threads (groups of related signals that tell a story)
        private Dictionary<string, NarrativeThread> _narrativeThreads = new Dictionary<string, NarrativeThread>();

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Set up singleton
            if (_instance != null)
            {
                QueueFree();
                return;
            }
            _instance = this;

            // Get references to other systems
            _gameState = GetNode<GameState>("/root/GameState");
            _signalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");

            // Connect to signals
            if (_signalsManager != null)
            {
                _signalsManager.SignalDiscovered += OnSignalDiscovered;
                _signalsManager.SignalDecoded += OnSignalDecoded;
            }

            // Initialize narrative threads
            InitializeNarrativeThreads();

            GD.Print("RadioNarrativeManager initialized");
        }

        // Initialize narrative threads
        private void InitializeNarrativeThreads()
        {
            // Main story thread
            var mainThread = new NarrativeThread
            {
                Id = "main_story",
                Name = "Signal Lost",
                Description = "The main story of Signal Lost, following the mysterious disappearance of a research team.",
                SignalIds = new List<string>
                {
                    "emergency_broadcast",
                    "mysterious_signal",
                    "research_lab",
                    "distress_call",
                    "final_message"
                },
                IsMainStory = true
            };
            _narrativeThreads[mainThread.Id] = mainThread;

            // Side story: Weather Station
            var weatherThread = new NarrativeThread
            {
                Id = "weather_station",
                Name = "Atmospheric Anomalies",
                Description = "Reports and data from the regional weather station about unusual atmospheric phenomena.",
                SignalIds = new List<string>
                {
                    "weather_report",
                    "atmospheric_data",
                    "storm_warning",
                    "weather_station_log"
                }
            };
            _narrativeThreads[weatherThread.Id] = weatherThread;

            // Side story: Local Radio
            var localRadioThread = new NarrativeThread
            {
                Id = "local_radio",
                Name = "Local Broadcasts",
                Description = "Regular broadcasts from the local radio station, with news and updates about the region.",
                SignalIds = new List<string>
                {
                    "radio_station",
                    "news_broadcast",
                    "interview_scientist",
                    "emergency_alert"
                }
            };
            _narrativeThreads[localRadioThread.Id] = localRadioThread;
        }

        // Handle signal discovered
        private void OnSignalDiscovered(string signalId)
        {
            if (!_narrativeState.ContainsKey(signalId))
            {
                _narrativeState[signalId] = new NarrativeSignalState
                {
                    SignalId = signalId,
                    DiscoveryTime = DateTime.Now,
                    IsDecoded = false
                };
            }

            // Check if this signal belongs to a narrative thread
            foreach (var thread in _narrativeThreads.Values)
            {
                if (thread.SignalIds.Contains(signalId) && !_discoveredNarrativeThreads.Contains(thread.Id))
                {
                    // Discovered a new narrative thread
                    _discoveredNarrativeThreads.Add(thread.Id);
                    EmitSignal(SignalName.NarrativeThreadDiscovered, thread.Id);
                    GD.Print($"Discovered narrative thread: {thread.Name}");
                }
            }

            // Update game state
            if (_gameState != null)
            {
                _gameState.AddDiscoveredSignal(signalId, _gameState.CurrentFrequency);
            }

            // Emit signal
            EmitSignal(SignalName.NarrativeSignalDiscovered, signalId);
        }

        // Handle signal decoded
        public void OnSignalDecoded(string signalId)
        {
            ProcessDecodedSignal(signalId);
        }

        // Public method for external systems to notify of signal decoding
        private void ProcessDecodedSignal(string signalId)
        {
            if (_narrativeState.ContainsKey(signalId))
            {
                _narrativeState[signalId].IsDecoded = true;
                _narrativeState[signalId].DecodeTime = DateTime.Now;
            }
            else
            {
                _narrativeState[signalId] = new NarrativeSignalState
                {
                    SignalId = signalId,
                    DiscoveryTime = DateTime.Now,
                    DecodeTime = DateTime.Now,
                    IsDecoded = true
                };
            }

            // Check for narrative progression
            CheckNarrativeProgression(signalId);

            // Emit signal
            EmitSignal(SignalName.NarrativeSignalDecoded, signalId);
        }

        // Check for narrative progression
        private void CheckNarrativeProgression(string signalId)
        {
            // Find which narrative thread this signal belongs to
            foreach (var thread in _narrativeThreads.Values)
            {
                if (thread.SignalIds.Contains(signalId))
                {
                    // Calculate progress in this thread
                    int decodedCount = 0;
                    foreach (string threadSignalId in thread.SignalIds)
                    {
                        if (_narrativeState.ContainsKey(threadSignalId) && _narrativeState[threadSignalId].IsDecoded)
                        {
                            decodedCount++;
                        }
                    }

                    // Calculate progress percentage
                    float progress = (float)decodedCount / thread.SignalIds.Count;

                    // Emit progress signal
                    EmitSignal(SignalName.NarrativeThreadProgressed, thread.Id, progress);

                    // Check if thread is complete
                    if (decodedCount == thread.SignalIds.Count)
                    {
                        EmitSignal(SignalName.NarrativeThreadCompleted, thread.Id);
                        GD.Print($"Completed narrative thread: {thread.Name}");

                        // If this is the main story, update game progress
                        if (thread.IsMainStory && _gameState != null)
                        {
                            _gameState.SetGameProgress(100);
                        }
                    }
                    else
                    {
                        // Update game progress for main story
                        if (thread.IsMainStory && _gameState != null)
                        {
                            int progressValue = (int)(progress * 100);
                            _gameState.SetGameProgress(progressValue);
                        }
                    }
                }
            }
        }

        // Get the next signal in a narrative thread
        public string GetNextSignalInThread(string threadId)
        {
            if (!_narrativeThreads.ContainsKey(threadId))
            {
                return "";
            }

            var thread = _narrativeThreads[threadId];
            foreach (string signalId in thread.SignalIds)
            {
                if (!_narrativeState.ContainsKey(signalId) || !_narrativeState[signalId].IsDecoded)
                {
                    return signalId;
                }
            }

            return ""; // All signals in this thread are decoded
        }

        // Get narrative thread progress
        public float GetThreadProgress(string threadId)
        {
            if (!_narrativeThreads.ContainsKey(threadId))
            {
                return 0.0f;
            }

            var thread = _narrativeThreads[threadId];
            int decodedCount = 0;
            foreach (string signalId in thread.SignalIds)
            {
                if (_narrativeState.ContainsKey(signalId) && _narrativeState[signalId].IsDecoded)
                {
                    decodedCount++;
                }
            }

            return (float)decodedCount / thread.SignalIds.Count;
        }

        // Get all discovered narrative threads
        public List<string> GetDiscoveredThreads()
        {
            return new List<string>(_discoveredNarrativeThreads);
        }

        // Get narrative thread info
        public NarrativeThread GetThreadInfo(string threadId)
        {
            if (_narrativeThreads.ContainsKey(threadId))
            {
                return _narrativeThreads[threadId];
            }
            return null;
        }

        // Set the active narrative thread
        public void SetActiveThread(string threadId)
        {
            if (_narrativeThreads.ContainsKey(threadId))
            {
                _activeNarrativeThread = threadId;
                EmitSignal(SignalName.ActiveNarrativeThreadChanged, threadId);
            }
        }

        // Get the active narrative thread
        public string GetActiveThread()
        {
            return _activeNarrativeThread;
        }

        // Signals
        [Signal] public delegate void NarrativeSignalDiscoveredEventHandler(string signalId);
        [Signal] public delegate void NarrativeSignalDecodedEventHandler(string signalId);
        [Signal] public delegate void NarrativeThreadDiscoveredEventHandler(string threadId);
        [Signal] public delegate void NarrativeThreadProgressedEventHandler(string threadId, float progress);
        [Signal] public delegate void NarrativeThreadCompletedEventHandler(string threadId);
        [Signal] public delegate void ActiveNarrativeThreadChangedEventHandler(string threadId);
    }

    // Narrative thread class
    public class NarrativeThread
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> SignalIds { get; set; } = new List<string>();
        public bool IsMainStory { get; set; } = false;
    }

    // Narrative signal state class
    public class NarrativeSignalState
    {
        public string SignalId { get; set; } = "";
        public DateTime DiscoveryTime { get; set; }
        public DateTime DecodeTime { get; set; }
        public bool IsDecoded { get; set; } = false;
    }
}

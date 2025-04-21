// Moved to tests/e2e/
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests.E2E
{
    /// <summary>
    /// Base class for end-to-end tests in the Signal Lost game.
    /// End-to-end tests focus on testing complete gameplay scenarios.
    /// </summary>
    [TestClass]
    public abstract partial class E2ETestBase : TestBase
    {
        // Common components used in E2E tests
        protected GameState _gameState;
        protected QuestSystem _questSystem;
        protected MapSystem _mapSystem;
        protected InventorySystem _inventorySystem;
        protected MessageManager _messageManager;
        protected AudioManager _audioManager;
        protected GameProgressionManager _progressionManager;
        protected RadioTuner _radioTuner;
        
        // List to track created nodes for cleanup
        private readonly List<Node> _createdNodes = new List<Node>();
        
        /// <summary>
        /// Called before each test. Sets up the complete game environment for E2E tests.
        /// </summary>
        public override async void Before()
        {
            base.Before();
            LogMessage($"Starting E2E test: {GetType().Name}");
            
            try
            {
                // Create instances of the components
                _gameState = new GameState();
                _questSystem = new QuestSystem();
                _mapSystem = new MapSystem();
                _inventorySystem = new InventorySystem();
                _messageManager = new MessageManager();
                _audioManager = new AudioManager();
                _progressionManager = new GameProgressionManager();
                
                // Add them to the scene tree using call_deferred
                SafeAddChild(_gameState);
                SafeAddChild(_questSystem);
                SafeAddChild(_mapSystem);
                SafeAddChild(_inventorySystem);
                SafeAddChild(_messageManager);
                SafeAddChild(_audioManager);
                SafeAddChild(_progressionManager);
                
                // Track created nodes for cleanup
                _createdNodes.Add(_gameState);
                _createdNodes.Add(_questSystem);
                _createdNodes.Add(_mapSystem);
                _createdNodes.Add(_inventorySystem);
                _createdNodes.Add(_messageManager);
                _createdNodes.Add(_audioManager);
                _createdNodes.Add(_progressionManager);
                
                // Wait a frame to ensure all nodes are added
                await ToSignal(GetTree(), "process_frame");
                
                // Set up audio buses for testing
                SetupAudioBuses();
                
                // Initialize the components
                _gameState._Ready();
                _questSystem._Ready();
                _mapSystem._Ready();
                _inventorySystem._Ready();
                _messageManager._Ready();
                _audioManager._Ready();
                _progressionManager._Ready();
                
                // Create a RadioTuner
                _radioTuner = CreateRadioTuner();
                if (_radioTuner != null)
                {
                    _radioTuner._Ready();
                }
                
                // Set up initial game state
                ResetGameState();
            }
            catch (Exception ex)
            {
                LogError($"Error in {GetType().Name}.Before: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Called after each test. Cleans up the environment.
        /// </summary>
        public override void After()
        {
            try
            {
                // Clean up audio
                if (_audioManager != null)
                {
                    _audioManager.StopStaticNoise();
                    _audioManager.StopSignal();
                }
                
                // Clean up all created nodes
                foreach (var node in _createdNodes)
                {
                    if (node != null && IsInstanceValid(node))
                    {
                        node.QueueFree();
                    }
                }
                
                _createdNodes.Clear();
                
                // Clear references
                _gameState = null;
                _questSystem = null;
                _mapSystem = null;
                _inventorySystem = null;
                _messageManager = null;
                _audioManager = null;
                _progressionManager = null;
                _radioTuner = null;
            }
            catch (Exception ex)
            {
                LogError($"Error in {GetType().Name}.After: {ex.Message}");
                LogError(ex.StackTrace);
            }
            
            LogMessage($"Finished E2E test: {GetType().Name}");
            base.After();
        }
        
        /// <summary>
        /// Sets up audio buses for testing.
        /// </summary>
        protected static void SetupAudioBuses()
        {
            // Make sure we have the Master bus
            if (AudioServer.GetBusCount() == 0)
            {
                AudioServer.AddBus();
                AudioServer.SetBusName(0, "Master");
            }

            // Create Static bus if it doesn't exist
            int staticBusIdx = AudioServer.GetBusIndex("Static");
            if (staticBusIdx == -1)
            {
                AudioServer.AddBus();
                staticBusIdx = AudioServer.GetBusCount() - 1;
                AudioServer.SetBusName(staticBusIdx, "Static");
                AudioServer.SetBusSend(staticBusIdx, "Master");
            }

            // Create Signal bus if it doesn't exist
            int signalBusIdx = AudioServer.GetBusIndex("Signal");
            if (signalBusIdx == -1)
            {
                AudioServer.AddBus();
                signalBusIdx = AudioServer.GetBusCount() - 1;
                AudioServer.SetBusName(signalBusIdx, "Signal");
                AudioServer.SetBusSend(signalBusIdx, "Master");
            }
        }
        
        /// <summary>
        /// Creates a RadioTuner for testing.
        /// </summary>
        /// <returns>A new RadioTuner instance</returns>
        protected RadioTuner CreateRadioTuner()
        {
            try
            {
                // Try to load the RadioTuner scene
                var radioTunerScene = GD.Load<PackedScene>("res://scenes/radio/RadioTuner.tscn");
                if (radioTunerScene != null)
                {
                    // Create a new instance of the RadioTuner scene
                    var radioTuner = radioTunerScene.Instantiate<RadioTuner>();
                    SafeAddChild(radioTuner);
                    _createdNodes.Add(radioTuner);
                    return radioTuner;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading RadioTuner scene: {ex.Message}");
                LogMessage("Creating a basic mock RadioTuner instead");
            }
            
            // Create a basic mock RadioTuner
            var mockRadioTuner = new RadioTuner();
            SafeAddChild(mockRadioTuner);
            _createdNodes.Add(mockRadioTuner);
            
            // Add mock UI elements
            var frequencyDisplay = new Label();
            frequencyDisplay.Name = "FrequencyDisplay";
            mockRadioTuner.AddChild(frequencyDisplay);
            
            var powerButton = new Button();
            powerButton.Name = "PowerButton";
            mockRadioTuner.AddChild(powerButton);
            
            var frequencySlider = new HSlider();
            frequencySlider.Name = "FrequencySlider";
            mockRadioTuner.AddChild(frequencySlider);
            
            var signalStrengthMeter = new ProgressBar();
            signalStrengthMeter.Name = "SignalStrengthMeter";
            mockRadioTuner.AddChild(signalStrengthMeter);
            
            var staticVisualization = new Control();
            staticVisualization.Name = "StaticVisualization";
            mockRadioTuner.AddChild(staticVisualization);
            
            var messageContainer = new Control();
            messageContainer.Name = "MessageContainer";
            mockRadioTuner.AddChild(messageContainer);
            
            var messageButton = new Button();
            messageButton.Name = "MessageButton";
            messageContainer.AddChild(messageButton);
            
            var messageDisplay = new Control();
            messageDisplay.Name = "MessageDisplay";
            messageContainer.AddChild(messageDisplay);
            
            var scanButton = new Button();
            scanButton.Name = "ScanButton";
            mockRadioTuner.AddChild(scanButton);
            
            var tuneDownButton = new Button();
            tuneDownButton.Name = "TuneDownButton";
            mockRadioTuner.AddChild(tuneDownButton);
            
            var tuneUpButton = new Button();
            tuneUpButton.Name = "TuneUpButton";
            mockRadioTuner.AddChild(tuneUpButton);
            
            return mockRadioTuner;
        }
        
        /// <summary>
        /// Resets the game state to the beginning of the game.
        /// </summary>
        protected void ResetGameState()
        {
            if (_gameState == null)
            {
                LogError("GameState is null, cannot reset game state");
                return;
            }
            
            // Reset game progress
            _gameState.SetGameProgress(0);
            
            // Reset radio state
            _gameState.SetFrequency(90.0f);
            if (_gameState.IsRadioOn)
            {
                _gameState.ToggleRadio(); // Ensure it's off
            }
            
            // Clear discovered frequencies
            _gameState.DiscoveredFrequencies.Clear();
            
            // Reset inventory
            if (_inventorySystem != null)
            {
                _inventorySystem.GetInventory().Clear();
            }
            
            // Reset quests
            if (_questSystem != null)
            {
                // Reset quest state (this is a simplified approach)
                foreach (var quest in _questSystem.GetAllQuests())
                {
                    quest.Value.IsDiscovered = false;
                    quest.Value.IsActive = false;
                    quest.Value.IsCompleted = false;
                    
                    foreach (var objective in quest.Value.Objectives)
                    {
                        objective.IsCompleted = false;
                        objective.CurrentAmount = 0;
                    }
                }
            }
            
            // Reset map
            if (_mapSystem != null)
            {
                // Reset discovered locations (this is a simplified approach)
                var locations = _mapSystem.GetAllLocations();
                foreach (var location in locations)
                {
                    if (location.Value.IsDiscovered && location.Key != "bunker")
                    {
                        location.Value.IsDiscovered = false;
                    }
                }
            }
            
            // Reset progression
            if (_progressionManager != null)
            {
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.Beginning);
            }
        }
        
        /// <summary>
        /// Simulates a complete radio tuning workflow.
        /// </summary>
        /// <param name="targetFrequency">The frequency to tune to</param>
        /// <returns>True if a signal was found at the target frequency, false otherwise</returns>
        protected async Task<bool> SimulateRadioTuning(float targetFrequency)
        {
            if (_gameState == null || _radioTuner == null)
            {
                LogError("GameState or RadioTuner is null, cannot simulate radio tuning");
                return false;
            }
            
            try
            {
                // Turn radio on
                if (!_gameState.IsRadioOn)
                {
                    _gameState.ToggleRadio();
                }
                
                // Set frequency
                _gameState.SetFrequency(targetFrequency);
                
                // Wait a frame
                await ToSignal(GetTree(), "process_frame");
                
                // Process the frequency manually
                var signalData = _gameState.FindSignalAtFrequency(targetFrequency);
                if (signalData != null)
                {
                    // Set the current signal ID
                    _radioTuner.Set("_currentSignalId", signalData.MessageId);
                    
                    // Add the frequency to discovered frequencies
                    _gameState.AddDiscoveredFrequency(signalData.Frequency);
                    
                    // Calculate signal strength
                    float signalStrength = GameState.CalculateSignalStrength(targetFrequency, signalData);
                    _radioTuner.Set("_signalStrength", signalStrength);
                    _radioTuner.Set("_staticIntensity", 1.0f - signalStrength);
                    
                    return true;
                }
                else
                {
                    // No signal found
                    _radioTuner.Set("_currentSignalId", "");
                    _radioTuner.Set("_signalStrength", 0.1f);
                    _radioTuner.Set("_staticIntensity", 0.9f);
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in SimulateRadioTuning: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Simulates scanning for radio signals.
        /// </summary>
        /// <param name="startFrequency">The frequency to start scanning from</param>
        /// <param name="endFrequency">The frequency to end scanning at</param>
        /// <param name="step">The frequency step for scanning</param>
        /// <returns>A list of frequencies where signals were found</returns>
        protected async Task<List<float>> SimulateRadioScanning(float startFrequency, float endFrequency, float step = 0.1f)
        {
            if (_gameState == null || _radioTuner == null)
            {
                LogError("GameState or RadioTuner is null, cannot simulate radio scanning");
                return new List<float>();
            }
            
            var foundFrequencies = new List<float>();
            
            try
            {
                // Turn radio on
                if (!_gameState.IsRadioOn)
                {
                    _gameState.ToggleRadio();
                }
                
                // Start scanning
                _radioTuner.Set("_isScanning", true);
                
                // Scan through frequencies
                for (float freq = startFrequency; freq <= endFrequency; freq += step)
                {
                    // Set frequency
                    _gameState.SetFrequency(freq);
                    
                    // Wait a frame
                    await ToSignal(GetTree(), "process_frame");
                    
                    // Process the frequency manually
                    var signalData = _gameState.FindSignalAtFrequency(freq);
                    if (signalData != null)
                    {
                        // Set the current signal ID
                        _radioTuner.Set("_currentSignalId", signalData.MessageId);
                        
                        // Add the frequency to discovered frequencies
                        _gameState.AddDiscoveredFrequency(signalData.Frequency);
                        
                        // Add to found frequencies
                        foundFrequencies.Add(freq);
                        
                        // Calculate signal strength
                        float signalStrength = GameState.CalculateSignalStrength(freq, signalData);
                        _radioTuner.Set("_signalStrength", signalStrength);
                        _radioTuner.Set("_staticIntensity", 1.0f - signalStrength);
                    }
                    else
                    {
                        // No signal found
                        _radioTuner.Set("_currentSignalId", "");
                        _radioTuner.Set("_signalStrength", 0.1f);
                        _radioTuner.Set("_staticIntensity", 0.9f);
                    }
                }
                
                // Stop scanning
                _radioTuner.Set("_isScanning", false);
            }
            catch (Exception ex)
            {
                LogError($"Error in SimulateRadioScanning: {ex.Message}");
            }
            
            return foundFrequencies;
        }
        
        /// <summary>
        /// Simulates completing a quest.
        /// </summary>
        /// <param name="questId">The ID of the quest to complete</param>
        /// <returns>True if the quest was completed successfully, false otherwise</returns>
        protected bool SimulateQuestCompletion(string questId)
        {
            if (_questSystem == null)
            {
                LogError("QuestSystem is null, cannot simulate quest completion");
                return false;
            }
            
            try
            {
                // Get the quest
                var quest = _questSystem.GetQuest(questId);
                if (quest == null)
                {
                    LogError($"Quest '{questId}' not found");
                    return false;
                }
                
                // Discover and activate the quest
                _questSystem.DiscoverQuest(questId);
                _questSystem.ActivateQuest(questId);
                
                // Complete all objectives
                foreach (var objective in quest.Objectives)
                {
                    objective.IsCompleted = true;
                    objective.CurrentAmount = objective.RequiredAmount;
                }
                
                // Complete the quest
                bool completed = _questSystem.CompleteQuest(questId);
                
                // Check progression requirements
                if (_progressionManager != null)
                {
                    _progressionManager.CheckProgressionRequirements();
                }
                
                return completed;
            }
            catch (Exception ex)
            {
                LogError($"Error in SimulateQuestCompletion: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Simulates discovering a location.
        /// </summary>
        /// <param name="locationId">The ID of the location to discover</param>
        /// <returns>True if the location was discovered successfully, false otherwise</returns>
        protected bool SimulateLocationDiscovery(string locationId)
        {
            if (_mapSystem == null)
            {
                LogError("MapSystem is null, cannot simulate location discovery");
                return false;
            }
            
            try
            {
                // Discover the location
                bool discovered = _mapSystem.DiscoverLocation(locationId);
                
                // Check progression requirements
                if (_progressionManager != null)
                {
                    _progressionManager.CheckProgressionRequirements();
                }
                
                return discovered;
            }
            catch (Exception ex)
            {
                LogError($"Error in SimulateLocationDiscovery: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Simulates adding an item to the inventory.
        /// </summary>
        /// <param name="itemId">The ID of the item to add</param>
        /// <param name="quantity">The quantity of the item to add</param>
        /// <returns>True if the item was added successfully, false otherwise</returns>
        protected bool SimulateItemAcquisition(string itemId, int quantity = 1)
        {
            if (_inventorySystem == null)
            {
                LogError("InventorySystem is null, cannot simulate item acquisition");
                return false;
            }
            
            try
            {
                // Add the item to the inventory
                bool added = _inventorySystem.AddItemToInventory(itemId, quantity);
                
                // Check progression requirements
                if (_progressionManager != null)
                {
                    _progressionManager.CheckProgressionRequirements();
                }
                
                return added;
            }
            catch (Exception ex)
            {
                LogError($"Error in SimulateItemAcquisition: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Simulates using an item from the inventory.
        /// </summary>
        /// <param name="itemId">The ID of the item to use</param>
        /// <returns>True if the item was used successfully, false otherwise</returns>
        protected bool SimulateItemUsage(string itemId)
        {
            if (_inventorySystem == null)
            {
                LogError("InventorySystem is null, cannot simulate item usage");
                return false;
            }
            
            try
            {
                // Use the item
                bool used = _inventorySystem.UseItem(itemId);
                
                // Check progression requirements
                if (_progressionManager != null)
                {
                    _progressionManager.CheckProgressionRequirements();
                }
                
                return used;
            }
            catch (Exception ex)
            {
                LogError($"Error in SimulateItemUsage: {ex.Message}");
                return false;
            }
        }
    }
}

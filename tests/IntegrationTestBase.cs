using Godot;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// Base class for integration tests in the Signal Lost game.
    /// Integration tests focus on testing how components work together.
    /// </summary>
    [TestClass]
    public abstract partial class IntegrationTestBase : TestBase
    {
        // Common components used in integration tests
        protected GameState _gameState;
        protected QuestSystem _questSystem;
        protected MapSystem _mapSystem;
        protected InventorySystem _inventorySystem;
        protected MessageManager _messageManager;
        protected AudioManager _audioManager;
        protected GameProgressionManager _progressionManager;
        
        // List to track created nodes for cleanup
        private readonly List<Node> _createdNodes = new List<Node>();
        
        /// <summary>
        /// Called before each test. Sets up the basic environment for integration tests.
        /// </summary>
        public override async void Before()
        {
            base.Before();
            LogMessage($"Starting integration test: {GetType().Name}");
            
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
                
                // Set up initial state
                _gameState.SetGameProgress(0);
                _gameState.SetFrequency(90.0f);
                if (_gameState.IsRadioOn)
                {
                    _gameState.ToggleRadio(); // Ensure it's off
                }
                
                _gameState.DiscoveredFrequencies.Clear();
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
            }
            catch (Exception ex)
            {
                LogError($"Error in {GetType().Name}.After: {ex.Message}");
                LogError(ex.StackTrace);
            }
            
            LogMessage($"Finished integration test: {GetType().Name}");
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
        /// Creates a mock RadioTuner for testing.
        /// </summary>
        /// <returns>A new RadioTuner instance</returns>
        protected RadioTuner CreateMockRadioTuner()
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
        /// Completes a quest by setting all objectives to completed.
        /// </summary>
        /// <param name="questId">The ID of the quest to complete</param>
        /// <returns>True if the quest was completed successfully, false otherwise</returns>
        protected bool CompleteQuest(string questId)
        {
            if (_questSystem == null)
            {
                LogError("QuestSystem is null, cannot complete quest");
                return false;
            }
            
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
            return _questSystem.CompleteQuest(questId);
        }
        
        /// <summary>
        /// Discovers a location on the map.
        /// </summary>
        /// <param name="locationId">The ID of the location to discover</param>
        /// <returns>True if the location was discovered successfully, false otherwise</returns>
        protected bool DiscoverLocation(string locationId)
        {
            if (_mapSystem == null)
            {
                LogError("MapSystem is null, cannot discover location");
                return false;
            }
            
            return _mapSystem.DiscoverLocation(locationId);
        }
        
        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
        /// <param name="itemId">The ID of the item to add</param>
        /// <param name="quantity">The quantity of the item to add</param>
        /// <returns>True if the item was added successfully, false otherwise</returns>
        protected bool AddItemToInventory(string itemId, int quantity = 1)
        {
            if (_inventorySystem == null)
            {
                LogError("InventorySystem is null, cannot add item to inventory");
                return false;
            }
            
            return _inventorySystem.AddItemToInventory(itemId, quantity);
        }
        
        /// <summary>
        /// Discovers a radio frequency.
        /// </summary>
        /// <param name="frequency">The frequency to discover</param>
        protected void DiscoverFrequency(float frequency)
        {
            if (_gameState == null)
            {
                LogError("GameState is null, cannot discover frequency");
                return;
            }
            
            _gameState.AddDiscoveredFrequency(frequency);
        }
        
        /// <summary>
        /// Decodes a message.
        /// </summary>
        /// <param name="messageId">The ID of the message to decode</param>
        /// <returns>True if the message was decoded successfully, false otherwise</returns>
        protected bool DecodeMessage(string messageId)
        {
            if (_gameState == null)
            {
                LogError("GameState is null, cannot decode message");
                return false;
            }
            
            return _gameState.DecodeMessage(messageId);
        }
    }
}

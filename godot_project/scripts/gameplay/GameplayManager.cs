using Godot;
using System.Collections.Generic;
using SignalLost.Radio;
using SignalLost.Field;

namespace SignalLost.Gameplay
{
    /// <summary>
    /// Manages the overall gameplay experience, coordinating between different game systems.
    /// </summary>
    [GlobalClass]
    public partial class GameplayManager : Node
    {
        // References to game systems
        private GameState _gameState;
        private QuestSystem _questSystem;
        private RadioSignalsManager _radioSignalsManager;
        private InventorySystem _inventorySystem;
        private MapSystem _mapSystem;
        
        // UI references
        [Export] private Control _radioPanel;
        [Export] private Control _inventoryPanel;
        [Export] private Control _questPanel;
        [Export] private Control _mapPanel;
        [Export] private Control _messagePanel;
        
        // Game state
        private bool _isGameInitialized = false;
        private bool _isRadioRepaired = false;
        private bool _isFirstSignalDiscovered = false;
        private bool _isFirstLocationVisited = false;
        
        // Signals
        [Signal]
        public delegate void GameStateChangedEventHandler(string state);
        
        [Signal]
        public delegate void TutorialMessageEventHandler(string message);
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to game systems
            _gameState = GetNode<GameState>("/root/GameState");
            _questSystem = GetNode<QuestSystem>("/root/QuestSystem");
            _radioSignalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");
            
            if (_gameState == null || _questSystem == null || _radioSignalsManager == null || 
                _inventorySystem == null || _mapSystem == null)
            {
                GD.PrintErr("GameplayManager: Failed to get references to game systems");
                return;
            }
            
            // Connect signals
            _gameState.MessageDecoded += OnMessageDecoded;
            _gameState.LocationChanged += OnLocationChanged;
            _gameState.InventoryChanged += OnInventoryChanged;
            _gameState.FrequencyChanged += OnFrequencyChanged;
            _gameState.RadioToggled += OnRadioToggled;
            
            _questSystem.QuestDiscovered += OnQuestDiscovered;
            _questSystem.QuestActivated += OnQuestActivated;
            _questSystem.QuestCompleted += OnQuestCompleted;
            _questSystem.QuestObjectiveCompleted += OnQuestObjectiveCompleted;
            
            _radioSignalsManager.SignalDiscovered += OnSignalDiscovered;
            _radioSignalsManager.SignalDecoded += OnSignalDecoded;
            
            _inventorySystem.ItemAdded += OnItemAdded;
            _inventorySystem.ItemRemoved += OnItemRemoved;
            _inventorySystem.ItemUsed += OnItemUsed;
            
            _mapSystem.LocationDiscovered += OnLocationDiscovered;
            
            // Initialize game state
            InitializeGameState();
        }
        
        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_gameState != null)
            {
                _gameState.MessageDecoded -= OnMessageDecoded;
                _gameState.LocationChanged -= OnLocationChanged;
                _gameState.InventoryChanged -= OnInventoryChanged;
                _gameState.FrequencyChanged -= OnFrequencyChanged;
                _gameState.RadioToggled -= OnRadioToggled;
            }
            
            if (_questSystem != null)
            {
                _questSystem.QuestDiscovered -= OnQuestDiscovered;
                _questSystem.QuestActivated -= OnQuestActivated;
                _questSystem.QuestCompleted -= OnQuestCompleted;
                _questSystem.QuestObjectiveCompleted -= OnQuestObjectiveCompleted;
            }
            
            if (_radioSignalsManager != null)
            {
                _radioSignalsManager.SignalDiscovered -= OnSignalDiscovered;
                _radioSignalsManager.SignalDecoded -= OnSignalDecoded;
            }
            
            if (_inventorySystem != null)
            {
                _inventorySystem.ItemAdded -= OnItemAdded;
                _inventorySystem.ItemRemoved -= OnItemRemoved;
                _inventorySystem.ItemUsed -= OnItemUsed;
            }
            
            if (_mapSystem != null)
            {
                _mapSystem.LocationDiscovered -= OnLocationDiscovered;
            }
        }
        
        // Initialize the game state
        private void InitializeGameState()
        {
            // Check if this is a new game or a loaded game
            if (_gameState.GameProgress == 0 && _gameState.Inventory.Count == 0)
            {
                // New game
                StartNewGame();
            }
            else
            {
                // Loaded game
                LoadGameState();
            }
            
            _isGameInitialized = true;
        }
        
        // Start a new game
        private void StartNewGame()
        {
            // Set initial game state
            _gameState.SetCurrentLocation("bunker");
            
            // Add initial items to inventory
            _inventorySystem.AddItemToInventory("flashlight");
            _inventorySystem.AddItemToInventory("water_bottle", 2);
            
            // Discover initial quests
            _questSystem.DiscoverQuest("quest_radio_repair");
            _questSystem.ActivateQuest("quest_radio_repair");
            
            // Show tutorial message
            EmitSignal(SignalName.TutorialMessage, "Welcome to Signal Lost. Your radio is damaged and needs repair. Look for radio parts to fix it.");
            
            // Set initial UI state
            UpdateUIState();
        }
        
        // Load game state
        private void LoadGameState()
        {
            // Check if radio is repaired
            _isRadioRepaired = _questSystem.IsQuestCompleted("quest_radio_repair");
            
            // Check if first signal is discovered
            _isFirstSignalDiscovered = _gameState.GetDiscoveredSignals().Count > 0;
            
            // Check if first location is visited
            _isFirstLocationVisited = _gameState.CurrentLocation != "bunker";
            
            // Update UI state
            UpdateUIState();
        }
        
        // Update UI state based on game state
        private void UpdateUIState()
        {
            // Show/hide radio panel based on radio repair status
            if (_radioPanel != null)
            {
                _radioPanel.Visible = _isRadioRepaired;
            }
            
            // Show/hide other panels based on game state
            if (_inventoryPanel != null)
            {
                _inventoryPanel.Visible = true; // Always visible
            }
            
            if (_questPanel != null)
            {
                _questPanel.Visible = true; // Always visible
            }
            
            if (_mapPanel != null)
            {
                _mapPanel.Visible = _isFirstLocationVisited;
            }
            
            // Emit game state changed signal
            EmitSignal(SignalName.GameStateChanged, GetCurrentGameState());
        }
        
        // Get the current game state as a string
        private string GetCurrentGameState()
        {
            if (!_isRadioRepaired)
            {
                return "radio_repair";
            }
            else if (!_isFirstSignalDiscovered)
            {
                return "signal_discovery";
            }
            else if (!_isFirstLocationVisited)
            {
                return "location_exploration";
            }
            else
            {
                return "main_gameplay";
            }
        }
        
        // Event handlers
        private void OnMessageDecoded(string messageId)
        {
            // Show tutorial message if this is the first decoded message
            if (!_isFirstSignalDiscovered)
            {
                _isFirstSignalDiscovered = true;
                EmitSignal(SignalName.TutorialMessage, "You've decoded your first signal! Keep exploring different frequencies to find more signals.");
                UpdateUIState();
            }
        }
        
        private void OnLocationChanged(string locationId)
        {
            // Show tutorial message if this is the first location change
            if (!_isFirstLocationVisited && locationId != "bunker")
            {
                _isFirstLocationVisited = true;
                EmitSignal(SignalName.TutorialMessage, "You've left the bunker! Explore the area to find more locations and items.");
                UpdateUIState();
            }
        }
        
        private void OnInventoryChanged()
        {
            // Check if radio parts are collected
            if (!_isRadioRepaired && _inventorySystem.HasItem("radio_part"))
            {
                EmitSignal(SignalName.TutorialMessage, "You've found a radio part! Use it to repair your radio.");
            }
        }
        
        private void OnFrequencyChanged(float frequency)
        {
            // No specific behavior for frequency changes
        }
        
        private void OnRadioToggled(bool isOn)
        {
            // Show tutorial message if radio is turned on for the first time
            if (isOn && _isRadioRepaired && !_isFirstSignalDiscovered)
            {
                EmitSignal(SignalName.TutorialMessage, "Your radio is working! Tune to different frequencies to find signals.");
            }
        }
        
        private void OnQuestDiscovered(string questId)
        {
            // Show tutorial message for new quests
            var quest = _questSystem.GetQuest(questId);
            if (quest != null)
            {
                EmitSignal(SignalName.TutorialMessage, $"New quest discovered: {quest.Title}");
            }
        }
        
        private void OnQuestActivated(string questId)
        {
            // No specific behavior for quest activation
        }
        
        private void OnQuestCompleted(string questId)
        {
            // Check if radio repair quest is completed
            if (questId == "quest_radio_repair" && !_isRadioRepaired)
            {
                _isRadioRepaired = true;
                EmitSignal(SignalName.TutorialMessage, "You've repaired your radio! Now you can tune to different frequencies to find signals.");
                UpdateUIState();
            }
        }
        
        private void OnQuestObjectiveCompleted(string questId, string objectiveId)
        {
            // No specific behavior for objective completion
        }
        
        private void OnSignalDiscovered(string signalId)
        {
            // Show tutorial message if this is the first discovered signal
            if (!_isFirstSignalDiscovered)
            {
                _isFirstSignalDiscovered = true;
                EmitSignal(SignalName.TutorialMessage, "You've discovered a signal! Listen to it to decode the message.");
                UpdateUIState();
            }
        }
        
        private void OnSignalDecoded(string signalId)
        {
            // No specific behavior for signal decoding
        }
        
        private void OnItemAdded(string itemId, int quantity)
        {
            // Show tutorial message for key items
            if (itemId == "radio_part")
            {
                EmitSignal(SignalName.TutorialMessage, "You've found a radio part! Use it to repair your radio.");
            }
            else if (itemId == "map_fragment")
            {
                EmitSignal(SignalName.TutorialMessage, "You've found a map fragment! This will reveal new locations on your map.");
            }
            else if (itemId == "key_cabin")
            {
                EmitSignal(SignalName.TutorialMessage, "You've found a key to the cabin! Use it to unlock the cabin door.");
            }
        }
        
        private void OnItemRemoved(string itemId, int quantity)
        {
            // No specific behavior for item removal
        }
        
        private void OnItemUsed(string itemId)
        {
            // Show tutorial message for key items
            if (itemId == "radio_part")
            {
                EmitSignal(SignalName.TutorialMessage, "You've used the radio part to repair your radio!");
            }
            else if (itemId == "key_cabin")
            {
                EmitSignal(SignalName.TutorialMessage, "You've unlocked the cabin door!");
            }
        }
        
        private void OnLocationDiscovered(string locationId)
        {
            // Show tutorial message for new locations
            EmitSignal(SignalName.TutorialMessage, $"New location discovered: {locationId}");
        }
    }
}

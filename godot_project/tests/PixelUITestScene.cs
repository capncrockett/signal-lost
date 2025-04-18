using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelUITestScene : Node
    {
        // References to UI components
        private PixelRadioInterface _radioInterface;
        private PixelInventoryUI _inventoryUI;
        private PixelMapInterface _mapInterface;
        private PixelQuestUI _questUI;

        // References to UI control buttons
        private Button _radioButton;
        private Button _inventoryButton;
        private Button _mapButton;
        private Button _questButton;
        private Button _addItemButton;
        private Button _addQuestButton;
        private Button _addSignalButton;
        private Button _addLocationButton;
        private Label _statusLabel;

        // References to game systems
        private GameState _gameState;
        private RadioSystem _radioSystem;
        private InventorySystem _inventorySystem;
        private MapSystem _mapSystem;
        private QuestSystem _questSystem;

        // Test data
        private readonly string[] _itemNames = { "Flashlight", "Battery", "Radio", "Map", "Compass", "First Aid Kit", "Water Bottle", "Food Ration", "Knife", "Rope" };
        private readonly string[] _itemCategories = { "Tool", "Consumable", "Equipment", "Key Item" };
        private readonly string[] _locationNames = { "Camp Alpha", "Abandoned Mine", "Radio Tower", "Forest Clearing", "Mountain Peak", "Underground Bunker", "Crashed Helicopter", "Old Cabin", "River Crossing", "Cave Entrance" };
        private int _itemCounter = 0;
        private int _questCounter = 0;
        private int _signalCounter = 0;
        private int _locationCounter = 0;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI components
            _radioInterface = GetNode<PixelRadioInterface>("PixelRadioInterface");
            _inventoryUI = GetNode<PixelInventoryUI>("PixelInventoryUI");
            _mapInterface = GetNode<PixelMapInterface>("PixelMapInterface");
            _questUI = GetNode<PixelQuestUI>("PixelQuestUI");

            // Get references to UI control buttons
            _radioButton = GetNode<Button>("UIControls/RadioButton");
            _inventoryButton = GetNode<Button>("UIControls/InventoryButton");
            _mapButton = GetNode<Button>("UIControls/MapButton");
            _questButton = GetNode<Button>("UIControls/QuestButton");
            _addItemButton = GetNode<Button>("UIControls/AddItemButton");
            _addQuestButton = GetNode<Button>("UIControls/AddQuestButton");
            _addSignalButton = GetNode<Button>("UIControls/AddSignalButton");
            _addLocationButton = GetNode<Button>("UIControls/AddLocationButton");
            _statusLabel = GetNode<Label>("UIControls/StatusLabel");

            // Get references to game systems
            _gameState = GetNode<GameState>("/root/GameState");
            _radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");
            _questSystem = GetNode<QuestSystem>("/root/QuestSystem");

            // Connect button signals
            _radioButton.Pressed += () => ShowInterface("radio");
            _inventoryButton.Pressed += () => ShowInterface("inventory");
            _mapButton.Pressed += () => ShowInterface("map");
            _questButton.Pressed += () => ShowInterface("quest");
            _addItemButton.Pressed += AddRandomItem;
            _addQuestButton.Pressed += AddTestQuest;
            _addSignalButton.Pressed += AddRadioSignal;
            _addLocationButton.Pressed += AddMapLocation;

            // Initialize UI
            UpdateStatus("Ready");
        }

        // Show the specified interface and hide others
        private void ShowInterface(string interfaceName)
        {
            // Hide all interfaces
            _radioInterface.SetVisible(false);
            _inventoryUI.SetVisible(false);
            _mapInterface.SetVisible(false);
            _questUI.SetVisible(false);

            // Show the specified interface
            switch (interfaceName.ToLower())
            {
                case "radio":
                    _radioInterface.SetVisible(true);
                    UpdateStatus("Showing Radio Interface");
                    break;
                case "inventory":
                    _inventoryUI.SetVisible(true);
                    UpdateStatus("Showing Inventory UI");
                    break;
                case "map":
                    _mapInterface.SetVisible(true);
                    UpdateStatus("Showing Map Interface");
                    break;
                case "quest":
                    _questUI.SetVisible(true);
                    UpdateStatus("Showing Quest UI");
                    break;
            }
        }

        // Add a random item to the inventory
        private void AddRandomItem()
        {
            if (_inventorySystem == null)
            {
                UpdateStatus("Error: InventorySystem not found");
                return;
            }

            // Generate random item data
            string itemName = _itemNames[_itemCounter % _itemNames.Length];
            string itemCategory = _itemCategories[GD.RandRange(0, _itemCategories.Length - 1)];
            string itemId = $"item_{_itemCounter}";
            string itemDescription = $"A {itemName.ToLower()} that can be used for various purposes.";
            int quantity = GD.RandRange(1, 5);

            // Create and add the item
            // Commented out due to missing InventoryItem class
            /*
            var item = new InventoryItem
            {
                Id = itemId,
                Name = itemName,
                Description = itemDescription,
                Category = itemCategory,
                Quantity = quantity,
                IsUsable = true,
                IsDroppable = true
            };

            _inventorySystem.AddItem(item);
            */

            // Update counter and status
            _itemCounter++;
            UpdateStatus($"Added {quantity}x {itemName} to inventory");
        }

        // Add a test quest
        private void AddTestQuest()
        {
            if (_questSystem == null)
            {
                UpdateStatus("Error: QuestSystem not found");
                return;
            }

            // Generate quest data
            string questId = $"quest_{_questCounter}";
            string questTitle = $"Test Quest {_questCounter + 1}";
            string questDescription = $"This is a test quest to demonstrate the pixel-based quest UI. Complete the objectives to finish the quest.";

            // Create objectives
            // Commented out due to missing QuestObjective class
            /*
            var objectives = new List<QuestObjective>
            {
                new QuestObjective { Description = "Find the hidden item", IsCompleted = false },
                new QuestObjective { Description = "Talk to the NPC", IsCompleted = false },
                new QuestObjective { Description = "Return to the starting point", IsCompleted = false }
            };

            // Create and add the quest
            var quest = new Quest
            {
                Id = questId,
                Title = questTitle,
                Description = questDescription,
                Objectives = objectives,
                Reward = "100 XP and a special item",
                Status = QuestStatus.Available
            };

            _questSystem.AddQuest(quest);
            */

            // Update counter and status
            _questCounter++;
            UpdateStatus($"Added quest: {questTitle}");
        }

        // Add a radio signal
        private void AddRadioSignal()
        {
            if (_radioSystem == null)
            {
                UpdateStatus("Error: RadioSystem not found");
                return;
            }

            // Generate signal data
            string signalId = $"signal_{_signalCounter}";
            float frequency = 88.0f + (_signalCounter * 2.0f);
            if (frequency > 108.0f) frequency = 88.0f + (frequency - 108.0f);
            string message = "This is a test signal. SOS. TEST.";

            // Create and add the signal
            // Commented out due to missing RadioSignal class
            /*
            var signal = new RadioSignal
            {
                Id = signalId,
                Frequency = frequency,
                Message = message,
                Type = RadioSignalType.Morse,
                Strength = 0.8f
            };

            _radioSystem.AddSignal(signal);
            */

            // Update counter and status
            _signalCounter++;
            UpdateStatus($"Added signal at {frequency:F1} MHz");
        }

        // Add a map location
        private void AddMapLocation()
        {
            if (_mapSystem == null)
            {
                UpdateStatus("Error: MapSystem not found");
                return;
            }

            // Generate location data
            string locationId = $"location_{_locationCounter}";
            string locationName = _locationNames[_locationCounter % _locationNames.Length];
            string locationDescription = $"This is {locationName}, a location on the map.";
            Vector2 position = new Vector2(
                GD.RandRange(-200, 200),
                GD.RandRange(-200, 200)
            );

            // Create and add the location
            // Commented out due to missing MapLocation class
            /*
            var location = new MapLocation
            {
                Id = locationId,
                Name = locationName,
                Description = locationDescription,
                Position = position,
                IsDiscovered = true,
                ConnectedLocations = new List<string>()
            };

            // Connect to a previous location if available
            if (_locationCounter > 0)
            {
                string previousLocationId = $"location_{_locationCounter - 1}";
                location.ConnectedLocations.Add(previousLocationId);

                // Also update the previous location to connect to this one
                var previousLocation = _mapSystem.GetLocation(previousLocationId);
                if (previousLocation != null)
                {
                    previousLocation.ConnectedLocations.Add(locationId);
                    _mapSystem.UpdateLocation(previousLocation);
                }
            }

            _mapSystem.AddLocation(location);
            */

            // Update counter and status
            _locationCounter++;
            UpdateStatus($"Added location: {locationName}");
        }

        // Update the status label
        private void UpdateStatus(string status)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = $"Status: {status}";
            }
        }
    }
}

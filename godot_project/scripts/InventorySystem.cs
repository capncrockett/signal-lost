using Godot;
using System.Collections.Generic;
using System.Linq;

namespace SignalLost
{
    [GlobalClass]
    public partial class InventorySystem : Node
    {
        // Item data
        public class ItemData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Category { get; set; } // e.g., "tool", "key", "document", "consumable"
            public bool IsUsable { get; set; }
            public bool IsConsumable { get; set; }
            public int Quantity { get; set; } = 1;
            public string IconPath { get; set; } = "res://assets/images/items/default_item.png";
        }

        // Dictionary of all items in the game
        private Dictionary<string, ItemData> _itemDatabase = new Dictionary<string, ItemData>();

        // Player's inventory
        private Dictionary<string, ItemData> _inventory = new Dictionary<string, ItemData>();

        // Maximum inventory capacity
        private int _maxInventorySize = 20;

        // Reference to the GameState
        private GameState _gameState;

        // Signals
        [Signal]
        public delegate void ItemAddedEventHandler(string itemId, int quantity);

        [Signal]
        public delegate void ItemRemovedEventHandler(string itemId, int quantity);

        [Signal]
        public delegate void ItemUsedEventHandler(string itemId);

        [Signal]
        public delegate void InventoryFullEventHandler();

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get reference to GameState
            _gameState = GetNode<GameState>("/root/GameState");
            if (_gameState == null)
            {
                GD.PrintErr("InventorySystem: Failed to get GameState reference");
                return;
            }

            // Initialize item database
            InitializeItemDatabase();

            // Sync with GameState's inventory
            SyncWithGameState();
        }

        // Initialize the item database with all available items in the game
        private void InitializeItemDatabase()
        {
            // Add items to the database
            AddItemToDatabase(new ItemData
            {
                Id = "flashlight",
                Name = "Flashlight",
                Description = "A battery-powered flashlight. Essential for dark areas.",
                Category = "tool",
                IsUsable = true,
                IsConsumable = false,
                IconPath = "res://assets/images/items/flashlight.png"
            });

            AddItemToDatabase(new ItemData
            {
                Id = "battery",
                Name = "Battery",
                Description = "A standard AA battery. Can be used to power various devices.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IconPath = "res://assets/images/items/battery.png"
            });

            AddItemToDatabase(new ItemData
            {
                Id = "medkit",
                Name = "First Aid Kit",
                Description = "A basic first aid kit containing bandages and antiseptics.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IconPath = "res://assets/images/items/medkit.png"
            });

            AddItemToDatabase(new ItemData
            {
                Id = "key_cabin",
                Name = "Cabin Key",
                Description = "A rusty key that might open a cabin door.",
                Category = "key",
                IsUsable = true,
                IsConsumable = false,
                IconPath = "res://assets/images/items/key.png"
            });

            AddItemToDatabase(new ItemData
            {
                Id = "map_fragment",
                Name = "Map Fragment",
                Description = "A torn piece of a map showing part of the area.",
                Category = "document",
                IsUsable = true,
                IsConsumable = false,
                IconPath = "res://assets/images/items/map_fragment.png"
            });

            AddItemToDatabase(new ItemData
            {
                Id = "radio_part",
                Name = "Radio Component",
                Description = "A component that could be used to repair or upgrade a radio.",
                Category = "tool",
                IsUsable = false,
                IsConsumable = false,
                IconPath = "res://assets/images/items/radio_part.png"
            });

            AddItemToDatabase(new ItemData
            {
                Id = "water_bottle",
                Name = "Water Bottle",
                Description = "A bottle of clean drinking water.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IconPath = "res://assets/images/items/water_bottle.png"
            });

            AddItemToDatabase(new ItemData
            {
                Id = "canned_food",
                Name = "Canned Food",
                Description = "A can of preserved food. Not tasty, but nutritious.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IconPath = "res://assets/images/items/canned_food.png"
            });
        }

        // Add an item to the database
        private void AddItemToDatabase(ItemData item)
        {
            _itemDatabase[item.Id] = item;
        }

        // Sync with GameState's inventory
        private void SyncWithGameState()
        {
            // Clear current inventory
            _inventory.Clear();

            // Add items from GameState's inventory
            foreach (var itemId in _gameState.Inventory)
            {
                if (_itemDatabase.ContainsKey(itemId))
                {
                    AddItemToInventory(itemId, 1, false); // Don't update GameState to avoid circular updates
                }
            }
        }

        // Update GameState's inventory
        private void UpdateGameState()
        {
            // Clear GameState's inventory
            _gameState.ClearInventory();

            // Add items to GameState's inventory
            foreach (var item in _inventory.Values)
            {
                for (int i = 0; i < item.Quantity; i++)
                {
                    _gameState.AddToInventory(item.Id);
                }
            }
        }

        // Get an item from the database by ID
        public ItemData GetItem(string itemId)
        {
            if (_itemDatabase.ContainsKey(itemId))
            {
                return _itemDatabase[itemId];
            }
            return null;
        }

        // Get all items in the database
        public Dictionary<string, ItemData> GetAllItems()
        {
            return _itemDatabase;
        }

        // Get all items in the player's inventory
        public Dictionary<string, ItemData> GetInventory()
        {
            return _inventory;
        }

        // Get the number of unique items in the inventory
        public int GetInventoryCount()
        {
            return _inventory.Count;
        }

        // Get the total number of items in the inventory (including stacked items)
        public int GetTotalItemCount()
        {
            return _inventory.Values.Sum(item => item.Quantity);
        }

        // Check if the inventory is full
        public bool IsInventoryFull()
        {
            return GetTotalItemCount() >= _maxInventorySize;
        }

        // Add an item to the inventory
        public bool AddItemToInventory(string itemId, int quantity = 1, bool updateGameState = true)
        {
            // Check if the item exists in the database
            if (!_itemDatabase.ContainsKey(itemId))
            {
                GD.PrintErr($"InventorySystem: Item {itemId} not found in the database");
                return false;
            }

            // Check if adding this item would exceed the inventory capacity
            if (GetTotalItemCount() + quantity > _maxInventorySize)
            {
                EmitSignal(SignalName.InventoryFull);
                return false;
            }

            // If the item is already in the inventory, increase the quantity
            if (_inventory.ContainsKey(itemId))
            {
                _inventory[itemId].Quantity += quantity;
            }
            else
            {
                // Otherwise, add a new item to the inventory
                var newItem = new ItemData
                {
                    Id = _itemDatabase[itemId].Id,
                    Name = _itemDatabase[itemId].Name,
                    Description = _itemDatabase[itemId].Description,
                    Category = _itemDatabase[itemId].Category,
                    IsUsable = _itemDatabase[itemId].IsUsable,
                    IsConsumable = _itemDatabase[itemId].IsConsumable,
                    Quantity = quantity,
                    IconPath = _itemDatabase[itemId].IconPath
                };

                _inventory[itemId] = newItem;
            }

            // Update GameState
            if (updateGameState)
            {
                UpdateGameState();
            }

            // Emit signal
            EmitSignal(SignalName.ItemAdded, itemId, quantity);

            return true;
        }

        // Remove an item from the inventory
        public bool RemoveItemFromInventory(string itemId, int quantity = 1)
        {
            // Check if the item is in the inventory
            if (!_inventory.ContainsKey(itemId))
            {
                GD.PrintErr($"InventorySystem: Item {itemId} not found in the inventory");
                return false;
            }

            // Check if there are enough items to remove
            if (_inventory[itemId].Quantity < quantity)
            {
                GD.PrintErr($"InventorySystem: Not enough items to remove (have {_inventory[itemId].Quantity}, want to remove {quantity})");
                return false;
            }

            // Decrease the quantity
            _inventory[itemId].Quantity -= quantity;

            // If the quantity is now 0, remove the item from the inventory
            if (_inventory[itemId].Quantity <= 0)
            {
                _inventory.Remove(itemId);
            }

            // Update GameState
            UpdateGameState();

            // Emit signal
            EmitSignal(SignalName.ItemRemoved, itemId, quantity);

            return true;
        }

        // Use an item
        public bool UseItem(string itemId)
        {
            // Check if the item is in the inventory
            if (!_inventory.ContainsKey(itemId))
            {
                GD.PrintErr($"InventorySystem: Item {itemId} not found in the inventory");
                return false;
            }

            // Check if the item is usable
            if (!_inventory[itemId].IsUsable)
            {
                GD.PrintErr($"InventorySystem: Item {itemId} is not usable");
                return false;
            }

            // Emit signal
            EmitSignal(SignalName.ItemUsed, itemId);

            // If the item is consumable, remove it from the inventory
            if (_inventory[itemId].IsConsumable)
            {
                RemoveItemFromInventory(itemId, 1);
            }

            return true;
        }

        // Get items by category
        public List<ItemData> GetItemsByCategory(string category)
        {
            return _inventory.Values.Where(item => item.Category == category).ToList();
        }

        // Check if the player has a specific item
        public bool HasItem(string itemId, int quantity = 1)
        {
            return _inventory.ContainsKey(itemId) && _inventory[itemId].Quantity >= quantity;
        }

        // Set the maximum inventory size
        public void SetMaxInventorySize(int size)
        {
            _maxInventorySize = size;
        }

        // Get the maximum inventory size
        public int GetMaxInventorySize()
        {
            return _maxInventorySize;
        }
    }
}

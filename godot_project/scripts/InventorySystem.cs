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
            public bool IsEquippable { get; set; }
            public bool IsCombineable { get; set; }
            public int Quantity { get; set; } = 1;
            public string IconPath { get; set; } = "res://assets/images/items/default_item.png";
            public int IconIndex { get; set; } = 0; // Index in the icon atlas
            public string Content { get; set; } = ""; // For documents or notes
            public List<string> CombinesWith { get; set; } = new List<string>(); // Items this can be combined with
            public string CombinationResult { get; set; } = ""; // Result of combining with another item
            public Dictionary<string, float> Effects { get; set; } = new Dictionary<string, float>(); // Effects when used (e.g., "health", 20)
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
        public delegate void ItemEquippedEventHandler(string itemId);

        [Signal]
        public delegate void ItemUnequippedEventHandler(string itemId);

        [Signal]
        public delegate void ItemsCombinedEventHandler(string item1Id, string item2Id, string resultId);

        [Signal]
        public delegate void ItemEffectAppliedEventHandler(string itemId, string effectType, float value);

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
            // Basic Tools
            AddItemToDatabase(new ItemData
            {
                Id = "flashlight",
                Name = "Flashlight",
                Description = "A battery-powered flashlight. Essential for dark areas.",
                Category = "tool",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/flashlight.png",
                IconIndex = 0,
                Effects = new Dictionary<string, float> { { "light", 1.0f } }
            });

            AddItemToDatabase(new ItemData
            {
                Id = "radio",
                Name = "Portable Radio",
                Description = "A battery-powered radio that can receive signals.",
                Category = "tool",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/radio.png",
                IconIndex = 1,
                Effects = new Dictionary<string, float> { { "signal_detection", 1.0f } }
            });

            AddItemToDatabase(new ItemData
            {
                Id = "map",
                Name = "Area Map",
                Description = "A map of the surrounding area. Shows known locations.",
                Category = "tool",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/map.png",
                IconIndex = 2,
                Effects = new Dictionary<string, float> { { "navigation", 1.0f } }
            });

            // Consumables
            AddItemToDatabase(new ItemData
            {
                Id = "battery",
                Name = "Battery",
                Description = "A standard AA battery. Can be used to power various devices.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IsEquippable = false,
                IconPath = "res://assets/images/items/battery.png",
                IconIndex = 3,
                Effects = new Dictionary<string, float> { { "power", 100.0f } }
            });

            AddItemToDatabase(new ItemData
            {
                Id = "medkit",
                Name = "First Aid Kit",
                Description = "Used to treat injuries and restore health.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IsEquippable = false,
                IconPath = "res://assets/images/items/medkit.png",
                IconIndex = 4,
                Effects = new Dictionary<string, float> { { "health", 50.0f } }
            });

            AddItemToDatabase(new ItemData
            {
                Id = "water_bottle",
                Name = "Water Bottle",
                Description = "Clean drinking water. Essential for survival.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IsEquippable = false,
                IconPath = "res://assets/images/items/water_bottle.png",
                IconIndex = 6,
                Effects = new Dictionary<string, float> { { "hydration", 40.0f } }
            });

            AddItemToDatabase(new ItemData
            {
                Id = "canned_food",
                Name = "Canned Food",
                Description = "A can of preserved food. Not tasty, but nutritious.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IsEquippable = false,
                IconPath = "res://assets/images/items/canned_food.png",
                IconIndex = 5,
                Effects = new Dictionary<string, float> { { "energy", 30.0f } }
            });

            // Key Items
            AddItemToDatabase(new ItemData
            {
                Id = "key_cabin",
                Name = "Cabin Key",
                Description = "A rusty key that might open a cabin door.",
                Category = "key",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/key.png",
                IconIndex = 7,
                Effects = new Dictionary<string, float> { { "unlock_cabin", 1.0f } }
            });

            AddItemToDatabase(new ItemData
            {
                Id = "keycard_research",
                Name = "Research Facility Keycard",
                Description = "A keycard that grants access to the research facility.",
                Category = "key",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/keycard.png",
                IconIndex = 8,
                Effects = new Dictionary<string, float> { { "unlock_research_facility", 1.0f } }
            });

            AddItemToDatabase(new ItemData
            {
                Id = "military_badge",
                Name = "Military ID Badge",
                Description = "An ID badge that grants access to military installations.",
                Category = "key",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/badge.png",
                IconIndex = 9,
                Effects = new Dictionary<string, float> { { "unlock_military_outpost", 1.0f } }
            });

            // Documents
            AddItemToDatabase(new ItemData
            {
                Id = "map_fragment",
                Name = "Map Fragment",
                Description = "A torn piece of a map showing part of the area.",
                Category = "document",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/map_fragment.png",
                IconIndex = 10,
                Content = "This appears to be part of a larger map. It shows a path leading to a structure labeled 'Research'."
            });

            AddItemToDatabase(new ItemData
            {
                Id = "research_notes",
                Name = "Research Notes",
                Description = "Notes detailing experiments conducted at the research facility.",
                Category = "document",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/document.png",
                IconIndex = 11,
                Content = "Project SIGNAL: Attempt to establish communication across [REDACTED]. Initial tests show promising results but significant side effects including [REDACTED] and temporal anomalies."
            });

            // Components
            AddItemToDatabase(new ItemData
            {
                Id = "radio_part",
                Name = "Radio Component",
                Description = "A component that could be used to repair or upgrade a radio.",
                Category = "component",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IsCombineable = true,
                IconPath = "res://assets/images/items/radio_part.png",
                IconIndex = 12,
                CombinesWith = new List<string> { "radio_broken" },
                CombinationResult = "radio"
            });

            AddItemToDatabase(new ItemData
            {
                Id = "radio_broken",
                Name = "Broken Radio",
                Description = "A damaged radio that needs repair.",
                Category = "component",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IsCombineable = true,
                IconPath = "res://assets/images/items/radio_broken.png",
                IconIndex = 13,
                CombinesWith = new List<string> { "radio_part" },
                CombinationResult = "radio"
            });

            AddItemToDatabase(new ItemData
            {
                Id = "signal_amplifier",
                Name = "Signal Amplifier",
                Description = "A device that can boost radio signals.",
                Category = "component",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IsCombineable = true,
                IconPath = "res://assets/images/items/amplifier.png",
                IconIndex = 14,
                CombinesWith = new List<string> { "radio" },
                CombinationResult = "radio_enhanced"
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
                    IsEquippable = _itemDatabase[itemId].IsEquippable,
                    IsCombineable = _itemDatabase[itemId].IsCombineable,
                    Quantity = quantity,
                    IconPath = _itemDatabase[itemId].IconPath,
                    IconIndex = _itemDatabase[itemId].IconIndex,
                    Content = _itemDatabase[itemId].Content,
                    CombinesWith = new List<string>(_itemDatabase[itemId].CombinesWith),
                    CombinationResult = _itemDatabase[itemId].CombinationResult,
                    Effects = new Dictionary<string, float>(_itemDatabase[itemId].Effects)
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

            // Apply item effects
            foreach (var effect in _inventory[itemId].Effects)
            {
                ApplyItemEffect(itemId, effect.Key, effect.Value);
                GD.Print($"Applied effect {effect.Key} with value {effect.Value} from item {itemId}");
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

        // Set GameState reference (for testing)
        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
        }

        // Equip an item
        public bool EquipItem(string itemId)
        {
            // Check if the item is in the inventory
            if (!_inventory.ContainsKey(itemId))
            {
                GD.PrintErr($"InventorySystem: Item {itemId} not found in the inventory");
                return false;
            }

            // Check if the item is equippable
            if (!_inventory[itemId].IsEquippable)
            {
                GD.PrintErr($"InventorySystem: Item {itemId} is not equippable");
                return false;
            }

            // Emit signal
            EmitSignal(SignalName.ItemEquipped, itemId);

            return true;
        }

        // Unequip an item
        public bool UnequipItem(string itemId)
        {
            // Check if the item is in the inventory
            if (!_inventory.ContainsKey(itemId))
            {
                GD.PrintErr($"InventorySystem: Item {itemId} not found in the inventory");
                return false;
            }

            // Check if the item is equippable
            if (!_inventory[itemId].IsEquippable)
            {
                GD.PrintErr($"InventorySystem: Item {itemId} is not equippable");
                return false;
            }

            // Emit signal
            EmitSignal(SignalName.ItemUnequipped, itemId);

            return true;
        }

        // Combine two items
        public bool CombineItems(string item1Id, string item2Id)
        {
            // Check if both items are in the inventory
            if (!_inventory.ContainsKey(item1Id) || !_inventory.ContainsKey(item2Id))
            {
                GD.PrintErr($"InventorySystem: One or both items not found in the inventory");
                return false;
            }

            // Check if the items are combineable
            if (!_inventory[item1Id].IsCombineable || !_inventory[item2Id].IsCombineable)
            {
                GD.PrintErr($"InventorySystem: One or both items are not combineable");
                return false;
            }

            // Check if the items can be combined with each other
            if (!_inventory[item1Id].CombinesWith.Contains(item2Id) && !_inventory[item2Id].CombinesWith.Contains(item1Id))
            {
                GD.PrintErr($"InventorySystem: These items cannot be combined");
                return false;
            }

            // Get the result item ID
            string resultItemId = "";
            if (_inventory[item1Id].CombinesWith.Contains(item2Id))
            {
                resultItemId = _inventory[item1Id].CombinationResult;
            }
            else
            {
                resultItemId = _inventory[item2Id].CombinationResult;
            }

            // Check if the result item exists in the database
            if (!_itemDatabase.ContainsKey(resultItemId))
            {
                GD.PrintErr($"InventorySystem: Result item {resultItemId} not found in the database");
                return false;
            }

            // Remove the combined items
            RemoveItemFromInventory(item1Id, 1);
            RemoveItemFromInventory(item2Id, 1);

            // Add the result item
            AddItemToInventory(resultItemId, 1);

            // Emit signal
            EmitSignal(SignalName.ItemsCombined, item1Id, item2Id, resultItemId);

            return true;
        }

        // Apply item effects
        public Dictionary<string, float> GetItemEffects(string itemId)
        {
            // Check if the item is in the inventory
            if (!_inventory.ContainsKey(itemId))
            {
                GD.PrintErr($"InventorySystem: Item {itemId} not found in the inventory");
                return new Dictionary<string, float>();
            }

            // Return the item's effects
            return _inventory[itemId].Effects;
        }

        // Apply an item effect
        public void ApplyItemEffect(string itemId, string effectType, float value)
        {
            // Emit signal
            EmitSignal(SignalName.ItemEffectApplied, itemId, effectType, value);
        }

        // Get item content (for documents)
        public string GetItemContent(string itemId)
        {
            // Check if the item is in the inventory
            if (!_inventory.ContainsKey(itemId))
            {
                GD.PrintErr($"InventorySystem: Item {itemId} not found in the inventory");
                return "";
            }

            // Return the item's content
            return _inventory[itemId].Content;
        }
    }
}

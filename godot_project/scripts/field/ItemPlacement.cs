using Godot;
using System.Collections.Generic;
using SignalLost.Inventory;

namespace SignalLost.Field
{
    /// <summary>
    /// Manages the placement of items in the game world.
    /// </summary>
    [GlobalClass]
    public partial class ItemPlacement : Node
    {
        // Dictionary of items placed in the world
        private Dictionary<string, PlacedItem> _placedItems = new Dictionary<string, PlacedItem>();

        // Reference to the inventory system
        private InventorySystemAdapter _inventorySystem;

        // Reference to the game state
        private GameState _gameState;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to game systems
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_inventorySystem == null || _gameState == null)
            {
                GD.PrintErr("ItemPlacement: Failed to get references to game systems");
                return;
            }

            // Initialize placed items
            InitializePlacedItems();
        }

        // Initialize placed items
        private void InitializePlacedItems()
        {
            // Bunker items
            AddPlacedItem("flashlight", "bunker", "desk", "A flashlight on the desk.");
            AddPlacedItem("battery", "bunker", "drawer", "A battery in the drawer.");
            AddPlacedItem("radio_broken", "bunker", "shelf", "A broken radio on the shelf.");
            AddPlacedItem("map_fragment", "bunker", "wall", "A torn map fragment pinned to the wall.");

            // Forest items
            AddPlacedItem("medkit", "forest", "fallen_tree", "A first aid kit near a fallen tree.");
            AddPlacedItem("water_bottle", "forest", "stream", "A water bottle by the stream.");
            AddPlacedItem("key_cabin", "forest", "stump", "A key hidden in a hollow tree stump.");

            // Cabin items
            AddPlacedItem("radio_part", "cabin", "workbench", "A radio component on the workbench.");
            AddPlacedItem("canned_food", "cabin", "cupboard", "Canned food in the cupboard.");
            AddPlacedItem("research_notes", "cabin", "desk", "Research notes on the desk.");
            AddPlacedItem("keycard_research", "cabin", "safe", "A keycard in the safe.");

            // Research facility items
            AddPlacedItem("signal_amplifier", "research_facility", "lab_table", "A signal amplifier on the lab table.");
            AddPlacedItem("military_badge", "research_facility", "locker", "A military badge in a locker.");
            AddPlacedItem("strange_crystal", "research_facility", "containment", "A strange crystal in a containment unit.");

            GD.Print($"ItemPlacement: Initialized {_placedItems.Count} placed items");
        }

        // Add a placed item
        private void AddPlacedItem(string itemId, string locationId, string containerName, string description)
        {
            string placedItemId = $"{locationId}_{containerName}_{itemId}";

            _placedItems[placedItemId] = new PlacedItem
            {
                ItemId = itemId,
                LocationId = locationId,
                ContainerName = containerName,
                Description = description,
                IsCollected = false
            };
        }

        // Get all placed items in a location
        public List<PlacedItem> GetPlacedItemsInLocation(string locationId)
        {
            List<PlacedItem> result = new List<PlacedItem>();

            foreach (var item in _placedItems.Values)
            {
                if (item.LocationId == locationId && !item.IsCollected)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        // Get a placed item by ID
        public PlacedItem GetPlacedItem(string placedItemId)
        {
            if (_placedItems.ContainsKey(placedItemId))
            {
                return _placedItems[placedItemId];
            }

            return null;
        }

        // Collect a placed item
        public bool CollectItem(string placedItemId)
        {
            // Check if the item exists
            if (!_placedItems.ContainsKey(placedItemId))
            {
                GD.PrintErr($"ItemPlacement: Item {placedItemId} not found");
                return false;
            }

            // Check if the item has already been collected
            if (_placedItems[placedItemId].IsCollected)
            {
                GD.Print($"ItemPlacement: Item {placedItemId} has already been collected");
                return false;
            }

            // Get the item
            PlacedItem placedItem = _placedItems[placedItemId];

            // Add the item to the inventory
            bool success = _inventorySystem.AddItemToInventory(placedItem.ItemId);

            if (success)
            {
                // Mark the item as collected
                placedItem.IsCollected = true;
                _placedItems[placedItemId] = placedItem;

                // Update quest progress if applicable
                UpdateQuestProgress(placedItem.ItemId);

                GD.Print($"ItemPlacement: Collected item {placedItem.ItemId} from {placedItem.LocationId}/{placedItem.ContainerName}");

                return true;
            }

            return false;
        }

        // Update quest progress based on collected item
        private void UpdateQuestProgress(string itemId)
        {
            // Get the quest system
            var questSystem = GetNode<QuestSystem>("/root/QuestSystem");

            if (questSystem == null)
            {
                GD.PrintErr("ItemPlacement: Failed to get QuestSystem reference");
                return;
            }

            // Update quest progress based on the item
            switch (itemId)
            {
                case "radio_part":
                    questSystem.UpdateQuestObjective("quest_radio_repair", "find_radio_part");
                    break;

                case "map_fragment":
                    questSystem.UpdateQuestObjective("quest_explore_forest", "find_map_fragment");
                    break;

                case "key_cabin":
                    questSystem.UpdateQuestObjective("quest_explore_forest", "find_cabin_key");
                    break;

                case "keycard_research":
                    questSystem.UpdateQuestObjective("quest_radio_signals", "find_research_keycard");
                    break;

                case "military_badge":
                    questSystem.UpdateQuestObjective("quest_radio_signals", "find_military_badge");
                    break;

                case "strange_crystal":
                    questSystem.UpdateQuestObjective("quest_radio_signals", "find_strange_crystal");
                    break;
            }
        }

        // Reset all placed items (for new game)
        public void ResetPlacedItems()
        {
            foreach (var key in _placedItems.Keys)
            {
                var item = _placedItems[key];
                item.IsCollected = false;
                _placedItems[key] = item;
            }

            GD.Print("ItemPlacement: Reset all placed items");
        }

        // Save placed items state
        public Dictionary<string, bool> SavePlacedItemsState()
        {
            Dictionary<string, bool> state = new Dictionary<string, bool>();

            foreach (var key in _placedItems.Keys)
            {
                state[key] = _placedItems[key].IsCollected;
            }

            return state;
        }

        // Load placed items state
        public void LoadPlacedItemsState(Dictionary<string, bool> state)
        {
            foreach (var key in state.Keys)
            {
                if (_placedItems.ContainsKey(key))
                {
                    var item = _placedItems[key];
                    item.IsCollected = state[key];
                    _placedItems[key] = item;
                }
            }

            GD.Print("ItemPlacement: Loaded placed items state");
        }
    }

    /// <summary>
    /// Represents an item placed in the game world.
    /// </summary>
    public class PlacedItem
    {
        public string ItemId;
        public string LocationId;
        public string ContainerName;
        public string Description;
        public bool IsCollected;
    }
}

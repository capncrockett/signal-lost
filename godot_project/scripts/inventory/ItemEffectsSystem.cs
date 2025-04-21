using Godot;
using System.Collections.Generic;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Handles item effects and interactions.
    /// </summary>
    [GlobalClass]
    public partial class ItemEffectsSystem : Node
    {
        // Reference to the inventory core
        private InventoryCore _inventoryCore;

        // Reference to the GameState
        private GameState _gameState;

        // Signals
        [Signal]
        public delegate void ItemUsedEventHandler(string itemId);

        [Signal]
        public delegate void ItemEquippedEventHandler(string itemId);

        [Signal]
        public delegate void ItemUnequippedEventHandler(string itemId);

        [Signal]
        public delegate void ItemEffectAppliedEventHandler(string itemId, string effectType, float value);

        // Currently equipped items by category
        private Dictionary<string, string> _equippedItems = new Dictionary<string, string>();

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get the InventoryCore
            _inventoryCore = GetNode<InventoryCore>("/root/InventoryCore");

            if (_inventoryCore == null)
            {
                GD.PrintErr("ItemEffectsSystem: InventoryCore not found");
                return;
            }

            // Get the GameState
            _gameState = GetNode<GameState>("/root/GameState");

            if (_gameState == null)
            {
                GD.PrintErr("ItemEffectsSystem: GameState not found");
                return;
            }
        }

        // Use an item
        public bool UseItem(string itemId)
        {
            // Check if the item is in the inventory
            if (!_inventoryCore.HasItem(itemId))
            {
                GD.PrintErr($"ItemEffectsSystem: Item {itemId} not found in the inventory");
                return false;
            }

            // Get the item
            var item = _inventoryCore.GetItem(itemId);

            // Check if the item is usable
            if (!item.IsUsable)
            {
                GD.PrintErr($"ItemEffectsSystem: Item {itemId} is not usable");
                return false;
            }

            // Apply item effects
            foreach (var effect in item.Effects)
            {
                ApplyItemEffect(itemId, effect.Key, effect.Value);
                GD.Print($"Applied effect {effect.Key} with value {effect.Value} from item {itemId}");
            }

            // Emit signal
            EmitSignal(SignalName.ItemUsed, itemId);

            // If the item is consumable, remove it from the inventory
            if (item.IsConsumable)
            {
                _inventoryCore.RemoveItemFromInventory(itemId, 1);
            }

            return true;
        }

        // Equip an item
        public bool EquipItem(string itemId)
        {
            // Check if the item is in the inventory
            if (!_inventoryCore.HasItem(itemId))
            {
                GD.PrintErr($"ItemEffectsSystem: Item {itemId} not found in the inventory");
                return false;
            }

            // Get the item
            var item = _inventoryCore.GetItem(itemId);

            // Check if the item is equippable
            if (!item.IsEquippable)
            {
                GD.PrintErr($"ItemEffectsSystem: Item {itemId} is not equippable");
                return false;
            }

            // Unequip any item in the same category
            if (_equippedItems.ContainsKey(item.Category))
            {
                UnequipItem(_equippedItems[item.Category]);
            }

            // Equip the item
            _equippedItems[item.Category] = itemId;

            // Emit signal
            EmitSignal(SignalName.ItemEquipped, itemId);

            return true;
        }

        // Unequip an item
        public bool UnequipItem(string itemId)
        {
            // Check if the item is in the inventory
            if (!_inventoryCore.HasItem(itemId))
            {
                GD.PrintErr($"ItemEffectsSystem: Item {itemId} not found in the inventory");
                return false;
            }

            // Get the item
            var item = _inventoryCore.GetItem(itemId);

            // Check if the item is equippable
            if (!item.IsEquippable)
            {
                GD.PrintErr($"ItemEffectsSystem: Item {itemId} is not equippable");
                return false;
            }

            // Check if the item is equipped
            if (!_equippedItems.ContainsKey(item.Category) || _equippedItems[item.Category] != itemId)
            {
                GD.PrintErr($"ItemEffectsSystem: Item {itemId} is not equipped");
                return false;
            }

            // Unequip the item
            _equippedItems.Remove(item.Category);

            // Emit signal
            EmitSignal(SignalName.ItemUnequipped, itemId);

            return true;
        }

        // Apply an item effect
        public void ApplyItemEffect(string itemId, string effectType, float value)
        {
            // Handle different effect types
            switch (effectType)
            {
                case "health":
                    // Apply health effect
                    // This would be implemented in a health system
                    break;

                case "energy":
                    // Apply energy effect
                    // This would be implemented in an energy system
                    break;

                case "hydration":
                    // Apply hydration effect
                    // This would be implemented in a survival system
                    break;

                case "light":
                    // Apply light effect
                    // This would be implemented in a lighting system
                    break;

                case "signal_detection":
                    // Apply signal detection effect
                    // This would be implemented in a radio system
                    break;

                case "navigation":
                    // Apply navigation effect
                    // This would be implemented in a map system
                    break;

                case "unlock_cabin":
                case "unlock_research_facility":
                case "unlock_military_outpost":
                case "unlock_mysterious_tower":
                    // Apply unlock effect
                    // This would be implemented in a location system
                    break;

                default:
                    GD.Print($"Unknown effect type: {effectType}");
                    break;
            }

            // Emit signal
            EmitSignal(SignalName.ItemEffectApplied, itemId, effectType, value);
        }

        // Get item content (for documents)
        public string GetItemContent(string itemId)
        {
            // Check if the item is in the inventory
            if (!_inventoryCore.HasItem(itemId))
            {
                GD.PrintErr($"ItemEffectsSystem: Item {itemId} not found in the inventory");
                return "";
            }

            // Get the item
            var item = _inventoryCore.GetItem(itemId);

            // Return the item's content
            return item.Content;
        }

        // Get equipped item in a category
        public string GetEquippedItem(string category)
        {
            if (_equippedItems.ContainsKey(category))
            {
                return _equippedItems[category];
            }

            return null;
        }

        // Check if an item is equipped
        public bool IsItemEquipped(string itemId)
        {
            // Get the item
            var item = _inventoryCore.GetItem(itemId);

            if (item == null)
            {
                return false;
            }

            // Check if the item is equipped
            return _equippedItems.ContainsKey(item.Category) && _equippedItems[item.Category] == itemId;
        }

        // Get all equipped items
        public Dictionary<string, string> GetEquippedItems()
        {
            return _equippedItems;
        }
    }
}

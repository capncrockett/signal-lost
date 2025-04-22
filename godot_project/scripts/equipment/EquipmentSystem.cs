using Godot;
using System;
using System.Collections.Generic;
using SignalLost.Inventory;

namespace SignalLost.Equipment
{
    /// <summary>
    /// Manages equipment slots, equipped items, and their effects.
    /// </summary>
    [GlobalClass]
    public partial class EquipmentSystem : Node
    {
        // Equipment slots
        public enum EquipmentSlot
        {
            Head,       // Headphones, helmets, etc.
            Body,       // Clothing, armor, etc.
            Hands,      // Gloves, tools, etc.
            Radio,      // Radio devices
            Accessory1, // Accessories (watches, compasses, etc.)
            Accessory2  // Additional accessory slot
        }

        // Equipment data
        private Dictionary<EquipmentSlot, string> _equippedItems = new Dictionary<EquipmentSlot, string>();
        private Dictionary<string, Dictionary<string, float>> _equipmentEffects = new Dictionary<string, Dictionary<string, float>>();
        private Dictionary<string, List<EquipmentSlot>> _validSlots = new Dictionary<string, List<EquipmentSlot>>();

        // References to other systems
        private InventorySystemAdapter _inventorySystem;
        private SignalLost.GameState _gameState;

        // Signals
        [Signal] public delegate void ItemEquippedEventHandler(string itemId, int slotIndex);
        [Signal] public delegate void ItemUnequippedEventHandler(string itemId, int slotIndex);
        [Signal] public delegate void EquipmentEffectsChangedEventHandler();

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references to other systems
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");
            _gameState = GetNode<SignalLost.GameState>("/root/GameState");

            // Connect to inventory system signals
            if (_inventorySystem != null)
            {
                _inventorySystem.ItemRemoved += OnItemRemoved;
            }

            // Initialize equipment data
            InitializeEquipmentData();

            GD.Print("EquipmentSystem: Initialized");
        }

        /// <summary>
        /// Initializes equipment data with valid slots and effects.
        /// </summary>
        private void InitializeEquipmentData()
        {
            // Define valid slots for each equipment type
            _validSlots["headphones_basic"] = new List<EquipmentSlot> { EquipmentSlot.Head };
            _validSlots["headphones_advanced"] = new List<EquipmentSlot> { EquipmentSlot.Head };
            _validSlots["radio_basic"] = new List<EquipmentSlot> { EquipmentSlot.Radio };
            _validSlots["radio_advanced"] = new List<EquipmentSlot> { EquipmentSlot.Radio };
            _validSlots["radio_military"] = new List<EquipmentSlot> { EquipmentSlot.Radio };
            _validSlots["signal_analyzer"] = new List<EquipmentSlot> { EquipmentSlot.Hands };
            _validSlots["signal_booster"] = new List<EquipmentSlot> { EquipmentSlot.Accessory1, EquipmentSlot.Accessory2 };
            _validSlots["compass"] = new List<EquipmentSlot> { EquipmentSlot.Accessory1, EquipmentSlot.Accessory2 };
            _validSlots["weather_gear"] = new List<EquipmentSlot> { EquipmentSlot.Body };
            _validSlots["radiation_suit"] = new List<EquipmentSlot> { EquipmentSlot.Body };
            _validSlots["strange_crystal"] = new List<EquipmentSlot> { EquipmentSlot.Accessory1, EquipmentSlot.Accessory2 };

            // Define equipment effects
            _equipmentEffects["headphones_basic"] = new Dictionary<string, float>
            {
                { "signal_clarity", 1.2f },
                { "audio_quality", 1.1f }
            };

            _equipmentEffects["headphones_advanced"] = new Dictionary<string, float>
            {
                { "signal_clarity", 1.5f },
                { "audio_quality", 1.3f },
                { "noise_reduction", 1.2f }
            };

            _equipmentEffects["radio_basic"] = new Dictionary<string, float>
            {
                { "signal_range", 1.0f },
                { "frequency_precision", 1.0f },
                { "battery_consumption", 1.0f }
            };

            _equipmentEffects["radio_advanced"] = new Dictionary<string, float>
            {
                { "signal_range", 1.5f },
                { "frequency_precision", 1.2f },
                { "battery_consumption", 0.8f },
                { "signal_boost", 1.2f }
            };

            _equipmentEffects["radio_military"] = new Dictionary<string, float>
            {
                { "signal_range", 2.0f },
                { "frequency_precision", 1.5f },
                { "battery_consumption", 0.6f },
                { "signal_boost", 1.5f },
                { "decode_encrypted", 1.0f }
            };

            _equipmentEffects["signal_analyzer"] = new Dictionary<string, float>
            {
                { "signal_analysis", 1.0f },
                { "detect_hidden_signals", 1.0f },
                { "decode_speed", 1.2f }
            };

            _equipmentEffects["signal_booster"] = new Dictionary<string, float>
            {
                { "signal_boost", 1.3f },
                { "signal_range", 1.2f }
            };

            _equipmentEffects["compass"] = new Dictionary<string, float>
            {
                { "navigation_accuracy", 1.5f },
                { "signal_direction", 1.0f }
            };

            _equipmentEffects["weather_gear"] = new Dictionary<string, float>
            {
                { "weather_protection", 1.5f },
                { "movement_speed_rain", 1.2f }
            };

            _equipmentEffects["radiation_suit"] = new Dictionary<string, float>
            {
                { "radiation_protection", 2.0f },
                { "movement_speed", 0.8f }
            };

            _equipmentEffects["strange_crystal"] = new Dictionary<string, float>
            {
                { "signal_boost", 1.5f },
                { "detect_hidden_signals", 1.0f },
                { "strange_effects", 1.0f }
            };
        }

        /// <summary>
        /// Equips an item to the specified slot.
        /// </summary>
        /// <param name="itemId">The ID of the item to equip</param>
        /// <param name="slot">The slot to equip the item to</param>
        /// <returns>True if the item was equipped successfully, false otherwise</returns>
        public bool EquipItem(string itemId, EquipmentSlot slot)
        {
            // Check if the item exists in the inventory
            if (!_inventorySystem.HasItem(itemId))
            {
                GD.PrintErr($"EquipmentSystem: Item {itemId} not found in the inventory");
                return false;
            }

            // Check if the item is equippable
            var item = _inventorySystem.GetItem(itemId);
            if (item == null || !item.IsEquippable)
            {
                GD.PrintErr($"EquipmentSystem: Item {itemId} is not equippable");
                return false;
            }

            // Check if the slot is valid for this item
            if (!_validSlots.ContainsKey(itemId) || !_validSlots[itemId].Contains(slot))
            {
                GD.PrintErr($"EquipmentSystem: Slot {slot} is not valid for item {itemId}");
                return false;
            }

            // Unequip any item currently in the slot
            if (_equippedItems.ContainsKey(slot))
            {
                UnequipItem(slot);
            }

            // Equip the item
            _equippedItems[slot] = itemId;

            // Emit signal
            EmitSignal(SignalName.ItemEquipped, itemId, (int)slot);
            EmitSignal(SignalName.EquipmentEffectsChanged);

            GD.Print($"EquipmentSystem: Equipped {itemId} to slot {slot}");

            return true;
        }

        /// <summary>
        /// Unequips the item in the specified slot.
        /// </summary>
        /// <param name="slot">The slot to unequip</param>
        /// <returns>True if an item was unequipped, false otherwise</returns>
        public bool UnequipItem(EquipmentSlot slot)
        {
            // Check if there's an item in the slot
            if (!_equippedItems.ContainsKey(slot))
            {
                return false;
            }

            // Get the item ID
            string itemId = _equippedItems[slot];

            // Remove the item from the slot
            _equippedItems.Remove(slot);

            // Emit signal
            EmitSignal(SignalName.ItemUnequipped, itemId, (int)slot);
            EmitSignal(SignalName.EquipmentEffectsChanged);

            GD.Print($"EquipmentSystem: Unequipped {itemId} from slot {slot}");

            return true;
        }

        /// <summary>
        /// Unequips the specified item from any slot it's equipped to.
        /// </summary>
        /// <param name="itemId">The ID of the item to unequip</param>
        /// <returns>True if the item was unequipped, false otherwise</returns>
        public bool UnequipItem(string itemId)
        {
            // Find the slot the item is equipped to
            foreach (var slot in _equippedItems.Keys)
            {
                if (_equippedItems[slot] == itemId)
                {
                    return UnequipItem(slot);
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the item equipped in the specified slot.
        /// </summary>
        /// <param name="slot">The slot to check</param>
        /// <returns>The ID of the equipped item, or null if no item is equipped</returns>
        public string GetEquippedItem(EquipmentSlot slot)
        {
            if (_equippedItems.ContainsKey(slot))
            {
                return _equippedItems[slot];
            }

            return null;
        }

        /// <summary>
        /// Gets all equipped items.
        /// </summary>
        /// <returns>A dictionary mapping slots to item IDs</returns>
        public Dictionary<EquipmentSlot, string> GetEquippedItems()
        {
            return new Dictionary<EquipmentSlot, string>(_equippedItems);
        }

        /// <summary>
        /// Checks if an item is equipped.
        /// </summary>
        /// <param name="itemId">The ID of the item to check</param>
        /// <returns>True if the item is equipped, false otherwise</returns>
        public bool IsItemEquipped(string itemId)
        {
            return _equippedItems.ContainsValue(itemId);
        }

        /// <summary>
        /// Gets the slot an item is equipped to.
        /// </summary>
        /// <param name="itemId">The ID of the item to check</param>
        /// <returns>The slot the item is equipped to, or null if the item is not equipped</returns>
        public EquipmentSlot? GetItemSlot(string itemId)
        {
            foreach (var slot in _equippedItems.Keys)
            {
                if (_equippedItems[slot] == itemId)
                {
                    return slot;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the valid slots for an item.
        /// </summary>
        /// <param name="itemId">The ID of the item to check</param>
        /// <returns>A list of valid slots for the item, or an empty list if the item has no valid slots</returns>
        public List<EquipmentSlot> GetValidSlots(string itemId)
        {
            if (_validSlots.ContainsKey(itemId))
            {
                return new List<EquipmentSlot>(_validSlots[itemId]);
            }

            return new List<EquipmentSlot>();
        }

        /// <summary>
        /// Gets the effects of an item.
        /// </summary>
        /// <param name="itemId">The ID of the item to check</param>
        /// <returns>A dictionary mapping effect names to values, or an empty dictionary if the item has no effects</returns>
        public Dictionary<string, float> GetItemEffects(string itemId)
        {
            if (_equipmentEffects.ContainsKey(itemId))
            {
                return new Dictionary<string, float>(_equipmentEffects[itemId]);
            }

            return new Dictionary<string, float>();
        }

        /// <summary>
        /// Gets the total effect value for a specific effect from all equipped items.
        /// </summary>
        /// <param name="effectName">The name of the effect to get</param>
        /// <returns>The total effect value, or 1.0 if no items provide the effect</returns>
        public float GetTotalEffectValue(string effectName)
        {
            float totalValue = 1.0f;
            bool hasEffect = false;

            // Check each equipped item
            foreach (var itemId in _equippedItems.Values)
            {
                if (_equipmentEffects.ContainsKey(itemId) && _equipmentEffects[itemId].ContainsKey(effectName))
                {
                    // For multiplicative effects (most effects)
                    totalValue *= _equipmentEffects[itemId][effectName];
                    hasEffect = true;
                }
            }

            // If no items provide this effect, return the base value
            return hasEffect ? totalValue : 1.0f;
        }

        /// <summary>
        /// Gets all active effects from equipped items.
        /// </summary>
        /// <returns>A dictionary mapping effect names to total values</returns>
        public Dictionary<string, float> GetAllActiveEffects()
        {
            Dictionary<string, float> activeEffects = new Dictionary<string, float>();

            // Check each equipped item
            foreach (var itemId in _equippedItems.Values)
            {
                if (_equipmentEffects.ContainsKey(itemId))
                {
                    foreach (var effect in _equipmentEffects[itemId])
                    {
                        string effectName = effect.Key;
                        float effectValue = effect.Value;

                        if (activeEffects.ContainsKey(effectName))
                        {
                            // For multiplicative effects (most effects)
                            activeEffects[effectName] *= effectValue;
                        }
                        else
                        {
                            activeEffects[effectName] = effectValue;
                        }
                    }
                }
            }

            return activeEffects;
        }

        /// <summary>
        /// Handles item removed from inventory.
        /// </summary>
        /// <param name="itemId">The ID of the removed item</param>
        /// <param name="quantity">The quantity removed</param>
        private void OnItemRemoved(string itemId, int quantity)
        {
            // If the item is equipped, unequip it
            if (IsItemEquipped(itemId))
            {
                UnequipItem(itemId);
            }
        }

        /// <summary>
        /// Registers a new equipment item with its valid slots and effects.
        /// </summary>
        /// <param name="itemId">The ID of the item</param>
        /// <param name="validSlots">The valid slots for the item</param>
        /// <param name="effects">The effects of the item</param>
        public void RegisterEquipmentItem(string itemId, List<EquipmentSlot> validSlots, Dictionary<string, float> effects)
        {
            _validSlots[itemId] = validSlots;
            _equipmentEffects[itemId] = effects;
        }

        /// <summary>
        /// Upgrades an equipment item with new effects.
        /// </summary>
        /// <param name="itemId">The ID of the item to upgrade</param>
        /// <param name="newEffects">The new effects to apply</param>
        /// <returns>True if the item was upgraded successfully, false otherwise</returns>
        public bool UpgradeEquipment(string itemId, Dictionary<string, float> newEffects)
        {
            // Check if the item exists
            if (!_equipmentEffects.ContainsKey(itemId))
            {
                GD.PrintErr($"EquipmentSystem: Item {itemId} not found in equipment database");
                return false;
            }

            // Apply the new effects
            foreach (var effect in newEffects)
            {
                _equipmentEffects[itemId][effect.Key] = effect.Value;
            }

            // Emit signal if the item is equipped
            if (IsItemEquipped(itemId))
            {
                EmitSignal(SignalName.EquipmentEffectsChanged);
            }

            GD.Print($"EquipmentSystem: Upgraded {itemId} with new effects");

            return true;
        }

        /// <summary>
        /// Checks if the player has a specific equipment effect active.
        /// </summary>
        /// <param name="effectName">The name of the effect to check</param>
        /// <returns>True if the effect is active, false otherwise</returns>
        public bool HasEffect(string effectName)
        {
            // Check each equipped item
            foreach (var itemId in _equippedItems.Values)
            {
                if (_equipmentEffects.ContainsKey(itemId) && _equipmentEffects[itemId].ContainsKey(effectName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

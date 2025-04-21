using Godot;
using System.Collections.Generic;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Adapter class to maintain backward compatibility with the old InventorySystem.
    /// </summary>
    [GlobalClass]
    public partial class InventorySystemAdapter : Node
    {
        // Reference to the inventory core
        private InventoryCore _inventoryCore;

        // Reference to the item effects system
        private ItemEffectsSystem _itemEffectsSystem;

        // Reference to the item combination system
        private ItemCombinationSystem _itemCombinationSystem;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get the InventoryCore
            _inventoryCore = GetNode<InventoryCore>("/root/InventoryCore");

            if (_inventoryCore == null)
            {
                GD.PrintErr("InventorySystemAdapter: InventoryCore not found");
                return;
            }

            // Get the ItemEffectsSystem
            _itemEffectsSystem = GetNode<ItemEffectsSystem>("/root/ItemEffectsSystem");

            if (_itemEffectsSystem == null)
            {
                GD.PrintErr("InventorySystemAdapter: ItemEffectsSystem not found");
                return;
            }

            // Get the ItemCombinationSystem
            _itemCombinationSystem = GetNode<ItemCombinationSystem>("/root/ItemCombinationSystem");

            if (_itemCombinationSystem == null)
            {
                GD.PrintErr("InventorySystemAdapter: ItemCombinationSystem not found");
                return;
            }

            // Connect signals
            _inventoryCore.ItemAdded += OnItemAdded;
            _inventoryCore.ItemRemoved += OnItemRemoved;
            _inventoryCore.InventoryFull += OnInventoryFull;
            _itemEffectsSystem.ItemUsed += OnItemUsed;
            _itemEffectsSystem.ItemEquipped += OnItemEquipped;
            _itemEffectsSystem.ItemUnequipped += OnItemUnequipped;
            _itemEffectsSystem.ItemEffectApplied += OnItemEffectApplied;
            _itemCombinationSystem.ItemsCombined += OnItemsCombined;
        }

        // Signal handlers
        private void OnItemAdded(string itemId, int quantity)
        {
            EmitSignal(SignalName.ItemAdded, itemId, quantity);
        }

        private void OnItemRemoved(string itemId, int quantity)
        {
            EmitSignal(SignalName.ItemRemoved, itemId, quantity);
        }

        private void OnInventoryFull()
        {
            EmitSignal(SignalName.InventoryFull);
        }

        private void OnItemUsed(string itemId)
        {
            EmitSignal(SignalName.ItemUsed, itemId);
        }

        private void OnItemEquipped(string itemId)
        {
            EmitSignal(SignalName.ItemEquipped, itemId);
        }

        private void OnItemUnequipped(string itemId)
        {
            EmitSignal(SignalName.ItemUnequipped, itemId);
        }

        private void OnItemEffectApplied(string itemId, string effectType, float value)
        {
            EmitSignal(SignalName.ItemEffectApplied, itemId, effectType, value);
        }

        private void OnItemsCombined(string item1Id, string item2Id, string resultId)
        {
            EmitSignal(SignalName.ItemsCombined, item1Id, item2Id, resultId);
        }

        // Signals (for backward compatibility)
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
        public delegate void ItemEffectAppliedEventHandler(string itemId, string effectType, float value);

        [Signal]
        public delegate void ItemsCombinedEventHandler(string item1Id, string item2Id, string resultId);

        [Signal]
        public delegate void InventoryFullEventHandler();

        // Methods (for backward compatibility)
        public ItemData GetItem(string itemId)
        {
            return _inventoryCore.GetItem(itemId);
        }

        public Dictionary<string, ItemData> GetAllItems()
        {
            return _inventoryCore.GetAllItems();
        }

        public Dictionary<string, ItemData> GetInventory()
        {
            return _inventoryCore.GetInventory();
        }

        public int GetInventoryCount()
        {
            return _inventoryCore.GetInventoryCount();
        }

        public int GetTotalItemCount()
        {
            return _inventoryCore.GetTotalItemCount();
        }

        public bool IsInventoryFull()
        {
            return _inventoryCore.IsInventoryFull();
        }

        public bool AddItemToInventory(string itemId, int quantity = 1, bool updateGameState = true)
        {
            return _inventoryCore.AddItemToInventory(itemId, quantity, updateGameState);
        }

        public bool RemoveItemFromInventory(string itemId, int quantity = 1)
        {
            return _inventoryCore.RemoveItemFromInventory(itemId, quantity);
        }

        public bool UseItem(string itemId)
        {
            return _itemEffectsSystem.UseItem(itemId);
        }

        public bool EquipItem(string itemId)
        {
            return _itemEffectsSystem.EquipItem(itemId);
        }

        public bool UnequipItem(string itemId)
        {
            return _itemEffectsSystem.UnequipItem(itemId);
        }

        public bool CombineItems(string item1Id, string item2Id)
        {
            return _itemCombinationSystem.CombineItems(item1Id, item2Id);
        }

        public List<ItemData> GetItemsByCategory(string category)
        {
            return _inventoryCore.GetItemsByCategory(category);
        }

        public bool HasItem(string itemId, int quantity = 1)
        {
            return _inventoryCore.HasItem(itemId, quantity);
        }

        public void SetMaxInventorySize(int size)
        {
            _inventoryCore.SetMaxInventorySize(size);
        }

        public int GetMaxInventorySize()
        {
            return _inventoryCore.GetMaxInventorySize();
        }

        public string GetItemContent(string itemId)
        {
            return _itemEffectsSystem.GetItemContent(itemId);
        }

        public Dictionary<string, float> GetItemEffects(string itemId)
        {
            var item = _inventoryCore.GetItem(itemId);
            return item?.Effects ?? new Dictionary<string, float>();
        }

        public bool IsItemEquipped(string itemId)
        {
            return _itemEffectsSystem.IsItemEquipped(itemId);
        }

        public string GetEquippedItem(string category)
        {
            return _itemEffectsSystem.GetEquippedItem(category);
        }

        public Dictionary<string, string> GetEquippedItems()
        {
            return _itemEffectsSystem.GetEquippedItems();
        }

        public bool CanCombineItems(string item1Id, string item2Id)
        {
            return _itemCombinationSystem.CanCombineItems(item1Id, item2Id);
        }

        public string GetCombinationResult(string item1Id, string item2Id)
        {
            return _itemCombinationSystem.GetCombinationResult(item1Id, item2Id);
        }

        // Set GameState reference (for testing)
        public void SetGameState(GameState gameState)
        {
            _inventoryCore.SetGameState(gameState);
        }
    }
}

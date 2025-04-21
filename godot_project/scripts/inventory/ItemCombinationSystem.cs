using Godot;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Handles item combinations.
    /// </summary>
    [GlobalClass]
    public partial class ItemCombinationSystem : Node
    {
        // Reference to the inventory core
        private InventoryCore _inventoryCore;

        // Signals
        [Signal]
        public delegate void ItemsCombinedEventHandler(string item1Id, string item2Id, string resultId);

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get the InventoryCore
            _inventoryCore = GetNode<InventoryCore>("/root/InventoryCore");

            if (_inventoryCore == null)
            {
                GD.PrintErr("ItemCombinationSystem: InventoryCore not found");
                return;
            }
        }

        // Combine two items
        public bool CombineItems(string item1Id, string item2Id)
        {
            // Check if both items are in the inventory
            if (!_inventoryCore.HasItem(item1Id) || !_inventoryCore.HasItem(item2Id))
            {
                GD.PrintErr($"ItemCombinationSystem: One or both items not found in the inventory");
                return false;
            }

            // Get the items
            var item1 = _inventoryCore.GetItem(item1Id);
            var item2 = _inventoryCore.GetItem(item2Id);

            // Check if the items are combineable
            if (!item1.IsCombineable || !item2.IsCombineable)
            {
                GD.PrintErr($"ItemCombinationSystem: One or both items are not combineable");
                return false;
            }

            // Check if the items can be combined with each other
            if (!item1.CombinesWith.Contains(item2Id) && !item2.CombinesWith.Contains(item1Id))
            {
                GD.PrintErr($"ItemCombinationSystem: These items cannot be combined");
                return false;
            }

            // Get the result item ID
            string resultItemId = "";
            if (item1.CombinesWith.Contains(item2Id))
            {
                resultItemId = item1.CombinationResult;
            }
            else
            {
                resultItemId = item2.CombinationResult;
            }

            // Check if the result item exists in the database
            if (_inventoryCore.GetItem(resultItemId) == null)
            {
                GD.PrintErr($"ItemCombinationSystem: Result item {resultItemId} not found in the database");
                return false;
            }

            // Remove the combined items
            _inventoryCore.RemoveItemFromInventory(item1Id, 1);
            _inventoryCore.RemoveItemFromInventory(item2Id, 1);

            // Add the result item
            _inventoryCore.AddItemToInventory(resultItemId, 1);

            // Emit signal
            EmitSignal(SignalName.ItemsCombined, item1Id, item2Id, resultItemId);

            return true;
        }

        // Check if two items can be combined
        public bool CanCombineItems(string item1Id, string item2Id)
        {
            // Check if both items are in the inventory
            if (!_inventoryCore.HasItem(item1Id) || !_inventoryCore.HasItem(item2Id))
            {
                return false;
            }

            // Get the items
            var item1 = _inventoryCore.GetItem(item1Id);
            var item2 = _inventoryCore.GetItem(item2Id);

            // Check if the items are combineable
            if (!item1.IsCombineable || !item2.IsCombineable)
            {
                return false;
            }

            // Check if the items can be combined with each other
            return item1.CombinesWith.Contains(item2Id) || item2.CombinesWith.Contains(item1Id);
        }

        // Get the result of combining two items
        public string GetCombinationResult(string item1Id, string item2Id)
        {
            // Check if both items are in the inventory
            if (!_inventoryCore.HasItem(item1Id) || !_inventoryCore.HasItem(item2Id))
            {
                return null;
            }

            // Get the items
            var item1 = _inventoryCore.GetItem(item1Id);
            var item2 = _inventoryCore.GetItem(item2Id);

            // Check if the items are combineable
            if (!item1.IsCombineable || !item2.IsCombineable)
            {
                return null;
            }

            // Check if the items can be combined with each other
            if (item1.CombinesWith.Contains(item2Id))
            {
                return item1.CombinationResult;
            }
            else if (item2.CombinesWith.Contains(item1Id))
            {
                return item2.CombinationResult;
            }

            return null;
        }
    }
}

using Godot;
using System;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Simple script to test the inventory system.
    /// </summary>
    [GlobalClass]
    public partial class InventorySystemTestScript : Node
    {
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Wait for the inventory system to be set up
            CallDeferred("TestInventorySystem");
        }

        // Test the inventory system
        private void TestInventorySystem()
        {
            // Get the inventory system
            var inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");

            if (inventorySystem == null)
            {
                GD.PrintErr("InventorySystemTestScript: InventorySystem not found");
                return;
            }

            // Add some items to the inventory
            inventorySystem.AddItemToInventory("flashlight");
            inventorySystem.AddItemToInventory("battery", 2);
            inventorySystem.AddItemToInventory("medkit");
            inventorySystem.AddItemToInventory("radio_broken");
            inventorySystem.AddItemToInventory("radio_part");

            // Print the inventory
            GD.Print("Inventory:");
            foreach (var item in inventorySystem.GetInventory().Values)
            {
                GD.Print($"- {item.Name} (x{item.Quantity})");
            }

            // Use an item
            GD.Print("\nUsing medkit...");
            inventorySystem.UseItem("medkit");

            // Print the inventory again
            GD.Print("\nInventory after using medkit:");
            foreach (var item in inventorySystem.GetInventory().Values)
            {
                GD.Print($"- {item.Name} (x{item.Quantity})");
            }

            // Combine items
            GD.Print("\nCombining radio_broken and radio_part...");
            inventorySystem.CombineItems("radio_broken", "radio_part");

            // Print the inventory again
            GD.Print("\nInventory after combining items:");
            foreach (var item in inventorySystem.GetInventory().Values)
            {
                GD.Print($"- {item.Name} (x{item.Quantity})");
            }

            // Equip an item
            GD.Print("\nEquipping flashlight...");
            inventorySystem.EquipItem("flashlight");

            // Print equipped items
            GD.Print("\nEquipped items:");
            foreach (var category in inventorySystem.GetEquippedItems().Keys)
            {
                var itemId = inventorySystem.GetEquippedItem(category);
                var item = inventorySystem.GetItem(itemId);
                GD.Print($"- {category}: {item.Name}");
            }

            GD.Print("\nInventory system test completed successfully!");
        }
    }
}

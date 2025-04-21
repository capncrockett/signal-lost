using Godot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalLost.Inventory;
using GUT;

namespace SignalLost.Tests
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public partial class RefactoredInventoryTests : Test
    {
        private InventoryCore _inventoryCore;
        private ItemEffectsSystem _itemEffectsSystem;
        private ItemCombinationSystem _itemCombinationSystem;
        private GameState _gameState;

        // Setup method called before each test
        public void Before()
        {
            // Create a mock GameState
            _gameState = new GameState();

            // Create the InventoryCore
            _inventoryCore = new InventoryCore();
            _inventoryCore.SetGameState(_gameState);

            // Create the ItemEffectsSystem
            _itemEffectsSystem = new ItemEffectsSystem();

            // Create the ItemCombinationSystem
            _itemCombinationSystem = new ItemCombinationSystem();

            // Initialize the item database
            InitializeTestItems();
        }

        // Teardown method called after each test
        public void After()
        {
            _inventoryCore = null;
            _itemEffectsSystem = null;
            _itemCombinationSystem = null;
            _gameState = null;
        }

        // Initialize test items
        private void InitializeTestItems()
        {
            // Add test items to the database
            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "test_item",
                Name = "Test Item",
                Description = "A test item.",
                Category = "test",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "test_consumable",
                Name = "Test Consumable",
                Description = "A test consumable item.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IsEquippable = false,
                Effects = new System.Collections.Generic.Dictionary<string, float> { { "test_effect", 10.0f } }
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "test_component1",
                Name = "Test Component 1",
                Description = "A test component for combining.",
                Category = "component",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IsCombineable = true,
                CombinesWith = new System.Collections.Generic.List<string> { "test_component2" },
                CombinationResult = "test_result"
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "test_component2",
                Name = "Test Component 2",
                Description = "Another test component for combining.",
                Category = "component",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IsCombineable = true,
                CombinesWith = new System.Collections.Generic.List<string> { "test_component1" },
                CombinationResult = "test_result"
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "test_result",
                Name = "Test Result",
                Description = "The result of combining test components.",
                Category = "test",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true
            });
        }

        // Test adding items to inventory
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestAddItemToInventory()
        {
            // Add an item to the inventory
            bool result = _inventoryCore.AddItemToInventory("test_item");

            // Verify the item was added
            Assert.IsTrue(result, "AddItemToInventory should return true");
            Assert.IsTrue(_inventoryCore.HasItem("test_item"), "Inventory should contain test_item");
            Assert.AreEqual(1, _inventoryCore.GetInventory()["test_item"].Quantity, "test_item quantity should be 1");
        }

        // Test removing items from inventory
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestRemoveItemFromInventory()
        {
            // Add an item to the inventory
            _inventoryCore.AddItemToInventory("test_item", 2);

            // Remove one item
            bool result = _inventoryCore.RemoveItemFromInventory("test_item");

            // Verify the item was removed
            Assert.IsTrue(result, "RemoveItemFromInventory should return true");
            Assert.IsTrue(_inventoryCore.HasItem("test_item"), "Inventory should still contain test_item");
            Assert.AreEqual(1, _inventoryCore.GetInventory()["test_item"].Quantity, "test_item quantity should be 1");

            // Remove the last item
            result = _inventoryCore.RemoveItemFromInventory("test_item");

            // Verify the item was completely removed
            Assert.IsTrue(result, "RemoveItemFromInventory should return true");
            Assert.IsFalse(_inventoryCore.HasItem("test_item"), "Inventory should not contain test_item anymore");
        }

        // Test using items
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestUseItem()
        {
            // Add items to the inventory
            _inventoryCore.AddItemToInventory("test_item");
            _inventoryCore.AddItemToInventory("test_consumable");

            // Use a non-consumable item
            bool result1 = _itemEffectsSystem.UseItem("test_item");

            // Verify the item was used but not consumed
            Assert.IsTrue(result1, "UseItem should return true for test_item");
            Assert.IsTrue(_inventoryCore.HasItem("test_item"), "test_item should still be in the inventory");

            // Use a consumable item
            bool result2 = _itemEffectsSystem.UseItem("test_consumable");

            // Verify the item was used and consumed
            Assert.IsTrue(result2, "UseItem should return true for test_consumable");
            Assert.IsFalse(_inventoryCore.HasItem("test_consumable"), "test_consumable should be removed from the inventory");
        }

        // Test combining items
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestCombineItems()
        {
            // Add items to the inventory
            _inventoryCore.AddItemToInventory("test_component1");
            _inventoryCore.AddItemToInventory("test_component2");

            // Combine the items
            bool result = _itemCombinationSystem.CombineItems("test_component1", "test_component2");

            // Verify the items were combined
            Assert.IsTrue(result, "CombineItems should return true");
            Assert.IsFalse(_inventoryCore.HasItem("test_component1"), "test_component1 should be removed from the inventory");
            Assert.IsFalse(_inventoryCore.HasItem("test_component2"), "test_component2 should be removed from the inventory");
            Assert.IsTrue(_inventoryCore.HasItem("test_result"), "test_result should be added to the inventory");
        }

        // Test equipping items
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestEquipItem()
        {
            // Add an item to the inventory
            _inventoryCore.AddItemToInventory("test_item");

            // Equip the item
            bool result = _itemEffectsSystem.EquipItem("test_item");

            // Verify the item was equipped
            Assert.IsTrue(result, "EquipItem should return true");
            Assert.IsTrue(_itemEffectsSystem.IsItemEquipped("test_item"), "test_item should be equipped");
            Assert.AreEqual("test_item", _itemEffectsSystem.GetEquippedItem("test"), "test_item should be equipped in the test category");

            // Unequip the item
            bool result2 = _itemEffectsSystem.UnequipItem("test_item");

            // Verify the item was unequipped
            Assert.IsTrue(result2, "UnequipItem should return true");
            Assert.IsFalse(_itemEffectsSystem.IsItemEquipped("test_item"), "test_item should not be equipped");
            Assert.IsNull(_itemEffectsSystem.GetEquippedItem("test"), "No item should be equipped in the test category");
        }
    }
}

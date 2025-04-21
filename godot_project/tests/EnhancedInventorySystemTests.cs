using Godot;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GUT;

namespace SignalLost.Tests
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public partial class EnhancedInventorySystemTests : Test
    {
        private InventorySystem _inventorySystem;
        private GameState _gameState;

        // Setup method called before each test
        public void Before()
        {
            // Create a mock GameState
            _gameState = new GameState();

            // Create the InventorySystem
            _inventorySystem = new InventorySystem();
            _inventorySystem.SetGameState(_gameState);

            // Call _Ready to initialize
            _inventorySystem.Call("_Ready");
        }

        // Teardown method called after each test
        public void After()
        {
            _inventorySystem = null;
            _gameState = null;
        }

        // Test item combination
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestItemCombination()
        {
            // Add items to the inventory
            _inventorySystem.AddItemToInventory("radio_broken");
            _inventorySystem.AddItemToInventory("radio_part");

            // Combine the items
            bool result = _inventorySystem.CombineItems("radio_broken", "radio_part");

            // Verify the result
            Assert.IsTrue(result, "CombineItems should return true");
            Assert.IsFalse(_inventorySystem.HasItem("radio_broken"), "radio_broken should be removed from inventory");
            Assert.IsFalse(_inventorySystem.HasItem("radio_part"), "radio_part should be removed from inventory");
            Assert.IsTrue(_inventorySystem.HasItem("radio"), "radio should be added to inventory");
        }

        // Test item effects
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestItemEffects()
        {
            // Add an item with effects to the inventory
            _inventorySystem.AddItemToInventory("medkit");

            // Get the item effects
            var effects = _inventorySystem.GetItemEffects("medkit");

            // Verify the effects
            Assert.IsTrue(effects.ContainsKey("health"), "medkit should have a health effect");
            Assert.AreEqual(50.0f, effects["health"], "medkit health effect should be 50.0");

            // Use the item
            bool result = _inventorySystem.UseItem("medkit");

            // Verify the result
            Assert.IsTrue(result, "UseItem should return true");
            Assert.IsFalse(_inventorySystem.HasItem("medkit"), "medkit should be removed from inventory after use");
        }

        // Test equipping items
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestEquipItems()
        {
            // Add equippable items to the inventory
            _inventorySystem.AddItemToInventory("flashlight");
            _inventorySystem.AddItemToInventory("radio");

            // Add non-equippable item
            _inventorySystem.AddItemToInventory("battery");

            // Equip the flashlight
            bool result1 = _inventorySystem.EquipItem("flashlight");

            // Verify the result
            Assert.IsTrue(result1, "EquipItem should return true for flashlight");

            // Try to equip a non-equippable item
            bool result2 = _inventorySystem.EquipItem("battery");

            // Verify the result
            Assert.IsFalse(result2, "EquipItem should return false for battery");

            // Unequip the flashlight
            bool result3 = _inventorySystem.UnequipItem("flashlight");

            // Verify the result
            Assert.IsTrue(result3, "UnequipItem should return true for flashlight");
        }

        // Test document content
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestDocumentContent()
        {
            // Add a document to the inventory
            _inventorySystem.AddItemToInventory("research_notes");

            // Get the document content
            string content = _inventorySystem.GetItemContent("research_notes");

            // Verify the content
            Assert.IsFalse(string.IsNullOrEmpty(content), "research_notes should have content");
            Assert.IsTrue(content.Contains("Project SIGNAL"), "research_notes content should contain 'Project SIGNAL'");
        }
    }
}

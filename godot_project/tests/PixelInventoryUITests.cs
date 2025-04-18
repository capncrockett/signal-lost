using Godot;
using System;
using GUT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public partial class PixelInventoryUITests : Test
    {
        private PixelInventoryUI _inventoryUI;
        private InventorySystem _inventorySystem;
        private GameState _gameState;

        // Setup method called before each test
        public override void Before()
        {
            // Create a mock GameState
            _gameState = new GameState();
            AddChild(_gameState);
            _gameState.Name = "GameState";

            // Create a mock InventorySystem
            _inventorySystem = new InventorySystem();
            AddChild(_inventorySystem);
            _inventorySystem.Name = "InventorySystem";

            // Create the PixelInventoryUI
            _inventoryUI = new PixelInventoryUI();
            AddChild(_inventoryUI);
            _inventoryUI.Name = "PixelInventoryUI";

            // Call _Ready to initialize
            _inventoryUI._Ready();
        }

        // Teardown method called after each test
        public override void After()
        {
            // Remove nodes
            _inventoryUI.QueueFree();
            _inventorySystem.QueueFree();
            _gameState.QueueFree();
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestInitialization()
        {
            // Verify that the inventory UI is initialized correctly
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(_inventoryUI, "PixelInventoryUI should not be null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(_inventoryUI.IsVisible(), "PixelInventoryUI should be hidden by default");
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestVisibility()
        {
            // Test setting visibility
            _inventoryUI.SetVisible(true);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(_inventoryUI.IsVisible(), "PixelInventoryUI should be visible after SetVisible(true)");

            _inventoryUI.SetVisible(false);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(_inventoryUI.IsVisible(), "PixelInventoryUI should be hidden after SetVisible(false)");

            // Test toggling visibility
            _inventoryUI.ToggleVisibility();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(_inventoryUI.IsVisible(), "PixelInventoryUI should be visible after ToggleVisibility()");

            _inventoryUI.ToggleVisibility();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(_inventoryUI.IsVisible(), "PixelInventoryUI should be hidden after ToggleVisibility()");
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestInventoryChanges()
        {
            // Add items to the inventory
            _inventorySystem.AddItemToInventory("flashlight");
            _inventorySystem.AddItemToInventory("battery");
            _inventorySystem.AddItemToInventory("medkit");

            // Verify that the inventory has the correct number of items
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(3, _inventorySystem.GetTotalItemCount(), "Inventory should have 3 items");

            // Make the inventory UI visible but don't actually draw it
            _inventoryUI.SetVisible(true);

            // Remove an item
            _inventorySystem.RemoveItemFromInventory("battery");

            // Verify that the inventory has the correct number of items
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(2, _inventorySystem.GetTotalItemCount(), "Inventory should have 2 items after removing battery");
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestInputHandling()
        {
            // Make the inventory UI visible
            _inventoryUI.SetVisible(true);

            // Simulate pressing the Escape key
            var escapeEvent = new InputEventKey();
            escapeEvent.Keycode = Key.Escape;
            escapeEvent.Pressed = true;
            _inventoryUI._Input(escapeEvent);

            // Verify that the inventory UI is hidden
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(_inventoryUI.IsVisible(), "PixelInventoryUI should be hidden after pressing Escape");
        }
    }
}

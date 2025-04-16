using Godot;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class InventorySystemTests : Node
    {
        private InventorySystem _inventorySystem;
        private GameState _gameState;

        // Setup method called before each test
        public void Setup()
        {
            // Create a new GameState
            _gameState = new GameState();
            AddChild(_gameState);
            
            // Create a new InventorySystem
            _inventorySystem = new InventorySystem();
            AddChild(_inventorySystem);
            
            // Set the GameState reference in InventorySystem
            _inventorySystem.Set("_gameState", _gameState);
            
            // Initialize the item database
            _inventorySystem.Call("InitializeItemDatabase");
        }

        // Teardown method called after each test
        public void Teardown()
        {
            // Clean up
            if (_inventorySystem != null)
            {
                _inventorySystem.QueueFree();
                _inventorySystem = null;
            }
            
            if (_gameState != null)
            {
                _gameState.QueueFree();
                _gameState = null;
            }
        }

        // Test getting an item from the database
        [TestMethod]
        public void TestGetItem()
        {
            // Arrange
            Setup();

            try
            {
                // Act
                var item = _inventorySystem.GetItem("flashlight");

                // Assert
                AssertNotNull(item, "Item should not be null");
                AssertEqual(item.Id, "flashlight", "Item ID should be 'flashlight'");
                AssertEqual(item.Name, "Flashlight", "Item name should be 'Flashlight'");
                AssertEqual(item.Category, "tool", "Item category should be 'tool'");
            }
            finally
            {
                Teardown();
            }
        }

        // Test adding an item to the inventory
        [TestMethod]
        public void TestAddItemToInventory()
        {
            // Arrange
            Setup();

            try
            {
                // Act
                bool result = _inventorySystem.AddItemToInventory("flashlight");
                var inventory = _inventorySystem.GetInventory();

                // Assert
                AssertTrue(result, "AddItemToInventory should return true");
                AssertEqual(inventory.Count, 1, "Inventory should have 1 item");
                AssertTrue(inventory.ContainsKey("flashlight"), "Inventory should contain 'flashlight'");
                AssertEqual(inventory["flashlight"].Quantity, 1, "Flashlight quantity should be 1");
            }
            finally
            {
                Teardown();
            }
        }

        // Test adding multiple items to the inventory
        [TestMethod]
        public void TestAddMultipleItems()
        {
            // Arrange
            Setup();

            try
            {
                // Act
                _inventorySystem.AddItemToInventory("flashlight");
                _inventorySystem.AddItemToInventory("battery");
                _inventorySystem.AddItemToInventory("flashlight"); // Add another flashlight
                var inventory = _inventorySystem.GetInventory();

                // Assert
                AssertEqual(inventory.Count, 2, "Inventory should have 2 unique items");
                AssertEqual(inventory["flashlight"].Quantity, 2, "Flashlight quantity should be 2");
                AssertEqual(inventory["battery"].Quantity, 1, "Battery quantity should be 1");
            }
            finally
            {
                Teardown();
            }
        }

        // Test removing an item from the inventory
        [TestMethod]
        public void TestRemoveItemFromInventory()
        {
            // Arrange
            Setup();
            _inventorySystem.AddItemToInventory("flashlight", 2);

            try
            {
                // Act
                bool result = _inventorySystem.RemoveItemFromInventory("flashlight");
                var inventory = _inventorySystem.GetInventory();

                // Assert
                AssertTrue(result, "RemoveItemFromInventory should return true");
                AssertEqual(inventory["flashlight"].Quantity, 1, "Flashlight quantity should be 1");

                // Remove the last flashlight
                result = _inventorySystem.RemoveItemFromInventory("flashlight");
                inventory = _inventorySystem.GetInventory();

                AssertTrue(result, "RemoveItemFromInventory should return true");
                AssertFalse(inventory.ContainsKey("flashlight"), "Inventory should not contain 'flashlight'");
            }
            finally
            {
                Teardown();
            }
        }

        // Test using an item
        [TestMethod]
        public void TestUseItem()
        {
            // Arrange
            Setup();
            _inventorySystem.AddItemToInventory("medkit");
            _inventorySystem.AddItemToInventory("flashlight");

            try
            {
                // Act - Use a consumable item
                bool result1 = _inventorySystem.UseItem("medkit");
                var inventory1 = _inventorySystem.GetInventory();

                // Assert
                AssertTrue(result1, "UseItem should return true for medkit");
                AssertFalse(inventory1.ContainsKey("medkit"), "Medkit should be consumed and removed from inventory");

                // Act - Use a non-consumable item
                bool result2 = _inventorySystem.UseItem("flashlight");
                var inventory2 = _inventorySystem.GetInventory();

                // Assert
                AssertTrue(result2, "UseItem should return true for flashlight");
                AssertTrue(inventory2.ContainsKey("flashlight"), "Flashlight should still be in inventory after use");
            }
            finally
            {
                Teardown();
            }
        }

        // Test inventory capacity
        [TestMethod]
        public void TestInventoryCapacity()
        {
            // Arrange
            Setup();
            _inventorySystem.SetMaxInventorySize(3);

            try
            {
                // Act
                bool result1 = _inventorySystem.AddItemToInventory("flashlight");
                bool result2 = _inventorySystem.AddItemToInventory("battery");
                bool result3 = _inventorySystem.AddItemToInventory("medkit");
                bool result4 = _inventorySystem.AddItemToInventory("key_cabin"); // This should fail due to capacity

                // Assert
                AssertTrue(result1, "Should be able to add flashlight");
                AssertTrue(result2, "Should be able to add battery");
                AssertTrue(result3, "Should be able to add medkit");
                AssertFalse(result4, "Should not be able to add key_cabin due to capacity");
                AssertEqual(_inventorySystem.GetTotalItemCount(), 3, "Total item count should be 3");
            }
            finally
            {
                Teardown();
            }
        }

        // Test getting items by category
        [TestMethod]
        public void TestGetItemsByCategory()
        {
            // Arrange
            Setup();
            _inventorySystem.AddItemToInventory("flashlight");
            _inventorySystem.AddItemToInventory("battery");
            _inventorySystem.AddItemToInventory("medkit");
            _inventorySystem.AddItemToInventory("key_cabin");

            try
            {
                // Act
                var tools = _inventorySystem.GetItemsByCategory("tool");
                var consumables = _inventorySystem.GetItemsByCategory("consumable");
                var keys = _inventorySystem.GetItemsByCategory("key");

                // Assert
                AssertEqual(tools.Count, 1, "Should have 1 tool");
                AssertEqual(consumables.Count, 2, "Should have 2 consumables");
                AssertEqual(keys.Count, 1, "Should have 1 key");
                AssertEqual(tools[0].Id, "flashlight", "Tool should be flashlight");
            }
            finally
            {
                Teardown();
            }
        }

        // Assertion methods
        protected void AssertTrue(bool condition, string message = null)
        {
            if (!condition)
            {
                GD.PrintErr($"Assertion failed: {message ?? "Expected true but got false"}");
                throw new Exception(message ?? "Assertion failed");
            }
        }

        protected void AssertFalse(bool condition, string message = null)
        {
            if (condition)
            {
                GD.PrintErr($"Assertion failed: {message ?? "Expected false but got true"}");
                throw new Exception(message ?? "Assertion failed");
            }
        }

        protected void AssertEqual<T>(T actual, T expected, string message = null)
        {
            if (!actual.Equals(expected))
            {
                GD.PrintErr($"Assertion failed: {message ?? $"Expected {expected} but got {actual}"}");
                throw new Exception(message ?? "Assertion failed");
            }
        }

        protected void AssertNotNull(object obj, string message = null)
        {
            if (obj == null)
            {
                GD.PrintErr($"Assertion failed: {message ?? "Expected non-null value but got null"}");
                throw new Exception(message ?? "Assertion failed");
            }
        }
    }
}

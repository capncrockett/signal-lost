using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// Tests for the InventorySystem class.
    /// </summary>
    [TestClass]
    public partial class InventorySystemTests : UnitTestBase
    {
        private InventorySystem _inventorySystem;
        private GameState _gameState;

        // Signal tracking
        private bool _itemAddedSignalReceived = false;
        private bool _itemRemovedSignalReceived = false;
        private bool _itemUsedSignalReceived = false;
        private bool _inventoryFullSignalReceived = false;
        private string _lastItemAddedId = null;
        private string _lastItemRemovedId = null;
        private string _lastItemUsedId = null;
        private int _lastItemAddedQuantity = 0;
        private int _lastItemRemovedQuantity = 0;

        /// <summary>
        /// Called before each test.
        /// </summary>
        public override async void Before()
        {
            base.Before();

            try
            {
                // Create instances of the components
                _gameState = CreateMockGameState();
                _inventorySystem = CreateMockInventorySystem(_gameState);

                // Wait a frame to ensure all nodes are added
                await WaitForSignal(GetTree(), "process_frame");

                // Connect to signals
                _inventorySystem.ItemAdded += OnItemAdded;
                _inventorySystem.ItemRemoved += OnItemRemoved;
                _inventorySystem.ItemUsed += OnItemUsed;
                _inventorySystem.InventoryFull += OnInventoryFull;

                // Reset signal tracking
                ResetSignalTracking();
            }
            catch (Exception ex)
            {
                LogError($"Error in InventorySystemTests.Before: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }

        /// <summary>
        /// Called after each test.
        /// </summary>
        public override void After()
        {
            try
            {
                // Disconnect signals
                if (_inventorySystem != null)
                {
                    _inventorySystem.ItemAdded -= OnItemAdded;
                    _inventorySystem.ItemRemoved -= OnItemRemoved;
                    _inventorySystem.ItemUsed -= OnItemUsed;
                    _inventorySystem.InventoryFull -= OnInventoryFull;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in InventorySystemTests.After: {ex.Message}");
                LogError(ex.StackTrace);
            }

            base.After(); // This will handle freeing the nodes
        }

        // Reset signal tracking
        private void ResetSignalTracking()
        {
            _itemAddedSignalReceived = false;
            _itemRemovedSignalReceived = false;
            _itemUsedSignalReceived = false;
            _inventoryFullSignalReceived = false;
            _lastItemAddedId = null;
            _lastItemRemovedId = null;
            _lastItemUsedId = null;
            _lastItemAddedQuantity = 0;
            _lastItemRemovedQuantity = 0;
        }

        // Signal handlers
        private void OnItemAdded(string itemId, int quantity)
        {
            _itemAddedSignalReceived = true;
            _lastItemAddedId = itemId;
            _lastItemAddedQuantity = quantity;
        }

        private void OnItemRemoved(string itemId, int quantity)
        {
            _itemRemovedSignalReceived = true;
            _lastItemRemovedId = itemId;
            _lastItemRemovedQuantity = quantity;
        }

        private void OnItemUsed(string itemId)
        {
            _itemUsedSignalReceived = true;
            _lastItemUsedId = itemId;
        }

        private void OnInventoryFull()
        {
            _inventoryFullSignalReceived = true;
        }

        /// <summary>
        /// Tests getting an item from the database.
        /// </summary>
        [TestMethod]
        public void TestGetItem()
        {
            // Get an item from the database
            var item = _inventorySystem.GetItem(TestData.ItemIds.Flashlight);

            // Verify the item properties
            Assert.IsNotNull(item, "Item should not be null");
            Assert.AreEqual(TestData.ItemIds.Flashlight, item.Id, "Item ID should be 'flashlight'");
            Assert.AreEqual("Flashlight", item.Name, "Item name should be 'Flashlight'");
            Assert.AreEqual("tool", item.Category, "Item category should be 'tool'");
            Assert.IsTrue(item.IsUsable, "Flashlight should be usable");
            Assert.IsFalse(item.IsConsumable, "Flashlight should not be consumable");
        }

        [TestMethod]
        public void TestGetNonExistentItem()
        {
            // Try to get a non-existent item
            var item = _inventorySystem.GetItem("non_existent_item");

            // Verify the item is null
            Assert.IsNull(item, "Non-existent item should be null");
        }

        /// <summary>
        /// Tests adding an item to the inventory.
        /// </summary>
        [TestMethod]
        public void TestAddItemToInventory()
        {
            // Add an item to the inventory
            bool result = _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight);
            var inventory = _inventorySystem.GetInventory();

            // Verify the item was added
            Assert.IsTrue(result, "AddItemToInventory should return true");
            Assert.AreEqual(1, inventory.Count, "Inventory should have 1 item");
            Assert.IsTrue(inventory.ContainsKey(TestData.ItemIds.Flashlight), "Inventory should contain 'flashlight'");
            Assert.AreEqual(1, inventory[TestData.ItemIds.Flashlight].Quantity, "Flashlight quantity should be 1");

            // Verify the signal was emitted
            Assert.IsTrue(_itemAddedSignalReceived, "ItemAdded signal should be received");
            Assert.AreEqual(TestData.ItemIds.Flashlight, _lastItemAddedId, "ItemAdded signal should include the correct item ID");
            Assert.AreEqual(1, _lastItemAddedQuantity, "ItemAdded signal should include the correct quantity");
        }

        /// <summary>
        /// Tests adding multiple items to the inventory.
        /// </summary>
        [TestMethod]
        public void TestAddMultipleItems()
        {
            // Add multiple items to the inventory
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight);
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Battery);
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight); // Add another flashlight
            var inventory = _inventorySystem.GetInventory();

            // Verify the items were added correctly
            Assert.AreEqual(2, inventory.Count, "Inventory should have 2 unique items");
            Assert.AreEqual(2, inventory[TestData.ItemIds.Flashlight].Quantity, "Flashlight quantity should be 2");
            Assert.AreEqual(1, inventory[TestData.ItemIds.Battery].Quantity, "Battery quantity should be 1");
        }

        /// <summary>
        /// Tests adding an item with a specific quantity.
        /// </summary>
        [TestMethod]
        public void TestAddItemWithQuantity()
        {
            // Add an item with a specific quantity
            bool result = _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight, 3);
            var inventory = _inventorySystem.GetInventory();

            // Verify the item was added with the correct quantity
            Assert.IsTrue(result, "AddItemToInventory should return true");
            Assert.AreEqual(1, inventory.Count, "Inventory should have 1 item");
            Assert.IsTrue(inventory.ContainsKey(TestData.ItemIds.Flashlight), "Inventory should contain 'flashlight'");
            Assert.AreEqual(3, inventory[TestData.ItemIds.Flashlight].Quantity, "Flashlight quantity should be 3");

            // Verify the signal was emitted
            Assert.IsTrue(_itemAddedSignalReceived, "ItemAdded signal should be received");
            Assert.AreEqual(TestData.ItemIds.Flashlight, _lastItemAddedId, "ItemAdded signal should include the correct item ID");
            Assert.AreEqual(3, _lastItemAddedQuantity, "ItemAdded signal should include the correct quantity");
        }

        /// <summary>
        /// Tests removing an item from the inventory.
        /// </summary>
        [TestMethod]
        public void TestRemoveItemFromInventory()
        {
            // Add an item to the inventory
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight, 2);

            // Reset signal tracking
            ResetSignalTracking();

            // Remove an item from the inventory
            bool result = _inventorySystem.RemoveItemFromInventory(TestData.ItemIds.Flashlight);
            var inventory = _inventorySystem.GetInventory();

            // Verify the item was removed
            Assert.IsTrue(result, "RemoveItemFromInventory should return true");
            Assert.AreEqual(1, inventory[TestData.ItemIds.Flashlight].Quantity, "Flashlight quantity should be 1");

            // Verify the signal was emitted
            Assert.IsTrue(_itemRemovedSignalReceived, "ItemRemoved signal should be received");
            Assert.AreEqual(TestData.ItemIds.Flashlight, _lastItemRemovedId, "ItemRemoved signal should include the correct item ID");
            Assert.AreEqual(1, _lastItemRemovedQuantity, "ItemRemoved signal should include the correct quantity");

            // Reset signal tracking
            ResetSignalTracking();

            // Remove the last flashlight
            result = _inventorySystem.RemoveItemFromInventory(TestData.ItemIds.Flashlight);
            inventory = _inventorySystem.GetInventory();

            // Verify the item was removed completely
            Assert.IsTrue(result, "RemoveItemFromInventory should return true");
            Assert.IsFalse(inventory.ContainsKey(TestData.ItemIds.Flashlight), "Inventory should not contain 'flashlight'");

            // Verify the signal was emitted
            Assert.IsTrue(_itemRemovedSignalReceived, "ItemRemoved signal should be received");
            Assert.AreEqual(TestData.ItemIds.Flashlight, _lastItemRemovedId, "ItemRemoved signal should include the correct item ID");
            Assert.AreEqual(1, _lastItemRemovedQuantity, "ItemRemoved signal should include the correct quantity");
        }

        /// <summary>
        /// Tests removing a non-existent item from the inventory.
        /// </summary>
        [TestMethod]
        public void TestRemoveNonExistentItem()
        {
            // Try to remove a non-existent item
            bool result = _inventorySystem.RemoveItemFromInventory("non_existent_item");

            // Verify the operation failed
            Assert.IsFalse(result, "RemoveItemFromInventory should return false for non-existent items");
            Assert.IsFalse(_itemRemovedSignalReceived, "ItemRemoved signal should not be received");
        }

        /// <summary>
        /// Tests removing more items than available from the inventory.
        /// </summary>
        [TestMethod]
        public void TestRemoveMoreThanAvailable()
        {
            // Add an item to the inventory
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight, 1);

            // Reset signal tracking
            ResetSignalTracking();

            // Try to remove more than available
            bool result = _inventorySystem.RemoveItemFromInventory(TestData.ItemIds.Flashlight, 2);

            // Verify the operation failed
            Assert.IsFalse(result, "RemoveItemFromInventory should return false when trying to remove more than available");
            Assert.IsFalse(_itemRemovedSignalReceived, "ItemRemoved signal should not be received");

            // Verify the item is still in the inventory
            var inventory = _inventorySystem.GetInventory();
            Assert.IsTrue(inventory.ContainsKey(TestData.ItemIds.Flashlight), "Inventory should still contain 'flashlight'");
            Assert.AreEqual(1, inventory[TestData.ItemIds.Flashlight].Quantity, "Flashlight quantity should still be 1");
        }

        /// <summary>
        /// Tests using items from the inventory.
        /// </summary>
        [TestMethod]
        public void TestUseItem()
        {
            // Add items to the inventory
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Medkit);
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight);

            // Reset signal tracking
            ResetSignalTracking();

            // Use a consumable item
            bool result1 = _inventorySystem.UseItem(TestData.ItemIds.Medkit);
            var inventory1 = _inventorySystem.GetInventory();

            // Verify the item was used and consumed
            Assert.IsTrue(result1, "UseItem should return true for medkit");
            Assert.IsFalse(inventory1.ContainsKey(TestData.ItemIds.Medkit), "Medkit should be consumed and removed from inventory");

            // Verify the signals were emitted
            Assert.IsTrue(_itemUsedSignalReceived, "ItemUsed signal should be received");
            Assert.AreEqual(TestData.ItemIds.Medkit, _lastItemUsedId, "ItemUsed signal should include the correct item ID");
            Assert.IsTrue(_itemRemovedSignalReceived, "ItemRemoved signal should be received");
            Assert.AreEqual(TestData.ItemIds.Medkit, _lastItemRemovedId, "ItemRemoved signal should include the correct item ID");

            // Reset signal tracking
            ResetSignalTracking();

            // Use a non-consumable item
            bool result2 = _inventorySystem.UseItem(TestData.ItemIds.Flashlight);
            var inventory2 = _inventorySystem.GetInventory();

            // Verify the item was used but not consumed
            Assert.IsTrue(result2, "UseItem should return true for flashlight");
            Assert.IsTrue(inventory2.ContainsKey(TestData.ItemIds.Flashlight), "Flashlight should still be in inventory after use");

            // Verify the signal was emitted
            Assert.IsTrue(_itemUsedSignalReceived, "ItemUsed signal should be received");
            Assert.AreEqual(TestData.ItemIds.Flashlight, _lastItemUsedId, "ItemUsed signal should include the correct item ID");
            Assert.IsFalse(_itemRemovedSignalReceived, "ItemRemoved signal should not be received for non-consumable items");
        }

        /// <summary>
        /// Tests using a non-existent item from the inventory.
        /// </summary>
        [TestMethod]
        public void TestUseNonExistentItem()
        {
            // Try to use a non-existent item
            bool result = _inventorySystem.UseItem("non_existent_item");

            // Verify the operation failed
            Assert.IsFalse(result, "UseItem should return false for non-existent items");
            Assert.IsFalse(_itemUsedSignalReceived, "ItemUsed signal should not be received");
        }

        /// <summary>
        /// Tests using a non-usable item from the inventory.
        /// </summary>
        [TestMethod]
        public void TestUseNonUsableItem()
        {
            // Add a non-usable item to the inventory
            _inventorySystem.AddItemToInventory(TestData.ItemIds.RadioPart); // Assuming radio_part is not usable

            // Reset signal tracking
            ResetSignalTracking();

            // Try to use a non-usable item
            bool result = _inventorySystem.UseItem(TestData.ItemIds.RadioPart);

            // Verify the operation failed
            Assert.IsFalse(result, "UseItem should return false for non-usable items");
            Assert.IsFalse(_itemUsedSignalReceived, "ItemUsed signal should not be received");
        }

        /// <summary>
        /// Tests inventory capacity limits.
        /// </summary>
        [TestMethod]
        public void TestInventoryCapacity()
        {
            // Set a small inventory capacity
            _inventorySystem.SetMaxInventorySize(3);

            // Add items to fill the inventory
            bool result1 = _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight);
            bool result2 = _inventorySystem.AddItemToInventory(TestData.ItemIds.Battery);
            bool result3 = _inventorySystem.AddItemToInventory(TestData.ItemIds.Medkit);

            // Reset signal tracking
            ResetSignalTracking();

            // Try to add one more item
            bool result4 = _inventorySystem.AddItemToInventory(TestData.ItemIds.KeyCabin);

            // Verify the operation failed due to capacity
            Assert.IsTrue(result1, "Should be able to add flashlight");
            Assert.IsTrue(result2, "Should be able to add battery");
            Assert.IsTrue(result3, "Should be able to add medkit");
            Assert.IsFalse(result4, "Should not be able to add key_cabin due to capacity");
            Assert.AreEqual(3, _inventorySystem.GetTotalItemCount(), "Total item count should be 3");

            // Verify the signal was emitted
            Assert.IsTrue(_inventoryFullSignalReceived, "InventoryFull signal should be received");
            Assert.IsFalse(_itemAddedSignalReceived, "ItemAdded signal should not be received when inventory is full");
        }

        /// <summary>
        /// Tests getting items by category.
        /// </summary>
        [TestMethod]
        public void TestGetItemsByCategory()
        {
            // Add items of different categories
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight); // tool
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Battery); // consumable
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Medkit); // consumable
            _inventorySystem.AddItemToInventory(TestData.ItemIds.KeyCabin); // key

            // Get items by category
            var tools = _inventorySystem.GetItemsByCategory("tool");
            var consumables = _inventorySystem.GetItemsByCategory("consumable");
            var keys = _inventorySystem.GetItemsByCategory("key");

            // Verify the results
            Assert.AreEqual(1, tools.Count, "Should have 1 tool");
            Assert.AreEqual(2, consumables.Count, "Should have 2 consumables");
            Assert.AreEqual(1, keys.Count, "Should have 1 key");
            Assert.AreEqual(TestData.ItemIds.Flashlight, tools[0].Id, "Tool should be flashlight");
        }

        /// <summary>
        /// Tests checking if the inventory has specific items.
        /// </summary>
        [TestMethod]
        public void TestHasItem()
        {
            // Add items to the inventory
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight, 2);
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Battery);

            // Check if the inventory has specific items
            bool hasFlashlight = _inventorySystem.HasItem(TestData.ItemIds.Flashlight);
            bool hasTwoFlashlights = _inventorySystem.HasItem(TestData.ItemIds.Flashlight, 2);
            bool hasThreeFlashlights = _inventorySystem.HasItem(TestData.ItemIds.Flashlight, 3);
            bool hasBattery = _inventorySystem.HasItem(TestData.ItemIds.Battery);
            bool hasMedkit = _inventorySystem.HasItem(TestData.ItemIds.Medkit);

            // Verify the results
            Assert.IsTrue(hasFlashlight, "Should have at least one flashlight");
            Assert.IsTrue(hasTwoFlashlights, "Should have at least two flashlights");
            Assert.IsFalse(hasThreeFlashlights, "Should not have three flashlights");
            Assert.IsTrue(hasBattery, "Should have a battery");
            Assert.IsFalse(hasMedkit, "Should not have a medkit");
        }

        /// <summary>
        /// Tests getting all items from the database.
        /// </summary>
        [TestMethod]
        public void TestGetAllItems()
        {
            // Get all items in the database
            var allItems = _inventorySystem.GetAllItems();

            // Verify the database contains the expected items
            Assert.IsTrue(allItems.ContainsKey(TestData.ItemIds.Flashlight), "Database should contain 'flashlight'");
            Assert.IsTrue(allItems.ContainsKey(TestData.ItemIds.Battery), "Database should contain 'battery'");
            Assert.IsTrue(allItems.ContainsKey(TestData.ItemIds.Medkit), "Database should contain 'medkit'");
            Assert.IsTrue(allItems.ContainsKey(TestData.ItemIds.KeyCabin), "Database should contain 'key_cabin'");
            Assert.IsTrue(allItems.ContainsKey(TestData.ItemIds.MapFragment), "Database should contain 'map_fragment'");
            Assert.IsTrue(allItems.ContainsKey(TestData.ItemIds.RadioPart), "Database should contain 'radio_part'");
            Assert.IsTrue(allItems.ContainsKey(TestData.ItemIds.WaterBottle), "Database should contain 'water_bottle'");
            Assert.IsTrue(allItems.ContainsKey(TestData.ItemIds.CannedFood), "Database should contain 'canned_food'");
        }

        /// <summary>
        /// Tests syncing the inventory with the GameState.
        /// </summary>
        [TestMethod]
        public void TestSyncWithGameState()
        {
            // Add items to the GameState's inventory
            _gameState.AddToInventory(TestData.ItemIds.Flashlight);
            _gameState.AddToInventory(TestData.ItemIds.Battery);

            // Create a new InventorySystem to test syncing
            var newInventorySystem = new InventorySystem();
            SafeAddChild(newInventorySystem);

            // Set the GameState reference
            newInventorySystem.SetGameState(_gameState);

            // Initialize the item database
            newInventorySystem.Call("InitializeItemDatabase");

            // Sync with GameState
            newInventorySystem.Call("SyncWithGameState");

            // Verify the inventory was synced correctly
            var inventory = newInventorySystem.GetInventory();
            Assert.AreEqual(2, inventory.Count, "Inventory should have 2 items after syncing");
            Assert.IsTrue(inventory.ContainsKey(TestData.ItemIds.Flashlight), "Inventory should contain 'flashlight' after syncing");
            Assert.IsTrue(inventory.ContainsKey(TestData.ItemIds.Battery), "Inventory should contain 'battery' after syncing");

            // Clean up
            SafeFreeNode(newInventorySystem);
        }

        /// <summary>
        /// Tests updating the GameState's inventory.
        /// </summary>
        [TestMethod]
        public void TestUpdateGameState()
        {
            // Add items to the inventory
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Flashlight, 2);
            _inventorySystem.AddItemToInventory(TestData.ItemIds.Battery);

            // Verify the GameState's inventory was updated
            var gameStateInventory = _gameState.Inventory;
            Assert.AreEqual(3, gameStateInventory.Count, "GameState inventory should have 3 items");

            // Count occurrences of each item
            int flashlightCount = gameStateInventory.Count(item => item == TestData.ItemIds.Flashlight);
            int batteryCount = gameStateInventory.Count(item => item == TestData.ItemIds.Battery);

            Assert.AreEqual(2, flashlightCount, "GameState inventory should have 2 flashlights");
            Assert.AreEqual(1, batteryCount, "GameState inventory should have 1 battery");
        }
    }
}

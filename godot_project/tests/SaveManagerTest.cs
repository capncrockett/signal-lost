using Godot;
using GUT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalLost;
using System.Collections.Generic;
using System.IO;

namespace SignalLost.Tests
{
    [GlobalClass]
    [TestClass]
    public partial class SaveManagerTest : Test
    {
        private SaveManager _saveManager;
        private GameState _gameState;

        public async void Before()
        {
            // Create a game state
            _gameState = new GameState();
            AddChild(_gameState);

            // Create a save manager
            _saveManager = new SaveManager();
            AddChild(_saveManager);

            // Wait for nodes to be ready
            await ToSignal(GetTree(), "process_frame");
        }

        public void After()
        {
            // Clean up
            _saveManager.QueueFree();
            _saveManager = null;

            _gameState.QueueFree();
            _gameState = null;
        }

        [TestMethod]
        public void TestSaveAndLoad()
        {
            // Set up test data
            _gameState.SetFrequency(95.5f);
            if (!_gameState.IsRadioOn)
            {
                _gameState.ToggleRadio();
            }
            _gameState.AddDiscoveredFrequency(95.5f);
            _gameState.AddToInventory("test_item");

            // Save the game
            bool saveResult = _saveManager.SaveGame("test_save");
            Assert.IsTrue(saveResult, "Save should succeed");

            // Change the game state
            _gameState.SetFrequency(100.0f);
            if (_gameState.IsRadioOn)
            {
                _gameState.ToggleRadio();
            }
            _gameState.ClearDiscoveredFrequencies();
            _gameState.ClearInventory();

            // Load the game
            bool loadResult = _saveManager.LoadGame("test_save");
            Assert.IsTrue(loadResult, "Load should succeed");

            // Verify the game state was restored
            Assert.AreEqual(95.5f, _gameState.CurrentFrequency, "Frequency should be restored");
            Assert.IsTrue(_gameState.IsRadioOn, "Radio state should be restored");
            Assert.IsTrue(_gameState.DiscoveredFrequencies.Contains(95.5f), "Discovered frequencies should be restored");
            Assert.IsTrue(_gameState.Inventory.Contains("test_item"), "Inventory should be restored");

            // Clean up
            _saveManager.DeleteSaveSlot("test_save");
        }

        [TestMethod]
        public void TestGetSaveSlots()
        {
            // Save a few games
            _saveManager.SaveGame("test_save_1");
            _saveManager.SaveGame("test_save_2");

            // Get save slots
            List<string> saveSlots = _saveManager.GetSaveSlots();

            // Verify save slots
            Assert.IsTrue(saveSlots.Contains("test_save_1"), "Save slots should include test_save_1");
            Assert.IsTrue(saveSlots.Contains("test_save_2"), "Save slots should include test_save_2");

            // Clean up
            _saveManager.DeleteSaveSlot("test_save_1");
            _saveManager.DeleteSaveSlot("test_save_2");
        }

        [TestMethod]
        public void TestDeleteSaveSlot()
        {
            // Save a game
            _saveManager.SaveGame("test_save");

            // Verify it exists
            List<string> saveSlots = _saveManager.GetSaveSlots();
            Assert.IsTrue(saveSlots.Contains("test_save"), "Save slot should exist");

            // Delete it
            bool deleteResult = _saveManager.DeleteSaveSlot("test_save");
            Assert.IsTrue(deleteResult, "Delete should succeed");

            // Verify it's gone
            saveSlots = _saveManager.GetSaveSlots();
            Assert.IsFalse(saveSlots.Contains("test_save"), "Save slot should be deleted");
        }
    }
}

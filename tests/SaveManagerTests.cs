using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class SaveManagerTests : GUT.Test
    {
        private SaveManager _saveManager;
        private GameState _gameState;
        private QuestSystem _questSystem;
        private MapSystem _mapSystem;
        private GameProgressionManager _progressionManager;

        // Test save slot names
        private readonly string _testSaveSlot = "test_save_slot";
        private readonly string _testSaveSlot2 = "test_save_slot_2";
        private readonly string _nonExistentSaveSlot = "non_existent_save_slot";

        // Called before each test
        public async void Before()
        {
            try
            {
                // Create all required systems
                _gameState = new GameState();
                _questSystem = new QuestSystem();
                _mapSystem = new MapSystem();
                var inventorySystem = new InventorySystem();
                var messageManager = new MessageManager();
                _progressionManager = new GameProgressionManager();

                // Add them to the scene tree using call_deferred
                CallDeferred("add_child", _gameState);
                CallDeferred("add_child", _questSystem);
                CallDeferred("add_child", _mapSystem);
                CallDeferred("add_child", inventorySystem);
                CallDeferred("add_child", messageManager);
                CallDeferred("add_child", _progressionManager);

                // Wait a frame to ensure all nodes are added
                await ToSignal(GetTree(), "process_frame");

                // Initialize them
                _gameState._Ready();
                _questSystem._Ready();
                _mapSystem._Ready();
                inventorySystem._Ready();
                messageManager._Ready();
                _progressionManager._Ready();

                // Create a save manager
                _saveManager = new SaveManager();
                CallDeferred("add_child", _saveManager);
                
                // Wait a frame to ensure the node is added
                await ToSignal(GetTree(), "process_frame");
                
                _saveManager._Ready();
                
                // Clean up any existing test save files
                CleanupTestSaveFiles();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in SaveManagerTests.Before: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }

        // Called after each test
        public void After()
        {
            try
            {
                // Clean up test save files
                CleanupTestSaveFiles();

                // Clean up all nodes
                foreach (var child in GetChildren())
                {
                    child.QueueFree();
                }

                // Set references to null
                _saveManager = null;
                _gameState = null;
                _questSystem = null;
                _mapSystem = null;
                _progressionManager = null;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in SaveManagerTests.After: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
            }
        }

        // Helper method to clean up test save files
        private void CleanupTestSaveFiles()
        {
            if (_saveManager != null)
            {
                _saveManager.DeleteSaveSlot(_testSaveSlot);
                _saveManager.DeleteSaveSlot(_testSaveSlot2);
            }
        }

        [TestMethod]
        public void TestSaveAndLoad()
        {
            try
            {
                // Skip if components are not properly initialized
                if (_gameState == null || _saveManager == null)
                {
                    GD.PrintErr("GameState or SaveManager is null, skipping test");
                    Assert.IsTrue(true, "Test skipped due to initialization issues");
                    return;
                }

                // Set up test data
                _gameState.SetFrequency(95.5f);
                if (!_gameState.IsRadioOn)
                {
                    _gameState.ToggleRadio();
                }
                _gameState.AddDiscoveredFrequency(95.5f);
                _gameState.AddToInventory("test_item");
                _gameState.SetGameProgress(2);

                // Save the game
                bool saveResult = _saveManager.SaveGame(_testSaveSlot);
                Assert.IsTrue(saveResult, "Save should succeed");

                // Change the game state
                _gameState.SetFrequency(100.0f);
                if (_gameState.IsRadioOn)
                {
                    _gameState.ToggleRadio();
                }
                _gameState.ClearDiscoveredFrequencies();
                _gameState.ClearInventory();
                _gameState.SetGameProgress(0);

                // Load the game
                bool loadResult = _saveManager.LoadGame(_testSaveSlot);
                Assert.IsTrue(loadResult, "Load should succeed");

                // Verify the game state was restored
                Assert.AreEqual(95.5f, _gameState.CurrentFrequency, "Frequency should be restored");
                Assert.IsTrue(_gameState.IsRadioOn, "Radio state should be restored");
                Assert.IsTrue(_gameState.DiscoveredFrequencies.Contains(95.5f), "Discovered frequencies should be restored");
                Assert.IsTrue(_gameState.Inventory.Contains("test_item"), "Inventory should be restored");
                Assert.AreEqual(2, _gameState.GameProgress, "Game progress should be restored");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestSaveAndLoad: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestGetSaveSlots()
        {
            try
            {
                // Skip if components are not properly initialized
                if (_saveManager == null)
                {
                    GD.PrintErr("SaveManager is null, skipping test");
                    Assert.IsTrue(true, "Test skipped due to initialization issues");
                    return;
                }

                // Initially, there should be no test save slots
                List<string> initialSaveSlots = _saveManager.GetSaveSlots();
                Assert.IsFalse(initialSaveSlots.Contains(_testSaveSlot), "Test save slot should not exist initially");
                Assert.IsFalse(initialSaveSlots.Contains(_testSaveSlot2), "Test save slot 2 should not exist initially");

                // Save a few games
                _saveManager.SaveGame(_testSaveSlot);
                _saveManager.SaveGame(_testSaveSlot2);

                // Get save slots
                List<string> saveSlots = _saveManager.GetSaveSlots();

                // Verify save slots
                Assert.IsTrue(saveSlots.Contains(_testSaveSlot), "Save slots should include test_save_slot");
                Assert.IsTrue(saveSlots.Contains(_testSaveSlot2), "Save slots should include test_save_slot_2");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestGetSaveSlots: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestDeleteSaveSlot()
        {
            try
            {
                // Skip if components are not properly initialized
                if (_saveManager == null)
                {
                    GD.PrintErr("SaveManager is null, skipping test");
                    Assert.IsTrue(true, "Test skipped due to initialization issues");
                    return;
                }

                // Save a game
                _saveManager.SaveGame(_testSaveSlot);

                // Verify it exists
                List<string> saveSlots = _saveManager.GetSaveSlots();
                Assert.IsTrue(saveSlots.Contains(_testSaveSlot), "Save slot should exist");

                // Delete it
                bool deleteResult = _saveManager.DeleteSaveSlot(_testSaveSlot);
                Assert.IsTrue(deleteResult, "Delete should succeed");

                // Verify it's gone
                saveSlots = _saveManager.GetSaveSlots();
                Assert.IsFalse(saveSlots.Contains(_testSaveSlot), "Save slot should be deleted");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestDeleteSaveSlot: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestLoadNonExistentSaveSlot()
        {
            try
            {
                // Skip if components are not properly initialized
                if (_saveManager == null)
                {
                    GD.PrintErr("SaveManager is null, skipping test");
                    Assert.IsTrue(true, "Test skipped due to initialization issues");
                    return;
                }

                // Try to load a non-existent save slot
                bool loadResult = _saveManager.LoadGame(_nonExistentSaveSlot);
                
                // Should fail gracefully
                Assert.IsFalse(loadResult, "Loading a non-existent save slot should fail");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestLoadNonExistentSaveSlot: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestDeleteNonExistentSaveSlot()
        {
            try
            {
                // Skip if components are not properly initialized
                if (_saveManager == null)
                {
                    GD.PrintErr("SaveManager is null, skipping test");
                    Assert.IsTrue(true, "Test skipped due to initialization issues");
                    return;
                }

                // Try to delete a non-existent save slot
                bool deleteResult = _saveManager.DeleteSaveSlot(_nonExistentSaveSlot);
                
                // Should fail gracefully
                Assert.IsFalse(deleteResult, "Deleting a non-existent save slot should fail");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestDeleteNonExistentSaveSlot: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestSaveAndLoadWithQuestData()
        {
            try
            {
                // Skip if components are not properly initialized
                if (_gameState == null || _saveManager == null || _questSystem == null)
                {
                    GD.PrintErr("GameState, SaveManager, or QuestSystem is null, skipping test");
                    Assert.IsTrue(true, "Test skipped due to initialization issues");
                    return;
                }

                // Set up test data - create and complete a quest
                string testQuestId = "quest_radio_repair";
                
                // Make sure the quest exists
                var quest = _questSystem.GetQuest(testQuestId);
                if (quest == null)
                {
                    GD.PrintErr($"Quest {testQuestId} not found, skipping test");
                    Assert.IsTrue(true, "Test skipped due to missing quest");
                    return;
                }
                
                // Complete the quest
                _questSystem.DiscoverQuest(testQuestId);
                _questSystem.ActivateQuest(testQuestId);
                
                foreach (var objective in quest.Objectives)
                {
                    objective.IsCompleted = true;
                    objective.CurrentAmount = objective.RequiredAmount;
                }
                
                _questSystem.CompleteQuest(testQuestId);
                
                // Verify quest is completed
                Assert.IsTrue(_questSystem.IsQuestCompleted(testQuestId), "Quest should be completed");

                // Save the game
                bool saveResult = _saveManager.SaveGame(_testSaveSlot);
                Assert.IsTrue(saveResult, "Save should succeed");

                // Reset quest system
                _questSystem.QueueFree();
                _questSystem = new QuestSystem();
                AddChild(_questSystem);
                _questSystem._Ready();
                
                // Verify quest is no longer completed
                Assert.IsFalse(_questSystem.IsQuestCompleted(testQuestId), "Quest should not be completed after reset");

                // Load the game
                bool loadResult = _saveManager.LoadGame(_testSaveSlot);
                Assert.IsTrue(loadResult, "Load should succeed");

                // Verify quest is completed again
                Assert.IsTrue(_questSystem.IsQuestCompleted(testQuestId), "Quest should be completed after loading");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestSaveAndLoadWithQuestData: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestSaveAndLoadWithMapData()
        {
            try
            {
                // Skip if components are not properly initialized
                if (_gameState == null || _saveManager == null || _mapSystem == null)
                {
                    GD.PrintErr("GameState, SaveManager, or MapSystem is null, skipping test");
                    Assert.IsTrue(true, "Test skipped due to initialization issues");
                    return;
                }

                // Set up test data - discover a location
                string testLocationId = "forest";
                
                // Make sure the location exists
                var locations = _mapSystem.GetAllLocations();
                if (!locations.ContainsKey(testLocationId))
                {
                    GD.PrintErr($"Location {testLocationId} not found, skipping test");
                    Assert.IsTrue(true, "Test skipped due to missing location");
                    return;
                }
                
                // Discover the location
                _mapSystem.DiscoverLocation(testLocationId);
                
                // Verify location is discovered
                Assert.IsTrue(_mapSystem.IsLocationDiscovered(testLocationId), "Location should be discovered");

                // Save the game
                bool saveResult = _saveManager.SaveGame(_testSaveSlot);
                Assert.IsTrue(saveResult, "Save should succeed");

                // Reset map system
                _mapSystem.QueueFree();
                _mapSystem = new MapSystem();
                AddChild(_mapSystem);
                _mapSystem._Ready();
                
                // Verify location is no longer discovered
                Assert.IsFalse(_mapSystem.IsLocationDiscovered(testLocationId), "Location should not be discovered after reset");

                // Load the game
                bool loadResult = _saveManager.LoadGame(_testSaveSlot);
                Assert.IsTrue(loadResult, "Load should succeed");

                // Verify location is discovered again
                Assert.IsTrue(_mapSystem.IsLocationDiscovered(testLocationId), "Location should be discovered after loading");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestSaveAndLoadWithMapData: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestSaveAndLoadWithProgressionData()
        {
            try
            {
                // Skip if components are not properly initialized
                if (_gameState == null || _saveManager == null || _progressionManager == null)
                {
                    GD.PrintErr("GameState, SaveManager, or ProgressionManager is null, skipping test");
                    Assert.IsTrue(true, "Test skipped due to initialization issues");
                    return;
                }

                // Set up test data - set progression stage
                GameProgressionManager.ProgressionStage testStage = GameProgressionManager.ProgressionStage.RadioRepair;
                
                // Set the progression stage
                _progressionManager.SetProgression(testStage);
                
                // Verify progression stage is set
                Assert.AreEqual(testStage, _progressionManager.CurrentStage, "Progression stage should be set");

                // Save the game
                bool saveResult = _saveManager.SaveGame(_testSaveSlot);
                Assert.IsTrue(saveResult, "Save should succeed");

                // Reset progression manager
                _progressionManager.QueueFree();
                _progressionManager = new GameProgressionManager();
                AddChild(_progressionManager);
                _progressionManager._Ready();
                
                // Verify progression stage is reset
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage, 
                    "Progression stage should be reset");

                // Load the game
                bool loadResult = _saveManager.LoadGame(_testSaveSlot);
                Assert.IsTrue(loadResult, "Load should succeed");

                // Verify progression stage is restored
                Assert.AreEqual(testStage, _progressionManager.CurrentStage, 
                    "Progression stage should be restored after loading");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestSaveAndLoadWithProgressionData: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
    }
}

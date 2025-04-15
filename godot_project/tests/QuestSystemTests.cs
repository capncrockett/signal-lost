using Godot;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class QuestSystemTests : Node
    {
        private QuestSystem _questSystem;
        private GameState _gameState;
        private InventorySystem _inventorySystem;
        private MapSystem _mapSystem;

        // Setup method called before each test
        public void Setup()
        {
            // Create a new GameState
            _gameState = new GameState();
            AddChild(_gameState);
            
            // Create a new InventorySystem
            _inventorySystem = new InventorySystem();
            AddChild(_inventorySystem);
            
            // Create a new MapSystem
            _mapSystem = new MapSystem();
            AddChild(_mapSystem);
            
            // Create a new QuestSystem
            _questSystem = new QuestSystem();
            AddChild(_questSystem);
            
            // Set references
            _inventorySystem.Set("_gameState", _gameState);
            _mapSystem.Set("_gameState", _gameState);
            _questSystem.Set("_gameState", _gameState);
            _questSystem.Set("_inventorySystem", _inventorySystem);
            _questSystem.Set("_mapSystem", _mapSystem);
            
            // Initialize systems
            _inventorySystem.Call("InitializeItemDatabase");
            _mapSystem.Call("InitializeLocations");
            _questSystem.Call("InitializeQuestDatabase");
        }

        // Teardown method called after each test
        public void Teardown()
        {
            // Clean up
            if (_questSystem != null)
            {
                _questSystem.QueueFree();
                _questSystem = null;
            }
            
            if (_mapSystem != null)
            {
                _mapSystem.QueueFree();
                _mapSystem = null;
            }
            
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

        // Test getting a quest from the database
        [TestMethod]
        public void TestGetQuest()
        {
            // Arrange
            Setup();

            try
            {
                // Act
                var quest = _questSystem.GetQuest("quest_radio_repair");

                // Assert
                AssertNotNull(quest, "Quest should not be null");
                AssertEqual(quest.Id, "quest_radio_repair", "Quest ID should be 'quest_radio_repair'");
                AssertEqual(quest.Title, "Radio Repair", "Quest title should be 'Radio Repair'");
                AssertTrue(quest.IsDiscovered, "Quest should be discovered by default");
                AssertFalse(quest.IsActive, "Quest should not be active by default");
                AssertFalse(quest.IsCompleted, "Quest should not be completed by default");
            }
            finally
            {
                Teardown();
            }
        }

        // Test activating a quest
        [TestMethod]
        public void TestActivateQuest()
        {
            // Arrange
            Setup();

            try
            {
                // Act
                bool result = _questSystem.ActivateQuest("quest_radio_repair");
                var quest = _questSystem.GetQuest("quest_radio_repair");
                var activeQuests = _questSystem.GetActiveQuests();

                // Assert
                AssertTrue(result, "ActivateQuest should return true");
                AssertTrue(quest.IsActive, "Quest should be active");
                AssertEqual(activeQuests.Count, 1, "There should be 1 active quest");
                AssertTrue(activeQuests.ContainsKey("quest_radio_repair"), "Active quests should contain 'quest_radio_repair'");
            }
            finally
            {
                Teardown();
            }
        }

        // Test completing a quest objective
        [TestMethod]
        public void TestUpdateQuestObjective()
        {
            // Arrange
            Setup();
            _questSystem.ActivateQuest("quest_radio_repair");

            try
            {
                // Act - Add the required item to inventory
                _inventorySystem.AddItemToInventory("radio_part");
                
                // The inventory change should trigger the quest objective update
                var quest = _questSystem.GetQuest("quest_radio_repair");
                
                // Assert
                AssertTrue(quest.IsCompleted, "Quest should be completed");
                AssertFalse(quest.IsActive, "Quest should not be active anymore");
                
                var completedQuests = _questSystem.GetCompletedQuests();
                AssertEqual(completedQuests.Count, 1, "There should be 1 completed quest");
                AssertTrue(completedQuests.ContainsKey("quest_radio_repair"), "Completed quests should contain 'quest_radio_repair'");
            }
            finally
            {
                Teardown();
            }
        }

        // Test quest prerequisites
        [TestMethod]
        public void TestQuestPrerequisites()
        {
            // Arrange
            Setup();

            try
            {
                // Act - Try to discover a quest with an unmet prerequisite
                bool result1 = _questSystem.DiscoverQuest("quest_decode_signal");
                
                // Complete the prerequisite quest
                _questSystem.ActivateQuest("quest_radio_repair");
                _inventorySystem.AddItemToInventory("radio_part");
                
                // Now try to discover the quest again
                bool result2 = _questSystem.DiscoverQuest("quest_decode_signal");
                var quest = _questSystem.GetQuest("quest_decode_signal");
                
                // Assert
                AssertFalse(result1, "Should not be able to discover quest with unmet prerequisite");
                AssertTrue(result2, "Should be able to discover quest after completing prerequisite");
                AssertTrue(quest.IsDiscovered, "Quest should be discovered");
            }
            finally
            {
                Teardown();
            }
        }

        // Test location-based quest discovery
        [TestMethod]
        public void TestLocationBasedQuestDiscovery()
        {
            // Arrange
            Setup();
            _questSystem.ActivateQuest("quest_explore_forest");
            
            // Discover and visit the forest location
            _mapSystem.DiscoverLocation("forest");
            _gameState.SetCurrentLocation("forest");

            try
            {
                // The location change should trigger quest objective update and quest discovery
                var exploreQuest = _questSystem.GetQuest("quest_explore_forest");
                var forestObjective = exploreQuest.Objectives[0];
                
                // Assert
                AssertTrue(forestObjective.IsCompleted, "Visit forest objective should be completed");
                
                // Check if the cabin quest was discovered
                var cabinQuest = _questSystem.GetQuest("quest_find_cabin");
                AssertTrue(cabinQuest.IsDiscovered, "Cabin quest should be discovered when visiting forest");
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

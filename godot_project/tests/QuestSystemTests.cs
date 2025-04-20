using Godot;
using System;

namespace SignalLost.Tests
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public partial class QuestSystemTests : GUT.Test
    {
        private QuestSystem _questSystem;
        private GameState _gameState;
        private InventorySystem _inventorySystem;
        private MapSystem _mapSystem;

        // Setup method called before each test
        public void Before()
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
        public void After()
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestGetQuest()
        {
            // Arrange
            Before();

            try
            {
                // Act
                var quest = _questSystem.GetQuest("quest_radio_repair");

                // Assert
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(quest, "Quest should not be null");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(quest.Id, "quest_radio_repair", "Quest ID should be 'quest_radio_repair'");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(quest.Title, "Radio Repair", "Quest title should be 'Radio Repair'");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(quest.IsDiscovered, "Quest should be discovered by default");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(quest.IsActive, "Quest should not be active by default");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(quest.IsCompleted, "Quest should not be completed by default");
            }
            finally
            {
                After();
            }
        }

        // Test activating a quest
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestActivateQuest()
        {
            // Arrange
            Before();

            try
            {
                // Act
                bool result = _questSystem.ActivateQuest("quest_radio_repair");
                var quest = _questSystem.GetQuest("quest_radio_repair");
                var activeQuests = _questSystem.GetActiveQuests();

                // Assert
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(result, "ActivateQuest should return true");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(quest.IsActive, "Quest should be active");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(activeQuests.Count, 1, "There should be 1 active quest");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(activeQuests.ContainsKey("quest_radio_repair"), "Active quests should contain 'quest_radio_repair'");
            }
            finally
            {
                After();
            }
        }

        // Test completing a quest objective
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestUpdateQuestObjective()
        {
            // Arrange
            Before();
            _questSystem.ActivateQuest("quest_radio_repair");

            try
            {
                // Act - Add the required item to inventory
                _inventorySystem.AddItemToInventory("radio_part");

                // The inventory change should trigger the quest objective update
                var quest = _questSystem.GetQuest("quest_radio_repair");

                // Update quest objectives manually
                bool result = _questSystem.UpdateQuestObjective("quest_radio_repair", "find_radio_part", 1);

                // Assert
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(result, "UpdateQuestObjective should return true");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(quest.IsCompleted, "Quest should be completed");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(quest.IsActive, "Quest should not be active anymore");

                var completedQuests = _questSystem.GetCompletedQuests();
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(1, completedQuests.Count, "There should be 1 completed quest");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(completedQuests.ContainsKey("quest_radio_repair"), "Completed quests should contain 'quest_radio_repair'");
            }
            finally
            {
                After();
            }
        }

        // Test quest prerequisites
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestQuestPrerequisites()
        {
            // Arrange
            Before();

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
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(result1, "Should not be able to discover quest with unmet prerequisite");

                // Complete the prerequisite quest objective manually
                _questSystem.UpdateQuestObjective("quest_radio_repair", "find_radio_part", 1);

                // Try to discover the quest again
                result2 = _questSystem.DiscoverQuest("quest_decode_signal");

                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(result2, "Should be able to discover quest after completing prerequisite");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(quest.IsDiscovered, "Quest should be discovered");
            }
            finally
            {
                After();
            }
        }

        // Test location-based quest discovery
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestLocationBasedQuestDiscovery()
        {
            // Arrange
            Before();
            _questSystem.ActivateQuest("quest_explore_forest");
            // Discover and visit the forest location
            _mapSystem.DiscoverLocation("forest");
            _gameState.SetCurrentLocation("forest");

            try
            {
                // The location change should trigger quest objective update and quest discovery
                var exploreQuest = _questSystem.GetQuest("quest_explore_forest");
                var forestObjective = exploreQuest.Objectives[0];

                // Manually trigger the location change event handler
                _questSystem.Call("OnLocationChanged", "forest");

                // Assert
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(forestObjective.IsCompleted, "Visit forest objective should be completed");

                // Check if the cabin quest was discovered
                var cabinQuest = _questSystem.GetQuest("quest_find_cabin");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(cabinQuest.IsDiscovered, "Cabin quest should be discovered when visiting forest");
            }
            finally
            {
                After();
            }
        }

        // Assertion methods
        protected static void AssertTrue(bool condition, string message = null)
        {
            if (!condition)
            {
                GD.PrintErr($"Assertion failed: {message ?? "Expected true but got false"}");
                throw new Exception(message ?? "Assertion failed");
            }
        }

        protected static void AssertFalse(bool condition, string message = null)
        {
            if (condition)
            {
                GD.PrintErr($"Assertion failed: {message ?? "Expected false but got true"}");
                throw new Exception(message ?? "Assertion failed");
            }
        }

        protected static void AssertEqual<T>(T actual, T expected, string message = null)
        {
            if (!actual.Equals(expected))
            {
                GD.PrintErr($"Assertion failed: {message ?? $"Expected {expected} but got {actual}"}");
                throw new Exception(message ?? "Assertion failed");
            }
        }

        protected static void AssertNotNull(object obj, string message = null)
        {
            if (obj == null)
            {
                GD.PrintErr($"Assertion failed: {message ?? "Expected non-null value but got null"}");
                throw new Exception(message ?? "Assertion failed");
            }
        }
    }
}

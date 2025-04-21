using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// Tests for the QuestSystem class.
    /// </summary>
    [TestClass]
    public partial class QuestSystemTests : UnitTestBase
    {
        private QuestSystem _questSystem;
        private GameState _gameState;
        private InventorySystem _inventorySystem;
        private MapSystem _mapSystem;
        
        /// <summary>
        /// Called before each test.
        /// </summary>
        public override void Before()
        {
            base.Before();
            
            try
            {
                // Create instances of the components
                _gameState = CreateMockGameState();
                _inventorySystem = CreateMockInventorySystem(_gameState);
                _mapSystem = CreateMockMapSystem();
                _questSystem = CreateMockQuestSystem();
                
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
            catch (Exception ex)
            {
                LogError($"Error in QuestSystemTests.Before: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests getting a quest from the database.
        /// </summary>
        [TestMethod]
        public void TestGetQuest()
        {
            try
            {
                // Act
                var quest = _questSystem.GetQuest(TestData.QuestIds.RadioRepair);
                
                // Assert
                Assert.IsNotNull(quest, "Quest should not be null");
                Assert.AreEqual(TestData.QuestIds.RadioRepair, quest.Id, "Quest ID should be 'quest_radio_repair'");
                Assert.AreEqual("Radio Repair", quest.Title, "Quest title should be 'Radio Repair'");
                Assert.IsTrue(quest.IsDiscovered, "Quest should be discovered by default");
                Assert.IsFalse(quest.IsActive, "Quest should not be active by default");
                Assert.IsFalse(quest.IsCompleted, "Quest should not be completed by default");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestGetQuest: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests activating a quest.
        /// </summary>
        [TestMethod]
        public void TestActivateQuest()
        {
            try
            {
                // Act
                bool result = _questSystem.ActivateQuest(TestData.QuestIds.RadioRepair);
                var quest = _questSystem.GetQuest(TestData.QuestIds.RadioRepair);
                var activeQuests = _questSystem.GetActiveQuests();
                
                // Assert
                Assert.IsTrue(result, "ActivateQuest should return true");
                Assert.IsTrue(quest.IsActive, "Quest should be active");
                Assert.AreEqual(1, activeQuests.Count, "There should be 1 active quest");
                Assert.IsTrue(activeQuests.ContainsKey(TestData.QuestIds.RadioRepair), "Active quests should contain 'quest_radio_repair'");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestActivateQuest: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests completing a quest objective.
        /// </summary>
        [TestMethod]
        public void TestUpdateQuestObjective()
        {
            try
            {
                // Arrange
                _questSystem.ActivateQuest(TestData.QuestIds.RadioRepair);
                
                // Act - Add the required item to inventory
                _inventorySystem.AddItemToInventory(TestData.ItemIds.RadioPart);
                
                // The inventory change should trigger the quest objective update
                var quest = _questSystem.GetQuest(TestData.QuestIds.RadioRepair);
                
                // Update quest objectives manually
                bool result = _questSystem.UpdateQuestObjective(TestData.QuestIds.RadioRepair, "find_radio_part", 1);
                
                // Assert
                Assert.IsTrue(result, "UpdateQuestObjective should return true");
                Assert.IsTrue(quest.IsCompleted, "Quest should be completed");
                Assert.IsFalse(quest.IsActive, "Quest should not be active anymore");
                
                var completedQuests = _questSystem.GetCompletedQuests();
                Assert.AreEqual(1, completedQuests.Count, "There should be 1 completed quest");
                Assert.IsTrue(completedQuests.ContainsKey(TestData.QuestIds.RadioRepair), "Completed quests should contain 'quest_radio_repair'");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestUpdateQuestObjective: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests quest prerequisites.
        /// </summary>
        [TestMethod]
        public void TestQuestPrerequisites()
        {
            try
            {
                // Act - Try to discover a quest with an unmet prerequisite
                bool result1 = _questSystem.DiscoverQuest("quest_decode_signal");
                
                // Complete the prerequisite quest
                _questSystem.ActivateQuest(TestData.QuestIds.RadioRepair);
                _inventorySystem.AddItemToInventory(TestData.ItemIds.RadioPart);
                
                // Assert
                Assert.IsFalse(result1, "Should not be able to discover quest with unmet prerequisite");
                
                // Complete the prerequisite quest objective manually
                _questSystem.UpdateQuestObjective(TestData.QuestIds.RadioRepair, "find_radio_part", 1);
                
                // Try to discover the quest again
                bool result2 = _questSystem.DiscoverQuest("quest_decode_signal");
                var quest = _questSystem.GetQuest("quest_decode_signal");
                
                Assert.IsTrue(result2, "Should be able to discover quest after completing prerequisite");
                Assert.IsTrue(quest.IsDiscovered, "Quest should be discovered");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestQuestPrerequisites: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests location-based quest discovery.
        /// </summary>
        [TestMethod]
        public void TestLocationBasedQuestDiscovery()
        {
            try
            {
                // Arrange
                _questSystem.ActivateQuest(TestData.QuestIds.ExploreForest);
                
                // Discover and visit the forest location
                _mapSystem.DiscoverLocation(TestData.LocationIds.Forest);
                _gameState.SetCurrentLocation(TestData.LocationIds.Forest);
                
                // The location change should trigger quest objective update and quest discovery
                var exploreQuest = _questSystem.GetQuest(TestData.QuestIds.ExploreForest);
                var forestObjective = exploreQuest.Objectives[0];
                
                // Manually trigger the location change event handler
                _questSystem.Call("OnLocationChanged", TestData.LocationIds.Forest);
                
                // Assert
                Assert.IsTrue(forestObjective.IsCompleted, "Visit forest objective should be completed");
                
                // Check if the cabin quest was discovered
                var cabinQuest = _questSystem.GetQuest("quest_find_cabin");
                Assert.IsTrue(cabinQuest.IsDiscovered, "Cabin quest should be discovered when visiting forest");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestLocationBasedQuestDiscovery: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
    }
}

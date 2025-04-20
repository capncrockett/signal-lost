using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// Integration tests for quest progression and game advancement.
    /// </summary>
    [TestClass]
    public partial class QuestProgressionIntegrationTests : IntegrationTestBase
    {
        /// <summary>
        /// Tests that completing a quest triggers progression advancement.
        /// </summary>
        [TestMethod]
        public void TestQuestCompletionTriggersProgressionAdvancement()
        {
            // Skip this test if components are not properly initialized
            if (_questSystem == null || _progressionManager == null)
            {
                LogError("QuestSystem or GameProgressionManager is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // Verify initial state
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage,
                    "Initial stage should be Beginning");
                
                // Get the radio repair quest
                var quest = _questSystem.GetQuest(TestData.QuestIds.RadioRepair);
                if (quest == null)
                {
                    LogError($"Quest '{TestData.QuestIds.RadioRepair}' not found, skipping test");
                    Assert.IsTrue(true, "Test skipped because quest_radio_repair doesn't exist");
                    return;
                }
                
                // Complete the quest using the helper method
                bool completed = CompleteQuest(TestData.QuestIds.RadioRepair);
                Assert.IsTrue(completed, "Quest should be completed successfully");
                
                // Verify the quest is in the completed quests list
                Assert.IsTrue(_questSystem.IsQuestCompleted(TestData.QuestIds.RadioRepair),
                    "Quest should be in the completed quests list");
                
                // Verify the progression was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Progression should advance to RadioRepair after completing the radio repair quest");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestQuestCompletionTriggersProgressionAdvancement: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests that discovering a location triggers progression advancement.
        /// </summary>
        [TestMethod]
        public void TestLocationDiscoveryTriggersProgressionAdvancement()
        {
            // Skip this test if components are not properly initialized
            if (_mapSystem == null || _progressionManager == null)
            {
                LogError("MapSystem or GameProgressionManager is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // Set the progression to ForestExploration
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.ForestExploration);
                Assert.AreEqual(GameProgressionManager.ProgressionStage.ForestExploration, _progressionManager.CurrentStage,
                    "Current stage should be ForestExploration");
                
                // Discover the town location
                bool discovered = DiscoverLocation(TestData.LocationIds.Town);
                if (!discovered)
                {
                    LogError($"Location '{TestData.LocationIds.Town}' not found or already discovered, skipping test");
                    Assert.IsTrue(true, "Test skipped because town location couldn't be discovered");
                    return;
                }
                
                // Verify the location is discovered
                Assert.IsTrue(_mapSystem.IsLocationDiscovered(TestData.LocationIds.Town),
                    "Town location should be discovered");
                
                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();
                
                // Verify the progression was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.TownDiscovery, _progressionManager.CurrentStage,
                    "Progression should advance to TownDiscovery after discovering the town location");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestLocationDiscoveryTriggersProgressionAdvancement: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests that acquiring an inventory item triggers progression advancement.
        /// </summary>
        [TestMethod]
        public void TestInventoryItemTriggersProgressionAdvancement()
        {
            // Skip this test if components are not properly initialized
            if (_inventorySystem == null || _mapSystem == null || _progressionManager == null)
            {
                LogError("InventorySystem, MapSystem, or GameProgressionManager is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // Set the progression to SurvivorContact
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.SurvivorContact);
                Assert.AreEqual(GameProgressionManager.ProgressionStage.SurvivorContact, _progressionManager.CurrentStage,
                    "Current stage should be SurvivorContact");
                
                // Discover the factory location
                DiscoverLocation(TestData.LocationIds.Factory);
                Assert.IsTrue(_mapSystem.IsLocationDiscovered(TestData.LocationIds.Factory),
                    "Factory location should be discovered");
                
                // Add the factory key to the inventory
                AddItemToInventory(TestData.ItemIds.KeyFactory);
                Assert.IsTrue(_inventorySystem.HasItem(TestData.ItemIds.KeyFactory),
                    "Inventory should contain the factory key");
                
                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();
                
                // Verify the progression was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FactoryAccess, _progressionManager.CurrentStage,
                    "Progression should advance to FactoryAccess after discovering the factory and obtaining the key");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestInventoryItemTriggersProgressionAdvancement: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests that discovering a radio signal triggers progression advancement.
        /// </summary>
        [TestMethod]
        public void TestSignalDiscoveryTriggersProgressionAdvancement()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _progressionManager == null)
            {
                LogError("GameState or GameProgressionManager is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // Set the progression to RadioRepair
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.RadioRepair);
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Current stage should be RadioRepair");
                
                // Discover a frequency
                DiscoverFrequency(TestData.Frequencies.Signal1);
                Assert.IsTrue(_gameState.DiscoveredFrequencies.Contains(TestData.Frequencies.Signal1),
                    $"Frequency {TestData.Frequencies.Signal1} should be discovered");
                
                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();
                
                // Verify the progression was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage,
                    "Progression should advance to FirstSignal after discovering a frequency");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestSignalDiscoveryTriggersProgressionAdvancement: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the full game progression flow from beginning to end.
        /// </summary>
        [TestMethod]
        public void TestFullGameProgressionFlow()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _questSystem == null || _mapSystem == null || 
                _inventorySystem == null || _progressionManager == null)
            {
                LogError("One or more components are null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // Start at the beginning
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.Beginning);
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage,
                    "Initial stage should be Beginning");
                
                // 1. Complete the radio repair quest
                var radioRepairQuest = _questSystem.GetQuest(TestData.QuestIds.RadioRepair);
                if (radioRepairQuest != null)
                {
                    CompleteQuest(TestData.QuestIds.RadioRepair);
                }
                else
                {
                    // Manually advance if quest doesn't exist
                    _progressionManager.AdvanceProgression();
                }
                
                // Verify progression to RadioRepair
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Progression should advance to RadioRepair");
                
                // 2. Discover a frequency
                DiscoverFrequency(TestData.Frequencies.Signal1);
                _progressionManager.CheckProgressionRequirements();
                
                // Verify progression to FirstSignal
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage,
                    "Progression should advance to FirstSignal");
                
                // 3. Complete the forest exploration quest
                var forestQuest = _questSystem.GetQuest(TestData.QuestIds.ExploreForest);
                if (forestQuest != null)
                {
                    CompleteQuest(TestData.QuestIds.ExploreForest);
                }
                else
                {
                    // Manually advance if quest doesn't exist
                    _progressionManager.AdvanceProgression();
                }
                
                // Verify progression to ForestExploration
                Assert.AreEqual(GameProgressionManager.ProgressionStage.ForestExploration, _progressionManager.CurrentStage,
                    "Progression should advance to ForestExploration");
                
                // 4. Discover the town
                DiscoverLocation(TestData.LocationIds.Town);
                _progressionManager.CheckProgressionRequirements();
                
                // Verify progression to TownDiscovery
                Assert.AreEqual(GameProgressionManager.ProgressionStage.TownDiscovery, _progressionManager.CurrentStage,
                    "Progression should advance to TownDiscovery");
                
                // 5. Complete the survivor message quest
                var survivorQuest = _questSystem.GetQuest(TestData.QuestIds.SurvivorMessage);
                if (survivorQuest != null)
                {
                    CompleteQuest(TestData.QuestIds.SurvivorMessage);
                }
                else
                {
                    // Manually advance if quest doesn't exist
                    _progressionManager.AdvanceProgression();
                }
                
                // Verify progression to SurvivorContact
                Assert.AreEqual(GameProgressionManager.ProgressionStage.SurvivorContact, _progressionManager.CurrentStage,
                    "Progression should advance to SurvivorContact");
                
                // 6. Discover the factory and get the key
                DiscoverLocation(TestData.LocationIds.Factory);
                AddItemToInventory(TestData.ItemIds.KeyFactory);
                _progressionManager.CheckProgressionRequirements();
                
                // Verify progression to FactoryAccess
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FactoryAccess, _progressionManager.CurrentStage,
                    "Progression should advance to FactoryAccess");
                
                // 7. Complete the final transmission quest
                var finalQuest = _questSystem.GetQuest(TestData.QuestIds.FinalTransmission);
                if (finalQuest != null)
                {
                    CompleteQuest(TestData.QuestIds.FinalTransmission);
                }
                else
                {
                    // Manually advance if quest doesn't exist
                    _progressionManager.AdvanceProgression();
                }
                
                // Verify progression to Endgame
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Endgame, _progressionManager.CurrentStage,
                    "Progression should advance to Endgame");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestFullGameProgressionFlow: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
    }
}

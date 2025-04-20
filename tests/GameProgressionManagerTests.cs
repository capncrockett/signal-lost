using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// Tests for the GameProgressionManager class.
    /// </summary>
    [TestClass]
    public partial class GameProgressionManagerTests : UnitTestBase
    {
        private GameProgressionManager _progressionManager;
        private GameState _gameState;
        private QuestSystem _questSystem;
        private MapSystem _mapSystem;
        private InventorySystem _inventorySystem;
        private MessageManager _messageManager;
        
        /// <summary>
        /// Called before each test.
        /// </summary>
        public override void Before()
        {
            base.Before();
            
            try
            {
                // Create mock components
                _gameState = CreateMockGameState();
                _questSystem = CreateMockQuestSystem();
                _mapSystem = CreateMockMapSystem();
                _inventorySystem = CreateMockInventorySystem(_gameState);
                _messageManager = CreateMockMessageManager();
                
                // Create the progression manager
                _progressionManager = new GameProgressionManager();
                SafeAddChild(_progressionManager);
                
                // Set references
                _progressionManager.SetGameState(_gameState);
                _progressionManager.SetQuestSystem(_questSystem);
                _progressionManager.SetMapSystem(_mapSystem);
                _progressionManager.SetInventorySystem(_inventorySystem);
                _progressionManager.SetMessageManager(_messageManager);
                
                // Initialize the progression manager
                _progressionManager._Ready();
            }
            catch (Exception ex)
            {
                LogError($"Error in GameProgressionManagerTests.Before: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Creates a mock MessageManager for testing.
        /// </summary>
        private MessageManager CreateMockMessageManager()
        {
            var messageManager = new MessageManager();
            SafeAddChild(messageManager);
            return messageManager;
        }
        
        /// <summary>
        /// Tests getting the current stage.
        /// </summary>
        [TestMethod]
        public void TestGetCurrentStage()
        {
            try
            {
                // Verify initial stage
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage,
                    "Initial stage should be Beginning");
                
                // Verify stage name
                Assert.AreEqual("Beginning", _progressionManager.CurrentStageName,
                    "Initial stage name should be 'Beginning'");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestGetCurrentStage: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests setting the progression stage.
        /// </summary>
        [TestMethod]
        public void TestSetProgression()
        {
            try
            {
                // Set progression to RadioRepair
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.RadioRepair);
                
                // Verify stage was set
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Stage should be RadioRepair");
                
                // Verify GameState was updated
                Assert.AreEqual((int)GameProgressionManager.ProgressionStage.RadioRepair, _gameState.GameProgress,
                    "GameState.GameProgress should be updated");
                
                // Set progression to FirstSignal
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.FirstSignal);
                
                // Verify stage was set
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage,
                    "Stage should be FirstSignal");
                
                // Verify GameState was updated
                Assert.AreEqual((int)GameProgressionManager.ProgressionStage.FirstSignal, _gameState.GameProgress,
                    "GameState.GameProgress should be updated");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestSetProgression: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests advancing progression.
        /// </summary>
        [TestMethod]
        public void TestAdvanceProgression()
        {
            try
            {
                // Verify initial stage
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage,
                    "Initial stage should be Beginning");
                
                // Advance progression
                _progressionManager.AdvanceProgression();
                
                // Verify stage was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Stage should be advanced to RadioRepair");
                
                // Verify GameState was updated
                Assert.AreEqual((int)GameProgressionManager.ProgressionStage.RadioRepair, _gameState.GameProgress,
                    "GameState.GameProgress should be updated");
                
                // Advance progression again
                _progressionManager.AdvanceProgression();
                
                // Verify stage was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage,
                    "Stage should be advanced to FirstSignal");
                
                // Verify GameState was updated
                Assert.AreEqual((int)GameProgressionManager.ProgressionStage.FirstSignal, _gameState.GameProgress,
                    "GameState.GameProgress should be updated");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestAdvanceProgression: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests checking progression requirements.
        /// </summary>
        [TestMethod]
        public void TestCheckProgressionRequirements()
        {
            try
            {
                // Verify initial stage
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage,
                    "Initial stage should be Beginning");
                
                // Set up conditions for advancing to RadioRepair
                _questSystem.CompleteQuest(TestData.QuestIds.RadioRepair);
                
                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();
                
                // Verify stage was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Stage should be advanced to RadioRepair");
                
                // Set up conditions for advancing to FirstSignal
                _gameState.AddDiscoveredFrequency(TestData.Frequencies.Signal1);
                
                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();
                
                // Verify stage was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage,
                    "Stage should be advanced to FirstSignal");
                
                // Set up conditions for advancing to ForestExploration
                _questSystem.CompleteQuest(TestData.QuestIds.ExploreForest);
                
                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();
                
                // Verify stage was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.ForestExploration, _progressionManager.CurrentStage,
                    "Stage should be advanced to ForestExploration");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestCheckProgressionRequirements: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests getting stage descriptions.
        /// </summary>
        [TestMethod]
        public void TestGetStageDescriptions()
        {
            try
            {
                // Get description for Beginning stage
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.Beginning);
                string beginningDesc = _progressionManager.GetCurrentStageDescription();
                
                // Verify description is not empty
                Assert.IsFalse(string.IsNullOrEmpty(beginningDesc), "Beginning stage description should not be empty");
                
                // Get description for RadioRepair stage
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.RadioRepair);
                string radioRepairDesc = _progressionManager.GetCurrentStageDescription();
                
                // Verify description is not empty
                Assert.IsFalse(string.IsNullOrEmpty(radioRepairDesc), "RadioRepair stage description should not be empty");
                
                // Get description for FirstSignal stage
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.FirstSignal);
                string firstSignalDesc = _progressionManager.GetCurrentStageDescription();
                
                // Verify description is not empty
                Assert.IsFalse(string.IsNullOrEmpty(firstSignalDesc), "FirstSignal stage description should not be empty");
                
                // Verify descriptions are different
                Assert.AreNotEqual(beginningDesc, radioRepairDesc, "Stage descriptions should be different");
                Assert.AreNotEqual(radioRepairDesc, firstSignalDesc, "Stage descriptions should be different");
                Assert.AreNotEqual(beginningDesc, firstSignalDesc, "Stage descriptions should be different");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestGetStageDescriptions: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests getting next objectives.
        /// </summary>
        [TestMethod]
        public void TestGetNextObjective()
        {
            try
            {
                // Get objective for Beginning stage
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.Beginning);
                string beginningObj = _progressionManager.GetNextObjective();
                
                // Verify objective is not empty
                Assert.IsFalse(string.IsNullOrEmpty(beginningObj), "Beginning stage objective should not be empty");
                
                // Get objective for RadioRepair stage
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.RadioRepair);
                string radioRepairObj = _progressionManager.GetNextObjective();
                
                // Verify objective is not empty
                Assert.IsFalse(string.IsNullOrEmpty(radioRepairObj), "RadioRepair stage objective should not be empty");
                
                // Get objective for FirstSignal stage
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.FirstSignal);
                string firstSignalObj = _progressionManager.GetNextObjective();
                
                // Verify objective is not empty
                Assert.IsFalse(string.IsNullOrEmpty(firstSignalObj), "FirstSignal stage objective should not be empty");
                
                // Verify objectives are different
                Assert.AreNotEqual(beginningObj, radioRepairObj, "Stage objectives should be different");
                Assert.AreNotEqual(radioRepairObj, firstSignalObj, "Stage objectives should be different");
                Assert.AreNotEqual(beginningObj, firstSignalObj, "Stage objectives should be different");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestGetNextObjective: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests progression to endgame.
        /// </summary>
        [TestMethod]
        public void TestProgressionToEndgame()
        {
            try
            {
                // Set progression to FactoryAccess
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.FactoryAccess);
                
                // Verify stage was set
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FactoryAccess, _progressionManager.CurrentStage,
                    "Stage should be FactoryAccess");
                
                // Set up conditions for advancing to Endgame
                _questSystem.CompleteQuest(TestData.QuestIds.FinalTransmission);
                
                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();
                
                // Verify stage was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Endgame, _progressionManager.CurrentStage,
                    "Stage should be advanced to Endgame");
                
                // Try to advance beyond Endgame
                _progressionManager.AdvanceProgression();
                
                // Verify stage is still Endgame
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Endgame, _progressionManager.CurrentStage,
                    "Stage should still be Endgame (cannot advance further)");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestProgressionToEndgame: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests progression events.
        /// </summary>
        [TestMethod]
        public void TestProgressionEvents()
        {
            try
            {
                // Set progression to RadioRepair
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.RadioRepair);
                
                // Verify quest was discovered
                Assert.IsTrue(_questSystem.IsQuestDiscovered("quest_find_signal"),
                    "quest_find_signal should be discovered");
                
                // Verify message was added
                Assert.IsTrue(_gameState.Messages.ContainsKey("msg_radio_repaired"),
                    "msg_radio_repaired should be added to messages");
                
                // Set progression to FirstSignal
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.FirstSignal);
                
                // Verify location was discovered
                Assert.IsTrue(_mapSystem.IsLocationDiscovered(TestData.LocationIds.Forest),
                    "Forest location should be discovered");
                
                // Verify quest was discovered
                Assert.IsTrue(_questSystem.IsQuestDiscovered(TestData.QuestIds.ExploreForest),
                    "quest_explore_forest should be discovered");
                
                // Verify message was added
                Assert.IsTrue(_gameState.Messages.ContainsKey("msg_forest_signal"),
                    "msg_forest_signal should be added to messages");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestProgressionEvents: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
    }
}

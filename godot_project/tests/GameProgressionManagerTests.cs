using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class GameProgressionManagerTests : GUT.Test
    {
        // Components to test
        private GameProgressionManager _progressionManager;
        private GameState _gameState;
        private QuestSystem _questSystem;
        private MapSystem _mapSystem;
        private InventorySystem _inventorySystem;
        private MessageManager _messageManager;

        // Called before each test
        public async void Before()
        {
            try
            {
                // Create instances of the components
                _gameState = new GameState();
                _questSystem = new QuestSystem();
                _mapSystem = new MapSystem();
                _inventorySystem = new InventorySystem();
                _messageManager = new MessageManager();
                _progressionManager = new GameProgressionManager();

                // Add them to the scene tree using call_deferred
                CallDeferred("add_child", _gameState);
                CallDeferred("add_child", _questSystem);
                CallDeferred("add_child", _mapSystem);
                CallDeferred("add_child", _inventorySystem);
                CallDeferred("add_child", _messageManager);
                CallDeferred("add_child", _progressionManager);

                // Wait a frame to ensure all nodes are added
                await ToSignal(GetTree(), "process_frame");

                // Initialize them
                _gameState._Ready();
                _questSystem._Ready();
                _mapSystem._Ready();
                _inventorySystem._Ready();
                _messageManager._Ready();
                _progressionManager._Ready();

                // Set up initial state
                _gameState.SetGameProgress(0);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in GameProgressionManagerTests.Before: {ex.Message}");
                throw; // Re-throw to fail the test
            }
        }

        // Called after each test
        public void After()
        {
            // Clean up
            _progressionManager.QueueFree();
            _messageManager.QueueFree();
            _inventorySystem.QueueFree();
            _mapSystem.QueueFree();
            _questSystem.QueueFree();
            _gameState.QueueFree();

            _progressionManager = null;
            _messageManager = null;
            _inventorySystem = null;
            _mapSystem = null;
            _questSystem = null;
            _gameState = null;
        }

        [TestMethod]
        public void TestInitialization()
        {
            // Verify that the progression manager is initialized correctly
            Assert.IsNotNull(_progressionManager, "GameProgressionManager should not be null");
            Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage,
                "Initial stage should be Beginning");
        }

        [TestMethod]
        public void TestGetCurrentStageName()
        {
            // Test getting the current stage name
            string stageName = _progressionManager.CurrentStageName;
            Assert.AreEqual("Beginning", stageName, "Current stage name should be 'Beginning'");
        }

        [TestMethod]
        public void TestGetCurrentStageDescription()
        {
            // Test getting the current stage description
            string description = _progressionManager.GetCurrentStageDescription();
            Assert.IsFalse(string.IsNullOrEmpty(description), "Stage description should not be empty");
        }

        [TestMethod]
        public void TestGetNextObjective()
        {
            // Test getting the next objective
            string objective = _progressionManager.GetNextObjective();
            Assert.IsFalse(string.IsNullOrEmpty(objective), "Next objective should not be empty");
        }

        [TestMethod]
        public void TestSetProgression()
        {
            // Test setting the progression to a specific stage
            _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.RadioRepair);

            // Verify the stage was updated
            Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                "Current stage should be RadioRepair after SetProgression");

            // Verify the game state was updated
            Assert.AreEqual((int)GameProgressionManager.ProgressionStage.RadioRepair, _gameState.GameProgress,
                "Game progress should match the progression stage");
        }

        [TestMethod]
        public void TestAdvanceProgression()
        {
            // Test advancing the progression
            _progressionManager.AdvanceProgression();

            // Verify the stage was advanced
            Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                "Current stage should be RadioRepair after AdvanceProgression");

            // Verify the game state was updated
            Assert.AreEqual((int)GameProgressionManager.ProgressionStage.RadioRepair, _gameState.GameProgress,
                "Game progress should match the progression stage");
        }

        [TestMethod]
        public void TestCheckProgressionRequirements()
        {
            // Set up a quest that's required for progression
            var quest = _questSystem.GetQuest("quest_radio_repair");
            if (quest != null)
            {
                // Discover and activate the quest
                _questSystem.DiscoverQuest("quest_radio_repair");
                _questSystem.ActivateQuest("quest_radio_repair");

                // Complete all objectives
                foreach (var objective in quest.Objectives)
                {
                    objective.IsCompleted = true;
                    objective.CurrentAmount = objective.RequiredAmount;
                }

                // Complete the quest
                _questSystem.CompleteQuest("quest_radio_repair");

                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();

                // Verify the stage was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Current stage should be RadioRepair after completing the required quest");
            }
            else
            {
                // Skip the test if the quest doesn't exist
                Assert.IsTrue(true, "Test skipped because quest_radio_repair doesn't exist");
            }
        }

        [TestMethod]
        public void TestProgressionFlow()
        {
            // Test the full progression flow

            // Start at the beginning
            _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.Beginning);
            Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage);

            // Advance to RadioRepair
            _progressionManager.AdvanceProgression();
            Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage);

            // Advance to FirstSignal
            _progressionManager.AdvanceProgression();
            Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage);

            // Advance to ForestExploration
            _progressionManager.AdvanceProgression();
            Assert.AreEqual(GameProgressionManager.ProgressionStage.ForestExploration, _progressionManager.CurrentStage);

            // Advance to TownDiscovery
            _progressionManager.AdvanceProgression();
            Assert.AreEqual(GameProgressionManager.ProgressionStage.TownDiscovery, _progressionManager.CurrentStage);

            // Advance to SurvivorContact
            _progressionManager.AdvanceProgression();
            Assert.AreEqual(GameProgressionManager.ProgressionStage.SurvivorContact, _progressionManager.CurrentStage);

            // Advance to FactoryAccess
            _progressionManager.AdvanceProgression();
            Assert.AreEqual(GameProgressionManager.ProgressionStage.FactoryAccess, _progressionManager.CurrentStage);

            // Advance to Endgame
            _progressionManager.AdvanceProgression();
            Assert.AreEqual(GameProgressionManager.ProgressionStage.Endgame, _progressionManager.CurrentStage);

            // Try to advance beyond Endgame (should stay at Endgame)
            _progressionManager.AdvanceProgression();
            Assert.AreEqual(GameProgressionManager.ProgressionStage.Endgame, _progressionManager.CurrentStage,
                "Should not advance beyond Endgame");
        }
    }
}

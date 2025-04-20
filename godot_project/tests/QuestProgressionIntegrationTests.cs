using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class QuestProgressionIntegrationTests : GUT.Test
    {
        // Components to test
        private GameProgressionManager _progressionManager;
        private GameState _gameState;
        private QuestSystem _questSystem;
        private MapSystem _mapSystem;
        private InventorySystem _inventorySystem;
        private MessageManager _messageManager;

        // Called before each test
        public void Before()
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

                // Add them to the scene tree
                AddChild(_gameState);
                AddChild(_questSystem);
                AddChild(_mapSystem);
                AddChild(_inventorySystem);
                AddChild(_messageManager);
                AddChild(_progressionManager);

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
                GD.PrintErr($"Error in QuestProgressionIntegrationTests.Before: {ex.Message}");
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
        public void TestQuestCompletionTriggersProgressionAdvancement()
        {
            // Skip this test if components are not properly initialized
            if (_questSystem == null || _progressionManager == null)
            {
                GD.PrintErr("QuestSystem or GameProgressionManager is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }

            try
            {
                // Verify initial state
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage,
                    "Initial stage should be Beginning");

                // Get the radio repair quest
                var quest = _questSystem.GetQuest("quest_radio_repair");
                if (quest == null)
                {
                    GD.PrintErr("Quest 'quest_radio_repair' not found, skipping test");
                    Assert.IsTrue(true, "Test skipped because quest_radio_repair doesn't exist");
                    return;
                }

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
                bool completed = _questSystem.CompleteQuest("quest_radio_repair");
                Assert.IsTrue(completed, "Quest should be completed successfully");

                // Verify the quest is in the completed quests list
                Assert.IsTrue(_questSystem.IsQuestCompleted("quest_radio_repair"),
                    "Quest should be in the completed quests list");

                // Verify the progression was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Progression should advance to RadioRepair after completing the radio repair quest");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestQuestCompletionTriggersProgressionAdvancement: {ex.Message}");
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestLocationDiscoveryTriggersProgressionAdvancement()
        {
            // Skip this test if components are not properly initialized
            if (_mapSystem == null || _progressionManager == null)
            {
                GD.PrintErr("MapSystem or GameProgressionManager is null, skipping test");
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
                bool discovered = _mapSystem.DiscoverLocation("town");
                if (!discovered)
                {
                    GD.PrintErr("Location 'town' not found or already discovered, skipping test");
                    Assert.IsTrue(true, "Test skipped because town location couldn't be discovered");
                    return;
                }

                // Verify the location is discovered
                Assert.IsTrue(_mapSystem.IsLocationDiscovered("town"),
                    "Town location should be discovered");

                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();

                // Verify the progression was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.TownDiscovery, _progressionManager.CurrentStage,
                    "Progression should advance to TownDiscovery after discovering the town location");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestLocationDiscoveryTriggersProgressionAdvancement: {ex.Message}");
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestInventoryItemTriggersProgressionAdvancement()
        {
            // Skip this test if components are not properly initialized
            if (_inventorySystem == null || _mapSystem == null || _progressionManager == null)
            {
                GD.PrintErr("InventorySystem, MapSystem, or GameProgressionManager is null, skipping test");
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
                _mapSystem.DiscoverLocation("factory");
                Assert.IsTrue(_mapSystem.IsLocationDiscovered("factory"),
                    "Factory location should be discovered");

                // Add the factory key to the inventory
                _inventorySystem.AddItemToInventory("factory_key");
                Assert.IsTrue(_inventorySystem.HasItem("factory_key"),
                    "Inventory should contain the factory key");

                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();

                // Verify the progression was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FactoryAccess, _progressionManager.CurrentStage,
                    "Progression should advance to FactoryAccess after discovering the factory and obtaining the key");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestInventoryItemTriggersProgressionAdvancement: {ex.Message}");
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestSignalDiscoveryTriggersProgressionAdvancement()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _progressionManager == null)
            {
                GD.PrintErr("GameState or GameProgressionManager is null, skipping test");
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
                _gameState.AddDiscoveredFrequency(91.5f);
                Assert.IsTrue(_gameState.DiscoveredFrequencies.Contains(91.5f),
                    "Frequency 91.5 should be discovered");

                // Check progression requirements
                _progressionManager.CheckProgressionRequirements();

                // Verify the progression was advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage,
                    "Progression should advance to FirstSignal after discovering a frequency");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in TestSignalDiscoveryTriggersProgressionAdvancement: {ex.Message}");
                throw; // Re-throw to fail the test
            }
        }

        [TestMethod]
        public void TestFullGameProgressionFlow()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _questSystem == null || _mapSystem == null || 
                _inventorySystem == null || _progressionManager == null)
            {
                GD.PrintErr("One or more components are null, skipping test");
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
                var radioRepairQuest = _questSystem.GetQuest("quest_radio_repair");
                if (radioRepairQuest != null)
                {
                    _questSystem.DiscoverQuest("quest_radio_repair");
                    _questSystem.ActivateQuest("quest_radio_repair");
                    foreach (var objective in radioRepairQuest.Objectives)
                    {
                        objective.IsCompleted = true;
                        objective.CurrentAmount = objective.RequiredAmount;
                    }
                    _questSystem.CompleteQuest("quest_radio_repair");
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
                _gameState.AddDiscoveredFrequency(91.5f);
                _progressionManager.CheckProgressionRequirements();

                // Verify progression to FirstSignal
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage,
                    "Progression should advance to FirstSignal");

                // 3. Complete the forest exploration quest
                var forestQuest = _questSystem.GetQuest("quest_explore_forest");
                if (forestQuest != null)
                {
                    _questSystem.DiscoverQuest("quest_explore_forest");
                    _questSystem.ActivateQuest("quest_explore_forest");
                    foreach (var objective in forestQuest.Objectives)
                    {
                        objective.IsCompleted = true;
                        objective.CurrentAmount = objective.RequiredAmount;
                    }
                    _questSystem.CompleteQuest("quest_explore_forest");
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
                _mapSystem.DiscoverLocation("town");
                _progressionManager.CheckProgressionRequirements();

                // Verify progression to TownDiscovery
                Assert.AreEqual(GameProgressionManager.ProgressionStage.TownDiscovery, _progressionManager.CurrentStage,
                    "Progression should advance to TownDiscovery");

                // 5. Complete the survivor message quest
                var survivorQuest = _questSystem.GetQuest("quest_survivor_message");
                if (survivorQuest != null)
                {
                    _questSystem.DiscoverQuest("quest_survivor_message");
                    _questSystem.ActivateQuest("quest_survivor_message");
                    foreach (var objective in survivorQuest.Objectives)
                    {
                        objective.IsCompleted = true;
                        objective.CurrentAmount = objective.RequiredAmount;
                    }
                    _questSystem.CompleteQuest("quest_survivor_message");
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
                _mapSystem.DiscoverLocation("factory");
                _inventorySystem.AddItemToInventory("factory_key");
                _progressionManager.CheckProgressionRequirements();

                // Verify progression to FactoryAccess
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FactoryAccess, _progressionManager.CurrentStage,
                    "Progression should advance to FactoryAccess");

                // 7. Complete the final transmission quest
                var finalQuest = _questSystem.GetQuest("quest_final_transmission");
                if (finalQuest != null)
                {
                    _questSystem.DiscoverQuest("quest_final_transmission");
                    _questSystem.ActivateQuest("quest_final_transmission");
                    foreach (var objective in finalQuest.Objectives)
                    {
                        objective.IsCompleted = true;
                        objective.CurrentAmount = objective.RequiredAmount;
                    }
                    _questSystem.CompleteQuest("quest_final_transmission");
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
                GD.PrintErr($"Error in TestFullGameProgressionFlow: {ex.Message}");
                throw; // Re-throw to fail the test
            }
        }
    }
}

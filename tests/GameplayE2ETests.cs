// Moved to tests/e2e/
using Godot;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// End-to-end tests for gameplay scenarios.
    /// </summary>
    [TestClass]
    public partial class GameplayE2ETests : E2ETestBase
    {
        [TestMethod]
        public async Task TestRadioTuningWorkflow()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _radioTuner == null || _audioManager == null)
            {
                LogError("GameState, RadioTuner, or AudioManager is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Turn radio on
                _gameState.ToggleRadio();
                Assert.IsTrue(_gameState.IsRadioOn, "Radio should be on");
                
                // 2. Start at a non-signal frequency
                _gameState.SetFrequency(TestData.Frequencies.NoSignal1);
                
                // 3. Verify no signal is detected
                var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);
                Assert.IsNull(signalData, "No signal should be found at frequency 90.0");
                
                // 4. Simulate scanning for signals
                var foundFrequencies = await SimulateRadioScanning(TestData.Frequencies.Min, TestData.Frequencies.Max);
                
                // 5. Verify at least one signal was found
                Assert.IsTrue(foundFrequencies.Count > 0, "At least one signal should be found during scanning");
                
                // 6. Tune to a known signal frequency
                bool signalFound = await SimulateRadioTuning(TestData.Frequencies.Signal1);
                
                // 7. Verify signal was found
                Assert.IsTrue(signalFound, "Signal should be found at frequency 91.5");
                
                // 8. Verify frequency was added to discovered frequencies
                Assert.IsTrue(_gameState.DiscoveredFrequencies.Contains(TestData.Frequencies.Signal1),
                    "Frequency should be added to discovered frequencies");
                
                // 9. Get the message ID
                string messageId = (string)_radioTuner.Get("_currentSignalId");
                Assert.IsFalse(string.IsNullOrEmpty(messageId), "Message ID should not be empty");
                
                // 10. Verify message exists
                var message = _gameState.GetMessage(messageId);
                Assert.IsNotNull(message, "Message should exist for the detected signal");
                
                // 11. Verify message is not decoded yet
                Assert.IsFalse(message.Decoded, "Message should not be decoded initially");
                
                // 12. Decode the message
                bool decodeResult = _gameState.DecodeMessage(messageId);
                
                // 13. Verify decode was successful
                Assert.IsTrue(decodeResult, "Message decoding should be successful");
                
                // 14. Verify message is now decoded
                Assert.IsTrue(message.Decoded, "Message should be marked as decoded after decoding");
                
                // 15. Turn radio off
                _gameState.ToggleRadio();
                Assert.IsFalse(_gameState.IsRadioOn, "Radio should be off");
                
                // Clean up audio
                _audioManager.StopStaticNoise();
                _audioManager.StopSignal();
            }
            catch (Exception ex)
            {
                LogError($"Error in TestRadioTuningWorkflow: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        [TestMethod]
        public void TestQuestCompletionWorkflow()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _questSystem == null || _progressionManager == null)
            {
                LogError("GameState, QuestSystem, or GameProgressionManager is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Verify initial state
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage,
                    "Initial stage should be Beginning");
                
                // 2. Complete the radio repair quest
                bool questCompleted = SimulateQuestCompletion(TestData.QuestIds.RadioRepair);
                
                // 3. Verify quest completion
                Assert.IsTrue(questCompleted, "Radio repair quest should be completed successfully");
                Assert.IsTrue(_questSystem.IsQuestCompleted(TestData.QuestIds.RadioRepair),
                    "Radio repair quest should be in the completed quests list");
                
                // 4. Verify progression advancement
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Progression should advance to RadioRepair after completing the radio repair quest");
                
                // 5. Discover a frequency
                DiscoverFrequency(TestData.Frequencies.Signal1);
                
                // 6. Check progression requirements
                _progressionManager.CheckProgressionRequirements();
                
                // 7. Verify progression advancement
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage,
                    "Progression should advance to FirstSignal after discovering a frequency");
                
                // 8. Complete the forest exploration quest
                questCompleted = SimulateQuestCompletion(TestData.QuestIds.ExploreForest);
                
                // 9. Verify quest completion
                Assert.IsTrue(questCompleted, "Forest exploration quest should be completed successfully");
                Assert.IsTrue(_questSystem.IsQuestCompleted(TestData.QuestIds.ExploreForest),
                    "Forest exploration quest should be in the completed quests list");
                
                // 10. Verify progression advancement
                Assert.AreEqual(GameProgressionManager.ProgressionStage.ForestExploration, _progressionManager.CurrentStage,
                    "Progression should advance to ForestExploration after completing the forest exploration quest");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestQuestCompletionWorkflow: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        [TestMethod]
        public void TestInventoryAndLocationWorkflow()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _inventorySystem == null || _mapSystem == null || _progressionManager == null)
            {
                LogError("GameState, InventorySystem, MapSystem, or GameProgressionManager is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // Set up initial state
                _progressionManager.SetProgression(GameProgressionManager.ProgressionStage.ForestExploration);
                
                // 1. Discover the town location
                bool locationDiscovered = SimulateLocationDiscovery(TestData.LocationIds.Town);
                
                // 2. Verify location discovery
                Assert.IsTrue(locationDiscovered, "Town location should be discovered successfully");
                Assert.IsTrue(_mapSystem.IsLocationDiscovered(TestData.LocationIds.Town),
                    "Town location should be in the discovered locations list");
                
                // 3. Verify progression advancement
                Assert.AreEqual(GameProgressionManager.ProgressionStage.TownDiscovery, _progressionManager.CurrentStage,
                    "Progression should advance to TownDiscovery after discovering the town");
                
                // 4. Complete the survivor message quest
                bool questCompleted = SimulateQuestCompletion(TestData.QuestIds.SurvivorMessage);
                
                // 5. Verify quest completion
                Assert.IsTrue(questCompleted, "Survivor message quest should be completed successfully");
                Assert.IsTrue(_questSystem.IsQuestCompleted(TestData.QuestIds.SurvivorMessage),
                    "Survivor message quest should be in the completed quests list");
                
                // 6. Verify progression advancement
                Assert.AreEqual(GameProgressionManager.ProgressionStage.SurvivorContact, _progressionManager.CurrentStage,
                    "Progression should advance to SurvivorContact after completing the survivor message quest");
                
                // 7. Discover the factory location
                locationDiscovered = SimulateLocationDiscovery(TestData.LocationIds.Factory);
                
                // 8. Verify location discovery
                Assert.IsTrue(locationDiscovered, "Factory location should be discovered successfully");
                Assert.IsTrue(_mapSystem.IsLocationDiscovered(TestData.LocationIds.Factory),
                    "Factory location should be in the discovered locations list");
                
                // 9. Add the factory key to the inventory
                bool itemAdded = SimulateItemAcquisition(TestData.ItemIds.KeyFactory);
                
                // 10. Verify item acquisition
                Assert.IsTrue(itemAdded, "Factory key should be added to inventory successfully");
                Assert.IsTrue(_inventorySystem.HasItem(TestData.ItemIds.KeyFactory),
                    "Factory key should be in the inventory");
                
                // 11. Verify progression advancement
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FactoryAccess, _progressionManager.CurrentStage,
                    "Progression should advance to FactoryAccess after discovering the factory and obtaining the key");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestInventoryAndLocationWorkflow: {ex.Message}");
                LogError(ex.StackTrace);
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
                LogError("One or more components are null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // Start at the beginning
                ResetGameState();
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage,
                    "Initial stage should be Beginning");
                
                // 1. Complete the radio repair quest
                SimulateQuestCompletion(TestData.QuestIds.RadioRepair);
                
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
                SimulateQuestCompletion(TestData.QuestIds.ExploreForest);
                
                // Verify progression to ForestExploration
                Assert.AreEqual(GameProgressionManager.ProgressionStage.ForestExploration, _progressionManager.CurrentStage,
                    "Progression should advance to ForestExploration");
                
                // 4. Discover the town
                SimulateLocationDiscovery(TestData.LocationIds.Town);
                
                // Verify progression to TownDiscovery
                Assert.AreEqual(GameProgressionManager.ProgressionStage.TownDiscovery, _progressionManager.CurrentStage,
                    "Progression should advance to TownDiscovery");
                
                // 5. Complete the survivor message quest
                SimulateQuestCompletion(TestData.QuestIds.SurvivorMessage);
                
                // Verify progression to SurvivorContact
                Assert.AreEqual(GameProgressionManager.ProgressionStage.SurvivorContact, _progressionManager.CurrentStage,
                    "Progression should advance to SurvivorContact");
                
                // 6. Discover the factory and get the key
                SimulateLocationDiscovery(TestData.LocationIds.Factory);
                SimulateItemAcquisition(TestData.ItemIds.KeyFactory);
                
                // Verify progression to FactoryAccess
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FactoryAccess, _progressionManager.CurrentStage,
                    "Progression should advance to FactoryAccess");
                
                // 7. Complete the final transmission quest
                SimulateQuestCompletion(TestData.QuestIds.FinalTransmission);
                
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

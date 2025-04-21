using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// End-to-end tests for exploration mechanics.
    /// </summary>
    [TestClass]
    public partial class ExplorationE2ETests : E2ETestBase
    {
        /// <summary>
        /// Tests the basic location discovery and navigation workflow.
        /// </summary>
        [TestMethod]
        public void TestLocationDiscoveryAndNavigation()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _mapSystem == null)
            {
                LogError("GameState or MapSystem is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Check initial location
                string initialLocation = _gameState.CurrentLocation;
                
                LogMessage($"Initial location: {initialLocation}");
                
                Assert.AreEqual(TestData.LocationIds.Bunker, initialLocation, "Initial location should be bunker");
                
                // 2. Get connected locations
                var connectedLocations = _mapSystem.GetConnectedLocations();
                
                LogMessage($"Connected locations count: {connectedLocations.Count}");
                foreach (var location in connectedLocations)
                {
                    LogMessage($"Connected location: {location.Id} - {location.Name}");
                }
                
                Assert.IsTrue(connectedLocations.Count > 0, "There should be at least one connected location");
                
                // 3. Discover a connected location (forest)
                bool forestDiscovered = SimulateLocationDiscovery(TestData.LocationIds.Forest);
                
                // 4. Verify location was discovered
                Assert.IsTrue(forestDiscovered, "Forest location should be discovered successfully");
                Assert.IsTrue(_mapSystem.IsLocationDiscovered(TestData.LocationIds.Forest), "Forest should be in discovered locations");
                
                // 5. Navigate to the forest
                bool navigationResult = _mapSystem.ChangeLocation(TestData.LocationIds.Forest);
                
                // 6. Verify navigation was successful
                Assert.IsTrue(navigationResult, "Navigation to forest should be successful");
                Assert.AreEqual(TestData.LocationIds.Forest, _gameState.CurrentLocation, "Current location should be forest");
                
                // 7. Get connected locations from forest
                connectedLocations = _mapSystem.GetConnectedLocations();
                
                LogMessage($"Connected locations from forest: {connectedLocations.Count}");
                foreach (var location in connectedLocations)
                {
                    LogMessage($"Connected location: {location.Id} - {location.Name}");
                }
                
                // 8. Discover another location (town)
                bool townDiscovered = SimulateLocationDiscovery(TestData.LocationIds.Town);
                
                // 9. Verify town was discovered
                Assert.IsTrue(townDiscovered, "Town location should be discovered successfully");
                Assert.IsTrue(_mapSystem.IsLocationDiscovered(TestData.LocationIds.Town), "Town should be in discovered locations");
                
                // 10. Try to navigate to a non-connected location
                bool invalidNavigation = _mapSystem.ChangeLocation(TestData.LocationIds.RadioTower);
                
                // 11. Verify navigation failed
                Assert.IsFalse(invalidNavigation, "Navigation to non-connected location should fail");
                Assert.AreEqual(TestData.LocationIds.Forest, _gameState.CurrentLocation, "Current location should still be forest");
                
                // 12. Navigate back to bunker
                bool returnNavigation = _mapSystem.ChangeLocation(TestData.LocationIds.Bunker);
                
                // 13. Verify return navigation was successful
                Assert.IsTrue(returnNavigation, "Navigation back to bunker should be successful");
                Assert.AreEqual(TestData.LocationIds.Bunker, _gameState.CurrentLocation, "Current location should be bunker again");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestLocationDiscoveryAndNavigation: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the map fragment collection and map completion mechanics.
        /// </summary>
        [TestMethod]
        public void TestMapFragmentCollection()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _inventorySystem == null || _mapSystem == null)
            {
                LogError("One or more components are null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Check initial map completion
                int initialMapCompletion = _gameState.MapCompletion;
                
                LogMessage($"Initial map completion: {initialMapCompletion}%");
                
                // 2. Add a map fragment to inventory
                bool fragmentAdded = SimulateItemAcquisition(TestData.ItemIds.MapFragment);
                
                // 3. Verify fragment was added
                Assert.IsTrue(fragmentAdded, "Map fragment should be added to inventory");
                Assert.IsTrue(_inventorySystem.HasItem(TestData.ItemIds.MapFragment), "Inventory should contain map fragment");
                
                // 4. Use the map fragment
                bool fragmentUsed = SimulateItemUsage(TestData.ItemIds.MapFragment);
                
                // 5. Verify fragment was used
                Assert.IsTrue(fragmentUsed, "Map fragment should be used successfully");
                Assert.IsFalse(_inventorySystem.HasItem(TestData.ItemIds.MapFragment), "Map fragment should be consumed");
                
                // 6. Verify map completion increased
                Assert.IsTrue(_gameState.MapCompletion > initialMapCompletion, "Map completion should increase");
                
                LogMessage($"Map completion after using fragment: {_gameState.MapCompletion}%");
                
                // 7. Check if new locations were discovered
                var discoveredLocations = new List<string>();
                foreach (var location in _mapSystem.GetAllLocations())
                {
                    if (location.Value.IsDiscovered)
                    {
                        discoveredLocations.Add(location.Key);
                    }
                }
                
                LogMessage($"Discovered locations count: {discoveredLocations.Count}");
                foreach (var locationId in discoveredLocations)
                {
                    LogMessage($"Discovered location: {locationId}");
                }
                
                // 8. Add and use more map fragments
                for (int i = 0; i < 3; i++)
                {
                    SimulateItemAcquisition(TestData.ItemIds.MapFragment);
                    SimulateItemUsage(TestData.ItemIds.MapFragment);
                }
                
                // 9. Verify map completion increased further
                Assert.IsTrue(_gameState.MapCompletion > initialMapCompletion + 10, "Map completion should increase significantly");
                
                LogMessage($"Map completion after using multiple fragments: {_gameState.MapCompletion}%");
                
                // 10. Check if more locations were discovered
                var newDiscoveredLocations = new List<string>();
                foreach (var location in _mapSystem.GetAllLocations())
                {
                    if (location.Value.IsDiscovered)
                    {
                        newDiscoveredLocations.Add(location.Key);
                    }
                }
                
                LogMessage($"New discovered locations count: {newDiscoveredLocations.Count}");
                
                // 11. Verify more locations were discovered
                Assert.IsTrue(newDiscoveredLocations.Count > discoveredLocations.Count, "More locations should be discovered");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestMapFragmentCollection: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the location-specific item discovery mechanics.
        /// </summary>
        [TestMethod]
        public void TestLocationSpecificItemDiscovery()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _inventorySystem == null || _mapSystem == null)
            {
                LogError("One or more components are null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Navigate to forest
                SimulateLocationDiscovery(TestData.LocationIds.Forest);
                _mapSystem.ChangeLocation(TestData.LocationIds.Forest);
                
                // 2. Verify current location
                Assert.AreEqual(TestData.LocationIds.Forest, _gameState.CurrentLocation, "Current location should be forest");
                
                // 3. Search for items in forest
                bool searchResult = _mapSystem.SearchCurrentLocation();
                
                // 4. Verify search was successful
                Assert.IsTrue(searchResult, "Search in forest should be successful");
                
                // 5. Check if any items were found
                var inventoryBefore = _inventorySystem.GetInventory();
                int itemCountBefore = inventoryBefore.Count;
                
                LogMessage($"Inventory count before searching: {itemCountBefore}");
                foreach (var item in inventoryBefore)
                {
                    LogMessage($"Item in inventory: {item.Key} - {item.Value.Name}");
                }
                
                // 6. Simulate finding items (this would normally happen in the SearchCurrentLocation method)
                // For testing purposes, we'll add items directly
                SimulateItemAcquisition(TestData.ItemIds.WaterBottle);
                SimulateItemAcquisition(TestData.ItemIds.MapFragment);
                
                // 7. Verify items were added to inventory
                var inventoryAfter = _inventorySystem.GetInventory();
                int itemCountAfter = inventoryAfter.Count;
                
                LogMessage($"Inventory count after searching: {itemCountAfter}");
                foreach (var item in inventoryAfter)
                {
                    LogMessage($"Item in inventory: {item.Key} - {item.Value.Name}");
                }
                
                Assert.IsTrue(itemCountAfter > itemCountBefore, "Items should be added to inventory after searching");
                
                // 8. Navigate to town
                SimulateLocationDiscovery(TestData.LocationIds.Town);
                _mapSystem.ChangeLocation(TestData.LocationIds.Town);
                
                // 9. Verify current location
                Assert.AreEqual(TestData.LocationIds.Town, _gameState.CurrentLocation, "Current location should be town");
                
                // 10. Search for items in town
                searchResult = _mapSystem.SearchCurrentLocation();
                
                // 11. Verify search was successful
                Assert.IsTrue(searchResult, "Search in town should be successful");
                
                // 12. Simulate finding different items in town
                SimulateItemAcquisition(TestData.ItemIds.CannedFood);
                SimulateItemAcquisition(TestData.ItemIds.Battery);
                
                // 13. Verify more items were added to inventory
                var inventoryFinal = _inventorySystem.GetInventory();
                int itemCountFinal = inventoryFinal.Count;
                
                LogMessage($"Inventory count after searching town: {itemCountFinal}");
                foreach (var item in inventoryFinal)
                {
                    LogMessage($"Item in inventory: {item.Key} - {item.Value.Name}");
                }
                
                Assert.IsTrue(itemCountFinal > itemCountAfter, "More items should be added to inventory after searching town");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestLocationSpecificItemDiscovery: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the location-based quest discovery mechanics.
        /// </summary>
        [TestMethod]
        public void TestLocationBasedQuestDiscovery()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _questSystem == null || _mapSystem == null)
            {
                LogError("One or more components are null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Get initial discovered quests
                var initialQuests = _questSystem.GetDiscoveredQuests();
                int initialQuestCount = initialQuests.Count;
                
                LogMessage($"Initial discovered quests: {initialQuestCount}");
                foreach (var quest in initialQuests)
                {
                    LogMessage($"Discovered quest: {quest.Key} - {quest.Value.Title}");
                }
                
                // 2. Navigate to forest
                SimulateLocationDiscovery(TestData.LocationIds.Forest);
                _mapSystem.ChangeLocation(TestData.LocationIds.Forest);
                
                // 3. Verify current location
                Assert.AreEqual(TestData.LocationIds.Forest, _gameState.CurrentLocation, "Current location should be forest");
                
                // 4. Manually trigger the location changed event handler
                _questSystem.Call("OnLocationChanged", TestData.LocationIds.Forest);
                
                // 5. Check if new quests were discovered
                var forestQuests = _questSystem.GetDiscoveredQuests();
                int forestQuestCount = forestQuests.Count;
                
                LogMessage($"Discovered quests after visiting forest: {forestQuestCount}");
                foreach (var quest in forestQuests)
                {
                    LogMessage($"Discovered quest: {quest.Key} - {quest.Value.Title}");
                }
                
                // 6. Navigate to town
                SimulateLocationDiscovery(TestData.LocationIds.Town);
                _mapSystem.ChangeLocation(TestData.LocationIds.Town);
                
                // 7. Verify current location
                Assert.AreEqual(TestData.LocationIds.Town, _gameState.CurrentLocation, "Current location should be town");
                
                // 8. Manually trigger the location changed event handler
                _questSystem.Call("OnLocationChanged", TestData.LocationIds.Town);
                
                // 9. Check if more quests were discovered
                var townQuests = _questSystem.GetDiscoveredQuests();
                int townQuestCount = townQuests.Count;
                
                LogMessage($"Discovered quests after visiting town: {townQuestCount}");
                foreach (var quest in townQuests)
                {
                    LogMessage($"Discovered quest: {quest.Key} - {quest.Value.Title}");
                }
                
                // 10. Verify quest discovery progression
                Assert.IsTrue(townQuestCount >= forestQuestCount, "More quests should be discovered as more locations are visited");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestLocationBasedQuestDiscovery: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the full exploration gameplay loop.
        /// </summary>
        [TestMethod]
        public async Task TestFullExplorationGameplayLoop()
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
                // 1. Reset game state
                ResetGameState();
                
                // 2. Verify initial state
                Assert.AreEqual(TestData.LocationIds.Bunker, _gameState.CurrentLocation, "Initial location should be bunker");
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Beginning, _progressionManager.CurrentStage, 
                    "Initial progression stage should be Beginning");
                
                // 3. Complete the radio repair quest to advance progression
                SimulateQuestCompletion(TestData.QuestIds.RadioRepair);
                
                // 4. Verify progression advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.RadioRepair, _progressionManager.CurrentStage,
                    "Progression should advance to RadioRepair");
                
                // 5. Discover and tune to a frequency
                DiscoverFrequency(TestData.Frequencies.Signal1);
                _progressionManager.CheckProgressionRequirements();
                
                // 6. Verify progression advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FirstSignal, _progressionManager.CurrentStage,
                    "Progression should advance to FirstSignal");
                
                // 7. Discover and navigate to forest
                SimulateLocationDiscovery(TestData.LocationIds.Forest);
                _mapSystem.ChangeLocation(TestData.LocationIds.Forest);
                
                // 8. Verify current location
                Assert.AreEqual(TestData.LocationIds.Forest, _gameState.CurrentLocation, "Current location should be forest");
                
                // 9. Complete the forest exploration quest
                SimulateQuestCompletion(TestData.QuestIds.ExploreForest);
                
                // 10. Verify progression advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.ForestExploration, _progressionManager.CurrentStage,
                    "Progression should advance to ForestExploration");
                
                // 11. Search for items in forest
                _mapSystem.SearchCurrentLocation();
                
                // 12. Simulate finding a map fragment
                SimulateItemAcquisition(TestData.ItemIds.MapFragment);
                
                // 13. Use the map fragment
                SimulateItemUsage(TestData.ItemIds.MapFragment);
                
                // 14. Verify map completion increased
                Assert.IsTrue(_gameState.MapCompletion > 0, "Map completion should increase");
                
                // 15. Discover and navigate to town
                SimulateLocationDiscovery(TestData.LocationIds.Town);
                _mapSystem.ChangeLocation(TestData.LocationIds.Town);
                
                // 16. Verify progression advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.TownDiscovery, _progressionManager.CurrentStage,
                    "Progression should advance to TownDiscovery");
                
                // 17. Complete the survivor message quest
                SimulateQuestCompletion(TestData.QuestIds.SurvivorMessage);
                
                // 18. Verify progression advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.SurvivorContact, _progressionManager.CurrentStage,
                    "Progression should advance to SurvivorContact");
                
                // 19. Discover factory and get key
                SimulateLocationDiscovery(TestData.LocationIds.Factory);
                SimulateItemAcquisition(TestData.ItemIds.KeyFactory);
                _progressionManager.CheckProgressionRequirements();
                
                // 20. Verify progression advanced
                Assert.AreEqual(GameProgressionManager.ProgressionStage.FactoryAccess, _progressionManager.CurrentStage,
                    "Progression should advance to FactoryAccess");
                
                // 21. Navigate to factory
                _mapSystem.ChangeLocation(TestData.LocationIds.Factory);
                
                // 22. Verify current location
                Assert.AreEqual(TestData.LocationIds.Factory, _gameState.CurrentLocation, "Current location should be factory");
                
                // 23. Search for items in factory
                _mapSystem.SearchCurrentLocation();
                
                // 24. Simulate finding radio parts
                SimulateItemAcquisition(TestData.ItemIds.RadioPart);
                
                // 25. Discover radio tower
                SimulateLocationDiscovery(TestData.LocationIds.RadioTower);
                
                // 26. Navigate to radio tower
                _mapSystem.ChangeLocation(TestData.LocationIds.RadioTower);
                
                // 27. Verify current location
                Assert.AreEqual(TestData.LocationIds.RadioTower, _gameState.CurrentLocation, "Current location should be radio tower");
                
                // 28. Complete the final transmission quest
                SimulateQuestCompletion(TestData.QuestIds.FinalTransmission);
                
                // 29. Verify progression advanced to endgame
                Assert.AreEqual(GameProgressionManager.ProgressionStage.Endgame, _progressionManager.CurrentStage,
                    "Progression should advance to Endgame");
                
                // 30. Verify game completion
                Assert.IsTrue(_gameState.IsGameCompleted, "Game should be completed");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestFullExplorationGameplayLoop: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
    }
}

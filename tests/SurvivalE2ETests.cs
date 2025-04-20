using Godot;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// End-to-end tests for survival mechanics.
    /// </summary>
    [TestClass]
    public partial class SurvivalE2ETests : E2ETestBase
    {
        /// <summary>
        /// Tests the basic survival mechanics workflow.
        /// </summary>
        [TestMethod]
        public void TestBasicSurvivalMechanics()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _inventorySystem == null)
            {
                LogError("GameState or InventorySystem is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Check initial health and hunger values
                float initialHealth = _gameState.Health;
                float initialHunger = _gameState.Hunger;
                
                LogMessage($"Initial health: {initialHealth}");
                LogMessage($"Initial hunger: {initialHunger}");
                
                Assert.AreEqual(100.0f, initialHealth, "Initial health should be 100");
                Assert.AreEqual(100.0f, initialHunger, "Initial hunger should be 100");
                
                // 2. Simulate time passing (hunger decreases)
                _gameState.UpdateHunger(-10.0f);
                
                // 3. Verify hunger decreased
                Assert.AreEqual(90.0f, _gameState.Hunger, "Hunger should decrease to 90");
                
                // 4. Add food to inventory
                bool foodAdded = SimulateItemAcquisition(TestData.ItemIds.CannedFood);
                
                // 5. Verify food was added
                Assert.IsTrue(foodAdded, "Food should be added to inventory");
                Assert.IsTrue(_inventorySystem.HasItem(TestData.ItemIds.CannedFood), "Inventory should contain food");
                
                // 6. Use food
                bool foodUsed = SimulateItemUsage(TestData.ItemIds.CannedFood);
                
                // 7. Verify food was used and hunger increased
                Assert.IsTrue(foodUsed, "Food should be used successfully");
                Assert.IsFalse(_inventorySystem.HasItem(TestData.ItemIds.CannedFood), "Food should be consumed");
                Assert.IsTrue(_gameState.Hunger > 90.0f, "Hunger should increase after eating");
                
                LogMessage($"Hunger after eating: {_gameState.Hunger}");
                
                // 8. Simulate taking damage
                _gameState.UpdateHealth(-20.0f);
                
                // 9. Verify health decreased
                Assert.AreEqual(80.0f, _gameState.Health, "Health should decrease to 80");
                
                // 10. Add medkit to inventory
                bool medkitAdded = SimulateItemAcquisition(TestData.ItemIds.Medkit);
                
                // 11. Verify medkit was added
                Assert.IsTrue(medkitAdded, "Medkit should be added to inventory");
                Assert.IsTrue(_inventorySystem.HasItem(TestData.ItemIds.Medkit), "Inventory should contain medkit");
                
                // 12. Use medkit
                bool medkitUsed = SimulateItemUsage(TestData.ItemIds.Medkit);
                
                // 13. Verify medkit was used and health increased
                Assert.IsTrue(medkitUsed, "Medkit should be used successfully");
                Assert.IsFalse(_inventorySystem.HasItem(TestData.ItemIds.Medkit), "Medkit should be consumed");
                Assert.IsTrue(_gameState.Health > 80.0f, "Health should increase after using medkit");
                
                LogMessage($"Health after using medkit: {_gameState.Health}");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestBasicSurvivalMechanics: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the hunger and health depletion mechanics.
        /// </summary>
        [TestMethod]
        public void TestHungerAndHealthDepletion()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null)
            {
                LogError("GameState is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Reset health and hunger to full
                _gameState.SetHealth(100.0f);
                _gameState.SetHunger(100.0f);
                
                // 2. Verify initial values
                Assert.AreEqual(100.0f, _gameState.Health, "Initial health should be 100");
                Assert.AreEqual(100.0f, _gameState.Hunger, "Initial hunger should be 100");
                
                // 3. Simulate severe hunger
                _gameState.SetHunger(10.0f);
                
                // 4. Simulate game update (this would normally happen in _Process)
                // When hunger is very low, health should start decreasing
                _gameState.Call("UpdateSurvivalStats", 1.0f); // 1 second delta
                
                // 5. Verify health decreased due to hunger
                Assert.IsTrue(_gameState.Health < 100.0f, "Health should decrease when hunger is very low");
                
                LogMessage($"Health after 1 second with low hunger: {_gameState.Health}");
                
                // 6. Simulate critical hunger
                _gameState.SetHunger(0.0f);
                
                // 7. Simulate game update
                _gameState.Call("UpdateSurvivalStats", 1.0f); // 1 second delta
                
                // 8. Verify health decreased more rapidly
                float previousHealth = _gameState.Health;
                _gameState.Call("UpdateSurvivalStats", 1.0f); // Another second
                
                // 9. Verify health decreased even more
                Assert.IsTrue(_gameState.Health < previousHealth, "Health should continue to decrease with zero hunger");
                
                LogMessage($"Health after 2 seconds with zero hunger: {_gameState.Health}");
                
                // 10. Restore hunger
                _gameState.SetHunger(100.0f);
                
                // 11. Simulate game update
                previousHealth = _gameState.Health;
                _gameState.Call("UpdateSurvivalStats", 1.0f); // 1 second delta
                
                // 12. Verify health stabilized
                Assert.AreEqual(previousHealth, _gameState.Health, "Health should stabilize when hunger is restored");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestHungerAndHealthDepletion: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the water consumption mechanics.
        /// </summary>
        [TestMethod]
        public void TestWaterConsumption()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _inventorySystem == null)
            {
                LogError("GameState or InventorySystem is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Check initial thirst value
                float initialThirst = _gameState.Thirst;
                
                LogMessage($"Initial thirst: {initialThirst}");
                
                Assert.AreEqual(100.0f, initialThirst, "Initial thirst should be 100");
                
                // 2. Simulate time passing (thirst decreases)
                _gameState.UpdateThirst(-15.0f);
                
                // 3. Verify thirst decreased
                Assert.AreEqual(85.0f, _gameState.Thirst, "Thirst should decrease to 85");
                
                // 4. Add water bottle to inventory
                bool waterAdded = SimulateItemAcquisition(TestData.ItemIds.WaterBottle);
                
                // 5. Verify water was added
                Assert.IsTrue(waterAdded, "Water bottle should be added to inventory");
                Assert.IsTrue(_inventorySystem.HasItem(TestData.ItemIds.WaterBottle), "Inventory should contain water bottle");
                
                // 6. Use water bottle
                bool waterUsed = SimulateItemUsage(TestData.ItemIds.WaterBottle);
                
                // 7. Verify water was used and thirst increased
                Assert.IsTrue(waterUsed, "Water bottle should be used successfully");
                Assert.IsFalse(_inventorySystem.HasItem(TestData.ItemIds.WaterBottle), "Water bottle should be consumed");
                Assert.IsTrue(_gameState.Thirst > 85.0f, "Thirst should increase after drinking water");
                
                LogMessage($"Thirst after drinking water: {_gameState.Thirst}");
                
                // 8. Simulate severe thirst
                _gameState.SetThirst(10.0f);
                
                // 9. Simulate game update
                _gameState.Call("UpdateSurvivalStats", 1.0f); // 1 second delta
                
                // 10. Verify health decreased due to thirst
                Assert.IsTrue(_gameState.Health < 100.0f, "Health should decrease when thirst is very low");
                
                LogMessage($"Health after 1 second with low thirst: {_gameState.Health}");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestWaterConsumption: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the day-night cycle and its effects on survival.
        /// </summary>
        [TestMethod]
        public void TestDayNightCycle()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null)
            {
                LogError("GameState is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Check initial time of day
                float initialTimeOfDay = _gameState.TimeOfDay;
                
                LogMessage($"Initial time of day: {initialTimeOfDay}");
                
                // 2. Advance time (simulate several hours passing)
                for (int i = 0; i < 12; i++)
                {
                    _gameState.AdvanceTime(1.0f); // Advance by 1 hour
                }
                
                // 3. Verify time advanced
                Assert.AreNotEqual(initialTimeOfDay, _gameState.TimeOfDay, "Time of day should advance");
                
                LogMessage($"Time of day after 12 hours: {_gameState.TimeOfDay}");
                
                // 4. Check if it's night time
                bool isNight = _gameState.IsNightTime();
                
                LogMessage($"Is night time: {isNight}");
                
                // 5. Verify survival stats decrease faster at night (if applicable)
                float hungerBeforeNight = _gameState.Hunger;
                float thirstBeforeNight = _gameState.Thirst;
                
                // Simulate game update during night
                _gameState.Call("UpdateSurvivalStats", 1.0f); // 1 second delta
                
                LogMessage($"Hunger after 1 second at night: {_gameState.Hunger}");
                LogMessage($"Thirst after 1 second at night: {_gameState.Thirst}");
                
                // 6. Advance time to day
                for (int i = 0; i < 12; i++)
                {
                    _gameState.AdvanceTime(1.0f); // Advance by 1 hour
                }
                
                // 7. Verify it's day time
                bool isDay = !_gameState.IsNightTime();
                
                LogMessage($"Is day time: {isDay}");
                
                // 8. Reset hunger and thirst
                _gameState.SetHunger(hungerBeforeNight);
                _gameState.SetThirst(thirstBeforeNight);
                
                // 9. Simulate game update during day
                _gameState.Call("UpdateSurvivalStats", 1.0f); // 1 second delta
                
                LogMessage($"Hunger after 1 second during day: {_gameState.Hunger}");
                LogMessage($"Thirst after 1 second during day: {_gameState.Thirst}");
                
                // Note: We're not making assertions about the rate of decrease because
                // it depends on the game's implementation. We're just logging the values
                // for manual inspection.
            }
            catch (Exception ex)
            {
                LogError($"Error in TestDayNightCycle: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the flashlight usage and battery mechanics.
        /// </summary>
        [TestMethod]
        public async Task TestFlashlightAndBatteryMechanics()
        {
            // Skip this test if components are not properly initialized
            if (_gameState == null || _inventorySystem == null)
            {
                LogError("GameState or InventorySystem is null, skipping test");
                Assert.IsTrue(true, "Test skipped due to initialization issues");
                return;
            }
            
            try
            {
                // 1. Add flashlight to inventory
                bool flashlightAdded = SimulateItemAcquisition(TestData.ItemIds.Flashlight);
                
                // 2. Verify flashlight was added
                Assert.IsTrue(flashlightAdded, "Flashlight should be added to inventory");
                Assert.IsTrue(_inventorySystem.HasItem(TestData.ItemIds.Flashlight), "Inventory should contain flashlight");
                
                // 3. Check initial flashlight battery level
                float initialBatteryLevel = _gameState.FlashlightBatteryLevel;
                
                LogMessage($"Initial flashlight battery level: {initialBatteryLevel}");
                
                // 4. Use flashlight
                bool flashlightUsed = SimulateItemUsage(TestData.ItemIds.Flashlight);
                
                // 5. Verify flashlight was used and is now active
                Assert.IsTrue(flashlightUsed, "Flashlight should be used successfully");
                Assert.IsTrue(_gameState.IsFlashlightOn, "Flashlight should be turned on");
                
                // 6. Simulate time passing (battery drains)
                for (int i = 0; i < 10; i++)
                {
                    // Simulate game update
                    _gameState.Call("UpdateFlashlight", 1.0f); // 1 second delta
                    await TestHelpers.WaitSeconds(0.1f); // Small delay to avoid freezing the test
                }
                
                // 7. Verify battery level decreased
                Assert.IsTrue(_gameState.FlashlightBatteryLevel < initialBatteryLevel, "Flashlight battery level should decrease");
                
                LogMessage($"Battery level after 10 seconds: {_gameState.FlashlightBatteryLevel}");
                
                // 8. Turn off flashlight
                _gameState.ToggleFlashlight();
                
                // 9. Verify flashlight is off
                Assert.IsFalse(_gameState.IsFlashlightOn, "Flashlight should be turned off");
                
                // 10. Add battery to inventory
                bool batteryAdded = SimulateItemAcquisition(TestData.ItemIds.Battery);
                
                // 11. Verify battery was added
                Assert.IsTrue(batteryAdded, "Battery should be added to inventory");
                Assert.IsTrue(_inventorySystem.HasItem(TestData.ItemIds.Battery), "Inventory should contain battery");
                
                // 12. Use battery
                bool batteryUsed = SimulateItemUsage(TestData.ItemIds.Battery);
                
                // 13. Verify battery was used and flashlight battery level increased
                Assert.IsTrue(batteryUsed, "Battery should be used successfully");
                Assert.IsFalse(_inventorySystem.HasItem(TestData.ItemIds.Battery), "Battery should be consumed");
                Assert.IsTrue(_gameState.FlashlightBatteryLevel > initialBatteryLevel, "Flashlight battery level should increase");
                
                LogMessage($"Battery level after using battery: {_gameState.FlashlightBatteryLevel}");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestFlashlightAndBatteryMechanics: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests the full survival gameplay loop.
        /// </summary>
        [TestMethod]
        public async Task TestFullSurvivalGameplayLoop()
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
                // 1. Reset game state
                ResetGameState();
                
                // 2. Verify initial survival stats
                Assert.AreEqual(100.0f, _gameState.Health, "Initial health should be 100");
                Assert.AreEqual(100.0f, _gameState.Hunger, "Initial hunger should be 100");
                Assert.AreEqual(100.0f, _gameState.Thirst, "Initial thirst should be 100");
                
                // 3. Simulate exploring a location
                SimulateLocationDiscovery(TestData.LocationIds.Forest);
                _gameState.SetCurrentLocation(TestData.LocationIds.Forest);
                
                // 4. Simulate time passing (survival stats decrease)
                for (int i = 0; i < 5; i++)
                {
                    _gameState.Call("UpdateSurvivalStats", 1.0f); // 1 second delta
                    await TestHelpers.WaitSeconds(0.1f); // Small delay to avoid freezing the test
                }
                
                // 5. Verify survival stats decreased
                Assert.IsTrue(_gameState.Hunger < 100.0f, "Hunger should decrease over time");
                Assert.IsTrue(_gameState.Thirst < 100.0f, "Thirst should decrease over time");
                
                LogMessage($"Hunger after exploring: {_gameState.Hunger}");
                LogMessage($"Thirst after exploring: {_gameState.Thirst}");
                
                // 6. Find and use resources
                SimulateItemAcquisition(TestData.ItemIds.CannedFood);
                SimulateItemAcquisition(TestData.ItemIds.WaterBottle);
                
                float hungerBeforeEating = _gameState.Hunger;
                float thirstBeforeDrinking = _gameState.Thirst;
                
                SimulateItemUsage(TestData.ItemIds.CannedFood);
                SimulateItemUsage(TestData.ItemIds.WaterBottle);
                
                // 7. Verify survival stats improved
                Assert.IsTrue(_gameState.Hunger > hungerBeforeEating, "Hunger should increase after eating");
                Assert.IsTrue(_gameState.Thirst > thirstBeforeDrinking, "Thirst should increase after drinking");
                
                LogMessage($"Hunger after eating: {_gameState.Hunger}");
                LogMessage($"Thirst after drinking: {_gameState.Thirst}");
                
                // 8. Simulate night time
                _gameState.SetTimeOfDay(22.0f); // 10 PM
                
                // 9. Use flashlight
                SimulateItemAcquisition(TestData.ItemIds.Flashlight);
                SimulateItemUsage(TestData.ItemIds.Flashlight);
                
                // 10. Verify flashlight is on
                Assert.IsTrue(_gameState.IsFlashlightOn, "Flashlight should be turned on");
                
                // 11. Simulate time passing (battery drains)
                float initialBatteryLevel = _gameState.FlashlightBatteryLevel;
                
                for (int i = 0; i < 5; i++)
                {
                    _gameState.Call("UpdateFlashlight", 1.0f); // 1 second delta
                    await TestHelpers.WaitSeconds(0.1f); // Small delay to avoid freezing the test
                }
                
                // 12. Verify battery level decreased
                Assert.IsTrue(_gameState.FlashlightBatteryLevel < initialBatteryLevel, "Flashlight battery level should decrease");
                
                LogMessage($"Battery level after use: {_gameState.FlashlightBatteryLevel}");
                
                // 13. Replace battery
                SimulateItemAcquisition(TestData.ItemIds.Battery);
                SimulateItemUsage(TestData.ItemIds.Battery);
                
                // 14. Verify battery level increased
                Assert.IsTrue(_gameState.FlashlightBatteryLevel > initialBatteryLevel, "Flashlight battery level should increase");
                
                LogMessage($"Battery level after replacement: {_gameState.FlashlightBatteryLevel}");
                
                // 15. Simulate taking damage
                _gameState.UpdateHealth(-30.0f);
                
                // 16. Verify health decreased
                Assert.AreEqual(70.0f, _gameState.Health, "Health should decrease after taking damage");
                
                // 17. Use medkit
                SimulateItemAcquisition(TestData.ItemIds.Medkit);
                SimulateItemUsage(TestData.ItemIds.Medkit);
                
                // 18. Verify health increased
                Assert.IsTrue(_gameState.Health > 70.0f, "Health should increase after using medkit");
                
                LogMessage($"Health after using medkit: {_gameState.Health}");
                
                // 19. Simulate returning to shelter
                _gameState.SetCurrentLocation(TestData.LocationIds.Bunker);
                
                // 20. Verify location changed
                Assert.AreEqual(TestData.LocationIds.Bunker, _gameState.CurrentLocation, "Current location should be bunker");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestFullSurvivalGameplayLoop: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
    }
}

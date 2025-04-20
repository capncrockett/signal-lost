using Godot;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// Tests for the MapSystem class.
    /// </summary>
    [TestClass]
    public partial class MapSystemTests : UnitTestBase
    {
        private TestMapSystem _mapSystem;
        
        /// <summary>
        /// Called before each test.
        /// </summary>
        public override void Before()
        {
            base.Before();
            
            try
            {
                // Create a new TestMapSystem
                _mapSystem = new TestMapSystem();
                SafeAddChild(_mapSystem);
                
                // Initialize the map system
                _mapSystem._Ready();
            }
            catch (Exception ex)
            {
                LogError($"Error in MapSystemTests.Before: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests getting a location by ID.
        /// </summary>
        [TestMethod]
        public void TestGetLocation()
        {
            try
            {
                // Act
                var location = TestMapSystem.GetLocation(TestData.LocationIds.Bunker);
                
                // Assert
                Assert.IsNotNull(location, "Location should not be null");
                Assert.AreEqual(TestData.LocationIds.Bunker, location.Id, "Location ID should match");
                Assert.AreEqual("Emergency Bunker", location.Name, "Location name should match");
                Assert.IsTrue(location.IsDiscovered, "Starting location should be discovered");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestGetLocation: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests discovering a location.
        /// </summary>
        [TestMethod]
        public void TestDiscoverLocation()
        {
            try
            {
                // Act
                bool result = TestMapSystem.DiscoverLocation(TestData.LocationIds.Forest);
                var location = TestMapSystem.GetLocation(TestData.LocationIds.Forest);
                
                // Assert
                Assert.IsTrue(result, "DiscoverLocation should return true");
                Assert.IsTrue(location.IsDiscovered, "Location should be discovered");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestDiscoverLocation: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests changing the current location.
        /// </summary>
        [TestMethod]
        public void TestChangeLocation()
        {
            try
            {
                // Debug output
                LogMessage($"Initial location: {_mapSystem.GetCurrentLocation()}");
                
                // Discover the forest location first
                bool discovered = TestMapSystem.DiscoverLocation(TestData.LocationIds.Forest);
                LogMessage($"Forest discovered: {discovered}");
                
                // Change to the forest location
                bool result = _mapSystem.ChangeLocation(TestData.LocationIds.Forest);
                LogMessage($"ChangeLocation result: {result}");
                LogMessage($"New location: {_mapSystem.GetCurrentLocation()}");
                
                // Assert
                Assert.IsTrue(result, "ChangeLocation should return true");
                Assert.AreEqual(TestData.LocationIds.Forest, _mapSystem.GetCurrentLocation(), "Current location should be updated");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestChangeLocation: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests getting locations connected to the current location.
        /// </summary>
        [TestMethod]
        public void TestGetConnectedLocations()
        {
            try
            {
                // Debug output
                LogMessage($"Current location: {_mapSystem.GetCurrentLocation()}");
                
                // Act
                var connectedLocations = _mapSystem.GetConnectedLocations();
                LogMessage($"Connected locations count: {connectedLocations.Count}");
                
                foreach (var location in connectedLocations)
                {
                    LogMessage($"Connected location: {location.Id} - {location.Name}");
                }
                
                // Assert
                Assert.AreEqual(2, connectedLocations.Count, "Bunker should have 2 connected locations");
                
                // Check that forest and road are connected to bunker
                bool hasForest = false;
                bool hasRoad = false;
                
                foreach (var location in connectedLocations)
                {
                    if (location.Id == TestData.LocationIds.Forest) hasForest = true;
                    if (location.Id == "road") hasRoad = true;
                }
                
                Assert.IsTrue(hasForest, "Forest should be connected to bunker");
                Assert.IsTrue(hasRoad, "Road should be connected to bunker");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestGetConnectedLocations: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        /// <summary>
        /// Tests checking if a location is connected to the current location.
        /// </summary>
        [TestMethod]
        public void TestIsLocationConnected()
        {
            try
            {
                // Debug output
                LogMessage($"Current location: {_mapSystem.GetCurrentLocation()}");
                
                // Get all locations
                var locations = TestMapSystem.GetAllLocations();
                LogMessage($"All locations count: {locations.Count}");
                
                // Check if forest is connected
                bool isForestConnected = _mapSystem.IsLocationConnected(TestData.LocationIds.Forest);
                LogMessage($"Is forest connected: {isForestConnected}");
                
                // Check if road is connected
                bool isRoadConnected = _mapSystem.IsLocationConnected("road");
                LogMessage($"Is road connected: {isRoadConnected}");
                
                // Check if lake is connected
                bool isLakeConnected = _mapSystem.IsLocationConnected("lake");
                LogMessage($"Is lake connected: {isLakeConnected}");
                
                // Assert
                Assert.IsTrue(isForestConnected, "Forest should be connected to bunker");
                Assert.IsTrue(isRoadConnected, "Road should be connected to bunker");
                Assert.IsFalse(isLakeConnected, "Lake should not be connected to bunker");
            }
            catch (Exception ex)
            {
                LogError($"Error in TestIsLocationConnected: {ex.Message}");
                LogError(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
    }
}

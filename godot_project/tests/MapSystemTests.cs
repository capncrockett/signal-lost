using Godot;
using System;
using System.Collections.Generic;
using SignalLost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class MapSystemTests : Node
    {
        private TestMapSystem _mapSystem;

        // Setup method called before each test
        public void Setup()
        {
            // Create a new TestMapSystem
            _mapSystem = new TestMapSystem();
            AddChild(_mapSystem);

            // Initialize the map system
            _mapSystem._Ready();
        }

        // Teardown method called after each test
        public void Teardown()
        {
            // Clean up
            if (_mapSystem != null)
            {
                _mapSystem.QueueFree();
                _mapSystem = null;
            }
        }

        // Test getting a location
        [TestMethod]
        public void TestGetLocation()
        {
            // Arrange
            Setup();

            try
            {
                // Act
                var location = _mapSystem.GetLocation("bunker");

                // Assert
                AssertNotNull(location, "Location should not be null");
                AssertEqual(location.Id, "bunker", "Location ID should match");
                AssertEqual(location.Name, "Emergency Bunker", "Location name should match");
                AssertTrue(location.IsDiscovered, "Starting location should be discovered");
            }
            finally
            {
                Teardown();
            }
        }

        // Test discovering a location
        [TestMethod]
        public void TestDiscoverLocation()
        {
            // Arrange
            Setup();

            try
            {
                // Act
                bool result = _mapSystem.DiscoverLocation("forest");
                var location = _mapSystem.GetLocation("forest");

                // Assert
                AssertTrue(result, "DiscoverLocation should return true");
                AssertTrue(location.IsDiscovered, "Location should be discovered");
            }
            finally
            {
                Teardown();
            }
        }

        // Test changing location
        [TestMethod]
        public void TestChangeLocation()
        {
            // Arrange
            Setup();

            try
            {
                // Debug output
                GD.Print($"Initial location: {_mapSystem.GetCurrentLocation()}");

                // Discover the forest location first
                bool discovered = _mapSystem.DiscoverLocation("forest");
                GD.Print($"Forest discovered: {discovered}");

                // Change to the forest location
                bool result = _mapSystem.ChangeLocation("forest");
                GD.Print($"ChangeLocation result: {result}");
                GD.Print($"New location: {_mapSystem.GetCurrentLocation()}");

                // Assert
                AssertTrue(result, "ChangeLocation should return true");
                AssertEqual(_mapSystem.GetCurrentLocation(), "forest", "Current location should be updated");
            }
            finally
            {
                Teardown();
            }
        }

        // Test getting connected locations
        [TestMethod]
        public void TestGetConnectedLocations()
        {
            // Arrange
            Setup();

            try
            {
                // Debug output
                GD.Print($"Current location: {_mapSystem.GetCurrentLocation()}");

                // Act
                var connectedLocations = _mapSystem.GetConnectedLocations();
                GD.Print($"Connected locations count: {connectedLocations.Count}");

                foreach (var location in connectedLocations)
                {
                    GD.Print($"Connected location: {location.Id} - {location.Name}");
                }

                // Assert
                AssertEqual(connectedLocations.Count, 2, "Bunker should have 2 connected locations");

                // Check that forest and road are connected to bunker
                bool hasForest = false;
                bool hasRoad = false;

                foreach (var location in connectedLocations)
                {
                    if (location.Id == "forest") hasForest = true;
                    if (location.Id == "road") hasRoad = true;
                }

                AssertTrue(hasForest, "Forest should be connected to bunker");
                AssertTrue(hasRoad, "Road should be connected to bunker");
            }
            finally
            {
                Teardown();
            }
        }

        // Test location connection check
        [TestMethod]
        public void TestIsLocationConnected()
        {
            // Arrange
            Setup();

            try
            {
                // Debug output
                GD.Print($"Current location: {_mapSystem.GetCurrentLocation()}");

                // Get all locations
                var locations = _mapSystem.GetAllLocations();
                GD.Print($"All locations count: {locations.Count}");

                // Check if forest is connected
                bool isForestConnected = _mapSystem.IsLocationConnected("forest");
                GD.Print($"Is forest connected: {isForestConnected}");

                // Check if road is connected
                bool isRoadConnected = _mapSystem.IsLocationConnected("road");
                GD.Print($"Is road connected: {isRoadConnected}");

                // Check if lake is connected
                bool isLakeConnected = _mapSystem.IsLocationConnected("lake");
                GD.Print($"Is lake connected: {isLakeConnected}");

                // Assert
                AssertTrue(isForestConnected, "Forest should be connected to bunker");
                AssertTrue(isRoadConnected, "Road should be connected to bunker");
                AssertFalse(isLakeConnected, "Lake should not be connected to bunker");
            }
            finally
            {
                Teardown();
            }
        }

        // Assertion methods
        protected void AssertTrue(bool condition, string message = null)
        {
            if (!condition)
            {
                throw new Exception(message ?? "Assertion failed: Expected true");
            }
        }

        protected void AssertFalse(bool condition, string message = null)
        {
            if (condition)
            {
                throw new Exception(message ?? "Assertion failed: Expected false");
            }
        }

        protected void AssertEqual<T>(T actual, T expected, string message = null)
        {
            if (!actual.Equals(expected))
            {
                throw new Exception(message ?? $"Assertion failed: Expected {expected}, got {actual}");
            }
        }

        protected void AssertNotEqual<T>(T actual, T expected, string message = null)
        {
            if (actual.Equals(expected))
            {
                throw new Exception(message ?? $"Assertion failed: Expected not {expected}, got {actual}");
            }
        }

        protected void AssertNull(object obj, string message = null)
        {
            if (obj != null)
            {
                throw new Exception(message ?? "Assertion failed: Expected null");
            }
        }

        protected void AssertNotNull(object obj, string message = null)
        {
            if (obj == null)
            {
                throw new Exception(message ?? "Assertion failed: Expected not null");
            }
        }
    }
}

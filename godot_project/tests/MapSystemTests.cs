using Godot;
using System;

namespace SignalLost.Tests
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
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
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestGetLocation()
        {
            // Arrange
            Setup();

            try
            {
                // Act
                var location = _mapSystem.GetLocation("bunker");

                // Assert
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(location, "Location should not be null");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(location.Id, "bunker", "Location ID should match");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(location.Name, "Emergency Bunker", "Location name should match");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(location.IsDiscovered, "Starting location should be discovered");
            }
            finally
            {
                Teardown();
            }
        }

        // Test discovering a location
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
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
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(result, "DiscoverLocation should return true");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(location.IsDiscovered, "Location should be discovered");
            }
            finally
            {
                Teardown();
            }
        }

        // Test changing location
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
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
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(result, "ChangeLocation should return true");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(_mapSystem.GetCurrentLocation(), "forest", "Current location should be updated");
            }
            finally
            {
                Teardown();
            }
        }

        // Test getting connected locations
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
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
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual(connectedLocations.Count, 2, "Bunker should have 2 connected locations");

                // Check that forest and road are connected to bunker
                bool hasForest = false;
                bool hasRoad = false;

                foreach (var location in connectedLocations)
                {
                    if (location.Id == "forest") hasForest = true;
                    if (location.Id == "road") hasRoad = true;
                }

                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(hasForest, "Forest should be connected to bunker");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(hasRoad, "Road should be connected to bunker");
            }
            finally
            {
                Teardown();
            }
        }

        // Test location connection check
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
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
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(isForestConnected, "Forest should be connected to bunker");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(isRoadConnected, "Road should be connected to bunker");
                Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(isLakeConnected, "Lake should not be connected to bunker");
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

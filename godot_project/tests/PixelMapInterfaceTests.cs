using Godot;
using System;
using GUT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class PixelMapInterfaceTests : Test
    {
        private PixelMapInterface _mapInterface;
        private MapSystem _mapSystem;
        private GameState _gameState;

        // Setup method called before each test
        public void Before()
        {
            // Create a mock GameState
            _gameState = new GameState();
            AddChild(_gameState);
            _gameState.Name = "GameState";

            // Create a mock MapSystem
            _mapSystem = new MapSystem();
            AddChild(_mapSystem);
            _mapSystem.Name = "MapSystem";

            // Create the PixelMapInterface
            _mapInterface = new PixelMapInterface();
            AddChild(_mapInterface);
            _mapInterface.Name = "PixelMapInterface";

            // Call _Ready to initialize
            _mapInterface._Ready();
        }

        // Teardown method called after each test
        public void After()
        {
            // Remove nodes
            _mapInterface.QueueFree();
            _mapSystem.QueueFree();
            _gameState.QueueFree();
        }

        [TestMethod]
        public void TestInitialization()
        {
            // Verify that the map interface is initialized correctly
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsNotNull(_mapInterface, "PixelMapInterface should not be null");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(_mapInterface.IsVisible(), "PixelMapInterface should be hidden by default");
        }

        [TestMethod]
        public void TestVisibility()
        {
            // Test setting visibility
            _mapInterface.SetVisible(true);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(_mapInterface.IsVisible(), "PixelMapInterface should be visible after SetVisible(true)");

            _mapInterface.SetVisible(false);
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(_mapInterface.IsVisible(), "PixelMapInterface should be hidden after SetVisible(false)");

            // Test toggling visibility
            _mapInterface.ToggleVisibility();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(_mapInterface.IsVisible(), "PixelMapInterface should be visible after ToggleVisibility()");

            _mapInterface.ToggleVisibility();
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(_mapInterface.IsVisible(), "PixelMapInterface should be hidden after ToggleVisibility()");
        }

        [TestMethod]
        public void TestLocationDiscovery()
        {
            // Make the map interface visible
            _mapInterface.SetVisible(true);

            // Simulate discovering a location
            _mapSystem.DiscoverLocation("forest");

            // Verify that the location is discovered
            var location = _mapSystem.GetLocation("forest");
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(location.IsDiscovered, "Forest location should be discovered");
        }

        [TestMethod]
        public void TestLocationChange()
        {
            // Make the map interface visible
            _mapInterface.SetVisible(true);

            // Simulate changing the current location
            _mapSystem.DiscoverLocation("forest");
            _gameState.SetCurrentLocation("forest");

            // Verify that the current location is changed
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual("forest", _gameState.CurrentLocation, "Current location should be forest");
        }

        [TestMethod]
        public void TestInputHandling()
        {
            // Make the map interface visible
            _mapInterface.SetVisible(true);

            // Simulate pressing the Escape key
            var escapeEvent = new InputEventKey();
            escapeEvent.Keycode = Key.Escape;
            escapeEvent.Pressed = true;
            _mapInterface._Input(escapeEvent);

            // Verify that the map interface is hidden
            Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsFalse(_mapInterface.IsVisible(), "PixelMapInterface should be hidden after pressing Escape");
        }
    }
}

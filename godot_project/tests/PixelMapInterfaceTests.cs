using Godot;
using System;
using GUT;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class PixelMapInterfaceTests : Test
    {
        private PixelMapInterface _mapInterface;
        private MapSystem _mapSystem;
        private GameState _gameState;

        // Setup method called before each test
        public override void Before()
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
        public override void After()
        {
            // Remove nodes
            _mapInterface.QueueFree();
            _mapSystem.QueueFree();
            _gameState.QueueFree();
        }

        [Test]
        public void TestInitialization()
        {
            // Verify that the map interface is initialized correctly
            AssertNotNull(_mapInterface, "PixelMapInterface should not be null");
            AssertFalse(_mapInterface.IsVisible(), "PixelMapInterface should be hidden by default");
        }

        [Test]
        public void TestVisibility()
        {
            // Test setting visibility
            _mapInterface.SetVisible(true);
            AssertTrue(_mapInterface.IsVisible(), "PixelMapInterface should be visible after SetVisible(true)");

            _mapInterface.SetVisible(false);
            AssertFalse(_mapInterface.IsVisible(), "PixelMapInterface should be hidden after SetVisible(false)");

            // Test toggling visibility
            _mapInterface.ToggleVisibility();
            AssertTrue(_mapInterface.IsVisible(), "PixelMapInterface should be visible after ToggleVisibility()");

            _mapInterface.ToggleVisibility();
            AssertFalse(_mapInterface.IsVisible(), "PixelMapInterface should be hidden after ToggleVisibility()");
        }

        [Test]
        public void TestLocationDiscovery()
        {
            // Make the map interface visible
            _mapInterface.SetVisible(true);

            // Simulate discovering a location
            _mapSystem.EmitSignal(MapSystem.SignalName.LocationDiscovered, "forest");

            // Simulate a frame update
            _mapInterface._Process(0.016);
            _mapInterface._Draw();

            // No assertion needed, just checking that it doesn't crash
        }

        [Test]
        public void TestLocationChange()
        {
            // Make the map interface visible
            _mapInterface.SetVisible(true);

            // Simulate changing the current location
            _mapSystem.EmitSignal(MapSystem.SignalName.LocationChanged, "bunker");

            // Simulate a frame update
            _mapInterface._Process(0.016);
            _mapInterface._Draw();

            // No assertion needed, just checking that it doesn't crash
        }

        [Test]
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
            AssertFalse(_mapInterface.IsVisible(), "PixelMapInterface should be hidden after pressing Escape");

            // Make the map interface visible again
            _mapInterface.SetVisible(true);

            // Simulate a mouse wheel event for zooming
            var wheelEvent = new InputEventMouseButton();
            wheelEvent.ButtonIndex = MouseButton.WheelUp;
            wheelEvent.Pressed = true;
            wheelEvent.Position = new Vector2(100, 100);
            _mapInterface._Input(wheelEvent);

            // Simulate a frame update
            _mapInterface._Process(0.016);
            _mapInterface._Draw();

            // No assertion needed for zoom, just checking that it doesn't crash
        }
    }
}

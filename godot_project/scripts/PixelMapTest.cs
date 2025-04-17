using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelMapTest : Control
    {
        // References to UI elements
        private PixelMapInterface _mapInterface;
        private Button _discoverForestButton;
        private Button _discoverLakeButton;
        private Button _discoverCabinButton;
        private Button _discoverRoadButton;
        private Button _discoverTownButton;
        private Button _discoverFactoryButton;
        private Button _takeScreenshotButton;

        // References to game systems
        private MapSystem _mapSystem;
        private GameState _gameState;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _mapInterface = GetNode<PixelMapInterface>("PixelMapInterface");
            _discoverForestButton = GetNode<Button>("TestButtons/DiscoverForestButton");
            _discoverLakeButton = GetNode<Button>("TestButtons/DiscoverLakeButton");
            _discoverCabinButton = GetNode<Button>("TestButtons/DiscoverCabinButton");
            _discoverRoadButton = GetNode<Button>("TestButtons/DiscoverRoadButton");
            _discoverTownButton = GetNode<Button>("TestButtons/DiscoverTownButton");
            _discoverFactoryButton = GetNode<Button>("TestButtons/DiscoverFactoryButton");
            _takeScreenshotButton = GetNode<Button>("TestButtons/TakeScreenshotButton");

            // Get references to game systems
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_mapSystem == null || _gameState == null)
            {
                GD.PrintErr("PixelMapTest: Failed to get MapSystem or GameState reference");
                return;
            }

            // Connect button signals
            _discoverForestButton.Pressed += () => DiscoverLocation("forest");
            _discoverLakeButton.Pressed += () => DiscoverLocation("lake");
            _discoverCabinButton.Pressed += () => DiscoverLocation("cabin");
            _discoverRoadButton.Pressed += () => DiscoverLocation("road");
            _discoverTownButton.Pressed += () => DiscoverLocation("town");
            _discoverFactoryButton.Pressed += () => DiscoverLocation("factory");
            _takeScreenshotButton.Pressed += TakeScreenshot;

            // Hide map interface initially
            _mapInterface.SetVisible(false);

            GD.Print("PixelMapTest ready!");
        }

        // Process input events
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Toggle map with M key
                if (keyEvent.Keycode == Key.M)
                {
                    _mapInterface.ToggleVisibility();
                }
            }
        }

        // Discover a location
        private void DiscoverLocation(string locationId)
        {
            if (_mapSystem.DiscoverLocation(locationId))
            {
                GD.Print($"Discovered {locationId}");
            }
            else
            {
                GD.Print($"Failed to discover {locationId}");
            }
        }

        // Take a screenshot
        private void TakeScreenshot()
        {
            var screenshotTaker = GetNode<ScreenshotTaker>("/root/ScreenshotTaker");
            
            if (screenshotTaker != null)
            {
                screenshotTaker.TakeScreenshot("pixel_map_test");
                GD.Print("Screenshot taken");
            }
            else
            {
                // Fallback if ScreenshotTaker is not available
                var image = GetViewport().GetTexture().GetImage();
                image.SavePng("user://pixel_map_test.png");
                GD.Print("Screenshot saved to user://pixel_map_test.png");
            }
        }
    }
}

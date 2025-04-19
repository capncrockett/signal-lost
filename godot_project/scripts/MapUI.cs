using Godot;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class MapUI : Control
    {
        // References to UI elements
        private Label _locationTitle;
        private Label _locationDescription;
        private VBoxContainer _locationsList;
        private Button _closeButton;
        private Panel _mapView;

        // References to game systems
        private MapSystem _mapSystem;
        private GameState _gameState;

        // Map location nodes
        private Dictionary<string, Button> _locationButtons = new Dictionary<string, Button>();

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _locationTitle = GetNode<Label>("LocationInfo/LocationTitle");
            _locationDescription = GetNode<Label>("LocationInfo/LocationDescription");
            _locationsList = GetNode<VBoxContainer>("LocationInfo/LocationsList");
            _closeButton = GetNode<Button>("CloseButton");
            _mapView = GetNode<Panel>("MapView");

            // Get references to game systems
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_mapSystem == null || _gameState == null)
            {
                GD.PrintErr("MapUI: Failed to get MapSystem or GameState reference");
                return;
            }

            // Connect signals
            _closeButton.Pressed += OnCloseButtonPressed;
            _mapSystem.LocationDiscovered += OnLocationDiscovered;
            _mapSystem.LocationChanged += OnLocationChanged;
            _gameState.LocationChanged += OnLocationChanged;

            // Initialize the map
            InitializeMap();
            UpdateLocationInfo(_gameState.CurrentLocation);
        }

        // Initialize the map with location buttons
        private void InitializeMap()
        {
            // Clear existing location buttons
            foreach (var button in _locationButtons.Values)
            {
                button.QueueFree();
            }
            _locationButtons.Clear();

            // Create buttons for all discovered locations
            var discoveredLocations = _mapSystem.GetDiscoveredLocations();
            foreach (var location in discoveredLocations.Values)
            {
                CreateLocationButton(location);
            }
        }

        // Create a button for a location on the map
        private void CreateLocationButton(MapSystem.LocationData location)
        {
            var button = new Button();
            button.Text = location.Name;
            button.Position = location.Position;
            button.Size = new Vector2(100, 30);
            button.Pressed += () => OnLocationButtonPressed(location.Id);

            _mapView.AddChild(button);
            _locationButtons[location.Id] = button;
        }

        // Update the location info panel
        private void UpdateLocationInfo(string locationId)
        {
            var location = _mapSystem.GetLocation(locationId);
            if (location != null)
            {
                _locationTitle.Text = location.Name;
                _locationDescription.Text = location.Description;

                // Clear the connected locations list
                foreach (var child in _locationsList.GetChildren())
                {
                    child.QueueFree();
                }

                // Add buttons for connected locations
                foreach (var connectedId in location.ConnectedLocations)
                {
                    var connectedLocation = _mapSystem.GetLocation(connectedId);
                    if (connectedLocation != null && connectedLocation.IsDiscovered)
                    {
                        var button = new Button();
                        button.Text = connectedLocation.Name;
                        button.Pressed += () => OnTravelButtonPressed(connectedId);
                        _locationsList.AddChild(button);
                    }
                }
            }
        }

        // Event handlers
        private void OnCloseButtonPressed()
        {
            // Hide the map UI
            Visible = false;
        }

        private void OnLocationButtonPressed(string locationId)
        {
            // Show location info
            UpdateLocationInfo(locationId);
        }

        private void OnTravelButtonPressed(string locationId)
        {
            // Travel to the location
            if (_mapSystem.ChangeLocation(locationId))
            {
                UpdateLocationInfo(locationId);
            }
        }

        private void OnLocationDiscovered(string locationId)
        {
            // Add a new button for the discovered location
            var location = _mapSystem.GetLocation(locationId);
            if (location != null && !_locationButtons.ContainsKey(locationId))
            {
                CreateLocationButton(location);
            }
        }

        private void OnLocationChanged(string locationId)
        {
            // Update the location info
            UpdateLocationInfo(locationId);
        }
    }
}

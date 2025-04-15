using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class MapSystem : Node
    {
        // Map data
        public class LocationData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public List<string> ConnectedLocations { get; set; } = new List<string>();
            public Vector2 Position { get; set; } // Position on the map
            public bool IsDiscovered { get; set; } = false;
        }

        // Dictionary of all locations in the game
        private Dictionary<string, LocationData> _locations = new Dictionary<string, LocationData>();

        // Reference to the GameState
        private GameState _gameState;

        // Signals
        [Signal]
        public delegate void LocationDiscoveredEventHandler(string locationId);

        [Signal]
        public delegate void LocationChangedEventHandler(string locationId);

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get reference to GameState
            if (_gameState == null) // Only set if not already set (for testing)
            {
                _gameState = GetNode<GameState>("/root/GameState");
                if (_gameState == null)
                {
                    GD.PrintErr("MapSystem: Failed to get GameState reference");
                    return;
                }
            }

            // Initialize locations
            InitializeLocations();
        }

        // Initialize all locations in the game
        private void InitializeLocations()
        {
            // Add locations
            AddLocation(new LocationData
            {
                Id = "bunker",
                Name = "Emergency Bunker",
                Description = "A small underground bunker with basic supplies. This is where you start.",
                Position = new Vector2(100, 100),
                IsDiscovered = true, // Starting location is already discovered
                ConnectedLocations = new List<string> { "forest", "road" }
            });

            AddLocation(new LocationData
            {
                Id = "forest",
                Name = "Dense Forest",
                Description = "A thick forest with tall trees. It's easy to get lost here.",
                Position = new Vector2(200, 50),
                ConnectedLocations = new List<string> { "bunker", "lake", "cabin" }
            });

            AddLocation(new LocationData
            {
                Id = "road",
                Name = "Abandoned Road",
                Description = "An old road with abandoned vehicles. It leads to the town.",
                Position = new Vector2(150, 200),
                ConnectedLocations = new List<string> { "bunker", "town" }
            });

            AddLocation(new LocationData
            {
                Id = "lake",
                Name = "Mountain Lake",
                Description = "A serene lake surrounded by mountains. There's a small dock.",
                Position = new Vector2(300, 100),
                ConnectedLocations = new List<string> { "forest", "cabin" }
            });

            AddLocation(new LocationData
            {
                Id = "cabin",
                Name = "Hunter's Cabin",
                Description = "An old cabin that belonged to a hunter. It might have useful supplies.",
                Position = new Vector2(250, 150),
                ConnectedLocations = new List<string> { "forest", "lake" }
            });

            AddLocation(new LocationData
            {
                Id = "town",
                Name = "Abandoned Town",
                Description = "A small town that has been abandoned. Many buildings are damaged.",
                Position = new Vector2(200, 300),
                ConnectedLocations = new List<string> { "road", "factory" }
            });

            AddLocation(new LocationData
            {
                Id = "factory",
                Name = "Old Factory",
                Description = "An industrial factory that has been repurposed as a shelter by survivors.",
                Position = new Vector2(300, 350),
                ConnectedLocations = new List<string> { "town" }
            });
        }

        // Add a location to the map
        private void AddLocation(LocationData location)
        {
            _locations[location.Id] = location;
        }

        // Get a location by ID
        public LocationData GetLocation(string locationId)
        {
            if (_locations.ContainsKey(locationId))
            {
                return _locations[locationId];
            }
            return null;
        }

        // Get all locations
        public Dictionary<string, LocationData> GetAllLocations()
        {
            return _locations;
        }

        // Get all discovered locations
        public Dictionary<string, LocationData> GetDiscoveredLocations()
        {
            var discoveredLocations = new Dictionary<string, LocationData>();
            foreach (var location in _locations)
            {
                if (location.Value.IsDiscovered)
                {
                    discoveredLocations[location.Key] = location.Value;
                }
            }
            return discoveredLocations;
        }

        // Discover a location
        public bool DiscoverLocation(string locationId)
        {
            if (_locations.ContainsKey(locationId) && !_locations[locationId].IsDiscovered)
            {
                _locations[locationId].IsDiscovered = true;
                EmitSignal(SignalName.LocationDiscovered, locationId);
                return true;
            }
            return false;
        }

        // Change the current location
        public bool ChangeLocation(string locationId)
        {
            // Debug output
            GD.Print($"ChangeLocation - Attempting to change to location: {locationId}");
            GD.Print($"ChangeLocation - Current location: {_gameState.CurrentLocation}");

            // Check if the location exists and is discovered
            if (!_locations.ContainsKey(locationId))
            {
                GD.Print($"ChangeLocation - Location {locationId} not found in _locations");
                return false;
            }

            if (!_locations[locationId].IsDiscovered)
            {
                GD.Print($"ChangeLocation - Location {locationId} is not discovered");
                return false;
            }

            // Check if the location is connected to the current location
            var currentLocation = _gameState.CurrentLocation;
            GD.Print($"ChangeLocation - Checking if {locationId} is connected to {currentLocation}");

            if (currentLocation != null &&
                _locations.ContainsKey(currentLocation) &&
                !_locations[currentLocation].ConnectedLocations.Contains(locationId))
            {
                GD.Print($"ChangeLocation - Location {locationId} is not connected to {currentLocation}");
                return false;
            }

            // Change the location
            GD.Print($"ChangeLocation - Changing location to {locationId}");
            _gameState.SetCurrentLocation(locationId);
            EmitSignal(SignalName.LocationChanged, locationId);
            GD.Print($"ChangeLocation - New current location: {_gameState.CurrentLocation}");
            return true;
        }

        // Check if a location is connected to the current location
        public bool IsLocationConnected(string locationId)
        {
            var currentLocation = _gameState.CurrentLocation;

            // Debug output
            GD.Print($"IsLocationConnected - Current location: {currentLocation}");
            GD.Print($"IsLocationConnected - Checking if {locationId} is connected");

            if (currentLocation != null &&
                _locations.ContainsKey(currentLocation) &&
                _locations.ContainsKey(locationId))
            {
                var isConnected = _locations[currentLocation].ConnectedLocations.Contains(locationId);
                GD.Print($"IsLocationConnected - {locationId} is connected: {isConnected}");
                return isConnected;
            }

            GD.Print("IsLocationConnected - Returning false (conditions not met)");
            return false;
        }

        // Get connected locations to the current location
        public List<LocationData> GetConnectedLocations()
        {
            var connectedLocations = new List<LocationData>();
            var currentLocation = _gameState.CurrentLocation;

            // Debug output
            GD.Print($"GetConnectedLocations - Current location: {currentLocation}");

            if (currentLocation != null && _locations.ContainsKey(currentLocation))
            {
                GD.Print($"GetConnectedLocations - Found current location in _locations");
                GD.Print($"GetConnectedLocations - Connected IDs: {string.Join(", ", _locations[currentLocation].ConnectedLocations)}");

                foreach (var connectedId in _locations[currentLocation].ConnectedLocations)
                {
                    GD.Print($"GetConnectedLocations - Checking connected ID: {connectedId}");

                    if (_locations.ContainsKey(connectedId))
                    {
                        GD.Print($"GetConnectedLocations - Adding {connectedId} to connected locations");
                        connectedLocations.Add(_locations[connectedId]);
                    }
                    else
                    {
                        GD.Print($"GetConnectedLocations - Connected ID {connectedId} not found in _locations");
                    }
                }
            }
            else
            {
                GD.Print($"GetConnectedLocations - Current location not found in _locations");
            }

            GD.Print($"GetConnectedLocations - Returning {connectedLocations.Count} connected locations");
            return connectedLocations;
        }
    }
}

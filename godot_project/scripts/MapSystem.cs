using Godot;
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
            public bool IsAccessible { get; set; } = true;
            public string RequiredItem { get; set; } = null; // Item required to access this location
            public string LocationType { get; set; } = "standard"; // Type of location (standard, signal_source, shelter, danger, etc.)
            public float SignalStrength { get; set; } = 0.0f; // Signal strength at this location (0.0 - 1.0)
            public List<string> AvailableItems { get; set; } = new List<string>(); // Items that can be found at this location
            public string Notes { get; set; } = null; // Additional notes about the location
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

        [Signal]
        public delegate void SignalDetectedEventHandler(string signalId, float frequency);

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
                Description = "A small underground bunker with basic supplies and a radio. This is where you start.",
                Position = new Vector2(100, 100),
                IsDiscovered = true, // Starting location is already discovered
                ConnectedLocations = new List<string> { "forest", "road" },
                LocationType = "shelter",
                SignalStrength = 0.2f,
                AvailableItems = new List<string> { "flashlight", "batteries" },
                Notes = "The bunker provides basic protection but supplies are limited."
            });

            AddLocation(new LocationData
            {
                Id = "forest",
                Name = "Dense Forest",
                Description = "A thick forest with tall trees. It's easy to get lost here, but the elevation might help with radio reception.",
                Position = new Vector2(200, 50),
                ConnectedLocations = new List<string> { "bunker", "lake", "cabin" },
                LocationType = "standard",
                SignalStrength = 0.4f,
                AvailableItems = new List<string> { "map_fragment", "berries" }
            });

            AddLocation(new LocationData
            {
                Id = "road",
                Name = "Abandoned Road",
                Description = "An old road with abandoned vehicles. It leads to the town. Some vehicles might have useful items.",
                Position = new Vector2(150, 200),
                ConnectedLocations = new List<string> { "bunker", "town" },
                LocationType = "standard",
                SignalStrength = 0.3f,
                AvailableItems = new List<string> { "car_battery", "fuel" }
            });

            AddLocation(new LocationData
            {
                Id = "lake",
                Name = "Mountain Lake",
                Description = "A serene lake surrounded by mountains. There's a small dock and the water is surprisingly clear.",
                Position = new Vector2(300, 100),
                ConnectedLocations = new List<string> { "forest", "cabin" },
                LocationType = "standard",
                SignalStrength = 0.5f,
                AvailableItems = new List<string> { "fishing_rod", "water_bottle" }
            });

            AddLocation(new LocationData
            {
                Id = "cabin",
                Name = "Hunter's Cabin",
                Description = "An old cabin that belonged to a hunter. It contains various supplies and equipment for survival.",
                Position = new Vector2(250, 150),
                ConnectedLocations = new List<string> { "forest", "lake" },
                LocationType = "shelter",
                SignalStrength = 0.3f,
                AvailableItems = new List<string> { "hunting_knife", "binoculars", "canned_food" }
            });

            AddLocation(new LocationData
            {
                Id = "town",
                Name = "Abandoned Town",
                Description = "A small town that has been abandoned. Many buildings are damaged, but some might contain valuable resources.",
                Position = new Vector2(200, 300),
                ConnectedLocations = new List<string> { "road", "factory", "radio_tower" },
                LocationType = "danger",
                SignalStrength = 0.6f,
                AvailableItems = new List<string> { "medical_supplies", "tools", "keycard_research" },
                Notes = "Be cautious when exploring the town. Some areas may be unstable or dangerous."
            });

            AddLocation(new LocationData
            {
                Id = "factory",
                Name = "Old Factory",
                Description = "An industrial factory that has been repurposed as a shelter by survivors. It's well-fortified but access is restricted.",
                Position = new Vector2(300, 350),
                ConnectedLocations = new List<string> { "town" },
                LocationType = "shelter",
                IsAccessible = false,
                RequiredItem = "factory_keycard",
                SignalStrength = 0.4f,
                AvailableItems = new List<string> { "electronic_parts", "generator" }
            });

            AddLocation(new LocationData
            {
                Id = "radio_tower",
                Name = "Radio Tower",
                Description = "A tall radio tower on a hill overlooking the area. It appears to still be operational and could boost signal reception significantly.",
                Position = new Vector2(250, 250),
                ConnectedLocations = new List<string> { "town" },
                LocationType = "signal_source",
                SignalStrength = 0.9f,
                AvailableItems = new List<string> { "signal_amplifier", "radio_manual" },
                Notes = "The tower seems to be the source of some of the signals you've been picking up."
            });

            AddLocation(new LocationData
            {
                Id = "research_facility",
                Name = "Research Facility",
                Description = "A secretive research facility hidden in the mountains. It's heavily secured and requires special access.",
                Position = new Vector2(350, 200),
                ConnectedLocations = new List<string> { "radio_tower" },
                LocationType = "danger",
                IsAccessible = false,
                RequiredItem = "keycard_research",
                SignalStrength = 0.8f,
                AvailableItems = new List<string> { "research_notes", "experimental_device" },
                Notes = "This facility appears to be connected to the strange signals and events in the area."
            });

            AddLocation(new LocationData
            {
                Id = "mysterious_cave",
                Name = "Mysterious Cave",
                Description = "A deep cave system with unusual rock formations. Strange echoes and signals emanate from within.",
                Position = new Vector2(150, 50),
                ConnectedLocations = new List<string> { "forest" },
                LocationType = "signal_source",
                SignalStrength = 0.7f,
                AvailableItems = new List<string> { "strange_crystal", "ancient_artifact" },
                Notes = "The cave seems to amplify radio signals in an unexplained way."
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

        // Check if a location is discovered
        public bool IsLocationDiscovered(string locationId)
        {
            return _locations.ContainsKey(locationId) && _locations[locationId].IsDiscovered;
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

        // Set GameState reference (for testing)
        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
        }

        // Discover locations based on signal detection
        public void DiscoverLocationsBySignal(float frequency, float signalStrength)
        {
            // Find locations that have a signal strength close to the detected frequency
            foreach (var location in _locations.Values)
            {
                // Skip already discovered locations
                if (location.IsDiscovered)
                    continue;

                // Check if this location is a signal source or has significant signal strength
                if (location.LocationType == "signal_source" || location.SignalStrength > 0.5f)
                {
                    // Calculate a probability based on signal strength and frequency match
                    float frequencyMatch = 1.0f - Mathf.Abs(frequency % 10.0f - location.SignalStrength * 10.0f) / 10.0f;
                    float discoveryChance = location.SignalStrength * frequencyMatch * signalStrength;

                    // Higher chance for signal sources
                    if (location.LocationType == "signal_source")
                        discoveryChance *= 1.5f;

                    // Random chance to discover based on calculated probability
                    if (GD.Randf() < discoveryChance)
                    {
                        DiscoverLocation(location.Id);
                        GD.Print($"Discovered location {location.Name} through signal detection!");
                    }
                }
            }
        }

        // Handle signal detection
        public void OnSignalDetected(string signalId, float frequency, float signalStrength)
        {
            // Discover locations based on the detected signal
            DiscoverLocationsBySignal(frequency, signalStrength);

            // Emit signal for other systems to respond to
            EmitSignal(SignalName.SignalDetected, signalId, frequency);

            // Check if this signal completes any quest objectives
            if (_gameState != null)
            {
                _gameState.AddDiscoveredSignal(signalId, frequency);
                _gameState.CheckProgressionTriggers();
            }
        }
    }
}

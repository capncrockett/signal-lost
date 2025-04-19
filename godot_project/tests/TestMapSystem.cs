using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalLost.Tests
{
    /// <summary>
    /// A simplified TestMapSystem class for testing
    /// </summary>
    [GlobalClass]
    public partial class TestMapSystem : Node
    {
        // Current location
        private string _currentLocation = "bunker";

        // Dictionary of locations
        private static Dictionary<string, MapLocation> _locations = new Dictionary<string, MapLocation>();

        // Dictionary of connections between locations
        private static Dictionary<string, List<string>> _connections = new Dictionary<string, List<string>>();

        // Flag to track if initialization has been done
        private static bool _initialized = false;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Only initialize once
            if (!_initialized)
            {
                // Initialize locations
                InitializeLocations();

                // Initialize connections
                InitializeConnections();

                _initialized = true;
            }
        }

        // Initialize test locations
        private void InitializeLocations()
        {
            // Clear existing locations first to avoid duplicate key errors
            _locations.Clear();

            try
            {
                // Add test locations
                _locations["bunker"] = new MapLocation
                {
                    Id = "bunker",
                    Name = "Emergency Bunker",
                    Description = "A concrete bunker built for emergencies.",
                    IsDiscovered = true
                };

                _locations["forest"] = new MapLocation
                {
                    Id = "forest",
                    Name = "Dense Forest",
                    Description = "A thick forest with tall trees.",
                    IsDiscovered = false
                };

                _locations["road"] = new MapLocation
                {
                    Id = "road",
                    Name = "Abandoned Road",
                    Description = "An old road that hasn't been used in years.",
                    IsDiscovered = false
                };

                _locations["lake"] = new MapLocation
                {
                    Id = "lake",
                    Name = "Misty Lake",
                    Description = "A small lake surrounded by mist.",
                    IsDiscovered = false
                };

                _locations["cabin"] = new MapLocation
                {
                    Id = "cabin",
                    Name = "Old Cabin",
                    Description = "A weathered wooden cabin in the woods.",
                    IsDiscovered = false
                };
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error initializing locations: {ex.Message}");
            }
        }

        // Initialize connections between locations
        private void InitializeConnections()
        {
            // Clear existing connections first to avoid duplicate key errors
            _connections.Clear();

            try
            {
                // Add connections
                _connections["bunker"] = new List<string> { "forest", "road" };
                _connections["forest"] = new List<string> { "bunker", "lake", "cabin" };
                _connections["road"] = new List<string> { "bunker", "cabin" };
                _connections["lake"] = new List<string> { "forest" };
                _connections["cabin"] = new List<string> { "forest", "road" };
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error initializing connections: {ex.Message}");
            }
        }

        // Get a location by ID
        public MapLocation GetLocation(string locationId)
        {
            if (_locations.ContainsKey(locationId))
            {
                return _locations[locationId];
            }

            return null;
        }

        // Get the current location ID
        public string GetCurrentLocation()
        {
            return _currentLocation;
        }

        // Get all locations
        public List<MapLocation> GetAllLocations()
        {
            List<MapLocation> result = new List<MapLocation>();

            foreach (var location in _locations.Values)
            {
                result.Add(location);
            }

            return result;
        }

        // Get locations connected to the current location
        public List<MapLocation> GetConnectedLocations()
        {
            List<MapLocation> result = new List<MapLocation>();

            if (_connections.ContainsKey(_currentLocation))
            {
                foreach (var locationId in _connections[_currentLocation])
                {
                    if (_locations.ContainsKey(locationId))
                    {
                        result.Add(_locations[locationId]);
                    }
                }
            }

            return result;
        }

        // Check if a location is connected to the current location
        public bool IsLocationConnected(string locationId)
        {
            if (_connections.ContainsKey(_currentLocation))
            {
                return _connections[_currentLocation].Contains(locationId);
            }

            return false;
        }

        // Discover a location
        public bool DiscoverLocation(string locationId)
        {
            if (_locations.ContainsKey(locationId))
            {
                _locations[locationId].IsDiscovered = true;
                return true;
            }

            return false;
        }

        // Change the current location
        public bool ChangeLocation(string locationId)
        {
            // Check if the location exists and is discovered
            if (_locations.ContainsKey(locationId) && _locations[locationId].IsDiscovered)
            {
                // Check if the location is connected to the current location
                if (IsLocationConnected(locationId))
                {
                    _currentLocation = locationId;
                    return true;
                }
            }

            return false;
        }
    }

    // MapLocation class
    public class MapLocation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDiscovered { get; set; }
    }
}

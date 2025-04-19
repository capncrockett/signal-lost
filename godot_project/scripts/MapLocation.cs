using Godot;
using System.Collections.Generic;

namespace SignalLost
{
    public class MapLocation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Vector2 Position { get; set; }
        public bool IsDiscovered { get; set; } = false;
        public List<string> ConnectedLocations { get; set; } = new List<string>();
    }
}

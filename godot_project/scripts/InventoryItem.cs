using Godot;
using System;

namespace SignalLost
{
    public class InventoryItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; } = 1;
        public bool IsUsable { get; set; } = true;
        public bool IsDroppable { get; set; } = true;
        public string IconPath { get; set; }
    }
}

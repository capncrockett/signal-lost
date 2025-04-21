using Godot;
using System.Collections.Generic;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Represents an item in the game.
    /// </summary>
    public class ItemData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; } // e.g., "tool", "key", "document", "consumable"
        public bool IsUsable { get; set; }
        public bool IsConsumable { get; set; }
        public bool IsEquippable { get; set; }
        public bool IsCombineable { get; set; }
        public int Quantity { get; set; } = 1;
        public string IconPath { get; set; } = "res://assets/images/items/default_item.png";
        public int IconIndex { get; set; } = 0; // Index in the icon atlas
        public string Content { get; set; } = ""; // For documents or notes
        public List<string> CombinesWith { get; set; } = new List<string>(); // Items this can be combined with
        public string CombinationResult { get; set; } = ""; // Result of combining with another item
        public Dictionary<string, float> Effects { get; set; } = new Dictionary<string, float>(); // Effects when used (e.g., "health", 20)
    }
}

using Godot;
using System.Collections.Generic;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Initializes and manages the item database.
    /// </summary>
    [GlobalClass]
    public partial class ItemDatabase : Node
    {
        // Reference to the inventory core
        private InventoryCore _inventoryCore;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get the InventoryCore
            _inventoryCore = GetNode<InventoryCore>("/root/InventoryCore");

            if (_inventoryCore == null)
            {
                GD.PrintErr("ItemDatabase: InventoryCore not found");
                return;
            }

            // Initialize the item database
            InitializeItemDatabase();
        }

        // Initialize the item database with all available items in the game
        private void InitializeItemDatabase()
        {
            // Basic Tools
            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "flashlight",
                Name = "Flashlight",
                Description = "A battery-powered flashlight. Essential for dark areas.",
                Category = "tool",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/flashlight.png",
                IconIndex = 0,
                Effects = new Dictionary<string, float> { { "light", 1.0f } }
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "radio",
                Name = "Portable Radio",
                Description = "A battery-powered radio that can receive signals.",
                Category = "tool",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/radio.png",
                IconIndex = 1,
                Effects = new Dictionary<string, float> { { "signal_detection", 1.0f } }
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "map",
                Name = "Area Map",
                Description = "A map of the surrounding area. Shows known locations.",
                Category = "tool",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/map.png",
                IconIndex = 2,
                Effects = new Dictionary<string, float> { { "navigation", 1.0f } }
            });

            // Consumables
            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "battery",
                Name = "Battery",
                Description = "A standard AA battery. Can be used to power various devices.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IsEquippable = false,
                IconPath = "res://assets/images/items/battery.png",
                IconIndex = 3,
                Effects = new Dictionary<string, float> { { "power", 100.0f } }
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "medkit",
                Name = "First Aid Kit",
                Description = "Used to treat injuries and restore health.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IsEquippable = false,
                IconPath = "res://assets/images/items/medkit.png",
                IconIndex = 4,
                Effects = new Dictionary<string, float> { { "health", 50.0f } }
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "water_bottle",
                Name = "Water Bottle",
                Description = "Clean drinking water. Essential for survival.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IsEquippable = false,
                IconPath = "res://assets/images/items/water_bottle.png",
                IconIndex = 6,
                Effects = new Dictionary<string, float> { { "hydration", 40.0f } }
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "canned_food",
                Name = "Canned Food",
                Description = "A can of preserved food. Not tasty, but nutritious.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                IsEquippable = false,
                IconPath = "res://assets/images/items/canned_food.png",
                IconIndex = 5,
                Effects = new Dictionary<string, float> { { "energy", 30.0f } }
            });

            // Key Items
            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "key_cabin",
                Name = "Cabin Key",
                Description = "A rusty key that might open a cabin door.",
                Category = "key",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/key.png",
                IconIndex = 7,
                Effects = new Dictionary<string, float> { { "unlock_cabin", 1.0f } }
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "keycard_research",
                Name = "Research Facility Keycard",
                Description = "A keycard that grants access to the research facility.",
                Category = "key",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/keycard.png",
                IconIndex = 8,
                Effects = new Dictionary<string, float> { { "unlock_research_facility", 1.0f } }
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "military_badge",
                Name = "Military ID Badge",
                Description = "An ID badge that grants access to military installations.",
                Category = "key",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/badge.png",
                IconIndex = 9,
                Effects = new Dictionary<string, float> { { "unlock_military_outpost", 1.0f } }
            });

            // Documents
            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "map_fragment",
                Name = "Map Fragment",
                Description = "A torn piece of a map showing part of the area.",
                Category = "document",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/map_fragment.png",
                IconIndex = 10,
                Content = "This appears to be part of a larger map. It shows a path leading to a structure labeled 'Research'."
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "research_notes",
                Name = "Research Notes",
                Description = "Notes detailing experiments conducted at the research facility.",
                Category = "document",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/document.png",
                IconIndex = 11,
                Content = "Project SIGNAL: Attempt to establish communication across [REDACTED]. Initial tests show promising results but significant side effects including [REDACTED] and temporal anomalies."
            });

            // Components
            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "radio_part",
                Name = "Radio Component",
                Description = "A component that could be used to repair or upgrade a radio.",
                Category = "component",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IsCombineable = true,
                IconPath = "res://assets/images/items/radio_part.png",
                IconIndex = 12,
                CombinesWith = new List<string> { "radio_broken" },
                CombinationResult = "radio"
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "radio_broken",
                Name = "Broken Radio",
                Description = "A damaged radio that needs repair.",
                Category = "component",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IsCombineable = true,
                IconPath = "res://assets/images/items/radio_broken.png",
                IconIndex = 13,
                CombinesWith = new List<string> { "radio_part" },
                CombinationResult = "radio"
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "signal_amplifier",
                Name = "Signal Amplifier",
                Description = "A device that can boost radio signals.",
                Category = "component",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IsCombineable = true,
                IconPath = "res://assets/images/items/amplifier.png",
                IconIndex = 14,
                CombinesWith = new List<string> { "radio" },
                CombinationResult = "radio_enhanced"
            });

            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "radio_enhanced",
                Name = "Enhanced Radio",
                Description = "A radio with an amplifier that can detect weaker signals.",
                Category = "tool",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/radio_enhanced.png",
                IconIndex = 15,
                Effects = new Dictionary<string, float> { { "signal_detection", 2.0f } }
            });

            // Special Items
            _inventoryCore.AddItemToDatabase(new ItemData
            {
                Id = "strange_crystal",
                Name = "Strange Crystal",
                Description = "A crystal that emits a faint glow and seems to resonate with radio frequencies.",
                Category = "special",
                IsUsable = true,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/crystal.png",
                IconIndex = 16,
                Effects = new Dictionary<string, float> { { "signal_boost", 1.5f }, { "reveal_hidden_signals", 1.0f } }
            });
        }
    }
}

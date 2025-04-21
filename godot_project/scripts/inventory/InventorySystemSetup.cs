using Godot;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Sets up the inventory system.
    /// </summary>
    [GlobalClass]
    public partial class InventorySystemSetup : Node
    {
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Create the inventory core
            var inventoryCore = new InventoryCore();
            inventoryCore.Name = "InventoryCore";
            AddChild(inventoryCore);

            // Create the item effects system
            var itemEffectsSystem = new ItemEffectsSystem();
            itemEffectsSystem.Name = "ItemEffectsSystem";
            AddChild(itemEffectsSystem);

            // Create the item combination system
            var itemCombinationSystem = new ItemCombinationSystem();
            itemCombinationSystem.Name = "ItemCombinationSystem";
            AddChild(itemCombinationSystem);

            // Create the item database
            var itemDatabase = new ItemDatabase();
            itemDatabase.Name = "ItemDatabase";
            AddChild(itemDatabase);

            // Create the item icon atlas
            var itemIconAtlas = new ItemIconAtlas();
            itemIconAtlas.Name = "ItemIconAtlas";
            AddChild(itemIconAtlas);

            // Create the adapter for backward compatibility
            var inventorySystemAdapter = new InventorySystemAdapter();
            inventorySystemAdapter.Name = "InventorySystem";
            AddChild(inventorySystemAdapter);

            // Make all nodes global
            GetTree().Root.AddChild(inventoryCore);
            GetTree().Root.AddChild(itemEffectsSystem);
            GetTree().Root.AddChild(itemCombinationSystem);
            GetTree().Root.AddChild(itemDatabase);
            GetTree().Root.AddChild(itemIconAtlas);
            GetTree().Root.AddChild(inventorySystemAdapter);

            // Remove the nodes from this node
            RemoveChild(inventoryCore);
            RemoveChild(itemEffectsSystem);
            RemoveChild(itemCombinationSystem);
            RemoveChild(itemDatabase);
            RemoveChild(itemIconAtlas);
            RemoveChild(inventorySystemAdapter);

            // Log success
            GD.Print("Inventory system setup complete");
        }
    }
}

using Godot;

namespace SignalLost.Field
{
    /// <summary>
    /// Sets up the item placement system.
    /// </summary>
    [GlobalClass]
    public partial class ItemPlacementSetup : Node
    {
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Create the item placement system
            var itemPlacement = new ItemPlacement();
            itemPlacement.Name = "ItemPlacement";
            AddChild(itemPlacement);
            
            // Make it global
            GetTree().Root.AddChild(itemPlacement);
            
            // Remove it from this node
            RemoveChild(itemPlacement);
            
            GD.Print("ItemPlacementSetup: Item placement system initialized");
        }
    }
}

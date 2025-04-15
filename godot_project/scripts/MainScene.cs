using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class MainScene : Node
    {
        // References to UI elements
        private Button _mapButton;
        private Control _mapUI;
        private Button _inventoryButton;
        private Control _inventoryUI;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _mapButton = GetNode<Button>("MapButton");
            _mapUI = GetNode<Control>("MapUI");
            _inventoryButton = GetNode<Button>("InventoryButton");
            _inventoryUI = GetNode<Control>("InventoryUI");

            // Connect signals
            _mapButton.Pressed += OnMapButtonPressed;
            _inventoryButton.Pressed += OnInventoryButtonPressed;

            // Hide the UIs initially
            _mapUI.Visible = false;
            _inventoryUI.Visible = false;
        }

        // Event handlers
        private void OnMapButtonPressed()
        {
            // Toggle the map UI
            _mapUI.Visible = !_mapUI.Visible;

            // Hide the inventory UI if the map UI is shown
            if (_mapUI.Visible)
            {
                _inventoryUI.Visible = false;
            }
        }

        private void OnInventoryButtonPressed()
        {
            // Toggle the inventory UI
            _inventoryUI.Visible = !_inventoryUI.Visible;

            // Hide the map UI if the inventory UI is shown
            if (_inventoryUI.Visible)
            {
                _mapUI.Visible = false;
            }
        }
    }
}

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

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _mapButton = GetNode<Button>("MapButton");
            _mapUI = GetNode<Control>("MapUI");

            // Connect signals
            _mapButton.Pressed += OnMapButtonPressed;
        }

        // Event handlers
        private void OnMapButtonPressed()
        {
            // Show the map UI
            _mapUI.Visible = true;
        }
    }
}

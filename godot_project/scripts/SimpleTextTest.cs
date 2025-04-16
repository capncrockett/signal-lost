using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class SimpleTextTest : Control
    {
        private Button _closeButton;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("SimpleTextTest ready!");
            
            // Get the close button
            _closeButton = GetNode<Button>("Panel/Button");
            
            // Connect the button's pressed signal
            _closeButton.Pressed += OnCloseButtonPressed;
        }
        
        // Handle close button press
        private void OnCloseButtonPressed()
        {
            GD.Print("Close button pressed");
            GetTree().Quit();
        }
    }
}

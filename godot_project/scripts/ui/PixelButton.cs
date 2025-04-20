using Godot;
using System;

namespace SignalLost.UI
{
    /// <summary>
    /// A pixel-styled button control.
    /// </summary>
    [GlobalClass]
    public partial class PixelButton : PixelUIBase
    {
        [Export] public string Text { get; set; } = "Button";
        [Export] public bool Disabled { get; set; } = false;
        
        // Button state
        private bool _isHovered = false;
        private bool _isPressed = false;
        
        // Signals
        [Signal] public delegate void PressedEventHandler();
        [Signal] public delegate void HoveredEventHandler();
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            base._Ready();
            
            // Set minimum size
            CustomMinimumSize = new Vector2(100, 30);
            
            // Connect to mouse enter/exit signals
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            base._Draw();
            
            // Draw button
            bool isHighlighted = _isHovered || _isPressed;
            DrawThemedButton(new Rect2(0, 0, Size.X, Size.Y), Text, isHighlighted, Disabled);
        }
        
        // Process input events
        public override void _GuiInput(InputEvent @event)
        {
            if (Disabled)
                return;
                
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    if (mouseButtonEvent.Pressed)
                    {
                        _isPressed = true;
                        QueueRedraw();
                    }
                    else if (_isPressed)
                    {
                        _isPressed = false;
                        EmitSignal(SignalName.Pressed);
                        QueueRedraw();
                    }
                }
            }
            else if (@event is InputEventMouseMotion)
            {
                if (!_isHovered)
                {
                    _isHovered = true;
                    EmitSignal(SignalName.Hovered);
                    QueueRedraw();
                }
            }
        }
        
        // Called when mouse enters the control
        private void OnMouseEntered()
        {
            if (!Disabled)
            {
                _isHovered = true;
                QueueRedraw();
            }
        }
        
        // Called when mouse exits the control
        private void OnMouseExited()
        {
            _isHovered = false;
            _isPressed = false;
            QueueRedraw();
        }
    }
}

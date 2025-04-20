using Godot;
using System;

namespace SignalLost
{
    /// <summary>
    /// A pixel-styled slider control.
    /// </summary>
    [GlobalClass]
    public partial class PixelSlider : PixelUIBase
    {
        [Export] public float Value { get; set; } = 0.5f;
        [Export] public float MinValue { get; set; } = 0.0f;
        [Export] public float MaxValue { get; set; } = 1.0f;
        [Export] public float Step { get; set; } = 0.01f;
        [Export] public bool Vertical { get; set; } = false;
        [Export] public bool Disabled { get; set; } = false;
        
        // Slider state
        private bool _isDragging = false;
        
        // Signals
        [Signal] public delegate void ValueChangedEventHandler(float value);
        [Signal] public delegate void DragStartedEventHandler();
        [Signal] public delegate void DragEndedEventHandler();
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            base._Ready();
            
            // Set minimum size
            if (Vertical)
                CustomMinimumSize = new Vector2(20, 100);
            else
                CustomMinimumSize = new Vector2(100, 20);
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            base._Draw();
            
            // Calculate normalized value
            float normalizedValue = (Value - MinValue) / (MaxValue - MinValue);
            
            // Draw slider
            DrawThemedSlider(new Rect2(0, 0, Size.X, Size.Y), normalizedValue, Vertical);
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
                        _isDragging = true;
                        EmitSignal(SignalName.DragStarted);
                        UpdateValueFromMousePosition(mouseButtonEvent.Position);
                    }
                    else if (_isDragging)
                    {
                        _isDragging = false;
                        EmitSignal(SignalName.DragEnded);
                    }
                }
            }
            else if (@event is InputEventMouseMotion mouseMotionEvent && _isDragging)
            {
                UpdateValueFromMousePosition(mouseMotionEvent.Position);
            }
        }
        
        // Update the value based on mouse position
        private void UpdateValueFromMousePosition(Vector2 position)
        {
            float normalizedValue;
            
            if (Vertical)
            {
                // For vertical slider, 0 is at the bottom, 1 is at the top
                normalizedValue = 1.0f - (position.Y / Size.Y);
            }
            else
            {
                // For horizontal slider, 0 is at the left, 1 is at the right
                normalizedValue = position.X / Size.X;
            }
            
            // Clamp to 0-1 range
            normalizedValue = Mathf.Clamp(normalizedValue, 0.0f, 1.0f);
            
            // Convert to actual value
            float newValue = MinValue + normalizedValue * (MaxValue - MinValue);
            
            // Apply step if needed
            if (Step > 0)
            {
                newValue = Mathf.Round(newValue / Step) * Step;
            }
            
            // Update value if changed
            if (newValue != Value)
            {
                Value = newValue;
                EmitSignal(SignalName.ValueChanged, Value);
                QueueRedraw();
            }
        }
    }
}

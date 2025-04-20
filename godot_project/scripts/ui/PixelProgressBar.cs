using Godot;
using System;

namespace SignalLost.UI
{
    /// <summary>
    /// A pixel-styled progress bar control.
    /// </summary>
    [GlobalClass]
    public partial class PixelProgressBar : PixelUIBase
    {
        [Export] public float Value { get; set; } = 0.5f;
        [Export] public float MinValue { get; set; } = 0.0f;
        [Export] public float MaxValue { get; set; } = 1.0f;
        [Export] public bool Vertical { get; set; } = false;
        [Export] public bool ShowPercentage { get; set; } = false;
        [Export] public string Label { get; set; } = "";
        
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
            
            // Draw progress bar
            DrawThemedProgressBar(new Rect2(0, 0, Size.X, Size.Y), normalizedValue, Vertical);
            
            // Draw label if provided
            if (!string.IsNullOrEmpty(Label))
            {
                DrawString(ThemeDB.FallbackFont, new Vector2(Size.X / 2, Size.Y / 2 + 6),
                    Label, HorizontalAlignment.Center, -1, 12, _theme.TextColor);
            }
            
            // Draw percentage if enabled
            if (ShowPercentage)
            {
                string percentText = $"{normalizedValue * 100:F0}%";
                DrawString(ThemeDB.FallbackFont, new Vector2(Size.X / 2, Size.Y / 2 + 6),
                    percentText, HorizontalAlignment.Center, -1, 12, _theme.TextColor);
            }
        }
        
        /// <summary>
        /// Sets the value and updates the display.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        public void SetValue(float newValue)
        {
            // Clamp to min/max range
            newValue = Mathf.Clamp(newValue, MinValue, MaxValue);
            
            // Update value if changed
            if (newValue != Value)
            {
                Value = newValue;
                QueueRedraw();
            }
        }
    }
}

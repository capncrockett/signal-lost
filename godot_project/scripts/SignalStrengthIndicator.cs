using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class SignalStrengthIndicator : Control
    {
        // Signal strength indicator properties
        [Export]
        public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        
        [Export]
        public Color BorderColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        
        [Export]
        public Color WeakSignalColor { get; set; } = new Color(0.8f, 0.2f, 0.2f, 1.0f);
        
        [Export]
        public Color MediumSignalColor { get; set; } = new Color(0.8f, 0.8f, 0.2f, 1.0f);
        
        [Export]
        public Color StrongSignalColor { get; set; } = new Color(0.2f, 0.8f, 0.2f, 1.0f);
        
        [Export]
        public int NumBars { get; set; } = 5;
        
        [Export]
        public float BarWidth { get; set; } = 6.0f;
        
        [Export]
        public float BarSpacing { get; set; } = 2.0f;
        
        [Export]
        public float BarHeight { get; set; } = 20.0f;
        
        [Export]
        public float BorderWidth { get; set; } = 2.0f;
        
        [Export]
        public bool ShowPercentage { get; set; } = true;
        
        // Signal strength value (0.0 to 1.0)
        private float _signalStrength = 0.0f;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("SignalStrengthIndicator ready!");
        }
        
        // Set the signal strength (0.0 to 1.0)
        public void SetSignalStrength(float strength)
        {
            _signalStrength = Mathf.Clamp(strength, 0.0f, 1.0f);
            QueueRedraw();
        }
        
        // Get the current signal strength
        public float GetSignalStrength()
        {
            return _signalStrength;
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            // Calculate dimensions
            float totalWidth = NumBars * BarWidth + (NumBars - 1) * BarSpacing + BorderWidth * 2;
            float totalHeight = BarHeight + BorderWidth * 2;
            
            // Draw background and border
            DrawRect(new Rect2(0, 0, totalWidth, totalHeight), BorderColor);
            DrawRect(new Rect2(BorderWidth, BorderWidth, totalWidth - BorderWidth * 2, totalHeight - BorderWidth * 2), BackgroundColor);
            
            // Calculate how many bars to fill based on signal strength
            int filledBars = Mathf.FloorToInt(_signalStrength * NumBars);
            
            // Draw the bars
            for (int i = 0; i < NumBars; i++)
            {
                float x = BorderWidth + i * (BarWidth + BarSpacing);
                float y = BorderWidth;
                
                // Determine bar color based on position and fill state
                Color barColor;
                
                if (i < filledBars)
                {
                    // This bar is filled - determine color based on position
                    float barPosition = (float)i / NumBars;
                    
                    if (barPosition < 0.33f)
                        barColor = WeakSignalColor;
                    else if (barPosition < 0.66f)
                        barColor = MediumSignalColor;
                    else
                        barColor = StrongSignalColor;
                }
                else
                {
                    // This bar is not filled - use a darker version of what it would be
                    float barPosition = (float)i / NumBars;
                    
                    if (barPosition < 0.33f)
                        barColor = new Color(WeakSignalColor.R * 0.3f, WeakSignalColor.G * 0.3f, WeakSignalColor.B * 0.3f, 1.0f);
                    else if (barPosition < 0.66f)
                        barColor = new Color(MediumSignalColor.R * 0.3f, MediumSignalColor.G * 0.3f, MediumSignalColor.B * 0.3f, 1.0f);
                    else
                        barColor = new Color(StrongSignalColor.R * 0.3f, StrongSignalColor.G * 0.3f, StrongSignalColor.B * 0.3f, 1.0f);
                }
                
                // Draw the bar
                DrawRect(new Rect2(x, y, BarWidth, BarHeight), barColor);
            }
            
            // Draw percentage text if enabled
            if (ShowPercentage)
            {
                string percentText = $"{_signalStrength * 100:F0}%";
                Vector2 textSize = ThemeDB.FallbackFont.GetStringSize(percentText, fontSize: 16);
                Vector2 textPosition = new Vector2(totalWidth + 10, totalHeight / 2 - textSize.Y / 2);
                
                DrawString(ThemeDB.FallbackFont, textPosition, percentText, HorizontalAlignment.Left, -1, 16, new Color(0.8f, 0.8f, 0.8f, 1.0f));
            }
        }
    }
}

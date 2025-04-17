using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelSignalStrengthIndicator : Control
    {
        // Signal strength indicator properties
        [Export]
        public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        
        [Export]
        public Color BorderColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        
        [Export]
        public Color SignalColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        
        [Export]
        public int PixelSize { get; set; } = 4;
        
        [Export]
        public bool ShowText { get; set; } = true;
        
        // Signal strength value (0.0 to 1.0)
        private float _signalStrength = 0.0f;
        
        // Grid dimensions for the pixel art
        private const int GRID_WIDTH = 16;
        private const int GRID_HEIGHT = 16;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("PixelSignalStrengthIndicator ready!");
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
            float width = GRID_WIDTH * PixelSize;
            float height = GRID_HEIGHT * PixelSize;
            
            // Calculate offset to center the grid in the control
            float offsetX = (Size.X - width) / 2;
            float offsetY = (Size.Y - height) / 2;
            
            // Draw background
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), BackgroundColor);
            
            // Draw the pixel art signal strength indicator
            DrawSignalStrengthIcon(offsetX, offsetY);
            
            // Draw text if enabled
            if (ShowText)
            {
                string strengthText = $"{_signalStrength * 100:F0}%";
                Vector2 textSize = ThemeDB.FallbackFont.GetStringSize(strengthText, fontSize: 16);
                Vector2 textPosition = new Vector2(Size.X / 2 - textSize.X / 2, height + offsetY + 20);
                
                DrawString(ThemeDB.FallbackFont, textPosition, strengthText, HorizontalAlignment.Center, -1, 16, SignalColor);
            }
        }
        
        // Draw the pixel art signal strength icon
        private void DrawSignalStrengthIcon(float offsetX, float offsetY)
        {
            // Define the signal strength icon as a grid of pixels
            // 0 = empty, 1 = always filled, 2-5 = filled based on signal strength
            int[,] iconGrid = new int[GRID_WIDTH, GRID_HEIGHT]
            {
                {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0},
                {0,0,0,0,0,1,1,1,1,1,1,0,0,0,0,0},
                {0,0,0,0,1,1,1,1,1,1,1,1,0,0,0,0},
                {0,0,0,1,1,1,1,1,1,1,1,1,1,0,0,0},
                {0,0,1,1,1,1,1,1,1,1,1,1,1,1,0,0},
                {0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0},
                {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,5,5,5,5,0,0,0,0,0,0},
                {0,0,0,0,0,4,4,4,4,4,4,0,0,0,0,0},
                {0,0,0,0,3,3,3,3,3,3,3,3,0,0,0,0}
            };
            
            // Calculate signal strength thresholds
            float threshold2 = 0.25f;
            float threshold3 = 0.5f;
            float threshold4 = 0.75f;
            float threshold5 = 1.0f;
            
            // Draw each pixel
            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    int pixelType = iconGrid[y, x];
                    
                    // Determine if this pixel should be drawn
                    bool drawPixel = false;
                    
                    switch (pixelType)
                    {
                        case 1: // Always filled
                            drawPixel = true;
                            break;
                        case 2: // Filled if signal strength >= 25%
                            drawPixel = _signalStrength >= threshold2;
                            break;
                        case 3: // Filled if signal strength >= 50%
                            drawPixel = _signalStrength >= threshold3;
                            break;
                        case 4: // Filled if signal strength >= 75%
                            drawPixel = _signalStrength >= threshold4;
                            break;
                        case 5: // Filled if signal strength = 100%
                            drawPixel = _signalStrength >= threshold5;
                            break;
                    }
                    
                    // Draw the pixel if needed
                    if (drawPixel)
                    {
                        float pixelX = offsetX + x * PixelSize;
                        float pixelY = offsetY + y * PixelSize;
                        
                        // Calculate color based on signal strength
                        Color pixelColor = SignalColor;
                        
                        // For signal bars, adjust color based on level
                        if (pixelType >= 2 && pixelType <= 5)
                        {
                            float intensity = Mathf.Min(1.0f, _signalStrength * 1.2f);
                            pixelColor.A = intensity;
                        }
                        
                        DrawRect(new Rect2(pixelX, pixelY, PixelSize, PixelSize), pixelColor);
                    }
                }
            }
            
            // Draw signal waves based on signal strength
            if (_signalStrength > 0.1f)
            {
                DrawSignalWaves(offsetX, offsetY);
            }
        }
        
        // Draw animated signal waves
        private void DrawSignalWaves(float offsetX, float offsetY)
        {
            // Calculate wave parameters based on signal strength
            float maxRadius = GRID_WIDTH * PixelSize * 0.8f;
            int numWaves = Mathf.FloorToInt(_signalStrength * 3) + 1;
            
            // Get current time for animation
            float time = (float)Time.GetTicksMsec() / 1000.0f;
            
            // Calculate center of the antenna
            float centerX = offsetX + GRID_WIDTH * PixelSize / 2;
            float centerY = offsetY + 4 * PixelSize;
            
            // Draw each wave
            for (int i = 0; i < numWaves; i++)
            {
                // Calculate wave radius based on time
                float phase = (time * 2.0f + i * 0.33f) % 1.0f;
                float radius = phase * maxRadius;
                
                // Calculate color based on phase
                Color waveColor = SignalColor;
                waveColor.A = (1.0f - phase) * _signalStrength;
                
                // Draw the wave as a circle
                if (waveColor.A > 0.05f)
                {
                    DrawArc(new Vector2(centerX, centerY), radius, 0, Mathf.Pi, 16, waveColor, 2.0f * PixelSize / 4.0f);
                }
            }
        }
    }
}

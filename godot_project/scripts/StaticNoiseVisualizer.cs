using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class StaticNoiseVisualizer : Control
    {
        // Noise properties
        [Export]
        public Color NoiseColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        
        [Export]
        public Color BackgroundColor { get; set; } = new Color(0.05f, 0.05f, 0.05f, 1.0f);
        
        [Export]
        public float NoiseIntensity { get; set; } = 0.5f;
        
        [Export]
        public int PixelSize { get; set; } = 4;
        
        [Export]
        public float AnimationSpeed { get; set; } = 0.1f;
        
        // Visualization state
        private float _timer = 0.0f;
        private RandomNumberGenerator _rng = new RandomNumberGenerator();
        private float _signalStrength = 0.0f;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            _rng.Randomize();
            GD.Print("StaticNoiseVisualizer ready!");
        }
        
        // Process function called every frame
        public override void _Process(double delta)
        {
            _timer += (float)delta * AnimationSpeed;
            
            // Redraw every frame to animate the static
            QueueRedraw();
        }
        
        // Set the signal strength (0.0 to 1.0)
        public void SetSignalStrength(float strength)
        {
            _signalStrength = Mathf.Clamp(strength, 0.0f, 1.0f);
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            // Draw background
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), BackgroundColor);
            
            // Calculate noise parameters based on signal strength
            float noiseAmount = (1.0f - _signalStrength) * NoiseIntensity;
            
            // Draw static noise
            int pixelsX = (int)(Size.X / PixelSize);
            int pixelsY = (int)(Size.Y / PixelSize);
            
            for (int x = 0; x < pixelsX; x++)
            {
                for (int y = 0; y < pixelsY; y++)
                {
                    // Generate noise value
                    float noise = _rng.Randf();
                    
                    // Only draw pixels that exceed the threshold
                    if (noise < noiseAmount)
                    {
                        // Calculate pixel position
                        float pixelX = x * PixelSize;
                        float pixelY = y * PixelSize;
                        
                        // Calculate pixel color based on noise value
                        float intensity = noise / noiseAmount;
                        Color pixelColor = NoiseColor;
                        pixelColor.A = intensity * 0.8f;
                        
                        // Draw the pixel
                        DrawRect(new Rect2(pixelX, pixelY, PixelSize, PixelSize), pixelColor);
                    }
                }
            }
            
            // Draw signal visualization if signal is strong enough
            if (_signalStrength > 0.3f)
            {
                DrawSignalVisualization();
            }
        }
        
        // Draw a visualization of the signal
        private void DrawSignalVisualization()
        {
            // Calculate parameters
            float amplitude = Size.Y * 0.2f * _signalStrength;
            float frequency = 0.05f + _signalStrength * 0.1f;
            float phase = _timer * 5.0f;
            
            // Draw a sine wave
            Vector2[] points = new Vector2[Mathf.CeilToInt(Size.X)];
            for (int x = 0; x < Size.X; x++)
            {
                float y = Size.Y / 2 + Mathf.Sin(x * frequency + phase) * amplitude;
                points[x] = new Vector2(x, y);
            }
            
            // Draw the line
            Color lineColor = NoiseColor;
            lineColor.A = _signalStrength;
            
            for (int i = 1; i < points.Length; i++)
            {
                DrawLine(points[i - 1], points[i], lineColor, 2.0f);
            }
        }
    }
}

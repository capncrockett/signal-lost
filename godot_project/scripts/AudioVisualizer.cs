using Godot;
using System;

[GlobalClass]
namespace SignalLost
{
    public partial class AudioVisualizer : ColorRect
    {
        // Audio visualizer properties
        [Export]
        public int NumBars { get; set; } = 32;

        [Export]
        public float BarWidth { get; set; } = 4.0f;

        [Export]
        public float BarSpacing { get; set; } = 2.0f;

        [Export]
        public float MinBarHeight { get; set; } = 5.0f;

        [Export]
        public float MaxBarHeight { get; set; } = 100.0f;

        [Export]
        public Color SignalColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);

        [Export]
        public Color StaticColor { get; set; } = new Color(0.8f, 0.8f, 0.8f, 1.0f);

        [Export]
        public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);

        // Local state
        private float[] _barHeights;
        private float _signalStrength = 0.0f;
        private float _staticIntensity = 1.0f;
        private int _noiseSeed = 0;
        private float _timePassed = 0.0f;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Initialize bar heights
            _barHeights = new float[NumBars];
            for (int i = 0; i < NumBars; i++)
            {
                _barHeights[i] = MinBarHeight;
            }

            // Set background color
            Color = BackgroundColor;

            // Initialize random seed
            Random random = new Random();
            _noiseSeed = random.Next();
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            _timePassed += (float)delta;
            QueueRedraw();
        }

        // Custom drawing function
        public override void _Draw()
        {
            Vector2 size = Size;
            float rectWidth = size.X;
            float rectHeight = size.Y;

            // Calculate total width needed for all bars
            float totalWidth = NumBars * (BarWidth + BarSpacing) - BarSpacing;

            // Calculate starting x position to center the bars
            float startX = (rectWidth - totalWidth) / 2;

            // Draw each bar
            for (int i = 0; i < NumBars; i++)
            {
                // Calculate bar height based on signal and static
                float height = CalculateBarHeight(i);

                // Calculate bar position
                float x = startX + i * (BarWidth + BarSpacing);
                float y = rectHeight - height;

                // Calculate bar color based on signal strength and static intensity
                Color barColor = SignalColor.Lerp(StaticColor, _staticIntensity);

                // Draw the bar
                DrawRect(new Rect2(x, y, BarWidth, height), barColor);
            }
        }

        // Calculate the height of a specific bar
        private float CalculateBarHeight(int index)
        {
            float height = MinBarHeight;

            // Signal component - smooth sine wave
            if (_signalStrength > 0.0f)
            {
                float signalFreq = 2.0f + _signalStrength * 8.0f;  // Higher frequency with stronger signal
                float signalPhase = _timePassed * signalFreq;
                float signalValue = Mathf.Sin(signalPhase + index * 0.2f);
                // Ensure signal value is positive by using the square of the sine wave
                float signalAmplitude = signalValue * signalValue;
                // Add a minimum amplitude to ensure height is always greater than MinBarHeight when signal is present
                signalAmplitude = Mathf.Max(signalAmplitude, 0.2f);
                height += signalAmplitude * _signalStrength * (MaxBarHeight - MinBarHeight) * 0.5f;
            }

            // Static component - random noise
            if (_staticIntensity > 0.0f)
            {
                float noiseValue = NoiseAt(index, _timePassed);
                // Ensure noise value is at least 0.2 to make visible bars
                noiseValue = Mathf.Max(noiseValue, 0.2f);
                height += noiseValue * _staticIntensity * (MaxBarHeight - MinBarHeight) * 0.5f;
            }

            // Ensure height is within bounds
            height = Mathf.Clamp(height, MinBarHeight, MaxBarHeight);

            // Smooth transitions between frames
            float targetHeight = height;
            float currentHeight = _barHeights[index];
            float smoothing = 0.3f;
            height = currentHeight + (targetHeight - currentHeight) * smoothing;

            // Update stored height
            _barHeights[index] = height;

            return height;
        }

        // Generate noise value at a specific position and time
        private float NoiseAt(int posIndex, float time)
        {
            float p = posIndex * 0.1f;
            float t = time * 2.0f;
            return (Mathf.Sin(p * 7.0f + t * 3.0f) * 0.5f + 0.5f) * (Mathf.Cos(p * 9.0f - t * 4.0f) * 0.5f + 0.5f);
        }

        // Set the signal strength (0.0 to 1.0)
        public void SetSignalStrength(float strength)
        {
            _signalStrength = Mathf.Clamp(strength, 0.0f, 1.0f);
        }

        // Set the static intensity (0.0 to 1.0)
        public void SetStaticIntensity(float intensity)
        {
            _staticIntensity = Mathf.Clamp(intensity, 0.0f, 1.0f);
        }
    }
}

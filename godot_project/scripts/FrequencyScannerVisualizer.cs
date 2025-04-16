using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class FrequencyScannerVisualizer : Control
    {
        // Scanner properties
        [Export]
        public Color ScannerColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        
        [Export]
        public Color BackgroundColor { get; set; } = new Color(0.05f, 0.05f, 0.05f, 1.0f);
        
        [Export]
        public Color GridColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        
        [Export]
        public float MinFrequency { get; set; } = 88.0f;
        
        [Export]
        public float MaxFrequency { get; set; } = 108.0f;
        
        [Export]
        public float ScanSpeed { get; set; } = 0.5f;
        
        // Visualization state
        private float _currentFrequency;
        private float _scannerPosition;
        private bool _isScanning = false;
        private float _timer = 0.0f;
        private List<SignalPeak> _signalPeaks = new List<SignalPeak>();
        private RandomNumberGenerator _rng = new RandomNumberGenerator();
        
        // Class to represent a signal peak
        private class SignalPeak
        {
            public float Frequency { get; set; }
            public float Strength { get; set; }
            public float Width { get; set; }
        }
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            _rng.Randomize();
            _currentFrequency = (MinFrequency + MaxFrequency) / 2.0f;
            
            // Generate some random signal peaks
            GenerateRandomSignalPeaks(5);
            
            GD.Print("FrequencyScannerVisualizer ready!");
        }
        
        // Generate random signal peaks
        private void GenerateRandomSignalPeaks(int count)
        {
            _signalPeaks.Clear();
            
            for (int i = 0; i < count; i++)
            {
                float frequency = _rng.RandfRange(MinFrequency, MaxFrequency);
                float strength = _rng.RandfRange(0.3f, 1.0f);
                float width = _rng.RandfRange(0.2f, 0.8f);
                
                _signalPeaks.Add(new SignalPeak
                {
                    Frequency = frequency,
                    Strength = strength,
                    Width = width
                });
            }
            
            // Sort by frequency
            _signalPeaks.Sort((a, b) => a.Frequency.CompareTo(b.Frequency));
        }
        
        // Process function called every frame
        public override void _Process(double delta)
        {
            _timer += (float)delta;
            
            // Update scanner position if scanning
            if (_isScanning)
            {
                _scannerPosition += ScanSpeed * (float)delta;
                
                // Wrap around when we reach the end
                if (_scannerPosition > 1.0f)
                {
                    _scannerPosition = 0.0f;
                }
                
                // Calculate current frequency based on scanner position
                _currentFrequency = MinFrequency + _scannerPosition * (MaxFrequency - MinFrequency);
            }
            else
            {
                // Calculate scanner position based on current frequency
                _scannerPosition = (_currentFrequency - MinFrequency) / (MaxFrequency - MinFrequency);
            }
            
            QueueRedraw();
        }
        
        // Set the current frequency
        public void SetFrequency(float frequency)
        {
            _currentFrequency = Mathf.Clamp(frequency, MinFrequency, MaxFrequency);
            _scannerPosition = (_currentFrequency - MinFrequency) / (MaxFrequency - MinFrequency);
        }
        
        // Start or stop scanning
        public void SetScanning(bool scanning)
        {
            _isScanning = scanning;
        }
        
        // Get the signal strength at the current frequency
        public float GetSignalStrength()
        {
            float strength = 0.0f;
            
            // Calculate signal strength based on proximity to signal peaks
            foreach (var peak in _signalPeaks)
            {
                float distance = Mathf.Abs(_currentFrequency - peak.Frequency);
                float peakWidth = peak.Width;
                
                if (distance < peakWidth)
                {
                    // Calculate strength based on distance from peak center
                    float peakStrength = peak.Strength * (1.0f - (distance / peakWidth));
                    
                    // Take the strongest signal
                    strength = Mathf.Max(strength, peakStrength);
                }
            }
            
            // Add some noise
            strength += _rng.RandfRange(-0.05f, 0.05f);
            
            return Mathf.Clamp(strength, 0.0f, 1.0f);
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            // Draw background
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), BackgroundColor);
            
            // Draw grid
            DrawGrid();
            
            // Draw frequency spectrum
            DrawFrequencySpectrum();
            
            // Draw scanner line
            float scannerX = _scannerPosition * Size.X;
            DrawLine(new Vector2(scannerX, 0), new Vector2(scannerX, Size.Y), ScannerColor, 2.0f);
            
            // Draw current frequency text
            string frequencyText = $"{_currentFrequency:F1} MHz";
            DrawString(ThemeDB.FallbackFont, new Vector2(Size.X / 2, 20), 
                frequencyText, HorizontalAlignment.Center, -1, 16, ScannerColor);
        }
        
        // Draw the grid
        private void DrawGrid()
        {
            // Draw horizontal grid lines
            for (int i = 0; i <= 4; i++)
            {
                float y = Size.Y * i / 4.0f;
                DrawLine(new Vector2(0, y), new Vector2(Size.X, y), GridColor, 1.0f);
            }
            
            // Draw vertical grid lines (frequency markers)
            for (float freq = MinFrequency; freq <= MaxFrequency; freq += 2.0f)
            {
                float x = (freq - MinFrequency) / (MaxFrequency - MinFrequency) * Size.X;
                
                // Draw taller lines for multiples of 10
                float height = (Mathf.Abs(freq % 10.0f) < 0.1f) ? Size.Y : Size.Y / 4.0f;
                float y = Size.Y - height;
                
                DrawLine(new Vector2(x, y), new Vector2(x, Size.Y), GridColor, 1.0f);
                
                // Draw frequency labels for multiples of 10
                if (Mathf.Abs(freq % 10.0f) < 0.1f)
                {
                    DrawString(ThemeDB.FallbackFont, new Vector2(x, Size.Y - 5), 
                        $"{freq:F0}", HorizontalAlignment.Center, -1, 12, GridColor);
                }
            }
        }
        
        // Draw the frequency spectrum
        private void DrawFrequencySpectrum()
        {
            // Draw signal peaks
            foreach (var peak in _signalPeaks)
            {
                float peakX = (peak.Frequency - MinFrequency) / (MaxFrequency - MinFrequency) * Size.X;
                float peakHeight = Size.Y * 0.8f * peak.Strength;
                float peakWidth = Size.X * peak.Width / (MaxFrequency - MinFrequency);
                
                // Draw peak as a gradient
                for (int i = 0; i < peakWidth / 2; i++)
                {
                    float ratio = 1.0f - (i / (peakWidth / 2));
                    float height = peakHeight * ratio;
                    
                    Color color = ScannerColor;
                    color.A = ratio * 0.5f;
                    
                    // Draw left side
                    DrawLine(
                        new Vector2(peakX - i, Size.Y - height),
                        new Vector2(peakX - i, Size.Y),
                        color, 1.0f
                    );
                    
                    // Draw right side
                    DrawLine(
                        new Vector2(peakX + i, Size.Y - height),
                        new Vector2(peakX + i, Size.Y),
                        color, 1.0f
                    );
                }
                
                // Draw peak center
                DrawLine(
                    new Vector2(peakX, Size.Y - peakHeight),
                    new Vector2(peakX, Size.Y),
                    ScannerColor, 2.0f
                );
            }
        }
    }
}

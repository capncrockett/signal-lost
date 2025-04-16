using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class MorseVisualizer : ColorRect
    {
        // Morse visualizer properties
        [Export]
        public Color ActiveColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);

        [Export]
        public Color InactiveColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);

        [Export]
        public float DotWidth { get; set; } = 10.0f;

        [Export]
        public float DashWidth { get; set; } = 30.0f;

        [Export]
        public float ElementSpacing { get; set; } = 5.0f;

        [Export]
        public float SymbolSpacing { get; set; } = 15.0f;

        [Export]
        public float HistoryDuration { get; set; } = 5.0f; // How many seconds of history to show

        // Local state
        private List<MorseElement> _morseHistory = new List<MorseElement>();
        private float _timeSinceLastElement = 0.0f;
        private bool _isActive = false;
        private float _activeTime = 0.0f;
        private bool _isDot = false;

        // Morse code for "TEST": - (T) . (E) ... (S) - (T)
        private readonly bool[] _morseTest = new bool[] {
            // T: -
            true, false,  // Dash, then symbol space

            // Letter space between T and E
            false,

            // E: .
            true, false,  // Dot, then symbol space

            // Letter space between E and S
            false,

            // S: ...
            true, false,  // Dot, then symbol space
            true, false,  // Dot, then symbol space
            true, false,  // Dot, then symbol space

            // Letter space between S and T
            false,

            // T: -
            true, false,  // Dash, then symbol space

            // Word space at the end before repeating
            false, false
        };

        // Morse timing for each element (in seconds)
        private readonly float[] _morseTiming = new float[] {
            // T: -
            0.3f, 0.1f,  // Dash duration, symbol space

            // Letter space between T and E
            0.3f,

            // E: .
            0.1f, 0.1f,  // Dot duration, symbol space

            // Letter space between E and S
            0.3f,

            // S: ...
            0.1f, 0.1f,  // Dot duration, symbol space
            0.1f, 0.1f,  // Dot duration, symbol space
            0.1f, 0.1f,  // Dot duration, symbol space

            // Letter space between S and T
            0.3f,

            // T: -
            0.3f, 0.1f,  // Dash duration, symbol space

            // Word space at the end before repeating
            0.7f, 0.7f
        };

        private int _morseIndex = 0;
        private float _morseTimer = 0.0f;
        private float _signalStrength = 0.0f;

        // Class to represent a Morse code element (dot or dash)
        private class MorseElement
        {
            public bool IsDot { get; set; }
            public float TimeAdded { get; set; }
            public float SignalStrength { get; set; }
        }

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Set background color
            Color = InactiveColor;
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;
            
            // Update the morse code pattern based on timing
            UpdateMorsePattern(deltaF);
            
            // Update the history
            UpdateHistory(deltaF);
            
            // Redraw
            QueueRedraw();
        }

        // Update the Morse code pattern based on timing
        private void UpdateMorsePattern(float delta)
        {
            // Only process if we have signal
            if (_signalStrength <= 0.05f)
            {
                _isActive = false;
                return;
            }

            // Update the timer
            _morseTimer += delta;

            // Get the current morse code element
            bool isOn = _morseTest[_morseIndex];
            float elementDuration = _morseTiming[_morseIndex];

            // Update active state
            _isActive = isOn;
            
            // If active, update active time
            if (_isActive)
            {
                _activeTime += delta;
                _isDot = elementDuration <= 0.15f; // If duration is short, it's a dot
            }

            // If the element duration has passed, move to the next element
            if (_morseTimer >= elementDuration)
            {
                _morseTimer = 0.0f;
                
                // If we just finished an active element, add it to history
                if (isOn)
                {
                    AddElementToHistory(_isDot);
                    _activeTime = 0.0f;
                }
                
                // Move to next element
                _morseIndex = (_morseIndex + 1) % _morseTest.Length;
            }
        }

        // Add a new element to the history
        private void AddElementToHistory(bool isDot)
        {
            _morseHistory.Add(new MorseElement
            {
                IsDot = isDot,
                TimeAdded = 0.0f,
                SignalStrength = _signalStrength
            });
        }

        // Update the history (age elements and remove old ones)
        private void UpdateHistory(float delta)
        {
            // Update time for all elements
            for (int i = 0; i < _morseHistory.Count; i++)
            {
                _morseHistory[i].TimeAdded += delta;
            }

            // Remove elements that are too old
            _morseHistory.RemoveAll(e => e.TimeAdded > HistoryDuration);
        }

        // Custom drawing function
        public override void _Draw()
        {
            Vector2 size = Size;
            float rectWidth = size.X;
            float rectHeight = size.Y;

            // Draw the history
            float x = 10.0f;
            float y = rectHeight / 2.0f;
            float height = rectHeight * 0.6f;

            // Draw each element in the history
            foreach (var element in _morseHistory)
            {
                float width = element.IsDot ? DotWidth : DashWidth;
                float alpha = 1.0f - (element.TimeAdded / HistoryDuration);
                Color elementColor = ActiveColor;
                elementColor.A = alpha * element.SignalStrength;

                DrawRect(new Rect2(x, y - height / 2.0f, width, height), elementColor);
                
                // Move to next position
                x += width + ElementSpacing;
                
                // If we've reached the edge, wrap to next line
                if (x > rectWidth - DashWidth)
                {
                    x = 10.0f;
                    y += height + ElementSpacing;
                    
                    // If we've reached the bottom, stop drawing
                    if (y > rectHeight - height / 2.0f)
                    {
                        break;
                    }
                }
            }

            // Draw the current active element if there is one
            if (_isActive && _signalStrength > 0.05f)
            {
                float width = _isDot ? DotWidth : DashWidth * (_activeTime / 0.3f); // Animate dash growing
                width = Mathf.Min(width, DashWidth); // Cap at max dash width
                
                Color activeElementColor = ActiveColor;
                activeElementColor.A = _signalStrength;
                
                DrawRect(new Rect2(x, y - height / 2.0f, width, height), activeElementColor);
            }
        }

        // Set the signal strength (0.0 to 1.0)
        public void SetSignalStrength(float strength)
        {
            _signalStrength = Mathf.Clamp(strength, 0.0f, 1.0f);
        }
    }
}

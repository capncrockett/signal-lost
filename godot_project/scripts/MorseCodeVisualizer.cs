using Godot;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalLost
{
    [GlobalClass]
    public partial class MorseCodeVisualizer : Control
    {
        // Morse code properties
        [Export]
        public Color ActiveColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        
        [Export]
        public Color InactiveColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        
        [Export]
        public float DotDuration { get; set; } = 0.2f;
        
        [Export]
        public string Message { get; set; } = "SOS";
        
        // Visualization state
        private float _timer = 0.0f;
        private int _currentIndex = 0;
        private string _morseSequence = "";
        private bool _isActive = false;
        private bool _isPlaying = false;
        
        // Dictionary for converting characters to Morse code
        private static readonly Dictionary<char, string> _morseCodeMap = new Dictionary<char, string>
        {
            {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."},
            {'F', "..-."}, {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"},
            {'K', "-.-"}, {'L', ".-.."}, {'M', "--"}, {'N', "-."}, {'O', "---"},
            {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."}, {'S', "..."}, {'T', "-"},
            {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"}, {'Y', "-.--"},
            {'Z', "--.."}, {'0', "-----"}, {'1', ".----"}, {'2', "..---"}, {'3', "...--"},
            {'4', "....-"}, {'5', "....."}, {'6', "-...."}, {'7', "--..."}, {'8', "---.."}, 
            {'9', "----."}, {' ', "/"}
        };
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Convert message to Morse code
            ConvertMessageToMorse();
            
            // Start playing
            _isPlaying = true;
            
            GD.Print($"MorseCodeVisualizer ready! Message: {Message}, Morse: {_morseSequence}");
        }
        
        // Convert a text message to Morse code
        private void ConvertMessageToMorse()
        {
            StringBuilder morse = new StringBuilder();
            
            foreach (char c in Message.ToUpper())
            {
                if (_morseCodeMap.TryGetValue(c, out string code))
                {
                    morse.Append(code);
                    morse.Append(" "); // Space between characters
                }
            }
            
            _morseSequence = morse.ToString();
            _currentIndex = 0;
        }
        
        // Process function called every frame
        public override void _Process(double delta)
        {
            if (!_isPlaying || string.IsNullOrEmpty(_morseSequence))
                return;
            
            _timer += (float)delta;
            
            // Determine if we should be active based on the current character and timer
            if (_currentIndex < _morseSequence.Length)
            {
                char currentChar = _morseSequence[_currentIndex];
                float duration = (currentChar == '.') ? DotDuration : DotDuration * 3; // Dash is 3x longer than dot
                
                if (_timer >= duration)
                {
                    _timer = 0;
                    _currentIndex++;
                    
                    // Skip spaces
                    while (_currentIndex < _morseSequence.Length && _morseSequence[_currentIndex] == ' ')
                    {
                        _currentIndex++;
                        _timer = DotDuration; // Space between characters is one dot duration
                    }
                    
                    // If we've reached the end, loop back
                    if (_currentIndex >= _morseSequence.Length)
                    {
                        _currentIndex = 0;
                        _timer = 0;
                        // Add a longer pause between repetitions
                        _timer = -DotDuration * 7; // 7 dot durations between words
                    }
                }
                
                // Active during dots and dashes, inactive during spaces
                _isActive = (currentChar == '.' || currentChar == '-');
            }
            
            QueueRedraw();
        }
        
        // Set a new message
        public void SetMessage(string message)
        {
            Message = message;
            ConvertMessageToMorse();
            _currentIndex = 0;
            _timer = 0;
        }
        
        // Start or stop playing
        public void SetPlaying(bool playing)
        {
            _isPlaying = playing;
            if (_isPlaying)
            {
                _currentIndex = 0;
                _timer = 0;
            }
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            // Draw background
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), InactiveColor);
            
            // Draw Morse code visualization
            if (_isPlaying)
            {
                // Draw active state
                if (_isActive)
                {
                    DrawRect(new Rect2(0, 0, Size.X, Size.Y), ActiveColor);
                }
                
                // Draw Morse code sequence
                float dotWidth = Size.X / 20;
                float dashWidth = dotWidth * 3;
                float spacing = dotWidth / 2;
                float height = Size.Y / 3;
                float y = Size.Y - height - 10;
                float x = 10;
                
                for (int i = 0; i < _morseSequence.Length; i++)
                {
                    char c = _morseSequence[i];
                    Color color = (i == _currentIndex && _isActive) ? ActiveColor : new Color(0.3f, 0.3f, 0.3f, 1.0f);
                    
                    if (c == '.')
                    {
                        DrawRect(new Rect2(x, y, dotWidth, height), color);
                        x += dotWidth + spacing;
                    }
                    else if (c == '-')
                    {
                        DrawRect(new Rect2(x, y, dashWidth, height), color);
                        x += dashWidth + spacing;
                    }
                    else if (c == ' ')
                    {
                        x += dotWidth * 2; // Space between characters
                    }
                    else if (c == '/')
                    {
                        x += dotWidth * 4; // Space between words
                    }
                    
                    // Wrap to next line if needed
                    if (x > Size.X - dashWidth)
                    {
                        x = 10;
                        y -= height + spacing;
                        
                        // If we've run out of vertical space, stop drawing
                        if (y < 0)
                            break;
                    }
                }
            }
        }
    }
}

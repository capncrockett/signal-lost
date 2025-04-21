using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost.UI
{
    /// <summary>
    /// A simple pixel font implementation for the pixel UI system.
    /// </summary>
    [GlobalClass]
    public partial class PixelFont : Resource
    {
        [Export] public int CharacterWidth { get; set; } = 5;
        [Export] public int CharacterHeight { get; set; } = 7;
        
        // Character patterns dictionary
        private Dictionary<char, bool[,]> _characterPatterns;
        
        // Constructor
        public PixelFont()
        {
            InitializeCharacterPatterns();
        }
        
        // Initialize the character patterns
        private void InitializeCharacterPatterns()
        {
            _characterPatterns = new Dictionary<char, bool[,]>();
            
            // Define some basic characters
            // Each character is defined as a 2D array of booleans
            // where true means a pixel is drawn and false means it's transparent
            
            // Character: A
            _characterPatterns['A'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true }
            };
            
            // Character: B
            _characterPatterns['B'] = new bool[,]
            {
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false }
            };
            
            // Character: C
            _characterPatterns['C'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            // Add more characters as needed...
            
            // Default character (used for undefined characters)
            bool[,] defaultPattern = new bool[CharacterHeight, CharacterWidth];
            for (int y = 0; y < CharacterHeight; y++)
            {
                for (int x = 0; x < CharacterWidth; x++)
                {
                    defaultPattern[y, x] = (x == 0 || x == CharacterWidth - 1 || y == 0 || y == CharacterHeight - 1);
                }
            }
            
            _characterPatterns['?'] = defaultPattern;
        }
        
        /// <summary>
        /// Gets the pattern for a specific character.
        /// </summary>
        /// <param name="c">The character to get the pattern for.</param>
        /// <returns>A 2D array of booleans representing the character pattern.</returns>
        public bool[,] GetCharacterPattern(char c)
        {
            // Convert to uppercase for simplicity
            c = char.ToUpper(c);
            
            // Return the pattern if it exists, otherwise return the default pattern
            if (_characterPatterns.ContainsKey(c))
            {
                return _characterPatterns[c];
            }
            else
            {
                return _characterPatterns['?'];
            }
        }
        
        /// <summary>
        /// Measures the width of a string in pixels.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <returns>The width of the text in pixels.</returns>
        public int MeasureString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
                
            int width = 0;
            int maxWidth = 0;
            
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    maxWidth = Math.Max(maxWidth, width);
                    width = 0;
                }
                else
                {
                    width += CharacterWidth + 1; // Add 1 for spacing
                }
            }
            
            return Math.Max(maxWidth, width);
        }
        
        /// <summary>
        /// Measures the height of a string in pixels.
        /// </summary>
        /// <param name="text">The text to measure.</param>
        /// <returns>The height of the text in pixels.</returns>
        public int MeasureStringHeight(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
                
            int lines = 1;
            
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    lines++;
                }
            }
            
            return lines * (CharacterHeight + 1); // Add 1 for line spacing
        }
    }
}

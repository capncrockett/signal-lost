using Godot;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelFont : Resource
    {
        // Character size
        [Export]
        public int CharacterWidth { get; set; } = 5;
        
        [Export]
        public int CharacterHeight { get; set; } = 7;
        
        // Character patterns
        private Dictionary<char, bool[,]> _characterPatterns = new Dictionary<char, bool[,]>();
        
        // Constructor
        public PixelFont()
        {
            InitializeDefaultFont();
        }
        
        // Initialize the default pixel font
        private void InitializeDefaultFont()
        {
            // Define patterns for uppercase letters
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
            
            _characterPatterns['D'] = new bool[,]
            {
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false }
            };
            
            _characterPatterns['E'] = new bool[,]
            {
                { true, true, true, true, true },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, true }
            };
            
            _characterPatterns['F'] = new bool[,]
            {
                { true, true, true, true, true },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false }
            };
            
            _characterPatterns['G'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, false },
                { true, false, true, true, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            _characterPatterns['H'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true }
            };
            
            _characterPatterns['I'] = new bool[,]
            {
                { false, true, true, true, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, true, true, true, false }
            };
            
            _characterPatterns['J'] = new bool[,]
            {
                { false, false, false, true, false },
                { false, false, false, true, false },
                { false, false, false, true, false },
                { false, false, false, true, false },
                { true, false, false, true, false },
                { true, false, false, true, false },
                { false, true, true, false, false }
            };
            
            _characterPatterns['K'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, true, false },
                { true, false, true, false, false },
                { true, true, false, false, false },
                { true, false, true, false, false },
                { true, false, false, true, false },
                { true, false, false, false, true }
            };
            
            _characterPatterns['L'] = new bool[,]
            {
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, true }
            };
            
            _characterPatterns['M'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, true, false, true, true },
                { true, false, true, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true }
            };
            
            _characterPatterns['N'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, true, false, false, true },
                { true, false, true, false, true },
                { true, false, false, true, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true }
            };
            
            _characterPatterns['O'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            _characterPatterns['P'] = new bool[,]
            {
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false },
                { true, false, false, false, false },
                { true, false, false, false, false },
                { true, false, false, false, false }
            };
            
            _characterPatterns['Q'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, true, false, true },
                { true, false, false, true, false },
                { false, true, true, false, true }
            };
            
            _characterPatterns['R'] = new bool[,]
            {
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, false },
                { true, false, true, false, false },
                { true, false, false, true, false },
                { true, false, false, false, true }
            };
            
            _characterPatterns['S'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, false },
                { false, true, true, true, false },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            _characterPatterns['T'] = new bool[,]
            {
                { true, true, true, true, true },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false }
            };
            
            _characterPatterns['U'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            _characterPatterns['V'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, false, true, false },
                { false, false, true, false, false }
            };
            
            _characterPatterns['W'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, true, false, true },
                { true, true, false, true, true },
                { true, false, false, false, true }
            };
            
            _characterPatterns['X'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, false, true, false },
                { false, false, true, false, false },
                { false, true, false, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true }
            };
            
            _characterPatterns['Y'] = new bool[,]
            {
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, false, true, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false }
            };
            
            _characterPatterns['Z'] = new bool[,]
            {
                { true, true, true, true, true },
                { false, false, false, false, true },
                { false, false, false, true, false },
                { false, false, true, false, false },
                { false, true, false, false, false },
                { true, false, false, false, false },
                { true, true, true, true, true }
            };
            
            // Define patterns for lowercase letters (simplified versions of uppercase)
            _characterPatterns['a'] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, true, true, true, false },
                { false, false, false, false, true },
                { false, true, true, true, true },
                { true, false, false, false, true },
                { false, true, true, true, true }
            };
            
            // Define patterns for numbers
            _characterPatterns['0'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, true, true },
                { true, false, true, false, true },
                { true, true, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            _characterPatterns['1'] = new bool[,]
            {
                { false, false, true, false, false },
                { false, true, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, true, true, true, false }
            };
            
            _characterPatterns['2'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { false, false, false, false, true },
                { false, false, false, true, false },
                { false, false, true, false, false },
                { false, true, false, false, false },
                { true, true, true, true, true }
            };
            
            _characterPatterns['3'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { false, false, false, false, true },
                { false, false, true, true, false },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            _characterPatterns['4'] = new bool[,]
            {
                { false, false, false, true, false },
                { false, false, true, true, false },
                { false, true, false, true, false },
                { true, false, false, true, false },
                { true, true, true, true, true },
                { false, false, false, true, false },
                { false, false, false, true, false }
            };
            
            _characterPatterns['5'] = new bool[,]
            {
                { true, true, true, true, true },
                { true, false, false, false, false },
                { true, true, true, true, false },
                { false, false, false, false, true },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            _characterPatterns['6'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, false },
                { true, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            _characterPatterns['7'] = new bool[,]
            {
                { true, true, true, true, true },
                { false, false, false, false, true },
                { false, false, false, true, false },
                { false, false, true, false, false },
                { false, true, false, false, false },
                { false, true, false, false, false },
                { false, true, false, false, false }
            };
            
            _characterPatterns['8'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            _characterPatterns['9'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, true },
                { false, false, false, false, true },
                { true, false, false, false, true },
                { false, true, true, true, false }
            };
            
            // Define patterns for punctuation
            _characterPatterns['.'] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false }
            };
            
            _characterPatterns[','] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false }
            };
            
            _characterPatterns['!'] = new bool[,]
            {
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false }
            };
            
            _characterPatterns['?'] = new bool[,]
            {
                { false, true, true, true, false },
                { true, false, false, false, true },
                { false, false, false, false, true },
                { false, false, false, true, false },
                { false, false, true, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false }
            };
            
            _characterPatterns[':'] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false },
                { false, false, false, false, false }
            };
            
            _characterPatterns[';'] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, true, false, false },
                { false, false, true, false, false }
            };
            
            _characterPatterns['-'] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, true, true, true, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false }
            };
            
            _characterPatterns['_'] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { true, true, true, true, true }
            };
            
            _characterPatterns[' '] = new bool[,]
            {
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false },
                { false, false, false, false, false }
            };
            
            // Add more characters as needed
        }
        
        // Get the pattern for a character
        public bool[,] GetCharacterPattern(char c)
        {
            if (_characterPatterns.ContainsKey(c))
            {
                return _characterPatterns[c];
            }
            
            // Return a default pattern for unknown characters
            return new bool[,]
            {
                { true, true, true, true, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, false, false, false, true },
                { true, true, true, true, true }
            };
        }
    }
}

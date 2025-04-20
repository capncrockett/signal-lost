using Godot;
using System;

namespace SignalLost
{
    /// <summary>
    /// Base class for all pixel-based UI elements.
    /// </summary>
    [GlobalClass]
    public partial class PixelUIBase : Control
    {
        // Theme reference
        protected PixelTheme _theme;
        
        // Visual effects state
        protected float _scanlineOffset = 0.0f;
        protected float _flickerTimer = 0.0f;
        protected bool _showCursor = true;
        protected float _cursorBlinkTimer = 0.0f;
        protected Random _random = new Random();
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get theme reference
            _theme = PixelTheme.Instance;
            
            // Set up processing
            SetProcess(true);
            SetProcessInput(true);
        }
        
        // Process function called every frame
        public override void _Process(double delta)
        {
            // Update scanlines
            if (_theme.EnableScanlines)
            {
                _scanlineOffset += (float)delta * 30.0f; // Speed of scanline movement
                if (_scanlineOffset > 10.0f) // Reset after 10 pixels
                {
                    _scanlineOffset = 0;
                }
            }
            
            // Update screen flicker
            if (_theme.EnableScreenFlicker)
            {
                _flickerTimer += (float)delta;
                if (_flickerTimer >= 0.1f)
                {
                    _flickerTimer = 0;
                    // Random chance of screen flicker
                    if (_random.NextDouble() < 0.05f)
                    {
                        // Flicker will be drawn in _Draw
                    }
                }
            }
            
            // Update cursor blink
            _cursorBlinkTimer += (float)delta;
            if (_cursorBlinkTimer >= _theme.CursorBlinkSpeed)
            {
                _cursorBlinkTimer = 0;
                _showCursor = !_showCursor;
            }
            
            // Redraw
            QueueRedraw();
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            // Draw background
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), _theme.BackgroundColor);
            
            // Draw scanlines
            if (_theme.EnableScanlines)
            {
                _theme.DrawScanlines(this, _scanlineOffset);
            }
        }
        
        /// <summary>
        /// Draws pixel text at the specified position.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position to draw at.</param>
        /// <param name="color">The color of the text.</param>
        protected void DrawPixelText(string text, Vector2 position, Color color)
        {
            if (string.IsNullOrEmpty(text))
                return;
                
            // Get pixel font
            PixelFont font = GetPixelFont();
            
            float x = position.X;
            float y = position.Y;
            
            // Draw each character
            foreach (char c in text)
            {
                if (c == '\n')
                {
                    // New line
                    x = position.X;
                    y += _theme.CharacterSize + _theme.LineSpacing;
                    continue;
                }
                
                // Get character pattern
                bool[,] pattern = font.GetCharacterPattern(c);
                
                // Draw each pixel in the pattern
                for (int py = 0; py < pattern.GetLength(0); py++)
                {
                    for (int px = 0; px < pattern.GetLength(1); px++)
                    {
                        if (pattern[py, px])
                        {
                            // Apply interference effect
                            if (_theme.NoiseIntensity > 0 && _random.NextDouble() < _theme.NoiseIntensity)
                            {
                                // Skip this pixel (creates static effect)
                                continue;
                            }
                            
                            // Draw the pixel
                            DrawRect(new Rect2(
                                x + px,
                                y + py,
                                1, 1),
                                color);
                        }
                    }
                }
                
                // Move to next character position
                x += _theme.CharacterSize + 1;
            }
        }
        
        /// <summary>
        /// Gets the pixel font resource.
        /// </summary>
        protected static PixelFont GetPixelFont()
        {
            // Try to get a shared instance from the resource cache
            var font = ResourceLoader.Load<PixelFont>("res://resources/pixel_font.tres");
            
            // If not found, create a new instance
            if (font == null)
            {
                font = new PixelFont();
            }
            
            return font;
        }
        
        /// <summary>
        /// Draws a button with the theme style.
        /// </summary>
        protected void DrawThemedButton(Rect2 rect, string text, bool isHighlighted = false, bool isDisabled = false)
        {
            _theme.DrawButton(this, rect, text, isHighlighted, isDisabled);
        }
        
        /// <summary>
        /// Draws a panel with the theme style.
        /// </summary>
        protected void DrawThemedPanel(Rect2 rect)
        {
            _theme.DrawPanel(this, rect);
        }
        
        /// <summary>
        /// Draws a progress bar with the theme style.
        /// </summary>
        protected void DrawThemedProgressBar(Rect2 rect, float value, bool vertical = false)
        {
            _theme.DrawProgressBar(this, rect, value, vertical);
        }
        
        /// <summary>
        /// Draws a slider with the theme style.
        /// </summary>
        protected void DrawThemedSlider(Rect2 rect, float value, bool vertical = false)
        {
            _theme.DrawSlider(this, rect, value, vertical);
        }
    }
}

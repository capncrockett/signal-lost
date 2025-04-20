using Godot;
using System;

namespace SignalLost.UI
{
    /// <summary>
    /// Base class for all pixel-styled UI elements.
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

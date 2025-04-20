using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelMessageDisplay : Control
    {
        // Display properties
        [Export]
        public Color BackgroundColor { get; set; } = new Color(0.05f, 0.05f, 0.05f, 1.0f);

        [Export]
        public Color BorderColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);

        [Export]
        public Color TextColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);

        [Export]
        public Color TitleColor { get; set; } = new Color(0.0f, 0.9f, 0.0f, 1.0f);

        [Export]
        public Color ButtonColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);

        [Export]
        public Color ButtonHighlightColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 1.0f);

        [Export]
        public Color ButtonTextColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);

        [Export]
        public Color ScanlineColor { get; set; } = new Color(0.0f, 0.0f, 0.0f, 0.2f);

        [Export]
        public float NoiseIntensity { get; set; } = 0.2f;

        [Export]
        public float TypewriterSpeed { get; set; } = 0.05f;

        [Export]
        public float TypewriterSpeedVariation { get; set; } = 0.02f;

        [Export]
        public int CharacterSize { get; set; } = 8;

        [Export]
        public int LineSpacing { get; set; } = 2;

        [Export]
        public bool EnableScanlines { get; set; } = true;

        [Export]
        public bool EnableScreenFlicker { get; set; } = true;

        [Export]
        public bool EnableTypewriterSound { get; set; } = true;

        [Export(PropertyHint.Enum, "Terminal,Radio,Note,Computer")]
        public string MessageType { get; set; } = "Terminal";

        [Export]
        public bool ShowTimestamp { get; set; } = true;

        // Message content
        private string _title = "";
        private string _content = "";
        private bool _isDecoded = false;
        private float _interference = 0.0f;
        private string _timestamp = "";
        private List<string> _asciiArt = new List<string>();

        // Display state
        private bool _isVisible = false;
        private bool _isTyping = false;
        private int _currentCharIndex = 0;
        private string _displayedText = "";
        private float _typingTimer = 0.0f;
        private float _currentTypewriterSpeed = 0.05f;
        private float _flickerTimer = 0.0f;
        private float _scanlineOffset = 0.0f;
        private bool _showCursor = true;
        private float _cursorBlinkTimer = 0.0f;
        private Random _random = new Random();

        // Audio
        private AudioStreamPlayer _typewriterSound;
        private AudioStreamPlayer _interfaceSound;

        // UI interaction areas
        private Rect2 _closeButtonRect;
        private Rect2 _decodeButtonRect;

        // Signals
        [Signal]
        public delegate void MessageClosedEventHandler();

        [Signal]
        public delegate void DecodeRequestedEventHandler(string messageId);

        // Current message ID
        private string _currentMessageId;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            UpdateInteractionAreas();

            // Initialize audio players
            _typewriterSound = new AudioStreamPlayer();
            _typewriterSound.VolumeDb = -10.0f;
            AddChild(_typewriterSound);

            _interfaceSound = new AudioStreamPlayer();
            _interfaceSound.VolumeDb = -8.0f;
            AddChild(_interfaceSound);

            // Load sound effects
            LoadSoundEffects();

            // Set initial timestamp
            UpdateTimestamp();
        }

        // Load sound effects based on message type
        private void LoadSoundEffects()
        {
            string typewriterSoundPath = "res://assets/audio/sfx/typewriter.wav";
            string interfaceSoundPath = "res://assets/audio/sfx/interface_click.wav";

            // Adjust sound paths based on message type
            switch (MessageType)
            {
                case "Terminal":
                    typewriterSoundPath = "res://assets/audio/sfx/terminal_type.wav";
                    interfaceSoundPath = "res://assets/audio/sfx/terminal_click.wav";
                    break;
                case "Radio":
                    typewriterSoundPath = "res://assets/audio/sfx/radio_static.wav";
                    interfaceSoundPath = "res://assets/audio/sfx/radio_tune.wav";
                    break;
                case "Note":
                    typewriterSoundPath = "res://assets/audio/sfx/paper_rustle.wav";
                    interfaceSoundPath = "res://assets/audio/sfx/paper_flip.wav";
                    break;
                case "Computer":
                    typewriterSoundPath = "res://assets/audio/sfx/computer_type.wav";
                    interfaceSoundPath = "res://assets/audio/sfx/computer_beep.wav";
                    break;
            }

            // Try to load the sound effects
            try
            {
                var typewriterSound = ResourceLoader.Load<AudioStream>(typewriterSoundPath);
                if (typewriterSound != null)
                {
                    _typewriterSound.Stream = typewriterSound;
                }

                var interfaceSound = ResourceLoader.Load<AudioStream>(interfaceSoundPath);
                if (interfaceSound != null)
                {
                    _interfaceSound.Stream = interfaceSound;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error loading sound effects: {ex.Message}");
            }
        }

        // Update the timestamp
        private void UpdateTimestamp()
        {
            DateTime now = DateTime.Now;
            _timestamp = $"{now:yyyy-MM-dd HH:mm:ss}";
        }

        // Update the interaction areas based on current size
        private void UpdateInteractionAreas()
        {
            Vector2 size = Size;
            float width = size.X;
            float height = size.Y;

            // Calculate button dimensions
            float buttonWidth = 100;
            float buttonHeight = 30;
            float buttonY = height - buttonHeight - 20;

            // Set up button rects
            _closeButtonRect = new Rect2(
                width - buttonWidth - 20,
                buttonY,
                buttonWidth,
                buttonHeight
            );

            _decodeButtonRect = new Rect2(
                20,
                buttonY,
                buttonWidth,
                buttonHeight
            );
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            if (!_isVisible) return;

            // Update typewriter effect
            if (_isTyping)
            {
                _typingTimer += (float)delta;

                if (_typingTimer >= _currentTypewriterSpeed)
                {
                    _typingTimer = 0;

                    if (_currentCharIndex < _content.Length)
                    {
                        _currentCharIndex++;
                        _displayedText = _content.Substring(0, _currentCharIndex);

                        // Vary the typewriter speed slightly for a more natural effect
                        _currentTypewriterSpeed = TypewriterSpeed +
                            ((float)_random.NextDouble() * 2 - 1) * TypewriterSpeedVariation;

                        // Play typing sound
                        if (EnableTypewriterSound && _typewriterSound != null && _typewriterSound.Stream != null)
                        {
                            if (!_typewriterSound.Playing)
                            {
                                _typewriterSound.Play();
                            }
                        }
                    }
                    else
                    {
                        _isTyping = false;
                    }
                }
            }

            // Update cursor blink
            _cursorBlinkTimer += (float)delta;
            if (_cursorBlinkTimer >= 0.5f)
            {
                _cursorBlinkTimer = 0;
                _showCursor = !_showCursor;
            }

            // Update screen flicker
            if (EnableScreenFlicker)
            {
                _flickerTimer += (float)delta;
                if (_flickerTimer >= 0.1f)
                {
                    _flickerTimer = 0;
                    // Random chance of screen flicker
                    if (_random.NextDouble() < 0.05f * _interference)
                    {
                        // Flicker will be drawn in _Draw
                    }
                }
            }

            // Update scanlines
            if (EnableScanlines)
            {
                _scanlineOffset += (float)delta * 30.0f; // Speed of scanline movement
                if (_scanlineOffset > 10.0f) // Reset after 10 pixels
                {
                    _scanlineOffset = 0;
                }
            }

            // Redraw
            QueueRedraw();
        }

        // Set the message to display
        public void SetMessage(string messageId, string title, string content, bool isDecoded, float interference = 0.0f)
        {
            _currentMessageId = messageId;
            _title = title;
            _content = content;
            _isDecoded = isDecoded;
            _interference = Mathf.Clamp(interference, 0.0f, 1.0f);

            // Reset typing state
            _isTyping = true;
            _currentCharIndex = 0;
            _displayedText = "";
            _typingTimer = 0;
            _currentTypewriterSpeed = TypewriterSpeed;

            // Update timestamp
            UpdateTimestamp();

            // Clear any ASCII art
            _asciiArt.Clear();

            // Show the message
            _isVisible = true;

            // Play interface sound
            if (_interfaceSound != null && _interfaceSound.Stream != null)
            {
                _interfaceSound.Play();
            }
        }

        // Set the message to display with ASCII art
        public void SetMessageWithArt(string messageId, string title, string content, List<string> asciiArt, bool isDecoded, float interference = 0.0f)
        {
            // Set the basic message
            SetMessage(messageId, title, content, isDecoded, interference);

            // Add the ASCII art
            _asciiArt = new List<string>(asciiArt);
        }

        // Show or hide the message display
        public new void SetVisible(bool visible)
        {
            _isVisible = visible;

            if (!_isVisible)
            {
                // Reset typing state
                _isTyping = false;
                _currentCharIndex = 0;
                _displayedText = "";
            }
        }

        // Custom drawing function
        public override void _Draw()
        {
            if (!_isVisible) return;

            Vector2 size = Size;
            float width = size.X;
            float height = size.Y;

            // Draw background with potential flicker effect
            Color bgColor = BackgroundColor;
            if (EnableScreenFlicker && _random.NextDouble() < 0.05f * _interference)
            {
                // Add slight brightness variation for flicker effect
                float flicker = (float)_random.NextDouble() * 0.2f;
                bgColor = new Color(
                    Mathf.Clamp(bgColor.R + flicker, 0, 1),
                    Mathf.Clamp(bgColor.G + flicker, 0, 1),
                    Mathf.Clamp(bgColor.B + flicker, 0, 1),
                    bgColor.A
                );
            }
            DrawRect(new Rect2(0, 0, width, height), bgColor);

            // Draw border
            DrawRect(new Rect2(0, 0, width, height), BorderColor, false, 2);

            // Draw title bar
            float titleHeight = 40;
            DrawRect(new Rect2(0, 0, width, titleHeight), new Color(0.1f, 0.1f, 0.1f, 1.0f));

            // Draw title text
            DrawString(ThemeDB.FallbackFont, new Vector2(width / 2, titleHeight / 2 + 8),
                _title, HorizontalAlignment.Center, -1, 18, TitleColor);

            // Draw timestamp if enabled
            if (ShowTimestamp && !string.IsNullOrEmpty(_timestamp))
            {
                DrawString(ThemeDB.FallbackFont, new Vector2(width - 10, titleHeight / 2 + 8),
                    _timestamp, HorizontalAlignment.Right, -1, 10, new Color(0.5f, 0.5f, 0.5f, 1.0f));
            }

            // Draw content area
            float contentY = titleHeight + 10;
            float contentHeight = height - contentY - 60; // Leave space for buttons

            // Draw ASCII art if available
            if (_asciiArt.Count > 0)
            {
                DrawAsciiArt(new Rect2(20, contentY, width - 40, contentHeight / 3));
                contentY += contentHeight / 3 + 10;
                contentHeight -= contentHeight / 3 + 10;
            }

            // Draw the text content with typewriter effect
            if (!string.IsNullOrEmpty(_displayedText))
            {
                DrawPixelText(_displayedText, new Rect2(20, contentY, width - 40, contentHeight));

                // Draw cursor at end of text if typing is complete and cursor is visible
                if (!_isTyping && _showCursor)
                {
                    // Calculate cursor position based on text layout
                    Vector2 cursorPos = CalculateCursorPosition(_displayedText, new Rect2(20, contentY, width - 40, contentHeight));
                    DrawRect(new Rect2(cursorPos.X, cursorPos.Y, 8, 2), TextColor);
                }
            }

            // Draw scanlines if enabled
            if (EnableScanlines)
            {
                DrawScanlines(width, height);
            }

            // Draw buttons
            DrawButton(_closeButtonRect, "Close", false);

            if (!_isDecoded)
            {
                DrawButton(_decodeButtonRect, "Decode", false);
            }
        }

        // Draw pixel-style text
        private void DrawPixelText(string text, Rect2 bounds)
        {
            float x = bounds.Position.X;
            float y = bounds.Position.Y;
            float maxWidth = bounds.Size.X;
            float maxHeight = bounds.Size.Y;

            // Split text into words
            string[] words = text.Split(' ');
            float currentX = x;
            float currentY = y;

            foreach (string word in words)
            {
                // Calculate word width
                float wordWidth = word.Length * (CharacterSize + 1);

                // Check if we need to wrap to next line
                if (currentX + wordWidth > x + maxWidth)
                {
                    currentX = x;
                    currentY += CharacterSize + LineSpacing;

                    // Check if we've exceeded the bounds
                    if (currentY + CharacterSize > y + maxHeight)
                    {
                        break;
                    }
                }

                // Draw each character in the word
                foreach (char c in word)
                {
                    DrawPixelCharacter(c, new Vector2(currentX, currentY));
                    currentX += CharacterSize + 1;
                }

                // Add space after word
                currentX += CharacterSize;

                // Check if we've exceeded the bounds
                if (currentX > x + maxWidth)
                {
                    currentX = x;
                    currentY += CharacterSize + LineSpacing;

                    if (currentY + CharacterSize > y + maxHeight)
                    {
                        break;
                    }
                }
            }
        }

        // Draw a single pixel character
        private void DrawPixelCharacter(char c, Vector2 position)
        {
            // Get the pixel pattern for this character
            PixelFont font = GetPixelFont();
            bool[,] pattern = font.GetCharacterPattern(c);

            // Draw each pixel in the pattern
            for (int y = 0; y < pattern.GetLength(0); y++)
            {
                for (int x = 0; x < pattern.GetLength(1); x++)
                {
                    if (pattern[y, x])
                    {
                        // Apply interference effect
                        if (_interference > 0 && _random.NextDouble() < _interference * 0.2)
                        {
                            // Skip this pixel (creates static effect)
                            continue;
                        }

                        // Draw the pixel
                        DrawRect(new Rect2(
                            position.X + x,
                            position.Y + y,
                            1, 1),
                            TextColor);
                    }
                }
            }

            // Add noise pixels if interference is active
            if (_interference > 0)
            {
                int noisePixels = (int)(NoiseIntensity * _interference * CharacterSize * CharacterSize);

                for (int i = 0; i < noisePixels; i++)
                {
                    int noiseX = _random.Next(0, CharacterSize);
                    int noiseY = _random.Next(0, CharacterSize);

                    DrawRect(new Rect2(
                        position.X + noiseX,
                        position.Y + noiseY,
                        1, 1),
                        TextColor);
                }
            }
        }

        // Get the pixel font resource
        private static PixelFont GetPixelFont()
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

        // Draw ASCII art in the specified bounds
        private void DrawAsciiArt(Rect2 bounds)
        {
            if (_asciiArt.Count == 0) return;

            float x = bounds.Position.X;
            float y = bounds.Position.Y;
            float maxWidth = bounds.Size.X;
            float maxHeight = bounds.Size.Y;

            // Calculate line height based on character size
            float lineHeight = CharacterSize + LineSpacing;

            // Draw each line of ASCII art
            for (int i = 0; i < _asciiArt.Count; i++)
            {
                // Skip if we've exceeded the bounds
                if (y + lineHeight > bounds.Position.Y + maxHeight)
                    break;

                string line = _asciiArt[i];

                // Draw the line character by character
                float currentX = x;
                foreach (char c in line)
                {
                    // Skip if we've exceeded the width
                    if (currentX + CharacterSize > x + maxWidth)
                        break;

                    DrawPixelCharacter(c, new Vector2(currentX, y));
                    currentX += CharacterSize + 1;
                }

                // Move to next line
                y += lineHeight;
            }
        }

        // Draw scanlines effect
        private void DrawScanlines(float width, float height)
        {
            int scanlineSpacing = 4; // Space between scanlines

            for (int y = (int)_scanlineOffset % scanlineSpacing; y < height; y += scanlineSpacing)
            {
                DrawRect(new Rect2(0, y, width, 1), ScanlineColor);
            }
        }

        // Calculate cursor position at the end of text
        private Vector2 CalculateCursorPosition(string text, Rect2 bounds)
        {
            float x = bounds.Position.X;
            float y = bounds.Position.Y;
            float maxWidth = bounds.Size.X;

            // Split text into words
            string[] words = text.Split(' ');
            float currentX = x;
            float currentY = y;

            foreach (string word in words)
            {
                // Calculate word width
                float wordWidth = word.Length * (CharacterSize + 1);

                // Check if we need to wrap to next line
                if (currentX + wordWidth > x + maxWidth)
                {
                    currentX = x;
                    currentY += CharacterSize + LineSpacing;
                }

                // Move past this word
                currentX += wordWidth;

                // Add space after word
                currentX += CharacterSize;

                // Check if we've exceeded the bounds
                if (currentX > x + maxWidth)
                {
                    currentX = x;
                    currentY += CharacterSize + LineSpacing;
                }
            }

            // Return position for cursor
            return new Vector2(currentX, currentY + CharacterSize);
        }

        // Helper to draw a button
        private void DrawButton(Rect2 rect, string text, bool isHighlighted)
        {
            // Draw button background
            Color bgColor = isHighlighted ? ButtonHighlightColor : ButtonColor;
            DrawRect(rect, bgColor);

            // Draw button border
            DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1.0f), false, 1);

            // Draw button text
            DrawString(ThemeDB.FallbackFont, new Vector2(rect.Position.X + rect.Size.X / 2, rect.Position.Y + rect.Size.Y / 2 + 6),
                text, HorizontalAlignment.Center, -1, 14, ButtonTextColor);
        }

        // Handle input events
        public override void _GuiInput(InputEvent @event)
        {
            if (!_isVisible) return;

            // Handle mouse button events
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
                {
                    Vector2 mousePos = mouseButtonEvent.Position;

                    // Check if mouse is over any button
                    if (_closeButtonRect.HasPoint(mousePos))
                    {
                        // Close button clicked
                        _isVisible = false;
                        EmitSignal(SignalName.MessageClosed);

                        // Play interface sound
                        if (_interfaceSound != null && _interfaceSound.Stream != null)
                        {
                            _interfaceSound.Play();
                        }
                    }
                    else if (!_isDecoded && _decodeButtonRect.HasPoint(mousePos))
                    {
                        // Decode button clicked
                        EmitSignal(SignalName.DecodeRequested, _currentMessageId);

                        // Play interface sound
                        if (_interfaceSound != null && _interfaceSound.Stream != null)
                        {
                            _interfaceSound.Play();
                        }
                    }
                    else if (_isTyping)
                    {
                        // Skip typing animation if clicked elsewhere
                        _currentCharIndex = _content.Length;
                        _displayedText = _content;
                        _isTyping = false;
                    }
                }
            }
            // Handle keyboard events
            else if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Escape)
                {
                    // Close on Escape key
                    _isVisible = false;
                    EmitSignal(SignalName.MessageClosed);

                    // Play interface sound
                    if (_interfaceSound != null && _interfaceSound.Stream != null)
                    {
                        _interfaceSound.Play();
                    }
                }
                else if (keyEvent.Keycode == Key.Space || keyEvent.Keycode == Key.Enter)
                {
                    if (_isTyping)
                    {
                        // Skip typing animation
                        _currentCharIndex = _content.Length;
                        _displayedText = _content;
                        _isTyping = false;
                    }
                    else if (!_isDecoded)
                    {
                        // Decode if not already decoded
                        EmitSignal(SignalName.DecodeRequested, _currentMessageId);

                        // Play interface sound
                        if (_interfaceSound != null && _interfaceSound.Stream != null)
                        {
                            _interfaceSound.Play();
                        }
                    }
                }
            }
        }

        // Called when the control is resized
        public override void _Notification(int what)
        {
            base._Notification(what);

            if (what == NotificationResized)
            {
                UpdateInteractionAreas();
            }
        }
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.Text;

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
        public float NoiseIntensity { get; set; } = 0.2f;

        [Export]
        public float TypewriterSpeed { get; set; } = 0.05f;

        [Export]
        public int CharacterSize { get; set; } = 8;

        [Export]
        public int LineSpacing { get; set; } = 2;

        // Message content
        private string _title = "";
        private string _content = "";
        private bool _isDecoded = false;
        private float _interference = 0.0f;

        // Display state
        private bool _isVisible = false;
        private bool _isTyping = false;
        private int _currentCharIndex = 0;
        private string _displayedText = "";
        private float _typingTimer = 0.0f;
        private Random _random = new Random();

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
            if (_isTyping && _isVisible)
            {
                _typingTimer += (float)delta;

                if (_typingTimer >= TypewriterSpeed)
                {
                    _typingTimer = 0;

                    if (_currentCharIndex < _content.Length)
                    {
                        _currentCharIndex++;
                        _displayedText = _content.Substring(0, _currentCharIndex);

                        // Play typing sound
                        // TODO: Add typing sound effect
                    }
                    else
                    {
                        _isTyping = false;
                    }
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

            // Show the message
            _isVisible = true;
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

            // Draw background
            DrawRect(new Rect2(0, 0, width, height), BackgroundColor);

            // Draw border
            DrawRect(new Rect2(0, 0, width, height), BorderColor, false, 2);

            // Draw title
            float titleHeight = 40;
            DrawRect(new Rect2(0, 0, width, titleHeight), new Color(0.1f, 0.1f, 0.1f, 1.0f));
            DrawString(ThemeDB.FallbackFont, new Vector2(width / 2, titleHeight / 2 + 8),
                _title, HorizontalAlignment.Center, -1, 18, TitleColor);

            // Draw content area
            float contentY = titleHeight + 10;
            float contentHeight = height - contentY - 60; // Leave space for buttons

            // Draw the text content with typewriter effect
            if (!string.IsNullOrEmpty(_displayedText))
            {
                DrawPixelText(_displayedText, new Rect2(20, contentY, width - 40, contentHeight));
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
        private PixelFont GetPixelFont()
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
                    }
                    else if (!_isDecoded && _decodeButtonRect.HasPoint(mousePos))
                    {
                        // Decode button clicked
                        EmitSignal(SignalName.DecodeRequested, _currentMessageId);
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

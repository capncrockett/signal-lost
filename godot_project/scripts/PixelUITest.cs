using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelUITest : Control
    {
        // UI properties
        private Color _backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        private Color _textColor = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        private Color _buttonColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        private Color _buttonHighlightColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);

        // UI state
        private bool _showMessage = false;
        private string _message = "This is a pixel-based UI demo for Signal Lost. It demonstrates how we can create UI elements using Godot's drawing primitives instead of relying on image assets.\n\nThe benefits of this approach include:\n- Reduced file size\n- Better performance\n- More authentic retro feel\n- Easier modifications\n- Consistent visual style";

        // UI interaction areas
        private Rect2 _showMessageButtonRect;
        private Rect2 _messageRect;
        private Rect2 _closeButtonRect;

        // Button hover state
        private bool _showButtonHovered = false;
        private bool _closeButtonHovered = false;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            UpdateInteractionAreas();
            GD.Print("PixelUITest ready!");

            // Visualize the initial UI state
            UIVisualizer.VisualizeUI(this);
        }

        // Update the interaction areas based on current size
        private void UpdateInteractionAreas()
        {
            Vector2 size = Size;
            float width = size.X;
            float height = size.Y;

            // Calculate button dimensions
            float buttonWidth = 200;
            float buttonHeight = 50;

            // Set up button rects
            _showMessageButtonRect = new Rect2(
                width / 2 - buttonWidth / 2,
                height / 2 - buttonHeight / 2,
                buttonWidth,
                buttonHeight
            );

            // Set up message rect
            float messageWidth = width * 0.8f;
            float messageHeight = height * 0.6f;
            _messageRect = new Rect2(
                width / 2 - messageWidth / 2,
                height / 2 - messageHeight / 2,
                messageWidth,
                messageHeight
            );

            // Set up close button rect
            float closeButtonWidth = 100;
            float closeButtonHeight = 40;
            _closeButtonRect = new Rect2(
                _messageRect.Position.X + _messageRect.Size.X - closeButtonWidth - 20,
                _messageRect.Position.Y + _messageRect.Size.Y - closeButtonHeight - 20,
                closeButtonWidth,
                closeButtonHeight
            );
        }

        // Custom drawing function
        public override void _Draw()
        {
            // Draw background
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), _backgroundColor);

            if (_showMessage)
            {
                // Draw message box with border
                DrawRect(_messageRect, new Color(0.05f, 0.05f, 0.05f, 1.0f));

                // Draw border around message box
                DrawRect(_messageRect, _textColor, false, 2);

                // Draw title
                DrawString(ThemeDB.FallbackFont,
                    new Vector2(_messageRect.Position.X + _messageRect.Size.X / 2, _messageRect.Position.Y + 30),
                    "Pixel-Based UI Demo",
                    HorizontalAlignment.Center, -1, 24, _textColor);

                // Draw message text with better spacing
                float titleHeight = 40;
                DrawString(ThemeDB.FallbackFont,
                    new Vector2(_messageRect.Position.X + 20, _messageRect.Position.Y + titleHeight + 20),
                    _message,
                    HorizontalAlignment.Left, (int)(_messageRect.Size.X - 40), 16, _textColor);

                // Draw close button with border
                DrawRect(_closeButtonRect, _closeButtonHovered ? _buttonHighlightColor : _buttonColor);
                DrawRect(_closeButtonRect, _textColor, false, 1);
                DrawString(ThemeDB.FallbackFont,
                    new Vector2(_closeButtonRect.Position.X + _closeButtonRect.Size.X / 2, _closeButtonRect.Position.Y + _closeButtonRect.Size.Y / 2 + 6),
                    "Close",
                    HorizontalAlignment.Center, -1, 16, _textColor);
            }
            else
            {
                // Draw show message button with border
                DrawRect(_showMessageButtonRect, _showButtonHovered ? _buttonHighlightColor : _buttonColor);
                DrawRect(_showMessageButtonRect, _textColor, false, 1);
                DrawString(ThemeDB.FallbackFont,
                    new Vector2(_showMessageButtonRect.Position.X + _showMessageButtonRect.Size.X / 2, _showMessageButtonRect.Position.Y + _showMessageButtonRect.Size.Y / 2 + 6),
                    "Show Pixel UI Demo",
                    HorizontalAlignment.Center, -1, 16, _textColor);
            }
        }

        // Handle input events
        public override void _GuiInput(InputEvent @event)
        {
            // Handle mouse button events
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
                {
                    Vector2 mousePos = mouseButtonEvent.Position;

                    if (_showMessage)
                    {
                        // Check if close button clicked
                        if (_closeButtonRect.HasPoint(mousePos))
                        {
                            _showMessage = false;
                            QueueRedraw();

                            // Visualize UI after state change
                            UIVisualizer.VisualizeUI(this);
                        }
                    }
                    else
                    {
                        // Check if show message button clicked
                        if (_showMessageButtonRect.HasPoint(mousePos))
                        {
                            _showMessage = true;
                            QueueRedraw();

                            // Visualize UI after state change
                            UIVisualizer.VisualizeUI(this);
                        }
                    }
                }
            }

            // Handle mouse motion for button hover effects
            if (@event is InputEventMouseMotion mouseMotionEvent)
            {
                Vector2 mousePos = mouseMotionEvent.Position;
                bool redraw = false;

                // Update hover states
                if (_showMessage)
                {
                    bool newHoverState = _closeButtonRect.HasPoint(mousePos);
                    if (newHoverState != _closeButtonHovered)
                    {
                        _closeButtonHovered = newHoverState;
                        redraw = true;
                    }
                }
                else
                {
                    bool newHoverState = _showMessageButtonRect.HasPoint(mousePos);
                    if (newHoverState != _showButtonHovered)
                    {
                        _showButtonHovered = newHoverState;
                        redraw = true;
                    }
                }

                if (redraw)
                {
                    QueueRedraw();
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
                QueueRedraw();
            }
        }
    }
}

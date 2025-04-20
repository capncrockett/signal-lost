using Godot;
using System;

namespace SignalLost.UI
{
    /// <summary>
    /// A pixel-styled panel control.
    /// </summary>
    [GlobalClass]
    public partial class PixelPanel : PixelUIBase
    {
        [Export] public string Title { get; set; } = "";
        [Export] public bool ShowTitle { get; set; } = false;
        [Export] public bool ShowBorder { get; set; } = true;
        [Export] public bool ShowCloseButton { get; set; } = false;
        
        // Signals
        [Signal] public delegate void CloseRequestedEventHandler();
        
        // UI state
        private Rect2 _closeButtonRect;
        private bool _closeButtonHovered = false;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            base._Ready();
            
            // Set minimum size
            CustomMinimumSize = new Vector2(100, 100);
            
            // Update close button rect
            UpdateCloseButtonRect();
        }
        
        // Update close button rect when resized
        public override void _Notification(int what)
        {
            base._Notification(what);
            
            if (what == NotificationResized)
            {
                UpdateCloseButtonRect();
            }
        }
        
        // Update close button rect
        private void UpdateCloseButtonRect()
        {
            if (ShowCloseButton)
            {
                float buttonSize = 20;
                _closeButtonRect = new Rect2(
                    Size.X - buttonSize - 5,
                    5,
                    buttonSize,
                    buttonSize
                );
            }
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            base._Draw();
            
            // Draw panel background
            DrawThemedPanel(new Rect2(0, 0, Size.X, Size.Y));
            
            // Draw title if enabled
            if (ShowTitle && !string.IsNullOrEmpty(Title))
            {
                float titleHeight = 30;
                
                // Draw title background
                DrawRect(new Rect2(0, 0, Size.X, titleHeight), new Color(0.15f, 0.15f, 0.15f, 1.0f));
                
                // Draw title text
                DrawString(ThemeDB.FallbackFont, new Vector2(Size.X / 2, titleHeight / 2 + 6),
                    Title, HorizontalAlignment.Center, -1, 16, _theme.TextColor);
            }
            
            // Draw close button if enabled
            if (ShowCloseButton)
            {
                Color closeButtonColor = _closeButtonHovered ? _theme.AlertColor : _theme.BorderColor;
                
                // Draw X
                float padding = 5;
                DrawLine(
                    new Vector2(_closeButtonRect.Position.X + padding, _closeButtonRect.Position.Y + padding),
                    new Vector2(_closeButtonRect.Position.X + _closeButtonRect.Size.X - padding, _closeButtonRect.Position.Y + _closeButtonRect.Size.Y - padding),
                    closeButtonColor,
                    2
                );
                
                DrawLine(
                    new Vector2(_closeButtonRect.Position.X + _closeButtonRect.Size.X - padding, _closeButtonRect.Position.Y + padding),
                    new Vector2(_closeButtonRect.Position.X + padding, _closeButtonRect.Position.Y + _closeButtonRect.Size.Y - padding),
                    closeButtonColor,
                    2
                );
            }
        }
        
        // Process input events
        public override void _GuiInput(InputEvent @event)
        {
            if (ShowCloseButton)
            {
                if (@event is InputEventMouseMotion mouseMotionEvent)
                {
                    bool wasHovered = _closeButtonHovered;
                    _closeButtonHovered = _closeButtonRect.HasPoint(mouseMotionEvent.Position);
                    
                    if (wasHovered != _closeButtonHovered)
                    {
                        QueueRedraw();
                    }
                }
                else if (@event is InputEventMouseButton mouseButtonEvent)
                {
                    if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed)
                    {
                        if (_closeButtonRect.HasPoint(mouseButtonEvent.Position))
                        {
                            EmitSignal(SignalName.CloseRequested);
                        }
                    }
                }
            }
        }
    }
}

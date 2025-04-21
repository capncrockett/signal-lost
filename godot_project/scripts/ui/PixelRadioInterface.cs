using Godot;
using System;
using SignalLost.Audio;

namespace SignalLost.UI
{
    /// <summary>
    /// A pixel-based radio interface for the Signal Lost game.
    /// </summary>
    [GlobalClass]
    public partial class PixelRadioInterface : Control
    {
        // Configuration
        [Export] public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        [Export] public Color BorderColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        [Export] public Color TextColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        [Export] public Color DialColor { get; set; } = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        [Export] public Color DialMarkerColor { get; set; } = new Color(0.9f, 0.1f, 0.1f, 1.0f);
        [Export] public Color ButtonColor { get; set; } = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        [Export] public Color ButtonHighlightColor { get; set; } = new Color(0.7f, 0.7f, 0.7f, 1.0f);
        [Export] public Color SignalMeterColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        [Export] public Color SignalMeterBackgroundColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);

        // Radio state
        [Export] public float MinFrequency { get; set; } = 88.0f;
        [Export] public float MaxFrequency { get; set; } = 108.0f;
        [Export] public float CurrentFrequency { get; set; } = 98.0f;
        [Export] public float SignalStrength { get; set; } = 0.0f;
        [Export] public bool IsPoweredOn { get; set; } = false;

        // UI elements
        private Rect2 _tuningDialRect;
        private Rect2 _frequencyDisplayRect;
        private Rect2 _signalMeterRect;
        private Rect2 _powerButtonRect;
        private Rect2 _scanButtonRect;
        private Rect2 _messageButtonRect;

        // Interaction state
        private bool _isDraggingDial = false;
        private Vector2 _lastMousePosition;
        private float _dialRotation = 0.0f;
        private bool _powerButtonHovered = false;
        private bool _scanButtonHovered = false;
        private bool _messageButtonHovered = false;
        private bool _messageAvailable = false;

        // Audio
        private RadioAudioManager _audioManager;

        // Signals
        [Signal] public delegate void FrequencyChangedEventHandler(float frequency);
        [Signal] public delegate void PowerToggleEventHandler(bool isPoweredOn);
        [Signal] public delegate void ScanRequestedEventHandler();
        [Signal] public delegate void MessageRequestedEventHandler();

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Initialize UI elements
            UpdateUIRects();

            // Get reference to audio manager
            _audioManager = GetNode<RadioAudioManager>("/root/RadioAudioManager");

            // If audio manager doesn't exist, try to find it as a sibling
            if (_audioManager == null)
            {
                var parent = GetParent();
                if (parent != null)
                {
                    foreach (Node child in parent.GetChildren())
                    {
                        if (child is RadioAudioManager audioManager)
                        {
                            _audioManager = audioManager;
                            break;
                        }
                    }
                }
            }
        }

        // Update UI element rectangles based on current size
        private void UpdateUIRects()
        {
            Vector2 size = Size;
            float width = size.X;
            float height = size.Y;

            // Calculate radio panel dimensions
            float panelWidth = width * 0.9f;
            float panelHeight = height * 0.8f;
            float panelX = (width - panelWidth) / 2;
            float panelY = (height - panelHeight) / 2;

            // Tuning dial (circular)
            float dialSize = Mathf.Min(panelWidth, panelHeight) * 0.4f;
            float dialX = panelX + panelWidth * 0.25f - dialSize / 2;
            float dialY = panelY + panelHeight * 0.5f - dialSize / 2;
            _tuningDialRect = new Rect2(dialX, dialY, dialSize, dialSize);

            // Frequency display
            float displayWidth = panelWidth * 0.4f;
            float displayHeight = panelHeight * 0.15f;
            float displayX = panelX + panelWidth * 0.55f;
            float displayY = panelY + panelHeight * 0.3f;
            _frequencyDisplayRect = new Rect2(displayX, displayY, displayWidth, displayHeight);

            // Signal meter
            float meterWidth = panelWidth * 0.4f;
            float meterHeight = panelHeight * 0.1f;
            float meterX = panelX + panelWidth * 0.55f;
            float meterY = panelY + panelHeight * 0.5f;
            _signalMeterRect = new Rect2(meterX, meterY, meterWidth, meterHeight);

            // Power button
            float buttonSize = panelHeight * 0.15f;
            float powerX = panelX + panelWidth * 0.55f;
            float powerY = panelY + panelHeight * 0.7f;
            _powerButtonRect = new Rect2(powerX, powerY, buttonSize, buttonSize);

            // Scan button
            float scanX = panelX + panelWidth * 0.7f;
            float scanY = panelY + panelHeight * 0.7f;
            _scanButtonRect = new Rect2(scanX, scanY, buttonSize, buttonSize);

            // Message button
            float messageX = panelX + panelWidth * 0.85f;
            float messageY = panelY + panelHeight * 0.7f;
            _messageButtonRect = new Rect2(messageX, messageY, buttonSize, buttonSize);
        }

        // Handle input events
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    Vector2 mousePos = mouseButtonEvent.Position;

                    if (mouseButtonEvent.Pressed)
                    {
                        // Check if clicking on tuning dial
                        if (_tuningDialRect.HasPoint(mousePos))
                        {
                            _isDraggingDial = true;
                            _lastMousePosition = mousePos;

                            // Start tuning audio effect
                            if (_audioManager != null && IsPoweredOn)
                            {
                                _audioManager.StartTuning();
                                _audioManager.PlayDialTurn();
                            }
                        }
                        // Check if clicking on power button
                        else if (_powerButtonRect.HasPoint(mousePos))
                        {
                            IsPoweredOn = !IsPoweredOn;
                            EmitSignal(SignalName.PowerToggle, IsPoweredOn);

                            // Play power button sound
                            if (_audioManager != null)
                            {
                                _audioManager.OnRadioToggled(IsPoweredOn);
                            }

                            QueueRedraw();
                        }
                        // Check if clicking on scan button
                        else if (_scanButtonRect.HasPoint(mousePos) && IsPoweredOn)
                        {
                            EmitSignal(SignalName.ScanRequested);

                            // Play button click sound
                            if (_audioManager != null)
                            {
                                _audioManager.PlayButtonClick();
                            }

                            QueueRedraw();
                        }
                        // Check if clicking on message button
                        else if (_messageButtonRect.HasPoint(mousePos) && IsPoweredOn && _messageAvailable)
                        {
                            EmitSignal(SignalName.MessageRequested);

                            // Play button click sound
                            if (_audioManager != null)
                            {
                                _audioManager.PlayButtonClick();
                            }

                            QueueRedraw();
                        }
                    }
                    else
                    {
                        // Stop tuning when mouse button is released
                        if (_isDraggingDial && _audioManager != null)
                        {
                            _audioManager.StopTuning();
                        }

                        _isDraggingDial = false;
                    }
                }
            }
            else if (@event is InputEventMouseMotion mouseMotionEvent && _isDraggingDial && IsPoweredOn)
            {
                // Calculate dial rotation based on mouse movement
                Vector2 center = new Vector2(_tuningDialRect.Position.X + _tuningDialRect.Size.X / 2,
                                           _tuningDialRect.Position.Y + _tuningDialRect.Size.Y / 2);

                Vector2 prevDirection = (_lastMousePosition - center).Normalized();
                Vector2 newDirection = (mouseMotionEvent.Position - center).Normalized();

                // Calculate angle between previous and new direction
                float angle = Mathf.Atan2(prevDirection.Cross(newDirection), prevDirection.Dot(newDirection));

                // Update dial rotation
                _dialRotation += angle;
                _dialRotation = Mathf.Wrap(_dialRotation, 0, Mathf.Pi * 2);

                // Update frequency based on dial rotation
                float frequencyRange = MaxFrequency - MinFrequency;
                float normalizedRotation = _dialRotation / (Mathf.Pi * 2);
                CurrentFrequency = MinFrequency + normalizedRotation * frequencyRange;

                // Clamp frequency to valid range
                CurrentFrequency = Mathf.Clamp(CurrentFrequency, MinFrequency, MaxFrequency);

                // Emit signal
                EmitSignal(SignalName.FrequencyChanged, CurrentFrequency);

                // Update audio manager
                if (_audioManager != null)
                {
                    _audioManager.OnFrequencyChanged(CurrentFrequency);
                }

                // Update last mouse position
                _lastMousePosition = mouseMotionEvent.Position;

                // Redraw
                QueueRedraw();
            }
            else if (@event is InputEventMouseMotion mouseMotion)
            {
                // Update hover states
                Vector2 mousePos = mouseMotion.Position;
                _powerButtonHovered = _powerButtonRect.HasPoint(mousePos);
                _scanButtonHovered = _scanButtonRect.HasPoint(mousePos) && IsPoweredOn;
                _messageButtonHovered = _messageButtonRect.HasPoint(mousePos) && IsPoweredOn && _messageAvailable;

                // Redraw if hover state changed
                QueueRedraw();
            }
        }

        // Draw the radio interface
        public override void _Draw()
        {
            Vector2 size = Size;

            // Draw radio panel background
            float panelWidth = size.X * 0.9f;
            float panelHeight = size.Y * 0.8f;
            float panelX = (size.X - panelWidth) / 2;
            float panelY = (size.Y - panelHeight) / 2;

            DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight), BackgroundColor);
            DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight), BorderColor, false, 2);

            // Draw radio title
            DrawString(ThemeDB.FallbackFont, new Vector2(size.X / 2, panelY + 30),
                      "SIGNAL LOST RADIO", HorizontalAlignment.Center, -1, 20, TextColor);

            // Draw frequency display
            Color displayColor = IsPoweredOn ? TextColor : new Color(TextColor, 0.3f);
            DrawRect(_frequencyDisplayRect, new Color(0.0f, 0.0f, 0.0f, 1.0f));
            DrawRect(_frequencyDisplayRect, BorderColor, false, 1);

            string frequencyText = IsPoweredOn ? $"{CurrentFrequency:F1} MHz" : "-- . - MHz";
            DrawString(ThemeDB.FallbackFont,
                      new Vector2(_frequencyDisplayRect.Position.X + _frequencyDisplayRect.Size.X / 2,
                                 _frequencyDisplayRect.Position.Y + _frequencyDisplayRect.Size.Y / 2 + 8),
                      frequencyText, HorizontalAlignment.Center, -1, 18, displayColor);

            // Draw signal meter
            DrawRect(_signalMeterRect, SignalMeterBackgroundColor);
            DrawRect(_signalMeterRect, BorderColor, false, 1);

            if (IsPoweredOn)
            {
                float meterFill = _signalMeterRect.Size.X * SignalStrength;
                DrawRect(new Rect2(_signalMeterRect.Position.X, _signalMeterRect.Position.Y,
                                  meterFill, _signalMeterRect.Size.Y),
                        SignalMeterColor);
            }

            DrawString(ThemeDB.FallbackFont,
                      new Vector2(_signalMeterRect.Position.X + _signalMeterRect.Size.X / 2,
                                 _signalMeterRect.Position.Y - 10),
                      "SIGNAL", HorizontalAlignment.Center, -1, 14, TextColor);

            // Draw tuning dial
            Vector2 dialCenter = new Vector2(_tuningDialRect.Position.X + _tuningDialRect.Size.X / 2,
                                           _tuningDialRect.Position.Y + _tuningDialRect.Size.Y / 2);
            float dialRadius = _tuningDialRect.Size.X / 2;

            // Draw dial background
            DrawCircle(dialCenter, dialRadius, new Color(0.2f, 0.2f, 0.2f, 1.0f));
            DrawArc(dialCenter, dialRadius, 0, Mathf.Pi * 2, 32, BorderColor, 2);

            // Draw dial
            Color actualDialColor = IsPoweredOn ? DialColor : new Color(DialColor, 0.5f);
            DrawCircle(dialCenter, dialRadius * 0.8f, actualDialColor);

            // Draw dial marker
            if (IsPoweredOn)
            {
                Vector2 markerDirection = Vector2.Right.Rotated(_dialRotation);
                Vector2 markerStart = dialCenter + markerDirection * (dialRadius * 0.4f);
                Vector2 markerEnd = dialCenter + markerDirection * (dialRadius * 0.7f);
                DrawLine(markerStart, markerEnd, DialMarkerColor, 3);
            }

            // Draw frequency ticks around the dial
            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.Pi * 2 / 10;
                Vector2 tickDirection = Vector2.Right.Rotated(angle);
                Vector2 tickStart = dialCenter + tickDirection * (dialRadius * 0.8f);
                Vector2 tickEnd = dialCenter + tickDirection * dialRadius;
                DrawLine(tickStart, tickEnd, BorderColor, 1);

                // Draw frequency labels
                float freq = MinFrequency + (MaxFrequency - MinFrequency) * (i / 10.0f);
                Vector2 labelPos = dialCenter + tickDirection * (dialRadius * 1.2f);
                DrawString(ThemeDB.FallbackFont, labelPos, $"{freq:F0}", HorizontalAlignment.Center, -1, 12, TextColor);
            }

            DrawString(ThemeDB.FallbackFont,
                      new Vector2(dialCenter.X, _tuningDialRect.Position.Y - 10),
                      "TUNE", HorizontalAlignment.Center, -1, 14, TextColor);

            // Draw power button
            Color powerBgColor = _powerButtonHovered ? ButtonHighlightColor : ButtonColor;
            DrawCircle(new Vector2(_powerButtonRect.Position.X + _powerButtonRect.Size.X / 2,
                                  _powerButtonRect.Position.Y + _powerButtonRect.Size.Y / 2),
                      _powerButtonRect.Size.X / 2, powerBgColor);

            // Draw power symbol
            Vector2 powerCenter = new Vector2(_powerButtonRect.Position.X + _powerButtonRect.Size.X / 2,
                                            _powerButtonRect.Position.Y + _powerButtonRect.Size.Y / 2);
            float powerSymbolRadius = _powerButtonRect.Size.X * 0.3f;
            DrawArc(powerCenter, powerSymbolRadius, Mathf.Pi * 0.7f, Mathf.Pi * 2.3f, 16,
                   IsPoweredOn ? new Color(0.0f, 0.8f, 0.0f, 1.0f) : new Color(0.8f, 0.0f, 0.0f, 1.0f), 2);
            DrawLine(powerCenter, new Vector2(powerCenter.X, powerCenter.Y - powerSymbolRadius * 1.2f),
                    IsPoweredOn ? new Color(0.0f, 0.8f, 0.0f, 1.0f) : new Color(0.8f, 0.0f, 0.0f, 1.0f), 2);

            DrawString(ThemeDB.FallbackFont,
                      new Vector2(_powerButtonRect.Position.X + _powerButtonRect.Size.X / 2,
                                 _powerButtonRect.Position.Y - 10),
                      "POWER", HorizontalAlignment.Center, -1, 14, TextColor);

            // Draw scan button
            Color scanBgColor = _scanButtonHovered ? ButtonHighlightColor : ButtonColor;
            DrawRect(_scanButtonRect, scanBgColor);
            DrawRect(_scanButtonRect, BorderColor, false, 1);

            // Draw scan symbol (triangles)
            float triangleSize = _scanButtonRect.Size.X * 0.3f;
            Vector2 scanCenter = new Vector2(_scanButtonRect.Position.X + _scanButtonRect.Size.X / 2,
                                           _scanButtonRect.Position.Y + _scanButtonRect.Size.Y / 2);

            Vector2[] leftTriangle = new Vector2[]
            {
                new Vector2(scanCenter.X - triangleSize, scanCenter.Y),
                new Vector2(scanCenter.X - triangleSize / 2, scanCenter.Y - triangleSize / 2),
                new Vector2(scanCenter.X - triangleSize / 2, scanCenter.Y + triangleSize / 2)
            };

            Vector2[] rightTriangle = new Vector2[]
            {
                new Vector2(scanCenter.X + triangleSize, scanCenter.Y),
                new Vector2(scanCenter.X + triangleSize / 2, scanCenter.Y - triangleSize / 2),
                new Vector2(scanCenter.X + triangleSize / 2, scanCenter.Y + triangleSize / 2)
            };

            Color scanSymbolColor = IsPoweredOn ? TextColor : new Color(TextColor, 0.3f);
            DrawPolygon(leftTriangle, new Color[] { scanSymbolColor });
            DrawPolygon(rightTriangle, new Color[] { scanSymbolColor });

            DrawString(ThemeDB.FallbackFont,
                      new Vector2(_scanButtonRect.Position.X + _scanButtonRect.Size.X / 2,
                                 _scanButtonRect.Position.Y - 10),
                      "SCAN", HorizontalAlignment.Center, -1, 14, TextColor);

            // Draw message button
            Color messageBgColor = _messageButtonHovered ? ButtonHighlightColor : ButtonColor;
            DrawRect(_messageButtonRect, messageBgColor);
            DrawRect(_messageButtonRect, BorderColor, false, 1);

            // Draw message symbol (envelope)
            Vector2 msgCenter = new Vector2(_messageButtonRect.Position.X + _messageButtonRect.Size.X / 2,
                                          _messageButtonRect.Position.Y + _messageButtonRect.Size.Y / 2);
            float envelopeWidth = _messageButtonRect.Size.X * 0.6f;
            float envelopeHeight = _messageButtonRect.Size.Y * 0.4f;

            Vector2 envelopeTopLeft = new Vector2(msgCenter.X - envelopeWidth / 2, msgCenter.Y - envelopeHeight / 2);
            Vector2 envelopeTopRight = new Vector2(msgCenter.X + envelopeWidth / 2, msgCenter.Y - envelopeHeight / 2);
            Vector2 envelopeBottomLeft = new Vector2(msgCenter.X - envelopeWidth / 2, msgCenter.Y + envelopeHeight / 2);
            Vector2 envelopeBottomRight = new Vector2(msgCenter.X + envelopeWidth / 2, msgCenter.Y + envelopeHeight / 2);

            Color messageSymbolColor = IsPoweredOn && _messageAvailable ? TextColor : new Color(TextColor, 0.3f);

            // Draw envelope outline
            DrawLine(envelopeTopLeft, envelopeTopRight, messageSymbolColor, 1);
            DrawLine(envelopeTopRight, envelopeBottomRight, messageSymbolColor, 1);
            DrawLine(envelopeBottomRight, envelopeBottomLeft, messageSymbolColor, 1);
            DrawLine(envelopeBottomLeft, envelopeTopLeft, messageSymbolColor, 1);

            // Draw envelope diagonals
            DrawLine(envelopeTopLeft, envelopeBottomRight, messageSymbolColor, 1);
            DrawLine(envelopeTopRight, envelopeBottomLeft, messageSymbolColor, 1);

            DrawString(ThemeDB.FallbackFont,
                      new Vector2(_messageButtonRect.Position.X + _messageButtonRect.Size.X / 2,
                                 _messageButtonRect.Position.Y - 10),
                      "MSG", HorizontalAlignment.Center, -1, 14, TextColor);
        }

        // Set the signal strength (0.0 to 1.0)
        public void SetSignalStrength(float strength)
        {
            SignalStrength = Mathf.Clamp(strength, 0.0f, 1.0f);
            QueueRedraw();
        }

        // Set whether a message is available
        public void SetMessageAvailable(bool available)
        {
            _messageAvailable = available;
            QueueRedraw();
        }

        // Set the current frequency
        public void SetFrequency(float frequency)
        {
            CurrentFrequency = Mathf.Clamp(frequency, MinFrequency, MaxFrequency);

            // Update dial rotation based on frequency
            float frequencyRange = MaxFrequency - MinFrequency;
            float normalizedFrequency = (CurrentFrequency - MinFrequency) / frequencyRange;
            _dialRotation = normalizedFrequency * Mathf.Pi * 2;

            // Update audio manager
            if (_audioManager != null)
            {
                _audioManager.OnFrequencyChanged(CurrentFrequency);
            }

            QueueRedraw();
        }

        // Handle resize
        public override void _Notification(int what)
        {
            if (what == NotificationResized)
            {
                UpdateUIRects();
                QueueRedraw();
            }
        }
    }
}

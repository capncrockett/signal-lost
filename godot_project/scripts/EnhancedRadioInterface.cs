using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class EnhancedRadioInterface : Control
    {
        // UI properties
        [Export]
        public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        
        [Export]
        public Color PanelColor { get; set; } = new Color(0.15f, 0.15f, 0.15f, 1.0f);
        
        [Export]
        public Color TextColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        
        [Export]
        public Color BorderColor { get; set; } = new Color(0.0f, 0.6f, 0.0f, 1.0f);
        
        // References to child nodes
        private StaticNoiseVisualizer _staticVisualizer;
        private MorseCodeVisualizer _morseVisualizer;
        private FrequencyScannerVisualizer _scannerVisualizer;
        
        // Reference to parent RadioTuner
        private RadioTuner _radioTuner;
        
        // UI interaction areas
        private Rect2 _tuningKnobRect;
        private Rect2 _powerButtonRect;
        private Rect2 _scanButtonRect;
        private Rect2 _frequencyDisplayRect;
        private Rect2 _signalMeterRect;
        
        // UI state
        private bool _isPowerOn = false;
        private bool _isScanning = false;
        private float _currentFrequency = 100.0f;
        private float _signalStrength = 0.0f;
        private bool _isDraggingKnob = false;
        private Vector2 _lastMousePosition;
        
        // Button hover state
        private bool _powerButtonHovered = false;
        private bool _scanButtonHovered = false;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to child nodes
            _staticVisualizer = GetNode<StaticNoiseVisualizer>("StaticNoiseVisualizer");
            _morseVisualizer = GetNode<MorseCodeVisualizer>("MorseCodeVisualizer");
            _scannerVisualizer = GetNode<FrequencyScannerVisualizer>("FrequencyScannerVisualizer");
            
            // Get reference to parent RadioTuner
            _radioTuner = GetParent<RadioTuner>();
            
            if (_radioTuner != null)
            {
                // Initialize state from RadioTuner
                _currentFrequency = _radioTuner.GetCurrentFrequency();
                _isPowerOn = _radioTuner.IsPowerOn();
                _isScanning = _radioTuner.IsScanning();
                
                // Set up visualizers
                if (_scannerVisualizer != null)
                {
                    _scannerVisualizer.MinFrequency = _radioTuner.MinFrequency;
                    _scannerVisualizer.MaxFrequency = _radioTuner.MaxFrequency;
                    _scannerVisualizer.SetFrequency(_currentFrequency);
                }
                
                if (_morseVisualizer != null)
                {
                    _morseVisualizer.SetMessage("SOS TEST");
                    _morseVisualizer.SetPlaying(_isPowerOn);
                }
            }
            
            // Set up UI interaction areas
            UpdateInteractionAreas();
            
            GD.Print("EnhancedRadioInterface ready!");
            
            // Visualize the initial UI state
            UIVisualizer.VisualizeUI(this);
        }
        
        // Update the interaction areas based on current size
        private void UpdateInteractionAreas()
        {
            Vector2 size = Size;
            float width = size.X;
            float height = size.Y;
            
            // Calculate panel dimensions
            float panelWidth = Mathf.Min(width - 40, 600);
            float panelHeight = Mathf.Min(height - 40, 400);
            float panelX = (width - panelWidth) / 2;
            float panelY = (height - panelHeight) / 2;
            
            // Set up tuning knob rect
            float knobSize = 80;
            _tuningKnobRect = new Rect2(
                panelX + panelWidth * 0.75f - knobSize / 2,
                panelY + panelHeight * 0.7f - knobSize / 2,
                knobSize,
                knobSize
            );
            
            // Set up button dimensions
            float buttonWidth = panelWidth * 0.2f;
            float buttonHeight = 40;
            
            // Set up power button rect
            _powerButtonRect = new Rect2(
                panelX + panelWidth * 0.25f - buttonWidth / 2,
                panelY + panelHeight * 0.7f - buttonHeight / 2,
                buttonWidth,
                buttonHeight
            );
            
            // Set up scan button rect
            _scanButtonRect = new Rect2(
                panelX + panelWidth * 0.5f - buttonWidth / 2,
                panelY + panelHeight * 0.7f - buttonHeight / 2,
                buttonWidth,
                buttonHeight
            );
            
            // Set up frequency display rect
            float displayWidth = panelWidth * 0.4f;
            float displayHeight = 40;
            _frequencyDisplayRect = new Rect2(
                panelX + panelWidth * 0.5f - displayWidth / 2,
                panelY + 20,
                displayWidth,
                displayHeight
            );
            
            // Set up signal meter rect
            float meterWidth = panelWidth * 0.8f;
            float meterHeight = 20;
            _signalMeterRect = new Rect2(
                panelX + panelWidth * 0.5f - meterWidth / 2,
                panelY + panelHeight - meterHeight - 20,
                meterWidth,
                meterHeight
            );
            
            // Update visualizer sizes
            if (_staticVisualizer != null)
            {
                _staticVisualizer.Position = new Vector2(panelX + 20, panelY + 80);
                _staticVisualizer.Size = new Vector2(panelWidth * 0.4f, panelHeight * 0.4f);
            }
            
            if (_morseVisualizer != null)
            {
                _morseVisualizer.Position = new Vector2(panelX + panelWidth * 0.6f, panelY + 80);
                _morseVisualizer.Size = new Vector2(panelWidth * 0.35f, panelHeight * 0.2f);
            }
            
            if (_scannerVisualizer != null)
            {
                _scannerVisualizer.Position = new Vector2(panelX + 20, panelY + panelHeight * 0.3f);
                _scannerVisualizer.Size = new Vector2(panelWidth - 40, panelHeight * 0.3f);
            }
        }
        
        // Process function called every frame
        public override void _Process(double delta)
        {
            if (_radioTuner != null)
            {
                // Update state from RadioTuner
                _currentFrequency = _radioTuner.GetCurrentFrequency();
                _isPowerOn = _radioTuner.IsPowerOn();
                _isScanning = _radioTuner.IsScanning();
                _signalStrength = _radioTuner.GetSignalStrength();
                
                // Update visualizers
                if (_staticVisualizer != null)
                {
                    _staticVisualizer.SetSignalStrength(_signalStrength);
                    _staticVisualizer.Visible = _isPowerOn;
                }
                
                if (_morseVisualizer != null)
                {
                    _morseVisualizer.SetPlaying(_isPowerOn && _signalStrength > 0.5f);
                    _morseVisualizer.Visible = _isPowerOn;
                }
                
                if (_scannerVisualizer != null)
                {
                    _scannerVisualizer.SetFrequency(_currentFrequency);
                    _scannerVisualizer.SetScanning(_isScanning);
                    _scannerVisualizer.Visible = _isPowerOn;
                }
            }
            
            QueueRedraw();
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            Vector2 size = Size;
            float width = size.X;
            float height = size.Y;
            
            // Calculate panel dimensions
            float panelWidth = Mathf.Min(width - 40, 600);
            float panelHeight = Mathf.Min(height - 40, 400);
            float panelX = (width - panelWidth) / 2;
            float panelY = (height - panelHeight) / 2;
            
            // Draw background
            DrawRect(new Rect2(0, 0, width, height), BackgroundColor);
            
            // Draw radio panel
            DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight), PanelColor);
            DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight), BorderColor, false, 2);
            
            // Draw frequency display
            DrawFrequencyDisplay();
            
            // Draw tuning knob
            DrawTuningKnob();
            
            // Draw buttons
            DrawButton(_powerButtonRect, "POWER", _isPowerOn, _powerButtonHovered);
            DrawButton(_scanButtonRect, "SCAN", _isScanning, _scanButtonHovered);
            
            // Draw signal meter
            DrawSignalMeter();
        }
        
        // Draw the frequency display
        private void DrawFrequencyDisplay()
        {
            // Draw display background
            DrawRect(_frequencyDisplayRect, new Color(0.05f, 0.05f, 0.05f, 1.0f));
            
            // Draw display border
            DrawRect(_frequencyDisplayRect, BorderColor, false, 1);
            
            // Draw frequency text
            string frequencyText = $"{_currentFrequency:F1} MHz";
            DrawString(ThemeDB.FallbackFont, 
                new Vector2(_frequencyDisplayRect.Position.X + _frequencyDisplayRect.Size.X / 2, 
                            _frequencyDisplayRect.Position.Y + _frequencyDisplayRect.Size.Y / 2 + 6),
                frequencyText, 
                HorizontalAlignment.Center, -1, 20, 
                _isPowerOn ? TextColor : new Color(0.3f, 0.3f, 0.3f, 1.0f));
        }
        
        // Draw the tuning knob
        private void DrawTuningKnob()
        {
            float knobX = _tuningKnobRect.Position.X;
            float knobY = _tuningKnobRect.Position.Y;
            float knobSize = _tuningKnobRect.Size.X;
            float knobRadius = knobSize / 2;
            Vector2 knobCenter = new Vector2(knobX + knobRadius, knobY + knobRadius);
            
            // Draw knob background
            DrawCircle(knobCenter, knobRadius, new Color(0.2f, 0.2f, 0.2f, 1.0f));
            
            // Draw knob border
            DrawArc(knobCenter, knobRadius, 0, Mathf.Pi * 2, 32, BorderColor, 2);
            
            // Draw knob indicator
            if (_isPowerOn)
            {
                // Calculate rotation based on frequency
                float frequencyRange = _radioTuner != null ? 
                    (_radioTuner.MaxFrequency - _radioTuner.MinFrequency) : 20.0f;
                float minFrequency = _radioTuner != null ? _radioTuner.MinFrequency : 88.0f;
                float normalizedFrequency = (_currentFrequency - minFrequency) / frequencyRange;
                float rotation = normalizedFrequency * Mathf.Pi * 1.5f - Mathf.Pi * 0.75f;
                
                Vector2 indicatorEnd = knobCenter + new Vector2(
                    Mathf.Cos(rotation) * (knobRadius - 5),
                    Mathf.Sin(rotation) * (knobRadius - 5)
                );
                
                DrawLine(knobCenter, indicatorEnd, TextColor, 3);
            }
            
            // Draw knob center
            DrawCircle(knobCenter, 5, new Color(0.1f, 0.1f, 0.1f, 1.0f));
            
            // Draw frequency markers around the knob
            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.Pi * 2 / 10 - Mathf.Pi * 0.75f;
                Vector2 markerStart = knobCenter + new Vector2(
                    Mathf.Cos(angle) * (knobRadius - 2),
                    Mathf.Sin(angle) * (knobRadius - 2)
                );
                Vector2 markerEnd = knobCenter + new Vector2(
                    Mathf.Cos(angle) * (knobRadius - 10),
                    Mathf.Sin(angle) * (knobRadius - 10)
                );
                
                DrawLine(markerStart, markerEnd, new Color(0.4f, 0.4f, 0.4f, 1.0f), 1);
            }
        }
        
        // Draw a button
        private void DrawButton(Rect2 rect, string text, bool isActive, bool isHovered)
        {
            // Determine button color based on state
            Color bgColor;
            if (isActive)
            {
                bgColor = new Color(0.0f, 0.5f, 0.0f, 1.0f);
            }
            else if (isHovered)
            {
                bgColor = new Color(0.25f, 0.25f, 0.25f, 1.0f);
            }
            else
            {
                bgColor = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            }
            
            // Draw button background
            DrawRect(rect, bgColor);
            
            // Draw button border
            DrawRect(rect, BorderColor, false, 1);
            
            // Draw button text
            DrawString(ThemeDB.FallbackFont, 
                new Vector2(rect.Position.X + rect.Size.X / 2, rect.Position.Y + rect.Size.Y / 2 + 6),
                text, 
                HorizontalAlignment.Center, -1, 16, TextColor);
        }
        
        // Draw the signal meter
        private void DrawSignalMeter()
        {
            // Draw meter background
            DrawRect(_signalMeterRect, new Color(0.05f, 0.05f, 0.05f, 1.0f));
            
            // Draw meter border
            DrawRect(_signalMeterRect, BorderColor, false, 1);
            
            // Draw meter fill based on signal strength
            if (_isPowerOn && _signalStrength > 0)
            {
                Rect2 fillRect = new Rect2(
                    _signalMeterRect.Position.X + 2,
                    _signalMeterRect.Position.Y + 2,
                    (_signalMeterRect.Size.X - 4) * _signalStrength,
                    _signalMeterRect.Size.Y - 4
                );
                
                DrawRect(fillRect, TextColor);
            }
            
            // Draw meter segments
            int segments = 10;
            float segmentWidth = (_signalMeterRect.Size.X - 4) / segments;
            
            for (int i = 1; i < segments; i++)
            {
                float x = _signalMeterRect.Position.X + 2 + i * segmentWidth;
                DrawLine(
                    new Vector2(x, _signalMeterRect.Position.Y + 2),
                    new Vector2(x, _signalMeterRect.Position.Y + _signalMeterRect.Size.Y - 2),
                    new Color(0.0f, 0.0f, 0.0f, 0.5f),
                    1
                );
            }
            
            // Draw meter label
            DrawString(ThemeDB.FallbackFont, 
                new Vector2(_signalMeterRect.Position.X + _signalMeterRect.Size.X / 2, 
                            _signalMeterRect.Position.Y - 10),
                "SIGNAL", 
                HorizontalAlignment.Center, -1, 12, TextColor);
        }
        
        // Handle input events
        public override void _GuiInput(InputEvent @event)
        {
            // Handle mouse button events
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    Vector2 mousePos = mouseButtonEvent.Position;
                    
                    if (mouseButtonEvent.Pressed)
                    {
                        // Check if power button clicked
                        if (_powerButtonRect.HasPoint(mousePos))
                        {
                            if (_radioTuner != null)
                            {
                                _radioTuner.TogglePower();
                            }
                            UIVisualizer.VisualizeUI(this);
                        }
                        // Check if scan button clicked
                        else if (_scanButtonRect.HasPoint(mousePos))
                        {
                            if (_radioTuner != null)
                            {
                                _radioTuner.ToggleScanning();
                            }
                            UIVisualizer.VisualizeUI(this);
                        }
                        // Check if tuning knob clicked
                        else if (_tuningKnobRect.HasPoint(mousePos))
                        {
                            _isDraggingKnob = true;
                            _lastMousePosition = mousePos;
                        }
                    }
                    else // Button released
                    {
                        _isDraggingKnob = false;
                    }
                }
            }
            // Handle mouse motion events
            else if (@event is InputEventMouseMotion mouseMotionEvent)
            {
                Vector2 mousePos = mouseMotionEvent.Position;
                bool redraw = false;
                
                // Update hover states
                bool newPowerHoverState = _powerButtonRect.HasPoint(mousePos);
                if (newPowerHoverState != _powerButtonHovered)
                {
                    _powerButtonHovered = newPowerHoverState;
                    redraw = true;
                }
                
                bool newScanHoverState = _scanButtonRect.HasPoint(mousePos);
                if (newScanHoverState != _scanButtonHovered)
                {
                    _scanButtonHovered = newScanHoverState;
                    redraw = true;
                }
                
                // Handle knob dragging
                if (_isDraggingKnob)
                {
                    Vector2 knobCenter = new Vector2(
                        _tuningKnobRect.Position.X + _tuningKnobRect.Size.X / 2,
                        _tuningKnobRect.Position.Y + _tuningKnobRect.Size.Y / 2
                    );
                    
                    Vector2 prevDir = (_lastMousePosition - knobCenter).Normalized();
                    Vector2 newDir = (mousePos - knobCenter).Normalized();
                    
                    float angle = Mathf.Atan2(
                        prevDir.X * newDir.Y - prevDir.Y * newDir.X,
                        prevDir.X * newDir.X + prevDir.Y * newDir.Y
                    );
                    
                    if (_radioTuner != null)
                    {
                        float frequencyRange = _radioTuner.MaxFrequency - _radioTuner.MinFrequency;
                        float frequencyChange = (angle / (Mathf.Pi * 2)) * frequencyRange * 0.5f;
                        _radioTuner.ChangeFrequency(frequencyChange);
                    }
                    
                    _lastMousePosition = mousePos;
                    redraw = true;
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

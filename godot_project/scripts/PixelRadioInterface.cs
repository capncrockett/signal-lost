using Godot;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelRadioInterface : Control
    {
        // Radio interface properties
        [Export]
        public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);

        [Export]
        public Color PanelColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);

        [Export]
        public Color DisplayColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);

        [Export]
        public Color KnobColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 1.0f);

        [Export]
        public Color KnobHighlightColor { get; set; } = new Color(0.4f, 0.4f, 0.4f, 1.0f);

        [Export]
        public Color KnobIndicatorColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);

        [Export]
        public Color ButtonColor { get; set; } = new Color(0.25f, 0.25f, 0.25f, 1.0f);

        [Export]
        public Color ButtonHighlightColor { get; set; } = new Color(0.35f, 0.35f, 0.35f, 1.0f);

        [Export]
        public Color ButtonTextColor { get; set; } = new Color(0.9f, 0.9f, 0.9f, 1.0f);

        [Export]
        public Color SignalMeterColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);

        [Export]
        public Color SignalMeterBgColor { get; set; } = new Color(0.15f, 0.15f, 0.15f, 1.0f);

        // References to game systems
        private RadioSystem _radioSystem;
        private GameState _gameState;

        // Local state
        private float _knobRotation = 0.0f;
        private float _signalStrength = 0.0f;
        private bool _isPowerOn = false;
        private bool _isScanning = false;
        private float _currentFrequency = 90.0f;
        private float _minFrequency = 88.0f;
        private float _maxFrequency = 108.0f;

        // UI interaction areas
        private Rect2 _powerButtonRect;
        private Rect2 _scanButtonRect;
        private Rect2 _tuneDownButtonRect;
        private Rect2 _tuneUpButtonRect;
        private Rect2 _knobRect;
        private Rect2 _frequencySliderRect;

        // Mouse interaction state
        private bool _isDraggingKnob = false;
        private bool _isDraggingSlider = false;
        private Vector2 _lastMousePosition;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to game systems
            _radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_radioSystem != null)
            {
                _minFrequency = _radioSystem.MinFrequency;
                _maxFrequency = _radioSystem.MaxFrequency;
                _currentFrequency = (_minFrequency + _maxFrequency) / 2.0f;
            }

            if (_gameState == null || _radioSystem == null)
            {
                GD.PrintErr("PixelRadioInterface: Failed to get references to game systems");
            }

            // Set up UI interaction areas
            UpdateInteractionAreas();
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

            // Calculate button dimensions
            float buttonWidth = panelWidth / 4 - 10;
            float buttonHeight = 40;
            float buttonY = panelY + panelHeight - buttonHeight - 20;
            float buttonSpacing = 10;

            // Set up button rects
            _powerButtonRect = new Rect2(
                panelX + 10,
                buttonY,
                buttonWidth,
                buttonHeight
            );

            _scanButtonRect = new Rect2(
                panelX + 10 + buttonWidth + buttonSpacing,
                buttonY,
                buttonWidth,
                buttonHeight
            );

            _tuneDownButtonRect = new Rect2(
                panelX + 10 + (buttonWidth + buttonSpacing) * 2,
                buttonY,
                buttonWidth,
                buttonHeight
            );

            _tuneUpButtonRect = new Rect2(
                panelX + 10 + (buttonWidth + buttonSpacing) * 3,
                buttonY,
                buttonWidth,
                buttonHeight
            );

            // Set up knob rect
            float knobSize = 80;
            _knobRect = new Rect2(
                panelX + panelWidth / 2 - knobSize / 2,
                panelY + panelHeight / 2 - knobSize / 2,
                knobSize,
                knobSize
            );

            // Set up frequency slider rect
            float sliderWidth = panelWidth - 40;
            float sliderHeight = 20;
            _frequencySliderRect = new Rect2(
                panelX + 20,
                panelY + panelHeight / 2 + knobSize / 2 + 20,
                sliderWidth,
                sliderHeight
            );
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            // Update state based on game systems
            if (_radioSystem != null && _gameState != null)
            {
                _currentFrequency = _gameState.CurrentFrequency;
                _isPowerOn = _gameState.IsRadioOn;
                _isScanning = false; // TODO: Add scanning state to RadioSystem
                _signalStrength = _radioSystem.GetSignalStrength();
            }

            // Calculate knob rotation based on frequency
            float frequencyRange = _maxFrequency - _minFrequency;
            float frequencyPercentage = (_currentFrequency - _minFrequency) / frequencyRange;
            _knobRotation = frequencyPercentage * 270.0f; // 270 degrees of rotation

            // Redraw
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

            // Draw frequency display
            float displayWidth = panelWidth - 40;
            float displayHeight = 50;
            float displayX = panelX + 20;
            float displayY = panelY + 20;

            DrawRect(new Rect2(displayX, displayY, displayWidth, displayHeight), new Color(0.0f, 0.0f, 0.0f, 1.0f));

            // Draw frequency text
            string frequencyText = $"{_currentFrequency:F1} MHz";
            DrawString(ThemeDB.FallbackFont, new Vector2(displayX + displayWidth / 2, displayY + displayHeight / 2 + 8),
                frequencyText, HorizontalAlignment.Center, -1, 24, _isPowerOn ? DisplayColor : new Color(0.2f, 0.2f, 0.2f, 1.0f));

            // Draw tuning knob
            DrawTuningKnob();

            // Draw frequency slider
            DrawFrequencySlider();

            // Draw buttons
            DrawButtons();

            // Draw signal strength meter
            DrawSignalStrengthMeter();

            // Draw visualization area
            DrawVisualizationArea();
        }

        // Draw the tuning knob
        private void DrawTuningKnob()
        {
            float knobX = _knobRect.Position.X;
            float knobY = _knobRect.Position.Y;
            float knobSize = _knobRect.Size.X;
            float knobRadius = knobSize / 2;
            Vector2 knobCenter = new Vector2(knobX + knobRadius, knobY + knobRadius);

            // Draw knob background
            DrawCircle(knobCenter, knobRadius, KnobColor);

            // Draw knob highlight
            DrawArc(knobCenter, knobRadius - 5, 0, Mathf.Pi * 2, 32, KnobHighlightColor, 2);

            // Draw knob indicator
            if (_isPowerOn)
            {
                float indicatorAngle = Mathf.DegToRad(_knobRotation - 135); // -135 degrees offset to start at 7 o'clock position
                Vector2 indicatorEnd = knobCenter + new Vector2(
                    Mathf.Cos(indicatorAngle) * (knobRadius - 10),
                    Mathf.Sin(indicatorAngle) * (knobRadius - 10)
                );

                DrawLine(knobCenter, indicatorEnd, KnobIndicatorColor, 3);
            }

            // Draw center dot
            DrawCircle(knobCenter, 5, new Color(0.15f, 0.15f, 0.15f, 1.0f));
        }

        // Draw the frequency slider
        private void DrawFrequencySlider()
        {
            float sliderX = _frequencySliderRect.Position.X;
            float sliderY = _frequencySliderRect.Position.Y;
            float sliderWidth = _frequencySliderRect.Size.X;
            float sliderHeight = _frequencySliderRect.Size.Y;

            // Draw slider background
            DrawRect(_frequencySliderRect, new Color(0.15f, 0.15f, 0.15f, 1.0f));

            // Draw slider handle
            float frequencyRange = _maxFrequency - _minFrequency;
            float frequencyPercentage = (_currentFrequency - _minFrequency) / frequencyRange;
            float handleX = sliderX + frequencyPercentage * sliderWidth;
            float handleWidth = 10;

            DrawRect(new Rect2(handleX - handleWidth / 2, sliderY - 5, handleWidth, sliderHeight + 10),
                _isPowerOn ? KnobIndicatorColor : new Color(0.3f, 0.3f, 0.3f, 1.0f));

            // Draw frequency markers
            for (float freq = _minFrequency; freq <= _maxFrequency; freq += 2.0f)
            {
                float markerPercentage = (freq - _minFrequency) / frequencyRange;
                float markerX = sliderX + markerPercentage * sliderWidth;
                float markerHeight = (freq % 10.0f < 0.1f) ? 10 : 5; // Taller markers for multiples of 10

                DrawLine(new Vector2(markerX, sliderY), new Vector2(markerX, sliderY + markerHeight),
                    new Color(0.5f, 0.5f, 0.5f, 1.0f), 1);

                // Draw frequency labels for multiples of 10
                if (freq % 10.0f < 0.1f)
                {
                    DrawString(ThemeDB.FallbackFont, new Vector2(markerX, sliderY + markerHeight + 15),
                        $"{freq:F0}", HorizontalAlignment.Center, -1, 12, new Color(0.7f, 0.7f, 0.7f, 1.0f));
                }
            }
        }

        // Draw the buttons
        private void DrawButtons()
        {
            // Draw power button
            DrawButton(_powerButtonRect, _isPowerOn ? "ON" : "OFF", _isPowerOn);

            // Draw scan button
            DrawButton(_scanButtonRect, _isScanning ? "Stop" : "Scan", _isScanning && _isPowerOn);

            // Draw tune down button
            DrawButton(_tuneDownButtonRect, "<", false);

            // Draw tune up button
            DrawButton(_tuneUpButtonRect, ">", false);
        }

        // Helper to draw a button
        private void DrawButton(Rect2 rect, string text, bool isActive)
        {
            // Draw button background
            Color bgColor = isActive ? ButtonHighlightColor : ButtonColor;
            DrawRect(rect, bgColor);

            // Draw button border
            DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 1.0f), false, 2);

            // Draw button text
            DrawString(ThemeDB.FallbackFont, new Vector2(rect.Position.X + rect.Size.X / 2, rect.Position.Y + rect.Size.Y / 2 + 6),
                text, HorizontalAlignment.Center, -1, 16, ButtonTextColor);
        }

        // Draw the signal strength meter
        private void DrawSignalStrengthMeter()
        {
            Vector2 size = Size;
            float width = size.X;
            float height = size.Y;

            // Calculate panel dimensions
            float panelWidth = Mathf.Min(width - 40, 600);
            float panelHeight = Mathf.Min(height - 40, 400);
            float panelX = (width - panelWidth) / 2;
            float panelY = (height - panelHeight) / 2;

            // Calculate meter dimensions
            float meterWidth = panelWidth - 40;
            float meterHeight = 20;
            float meterX = panelX + 20;
            float meterY = panelY + panelHeight - meterHeight - 80;

            // Draw meter label
            DrawString(ThemeDB.FallbackFont, new Vector2(meterX + meterWidth / 2, meterY - 10),
                "Signal Strength", HorizontalAlignment.Center, -1, 16, new Color(0.8f, 0.8f, 0.8f, 1.0f));

            // Draw meter background
            DrawRect(new Rect2(meterX, meterY, meterWidth, meterHeight), SignalMeterBgColor);

            // Draw meter fill
            if (_isPowerOn)
            {
                float fillWidth = meterWidth * _signalStrength;
                DrawRect(new Rect2(meterX, meterY, fillWidth, meterHeight), SignalMeterColor);
            }

            // Draw meter border
            DrawRect(new Rect2(meterX, meterY, meterWidth, meterHeight), new Color(0.1f, 0.1f, 0.1f, 1.0f), false, 1);

            // Draw meter markers
            for (int i = 0; i <= 10; i++)
            {
                float markerX = meterX + (meterWidth * i / 10.0f);
                float markerHeight = (i % 5 == 0) ? 10 : 5; // Taller markers for multiples of 5

                DrawLine(new Vector2(markerX, meterY), new Vector2(markerX, meterY + markerHeight),
                    new Color(0.3f, 0.3f, 0.3f, 1.0f), 1);
            }
        }

        // Draw the visualization area
        private void DrawVisualizationArea()
        {
            Vector2 size = Size;
            float width = size.X;
            float height = size.Y;

            // Calculate panel dimensions
            float panelWidth = Mathf.Min(width - 40, 600);
            float panelHeight = Mathf.Min(height - 40, 400);
            float panelX = (width - panelWidth) / 2;
            float panelY = (height - panelHeight) / 2;

            // Calculate visualization dimensions
            float vizWidth = panelWidth - 40;
            float vizHeight = 80;
            float vizX = panelX + 20;
            float vizY = panelY + 80;

            // Draw visualization background
            DrawRect(new Rect2(vizX, vizY, vizWidth, vizHeight), new Color(0.05f, 0.05f, 0.05f, 1.0f));

            // Draw visualization border
            DrawRect(new Rect2(vizX, vizY, vizWidth, vizHeight), new Color(0.1f, 0.1f, 0.1f, 1.0f), false, 1);

            // Draw visualization content if radio is on
            if (_isPowerOn)
            {
                // Draw signal visualization (simple bars)
                int numBars = 32;
                float barWidth = (vizWidth - (numBars - 1) * 2) / numBars;
                float maxBarHeight = vizHeight - 10;

                for (int i = 0; i < numBars; i++)
                {
                    // Calculate bar height based on signal strength and some randomness
                    float noise = (float)GD.RandRange(-0.2, 0.2);
                    float signalFactor = _signalStrength > 0.1f ? _signalStrength : 0;
                    float staticFactor = 1.0f - signalFactor;

                    // Signal component - smooth sine wave
                    float signalHeight = 0;
                    if (signalFactor > 0)
                    {
                        float phase = (float)Time.GetTicksMsec() / 200.0f;
                        signalHeight = Mathf.Sin(phase + i * 0.2f) * signalFactor * maxBarHeight * 0.5f;
                        signalHeight = Mathf.Abs(signalHeight) + (maxBarHeight * 0.1f * signalFactor);
                    }

                    // Static component - random noise
                    float staticHeight = (float)GD.RandRange(5, maxBarHeight * 0.7f) * staticFactor;

                    // Combine signal and static
                    float barHeight = Mathf.Max(signalHeight, staticHeight);
                    barHeight = Mathf.Min(barHeight, maxBarHeight);

                    // Calculate bar position
                    float barX = vizX + i * (barWidth + 2);
                    float barY = vizY + vizHeight - barHeight - 5;

                    // Draw the bar
                    Color barColor = SignalMeterColor.Lerp(new Color(0.7f, 0.7f, 0.7f, 1.0f), staticFactor);
                    DrawRect(new Rect2(barX, barY, barWidth, barHeight), barColor);
                }
            }
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
                        // Check if mouse is over any interactive element
                        if (_powerButtonRect.HasPoint(mousePos))
                        {
                            _gameState?.ToggleRadio();
                        }
                        else if (_scanButtonRect.HasPoint(mousePos))
                        {
                            // TODO: Implement scanning in RadioSystem
                            GD.Print("Scanning not implemented yet");
                        }
                        else if (_tuneDownButtonRect.HasPoint(mousePos))
                        {
                            ChangeFrequency(-0.1f);
                        }
                        else if (_tuneUpButtonRect.HasPoint(mousePos))
                        {
                            ChangeFrequency(0.1f);
                        }
                        else if (_knobRect.HasPoint(mousePos))
                        {
                            _isDraggingKnob = true;
                            _lastMousePosition = mousePos;
                        }
                        else if (_frequencySliderRect.HasPoint(mousePos))
                        {
                            _isDraggingSlider = true;
                            UpdateFrequencyFromSliderPosition(mousePos.X);
                        }
                    }
                    else
                    {
                        // Mouse released
                        _isDraggingKnob = false;
                        _isDraggingSlider = false;
                    }
                }
            }
            // Handle mouse motion events
            else if (@event is InputEventMouseMotion mouseMotionEvent && (_isDraggingKnob || _isDraggingSlider))
            {
                Vector2 mousePos = mouseMotionEvent.Position;

                if (_isDraggingKnob)
                {
                    // Calculate rotation based on mouse movement
                    Vector2 knobCenter = new Vector2(
                        _knobRect.Position.X + _knobRect.Size.X / 2,
                        _knobRect.Position.Y + _knobRect.Size.Y / 2
                    );

                    Vector2 prevDir = (_lastMousePosition - knobCenter).Normalized();
                    Vector2 newDir = (mousePos - knobCenter).Normalized();

                    float angle = Mathf.Atan2(
                        newDir.Y - prevDir.Y,
                        newDir.X - prevDir.X
                    );

                    // Convert angle to frequency change
                    float angleInDegrees = Mathf.RadToDeg(angle);
                    float frequencyChange = angleInDegrees * 0.05f;

                    // Apply frequency change
                    ChangeFrequency(frequencyChange);

                    _lastMousePosition = mousePos;
                }
                else if (_isDraggingSlider)
                {
                    // Update frequency based on slider position
                    UpdateFrequencyFromSliderPosition(mousePos.X);
                }
            }
        }

        // Change the frequency by a specific amount
        private void ChangeFrequency(float amount)
        {
            if (_gameState == null) return;

            float newFreq = _currentFrequency + amount;
            newFreq = Mathf.Clamp(newFreq, _minFrequency, _maxFrequency);
            newFreq = Mathf.Snapped(newFreq, 0.1f);  // Round to nearest 0.1

            _gameState.SetFrequency(newFreq);
        }

        // Update frequency based on slider position
        private void UpdateFrequencyFromSliderPosition(float positionX)
        {
            if (_gameState == null) return;

            float sliderStart = _frequencySliderRect.Position.X;
            float sliderEnd = sliderStart + _frequencySliderRect.Size.X;
            float percentage = Mathf.Clamp((positionX - sliderStart) / (sliderEnd - sliderStart), 0.0f, 1.0f);

            float newFreq = _minFrequency + percentage * (_maxFrequency - _minFrequency);
            newFreq = Mathf.Snapped(newFreq, 0.1f);  // Round to nearest 0.1

            _gameState.SetFrequency(newFreq);
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

        // Show or hide the radio interface
        public new void SetVisible(bool visible)
        {
            Visible = visible;
            SetProcess(visible);
            SetProcessInput(visible);
        }

        // Check if the radio interface is visible
        public new bool IsVisible()
        {
            return Visible;
        }

        // Set GameState reference (for testing)
        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
        }

        // Set RadioSystem reference (for testing)
        public void SetRadioSystem(RadioSystem radioSystem)
        {
            _radioSystem = radioSystem;
        }
    }
}

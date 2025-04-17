using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class SimpleRadioTest : Control
    {
        // Pixel art settings
        private const int PIXEL_SIZE = 4; // Size of each "pixel" in our pixel art
        private const int GRID_WIDTH = 160; // Width in virtual pixels
        private const int GRID_HEIGHT = 120; // Height in virtual pixels

        // Color palette (retro-inspired)
        private readonly Color[] _palette = new Color[] {
            new Color(0.0f, 0.0f, 0.0f),       // Black
            new Color(0.33f, 0.2f, 0.53f),      // Dark Purple
            new Color(0.2f, 0.4f, 0.6f),        // Navy Blue
            new Color(0.26f, 0.53f, 0.96f),     // Bright Blue
            new Color(0.6f, 0.73f, 1.0f),       // Light Blue
            new Color(0.93f, 0.94f, 0.9f),      // Off White
            new Color(0.9f, 0.4f, 0.4f),        // Red
            new Color(0.4f, 0.73f, 0.4f),       // Green
            new Color(0.93f, 0.9f, 0.55f)       // Yellow
        };

        // Noise texture for static effect
        private List<List<int>> _noiseGrid;
        private float _noiseTimer = 0.0f;
        // Radio properties
        private float _minFrequency = 88.0f;
        private float _maxFrequency = 108.0f;
        private float _currentFrequency = 98.0f;
        private bool _isPowerOn = false;
        private bool _isScanning = false;
        private float _signalStrength = 0.0f;



        // Dragging state
        private bool _isDraggingSlider = false;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("SimpleRadioTest ready!");

            // Initialize noise grid for static effect
            InitializeNoiseGrid();
        }

        // Initialize the noise grid for static visualization
        private void InitializeNoiseGrid()
        {
            _noiseGrid = new List<List<int>>();
            Random random = new Random();

            // Create a 2D grid of random noise values
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                List<int> row = new List<int>();
                for (int x = 0; x < GRID_WIDTH; x++)
                {
                    row.Add(random.Next(0, _palette.Length));
                }
                _noiseGrid.Add(row);
            }
        }

        // Update the noise grid for animation
        private void UpdateNoiseGrid(float delta)
        {
            if (_noiseGrid == null) return;

            _noiseTimer += delta;

            // Update noise every 0.1 seconds for animation
            if (_noiseTimer >= 0.1f)
            {
                _noiseTimer = 0;

                Random random = new Random();

                // Update about 20% of the pixels each frame for a dynamic effect
                int pixelsToUpdate = (GRID_WIDTH * GRID_HEIGHT) / 5;

                for (int i = 0; i < pixelsToUpdate; i++)
                {
                    int x = random.Next(0, GRID_WIDTH);
                    int y = random.Next(0, GRID_HEIGHT);

                    // More likely to be dark colors for static
                    int colorIndex = random.Next(0, 5); // Limit to first 5 colors in palette
                    _noiseGrid[y][x] = colorIndex;
                }
            }
        }



        // Process function called every frame
        public override void _Process(double delta)
        {
            float deltaF = (float)delta;

            // Update noise grid for static effect
            UpdateNoiseGrid(deltaF);

            // If scanning, update frequency
            if (_isPowerOn && _isScanning)
            {
                _currentFrequency += 0.1f;
                if (_currentFrequency > _maxFrequency)
                    _currentFrequency = _minFrequency;

                // Generate random signal strength
                _signalStrength = (float)GD.RandRange(0.0, 1.0);
            }

            // Simulate signal strength fluctuations for realism
            if (_isPowerOn && _signalStrength > 0)
            {
                // Add some random fluctuation to signal strength
                float fluctuation = (float)GD.RandRange(-0.05, 0.05);
                _signalStrength = Mathf.Clamp(_signalStrength + fluctuation, 0.0f, 1.0f);
            }

            // Redraw the UI
            QueueRedraw();
        }

        // Custom drawing function
        public override void _Draw()
        {
            // Calculate the scale to fit our pixel grid to the actual screen size
            Vector2 size = Size;
            float scaleX = size.X / (GRID_WIDTH * PIXEL_SIZE);
            float scaleY = size.Y / (GRID_HEIGHT * PIXEL_SIZE);
            float scale = Mathf.Min(scaleX, scaleY);

            // Center the pixel grid on screen
            float offsetX = (size.X - (GRID_WIDTH * PIXEL_SIZE * scale)) / 2;
            float offsetY = (size.Y - (GRID_HEIGHT * PIXEL_SIZE * scale)) / 2;

            // Draw background (dark color)
            DrawRect(new Rect2(0, 0, size.X, size.Y), _palette[0]);

            // Draw pixel grid border
            DrawPixelRect(0, 0, GRID_WIDTH, GRID_HEIGHT, 1, offsetX, offsetY, scale);

            // Draw radio background (dark blue)
            DrawPixelRect(2, 2, GRID_WIDTH - 4, GRID_HEIGHT - 4, 2, offsetX, offsetY, scale);

            // Draw display area
            int displayX = 10;
            int displayY = 10;
            int displayWidth = GRID_WIDTH - 20;
            int displayHeight = 20;

            // Display background
            DrawPixelRect(displayX, displayY, displayWidth, displayHeight, _isPowerOn ? 1 : 0, offsetX, offsetY, scale);

            // Draw frequency text if power is on
            if (_isPowerOn)
            {
                // Draw frequency as pixel text
                string freqText = $"{_currentFrequency:F1}";
                DrawPixelText(freqText, displayX + 5, displayY + 6, 5, offsetX, offsetY, scale);

                // Draw MHz label
                DrawPixelText("MHz", displayX + displayWidth - 25, displayY + 6, 5, offsetX, offsetY, scale);

                // Draw static visualization in the display if signal is weak
                if (_signalStrength < 0.7f)
                {
                    // Draw static noise in the display area
                    DrawStaticNoise(displayX + 2, displayY + 2, displayWidth - 4, displayHeight - 4,
                                    1.0f - _signalStrength, offsetX, offsetY, scale);
                }
            }

            // Draw buttons
            int buttonY = GRID_HEIGHT - 30;
            int buttonHeight = 15;

            // Power button
            DrawPixelButton(10, buttonY, 30, buttonHeight, _isPowerOn ? "ON" : "OFF", _isPowerOn ? 6 : 2, offsetX, offsetY, scale);

            // Scan button
            DrawPixelButton(45, buttonY, 30, buttonHeight, _isScanning ? "STOP" : "SCAN",
                           (_isScanning && _isPowerOn) ? 7 : 2, offsetX, offsetY, scale);

            // Tune buttons
            DrawPixelButton(GRID_WIDTH - 75, buttonY, 20, buttonHeight, "<", 2, offsetX, offsetY, scale);
            DrawPixelButton(GRID_WIDTH - 30, buttonY, 20, buttonHeight, ">", 2, offsetX, offsetY, scale);

            // Draw frequency slider
            int sliderY = GRID_HEIGHT / 2;
            int sliderHeight = 8;
            DrawPixelRect(10, sliderY, GRID_WIDTH - 20, sliderHeight, 1, offsetX, offsetY, scale);

            // Draw slider handle
            if (_isPowerOn)
            {
                float frequencyRange = _maxFrequency - _minFrequency;
                float frequencyPercentage = (_currentFrequency - _minFrequency) / frequencyRange;
                int handleX = 10 + (int)(frequencyPercentage * (GRID_WIDTH - 20 - 6));

                DrawPixelRect(handleX, sliderY - 2, 6, sliderHeight + 4, 3, offsetX, offsetY, scale);
            }

            // Draw signal strength meter
            int meterY = sliderY + 20;
            int meterHeight = 8;
            DrawPixelRect(10, meterY, GRID_WIDTH - 20, meterHeight, 1, offsetX, offsetY, scale);

            // Draw signal meter fill
            if (_isPowerOn)
            {
                int fillWidth = (int)((GRID_WIDTH - 20) * _signalStrength);
                DrawPixelRect(10, meterY, fillWidth, meterHeight, 7, offsetX, offsetY, scale);
            }

            // Draw signal visualization
            if (_isPowerOn)
            {
                int vizY = meterY + meterHeight + 10;
                int vizHeight = 20;

                // Background for visualization
                DrawPixelRect(10, vizY, GRID_WIDTH - 20, vizHeight, 1, offsetX, offsetY, scale);

                // Draw static noise based on signal strength
                DrawStaticNoise(10, vizY, GRID_WIDTH - 20, vizHeight, 1.0f - _signalStrength, offsetX, offsetY, scale);

                // Draw signal wave if we have good reception
                if (_signalStrength > 0.3f)
                {
                    DrawSignalWave(10, vizY, GRID_WIDTH - 20, vizHeight, _signalStrength, offsetX, offsetY, scale);
                }
            }
        }

        // Helper method to draw a pixel rectangle
        private void DrawPixelRect(int x, int y, int width, int height, int colorIndex, float offsetX, float offsetY, float scale)
        {
            Color color = _palette[colorIndex];
            DrawRect(
                new Rect2(
                    offsetX + x * PIXEL_SIZE * scale,
                    offsetY + y * PIXEL_SIZE * scale,
                    width * PIXEL_SIZE * scale,
                    height * PIXEL_SIZE * scale
                ),
                color
            );
        }

        // Helper method to draw a pixel button
        private void DrawPixelButton(int x, int y, int width, int height, string text, int colorIndex, float offsetX, float offsetY, float scale)
        {
            // Button background
            DrawPixelRect(x, y, width, height, colorIndex, offsetX, offsetY, scale);

            // Button border
            DrawPixelRect(x, y, width, 1, 0, offsetX, offsetY, scale); // Top
            DrawPixelRect(x, y, 1, height, 0, offsetX, offsetY, scale); // Left
            DrawPixelRect(x, y + height - 1, width, 1, 0, offsetX, offsetY, scale); // Bottom
            DrawPixelRect(x + width - 1, y, 1, height, 0, offsetX, offsetY, scale); // Right

            // Button text
            DrawPixelText(text, x + 3, y + 4, 5, offsetX, offsetY, scale);
        }

        // Helper method to draw pixel text
        private void DrawPixelText(string text, int x, int y, int colorIndex, float offsetX, float offsetY, float scale)
        {
            Color color = _palette[colorIndex];

            // Draw text using small rectangles for a pixelated look
            // This is a simplified approach - a real implementation would use a pixel font
            DrawString(
                ThemeDB.FallbackFont,
                new Vector2(
                    offsetX + x * PIXEL_SIZE * scale,
                    offsetY + y * PIXEL_SIZE * scale
                ),
                text,
                HorizontalAlignment.Left,
                -1,
                (int)(8 * PIXEL_SIZE * scale),
                color
            );
        }

        // Helper method to draw static noise
        private void DrawStaticNoise(int x, int y, int width, int height, float intensity, float offsetX, float offsetY, float scale)
        {
            if (_noiseGrid == null) return;

            // Only draw some pixels based on intensity
            Random random = new Random();

            for (int py = 0; py < height; py++)
            {
                for (int px = 0; px < width; px++)
                {
                    // Skip pixels based on intensity
                    if (random.NextDouble() > intensity) continue;

                    // Get a noise value from our grid
                    int gridX = (x + px) % GRID_WIDTH;
                    int gridY = (y + py) % GRID_HEIGHT;
                    int colorIndex = _noiseGrid[gridY][gridX];

                    // Draw a single pixel
                    DrawRect(
                        new Rect2(
                            offsetX + (x + px) * PIXEL_SIZE * scale,
                            offsetY + (y + py) * PIXEL_SIZE * scale,
                            PIXEL_SIZE * scale,
                            PIXEL_SIZE * scale
                        ),
                        _palette[colorIndex]
                    );
                }
            }
        }

        // Helper method to draw a signal wave
        private void DrawSignalWave(int x, int y, int width, int height, float strength, float offsetX, float offsetY, float scale)
        {
            int centerY = y + height / 2;
            int amplitude = (int)(height / 3 * strength);

            // Draw a sine wave
            for (int px = 0; px < width; px++)
            {
                float phase = (float)px / width * Mathf.Pi * 8; // 4 complete waves
                int waveY = centerY + (int)(Mathf.Sin(phase) * amplitude);

                // Draw a pixel of the wave
                DrawRect(
                    new Rect2(
                        offsetX + (x + px) * PIXEL_SIZE * scale,
                        offsetY + waveY * PIXEL_SIZE * scale,
                        PIXEL_SIZE * scale,
                        PIXEL_SIZE * scale
                    ),
                    _palette[8] // Yellow
                );
            }
        }



        // Handle input events
        public override void _GuiInput(InputEvent @event)
        {
            // Calculate the scale to fit our pixel grid to the actual screen size
            Vector2 size = Size;
            float scaleX = size.X / (GRID_WIDTH * PIXEL_SIZE);
            float scaleY = size.Y / (GRID_HEIGHT * PIXEL_SIZE);
            float scale = Mathf.Min(scaleX, scaleY);

            // Center the pixel grid on screen
            float offsetX = (size.X - (GRID_WIDTH * PIXEL_SIZE * scale)) / 2;
            float offsetY = (size.Y - (GRID_HEIGHT * PIXEL_SIZE * scale)) / 2;

            // Handle mouse button events
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    Vector2 mousePos = mouseButtonEvent.Position;

                    // Convert screen coordinates to our pixel grid coordinates
                    int pixelX = (int)((mousePos.X - offsetX) / (PIXEL_SIZE * scale));
                    int pixelY = (int)((mousePos.Y - offsetY) / (PIXEL_SIZE * scale));

                    // Check if click is within our grid
                    if (pixelX >= 0 && pixelX < GRID_WIDTH && pixelY >= 0 && pixelY < GRID_HEIGHT)
                    {
                        if (mouseButtonEvent.Pressed)
                        {
                            // Button positions
                            int buttonY = GRID_HEIGHT - 30;
                            int buttonHeight = 15;

                            // Check power button
                            if (IsPointInRect(pixelX, pixelY, 10, buttonY, 30, buttonHeight))
                            {
                                _isPowerOn = !_isPowerOn;
                                if (!_isPowerOn) _isScanning = false; // Turn off scanning when power is off
                            }
                            // Check scan button
                            else if (IsPointInRect(pixelX, pixelY, 45, buttonY, 30, buttonHeight) && _isPowerOn)
                            {
                                _isScanning = !_isScanning;
                            }
                            // Check tune down button
                            else if (IsPointInRect(pixelX, pixelY, GRID_WIDTH - 75, buttonY, 20, buttonHeight) && _isPowerOn)
                            {
                                _currentFrequency -= 0.1f;
                                if (_currentFrequency < _minFrequency)
                                    _currentFrequency = _minFrequency;

                                // Generate random signal strength when tuning
                                _signalStrength = (float)GD.RandRange(0.0, 1.0);
                            }
                            // Check tune up button
                            else if (IsPointInRect(pixelX, pixelY, GRID_WIDTH - 30, buttonY, 20, buttonHeight) && _isPowerOn)
                            {
                                _currentFrequency += 0.1f;
                                if (_currentFrequency > _maxFrequency)
                                    _currentFrequency = _maxFrequency;

                                // Generate random signal strength when tuning
                                _signalStrength = (float)GD.RandRange(0.0, 1.0);
                            }
                            // Check slider
                            else if (_isPowerOn)
                            {
                                int sliderY = GRID_HEIGHT / 2;
                                int sliderHeight = 8;

                                if (IsPointInRect(pixelX, pixelY, 10, sliderY - 2, GRID_WIDTH - 20, sliderHeight + 4))
                                {
                                    _isDraggingSlider = true;
                                    UpdateFrequencyFromPixelPosition(pixelX);
                                }
                            }
                        }
                        else
                        {
                            // Mouse released
                            _isDraggingSlider = false;
                        }
                    }
                }
            }
            // Handle mouse motion events
            else if (@event is InputEventMouseMotion mouseMotionEvent && _isDraggingSlider && _isPowerOn)
            {
                // Convert screen coordinates to our pixel grid coordinates
                int pixelX = (int)((mouseMotionEvent.Position.X - offsetX) / (PIXEL_SIZE * scale));

                // Update frequency based on pixel position
                UpdateFrequencyFromPixelPosition(pixelX);
            }
        }

        // Helper to check if a point is in a rectangle
        private bool IsPointInRect(int x, int y, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            return x >= rectX && x < rectX + rectWidth && y >= rectY && y < rectY + rectHeight;
        }

        // Update frequency based on pixel position
        private void UpdateFrequencyFromPixelPosition(int pixelX)
        {
            // Clamp to slider bounds
            pixelX = Mathf.Clamp(pixelX, 10, GRID_WIDTH - 10);

            // Calculate percentage along slider
            float percentage = (float)(pixelX - 10) / (GRID_WIDTH - 20 - 6);
            percentage = Mathf.Clamp(percentage, 0, 1);

            // Calculate new frequency
            float frequencyRange = _maxFrequency - _minFrequency;
            float newFrequency = _minFrequency + percentage * frequencyRange;

            // Round to nearest 0.1
            _currentFrequency = Mathf.Snapped(newFrequency, 0.1f);

            // Generate random signal strength when tuning
            _signalStrength = (float)GD.RandRange(0.0, 1.0);
        }



        // Called when the control is resized
        public override void _Notification(int what)
        {
            base._Notification(what);

            if (what == NotificationResized)
            {
                // Our pixel grid automatically scales with the window size
                QueueRedraw();
            }
        }
    }
}

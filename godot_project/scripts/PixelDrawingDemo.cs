using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    /// <summary>
    /// A simple demo for visualizing pixel-based drawing.
    /// </summary>
    public partial class PixelDrawingDemo : Control
    {
        // Configuration
        [Export] public int GridSize { get; set; } = 8;
        [Export] public Color GridColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        [Export] public Color ActivePixelColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        [Export] public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        
        // State
        private bool[,] _pixels;
        private int _gridWidth;
        private int _gridHeight;
        private bool _isDrawing = false;
        private bool _isErasing = false;
        private Vector2I _lastPixelPos = new Vector2I(-1, -1);
        private List<Vector2I> _currentStroke = new List<Vector2I>();
        
        // UI Elements
        private Button _clearButton;
        private Button _saveButton;
        private Button _loadButton;
        private Label _infoLabel;
        private SpinBox _gridSizeSpinBox;
        private ColorPickerButton _pixelColorPicker;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _clearButton = GetNode<Button>("ControlPanel/ClearButton");
            _saveButton = GetNode<Button>("ControlPanel/SaveButton");
            _loadButton = GetNode<Button>("ControlPanel/LoadButton");
            _infoLabel = GetNode<Label>("ControlPanel/InfoLabel");
            _gridSizeSpinBox = GetNode<SpinBox>("ControlPanel/GridSizeSpinBox");
            _pixelColorPicker = GetNode<ColorPickerButton>("ControlPanel/PixelColorPicker");
            
            // Connect signals
            _clearButton.Pressed += OnClearButtonPressed;
            _saveButton.Pressed += OnSaveButtonPressed;
            _loadButton.Pressed += OnLoadButtonPressed;
            _gridSizeSpinBox.ValueChanged += OnGridSizeChanged;
            _pixelColorPicker.ColorChanged += OnPixelColorChanged;
            
            // Initialize UI
            _gridSizeSpinBox.Value = GridSize;
            _pixelColorPicker.Color = ActivePixelColor;
            
            // Initialize the pixel grid
            InitializeGrid();
            
            // Update info
            UpdateInfoLabel();
        }
        
        // Initialize the pixel grid
        private void InitializeGrid()
        {
            Vector2 size = GetViewportRect().Size;
            _gridWidth = (int)(size.X * 0.7f) / GridSize;
            _gridHeight = (int)(size.Y) / GridSize;
            
            _pixels = new bool[_gridWidth, _gridHeight];
            
            // Clear all pixels
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _pixels[x, y] = false;
                }
            }
            
            // Queue redraw
            QueueRedraw();
        }
        
        // Update the info label
        private void UpdateInfoLabel()
        {
            int activePixels = CountActivePixels();
            float percentage = (float)activePixels / (_gridWidth * _gridHeight) * 100;
            
            _infoLabel.Text = $"Grid Size: {GridSize}px\n" +
                             $"Grid Dimensions: {_gridWidth} x {_gridHeight}\n" +
                             $"Active Pixels: {activePixels} ({percentage:F2}%)\n\n" +
                             "Left-click to draw\n" +
                             "Right-click to erase\n" +
                             "Shift+click to draw lines";
        }
        
        // Count active pixels
        private int CountActivePixels()
        {
            int count = 0;
            
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_pixels[x, y])
                    {
                        count++;
                    }
                }
            }
            
            return count;
        }
        
        // Handle input events
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButtonEvent)
            {
                if (mouseButtonEvent.ButtonIndex == MouseButton.Left)
                {
                    _isDrawing = mouseButtonEvent.Pressed;
                    _isErasing = false;
                    
                    if (_isDrawing)
                    {
                        Vector2I pixelPos = GetPixelPos(mouseButtonEvent.Position);
                        if (IsValidPixelPos(pixelPos))
                        {
                            _lastPixelPos = pixelPos;
                            _currentStroke.Clear();
                            _currentStroke.Add(pixelPos);
                            SetPixel(pixelPos.X, pixelPos.Y, true);
                        }
                    }
                    else
                    {
                        _lastPixelPos = new Vector2I(-1, -1);
                    }
                }
                else if (mouseButtonEvent.ButtonIndex == MouseButton.Right)
                {
                    _isErasing = mouseButtonEvent.Pressed;
                    _isDrawing = false;
                    
                    if (_isErasing)
                    {
                        Vector2I pixelPos = GetPixelPos(mouseButtonEvent.Position);
                        if (IsValidPixelPos(pixelPos))
                        {
                            _lastPixelPos = pixelPos;
                            SetPixel(pixelPos.X, pixelPos.Y, false);
                        }
                    }
                    else
                    {
                        _lastPixelPos = new Vector2I(-1, -1);
                    }
                }
            }
            else if (@event is InputEventMouseMotion mouseMotionEvent && (_isDrawing || _isErasing))
            {
                Vector2I pixelPos = GetPixelPos(mouseMotionEvent.Position);
                if (IsValidPixelPos(pixelPos) && pixelPos != _lastPixelPos)
                {
                    if (Input.IsKeyPressed(Key.Shift) && _lastPixelPos.X >= 0 && _lastPixelPos.Y >= 0)
                    {
                        // Draw a line from last position to current position
                        DrawLine(_lastPixelPos, pixelPos, _isDrawing);
                    }
                    else
                    {
                        // Just set the current pixel
                        SetPixel(pixelPos.X, pixelPos.Y, _isDrawing);
                    }
                    
                    _lastPixelPos = pixelPos;
                    if (_isDrawing)
                    {
                        _currentStroke.Add(pixelPos);
                    }
                }
            }
        }
        
        // Draw a line between two pixel positions
        private void DrawLine(Vector2I from, Vector2I to, bool value)
        {
            // Bresenham's line algorithm
            int dx = Math.Abs(to.X - from.X);
            int dy = Math.Abs(to.Y - from.Y);
            int sx = from.X < to.X ? 1 : -1;
            int sy = from.Y < to.Y ? 1 : -1;
            int err = dx - dy;
            
            while (true)
            {
                SetPixel(from.X, from.Y, value);
                if (_isDrawing)
                {
                    _currentStroke.Add(from);
                }
                
                if (from.X == to.X && from.Y == to.Y)
                    break;
                
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    from.X += sx;
                }
                
                if (e2 < dx)
                {
                    err += dx;
                    from.Y += sy;
                }
            }
        }
        
        // Convert screen position to pixel grid position
        private Vector2I GetPixelPos(Vector2 screenPos)
        {
            int x = (int)(screenPos.X / GridSize);
            int y = (int)(screenPos.Y / GridSize);
            return new Vector2I(x, y);
        }
        
        // Check if a pixel position is valid
        private bool IsValidPixelPos(Vector2I pos)
        {
            return pos.X >= 0 && pos.X < _gridWidth && pos.Y >= 0 && pos.Y < _gridHeight;
        }
        
        // Set a pixel value
        private void SetPixel(int x, int y, bool value)
        {
            if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight)
            {
                _pixels[x, y] = value;
                QueueRedraw();
                UpdateInfoLabel();
            }
        }
        
        // Draw function
        public override void _Draw()
        {
            // Draw background
            DrawRect(new Rect2(0, 0, _gridWidth * GridSize, _gridHeight * GridSize), BackgroundColor);
            
            // Draw grid
            for (int x = 0; x <= _gridWidth; x++)
            {
                DrawLine(new Vector2(x * GridSize, 0), new Vector2(x * GridSize, _gridHeight * GridSize), GridColor);
            }
            
            for (int y = 0; y <= _gridHeight; y++)
            {
                DrawLine(new Vector2(0, y * GridSize), new Vector2(_gridWidth * GridSize, y * GridSize), GridColor);
            }
            
            // Draw active pixels
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (_pixels[x, y])
                    {
                        DrawRect(new Rect2(x * GridSize, y * GridSize, GridSize, GridSize), ActivePixelColor);
                    }
                }
            }
        }
        
        // Handle resize
        public override void _Notification(int what)
        {
            if (what == NotificationResized)
            {
                InitializeGrid();
            }
        }
        
        // Event handlers
        private void OnClearButtonPressed()
        {
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    _pixels[x, y] = false;
                }
            }
            
            QueueRedraw();
            UpdateInfoLabel();
        }
        
        private void OnSaveButtonPressed()
        {
            // Convert pixel data to a string representation
            string data = "";
            
            for (int y = 0; y < _gridHeight; y++)
            {
                for (int x = 0; x < _gridWidth; x++)
                {
                    data += _pixels[x, y] ? "1" : "0";
                }
                data += "\n";
            }
            
            // Save to a file
            string path = "user://pixel_drawing.txt";
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            if (file != null)
            {
                file.StoreString(data);
                GD.Print($"Pixel data saved to {path}");
                _infoLabel.Text = $"Pixel data saved to {path}\n\n" + _infoLabel.Text;
            }
            else
            {
                GD.PrintErr($"Failed to save pixel data to {path}");
                _infoLabel.Text = $"Failed to save pixel data\n\n" + _infoLabel.Text;
            }
        }
        
        private void OnLoadButtonPressed()
        {
            // Load from a file
            string path = "user://pixel_drawing.txt";
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                string data = file.GetAsText();
                string[] lines = data.Split('\n');
                
                // Clear current pixels
                OnClearButtonPressed();
                
                // Parse the data
                for (int y = 0; y < Math.Min(lines.Length, _gridHeight); y++)
                {
                    string line = lines[y];
                    for (int x = 0; x < Math.Min(line.Length, _gridWidth); x++)
                    {
                        _pixels[x, y] = line[x] == '1';
                    }
                }
                
                GD.Print($"Pixel data loaded from {path}");
                _infoLabel.Text = $"Pixel data loaded from {path}\n\n" + _infoLabel.Text;
                QueueRedraw();
                UpdateInfoLabel();
            }
            else
            {
                GD.PrintErr($"Failed to load pixel data from {path}");
                _infoLabel.Text = $"Failed to load pixel data\n\n" + _infoLabel.Text;
            }
        }
        
        private void OnGridSizeChanged(double value)
        {
            GridSize = (int)value;
            InitializeGrid();
        }
        
        private void OnPixelColorChanged(Color color)
        {
            ActivePixelColor = color;
            QueueRedraw();
        }
    }
}

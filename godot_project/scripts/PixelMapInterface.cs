using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelMapInterface : Control
    {
        // Configuration
        [Export] public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [Export] public Color GridColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        [Export] public Color LocationColor { get; set; } = new Color(0.3f, 0.7f, 0.3f, 1.0f);
        [Export] public Color ConnectionColor { get; set; } = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        [Export] public Color HighlightColor { get; set; } = new Color(0.8f, 0.8f, 0.2f, 1.0f);
        [Export] public Color TextColor { get; set; } = new Color(0.9f, 0.9f, 0.9f, 1.0f);
        [Export] public Color UndiscoveredColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [Export] public int GridSize { get; set; } = 20; // Size of each grid cell in pixels
        [Export] public int LocationSize { get; set; } = 10; // Size of location markers in pixels
        [Export] public bool ShowGrid { get; set; } = true;
        [Export] public float ZoomLevel { get; set; } = 1.0f;
        [Export] public Vector2 MapOffset { get; set; } = Vector2.Zero;

        // References to game systems
        private MapSystem _mapSystem;
        private GameState _gameState;
        private PixelFont _pixelFont;

        // UI state
        private bool _isVisible = false;
        private string _selectedLocationId = null;
        private Vector2 _mousePosition = Vector2.Zero;
        private bool _isDragging = false;
        private Vector2 _dragStartPosition = Vector2.Zero;
        private Dictionary<string, Rect2> _locationRects = new Dictionary<string, Rect2>();

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to game systems
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_mapSystem == null || _gameState == null)
            {
                GD.PrintErr("PixelMapInterface: Failed to get MapSystem or GameState reference");
                return;
            }

            // Initialize pixel font
            _pixelFont = new PixelFont();

            // Connect signals
            _mapSystem.LocationDiscovered += OnLocationDiscovered;
            _mapSystem.LocationChanged += OnLocationChanged;
            _gameState.LocationChanged += OnLocationChanged;

            // Set up input processing
            SetProcessInput(true);
        }

        // Show or hide the map interface
        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            SetProcess(visible);
            SetProcessInput(visible);
            QueueRedraw();
        }

        // Toggle visibility
        public void ToggleVisibility()
        {
            SetVisible(!_isVisible);
        }

        // Check if the map interface is visible
        public bool IsVisible()
        {
            return _isVisible;
        }

        // Process input events
        public override void _Input(InputEvent @event)
        {
            if (!_isVisible)
                return;

            if (@event is InputEventMouseMotion mouseMotion)
            {
                _mousePosition = mouseMotion.Position;
                
                // Handle dragging
                if (_isDragging)
                {
                    MapOffset += (mouseMotion.Position - _dragStartPosition) / ZoomLevel;
                    _dragStartPosition = mouseMotion.Position;
                    QueueRedraw();
                }
                
                // Update hover state for locations
                UpdateLocationHover();
            }
            else if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left)
                {
                    if (mouseButton.Pressed)
                    {
                        // Start dragging the map
                        _isDragging = true;
                        _dragStartPosition = mouseButton.Position;
                    }
                    else
                    {
                        // Stop dragging
                        _isDragging = false;
                        
                        // Check if a location was clicked
                        foreach (var locationId in _locationRects.Keys)
                        {
                            if (_locationRects[locationId].HasPoint(_mousePosition))
                            {
                                SelectLocation(locationId);
                                break;
                            }
                        }
                    }
                }
                else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                {
                    // Zoom in
                    ZoomLevel = Mathf.Min(ZoomLevel * 1.1f, 3.0f);
                    QueueRedraw();
                }
                else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                {
                    // Zoom out
                    ZoomLevel = Mathf.Max(ZoomLevel / 1.1f, 0.5f);
                    QueueRedraw();
                }
            }
            else if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Close map with Escape key
                if (keyEvent.Keycode == Key.Escape)
                {
                    SetVisible(false);
                }
            }
        }

        // Custom drawing function
        public override void _Draw()
        {
            if (!_isVisible)
                return;

            Vector2 size = Size;
            
            // Draw background
            DrawRect(new Rect2(0, 0, size.X, size.Y), BackgroundColor);
            
            // Draw grid
            if (ShowGrid)
            {
                DrawGrid();
            }
            
            // Draw connections between locations
            DrawConnections();
            
            // Draw locations
            DrawLocations();
            
            // Draw UI elements (title, info panel, etc.)
            DrawUI();
        }

        // Draw the grid
        private void DrawGrid()
        {
            Vector2 size = Size;
            float scaledGridSize = GridSize * ZoomLevel;
            
            // Calculate grid offset based on map offset
            float offsetX = (MapOffset.X * ZoomLevel) % scaledGridSize;
            float offsetY = (MapOffset.Y * ZoomLevel) % scaledGridSize;
            
            // Draw vertical grid lines
            for (float x = offsetX; x < size.X; x += scaledGridSize)
            {
                DrawLine(new Vector2(x, 0), new Vector2(x, size.Y), GridColor, 1);
            }
            
            // Draw horizontal grid lines
            for (float y = offsetY; y < size.Y; y += scaledGridSize)
            {
                DrawLine(new Vector2(0, y), new Vector2(size.X, y), GridColor, 1);
            }
        }

        // Draw connections between locations
        private void DrawConnections()
        {
            var locations = _mapSystem.GetAllLocations();
            
            foreach (var location in locations.Values)
            {
                if (!location.IsDiscovered)
                    continue;
                
                Vector2 startPos = GetScreenPosition(location.Position);
                
                foreach (var connectedId in location.ConnectedLocations)
                {
                    if (!locations.ContainsKey(connectedId))
                        continue;
                    
                    var connectedLocation = locations[connectedId];
                    
                    // Only draw connections to discovered locations
                    if (!connectedLocation.IsDiscovered)
                        continue;
                    
                    Vector2 endPos = GetScreenPosition(connectedLocation.Position);
                    
                    // Draw the connection line
                    DrawLine(startPos, endPos, ConnectionColor, 2);
                }
            }
        }

        // Draw locations
        private void DrawLocations()
        {
            _locationRects.Clear();
            
            var locations = _mapSystem.GetAllLocations();
            string currentLocationId = _gameState.CurrentLocation;
            
            foreach (var location in locations.Values)
            {
                // Skip undiscovered locations
                if (!location.IsDiscovered)
                    continue;
                
                Vector2 pos = GetScreenPosition(location.Position);
                float size = LocationSize * ZoomLevel;
                
                // Determine location color
                Color color = LocationColor;
                
                if (location.Id == currentLocationId)
                {
                    // Current location
                    color = HighlightColor;
                    size *= 1.2f; // Make current location slightly larger
                }
                else if (location.Id == _selectedLocationId)
                {
                    // Selected location
                    color = new Color(0.7f, 0.3f, 0.3f, 1.0f);
                }
                
                // Draw location marker
                DrawCircle(pos, size, color);
                
                // Draw location name
                DrawString(ThemeDB.FallbackFont, pos + new Vector2(0, size + 10), 
                    location.Name, HorizontalAlignment.Center, -1, (int)(12 * ZoomLevel), TextColor);
                
                // Store location rect for interaction
                _locationRects[location.Id] = new Rect2(pos.X - size, pos.Y - size, size * 2, size * 2);
            }
        }

        // Draw UI elements
        private void DrawUI()
        {
            Vector2 size = Size;
            
            // Draw title
            DrawString(ThemeDB.FallbackFont, new Vector2(20, 30), 
                "SIGNAL LOST - MAP", HorizontalAlignment.Left, -1, 24, TextColor);
            
            // Draw selected location info if any
            if (_selectedLocationId != null)
            {
                var location = _mapSystem.GetLocation(_selectedLocationId);
                
                if (location != null)
                {
                    // Draw info panel
                    float panelWidth = 300;
                    float panelHeight = 200;
                    float panelX = size.X - panelWidth - 20;
                    float panelY = 20;
                    
                    DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight), 
                        new Color(0.15f, 0.15f, 0.15f, 0.9f));
                    
                    // Draw location name
                    DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, panelY + 30), 
                        location.Name, HorizontalAlignment.Left, -1, 18, TextColor);
                    
                    // Draw location description
                    DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, panelY + 60), 
                        location.Description, HorizontalAlignment.Left, (int)panelWidth - 20, 14, TextColor);
                    
                    // Draw connected locations
                    DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, panelY + 120), 
                        "Connected to:", HorizontalAlignment.Left, -1, 14, TextColor);
                    
                    float y = panelY + 140;
                    foreach (var connectedId in location.ConnectedLocations)
                    {
                        var connectedLocation = _mapSystem.GetLocation(connectedId);
                        
                        if (connectedLocation != null && connectedLocation.IsDiscovered)
                        {
                            DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 20, y), 
                                "• " + connectedLocation.Name, HorizontalAlignment.Left, -1, 12, TextColor);
                            y += 20;
                        }
                    }
                }
            }
            
            // Draw help text
            DrawString(ThemeDB.FallbackFont, new Vector2(20, size.Y - 20), 
                "Drag to pan | Scroll to zoom | ESC to close", HorizontalAlignment.Left, -1, 12, TextColor);
        }

        // Convert map position to screen position
        private Vector2 GetScreenPosition(Vector2 mapPosition)
        {
            Vector2 size = Size;
            Vector2 center = size / 2;
            
            return center + (mapPosition + MapOffset) * ZoomLevel;
        }

        // Convert screen position to map position
        private Vector2 GetMapPosition(Vector2 screenPosition)
        {
            Vector2 size = Size;
            Vector2 center = size / 2;
            
            return (screenPosition - center) / ZoomLevel - MapOffset;
        }

        // Update hover state for locations
        private void UpdateLocationHover()
        {
            bool needsRedraw = false;
            
            foreach (var locationId in _locationRects.Keys)
            {
                bool isHovered = _locationRects[locationId].HasPoint(_mousePosition);
                
                // If hover state changed, redraw
                if (isHovered && _selectedLocationId != locationId)
                {
                    _selectedLocationId = locationId;
                    needsRedraw = true;
                }
            }
            
            if (needsRedraw)
            {
                QueueRedraw();
            }
        }

        // Select a location
        private void SelectLocation(string locationId)
        {
            if (_selectedLocationId != locationId)
            {
                _selectedLocationId = locationId;
                QueueRedraw();
            }
        }

        // Event handlers
        private void OnLocationDiscovered(string locationId)
        {
            QueueRedraw();
        }

        private void OnLocationChanged(string locationId)
        {
            _selectedLocationId = locationId;
            QueueRedraw();
        }
    }
}

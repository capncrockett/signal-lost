using Godot;
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
        public new void SetVisible(bool visible)
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
        public new bool IsVisible()
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

                // Determine location color based on type
                Color color = LocationColor;

                switch (location.LocationType)
                {
                    case "shelter":
                        color = new Color(0.0f, 0.7f, 0.0f, 1.0f); // Green for shelters
                        break;
                    case "danger":
                        color = new Color(0.8f, 0.0f, 0.0f, 1.0f); // Red for dangerous areas
                        break;
                    case "signal_source":
                        color = new Color(0.0f, 0.5f, 0.8f, 1.0f); // Blue for signal sources
                        break;
                    default:
                        color = LocationColor; // Default color for standard locations
                        break;
                }

                if (location.Id == currentLocationId)
                {
                    // Current location
                    color = HighlightColor;
                    size *= 1.2f; // Make current location slightly larger
                }
                else if (location.Id == _selectedLocationId)
                {
                    // Selected location
                    color = new Color(0.9f, 0.9f, 0.2f, 1.0f); // Bright yellow for selected
                }

                // Draw location marker based on type
                switch (location.LocationType)
                {
                    case "shelter":
                        // Draw a house/shelter icon
                        DrawRect(new Rect2(pos.X - size, pos.Y - size, size * 2, size * 2), color);
                        DrawRect(new Rect2(pos.X - size * 0.8f, pos.Y - size * 0.5f, size * 1.6f, size), color.Darkened(0.3f));
                        break;
                    case "danger":
                        // Draw a warning triangle
                        Vector2[] trianglePoints = {
                            new Vector2(pos.X, pos.Y - size),
                            new Vector2(pos.X - size, pos.Y + size),
                            new Vector2(pos.X + size, pos.Y + size)
                        };
                        DrawPolygon(trianglePoints, new Color[] { color });
                        break;
                    case "signal_source":
                        // Draw a radio tower icon
                        DrawRect(new Rect2(pos.X - size * 0.2f, pos.Y - size, size * 0.4f, size * 2), color);
                        DrawLine(new Vector2(pos.X - size, pos.Y - size * 0.5f),
                                new Vector2(pos.X + size, pos.Y - size * 0.5f), color, 2);
                        DrawLine(new Vector2(pos.X - size * 0.7f, pos.Y - size * 0.8f),
                                new Vector2(pos.X + size * 0.7f, pos.Y - size * 0.8f), color, 2);
                        break;
                    default:
                        // Draw a circle for standard locations
                        DrawCircle(pos, size, color);
                        break;
                }

                // Draw location name
                DrawString(ThemeDB.FallbackFont, pos + new Vector2(0, size + 10),
                    location.Name, HorizontalAlignment.Center, -1, (int)(12 * ZoomLevel), TextColor);

                // Draw signal strength indicator if it's a signal source
                if (location.SignalStrength > 0.0f)
                {
                    // Draw signal waves
                    int waveCount = Mathf.CeilToInt(location.SignalStrength * 3);
                    for (int i = 1; i <= waveCount; i++)
                    {
                        float waveSize = size * (1.0f + (i * 0.4f));
                        float alpha = 1.0f - ((float)i / (waveCount + 1));
                        DrawArc(pos, waveSize, 0, Mathf.Pi * 2, 32, new Color(color.R, color.G, color.B, alpha * 0.5f), 1);
                    }
                }

                // Indicate if location requires an item
                if (!location.IsAccessible && !string.IsNullOrEmpty(location.RequiredItem))
                {
                    DrawRect(new Rect2(pos.X - size * 0.5f, pos.Y - size * 1.5f, size, size), new Color(0.8f, 0.8f, 0.0f, 0.8f));
                    DrawString(ThemeDB.FallbackFont, pos + new Vector2(0, -size * 1.2f),
                        "🔒", HorizontalAlignment.Center, -1, (int)(16 * ZoomLevel), new Color(0.0f, 0.0f, 0.0f, 1.0f));
                }

                // Store location rect for interaction
                _locationRects[location.Id] = new Rect2(pos.X - size * 1.5f, pos.Y - size * 1.5f, size * 3, size * 3);
            }
        }

        // Draw UI elements
        private void DrawUI()
        {
            Vector2 size = Size;

            // Draw title
            DrawString(ThemeDB.FallbackFont, new Vector2(20, 30),
                "SIGNAL LOST - MAP", HorizontalAlignment.Left, -1, 24, TextColor);

            // Draw legend
            float legendX = 20;
            float legendY = 70;
            float legendItemHeight = 25;

            DrawString(ThemeDB.FallbackFont, new Vector2(legendX, legendY),
                "Legend:", HorizontalAlignment.Left, -1, 16, TextColor);

            // Standard location
            DrawCircle(new Vector2(legendX + 15, legendY + legendItemHeight), 8, LocationColor);
            DrawString(ThemeDB.FallbackFont, new Vector2(legendX + 30, legendY + legendItemHeight + 5),
                "Standard Location", HorizontalAlignment.Left, -1, 12, TextColor);

            // Shelter
            DrawRect(new Rect2(legendX + 7, legendY + legendItemHeight * 2 - 8, 16, 16), new Color(0.0f, 0.7f, 0.0f, 1.0f));
            DrawString(ThemeDB.FallbackFont, new Vector2(legendX + 30, legendY + legendItemHeight * 2 + 5),
                "Shelter", HorizontalAlignment.Left, -1, 12, TextColor);

            // Danger zone
            Vector2[] trianglePoints = {
                new Vector2(legendX + 15, legendY + legendItemHeight * 3 - 8),
                new Vector2(legendX + 7, legendY + legendItemHeight * 3 + 8),
                new Vector2(legendX + 23, legendY + legendItemHeight * 3 + 8)
            };
            DrawPolygon(trianglePoints, new Color[] { new Color(0.8f, 0.0f, 0.0f, 1.0f) });
            DrawString(ThemeDB.FallbackFont, new Vector2(legendX + 30, legendY + legendItemHeight * 3 + 5),
                "Danger Zone", HorizontalAlignment.Left, -1, 12, TextColor);

            // Signal source
            DrawRect(new Rect2(legendX + 13, legendY + legendItemHeight * 4 - 8, 4, 16), new Color(0.0f, 0.5f, 0.8f, 1.0f));
            DrawString(ThemeDB.FallbackFont, new Vector2(legendX + 30, legendY + legendItemHeight * 4 + 5),
                "Signal Source", HorizontalAlignment.Left, -1, 12, TextColor);

            // Locked location
            DrawRect(new Rect2(legendX + 7, legendY + legendItemHeight * 5 - 8, 16, 16), new Color(0.8f, 0.8f, 0.0f, 0.8f));
            DrawString(ThemeDB.FallbackFont, new Vector2(legendX + 30, legendY + legendItemHeight * 5 + 5),
                "Locked Location", HorizontalAlignment.Left, -1, 12, TextColor);

            // Draw selected location info if any
            if (_selectedLocationId != null)
            {
                var location = _mapSystem.GetLocation(_selectedLocationId);

                if (location != null)
                {
                    // Draw info panel
                    float panelWidth = 300;
                    float panelHeight = 300; // Increased height for more info
                    float panelX = size.X - panelWidth - 20;
                    float panelY = 20;

                    DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight),
                        new Color(0.15f, 0.15f, 0.15f, 0.9f));

                    // Draw panel border
                    DrawRect(new Rect2(panelX, panelY, panelWidth, 2), TextColor); // Top
                    DrawRect(new Rect2(panelX, panelY + panelHeight - 2, panelWidth, 2), TextColor); // Bottom
                    DrawRect(new Rect2(panelX, panelY, 2, panelHeight), TextColor); // Left
                    DrawRect(new Rect2(panelX + panelWidth - 2, panelY, 2, panelHeight), TextColor); // Right

                    // Draw location name with type indicator
                    string typeIndicator = "";
                    switch (location.LocationType)
                    {
                        case "shelter": typeIndicator = "[SHELTER]"; break;
                        case "danger": typeIndicator = "[DANGER]"; break;
                        case "signal_source": typeIndicator = "[SIGNAL SOURCE]"; break;
                        default: typeIndicator = "[LOCATION]"; break;
                    }

                    DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, panelY + 30),
                        location.Name, HorizontalAlignment.Left, -1, 18, TextColor);
                    DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, panelY + 50),
                        typeIndicator, HorizontalAlignment.Left, -1, 14, GetLocationTypeColor(location.LocationType));

                    // Draw location description
                    DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, panelY + 80),
                        location.Description, HorizontalAlignment.Left, (int)panelWidth - 20, 14, TextColor);

                    float y = panelY + 140;

                    // Draw signal strength if applicable
                    if (location.SignalStrength > 0.0f)
                    {
                        DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, y),
                            "Signal Strength: " + (location.SignalStrength * 100).ToString("0") + "%",
                            HorizontalAlignment.Left, -1, 14, TextColor);

                        // Draw signal strength bar
                        float barWidth = 150;
                        float barHeight = 10;
                        float barX = panelX + 10;
                        float barY = y + 20;

                        // Draw background
                        DrawRect(new Rect2(barX, barY, barWidth, barHeight), new Color(0.3f, 0.3f, 0.3f, 1.0f));

                        // Draw fill
                        Color signalColor = new Color(0.0f, 0.5f, 0.8f, 1.0f);
                        if (location.SignalStrength > 0.7f) signalColor = new Color(0.0f, 0.8f, 0.0f, 1.0f);
                        else if (location.SignalStrength > 0.3f) signalColor = new Color(0.8f, 0.8f, 0.0f, 1.0f);

                        DrawRect(new Rect2(barX, barY, barWidth * location.SignalStrength, barHeight), signalColor);

                        y += 40;
                    }

                    // Draw available items if any
                    if (location.AvailableItems.Count > 0)
                    {
                        DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, y),
                            "Available Items:", HorizontalAlignment.Left, -1, 14, TextColor);

                        y += 20;
                        foreach (var item in location.AvailableItems)
                        {
                            DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 20, y),
                                "• " + FormatItemName(item), HorizontalAlignment.Left, -1, 12, TextColor);
                            y += 20;
                        }

                        y += 10;
                    }

                    // Draw connected locations
                    DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, y),
                        "Connected to:", HorizontalAlignment.Left, -1, 14, TextColor);

                    y += 20;
                    foreach (var connectedId in location.ConnectedLocations)
                    {
                        var connectedLocation = _mapSystem.GetLocation(connectedId);

                        if (connectedLocation != null)
                        {
                            if (connectedLocation.IsDiscovered)
                            {
                                DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 20, y),
                                    "• " + connectedLocation.Name, HorizontalAlignment.Left, -1, 12, TextColor);
                            }
                            else
                            {
                                DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 20, y),
                                    "• Unknown Location", HorizontalAlignment.Left, -1, 12, new Color(0.5f, 0.5f, 0.5f, 1.0f));
                            }
                            y += 20;
                        }
                    }

                    // Draw notes if any
                    if (!string.IsNullOrEmpty(location.Notes))
                    {
                        y += 10;
                        DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, y),
                            "Notes:", HorizontalAlignment.Left, -1, 14, TextColor);

                        y += 20;
                        DrawString(ThemeDB.FallbackFont, new Vector2(panelX + 10, y),
                            location.Notes, HorizontalAlignment.Left, (int)panelWidth - 20, 12, TextColor);
                    }
                }
            }

            // Draw help text
            DrawString(ThemeDB.FallbackFont, new Vector2(20, size.Y - 20),
                "Drag to pan | Scroll to zoom | Click on locations | ESC to close", HorizontalAlignment.Left, -1, 12, TextColor);
        }

        // Helper method to get color for location type
        private Color GetLocationTypeColor(string locationType)
        {
            switch (locationType)
            {
                case "shelter":
                    return new Color(0.0f, 0.7f, 0.0f, 1.0f); // Green for shelters
                case "danger":
                    return new Color(0.8f, 0.0f, 0.0f, 1.0f); // Red for dangerous areas
                case "signal_source":
                    return new Color(0.0f, 0.5f, 0.8f, 1.0f); // Blue for signal sources
                default:
                    return LocationColor; // Default color for standard locations
            }
        }

        // Helper method to format item names
        private string FormatItemName(string itemId)
        {
            // Convert snake_case to Title Case
            string[] words = itemId.Split('_');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
                }
            }
            return string.Join(" ", words);
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

        // Set MapSystem reference (for testing)
        public void SetMapSystem(MapSystem mapSystem)
        {
            _mapSystem = mapSystem;
        }

        // Set GameState reference (for testing)
        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
        }
    }
}

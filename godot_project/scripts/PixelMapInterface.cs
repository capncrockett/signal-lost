using Godot;
using System;
using System.Collections.Generic;
using SignalLost.UI;

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

        // Visual effects configuration
        [Export] public bool EnableWeatherEffects { get; set; } = true;
        [Export] public bool EnableDayNightCycle { get; set; } = true;
        [Export] public bool EnableAnimations { get; set; } = true;
        [Export] public float AnimationSpeed { get; set; } = 1.0f;

        // References to game systems
        private MapSystem _mapSystem;
        private GameState _gameState;
        private PixelFont _pixelFont;
        private PixelVisualizationManager _visualManager;

        // Visual effects state
        private float _timeOfDay = 0.5f; // 0.0 = midnight, 0.5 = noon, 1.0 = midnight
        private int _currentWeather = 0; // 0 = Clear, 1 = Cloudy, 2 = Rainy, 3 = Stormy, 4 = Foggy
        private float _weatherIntensity = 0.0f;
        private List<RainDrop> _raindrops = new List<RainDrop>();
        private List<CloudParticle> _cloudParticles = new List<CloudParticle>();
        private List<LightningBolt> _lightningBolts = new List<LightningBolt>();
        private float _fogDensity = 0.0f;
        private float _animationTimer = 0.0f;
        private float _lightningTimer = 0.0f;
        private Random _random = new Random();

        // Visual effect classes
        private class RainDrop
        {
            public Vector2 Position;
            public float Speed;
            public float Length;
            public float Alpha;

            public RainDrop(Vector2 position, float speed, float length, float alpha)
            {
                Position = position;
                Speed = speed;
                Length = length;
                Alpha = alpha;
            }
        }

        private class CloudParticle
        {
            public Vector2 Position;
            public float Size;
            public float Speed;
            public float Alpha;

            public CloudParticle(Vector2 position, float size, float speed, float alpha)
            {
                Position = position;
                Size = size;
                Speed = speed;
                Alpha = alpha;
            }
        }

        private class LightningBolt
        {
            public Vector2 Start;
            public Vector2 End;
            public float Width;
            public float Alpha;
            public float Duration;
            public float ElapsedTime;
            public List<Vector2> Points;

            public LightningBolt(Vector2 start, Vector2 end, float width, float alpha, float duration)
            {
                Start = start;
                End = end;
                Width = width;
                Alpha = alpha;
                Duration = duration;
                ElapsedTime = 0.0f;
                Points = GenerateLightningPoints(start, end, 0.3f, 4);
            }

            private List<Vector2> GenerateLightningPoints(Vector2 start, Vector2 end, float jaggedness, int iterations)
            {
                List<Vector2> points = new List<Vector2> { start };
                Vector2 direction = end - start;
                float distance = direction.Length();
                Vector2 normal = new Vector2(-direction.Y, direction.X).Normalized();

                Random random = new Random();

                // Start with a line from start to end
                List<Vector2> currentPoints = new List<Vector2> { start, end };

                // Iteratively add jaggedness
                for (int i = 0; i < iterations; i++)
                {
                    List<Vector2> newPoints = new List<Vector2>();
                    newPoints.Add(currentPoints[0]); // Always keep the start point

                    for (int j = 0; j < currentPoints.Count - 1; j++)
                    {
                        Vector2 midPoint = (currentPoints[j] + currentPoints[j + 1]) / 2;
                        float scale = distance * jaggedness * ((float)random.NextDouble() - 0.5f) * (1.0f / (i + 1));
                        midPoint += normal * scale;

                        newPoints.Add(midPoint);
                        newPoints.Add(currentPoints[j + 1]);
                    }

                    currentPoints = newPoints;
                }

                return currentPoints;
            }
        }

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
            _visualManager = GetNode<PixelVisualizationManager>("/root/PixelVisualizationManager");

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

            // Connect to visualization manager if available
            if (_visualManager != null)
            {
                _visualManager.TimeOfDayChanged += OnTimeOfDayChanged;
                _visualManager.WeatherChanged += OnWeatherChanged;
                _visualManager.WeatherTransitioning += OnWeatherTransitioning;

                // Initialize with current values
                _timeOfDay = _visualManager.GetTimeOfDay();
                _currentWeather = _visualManager.GetCurrentWeather();
                _weatherIntensity = _visualManager.GetWeatherIntensity();

                // Initialize weather effects
                InitializeWeatherEffects();
            }

            // Set up input processing
            SetProcessInput(true);
            SetProcess(true);
        }

        // Initialize weather effects based on current weather
        private void InitializeWeatherEffects()
        {
            // Clear existing effects
            _raindrops.Clear();
            _cloudParticles.Clear();
            _lightningBolts.Clear();

            // Initialize based on weather type
            switch (_currentWeather)
            {
                case 1: // Cloudy
                    InitializeClouds();
                    break;
                case 2: // Rainy
                    InitializeClouds();
                    InitializeRain();
                    break;
                case 3: // Stormy
                    InitializeClouds();
                    InitializeRain();
                    _lightningTimer = 0.0f;
                    break;
                case 4: // Foggy
                    _fogDensity = _weatherIntensity * 0.5f;
                    break;
            }
        }

        // Initialize cloud particles
        private void InitializeClouds()
        {
            int cloudCount = (int)(20 * _weatherIntensity);
            Vector2 size = Size;

            for (int i = 0; i < cloudCount; i++)
            {
                float cloudSize = 20.0f + (float)_random.NextDouble() * 40.0f;
                float cloudSpeed = 5.0f + (float)_random.NextDouble() * 10.0f;
                float cloudAlpha = 0.1f + (float)_random.NextDouble() * 0.3f;

                Vector2 position = new Vector2(
                    (float)_random.NextDouble() * size.X,
                    (float)_random.NextDouble() * size.Y * 0.5f
                );

                _cloudParticles.Add(new CloudParticle(position, cloudSize, cloudSpeed, cloudAlpha));
            }
        }

        // Initialize raindrops
        private void InitializeRain()
        {
            int rainCount = (int)(100 * _weatherIntensity);
            Vector2 size = Size;

            for (int i = 0; i < rainCount; i++)
            {
                float rainSpeed = 200.0f + (float)_random.NextDouble() * 300.0f;
                float rainLength = 10.0f + (float)_random.NextDouble() * 20.0f;
                float rainAlpha = 0.3f + (float)_random.NextDouble() * 0.5f;

                Vector2 position = new Vector2(
                    (float)_random.NextDouble() * size.X,
                    (float)_random.NextDouble() * size.Y
                );

                _raindrops.Add(new RainDrop(position, rainSpeed, rainLength, rainAlpha));
            }
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            if (!_isVisible) return;

            // Update animation timer
            _animationTimer += (float)delta * AnimationSpeed;

            // Update weather effects
            if (EnableWeatherEffects)
            {
                UpdateWeatherEffects(delta);
            }

            // Redraw
            QueueRedraw();
        }

        // Update weather effects
        private void UpdateWeatherEffects(double delta)
        {
            // Update based on weather type
            switch (_currentWeather)
            {
                case 1: // Cloudy
                    UpdateClouds(delta);
                    break;
                case 2: // Rainy
                    UpdateClouds(delta);
                    UpdateRain(delta);
                    break;
                case 3: // Stormy
                    UpdateClouds(delta);
                    UpdateRain(delta);
                    UpdateLightning(delta);
                    break;
                case 4: // Foggy
                    // Fog is static, just ensure density is correct
                    _fogDensity = _weatherIntensity * 0.5f;
                    break;
            }
        }

        // Update cloud particles
        private void UpdateClouds(double delta)
        {
            Vector2 size = Size;
            List<CloudParticle> cloudsToRemove = new List<CloudParticle>();

            foreach (var cloud in _cloudParticles)
            {
                // Move cloud
                cloud.Position = new Vector2(
                    cloud.Position.X + cloud.Speed * (float)delta,
                    cloud.Position.Y
                );

                // Check if cloud is off-screen
                if (cloud.Position.X > size.X + cloud.Size)
                {
                    cloudsToRemove.Add(cloud);
                }
            }

            // Remove clouds that are off-screen
            foreach (var cloud in cloudsToRemove)
            {
                _cloudParticles.Remove(cloud);
            }

            // Add new clouds if needed
            if (_cloudParticles.Count < 20 * _weatherIntensity)
            {
                float cloudSize = 20.0f + (float)_random.NextDouble() * 40.0f;
                float cloudSpeed = 5.0f + (float)_random.NextDouble() * 10.0f;
                float cloudAlpha = 0.1f + (float)_random.NextDouble() * 0.3f;

                Vector2 position = new Vector2(
                    -cloudSize,
                    (float)_random.NextDouble() * size.Y * 0.5f
                );

                _cloudParticles.Add(new CloudParticle(position, cloudSize, cloudSpeed, cloudAlpha));
            }
        }

        // Update raindrops
        private void UpdateRain(double delta)
        {
            Vector2 size = Size;
            List<RainDrop> dropsToRemove = new List<RainDrop>();

            foreach (var drop in _raindrops)
            {
                // Move raindrop
                drop.Position = new Vector2(
                    drop.Position.X + drop.Speed * 0.1f * (float)delta,
                    drop.Position.Y + drop.Speed * (float)delta
                );

                // Check if raindrop is off-screen
                if (drop.Position.Y > size.Y || drop.Position.X > size.X)
                {
                    dropsToRemove.Add(drop);
                }
            }

            // Remove raindrops that are off-screen
            foreach (var drop in dropsToRemove)
            {
                _raindrops.Remove(drop);
            }

            // Add new raindrops if needed
            if (_raindrops.Count < 100 * _weatherIntensity)
            {
                float rainSpeed = 200.0f + (float)_random.NextDouble() * 300.0f;
                float rainLength = 10.0f + (float)_random.NextDouble() * 20.0f;
                float rainAlpha = 0.3f + (float)_random.NextDouble() * 0.5f;

                Vector2 position = new Vector2(
                    (float)_random.NextDouble() * size.X,
                    -rainLength
                );

                _raindrops.Add(new RainDrop(position, rainSpeed, rainLength, rainAlpha));
            }
        }

        // Update lightning effects
        private void UpdateLightning(double delta)
        {
            // Update existing lightning bolts
            List<LightningBolt> boltsToRemove = new List<LightningBolt>();

            foreach (var bolt in _lightningBolts)
            {
                bolt.ElapsedTime += (float)delta;

                if (bolt.ElapsedTime >= bolt.Duration)
                {
                    boltsToRemove.Add(bolt);
                }
                else
                {
                    // Update alpha based on time
                    float progress = bolt.ElapsedTime / bolt.Duration;
                    if (progress < 0.2f)
                    {
                        // Fade in
                        bolt.Alpha = progress / 0.2f;
                    }
                    else
                    {
                        // Fade out
                        bolt.Alpha = 1.0f - ((progress - 0.2f) / 0.8f);
                    }
                }
            }

            // Remove expired lightning bolts
            foreach (var bolt in boltsToRemove)
            {
                _lightningBolts.Remove(bolt);
            }

            // Create new lightning bolts randomly
            _lightningTimer += (float)delta;

            if (_lightningTimer > 2.0f + (1.0f - _weatherIntensity) * 8.0f)
            {
                _lightningTimer = 0.0f;

                if (_random.NextDouble() < _weatherIntensity * 0.5f)
                {
                    CreateLightningBolt();
                }
            }
        }

        // Create a new lightning bolt
        private void CreateLightningBolt()
        {
            Vector2 size = Size;

            // Random start position at top of screen
            Vector2 start = new Vector2(
                (float)_random.NextDouble() * size.X,
                0
            );

            // Random end position at bottom of screen
            Vector2 end = new Vector2(
                start.X + ((float)_random.NextDouble() * 200.0f - 100.0f),
                size.Y
            );

            // Create lightning bolt
            _lightningBolts.Add(new LightningBolt(start, end, 2.0f, 1.0f, 0.5f));
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

            // Get ambient light color based on time of day
            Color ambientColor = GetAmbientLightColor();
            float ambientIntensity = GetAmbientLightIntensity();

            // Apply ambient light to background color
            Color adjustedBackgroundColor = BackgroundColor * ambientColor * ambientIntensity;

            // Draw background
            DrawRect(new Rect2(0, 0, size.X, size.Y), adjustedBackgroundColor);

            // Draw grid
            if (ShowGrid)
            {
                DrawGrid(ambientColor, ambientIntensity);
            }

            // Draw weather effects (background layer)
            if (EnableWeatherEffects)
            {
                DrawWeatherBackground(ambientColor, ambientIntensity);
            }

            // Draw connections between locations
            DrawConnections(ambientColor, ambientIntensity);

            // Draw locations
            DrawLocations(ambientColor, ambientIntensity);

            // Draw weather effects (foreground layer)
            if (EnableWeatherEffects)
            {
                DrawWeatherForeground(ambientColor, ambientIntensity);
            }

            // Draw UI elements (title, info panel, etc.)
            DrawUI();
        }

        // Draw the grid
        private void DrawGrid(Color ambientColor, float ambientIntensity)
        {
            Vector2 size = Size;
            float scaledGridSize = GridSize * ZoomLevel;

            // Apply ambient light to grid color
            Color adjustedGridColor = GridColor * ambientColor * ambientIntensity;

            // Calculate grid offset based on map offset
            float offsetX = (MapOffset.X * ZoomLevel) % scaledGridSize;
            float offsetY = (MapOffset.Y * ZoomLevel) % scaledGridSize;

            // Draw vertical grid lines
            for (float x = offsetX; x < size.X; x += scaledGridSize)
            {
                DrawLine(new Vector2(x, 0), new Vector2(x, size.Y), adjustedGridColor, 1);
            }

            // Draw horizontal grid lines
            for (float y = offsetY; y < size.Y; y += scaledGridSize)
            {
                DrawLine(new Vector2(0, y), new Vector2(size.X, y), adjustedGridColor, 1);
            }
        }

        // Draw connections between locations
        private void DrawConnections(Color ambientColor, float ambientIntensity)
        {
            var locations = _mapSystem.GetAllLocations();

            // Apply ambient light to connection color
            Color adjustedConnectionColor = ConnectionColor * ambientColor * ambientIntensity;

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
                    DrawLine(startPos, endPos, adjustedConnectionColor, 2);

                    // Add animated pulse effect along the connection
                    if (EnableAnimations)
                    {
                        float distance = (endPos - startPos).Length();
                        Vector2 direction = (endPos - startPos).Normalized();

                        // Calculate pulse position based on animation timer
                        float pulsePos = (_animationTimer % 2.0f) / 2.0f; // 0.0 to 1.0

                        // Draw pulse if it's within the line segment
                        if (pulsePos <= 1.0f)
                        {
                            Vector2 pulseCenter = startPos + direction * distance * pulsePos;
                            float pulseSize = 4.0f * ZoomLevel;

                            // Draw pulse circle
                            Color pulseColor = adjustedConnectionColor;
                            pulseColor.A = 0.7f * (1.0f - Mathf.Abs(pulsePos - 0.5f) * 2.0f); // Fade at start and end

                            DrawCircle(pulseCenter, pulseSize, pulseColor);
                        }
                    }
                }
            }
        }

        // Draw locations
        private void DrawLocations(Color ambientColor, float ambientIntensity)
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

                // Apply ambient light to location color
                Color adjustedColor = color * ambientColor * ambientIntensity;

                // Add pulsing effect for current location
                if (location.Id == currentLocationId && EnableAnimations)
                {
                    float pulse = 0.7f + 0.3f * Mathf.Sin(_animationTimer * 3.0f);
                    adjustedColor = adjustedColor * pulse;
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

        // Draw weather effects (background layer)
        private void DrawWeatherBackground(Color ambientColor, float ambientIntensity)
        {
            Vector2 size = Size;

            // Draw clouds
            if (_currentWeather >= 1) // Cloudy, Rainy, or Stormy
            {
                foreach (var cloud in _cloudParticles)
                {
                    // Draw cloud as a soft circle
                    Color cloudColor = new Color(0.9f, 0.9f, 0.9f, cloud.Alpha * ambientIntensity);
                    DrawCircle(cloud.Position, cloud.Size, cloudColor);

                    // Add some detail to the cloud
                    for (int i = 0; i < 3; i++)
                    {
                        float offset = cloud.Size * 0.4f;
                        Vector2 detailPos = cloud.Position + new Vector2(
                            (float)_random.NextDouble() * offset - offset/2,
                            (float)_random.NextDouble() * offset - offset/2
                        );
                        DrawCircle(detailPos, cloud.Size * 0.6f, cloudColor);
                    }
                }
            }

            // Draw fog overlay
            if (_currentWeather == 4) // Foggy
            {
                // Draw semi-transparent fog overlay
                Color fogColor = new Color(0.8f, 0.8f, 0.8f, _fogDensity * 0.5f);
                DrawRect(new Rect2(0, 0, size.X, size.Y), fogColor);

                // Add some fog particles for texture
                int fogParticleCount = (int)(50 * _weatherIntensity);
                for (int i = 0; i < fogParticleCount; i++)
                {
                    float x = (float)_random.NextDouble() * size.X;
                    float y = (float)_random.NextDouble() * size.Y;
                    float fogSize = 20.0f + (float)_random.NextDouble() * 40.0f;
                    float fogAlpha = 0.05f + (float)_random.NextDouble() * 0.1f;

                    Color particleColor = new Color(0.9f, 0.9f, 0.9f, fogAlpha);
                    DrawCircle(new Vector2(x, y), fogSize, particleColor);
                }
            }
        }

        // Draw weather effects (foreground layer)
        private void DrawWeatherForeground(Color ambientColor, float ambientIntensity)
        {
            // Draw rain
            if (_currentWeather >= 2) // Rainy or Stormy
            {
                foreach (var drop in _raindrops)
                {
                    // Draw raindrop as a line
                    Vector2 start = drop.Position;
                    Vector2 end = new Vector2(
                        start.X - drop.Length * 0.2f,
                        start.Y - drop.Length
                    );

                    Color rainColor = new Color(0.7f, 0.7f, 0.9f, drop.Alpha * ambientIntensity);
                    DrawLine(start, end, rainColor, 1);
                }
            }

            // Draw lightning
            if (_currentWeather == 3) // Stormy
            {
                foreach (var bolt in _lightningBolts)
                {
                    // Draw lightning as a series of connected lines
                    Color lightningColor = new Color(0.9f, 0.9f, 1.0f, bolt.Alpha);

                    for (int i = 0; i < bolt.Points.Count - 1; i++)
                    {
                        DrawLine(bolt.Points[i], bolt.Points[i + 1], lightningColor, bolt.Width);
                    }

                    // Add a glow effect
                    if (bolt.Alpha > 0.5f)
                    {
                        // Flash the entire screen briefly
                        Color flashColor = new Color(0.9f, 0.9f, 1.0f, bolt.Alpha * 0.2f);
                        DrawRect(new Rect2(0, 0, Size.X, Size.Y), flashColor);
                    }
                }
            }
        }

        // Get ambient light color based on time of day
        private Color GetAmbientLightColor()
        {
            if (_visualManager != null && EnableDayNightCycle)
            {
                return _visualManager.GetAmbientLightColor();
            }

            // Default implementation if visualization manager is not available
            if (_timeOfDay < 0.25f)
            {
                // Night to morning transition
                float t = _timeOfDay / 0.25f;
                return new Color(
                    Mathf.Lerp(0.1f, 1.0f, t),
                    Mathf.Lerp(0.1f, 0.8f, t),
                    Mathf.Lerp(0.3f, 0.6f, t),
                    1.0f
                );
            }
            else if (_timeOfDay < 0.5f)
            {
                // Morning to noon transition
                float t = (_timeOfDay - 0.25f) / 0.25f;
                return new Color(
                    Mathf.Lerp(1.0f, 1.0f, t),
                    Mathf.Lerp(0.8f, 1.0f, t),
                    Mathf.Lerp(0.6f, 1.0f, t),
                    1.0f
                );
            }
            else if (_timeOfDay < 0.75f)
            {
                // Noon to evening transition
                float t = (_timeOfDay - 0.5f) / 0.25f;
                return new Color(
                    Mathf.Lerp(1.0f, 1.0f, t),
                    Mathf.Lerp(1.0f, 0.8f, t),
                    Mathf.Lerp(1.0f, 0.6f, t),
                    1.0f
                );
            }
            else
            {
                // Evening to night transition
                float t = (_timeOfDay - 0.75f) / 0.25f;
                return new Color(
                    Mathf.Lerp(1.0f, 0.1f, t),
                    Mathf.Lerp(0.8f, 0.1f, t),
                    Mathf.Lerp(0.6f, 0.3f, t),
                    1.0f
                );
            }
        }

        // Get ambient light intensity based on time of day
        private float GetAmbientLightIntensity()
        {
            if (_visualManager != null && EnableDayNightCycle)
            {
                return _visualManager.GetAmbientLightIntensity();
            }

            // Default implementation if visualization manager is not available
            if (_timeOfDay < 0.25f)
            {
                // Night to morning transition
                float t = _timeOfDay / 0.25f;
                return Mathf.Lerp(0.2f, 0.5f, t);
            }
            else if (_timeOfDay < 0.5f)
            {
                // Morning to noon transition
                float t = (_timeOfDay - 0.25f) / 0.25f;
                return Mathf.Lerp(0.5f, 1.0f, t);
            }
            else if (_timeOfDay < 0.75f)
            {
                // Noon to evening transition
                float t = (_timeOfDay - 0.5f) / 0.25f;
                return Mathf.Lerp(1.0f, 0.5f, t);
            }
            else
            {
                // Evening to night transition
                float t = (_timeOfDay - 0.75f) / 0.25f;
                return Mathf.Lerp(0.5f, 0.2f, t);
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

        private void OnTimeOfDayChanged(float timeOfDay)
        {
            _timeOfDay = timeOfDay;
            QueueRedraw();
        }

        private void OnWeatherChanged(int weatherType, float intensity)
        {
            _currentWeather = weatherType;
            _weatherIntensity = intensity;

            // Initialize weather effects
            InitializeWeatherEffects();

            QueueRedraw();
        }

        private void OnWeatherTransitioning(int fromWeather, int toWeather, float progress)
        {
            // Handle weather transition if needed
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

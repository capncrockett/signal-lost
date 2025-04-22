using Godot;
using System;

namespace SignalLost.Field
{
    /// <summary>
    /// Manages the day/night cycle in the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class DayNightCycleManager : Node
    {
        // Day/night cycle properties
        [Export] public float DayDuration { get; set; } = 600.0f; // Duration of a full day in seconds (10 minutes)
        [Export] public float StartTime { get; set; } = 8.0f; // Starting time in 24-hour format
        [Export] public bool AutomaticCycle { get; set; } = true; // Whether the cycle advances automatically
        [Export] public bool PauseAtNight { get; set; } = false; // Whether to pause the cycle at night
        [Export] public float SunriseTime { get; set; } = 6.0f; // Time of sunrise
        [Export] public float SunsetTime { get; set; } = 18.0f; // Time of sunset

        // Visual properties
        [Export] public Color DaytimeColor { get; set; } = new Color(1.0f, 1.0f, 1.0f, 1.0f); // Full brightness
        [Export] public Color NighttimeColor { get; set; } = new Color(0.2f, 0.2f, 0.4f, 1.0f); // Dark blue tint
        [Export] public Color SunriseColor { get; set; } = new Color(1.0f, 0.8f, 0.6f, 1.0f); // Orange tint
        [Export] public Color SunsetColor { get; set; } = new Color(1.0f, 0.6f, 0.4f, 1.0f); // Red-orange tint

        // State
        private float _currentTime; // Current time in 24-hour format
        private bool _isDaytime; // Whether it's currently daytime
        private float _elapsedTime; // Elapsed time since start
        private CanvasModulate _canvasModulate; // For tinting the scene
        private SignalLost.GameState _gameState; // Reference to the game state

        // Signals
        [Signal] public delegate void TimeChangedEventHandler(float time);
        [Signal] public delegate void DayNightChangedEventHandler(bool isDaytime);
        [Signal] public delegate void SunriseEventHandler();
        [Signal] public delegate void SunsetEventHandler();

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Initialize time
            _currentTime = StartTime;
            _isDaytime = _currentTime >= SunriseTime && _currentTime <= SunsetTime;
            _elapsedTime = 0.0f;

            // Get references
            _gameState = GetNode<SignalLost.GameState>("/root/GameState");
            
            // Create canvas modulate for tinting if it doesn't exist
            _canvasModulate = GetNodeOrNull<CanvasModulate>("CanvasModulate");
            if (_canvasModulate == null)
            {
                _canvasModulate = new CanvasModulate();
                _canvasModulate.Name = "CanvasModulate";
                AddChild(_canvasModulate);
            }

            // Set initial color
            UpdateVisuals();

            // Update game state
            if (_gameState != null)
            {
                _gameState.SetTimeOfDay(_currentTime);
            }

            GD.Print($"DayNightCycleManager: Initialized at time {_currentTime}, is daytime: {_isDaytime}");
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="delta">Time since the last frame</param>
        public override void _Process(double delta)
        {
            if (!AutomaticCycle)
                return;

            // Don't advance time if paused at night and it's night
            if (PauseAtNight && !_isDaytime)
                return;

            // Update elapsed time
            _elapsedTime += (float)delta;

            // Calculate new time
            float previousTime = _currentTime;
            _currentTime = (StartTime + (_elapsedTime / DayDuration) * 24.0f) % 24.0f;

            // Check for day/night transition
            bool wasDaytime = _isDaytime;
            _isDaytime = _currentTime >= SunriseTime && _currentTime <= SunsetTime;

            // Check for sunrise/sunset events
            if (previousTime < SunriseTime && _currentTime >= SunriseTime)
            {
                EmitSignal(SignalName.Sunrise);
                GD.Print("DayNightCycleManager: Sunrise event");
            }
            else if (previousTime < SunsetTime && _currentTime >= SunsetTime)
            {
                EmitSignal(SignalName.Sunset);
                GD.Print("DayNightCycleManager: Sunset event");
            }

            // Check for day/night transition
            if (wasDaytime != _isDaytime)
            {
                EmitSignal(SignalName.DayNightChanged, _isDaytime);
                GD.Print($"DayNightCycleManager: Day/night changed to {(_isDaytime ? "day" : "night")}");
            }

            // Update visuals
            UpdateVisuals();

            // Update game state
            if (_gameState != null)
            {
                _gameState.SetTimeOfDay(_currentTime);
            }

            // Emit time changed signal
            EmitSignal(SignalName.TimeChanged, _currentTime);
        }

        /// <summary>
        /// Updates the visual appearance based on the current time.
        /// </summary>
        private void UpdateVisuals()
        {
            if (_canvasModulate == null)
                return;

            // Calculate color based on time of day
            Color targetColor;

            if (_currentTime >= SunriseTime && _currentTime < SunriseTime + 2.0f)
            {
                // Sunrise transition (2 hours)
                float t = (_currentTime - SunriseTime) / 2.0f;
                targetColor = SunriseColor.Lerp(DaytimeColor, t);
            }
            else if (_currentTime >= SunsetTime - 2.0f && _currentTime < SunsetTime)
            {
                // Sunset transition (2 hours)
                float t = (_currentTime - (SunsetTime - 2.0f)) / 2.0f;
                targetColor = DaytimeColor.Lerp(SunsetColor, t);
            }
            else if (_currentTime >= SunsetTime && _currentTime < SunsetTime + 1.0f)
            {
                // Sunset to night transition (1 hour)
                float t = (_currentTime - SunsetTime) / 1.0f;
                targetColor = SunsetColor.Lerp(NighttimeColor, t);
            }
            else if (_currentTime >= SunriseTime - 1.0f && _currentTime < SunriseTime)
            {
                // Night to sunrise transition (1 hour)
                float t = (_currentTime - (SunriseTime - 1.0f)) / 1.0f;
                targetColor = NighttimeColor.Lerp(SunriseColor, t);
            }
            else if (_isDaytime)
            {
                // Daytime
                targetColor = DaytimeColor;
            }
            else
            {
                // Nighttime
                targetColor = NighttimeColor;
            }

            // Apply color
            _canvasModulate.Color = targetColor;
        }

        /// <summary>
        /// Sets the current time.
        /// </summary>
        /// <param name="time">The new time in 24-hour format</param>
        public void SetTime(float time)
        {
            _currentTime = Mathf.Clamp(time, 0.0f, 24.0f) % 24.0f;
            _isDaytime = _currentTime >= SunriseTime && _currentTime <= SunsetTime;

            // Calculate elapsed time based on current time
            _elapsedTime = (((_currentTime - StartTime + 24.0f) % 24.0f) / 24.0f) * DayDuration;

            // Update visuals
            UpdateVisuals();

            // Update game state
            if (_gameState != null)
            {
                _gameState.SetTimeOfDay(_currentTime);
            }

            // Emit signals
            EmitSignal(SignalName.TimeChanged, _currentTime);
            EmitSignal(SignalName.DayNightChanged, _isDaytime);

            GD.Print($"DayNightCycleManager: Time set to {_currentTime}, is daytime: {_isDaytime}");
        }

        /// <summary>
        /// Gets the current time.
        /// </summary>
        /// <returns>The current time in 24-hour format</returns>
        public float GetTime()
        {
            return _currentTime;
        }

        /// <summary>
        /// Gets whether it's currently daytime.
        /// </summary>
        /// <returns>True if it's daytime, false if it's nighttime</returns>
        public bool IsDaytime()
        {
            return _isDaytime;
        }

        /// <summary>
        /// Gets the formatted time string.
        /// </summary>
        /// <returns>The time in HH:MM format</returns>
        public string GetTimeString()
        {
            int hours = (int)_currentTime;
            int minutes = (int)((_currentTime - hours) * 60);
            return $"{hours:D2}:{minutes:D2}";
        }

        /// <summary>
        /// Sets whether the cycle advances automatically.
        /// </summary>
        /// <param name="automatic">Whether the cycle advances automatically</param>
        public void SetAutomaticCycle(bool automatic)
        {
            AutomaticCycle = automatic;
        }

        /// <summary>
        /// Advances the time by the specified amount.
        /// </summary>
        /// <param name="hours">The number of hours to advance</param>
        public void AdvanceTime(float hours)
        {
            SetTime((_currentTime + hours) % 24.0f);
        }
    }
}

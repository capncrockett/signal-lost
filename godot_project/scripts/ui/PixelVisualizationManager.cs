using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost.UI
{
    /// <summary>
    /// Manages visual effects and animations for the pixel-based UI system.
    /// </summary>
    [GlobalClass]
    public partial class PixelVisualizationManager : Node
    {
        // Singleton instance
        private static PixelVisualizationManager _instance;
        public static PixelVisualizationManager Instance => _instance;

        // Visual effect settings
        [Export] public bool EnableWeatherEffects { get; set; } = true;
        [Export] public bool EnableDayNightCycle { get; set; } = true;
        [Export] public bool EnableAnimations { get; set; } = true;

        // Weather state
        private enum WeatherType { Clear, Cloudy, Rainy, Stormy, Foggy }
        private WeatherType _currentWeather = WeatherType.Clear;
        private float _weatherIntensity = 0.0f;
        private float _weatherTransitionTime = 0.0f;
        private WeatherType _targetWeather = WeatherType.Clear;

        // Day/night cycle
        private float _timeOfDay = 0.5f; // 0.0 = midnight, 0.5 = noon, 1.0 = midnight
        private float _dayLength = 600.0f; // 10 minutes per day
        private bool _isPaused = false;

        // Animation tracking
        private Dictionary<string, AnimationData> _activeAnimations = new Dictionary<string, AnimationData>();

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Set up singleton
            if (_instance != null)
            {
                QueueFree();
                return;
            }
            _instance = this;

            // Initialize
            SetProcess(true);
            GD.Print("PixelVisualizationManager initialized");
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            if (_isPaused) return;

            // Update day/night cycle
            if (EnableDayNightCycle)
            {
                UpdateDayNightCycle(delta);
            }

            // Update weather effects
            if (EnableWeatherEffects)
            {
                UpdateWeather(delta);
            }

            // Update animations
            if (EnableAnimations)
            {
                UpdateAnimations(delta);
            }
        }

        #region Day/Night Cycle

        // Update the day/night cycle
        private void UpdateDayNightCycle(double delta)
        {
            // Update time of day
            _timeOfDay += (float)(delta / _dayLength);
            if (_timeOfDay >= 1.0f)
            {
                _timeOfDay -= 1.0f;
                // Emit day completed signal
                EmitSignal(SignalName.DayCompleted);
            }

            // Emit time changed signal
            EmitSignal(SignalName.TimeOfDayChanged, _timeOfDay);
        }

        // Get the current time of day (0.0 - 1.0)
        public float GetTimeOfDay()
        {
            return _timeOfDay;
        }

        // Set the time of day (0.0 - 1.0)
        public void SetTimeOfDay(float time)
        {
            _timeOfDay = Mathf.Clamp(time, 0.0f, 1.0f);
            EmitSignal(SignalName.TimeOfDayChanged, _timeOfDay);
        }

        // Get the current ambient light color based on time of day
        public Color GetAmbientLightColor()
        {
            // Early morning (0.0 - 0.25): Dark blue to light orange
            // Day (0.25 - 0.75): Light orange to white to light orange
            // Evening (0.75 - 1.0): Light orange to dark blue

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

        // Get the current ambient light intensity based on time of day
        public float GetAmbientLightIntensity()
        {
            // Night (0.0 - 0.25): 0.2 to 0.5
            // Day (0.25 - 0.75): 0.5 to 1.0 to 0.5
            // Night (0.75 - 1.0): 0.5 to 0.2

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

        // Set the day length in seconds
        public void SetDayLength(float seconds)
        {
            _dayLength = Mathf.Max(1.0f, seconds);
        }

        // Pause/unpause the day/night cycle
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
        }

        #endregion

        #region Weather Effects

        // Update weather effects
        private void UpdateWeather(double delta)
        {
            // Handle weather transitions
            if (_currentWeather != _targetWeather)
            {
                _weatherTransitionTime += (float)delta;
                float transitionDuration = 10.0f; // 10 seconds to transition

                if (_weatherTransitionTime >= transitionDuration)
                {
                    // Transition complete
                    _currentWeather = _targetWeather;
                    _weatherTransitionTime = 0.0f;
                    EmitSignal(SignalName.WeatherChanged, (int)_currentWeather, _weatherIntensity);
                }
                else
                {
                    // Transition in progress
                    float progress = _weatherTransitionTime / transitionDuration;
                    EmitSignal(SignalName.WeatherTransitioning, (int)_currentWeather, (int)_targetWeather, progress);
                }
            }
        }

        // Set the current weather
        public void SetWeather(int weatherType, float intensity = 0.5f, bool immediate = false)
        {
            _targetWeather = (WeatherType)Mathf.Clamp(weatherType, 0, 4);
            _weatherIntensity = Mathf.Clamp(intensity, 0.0f, 1.0f);

            if (immediate)
            {
                _currentWeather = _targetWeather;
                _weatherTransitionTime = 0.0f;
                EmitSignal(SignalName.WeatherChanged, (int)_currentWeather, _weatherIntensity);
            }
            else
            {
                _weatherTransitionTime = 0.0f;
            }
        }

        // Get the current weather
        public int GetCurrentWeather()
        {
            return (int)_currentWeather;
        }

        // Get the current weather intensity
        public float GetWeatherIntensity()
        {
            return _weatherIntensity;
        }

        // Get weather effect on signal strength (0.0 - 1.0, where 1.0 is no effect)
        public float GetWeatherSignalEffect()
        {
            switch (_currentWeather)
            {
                case WeatherType.Clear:
                    return 1.0f;
                case WeatherType.Cloudy:
                    return 1.0f - (0.1f * _weatherIntensity);
                case WeatherType.Rainy:
                    return 1.0f - (0.3f * _weatherIntensity);
                case WeatherType.Stormy:
                    return 1.0f - (0.6f * _weatherIntensity);
                case WeatherType.Foggy:
                    return 1.0f - (0.2f * _weatherIntensity);
                default:
                    return 1.0f;
            }
        }

        #endregion

        #region Animations

        // Animation data structure
        private class AnimationData
        {
            public float Duration;
            public float ElapsedTime;
            public Action<float> UpdateCallback;
            public Action CompletionCallback;

            public AnimationData(float duration, Action<float> updateCallback, Action completionCallback = null)
            {
                Duration = duration;
                ElapsedTime = 0.0f;
                UpdateCallback = updateCallback;
                CompletionCallback = completionCallback;
            }
        }

        // Update all active animations
        private void UpdateAnimations(double delta)
        {
            List<string> completedAnimations = new List<string>();

            foreach (var kvp in _activeAnimations)
            {
                string id = kvp.Key;
                AnimationData animation = kvp.Value;

                // Update elapsed time
                animation.ElapsedTime += (float)delta;

                // Calculate progress (0.0 - 1.0)
                float progress = Mathf.Clamp(animation.ElapsedTime / animation.Duration, 0.0f, 1.0f);

                // Call update callback
                animation.UpdateCallback?.Invoke(progress);

                // Check if animation is complete
                if (progress >= 1.0f)
                {
                    // Call completion callback
                    animation.CompletionCallback?.Invoke();
                    completedAnimations.Add(id);
                }
            }

            // Remove completed animations
            foreach (string id in completedAnimations)
            {
                _activeAnimations.Remove(id);
            }
        }

        // Start a new animation
        public string StartAnimation(float duration, Action<float> updateCallback, Action completionCallback = null)
        {
            string id = Guid.NewGuid().ToString();
            _activeAnimations[id] = new AnimationData(duration, updateCallback, completionCallback);
            return id;
        }

        // Stop an animation
        public void StopAnimation(string id, bool triggerCompletion = false)
        {
            if (_activeAnimations.TryGetValue(id, out AnimationData animation))
            {
                if (triggerCompletion)
                {
                    animation.CompletionCallback?.Invoke();
                }
                _activeAnimations.Remove(id);
            }
        }

        // Stop all animations
        public void StopAllAnimations(bool triggerCompletion = false)
        {
            if (triggerCompletion)
            {
                foreach (var animation in _activeAnimations.Values)
                {
                    animation.CompletionCallback?.Invoke();
                }
            }
            _activeAnimations.Clear();
        }

        #endregion

        // Signals
        [Signal] public delegate void TimeOfDayChangedEventHandler(float timeOfDay);
        [Signal] public delegate void DayCompletedEventHandler();
        [Signal] public delegate void WeatherChangedEventHandler(int weatherType, float intensity);
        [Signal] public delegate void WeatherTransitioningEventHandler(int fromWeather, int toWeather, float progress);
    }
}

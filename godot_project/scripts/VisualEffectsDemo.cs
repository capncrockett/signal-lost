using Godot;
using System;
using SignalLost.UI;

namespace SignalLost
{
    public partial class VisualEffectsDemo : Control
    {
        private PixelVisualizationManager _visualManager;
        private PixelMapInterface _mapInterface;
        private HSlider _timeSlider;
        private OptionButton _weatherOptions;
        private HSlider _intensitySlider;
        private HSlider _animationSpeedSlider;
        private CheckButton _toggleAnimations;
        private CheckButton _toggleWeather;
        private CheckButton _toggleDayNight;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to nodes
            _visualManager = GetNode<PixelVisualizationManager>("PixelVisualizationManager");
            _mapInterface = GetNode<PixelMapInterface>("PixelMapInterface");
            _timeSlider = GetNode<HSlider>("Controls/TimeSlider");
            _weatherOptions = GetNode<OptionButton>("Controls/WeatherOptions");
            _intensitySlider = GetNode<HSlider>("Controls/IntensitySlider");
            _animationSpeedSlider = GetNode<HSlider>("Controls/AnimationSpeedSlider");
            _toggleAnimations = GetNode<CheckButton>("Controls/ToggleAnimations");
            _toggleWeather = GetNode<CheckButton>("Controls/ToggleWeather");
            _toggleDayNight = GetNode<CheckButton>("Controls/ToggleDayNight");

            // Initialize UI
            _timeSlider.Value = 0.5;
            _weatherOptions.Selected = 0;
            _intensitySlider.Value = 0.5;
            _animationSpeedSlider.Value = 1.0;
            _toggleAnimations.ButtonPressed = true;
            _toggleWeather.ButtonPressed = true;
            _toggleDayNight.ButtonPressed = true;

            // Initialize visualization manager
            _visualManager.SetTimeOfDay((float)_timeSlider.Value);
            _visualManager.SetWeather(_weatherOptions.Selected, (float)_intensitySlider.Value);
            _visualManager.EnableAnimations = _toggleAnimations.ButtonPressed;
            _visualManager.EnableWeatherEffects = _toggleWeather.ButtonPressed;
            _visualManager.EnableDayNightCycle = _toggleDayNight.ButtonPressed;

            // Initialize map interface
            _mapInterface.EnableAnimations = _toggleAnimations.ButtonPressed;
            _mapInterface.EnableWeatherEffects = _toggleWeather.ButtonPressed;
            _mapInterface.EnableDayNightCycle = _toggleDayNight.ButtonPressed;
            _mapInterface.AnimationSpeed = (float)_animationSpeedSlider.Value;

            // Create a sample map for demonstration
            CreateSampleMap();
        }

        // Create a sample map for demonstration
        private void CreateSampleMap()
        {
            // Get the map system
            var mapSystem = GetNode<MapSystem>("/root/MapSystem");
            if (mapSystem == null)
            {
                GD.PrintErr("VisualEffectsDemo: Failed to get MapSystem reference");
                return;
            }

            // Clear existing locations
            mapSystem.ClearLocations();

            // Add some sample locations
            mapSystem.AddLocation("location1", "Base Camp", new Vector2(0, 0), "shelter");
            mapSystem.AddLocation("location2", "Forest", new Vector2(100, -50), "standard");
            mapSystem.AddLocation("location3", "Mountain", new Vector2(200, -100), "standard");
            mapSystem.AddLocation("location4", "Cave", new Vector2(150, 50), "danger");
            mapSystem.AddLocation("location5", "Radio Tower", new Vector2(50, 100), "signal_source");
            mapSystem.AddLocation("location6", "Lake", new Vector2(-50, 50), "standard");
            mapSystem.AddLocation("location7", "Abandoned Building", new Vector2(-100, -50), "danger");
            mapSystem.AddLocation("location8", "Signal Source", new Vector2(-150, -100), "signal_source");

            // Add connections between locations
            mapSystem.AddConnection("location1", "location2");
            mapSystem.AddConnection("location1", "location5");
            mapSystem.AddConnection("location1", "location6");
            mapSystem.AddConnection("location2", "location3");
            mapSystem.AddConnection("location2", "location4");
            mapSystem.AddConnection("location3", "location4");
            mapSystem.AddConnection("location5", "location4");
            mapSystem.AddConnection("location5", "location6");
            mapSystem.AddConnection("location6", "location7");
            mapSystem.AddConnection("location7", "location8");

            // Discover all locations for demonstration
            mapSystem.DiscoverLocation("location1");
            mapSystem.DiscoverLocation("location2");
            mapSystem.DiscoverLocation("location3");
            mapSystem.DiscoverLocation("location4");
            mapSystem.DiscoverLocation("location5");
            mapSystem.DiscoverLocation("location6");
            mapSystem.DiscoverLocation("location7");
            mapSystem.DiscoverLocation("location8");

            // Set current location
            var gameState = GetNode<GameState>("/root/GameState");
            if (gameState != null)
            {
                gameState.SetCurrentLocation("location1");
            }
        }

        // Time slider value changed
        private void _on_time_slider_value_changed(double value)
        {
            if (_visualManager != null)
            {
                _visualManager.SetTimeOfDay((float)value);
            }
        }

        // Weather option selected
        private void _on_weather_options_item_selected(long index)
        {
            if (_visualManager != null)
            {
                _visualManager.SetWeather((int)index, (float)_intensitySlider.Value);
            }
        }

        // Intensity slider value changed
        private void _on_intensity_slider_value_changed(double value)
        {
            if (_visualManager != null && _weatherOptions != null)
            {
                _visualManager.SetWeather(_weatherOptions.Selected, (float)value);
            }
        }

        // Animation speed slider value changed
        private void _on_animation_speed_slider_value_changed(double value)
        {
            if (_mapInterface != null)
            {
                _mapInterface.AnimationSpeed = (float)value;
            }
        }

        // Toggle animations
        private void _on_toggle_animations_toggled(bool toggled)
        {
            if (_visualManager != null)
            {
                _visualManager.EnableAnimations = toggled;
            }

            if (_mapInterface != null)
            {
                _mapInterface.EnableAnimations = toggled;
            }
        }

        // Toggle weather effects
        private void _on_toggle_weather_toggled(bool toggled)
        {
            if (_visualManager != null)
            {
                _visualManager.EnableWeatherEffects = toggled;
            }

            if (_mapInterface != null)
            {
                _mapInterface.EnableWeatherEffects = toggled;
            }
        }

        // Toggle day/night cycle
        private void _on_toggle_day_night_toggled(bool toggled)
        {
            if (_visualManager != null)
            {
                _visualManager.EnableDayNightCycle = toggled;
            }

            if (_mapInterface != null)
            {
                _mapInterface.EnableDayNightCycle = toggled;
            }
        }
    }
}

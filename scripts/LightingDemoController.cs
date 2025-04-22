using Godot;
using System;
using SignalLost.UI;

namespace SignalLost
{
    /// <summary>
    /// Controller for the dynamic lighting demo scene.
    /// </summary>
    [GlobalClass]
    public partial class LightingDemoController : Control
    {
        // References
        private DynamicLightingManager _lightingManager;
        private PixelVisualizationManager _visualManager;
        
        // UI elements
        private ColorPickerButton _lightColorPicker;
        private HSlider _intensitySlider;
        private HSlider _radiusSlider;
        private OptionButton _lightTypeOption;
        private Button _addLightButton;
        private Button _clearLightsButton;
        
        // Light tracking
        private int _selectedLightId = -1;
        private System.Collections.Generic.List<int> _lightIds = new System.Collections.Generic.List<int>();
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references
            _lightingManager = GetNode<DynamicLightingManager>("/root/DynamicLightingManager");
            _visualManager = GetNode<PixelVisualizationManager>("/root/PixelVisualizationManager");
            
            // Get UI elements
            _lightColorPicker = GetNode<ColorPickerButton>("Controls/LightColorPicker");
            _intensitySlider = GetNode<HSlider>("Controls/IntensitySlider");
            _radiusSlider = GetNode<HSlider>("Controls/RadiusSlider");
            _lightTypeOption = GetNode<OptionButton>("Controls/LightTypeOption");
            _addLightButton = GetNode<Button>("Controls/AddLightButton");
            _clearLightsButton = GetNode<Button>("Controls/ClearLightsButton");
            
            // Set up UI
            _lightColorPicker.Color = new Color(1.0f, 0.8f, 0.2f, 1.0f);
            _intensitySlider.Value = 0.8;
            _radiusSlider.Value = 100.0;
            
            // Add light types
            _lightTypeOption.AddItem("Normal Light");
            _lightTypeOption.AddItem("Flickering Light");
            _lightTypeOption.AddItem("Pulsing Light");
            _lightTypeOption.AddItem("Temporary Light");
            _lightTypeOption.Selected = 0;
            
            // Connect signals
            _addLightButton.Pressed += OnAddLightPressed;
            _clearLightsButton.Pressed += OnClearLightsPressed;
            
            // Create some initial lights
            CreateDemoLights();
        }
        
        // Called every frame
        public override void _Process(double delta)
        {
            // Update UI based on current state
            UpdateUI();
        }
        
        // Create some demo lights
        private void CreateDemoLights()
        {
            if (_lightingManager == null) return;
            
            // Clear existing lights
            _lightingManager.ClearLights();
            _lightIds.Clear();
            
            // Add ambient light in the center
            int id = _lightingManager.AddLight(new Vector2(GetViewportRect().Size.X / 2, GetViewportRect().Size.Y / 2), 
                200.0f, new Color(0.2f, 0.2f, 0.8f, 1.0f), 0.5f);
            _lightIds.Add(id);
            
            // Add flickering light (like a torch)
            id = _lightingManager.AddFlickeringLight(new Vector2(100, 100), 
                150.0f, new Color(1.0f, 0.6f, 0.2f, 1.0f), 0.7f, 0.1f);
            _lightIds.Add(id);
            
            // Add pulsing light (like a beacon)
            id = _lightingManager.AddPulsingLight(new Vector2(GetViewportRect().Size.X - 100, 100), 
                120.0f, new Color(1.0f, 0.2f, 0.2f, 1.0f), 0.6f, 0.5f);
            _lightIds.Add(id);
        }
        
        // Update UI based on current state
        private void UpdateUI()
        {
            // Update button states
            _addLightButton.Disabled = _lightingManager == null;
            _clearLightsButton.Disabled = _lightingManager == null || _lightIds.Count == 0;
        }
        
        // Handle mouse input
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    // Left click to add a light at the mouse position
                    if (Input.IsKeyPressed(Key.Shift))
                    {
                        AddLightAtPosition(GetViewport().GetMousePosition());
                    }
                }
                else if (mouseEvent.ButtonIndex == MouseButton.Right)
                {
                    // Right click to remove the nearest light
                    RemoveNearestLight(GetViewport().GetMousePosition());
                }
            }
        }
        
        // Add a light at the specified position
        private void AddLightAtPosition(Vector2 position)
        {
            if (_lightingManager == null) return;
            
            // Get light properties from UI
            Color color = _lightColorPicker.Color;
            float intensity = (float)_intensitySlider.Value;
            float radius = (float)_radiusSlider.Value;
            int lightType = _lightTypeOption.Selected;
            
            // Add the appropriate type of light
            int id = -1;
            switch (lightType)
            {
                case 0: // Normal light
                    id = _lightingManager.AddLight(position, radius, color, intensity);
                    break;
                case 1: // Flickering light
                    id = _lightingManager.AddFlickeringLight(position, radius, color, intensity, 0.1f);
                    break;
                case 2: // Pulsing light
                    id = _lightingManager.AddPulsingLight(position, radius, color, intensity, 0.5f);
                    break;
                case 3: // Temporary light
                    id = _lightingManager.AddTemporaryLight(position, radius, color, intensity, 3.0f);
                    break;
            }
            
            if (id >= 0)
            {
                _lightIds.Add(id);
                _selectedLightId = id;
            }
        }
        
        // Remove the nearest light to the specified position
        private void RemoveNearestLight(Vector2 position)
        {
            if (_lightingManager == null || _lightIds.Count == 0) return;
            
            // Find the nearest light
            int nearestId = -1;
            float nearestDistance = float.MaxValue;
            
            foreach (var lightSource in _lightingManager.GetLightSources())
            {
                float distance = position.DistanceTo(lightSource.Position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestId = lightSource.Id;
                }
            }
            
            // Remove the nearest light
            if (nearestId >= 0)
            {
                _lightingManager.RemoveLight(nearestId);
                _lightIds.Remove(nearestId);
                
                if (_selectedLightId == nearestId)
                {
                    _selectedLightId = -1;
                }
            }
        }
        
        // Add light button pressed
        private void OnAddLightPressed()
        {
            AddLightAtPosition(GetViewportRect().Size / 2);
        }
        
        // Clear lights button pressed
        private void OnClearLightsPressed()
        {
            if (_lightingManager == null) return;
            
            _lightingManager.ClearLights();
            _lightIds.Clear();
            _selectedLightId = -1;
        }
    }
}

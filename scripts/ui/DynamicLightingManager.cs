using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost.UI
{
    /// <summary>
    /// Manages dynamic lighting effects for the pixel-based UI system.
    /// </summary>
    [GlobalClass]
    public partial class DynamicLightingManager : Node
    {
        // Lighting settings
        [Export] public bool EnableLighting { get; set; } = true;
        [Export] public Color AmbientLightColor { get; set; } = new Color(0.1f, 0.1f, 0.2f, 1.0f);
        [Export] public float AmbientLightIntensity { get; set; } = 0.2f;
        [Export] public int MaxLights { get; set; } = 10;
        
        // Light sources
        private List<LightSource> _lightSources = new List<LightSource>();
        
        // Singleton instance
        private static DynamicLightingManager _instance;
        public static DynamicLightingManager Instance => _instance;
        
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
            GD.Print("DynamicLightingManager initialized");
        }
        
        // Process function called every frame
        public override void _Process(double delta)
        {
            if (!EnableLighting) return;
            
            // Update light sources
            for (int i = _lightSources.Count - 1; i >= 0; i--)
            {
                var light = _lightSources[i];
                
                // Update light properties
                light.ElapsedTime += (float)delta;
                
                // Handle flickering
                if (light.Flickering)
                {
                    light.FlickerTimer += (float)delta;
                    if (light.FlickerTimer >= light.FlickerInterval)
                    {
                        light.FlickerTimer = 0;
                        light.CurrentIntensity = light.Intensity * (0.7f + 0.3f * (float)GD.RandRange(0.0, 1.0));
                    }
                }
                
                // Handle pulsing
                if (light.Pulsing)
                {
                    float pulsePhase = (light.ElapsedTime * light.PulseSpeed) % (2 * Mathf.Pi);
                    light.CurrentIntensity = light.Intensity * (0.7f + 0.3f * Mathf.Sin(pulsePhase));
                }
                
                // Handle temporary lights
                if (light.Temporary)
                {
                    if (light.ElapsedTime >= light.Duration)
                    {
                        _lightSources.RemoveAt(i);
                        continue;
                    }
                    
                    // Fade in/out for temporary lights
                    float fadeInTime = Mathf.Min(0.5f, light.Duration * 0.2f);
                    float fadeOutTime = Mathf.Min(0.5f, light.Duration * 0.2f);
                    
                    if (light.ElapsedTime < fadeInTime)
                    {
                        // Fade in
                        float t = light.ElapsedTime / fadeInTime;
                        light.CurrentIntensity = light.Intensity * t;
                    }
                    else if (light.ElapsedTime > light.Duration - fadeOutTime)
                    {
                        // Fade out
                        float t = (light.Duration - light.ElapsedTime) / fadeOutTime;
                        light.CurrentIntensity = light.Intensity * t;
                    }
                    else
                    {
                        // Full intensity
                        light.CurrentIntensity = light.Intensity;
                    }
                }
            }
        }
        
        /// <summary>
        /// Adds a new light source.
        /// </summary>
        /// <param name="position">Position of the light</param>
        /// <param name="radius">Radius of the light</param>
        /// <param name="color">Color of the light</param>
        /// <param name="intensity">Intensity of the light (0.0 - 1.0)</param>
        /// <returns>ID of the light source</returns>
        public int AddLight(Vector2 position, float radius, Color color, float intensity = 1.0f)
        {
            // Limit the number of lights
            if (_lightSources.Count >= MaxLights)
            {
                // Remove the oldest light
                _lightSources.RemoveAt(0);
            }
            
            // Create a new light source
            var light = new LightSource
            {
                Position = position,
                Radius = radius,
                Color = color,
                Intensity = intensity,
                CurrentIntensity = intensity,
                Id = GetNextLightId()
            };
            
            _lightSources.Add(light);
            return light.Id;
        }
        
        /// <summary>
        /// Adds a temporary light source that will fade out after a duration.
        /// </summary>
        /// <param name="position">Position of the light</param>
        /// <param name="radius">Radius of the light</param>
        /// <param name="color">Color of the light</param>
        /// <param name="intensity">Intensity of the light (0.0 - 1.0)</param>
        /// <param name="duration">Duration in seconds</param>
        /// <returns>ID of the light source</returns>
        public int AddTemporaryLight(Vector2 position, float radius, Color color, float intensity = 1.0f, float duration = 2.0f)
        {
            int id = AddLight(position, radius, color, intensity);
            var light = GetLightById(id);
            if (light != null)
            {
                light.Temporary = true;
                light.Duration = duration;
            }
            return id;
        }
        
        /// <summary>
        /// Adds a flickering light source.
        /// </summary>
        /// <param name="position">Position of the light</param>
        /// <param name="radius">Radius of the light</param>
        /// <param name="color">Color of the light</param>
        /// <param name="intensity">Intensity of the light (0.0 - 1.0)</param>
        /// <param name="flickerInterval">Interval between flickers in seconds</param>
        /// <returns>ID of the light source</returns>
        public int AddFlickeringLight(Vector2 position, float radius, Color color, float intensity = 1.0f, float flickerInterval = 0.1f)
        {
            int id = AddLight(position, radius, color, intensity);
            var light = GetLightById(id);
            if (light != null)
            {
                light.Flickering = true;
                light.FlickerInterval = flickerInterval;
            }
            return id;
        }
        
        /// <summary>
        /// Adds a pulsing light source.
        /// </summary>
        /// <param name="position">Position of the light</param>
        /// <param name="radius">Radius of the light</param>
        /// <param name="color">Color of the light</param>
        /// <param name="intensity">Intensity of the light (0.0 - 1.0)</param>
        /// <param name="pulseSpeed">Speed of the pulse (cycles per second)</param>
        /// <returns>ID of the light source</returns>
        public int AddPulsingLight(Vector2 position, float radius, Color color, float intensity = 1.0f, float pulseSpeed = 1.0f)
        {
            int id = AddLight(position, radius, color, intensity);
            var light = GetLightById(id);
            if (light != null)
            {
                light.Pulsing = true;
                light.PulseSpeed = pulseSpeed;
            }
            return id;
        }
        
        /// <summary>
        /// Removes a light source.
        /// </summary>
        /// <param name="id">ID of the light source</param>
        public void RemoveLight(int id)
        {
            for (int i = 0; i < _lightSources.Count; i++)
            {
                if (_lightSources[i].Id == id)
                {
                    _lightSources.RemoveAt(i);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Updates a light source's position.
        /// </summary>
        /// <param name="id">ID of the light source</param>
        /// <param name="position">New position</param>
        public void UpdateLightPosition(int id, Vector2 position)
        {
            var light = GetLightById(id);
            if (light != null)
            {
                light.Position = position;
            }
        }
        
        /// <summary>
        /// Updates a light source's properties.
        /// </summary>
        /// <param name="id">ID of the light source</param>
        /// <param name="radius">New radius</param>
        /// <param name="color">New color</param>
        /// <param name="intensity">New intensity</param>
        public void UpdateLightProperties(int id, float radius, Color color, float intensity)
        {
            var light = GetLightById(id);
            if (light != null)
            {
                light.Radius = radius;
                light.Color = color;
                light.Intensity = intensity;
                light.CurrentIntensity = intensity;
            }
        }
        
        /// <summary>
        /// Gets all light sources.
        /// </summary>
        /// <returns>List of light sources</returns>
        public List<LightSource> GetLightSources()
        {
            return _lightSources;
        }
        
        /// <summary>
        /// Gets a light source by ID.
        /// </summary>
        /// <param name="id">ID of the light source</param>
        /// <returns>Light source or null if not found</returns>
        private LightSource GetLightById(int id)
        {
            foreach (var light in _lightSources)
            {
                if (light.Id == id)
                {
                    return light;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Gets the next available light ID.
        /// </summary>
        /// <returns>Next light ID</returns>
        private int GetNextLightId()
        {
            int maxId = 0;
            foreach (var light in _lightSources)
            {
                maxId = Mathf.Max(maxId, light.Id);
            }
            return maxId + 1;
        }
        
        /// <summary>
        /// Calculates the lighting at a specific position.
        /// </summary>
        /// <param name="position">Position to calculate lighting for</param>
        /// <returns>Lighting color and intensity at the position</returns>
        public (Color color, float intensity) CalculateLightingAt(Vector2 position)
        {
            if (!EnableLighting || _lightSources.Count == 0)
            {
                return (AmbientLightColor, AmbientLightIntensity);
            }
            
            // Start with ambient light
            float r = AmbientLightColor.R * AmbientLightIntensity;
            float g = AmbientLightColor.G * AmbientLightIntensity;
            float b = AmbientLightColor.B * AmbientLightIntensity;
            float totalIntensity = AmbientLightIntensity;
            
            // Add contribution from each light source
            foreach (var light in _lightSources)
            {
                float distance = position.DistanceTo(light.Position);
                if (distance <= light.Radius)
                {
                    // Calculate falloff (1.0 at center, 0.0 at radius)
                    float falloff = 1.0f - (distance / light.Radius);
                    falloff = Mathf.Pow(falloff, 2); // Quadratic falloff for more realistic lighting
                    
                    // Calculate contribution
                    float contribution = falloff * light.CurrentIntensity;
                    
                    // Add to total
                    r += light.Color.R * contribution;
                    g += light.Color.G * contribution;
                    b += light.Color.B * contribution;
                    totalIntensity += contribution;
                }
            }
            
            // Clamp values
            r = Mathf.Clamp(r, 0.0f, 1.0f);
            g = Mathf.Clamp(g, 0.0f, 1.0f);
            b = Mathf.Clamp(b, 0.0f, 1.0f);
            totalIntensity = Mathf.Clamp(totalIntensity, 0.0f, 1.0f);
            
            return (new Color(r, g, b, 1.0f), totalIntensity);
        }
        
        /// <summary>
        /// Clears all light sources.
        /// </summary>
        public void ClearLights()
        {
            _lightSources.Clear();
        }
    }
    
    /// <summary>
    /// Represents a light source in the scene.
    /// </summary>
    public class LightSource
    {
        public int Id { get; set; }
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public Color Color { get; set; }
        public float Intensity { get; set; }
        public float CurrentIntensity { get; set; }
        public bool Temporary { get; set; }
        public float Duration { get; set; }
        public float ElapsedTime { get; set; }
        public bool Flickering { get; set; }
        public float FlickerInterval { get; set; }
        public float FlickerTimer { get; set; }
        public bool Pulsing { get; set; }
        public float PulseSpeed { get; set; }
    }
}

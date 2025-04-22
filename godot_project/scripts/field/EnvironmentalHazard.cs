using Godot;
using System;

namespace SignalLost.Field
{
    /// <summary>
    /// Represents an environmental hazard in the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class EnvironmentalHazard : Node2D
    {
        /// <summary>
        /// Types of environmental hazards.
        /// </summary>
        public enum HazardType
        {
            Weather,    // Weather-related hazards (rain, snow, fog)
            Terrain,    // Terrain-related hazards (mud, rocks, water)
            Radiation,  // Radiation-related hazards (signal interference)
            Electrical, // Electrical hazards (power lines, equipment)
            Chemical    // Chemical hazards (spills, gas)
        }

        // Hazard properties
        [Export] public string HazardId { get; set; } = "";
        [Export] public string HazardName { get; set; } = "";
        [Export] public string HazardDescription { get; set; } = "";
        [Export] public HazardType Type { get; set; } = HazardType.Weather;
        [Export] public float Intensity { get; set; } = 0.5f; // 0.0 to 1.0
        [Export] public float Range { get; set; } = 3.0f; // Range in grid cells
        [Export] public bool IsActive { get; set; } = true;
        [Export] public bool IsPermanent { get; set; } = false;
        [Export] public float Duration { get; set; } = 60.0f; // Duration in seconds (if not permanent)
        [Export] public string RequiredItemToPass { get; set; } = ""; // Item required to pass through hazard
        [Export] public float DamagePerSecond { get; set; } = 0.0f; // Damage per second to player
        [Export] public float MovementPenalty { get; set; } = 0.5f; // Movement speed multiplier (0.0 to 1.0)
        [Export] public float SignalInterference { get; set; } = 0.0f; // Signal interference (0.0 to 1.0)

        // Visual properties
        [Export] public Color HazardColor { get; set; } = new Color(1.0f, 0.0f, 0.0f, 0.5f); // Red with transparency
        [Export] public Texture2D HazardTexture { get; set; } = null;
        [Export] public bool PulsateEffect { get; set; } = false;
        [Export] public float PulsateSpeed { get; set; } = 1.0f;
        [Export] public float PulsateIntensity { get; set; } = 0.2f;

        // State
        private float _activeTime = 0.0f;
        private float _pulsateTimer = 0.0f;
        private float _currentPulsateIntensity = 0.0f;
        private bool _isPlayerInside = false;
        private PlayerController _player = null;
        private GridSystem _gridSystem = null;
        private SignalLost.GameState _gameState = null;
        private InventorySystemAdapter _inventorySystem = null;

        // Signals
        [Signal] public delegate void PlayerEnteredHazardEventHandler(string hazardId);
        [Signal] public delegate void PlayerExitedHazardEventHandler(string hazardId);
        [Signal] public delegate void HazardDeactivatedEventHandler(string hazardId);

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Add to the Hazard group for easier management
            AddToGroup("Hazard");

            // Get references to game systems
            _gameState = GetNode<SignalLost.GameState>("/root/GameState");
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystemAdapter");

            // Try to get the grid system from the scene
            var gridSystemNode = GetNodeOrNull("/root/FieldExplorationScene/GridSystem");
            if (gridSystemNode != null && gridSystemNode is GridSystem gs)
            {
                _gridSystem = gs;
            }
            else
            {
                // In tests, we might have a direct parent that's a GridSystem
                var parentGridSystem = GetParentOrNull<GridSystem>();
                if (parentGridSystem != null)
                {
                    _gridSystem = parentGridSystem;
                }
                else
                {
                    GD.PrintErr("EnvironmentalHazard: Failed to find GridSystem node");
                }
            }

            // Try to get the player from the scene
            var playerNode = GetNodeOrNull("/root/FieldExplorationScene/PlayerController");
            if (playerNode != null && playerNode is PlayerController pc)
            {
                _player = pc;
            }
            else
            {
                GD.PrintErr("EnvironmentalHazard: Failed to find PlayerController node");
            }

            // Initialize state
            _activeTime = 0.0f;
            _pulsateTimer = 0.0f;
            _currentPulsateIntensity = 0.0f;
            _isPlayerInside = false;

            GD.Print($"EnvironmentalHazard: Initialized {HazardName} at position {Position}");
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="delta">Time since the last frame</param>
        public override void _Process(double delta)
        {
            if (!IsActive)
                return;

            // Update active time
            if (!IsPermanent)
            {
                _activeTime += (float)delta;
                if (_activeTime >= Duration)
                {
                    // Deactivate hazard
                    IsActive = false;
                    EmitSignal(SignalName.HazardDeactivated, HazardId);
                    GD.Print($"EnvironmentalHazard: {HazardName} deactivated after {Duration} seconds");
                    return;
                }
            }

            // Update pulsate effect
            if (PulsateEffect)
            {
                _pulsateTimer += (float)delta * PulsateSpeed;
                _currentPulsateIntensity = Mathf.Sin(_pulsateTimer) * PulsateIntensity;
            }

            // Check if player is inside hazard
            if (_player != null && _gridSystem != null)
            {
                Vector2I playerPosition = _player.GetGridPosition();
                Vector2I hazardPosition = _gridSystem.WorldToGridPosition(Position);
                float distance = playerPosition.DistanceTo(hazardPosition);

                bool wasInside = _isPlayerInside;
                _isPlayerInside = distance <= Range;

                // Player entered hazard
                if (_isPlayerInside && !wasInside)
                {
                    OnPlayerEntered();
                }
                // Player exited hazard
                else if (!_isPlayerInside && wasInside)
                {
                    OnPlayerExited();
                }
                // Player is inside hazard
                else if (_isPlayerInside)
                {
                    OnPlayerInside((float)delta);
                }
            }

            // Draw the hazard
            QueueRedraw();
        }

        /// <summary>
        /// Called when the player enters the hazard.
        /// </summary>
        private void OnPlayerEntered()
        {
            GD.Print($"EnvironmentalHazard: Player entered {HazardName}");
            EmitSignal(SignalName.PlayerEnteredHazard, HazardId);

            // Check if player has required item to pass
            if (!string.IsNullOrEmpty(RequiredItemToPass) && _inventorySystem != null)
            {
                if (!_inventorySystem.HasItem(RequiredItemToPass))
                {
                    GD.Print($"EnvironmentalHazard: Player needs {RequiredItemToPass} to pass through {HazardName}");
                    // TODO: Show message to player
                }
            }

            // Apply immediate effects
            ApplyHazardEffects();
        }

        /// <summary>
        /// Called when the player exits the hazard.
        /// </summary>
        private void OnPlayerExited()
        {
            GD.Print($"EnvironmentalHazard: Player exited {HazardName}");
            EmitSignal(SignalName.PlayerExitedHazard, HazardId);

            // Remove hazard effects
            RemoveHazardEffects();
        }

        /// <summary>
        /// Called while the player is inside the hazard.
        /// </summary>
        /// <param name="delta">Time since the last frame</param>
        private void OnPlayerInside(float delta)
        {
            // Apply continuous effects
            if (DamagePerSecond > 0.0f && _gameState != null)
            {
                // Apply damage to player
                // TODO: Implement player health system
                GD.Print($"EnvironmentalHazard: Player taking {DamagePerSecond * delta} damage from {HazardName}");
            }
        }

        /// <summary>
        /// Applies the hazard effects to the player and game systems.
        /// </summary>
        private void ApplyHazardEffects()
        {
            // Apply movement penalty
            if (_player != null && MovementPenalty < 1.0f)
            {
                _player.SetMovementPenalty(MovementPenalty);
            }

            // Apply signal interference
            if (_gameState != null && SignalInterference > 0.0f)
            {
                _gameState.AddSignalInterference(HazardId, SignalInterference);
            }
        }

        /// <summary>
        /// Removes the hazard effects from the player and game systems.
        /// </summary>
        private void RemoveHazardEffects()
        {
            // Remove movement penalty
            if (_player != null)
            {
                _player.ResetMovementPenalty();
            }

            // Remove signal interference
            if (_gameState != null && SignalInterference > 0.0f)
            {
                _gameState.RemoveSignalInterference(HazardId);
            }
        }

        /// <summary>
        /// Custom drawing function for the hazard.
        /// </summary>
        public override void _Draw()
        {
            if (!IsActive)
                return;

            // Calculate cell size
            int cellSize = _gridSystem != null ? _gridSystem.GetCellSize() : 32;

            // Calculate hazard radius
            float radius = Range * cellSize;

            // Adjust color based on pulsate effect
            Color drawColor = HazardColor;
            if (PulsateEffect)
            {
                drawColor.A = Mathf.Clamp(drawColor.A + _currentPulsateIntensity, 0.1f, 0.9f);
            }

            // Draw hazard area
            if (HazardTexture != null)
            {
                // Draw texture
                float scale = (radius * 2) / HazardTexture.GetWidth();
                DrawTextureRect(HazardTexture, new Rect2(-radius, -radius, radius * 2, radius * 2), false, drawColor);
            }
            else
            {
                // Draw a simple shape based on hazard type
                switch (Type)
                {
                    case HazardType.Weather:
                        // Draw cloud-like shape
                        DrawCircle(Vector2.Zero, radius, new Color(drawColor, 0.3f));
                        // Draw some smaller circles for cloud effect
                        for (int i = 0; i < 5; i++)
                        {
                            float angle = i * Mathf.Pi * 2 / 5;
                            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * 0.5f;
                            DrawCircle(offset, radius * 0.3f, new Color(drawColor, 0.5f));
                        }
                        break;

                    case HazardType.Terrain:
                        // Draw irregular shape
                        DrawCircle(Vector2.Zero, radius, new Color(drawColor, 0.3f));
                        // Draw some rocks or terrain features
                        for (int i = 0; i < 8; i++)
                        {
                            float angle = i * Mathf.Pi * 2 / 8;
                            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * 0.7f;
                            DrawCircle(offset, radius * 0.15f, new Color(drawColor, 0.7f));
                        }
                        break;

                    case HazardType.Radiation:
                        // Draw radiation symbol
                        DrawCircle(Vector2.Zero, radius, new Color(drawColor, 0.3f));
                        DrawCircle(Vector2.Zero, radius * 0.8f, new Color(0, 0, 0, 0), 2.0f, true);
                        DrawCircle(Vector2.Zero, radius * 0.2f, new Color(drawColor, 0.7f));
                        // Draw radiation "blades"
                        for (int i = 0; i < 3; i++)
                        {
                            float angle = i * Mathf.Pi * 2 / 3;
                            Vector2 start = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * 0.3f;
                            Vector2 end = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * 0.7f;
                            DrawLine(start, end, new Color(drawColor, 0.7f), 3.0f);
                        }
                        break;

                    case HazardType.Electrical:
                        // Draw electrical hazard
                        DrawCircle(Vector2.Zero, radius, new Color(drawColor, 0.3f));
                        // Draw lightning bolt
                        Vector2[] lightning = new Vector2[]
                        {
                            new Vector2(0, -radius * 0.7f),
                            new Vector2(radius * 0.2f, -radius * 0.3f),
                            new Vector2(0, -radius * 0.1f),
                            new Vector2(radius * 0.3f, radius * 0.3f),
                            new Vector2(0, radius * 0.7f)
                        };
                        for (int i = 0; i < lightning.Length - 1; i++)
                        {
                            DrawLine(lightning[i], lightning[i + 1], new Color(drawColor, 0.7f), 2.0f);
                        }
                        break;

                    case HazardType.Chemical:
                        // Draw chemical hazard
                        DrawCircle(Vector2.Zero, radius, new Color(drawColor, 0.3f));
                        // Draw chemical symbol (simplified)
                        DrawCircle(Vector2.Zero, radius * 0.5f, new Color(0, 0, 0, 0), 2.0f, true);
                        // Draw some bubbles
                        for (int i = 0; i < 10; i++)
                        {
                            float angle = i * Mathf.Pi * 2 / 10;
                            float distance = radius * (0.3f + (i % 3) * 0.15f);
                            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                            DrawCircle(offset, radius * 0.1f, new Color(drawColor, 0.5f));
                        }
                        break;

                    default:
                        // Default simple circle
                        DrawCircle(Vector2.Zero, radius, new Color(drawColor, 0.5f));
                        break;
                }
            }

            // Draw hazard name if player is nearby
            if (_isPlayerInside)
            {
                DrawString(ThemeDB.FallbackFont, new Vector2(0, -radius - 20), HazardName,
                    HorizontalAlignment.Center, -1, 16, new Color(1, 1, 1, 0.8f));
            }
        }

        /// <summary>
        /// Activates the hazard.
        /// </summary>
        public void Activate()
        {
            if (!IsActive)
            {
                IsActive = true;
                _activeTime = 0.0f;
                GD.Print($"EnvironmentalHazard: {HazardName} activated");
            }
        }

        /// <summary>
        /// Deactivates the hazard.
        /// </summary>
        public void Deactivate()
        {
            if (IsActive)
            {
                IsActive = false;
                EmitSignal(SignalName.HazardDeactivated, HazardId);
                GD.Print($"EnvironmentalHazard: {HazardName} deactivated");

                // Remove effects if player is inside
                if (_isPlayerInside)
                {
                    RemoveHazardEffects();
                    _isPlayerInside = false;
                }
            }
        }

        /// <summary>
        /// Sets the intensity of the hazard.
        /// </summary>
        /// <param name="intensity">The new intensity (0.0 to 1.0)</param>
        public void SetIntensity(float intensity)
        {
            Intensity = Mathf.Clamp(intensity, 0.0f, 1.0f);
            
            // Update effects based on intensity
            DamagePerSecond = DamagePerSecond * (Intensity / 0.5f);
            MovementPenalty = Mathf.Lerp(1.0f, MovementPenalty, Intensity);
            SignalInterference = SignalInterference * Intensity;
            
            GD.Print($"EnvironmentalHazard: {HazardName} intensity set to {Intensity}");
        }
    }
}

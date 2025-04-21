using Godot;
using System.Collections.Generic;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Visualizes item effects.
    /// </summary>
    [GlobalClass]
    public partial class ItemEffectVisualizer : Node2D
    {
        // List of active effect visualizations
        private List<EffectVisualization> _activeEffects = new List<EffectVisualization>();

        // Reference to the item effects system
        private ItemEffectsSystem _itemEffectsSystem;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get reference to the item effects system
            _itemEffectsSystem = GetNode<ItemEffectsSystem>("/root/ItemEffectsSystem");

            if (_itemEffectsSystem == null)
            {
                GD.PrintErr("ItemEffectVisualizer: ItemEffectsSystem not found");
                return;
            }

            // Connect signals
            _itemEffectsSystem.ItemEffectApplied += OnItemEffectApplied;
        }

        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_itemEffectsSystem != null)
            {
                _itemEffectsSystem.ItemEffectApplied -= OnItemEffectApplied;
            }
        }

        // Called every frame
        public override void _Process(double delta)
        {
            // Update active effects
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                effect.TimeRemaining -= (float)delta;

                if (effect.TimeRemaining <= 0)
                {
                    // Remove the effect
                    _activeEffects.RemoveAt(i);
                    QueueRedraw();
                }
            }
        }

        // Draw the effects
        public override void _Draw()
        {
            // Draw active effects
            foreach (var effect in _activeEffects)
            {
                // Calculate alpha based on time remaining
                float alpha = effect.TimeRemaining / effect.Duration;
                Color color = effect.Color;
                color.A = alpha;

                // Draw the effect
                switch (effect.Type)
                {
                    case EffectType.Heal:
                        DrawHealEffect(effect.Position, color);
                        break;

                    case EffectType.Boost:
                        DrawBoostEffect(effect.Position, color);
                        break;

                    case EffectType.Unlock:
                        DrawUnlockEffect(effect.Position, color);
                        break;

                    case EffectType.Light:
                        DrawLightEffect(effect.Position, color);
                        break;

                    case EffectType.Signal:
                        DrawSignalEffect(effect.Position, color);
                        break;
                }
            }
        }

        // Handle item effect applied
        private void OnItemEffectApplied(string itemId, string effectType, float value)
        {
            // Determine effect type
            EffectType type = EffectType.Boost;
            Color color = Colors.White;

            switch (effectType)
            {
                case "health":
                    type = EffectType.Heal;
                    color = Colors.Red;
                    break;

                case "energy":
                case "hydration":
                case "power":
                    type = EffectType.Boost;
                    color = Colors.Green;
                    break;

                case "unlock_cabin":
                case "unlock_research_facility":
                case "unlock_military_outpost":
                case "unlock_mysterious_tower":
                    type = EffectType.Unlock;
                    color = Colors.Yellow;
                    break;

                case "light":
                    type = EffectType.Light;
                    color = Colors.White;
                    break;

                case "signal_detection":
                case "signal_boost":
                case "reveal_hidden_signals":
                    type = EffectType.Signal;
                    color = Colors.Cyan;
                    break;
            }

            // Add the effect
            AddEffect(type, GetViewport().GetVisibleRect().GetCenter(), color, 2.0f);
        }

        // Add an effect
        public void AddEffect(EffectType type, Vector2 position, Color color, float duration)
        {
            _activeEffects.Add(new EffectVisualization
            {
                Type = type,
                Position = position,
                Color = color,
                Duration = duration,
                TimeRemaining = duration
            });

            QueueRedraw();
        }

        // Draw a heal effect (plus sign)
        private void DrawHealEffect(Vector2 position, Color color)
        {
            float size = 40;
            float thickness = 8;

            // Draw vertical line
            DrawRect(new Rect2(position.X - thickness / 2, position.Y - size / 2, thickness, size), color);

            // Draw horizontal line
            DrawRect(new Rect2(position.X - size / 2, position.Y - thickness / 2, size, thickness), color);
        }

        // Draw a boost effect (up arrow)
        private void DrawBoostEffect(Vector2 position, Color color)
        {
            float size = 40;

            // Draw arrow
            Vector2[] points = new Vector2[]
            {
                new Vector2(position.X, position.Y - size / 2),
                new Vector2(position.X + size / 2, position.Y + size / 2),
                new Vector2(position.X - size / 2, position.Y + size / 2)
            };

            DrawPolygon(points, new Color[] { color });
        }

        // Draw an unlock effect (key)
        private void DrawUnlockEffect(Vector2 position, Color color)
        {
            float size = 30;
            float thickness = 6;

            // Draw key head (circle)
            DrawCircle(new Vector2(position.X, position.Y - size / 3), size / 3, color);

            // Draw key stem
            DrawRect(new Rect2(position.X - thickness / 2, position.Y - size / 3, thickness, size), color);

            // Draw key teeth
            DrawRect(new Rect2(position.X, position.Y + size / 3, size / 3, thickness), color);
            DrawRect(new Rect2(position.X, position.Y + size / 6, size / 4, thickness), color);
        }

        // Draw a light effect (radial gradient)
        private void DrawLightEffect(Vector2 position, Color color)
        {
            float size = 60;

            // Draw concentric circles
            for (int i = 0; i < 5; i++)
            {
                float radius = size * (1 - i / 5.0f);
                Color circleColor = color;
                circleColor.A = color.A * (1 - i / 5.0f);
                DrawCircle(position, radius, circleColor);
            }
        }

        // Draw a signal effect (radio waves)
        private void DrawSignalEffect(Vector2 position, Color color)
        {
            float size = 60;
            float thickness = 3;

            // Draw radio waves (arcs)
            for (int i = 0; i < 3; i++)
            {
                float radius = size * (i + 1) / 3.0f;
                Color arcColor = color;
                arcColor.A = color.A * (1 - i / 3.0f);

                // Draw arc
                int pointCount = 16;
                float startAngle = Mathf.Pi / 4;
                float endAngle = 3 * Mathf.Pi / 4;
                float angleStep = (endAngle - startAngle) / pointCount;

                for (int j = 0; j < pointCount; j++)
                {
                    float angle1 = startAngle + j * angleStep;
                    float angle2 = startAngle + (j + 1) * angleStep;

                    Vector2 point1 = position + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
                    Vector2 point2 = position + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;

                    DrawLine(point1, point2, arcColor, thickness);
                }
            }
        }
    }

    // Effect type
    public enum EffectType
    {
        Heal,
        Boost,
        Unlock,
        Light,
        Signal
    }

    // Effect visualization
    public class EffectVisualization
    {
        public EffectType Type { get; set; }
        public Vector2 Position { get; set; }
        public Color Color { get; set; }
        public float Duration { get; set; }
        public float TimeRemaining { get; set; }
    }
}

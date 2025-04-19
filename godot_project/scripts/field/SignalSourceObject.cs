using Godot;
using System;

namespace SignalLost.Field
{
    /// <summary>
    /// An interactable object that represents a radio signal source.
    /// </summary>
    [GlobalClass]
    public partial class SignalSourceObject : InteractableObject
    {
        // Signal properties
        [Export]
        public float Frequency { get; set; } = 90.0f;
        
        [Export]
        public string MessageId { get; set; } = "";
        
        [Export]
        public float SignalStrength { get; set; } = 1.0f;
        
        [Export]
        public float SignalRange { get; set; } = 5.0f; // Range in grid cells
        
        // References
        private GameState _gameState;
        private RadioSystem _radioSystem;
        
        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references
            _gameState = GetNode<GameState>("/root/GameState");
            if (_gameState == null)
            {
                GD.PrintErr("SignalSourceObject: Failed to find GameState node");
                return;
            }
            
            _radioSystem = _gameState.GetRadioSystem();
            if (_radioSystem == null)
            {
                GD.PrintErr("SignalSourceObject: Failed to find RadioSystem");
                return;
            }
            
            // Set default values if not specified
            if (string.IsNullOrEmpty(DisplayName))
            {
                DisplayName = "Signal Source";
            }
            
            // Register this signal source with the radio system
            _radioSystem.RegisterSignalSource(Frequency, SignalStrength, MessageId, this);
        }
        
        /// <summary>
        /// Called when the player interacts with this signal source.
        /// </summary>
        /// <returns>True if interaction was successful, false otherwise</returns>
        public override bool Interact()
        {
            if (IsOneTimeInteraction && _hasBeenInteracted)
            {
                // Already interacted with this signal source
                return false;
            }
            
            // Show message about the signal source
            var messageManager = GetNode<MessageManager>("/root/MessageManager");
            if (messageManager != null)
            {
                messageManager.ShowMessage($"Signal source detected at {Frequency:F1} MHz");
            }
            
            // Mark as interacted
            _hasBeenInteracted = true;
            
            // Emit signal
            EmitSignal(SignalName.InteractionCompleted, ObjectId);
            
            return true;
        }
        
        /// <summary>
        /// Gets the interaction prompt text for this signal source.
        /// </summary>
        /// <returns>The interaction prompt text</returns>
        public override string GetInteractionPrompt()
        {
            return $"Press E to examine {DisplayName}";
        }
        
        /// <summary>
        /// Calculates the signal strength at the specified position.
        /// </summary>
        /// <param name="position">The position to calculate signal strength for</param>
        /// <returns>The signal strength at the position (0.0 to 1.0)</returns>
        public float CalculateSignalStrengthAtPosition(Vector2I position)
        {
            // Get this object's grid position
            var gridSystem = GetNode<GridSystem>("/root/FieldExplorationScene/GridSystem");
            if (gridSystem == null)
            {
                return 0.0f;
            }
            
            Vector2I sourcePosition = gridSystem.WorldToGridPosition(GlobalPosition);
            
            // Calculate distance
            float distance = sourcePosition.DistanceTo(position);
            
            // Calculate signal strength based on distance and range
            float strength = Mathf.Max(0.0f, 1.0f - (distance / SignalRange)) * SignalStrength;
            
            return strength;
        }
        
        /// <summary>
        /// Custom drawing function for the signal source.
        /// </summary>
        public override void _Draw()
        {
            // Draw a simple signal source representation
            Color signalColor = new Color(0.2f, 0.8f, 0.2f); // Green
            
            // Draw base
            float baseSize = 20.0f;
            Vector2 basePosition = new Vector2(-baseSize / 2, -baseSize / 2);
            DrawRect(new Rect2(basePosition, new Vector2(baseSize, baseSize)), signalColor);
            
            // Draw antenna
            float antennaHeight = 16.0f;
            Vector2 antennaStart = new Vector2(0, -baseSize / 2);
            Vector2 antennaEnd = new Vector2(0, -baseSize / 2 - antennaHeight);
            DrawLine(antennaStart, antennaEnd, signalColor, 2.0f);
            
            // Draw signal waves
            Color waveColor = new Color(signalColor, 0.5f);
            float waveRadius = 8.0f;
            float waveWidth = 1.0f;
            
            for (int i = 0; i < 3; i++)
            {
                float radius = waveRadius * (i + 1);
                DrawArc(antennaEnd, radius, 0, Mathf.Pi, 16, waveColor, waveWidth);
            }
        }
    }
}

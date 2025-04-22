using Godot;

namespace SignalLost.Field
{
    /// <summary>
    /// Controls the player character in the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class PlayerController : Node2D
    {
        // Current position on grid
        private Vector2I _gridPosition = new Vector2I(1, 1);

        // References
        private GridSystem _gridSystem;
        private SignalLost.GameState _gameState;

        // Movement properties
        [Export]
        private float _moveSpeed = 4.0f; // Cells per second
        private float _baseMoveSpeed = 4.0f; // Base movement speed
        private float _movementPenalty = 1.0f; // Movement penalty multiplier (1.0 = no penalty)

        // Movement state
        private bool _isMoving = false;
        private Vector2 _targetPosition;
        private Vector2 _startPosition;
        private float _movementProgress = 0.0f;
        private Vector2I _facingDirection = new Vector2I(0, 1); // Default facing down

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Find the grid system
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
                    GD.PrintErr("PlayerController: Failed to find GridSystem node");
                    return;
                }
            }

            // Store base movement speed
            _baseMoveSpeed = _moveSpeed;

            // Find the game state
            var gameStateNode = GetNodeOrNull("/root/GameState");
            if (gameStateNode != null)
            {
                // Try to cast to our GameState type
                if (gameStateNode is SignalLost.GameState gameState)
                {
                    _gameState = gameState;
                }
                else
                {
                    GD.PrintErr("PlayerController: GameState node is not of the expected type");
                }
            }

            // Set initial position
            Position = _gridSystem.GridToWorldPosition(_gridPosition);
            _targetPosition = Position;
            _startPosition = Position;

            GD.Print($"PlayerController: Initialized at grid position {_gridPosition}");
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="delta">Time since the last frame</param>
        public override void _Process(double delta)
        {
            // Handle movement
            if (_isMoving)
            {
                _movementProgress += (float)delta * _moveSpeed;
                if (_movementProgress >= 1.0f)
                {
                    // Movement complete
                    _isMoving = false;
                    Position = _targetPosition;
                    _movementProgress = 0.0f;
                }
                else
                {
                    // Interpolate position
                    Position = _startPosition.Lerp(_targetPosition, _movementProgress);
                }
            }
            else
            {
                // Handle input
                HandleInput();
            }

            // Draw the player
            QueueRedraw();
        }

        /// <summary>
        /// Handles player input.
        /// </summary>
        private void HandleInput()
        {
            // Movement input
            Vector2I direction = Vector2I.Zero;

            if (Input.IsActionPressed("ui_up"))
            {
                direction = new Vector2I(0, -1);
            }
            else if (Input.IsActionPressed("ui_down"))
            {
                direction = new Vector2I(0, 1);
            }
            else if (Input.IsActionPressed("ui_left"))
            {
                direction = new Vector2I(-1, 0);
            }
            else if (Input.IsActionPressed("ui_right"))
            {
                direction = new Vector2I(1, 0);
            }

            // Try to move in the input direction
            if (direction != Vector2I.Zero)
            {
                TryMove(direction);
            }
        }

        /// <summary>
        /// Tries to move the player in the specified direction.
        /// </summary>
        /// <param name="direction">The direction to move</param>
        /// <returns>True if movement was successful, false otherwise</returns>
        public bool TryMove(Vector2I direction)
        {
            // Set facing direction
            _facingDirection = direction;

            // Calculate new position
            Vector2I newPosition = _gridPosition + direction;

            // Check if the new position is valid
            if (_gridSystem.IsValidPosition(newPosition))
            {
                // Start movement
                _gridPosition = newPosition;
                _startPosition = Position;
                _targetPosition = _gridSystem.GridToWorldPosition(_gridPosition);
                _isMoving = true;
                _movementProgress = 0.0f;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the current grid position of the player.
        /// </summary>
        /// <returns>The current grid position</returns>
        public Vector2I GetGridPosition()
        {
            return _gridPosition;
        }

        /// <summary>
        /// Gets the current facing direction of the player.
        /// </summary>
        /// <returns>The current facing direction</returns>
        public Vector2I GetFacingDirection()
        {
            return _facingDirection;
        }

        /// <summary>
        /// Sets the facing direction of the player.
        /// </summary>
        /// <param name="direction">The new facing direction</param>
        public void SetFacingDirection(Vector2I direction)
        {
            _facingDirection = direction;
        }

        /// <summary>
        /// Sets the grid position of the player.
        /// </summary>
        /// <param name="position">The new grid position</param>
        public void SetGridPosition(Vector2I position)
        {
            if (_gridSystem.IsValidPosition(position))
            {
                _gridPosition = position;
                Position = _gridSystem.GridToWorldPosition(_gridPosition);
                _targetPosition = Position;
                _startPosition = Position;
                _isMoving = false;
                _movementProgress = 0.0f;
            }
        }

        /// <summary>
        /// Sets a movement penalty on the player.
        /// </summary>
        /// <param name="penaltyMultiplier">The movement speed multiplier (0.0 to 1.0)</param>
        public void SetMovementPenalty(float penaltyMultiplier)
        {
            _movementPenalty = Mathf.Clamp(penaltyMultiplier, 0.1f, 1.0f);
            _moveSpeed = _baseMoveSpeed * _movementPenalty;
            GD.Print($"PlayerController: Movement penalty set to {_movementPenalty}, speed now {_moveSpeed}");
        }

        /// <summary>
        /// Resets any movement penalties on the player.
        /// </summary>
        public void ResetMovementPenalty()
        {
            _movementPenalty = 1.0f;
            _moveSpeed = _baseMoveSpeed;
            GD.Print("PlayerController: Movement penalty reset");
        }



        /// <summary>
        /// Custom drawing function for the player character.
        /// </summary>
        public override void _Draw()
        {
            // Get cell size
            int cellSize = _gridSystem.GetCellSize();

            // Draw player character (simple pixel art style)
            Color playerColor = new Color(0.0f, 0.7f, 1.0f); // Cyan-blue

            // Draw body (rectangle)
            Vector2 bodySize = new Vector2(cellSize * 0.6f, cellSize * 0.6f);
            Vector2 bodyPosition = new Vector2(-bodySize.X / 2, -bodySize.Y / 2);
            DrawRect(new Rect2(bodyPosition, bodySize), playerColor);

            // Draw head (circle)
            float headRadius = cellSize * 0.2f;
            Vector2 headPosition = new Vector2(0, -bodySize.Y / 2 - headRadius * 0.8f);
            DrawCircle(headPosition, headRadius, playerColor);

            // Draw direction indicator (eyes)
            Color eyeColor = new Color(0.1f, 0.1f, 0.1f); // Dark gray
            float eyeRadius = cellSize * 0.05f;

            // Position eyes based on facing direction
            if (_facingDirection.X == 0 && _facingDirection.Y == -1) // Facing up
            {
                // Eyes on top of head
                DrawCircle(headPosition + new Vector2(-headRadius * 0.4f, -headRadius * 0.2f), eyeRadius, eyeColor);
                DrawCircle(headPosition + new Vector2(headRadius * 0.4f, -headRadius * 0.2f), eyeRadius, eyeColor);
            }
            else if (_facingDirection.X == 0 && _facingDirection.Y == 1) // Facing down
            {
                // Eyes on bottom of head
                DrawCircle(headPosition + new Vector2(-headRadius * 0.4f, headRadius * 0.2f), eyeRadius, eyeColor);
                DrawCircle(headPosition + new Vector2(headRadius * 0.4f, headRadius * 0.2f), eyeRadius, eyeColor);
            }
            else if (_facingDirection.X == -1 && _facingDirection.Y == 0) // Facing left
            {
                // Eyes on left side of head
                DrawCircle(headPosition + new Vector2(-headRadius * 0.4f, 0), eyeRadius, eyeColor);
            }
            else if (_facingDirection.X == 1 && _facingDirection.Y == 0) // Facing right
            {
                // Eyes on right side of head
                DrawCircle(headPosition + new Vector2(headRadius * 0.4f, 0), eyeRadius, eyeColor);
            }

            // Draw signal strength indicator if game state is available
            if (_gameState != null && _gridSystem != null)
            {
                float signalStrength = 0.0f;
                float frequency = 90.0f;

                // Check if radio is on
                if (_gameState.IsRadioOn)
                {
                    // Get current frequency and signal strength
                    frequency = _gameState.CurrentFrequency;

                    // Find signal at current frequency
                    var signalData = _gameState.FindSignalAtFrequency(frequency);
                    if (signalData != null)
                    {
                        signalStrength = SignalLost.GameState.CalculateSignalStrength(frequency, signalData);
                    }

                    // Draw signal strength indicator
                    if (signalStrength > 0.0f)
                    {
                    // Draw signal indicator above player
                    float indicatorSize = cellSize * 0.4f;
                    Vector2 indicatorPosition = new Vector2(-indicatorSize / 2, -bodySize.Y / 2 - headRadius * 2 - indicatorSize);

                    // Background
                    Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
                    DrawRect(new Rect2(indicatorPosition, new Vector2(indicatorSize, indicatorSize)), bgColor);

                    // Signal strength bar
                    Color signalColor = new Color(0.0f, 0.8f, 0.0f, 0.8f); // Green
                    float barHeight = indicatorSize * signalStrength;
                    Vector2 barPosition = new Vector2(indicatorPosition.X, indicatorPosition.Y + indicatorSize - barHeight);
                    DrawRect(new Rect2(barPosition, new Vector2(indicatorSize, barHeight)), signalColor);

                    // Draw frequency text
                    DrawString(null, new Vector2(indicatorPosition.X - indicatorSize, indicatorPosition.Y - indicatorSize * 0.5f),
                        $"{frequency:F1} MHz", HorizontalAlignment.Left, -1, 8, new Color(1.0f, 1.0f, 1.0f, 0.8f));
                    }
                }
            }
        }
    }
}

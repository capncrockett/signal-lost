using Godot;
using System;

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
        
        // Reference to grid system
        private GridSystem _gridSystem;
        
        // Movement properties
        [Export]
        private float _moveSpeed = 4.0f; // Cells per second
        
        [Export]
        private float _moveTime = 0.25f; // Time to move one cell
        
        // Movement state
        private bool _isMoving = false;
        private Vector2 _targetPosition;
        private Vector2 _startPosition;
        private float _movementProgress = 0.0f;
        private Vector2I _facingDirection = new Vector2I(0, 1); // Default facing down
        
        // Input cooldown
        private float _inputCooldown = 0.0f;
        private const float INPUT_COOLDOWN_TIME = 0.1f;
        
        // Interaction
        private bool _canInteract = false;
        private Cell _interactionCell;
        
        // Signals
        [Signal]
        public delegate void PlayerMovedEventHandler(Vector2I newPosition);
        
        [Signal]
        public delegate void InteractionAvailableEventHandler(string prompt);
        
        [Signal]
        public delegate void InteractionUnavailableEventHandler();
        
        [Signal]
        public delegate void InteractionCompletedEventHandler(string objectId);
        
        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Find the grid system
            _gridSystem = GetNode<GridSystem>("/root/FieldExplorationScene/GridSystem");
            if (_gridSystem == null)
            {
                GD.PrintErr("PlayerController: Failed to find GridSystem node");
                return;
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
            // Update input cooldown
            if (_inputCooldown > 0)
            {
                _inputCooldown -= (float)delta;
            }
            
            // Handle movement
            if (_isMoving)
            {
                _movementProgress += (float)delta / _moveTime;
                if (_movementProgress >= 1.0f)
                {
                    // Movement complete
                    _isMoving = false;
                    Position = _targetPosition;
                    _movementProgress = 0.0f;
                    
                    // Check for interactable objects
                    CheckForInteractables();
                }
                else
                {
                    // Interpolate position
                    Position = _startPosition.Lerp(_targetPosition, _movementProgress);
                }
            }
            else if (_inputCooldown <= 0)
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
            
            // Interaction input
            if (Input.IsActionJustPressed("ui_accept") || Input.IsActionJustPressed("ui_select"))
            {
                Interact();
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
                _inputCooldown = INPUT_COOLDOWN_TIME;
                
                // Emit signal
                EmitSignal(SignalName.PlayerMoved, _gridPosition);
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Checks for interactable objects in the current and adjacent cells.
        /// </summary>
        private void CheckForInteractables()
        {
            // Check the cell in front of the player
            Vector2I interactionPosition = _gridPosition + _facingDirection;
            Cell cell = _gridSystem.GetCell(interactionPosition);
            
            if (cell != null && cell.HasInteractable)
            {
                // Interactable object found
                _canInteract = true;
                _interactionCell = cell;
                
                // Get interaction prompt
                InteractableObject interactable = cell.GetInteractable();
                if (interactable != null)
                {
                    string prompt = interactable.GetInteractionPrompt();
                    EmitSignal(SignalName.InteractionAvailable, prompt);
                }
            }
            else
            {
                // No interactable object found
                _canInteract = false;
                _interactionCell = null;
                EmitSignal(SignalName.InteractionUnavailable);
            }
        }
        
        /// <summary>
        /// Interacts with the object in front of the player.
        /// </summary>
        /// <returns>True if interaction was successful, false otherwise</returns>
        public bool Interact()
        {
            if (_canInteract && _interactionCell != null)
            {
                bool success = _interactionCell.Interact();
                
                if (success)
                {
                    // Get the interactable object
                    InteractableObject interactable = _interactionCell.GetInteractable();
                    if (interactable != null)
                    {
                        // Emit signal
                        EmitSignal(SignalName.InteractionCompleted, interactable.ObjectId);
                    }
                    
                    // Check for interactables again (in case the interaction changed something)
                    CheckForInteractables();
                }
                
                return success;
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
                
                // Emit signal
                EmitSignal(SignalName.PlayerMoved, _gridPosition);
                
                // Check for interactables
                CheckForInteractables();
            }
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
        }
    }
}

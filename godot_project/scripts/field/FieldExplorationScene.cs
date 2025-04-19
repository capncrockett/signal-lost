using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost.Field
{
    /// <summary>
    /// Main scene for the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class FieldExplorationScene : Node2D
    {
        // References
        private GridSystem _gridSystem;
        private PlayerController _playerController;
        private Camera2D _camera;
        
        // UI elements
        private Control _interactionPrompt;
        private Label _promptLabel;
        
        // Game state
        private GameState _gameState;
        private RadioSystem _radioSystem;
        private InventorySystem _inventorySystem;
        private QuestSystem _questSystem;
        
        // Map data
        [Export]
        private string _mapPath = "res://assets/maps/field.json";
        
        // Signal sources
        private List<SignalSourceObject> _signalSources = new List<SignalSourceObject>();
        
        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references
            _gridSystem = GetNode<GridSystem>("GridSystem");
            _playerController = GetNode<PlayerController>("PlayerController");
            _camera = GetNode<Camera2D>("Camera2D");
            
            _interactionPrompt = GetNode<Control>("UI/InteractionPrompt");
            _promptLabel = GetNode<Label>("UI/InteractionPrompt/Label");
            
            _gameState = GetNode<GameState>("/root/GameState");
            if (_gameState == null)
            {
                GD.PrintErr("FieldExplorationScene: Failed to find GameState node");
                return;
            }
            
            _radioSystem = _gameState.GetRadioSystem();
            _inventorySystem = _gameState.GetInventorySystem();
            _questSystem = _gameState.GetQuestSystem();
            
            // Connect signals
            _playerController.InteractionAvailable += OnInteractionAvailable;
            _playerController.InteractionUnavailable += OnInteractionUnavailable;
            _playerController.InteractionCompleted += OnInteractionCompleted;
            _playerController.PlayerMoved += OnPlayerMoved;
            
            // Hide interaction prompt initially
            _interactionPrompt.Visible = false;
            
            // Load map
            LoadMap();
            
            // Initialize camera
            InitializeCamera();
            
            GD.Print("FieldExplorationScene: Initialized");
        }
        
        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="delta">Time since the last frame</param>
        public override void _Process(double delta)
        {
            // Update camera position to follow player
            if (_camera != null && _playerController != null)
            {
                _camera.Position = _playerController.Position;
            }
            
            // Update signal strengths based on player position
            UpdateSignalStrengths();
        }
        
        /// <summary>
        /// Loads the map and initializes the grid.
        /// </summary>
        private void LoadMap()
        {
            if (_gridSystem != null)
            {
                _gridSystem.LoadMapFromJson(_mapPath);
                
                // Place objects on the map
                PlaceObjects();
            }
        }
        
        /// <summary>
        /// Places objects on the map.
        /// </summary>
        private void PlaceObjects()
        {
            // Example: Place a signal source
            PlaceSignalSource(new Vector2I(5, 5), 91.5f, "signal_tutorial", "Tutorial Signal", 
                "A weak signal source that introduces you to the radio mechanics.");
            
            // Example: Place an item
            PlaceItem(new Vector2I(3, 7), "medkit", "Medkit", "A medical kit that can be used to heal injuries.", 1);
            
            // Example: Place another signal source
            PlaceSignalSource(new Vector2I(10, 8), 104.7f, "signal_bunker", "Bunker Signal", 
                "A strong signal coming from the emergency bunker.");
        }
        
        /// <summary>
        /// Places a signal source on the map.
        /// </summary>
        /// <param name="position">The grid position to place the signal source</param>
        /// <param name="frequency">The frequency of the signal</param>
        /// <param name="messageId">The ID of the message associated with the signal</param>
        /// <param name="displayName">The display name of the signal source</param>
        /// <param name="description">The description of the signal source</param>
        private void PlaceSignalSource(Vector2I position, float frequency, string messageId, string displayName, string description)
        {
            // Create signal source object
            var signalSource = new SignalSourceObject();
            signalSource.ObjectId = $"signal_{position.X}_{position.Y}";
            signalSource.Frequency = frequency;
            signalSource.MessageId = messageId;
            signalSource.DisplayName = displayName;
            signalSource.Description = description;
            
            // Add to scene
            AddChild(signalSource);
            
            // Position in world
            signalSource.Position = _gridSystem.GridToWorldPosition(position);
            
            // Place on grid
            _gridSystem.PlaceInteractable(position, signalSource);
            
            // Add to list of signal sources
            _signalSources.Add(signalSource);
            
            GD.Print($"FieldExplorationScene: Placed signal source at {position} (frequency: {frequency})");
        }
        
        /// <summary>
        /// Places an item on the map.
        /// </summary>
        /// <param name="position">The grid position to place the item</param>
        /// <param name="itemId">The ID of the item</param>
        /// <param name="displayName">The display name of the item</param>
        /// <param name="description">The description of the item</param>
        /// <param name="quantity">The quantity of the item</param>
        private void PlaceItem(Vector2I position, string itemId, string displayName, string description, int quantity)
        {
            // Create item object
            var item = new ItemObject();
            item.ObjectId = $"item_{position.X}_{position.Y}";
            item.ItemId = itemId;
            item.DisplayName = displayName;
            item.Description = description;
            item.Quantity = quantity;
            
            // Add to scene
            AddChild(item);
            
            // Position in world
            item.Position = _gridSystem.GridToWorldPosition(position);
            
            // Place on grid
            _gridSystem.PlaceInteractable(position, item);
            
            GD.Print($"FieldExplorationScene: Placed item {itemId} at {position}");
        }
        
        /// <summary>
        /// Initializes the camera.
        /// </summary>
        private void InitializeCamera()
        {
            if (_camera != null && _playerController != null)
            {
                // Set initial camera position to follow player
                _camera.Position = _playerController.Position;
                
                // Set camera limits based on grid size
                int cellSize = _gridSystem.GetCellSize();
                int width = _gridSystem.GetWidth();
                int height = _gridSystem.GetHeight();
                
                _camera.LimitLeft = 0;
                _camera.LimitTop = 0;
                _camera.LimitRight = width * cellSize;
                _camera.LimitBottom = height * cellSize;
                
                GD.Print($"FieldExplorationScene: Set camera limits to {width * cellSize}x{height * cellSize}");
            }
        }
        
        /// <summary>
        /// Updates signal strengths based on player position.
        /// </summary>
        private void UpdateSignalStrengths()
        {
            if (_playerController != null && _radioSystem != null)
            {
                Vector2I playerPosition = _playerController.GetGridPosition();
                
                // Update signal strengths for all signal sources
                foreach (var signalSource in _signalSources)
                {
                    float strength = signalSource.CalculateSignalStrengthAtPosition(playerPosition);
                    _radioSystem.UpdateSignalSourceStrength(signalSource.Frequency, strength);
                }
            }
        }
        
        /// <summary>
        /// Called when an interaction becomes available.
        /// </summary>
        /// <param name="prompt">The interaction prompt text</param>
        private void OnInteractionAvailable(string prompt)
        {
            // Show interaction prompt
            _promptLabel.Text = prompt;
            _interactionPrompt.Visible = true;
        }
        
        /// <summary>
        /// Called when an interaction becomes unavailable.
        /// </summary>
        private void OnInteractionUnavailable()
        {
            // Hide interaction prompt
            _interactionPrompt.Visible = false;
        }
        
        /// <summary>
        /// Called when an interaction is completed.
        /// </summary>
        /// <param name="objectId">The ID of the interacted object</param>
        private void OnInteractionCompleted(string objectId)
        {
            // Handle interaction completion
            GD.Print($"FieldExplorationScene: Interaction completed with object {objectId}");
            
            // Check for quest updates
            if (_questSystem != null)
            {
                _questSystem.CheckInteractionForQuests(objectId);
            }
        }
        
        /// <summary>
        /// Called when the player moves.
        /// </summary>
        /// <param name="newPosition">The new position of the player</param>
        private void OnPlayerMoved(Vector2I newPosition)
        {
            // Handle player movement
            GD.Print($"FieldExplorationScene: Player moved to {newPosition}");
            
            // Check for quests in the current location
            if (_questSystem != null)
            {
                string locationId = GetLocationIdForPosition(newPosition);
                if (!string.IsNullOrEmpty(locationId))
                {
                    _questSystem.CheckLocationForQuests(locationId);
                }
            }
        }
        
        /// <summary>
        /// Gets the location ID for a grid position.
        /// </summary>
        /// <param name="position">The grid position</param>
        /// <returns>The location ID, or an empty string if no location is found</returns>
        private string GetLocationIdForPosition(Vector2I position)
        {
            // This is a placeholder - in a real implementation, you would have a mapping of grid positions to location IDs
            // For now, we'll just return an empty string
            return "";
        }
    }
}

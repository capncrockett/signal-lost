using Godot;
using System.Collections.Generic;
using SignalLost;

namespace SignalLost.Field
{
    /// <summary>
    /// Manages the field exploration system, including player movement, interactions, and scene transitions.
    /// </summary>
    [GlobalClass]
    public partial class FieldExplorationManager : Node
    {
        // References
        private PlayerController _player;
        private GridSystem _gridSystem;
        private SignalLost.GameState _gameState;
        private QuestSystem _questSystem;
        private InventorySystem _inventorySystem;
        private MessageManager _messageManager;

        // UI elements
        private Panel _interactionPanel;
        private Label _interactionNameLabel;
        private Label _interactionDescriptionLabel;
        private Button _interactionButton;

        // Interactive objects
        private Dictionary<Vector2I, InteractiveObject> _interactiveObjectsMap = new Dictionary<Vector2I, InteractiveObject>();
        private InteractiveObject _currentInteractiveObject;

        // Scene transitions
        private Dictionary<string, string> _sceneTransitions = new Dictionary<string, string>
        {
            { "bunker_exit", "res://scenes/field/ForestScene.tscn" },
            { "forest_entrance", "res://scenes/field/BunkerScene.tscn" },
            { "forest_cabin", "res://scenes/field/CabinScene.tscn" },
            { "cabin_exit", "res://scenes/field/ForestScene.tscn" }
        };

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to game systems
            _gameState = GetNode<SignalLost.GameState>("/root/GameState");
            _questSystem = GetNode<QuestSystem>("/root/QuestSystem");
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _messageManager = GetNode<MessageManager>("/root/MessageManager");

            if (_gameState == null || _questSystem == null || _inventorySystem == null || _messageManager == null)
            {
                GD.PrintErr("FieldExplorationManager: Failed to get references to game systems");
                return;
            }

            // Get references to scene nodes
            _gridSystem = GetNode<GridSystem>("../GridSystem");
            _player = GetNode<PlayerController>("../GridSystem/Player");

            if (_gridSystem == null || _player == null)
            {
                GD.PrintErr("FieldExplorationManager: Failed to get references to scene nodes");
                return;
            }

            // Get references to UI elements
            _interactionPanel = GetNode<Panel>("../UI/InteractionPanel");
            _interactionNameLabel = GetNode<Label>("../UI/InteractionPanel/MarginContainer/VBoxContainer/InteractionNameLabel");
            _interactionDescriptionLabel = GetNode<Label>("../UI/InteractionPanel/MarginContainer/VBoxContainer/InteractionDescriptionLabel");
            _interactionButton = GetNode<Button>("../UI/InteractionPanel/MarginContainer/VBoxContainer/InteractionButton");

            if (_interactionPanel == null || _interactionNameLabel == null ||
                _interactionDescriptionLabel == null || _interactionButton == null)
            {
                GD.PrintErr("FieldExplorationManager: Failed to get references to UI elements");
                return;
            }

            // Connect signals
            _interactionButton.Pressed += OnInteractionButtonPressed;

            // Initialize UI
            _interactionPanel.Visible = false;

            // Map interactive objects
            MapInteractiveObjects();
        }

        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_interactionButton != null)
            {
                _interactionButton.Pressed -= OnInteractionButtonPressed;
            }
        }

        // Called every frame
        public override void _Process(double delta)
        {
            // Check for nearby interactive objects
            CheckForInteractiveObjects();
        }

        // Map interactive objects to their grid positions
        private void MapInteractiveObjects()
        {
            // Clear the map
            _interactiveObjectsMap.Clear();

            // Get all interactive objects in the scene
            var interactiveObjects = GetNode<Node2D>("../GridSystem/InteractiveObjects").GetChildren();

            foreach (var obj in interactiveObjects)
            {
                if (obj is InteractiveObject interactiveObject)
                {
                    // Get the grid position of the object
                    Vector2I gridPosition = _gridSystem.WorldToGridPosition(interactiveObject.Position);

                    // Add to the map
                    _interactiveObjectsMap[gridPosition] = interactiveObject;

                    // Connect signals
                    interactiveObject.Interaction += OnInteractiveObjectInteraction;
                    interactiveObject.PlayerNearby += OnInteractiveObjectPlayerNearby;

                    GD.Print($"Mapped interactive object {interactiveObject.ObjectId} at grid position {gridPosition}");
                }
            }
        }

        // Check for interactive objects near the player
        private void CheckForInteractiveObjects()
        {
            if (_player == null || _gridSystem == null)
                return;

            // Get the player's grid position
            Vector2I playerPosition = _player.GetGridPosition();

            // Check adjacent cells
            Vector2I[] adjacentCells = new Vector2I[]
            {
                playerPosition,                          // Current cell
                playerPosition + new Vector2I(1, 0),     // Right
                playerPosition + new Vector2I(-1, 0),    // Left
                playerPosition + new Vector2I(0, 1),     // Down
                playerPosition + new Vector2I(0, -1)     // Up
            };

            // Check if any of the adjacent cells contain an interactive object
            bool foundObject = false;
            foreach (var cell in adjacentCells)
            {
                if (_interactiveObjectsMap.ContainsKey(cell))
                {
                    var interactiveObject = _interactiveObjectsMap[cell];

                    // Set the object as the current interactive object
                    _currentInteractiveObject = interactiveObject;

                    // Set the player as nearby
                    interactiveObject.SetPlayerNearby(true);

                    // Show the interaction panel
                    ShowInteractionPanel(interactiveObject);

                    foundObject = true;
                    break;
                }
            }

            // If no object was found, hide the interaction panel
            if (!foundObject && _currentInteractiveObject != null)
            {
                // Set the player as not nearby
                _currentInteractiveObject.SetPlayerNearby(false);

                // Clear the current interactive object
                _currentInteractiveObject = null;

                // Hide the interaction panel
                HideInteractionPanel();
            }
        }

        // Show the interaction panel for an interactive object
        private void ShowInteractionPanel(InteractiveObject interactiveObject)
        {
            if (_interactionPanel == null || _interactionNameLabel == null ||
                _interactionDescriptionLabel == null || _interactionButton == null)
                return;

            // Set the panel content
            _interactionNameLabel.Text = interactiveObject.ObjectName;
            _interactionDescriptionLabel.Text = interactiveObject.ObjectDescription;

            // Show the panel
            _interactionPanel.Visible = true;
        }

        // Hide the interaction panel
        private void HideInteractionPanel()
        {
            if (_interactionPanel == null)
                return;

            // Hide the panel
            _interactionPanel.Visible = false;
        }

        // Handle interaction button pressed
        private void OnInteractionButtonPressed()
        {
            if (_currentInteractiveObject == null)
                return;

            // Interact with the object
            _currentInteractiveObject.Interact();
        }

        // Handle interactive object interaction
        private void OnInteractiveObjectInteraction(string objectId)
        {
            GD.Print($"Interactive object interaction: {objectId}");

            // Check if this is a scene transition
            if (_sceneTransitions.ContainsKey(objectId))
            {
                // Get the target scene
                string targetScene = _sceneTransitions[objectId];

                // Change the scene
                GetTree().ChangeSceneToFile(targetScene);
            }
        }

        // Handle interactive object player nearby
        private void OnInteractiveObjectPlayerNearby(string objectId, bool isNearby)
        {
            GD.Print($"Interactive object player nearby: {objectId}, {isNearby}");
        }
    }
}

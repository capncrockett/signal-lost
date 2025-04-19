using Godot;
using System;

namespace SignalLost.Field
{
    /// <summary>
    /// An interactable object that represents a collectible item.
    /// </summary>
    [GlobalClass]
    public partial class ItemObject : InteractableObject
    {
        // Item properties
        [Export]
        public string ItemId { get; set; } = "";
        
        [Export]
        public int Quantity { get; set; } = 1;
        
        // References
        private GameState _gameState;
        private InventorySystem _inventorySystem;
        
        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references
            _gameState = GetNode<GameState>("/root/GameState");
            if (_gameState == null)
            {
                GD.PrintErr("ItemObject: Failed to find GameState node");
                return;
            }
            
            _inventorySystem = _gameState.GetInventorySystem();
            if (_inventorySystem == null)
            {
                GD.PrintErr("ItemObject: Failed to find InventorySystem");
                return;
            }
            
            // Set default values if not specified
            if (string.IsNullOrEmpty(ItemId))
            {
                ItemId = ObjectId;
            }
            
            if (string.IsNullOrEmpty(DisplayName))
            {
                DisplayName = "Item";
            }
            
            // One-time interaction by default for items
            IsOneTimeInteraction = true;
        }
        
        /// <summary>
        /// Called when the player interacts with this item.
        /// </summary>
        /// <returns>True if interaction was successful, false otherwise</returns>
        public override bool Interact()
        {
            if (IsOneTimeInteraction && _hasBeenInteracted)
            {
                // Already collected this item
                return false;
            }
            
            // Add item to inventory
            bool success = _inventorySystem.AddItem(ItemId, Quantity);
            
            if (success)
            {
                // Mark as interacted
                _hasBeenInteracted = true;
                
                // Show message
                var messageManager = GetNode<MessageManager>("/root/MessageManager");
                if (messageManager != null)
                {
                    messageManager.ShowMessage($"Collected: {DisplayName} x{Quantity}");
                }
                
                // Emit signal
                EmitSignal(SignalName.InteractionCompleted, ObjectId);
                
                // Hide the item
                Visible = false;
                
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Gets the interaction prompt text for this item.
        /// </summary>
        /// <returns>The interaction prompt text</returns>
        public override string GetInteractionPrompt()
        {
            return $"Press E to collect {DisplayName}";
        }
        
        /// <summary>
        /// Custom drawing function for the item.
        /// </summary>
        public override void _Draw()
        {
            // Draw a simple item representation
            Color itemColor = new Color(1.0f, 0.8f, 0.2f); // Gold-yellow
            
            // Draw item box
            float size = 16.0f;
            Vector2 position = new Vector2(-size / 2, -size / 2);
            DrawRect(new Rect2(position, new Vector2(size, size)), itemColor);
            
            // Draw highlight
            Color highlightColor = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            Vector2 highlightPosition = new Vector2(-size / 2, -size / 2);
            Vector2 highlightSize = new Vector2(size, size / 4);
            DrawRect(new Rect2(highlightPosition, highlightSize), highlightColor);
        }
    }
}

using Godot;
using System.Collections.Generic;
using SignalLost;
using SignalLost.Inventory;

namespace SignalLost.Field
{
    /// <summary>
    /// Represents an interactive object in the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class InteractiveObject : Node2D
    {
        // Object properties
        [Export] public string ObjectId { get; set; } = "";
        [Export] public string ObjectName { get; set; } = "";
        [Export] public string ObjectDescription { get; set; } = "";
        [Export] public bool IsInteractable { get; set; } = true;
        [Export] public bool RequiresItem { get; set; } = false;
        [Export] public string RequiredItemId { get; set; } = "";
        [Export] public string GrantsItemId { get; set; } = "";
        [Export] public int GrantsItemQuantity { get; set; } = 1;
        [Export] public bool IsOneTimeInteraction { get; set; } = false;
        [Export] public string InteractionMessage { get; set; } = "";
        [Export] public string QuestId { get; set; } = "";
        [Export] public string QuestObjectiveId { get; set; } = "";

        // Item placement properties
        [Export] public bool ContainsPlacedItem { get; set; } = false;
        [Export] public string PlacedItemId { get; set; } = "";
        [Export] public string ContainerName { get; set; } = "";

        // Visual properties
        [Export] public Color ObjectColor { get; set; } = new Color(0.0f, 0.7f, 0.0f); // Green
        [Export] public int ObjectSize { get; set; } = 16;
        [Export] public Texture2D ObjectTexture { get; set; } = null;

        // State
        private bool _hasBeenInteractedWith = false;
        private bool _isPlayerNearby = false;

        // References
        private SignalLost.GameState _gameState;
        private QuestSystem _questSystem;
        private InventorySystemAdapter _inventorySystem;
        private MessageManager _messageManager;
        private ItemPlacement _itemPlacement;

        // Signals
        [Signal]
        public delegate void InteractionEventHandler(string objectId);

        [Signal]
        public delegate void PlayerNearbyEventHandler(string objectId, bool isNearby);

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to game systems
            _gameState = GetNode<SignalLost.GameState>("/root/GameState");
            _questSystem = GetNode<QuestSystem>("/root/QuestSystem");
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");
            _messageManager = GetNode<MessageManager>("/root/MessageManager");
            _itemPlacement = GetNode<ItemPlacement>("/root/ItemPlacement");

            if (_gameState == null || _questSystem == null || _inventorySystem == null || _messageManager == null)
            {
                GD.PrintErr($"InteractiveObject {ObjectId}: Failed to get references to game systems");
                return;
            }

            // Check if this object has already been interacted with
            if (IsOneTimeInteraction)
            {
                _hasBeenInteractedWith = _gameState.IsObjectInteractedWith(ObjectId);
            }
        }

        // Called every frame
        public override void _Process(double delta)
        {
            // Draw the object
            QueueRedraw();
        }

        // Custom drawing function
        public override void _Draw()
        {
            // If the object has been interacted with and is one-time, make it semi-transparent
            Color drawColor = ObjectColor;
            if (_hasBeenInteractedWith && IsOneTimeInteraction)
            {
                drawColor.A = 0.5f;
            }

            // If player is nearby, add a highlight
            if (_isPlayerNearby)
            {
                // Draw highlight
                DrawCircle(Vector2.Zero, ObjectSize * 0.6f, new Color(1.0f, 1.0f, 1.0f, 0.3f));
            }

            // Draw the object
            if (ObjectTexture != null)
            {
                // Draw texture
                DrawTexture(ObjectTexture, new Vector2(-ObjectSize / 2, -ObjectSize / 2), drawColor);
            }
            else
            {
                // Draw a simple shape
                DrawCircle(Vector2.Zero, ObjectSize * 0.4f, drawColor);
            }

            // If the object requires an item, draw an indicator
            if (RequiresItem && !string.IsNullOrEmpty(RequiredItemId))
            {
                // Draw a small lock icon
                DrawRect(new Rect2(new Vector2(-ObjectSize * 0.2f, -ObjectSize * 0.2f), new Vector2(ObjectSize * 0.4f, ObjectSize * 0.4f)), new Color(0.8f, 0.8f, 0.8f, 0.8f));
            }
        }

        /// <summary>
        /// Called when the player interacts with this object.
        /// </summary>
        /// <returns>True if the interaction was successful, false otherwise</returns>
        public bool Interact()
        {
            // Check if the object is interactable
            if (!IsInteractable)
            {
                ShowMessage("This object cannot be interacted with.");
                return false;
            }

            // Check if the object has already been interacted with
            if (_hasBeenInteractedWith && IsOneTimeInteraction)
            {
                ShowMessage("You've already interacted with this object.");
                return false;
            }

            // Check if the object requires an item
            if (RequiresItem && !string.IsNullOrEmpty(RequiredItemId))
            {
                if (!_inventorySystem.HasItem(RequiredItemId))
                {
                    ShowMessage($"You need a {RequiredItemId} to interact with this object.");
                    return false;
                }

                // Use the item
                _inventorySystem.UseItem(RequiredItemId);
            }

            // Mark the object as interacted with
            _hasBeenInteractedWith = true;
            if (IsOneTimeInteraction)
            {
                _gameState.SetObjectInteractedWith(ObjectId);
            }

            // Handle placed items
            if (ContainsPlacedItem && !string.IsNullOrEmpty(PlacedItemId) && _itemPlacement != null)
            {
                string fullPlacedItemId = $"{_gameState.CurrentLocation}_{ContainerName}_{PlacedItemId}";
                bool collected = _itemPlacement.CollectItem(fullPlacedItemId);

                if (collected)
                {
                    var itemData = _inventorySystem.GetItem(PlacedItemId);
                    if (itemData != null)
                    {
                        ShowMessage($"You found {itemData.Name}!");
                    }
                    else
                    {
                        ShowMessage($"You found an item!");
                    }
                }
            }

            // Grant item if applicable
            if (!string.IsNullOrEmpty(GrantsItemId))
            {
                _inventorySystem.AddItemToInventory(GrantsItemId, GrantsItemQuantity);
                ShowMessage($"You found {GrantsItemQuantity} {GrantsItemId}!");
            }

            // Update quest if applicable
            if (!string.IsNullOrEmpty(QuestId) && !string.IsNullOrEmpty(QuestObjectiveId))
            {
                _questSystem.UpdateQuestObjective(QuestId, QuestObjectiveId);
            }

            // Show interaction message
            if (!string.IsNullOrEmpty(InteractionMessage))
            {
                ShowMessage(InteractionMessage);
            }

            // Emit interaction signal
            EmitSignal(SignalName.Interaction, ObjectId);

            return true;
        }

        /// <summary>
        /// Called when the player is nearby this object.
        /// </summary>
        /// <param name="isNearby">Whether the player is nearby</param>
        public void SetPlayerNearby(bool isNearby)
        {
            _isPlayerNearby = isNearby;
            EmitSignal(SignalName.PlayerNearby, ObjectId, isNearby);
        }

        /// <summary>
        /// Shows a message to the player.
        /// </summary>
        /// <param name="message">The message to show</param>
        private void ShowMessage(string message)
        {
            if (_messageManager != null)
            {
                _messageManager.ShowMessage(message);
            }
            else
            {
                GD.Print(message);
            }
        }
    }
}

using Godot;

namespace SignalLost.Inventory.UI
{
    /// <summary>
    /// Represents a single inventory slot in the UI.
    /// </summary>
    [GlobalClass]
    public partial class InventorySlotUI : Control
    {
        // The item ID displayed in this slot
        private string _itemId;

        // Whether the slot is selected
        private bool _isSelected;

        // Whether the slot is hovered
        private bool _isHovered;

        // The inventory core
        private InventoryCore _inventoryCore;

        // The item effects system
        private ItemEffectsSystem _itemEffectsSystem;

        // The item icon atlas
        private ItemIconAtlas _itemIconAtlas;

        // Signals
        [Signal]
        public delegate void SlotSelectedEventHandler(string itemId);

        [Signal]
        public delegate void SlotClickedEventHandler(string itemId, int buttonIndex);

        [Signal]
        public delegate void SlotHoveredEventHandler(string itemId);

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get the InventoryCore
            _inventoryCore = GetNode<InventoryCore>("/root/InventoryCore");

            if (_inventoryCore == null)
            {
                GD.PrintErr("InventorySlotUI: InventoryCore not found");
                return;
            }

            // Get the ItemEffectsSystem
            _itemEffectsSystem = GetNode<ItemEffectsSystem>("/root/ItemEffectsSystem");

            // Get the ItemIconAtlas
            _itemIconAtlas = GetNode<ItemIconAtlas>("/root/ItemIconAtlas");
        }

        // Set the item ID
        public void SetItemId(string itemId)
        {
            _itemId = itemId;
            QueueRedraw();
        }

        // Get the item ID
        public string GetItemId()
        {
            return _itemId;
        }

        // Set whether the slot is selected
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            QueueRedraw();
        }

        // Get whether the slot is selected
        public bool IsSelected()
        {
            return _isSelected;
        }

        // Handle input events
        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                EmitSignal(SignalName.SlotClicked, _itemId, (int)mouseButton.ButtonIndex);
                EmitSignal(SignalName.SlotSelected, _itemId);
            }
            else if (@event is InputEventMouseMotion)
            {
                if (!_isHovered)
                {
                    _isHovered = true;
                    EmitSignal(SignalName.SlotHovered, _itemId);
                    QueueRedraw();
                }
            }
        }

        // Handle mouse exit
        public void _on_mouse_exited()
        {
            _isHovered = false;
            QueueRedraw();
        }

        // Draw the slot
        public override void _Draw()
        {
            // Draw slot background
            Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);
            DrawRect(new Rect2(0, 0, Size.X, Size.Y), backgroundColor);

            // Draw slot border
            Color borderColor = new Color(0.3f, 0.3f, 0.3f, 1.0f);

            if (_isSelected)
            {
                borderColor = new Color(0.8f, 0.8f, 0.2f, 1.0f);
            }
            else if (_isHovered)
            {
                borderColor = new Color(0.5f, 0.5f, 0.5f, 1.0f);
            }

            DrawRect(new Rect2(0, 0, Size.X, Size.Y), borderColor, false, 2);

            // Draw item if there is one
            if (!string.IsNullOrEmpty(_itemId) && _inventoryCore.HasItem(_itemId))
            {
                var item = _inventoryCore.GetItem(_itemId);

                // Draw item icon
                if (_itemIconAtlas != null && _itemIconAtlas.GetAtlasTexture() != null)
                {
                    // Draw icon from atlas
                    Rect2 iconRegion = _itemIconAtlas.GetIconRegion(item.IconIndex);
                    DrawTextureRectRegion(
                        _itemIconAtlas.GetAtlasTexture(),
                        new Rect2(4, 4, Size.X - 8, Size.Y - 8),
                        iconRegion
                    );
                }
                else
                {
                    // Draw colored rectangle as fallback
                    Color itemColor = GetItemColor(item.Category);
                    DrawRect(new Rect2(4, 4, Size.X - 8, Size.Y - 8), itemColor);
                }

                // Draw quantity if more than 1
                if (item.Quantity > 1)
                {
                    string quantityText = item.Quantity.ToString();
                    DrawString(ThemeDB.FallbackFont, new Vector2(Size.X - 4 - quantityText.Length * 6, Size.Y - 4),
                        quantityText, HorizontalAlignment.Right, -1, 12, new Color(1.0f, 1.0f, 1.0f, 1.0f));
                }

                // Draw equipped indicator
                if (_itemEffectsSystem != null && _itemEffectsSystem.IsItemEquipped(_itemId))
                {
                    DrawRect(new Rect2(0, 0, 8, 8), new Color(0.0f, 0.8f, 0.0f, 1.0f));
                }
            }
        }

        // Get a color for an item based on its category
        private static Color GetItemColor(string category)
        {
            switch (category.ToLower())
            {
                case "tool":
                    return new Color(0.2f, 0.6f, 0.8f, 1.0f); // Blue
                case "consumable":
                    return new Color(0.8f, 0.2f, 0.2f, 1.0f); // Red
                case "key":
                    return new Color(0.8f, 0.8f, 0.2f, 1.0f); // Yellow
                case "document":
                    return new Color(0.8f, 0.6f, 0.2f, 1.0f); // Orange
                case "component":
                    return new Color(0.2f, 0.8f, 0.2f, 1.0f); // Green
                case "special":
                    return new Color(0.8f, 0.2f, 0.8f, 1.0f); // Purple
                default:
                    return new Color(0.5f, 0.5f, 0.5f, 1.0f); // Gray
            }
        }
    }
}

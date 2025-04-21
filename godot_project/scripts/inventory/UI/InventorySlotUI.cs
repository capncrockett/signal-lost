using Godot;

namespace SignalLost.Inventory.UI
{
    /// <summary>
    /// UI element for an inventory slot.
    /// </summary>
    [GlobalClass]
    public partial class InventorySlotUI : Control
    {
        // Item ID
        private string _itemId;

        // Is this slot selected
        private bool _isSelected = false;

        // Is this item equipped
        private bool _isEquipped = false;

        // Reference to the inventory system
        private InventorySystemAdapter _inventorySystem;

        // Reference to the item icon atlas
        private ItemIconAtlas _itemIconAtlas;

        // UI elements
        private TextureRect _iconRect;
        private Label _quantityLabel;
        private Panel _selectionPanel;
        private Panel _equippedPanel;
        private Control _tooltipControl;
        private Label _tooltipNameLabel;
        private Label _tooltipDescriptionLabel;
        private Label _tooltipCategoryLabel;

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
            // Get references to systems
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");
            _itemIconAtlas = GetNode<ItemIconAtlas>("/root/ItemIconAtlas");

            // Create UI elements
            _iconRect = new TextureRect();
            _iconRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            _iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            _iconRect.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _iconRect.SizeFlagsVertical = SizeFlags.ExpandFill;
            _iconRect.CustomMinimumSize = new Vector2(40, 40);

            _quantityLabel = new Label();
            _quantityLabel.HorizontalAlignment = HorizontalAlignment.Right;
            _quantityLabel.VerticalAlignment = VerticalAlignment.Bottom;
            _quantityLabel.CustomMinimumSize = new Vector2(40, 40);

            _selectionPanel = new Panel();
            _selectionPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _selectionPanel.SizeFlagsVertical = SizeFlags.ExpandFill;
            _selectionPanel.Visible = false;

            _equippedPanel = new Panel();
            _equippedPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _equippedPanel.SizeFlagsVertical = SizeFlags.ExpandFill;
            _equippedPanel.Visible = false;

            // Create tooltip
            _tooltipControl = new Control();
            _tooltipControl.Visible = false;

            var tooltipPanel = new Panel();
            tooltipPanel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            tooltipPanel.SizeFlagsVertical = SizeFlags.ExpandFill;

            var tooltipContainer = new VBoxContainer();
            tooltipContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            tooltipContainer.SizeFlagsVertical = SizeFlags.ExpandFill;

            _tooltipNameLabel = new Label();
            _tooltipNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _tooltipNameLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1));

            _tooltipCategoryLabel = new Label();
            _tooltipCategoryLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _tooltipCategoryLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));

            _tooltipDescriptionLabel = new Label();
            _tooltipDescriptionLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _tooltipDescriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _tooltipDescriptionLabel.CustomMinimumSize = new Vector2(200, 0);
            _tooltipDescriptionLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));

            // Add elements to the tooltip
            tooltipContainer.AddChild(_tooltipNameLabel);
            tooltipContainer.AddChild(_tooltipCategoryLabel);
            tooltipContainer.AddChild(_tooltipDescriptionLabel);

            tooltipPanel.AddChild(tooltipContainer);
            _tooltipControl.AddChild(tooltipPanel);

            // Add UI elements to the slot
            AddChild(_selectionPanel);
            AddChild(_equippedPanel);
            AddChild(_iconRect);
            AddChild(_quantityLabel);
            AddChild(_tooltipControl);

            // Connect signals
            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
            GuiInput += OnGuiInput;

            // Update the slot
            UpdateSlot();
        }

        // Set the item ID
        public void SetItemId(string itemId)
        {
            _itemId = itemId;
            UpdateSlot();
        }

        // Get the item ID
        public string GetItemId()
        {
            return _itemId;
        }

        // Set whether this slot is selected
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateSlot();
        }

        // Set whether this item is equipped
        public void SetEquipped(bool equipped)
        {
            _isEquipped = equipped;
            UpdateSlot();
        }

        // Update the slot
        private void UpdateSlot()
        {
            // Update selection panel
            _selectionPanel.Visible = _isSelected;

            // Update equipped panel
            _equippedPanel.Visible = _isEquipped;

            // Check if we have an item
            if (_itemId != null && _inventorySystem != null && _inventorySystem.HasItem(_itemId))
            {
                // Get the item
                var item = _inventorySystem.GetItem(_itemId);

                // Update icon
                if (_itemIconAtlas != null)
                {
                    // Get the atlas texture and icon region
                    var atlasTexture = _itemIconAtlas.GetAtlasTexture();
                    var iconRegion = _itemIconAtlas.GetIconRegion(item.IconIndex);

                    // Create an AtlasTexture for the specific icon
                    var iconTexture = new AtlasTexture();
                    iconTexture.Atlas = atlasTexture;
                    iconTexture.Region = iconRegion;

                    // Set the texture
                    _iconRect.Texture = iconTexture;
                }

                // Update quantity
                if (item.Quantity > 1)
                {
                    _quantityLabel.Text = item.Quantity.ToString();
                }
                else
                {
                    _quantityLabel.Text = "";
                }

                // Update tooltip
                _tooltipNameLabel.Text = item.Name;
                _tooltipCategoryLabel.Text = $"[{item.Category.ToUpper()}]";
                _tooltipDescriptionLabel.Text = item.Description;

                // Update equipped status
                if (_inventorySystem.IsItemEquipped(_itemId))
                {
                    SetEquipped(true);
                }
                else
                {
                    SetEquipped(false);
                }
            }
            else
            {
                // No item
                _iconRect.Texture = null;
                _quantityLabel.Text = "";
                _tooltipNameLabel.Text = "Empty Slot";
                _tooltipCategoryLabel.Text = "";
                _tooltipDescriptionLabel.Text = "";
                SetEquipped(false);
            }
        }

        // Handle mouse entered
        private void OnMouseEntered()
        {
            // Show tooltip
            _tooltipControl.Visible = true;

            // Emit signal
            EmitSignal(SignalName.SlotHovered, _itemId);
        }

        // Handle mouse exited
        private void OnMouseExited()
        {
            // Hide tooltip
            _tooltipControl.Visible = false;
        }

        // Handle GUI input
        private void OnGuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
            {
                // Emit signals
                EmitSignal(SignalName.SlotClicked, _itemId, (int)mouseButton.ButtonIndex);

                if (mouseButton.ButtonIndex == MouseButton.Left)
                {
                    EmitSignal(SignalName.SlotSelected, _itemId);
                }
            }
        }
    }
}

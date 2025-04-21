using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelInventoryUI : Control
    {
        // Configuration
        [Export] public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [Export] public Color PanelColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        [Export] public Color BorderColor { get; set; } = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        [Export] public Color TextColor { get; set; } = new Color(0.9f, 0.9f, 0.9f, 1.0f);
        [Export] public Color HighlightColor { get; set; } = new Color(0.3f, 0.7f, 0.3f, 1.0f);
        [Export] public Color ButtonColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        [Export] public Color ButtonHoverColor { get; set; } = new Color(0.4f, 0.4f, 0.4f, 1.0f);
        [Export] public Color ButtonPressColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        [Export] public int GridSize { get; set; } = 4; // Number of slots per row/column
        [Export] public int SlotSize { get; set; } = 40; // Size of each inventory slot in pixels
        [Export] public int SlotPadding { get; set; } = 4; // Padding between slots
        [Export] public int BorderWidth { get; set; } = 1; // Width of borders
        [Export] public bool ShowTooltips { get; set; } = true;

        // References to game systems
        private InventorySystem _inventorySystem;
        private GameState _gameState;
        private PixelFont _pixelFont;

        // UI state
        private bool _isVisible = false;
        private string _selectedItemId = null;
        private Vector2 _mousePosition = Vector2.Zero;
        private bool _isDragging = false;
        private string _draggedItemId = null;
        private Vector2 _dragStartPosition = Vector2.Zero;
        private Dictionary<string, Rect2> _itemSlotRects = new Dictionary<string, Rect2>();
        private Dictionary<string, Rect2> _buttonRects = new Dictionary<string, Rect2>();
        private Dictionary<string, bool> _buttonHovered = new Dictionary<string, bool>();
        private Dictionary<string, bool> _buttonPressed = new Dictionary<string, bool>();
        private string _tooltipText = "";
        private bool _showTooltip = false;
        private Vector2 _tooltipPosition = Vector2.Zero;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to game systems
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_inventorySystem == null || _gameState == null)
            {
                GD.PrintErr("PixelInventoryUI: Failed to get InventorySystem or GameState reference");
                return;
            }

            // Initialize pixel font
            _pixelFont = new PixelFont();

            // Connect signals
            _inventorySystem.ItemAdded += OnInventoryChanged;
            _inventorySystem.ItemRemoved += OnInventoryChanged;
            _gameState.InventoryChanged += OnInventoryChanged;

            // Set up input processing
            SetProcessInput(true);
        }

        // Show or hide the inventory UI
        public new void SetVisible(bool visible)
        {
            _isVisible = visible;
            SetProcess(visible);
            SetProcessInput(visible);
            QueueRedraw();
        }

        // Toggle visibility
        public void ToggleVisibility()
        {
            SetVisible(!_isVisible);
        }

        // Check if the inventory UI is visible
        public new bool IsVisible()
        {
            return _isVisible;
        }

        // Set InventorySystem reference (for testing)
        public void SetInventorySystem(InventorySystem inventorySystem)
        {
            _inventorySystem = inventorySystem;
        }

        // Set GameState reference (for testing)
        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
        }

        // Process input events
        public override void _Input(InputEvent @event)
        {
            if (!_isVisible)
                return;

            if (@event is InputEventMouseMotion mouseMotion)
            {
                _mousePosition = mouseMotion.Position;

                // Update button hover states
                foreach (var buttonId in _buttonRects.Keys)
                {
                    _buttonHovered[buttonId] = _buttonRects[buttonId].HasPoint(_mousePosition);
                }

                // Update tooltip
                UpdateTooltip();

                // Handle dragging
                if (_isDragging)
                {
                    QueueRedraw();
                }

                QueueRedraw();
            }
            else if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left)
                {
                    if (mouseButton.Pressed)
                    {
                        // Check if a button was clicked
                        foreach (var buttonId in _buttonRects.Keys)
                        {
                            if (_buttonRects[buttonId].HasPoint(_mousePosition))
                            {
                                _buttonPressed[buttonId] = true;
                                QueueRedraw();
                                return;
                            }
                        }

                        // Check if an item slot was clicked
                        foreach (var itemId in _itemSlotRects.Keys)
                        {
                            if (_itemSlotRects[itemId].HasPoint(_mousePosition))
                            {
                                _selectedItemId = itemId;

                                // Start dragging
                                _isDragging = true;
                                _draggedItemId = itemId;
                                _dragStartPosition = _mousePosition;

                                QueueRedraw();
                                return;
                            }
                        }

                        // If nothing was clicked, deselect
                        _selectedItemId = null;
                        QueueRedraw();
                    }
                    else // Button released
                    {
                        // Check if a button was released
                        foreach (var buttonId in _buttonRects.Keys)
                        {
                            if (_buttonPressed[buttonId] && _buttonHovered[buttonId])
                            {
                                HandleButtonClick(buttonId);
                            }
                            _buttonPressed[buttonId] = false;
                        }

                        // End dragging
                        if (_isDragging)
                        {
                            // Check if the item was dropped on another slot
                            foreach (var itemId in _itemSlotRects.Keys)
                            {
                                if (itemId != _draggedItemId && _itemSlotRects[itemId].HasPoint(_mousePosition))
                                {
                                    // Handle item swap or stack
                                    HandleItemDrop(_draggedItemId, itemId);
                                    break;
                                }
                            }

                            _isDragging = false;
                            _draggedItemId = null;
                        }

                        QueueRedraw();
                    }
                }
                else if (mouseButton.ButtonIndex == MouseButton.Right && mouseButton.Pressed)
                {
                    // Right-click to use item
                    foreach (var itemId in _itemSlotRects.Keys)
                    {
                        if (_itemSlotRects[itemId].HasPoint(_mousePosition))
                        {
                            UseItem(itemId);
                            QueueRedraw();
                            return;
                        }
                    }
                }
            }
            else if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Close inventory with Escape key
                if (keyEvent.Keycode == Key.Escape)
                {
                    SetVisible(false);
                }
            }
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            if (!_isVisible)
                return;

            // Update tooltip position
            if (_showTooltip)
            {
                _tooltipPosition = _mousePosition + new Vector2(10, 10);
                QueueRedraw();
            }
        }

        // Custom drawing function
        public override void _Draw()
        {
            if (!_isVisible)
                return;

            Vector2 size = Size;
            float width = size.X;
            float height = size.Y;

            // Calculate panel dimensions
            float panelWidth = Mathf.Min(width - 40, 600);
            float panelHeight = Mathf.Min(height - 40, 400);
            float panelX = (width - panelWidth) / 2;
            float panelY = (height - panelHeight) / 2;

            // Draw background overlay
            DrawRect(new Rect2(0, 0, width, height), BackgroundColor);

            // Draw inventory panel
            DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight), PanelColor);
            DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight), BorderColor, false, BorderWidth);

            // Draw title
            DrawPixelText("INVENTORY", new Vector2(panelX + 10, panelY + 15), TextColor, 2);

            // Draw item count
            string itemCountText = $"ITEMS: {_inventorySystem.GetTotalItemCount()}/{_inventorySystem.GetMaxInventorySize()}";
            DrawPixelText(itemCountText, new Vector2(panelX + panelWidth - 10 - (itemCountText.Length * 6 * 2), panelY + 15), TextColor, 2);

            // Draw inventory grid
            DrawInventoryGrid(panelX + 10, panelY + 40, panelWidth - 20, panelHeight - 100);

            // Draw item info panel if an item is selected
            if (_selectedItemId != null && _inventorySystem.GetInventory().ContainsKey(_selectedItemId))
            {
                DrawItemInfoPanel(panelX + 10, panelY + panelHeight - 50, panelWidth - 20, 40);
            }

            // Draw action buttons
            DrawActionButtons(panelX + 10, panelY + panelHeight - 50, panelWidth - 20, 40);

            // Draw dragged item
            if (_isDragging && _draggedItemId != null)
            {
                DrawDraggedItem();
            }

            // Draw tooltip
            if (_showTooltip && !string.IsNullOrEmpty(_tooltipText))
            {
                DrawTooltip();
            }
        }

        // Draw the inventory grid
        private void DrawInventoryGrid(float x, float y, float width, float height)
        {
            // Calculate grid dimensions
            int totalSlots = _inventorySystem.GetMaxInventorySize();
            int slotsPerRow = GridSize;
            int rows = (int)Math.Ceiling((float)totalSlots / slotsPerRow);

            // Calculate slot size based on available space
            float availableWidth = width - (slotsPerRow + 1) * SlotPadding;
            float availableHeight = height - (rows + 1) * SlotPadding;
            int calculatedSlotSize = (int)Math.Min(availableWidth / slotsPerRow, availableHeight / rows);
            int slotSize = Math.Min(calculatedSlotSize, SlotSize);

            // Clear previous slot rects
            _itemSlotRects.Clear();

            // Get inventory
            var inventory = _inventorySystem.GetInventory();

            // Draw grid
            for (int i = 0; i < totalSlots; i++)
            {
                int row = i / slotsPerRow;
                int col = i % slotsPerRow;

                float slotX = x + col * (slotSize + SlotPadding) + SlotPadding;
                float slotY = y + row * (slotSize + SlotPadding) + SlotPadding;

                // Draw slot background
                DrawRect(new Rect2(slotX, slotY, slotSize, slotSize), new Color(0.15f, 0.15f, 0.15f, 1.0f));

                // Draw slot border
                Color borderColor = BorderColor;
                if (_selectedItemId != null && i < inventory.Count && inventory.ElementAt(i).Key == _selectedItemId)
                {
                    borderColor = HighlightColor;
                }
                DrawRect(new Rect2(slotX, slotY, slotSize, slotSize), borderColor, false, BorderWidth);

                // Draw item if slot is filled
                if (i < inventory.Count)
                {
                    var item = inventory.ElementAt(i).Value;
                    string itemId = item.Id;

                    // Skip if this is the dragged item
                    if (_isDragging && _draggedItemId == itemId)
                        continue;

                                // Draw item icon from atlas if available
                    var iconAtlas = GetNode<ItemIconAtlas>("/root/ItemIconAtlas");
                    if (iconAtlas != null && iconAtlas.GetAtlasTexture() != null)
                    {
                        // Draw icon from atlas
                        Rect2 iconRegion = iconAtlas.GetIconRegion(item.IconIndex);
                        DrawTextureRectRegion(
                            iconAtlas.GetAtlasTexture(),
                            new Rect2(slotX + 4, slotY + 4, slotSize - 8, slotSize - 8),
                            iconRegion
                        );
                    }
                    else
                    {
                        // Fallback to colored square
                        Color itemColor = GetItemColor(item.Category);
                        DrawRect(new Rect2(slotX + 4, slotY + 4, slotSize - 8, slotSize - 8), itemColor);

                        // Draw item initial
                        string initial = item.Name.Length > 0 ? item.Name[0].ToString() : "?";
                        DrawPixelText(initial, new Vector2(slotX + slotSize / 2 - 3, slotY + slotSize / 2 + 3), TextColor, 1);
                    }

                    // Draw quantity if more than 1
                    if (item.Quantity > 1)
                    {
                        string quantityText = item.Quantity.ToString();
                        DrawPixelText(quantityText, new Vector2(slotX + slotSize - 4 - quantityText.Length * 6, slotY + slotSize - 10), TextColor, 1);
                    }

                    // Store slot rect for interaction
                    _itemSlotRects[itemId] = new Rect2(slotX, slotY, slotSize, slotSize);
                }
            }
        }

        // Draw the item info panel
        private void DrawItemInfoPanel(float x, float y, float width, float height)
        {
            if (_selectedItemId == null || !_inventorySystem.GetInventory().ContainsKey(_selectedItemId))
                return;

            var item = _inventorySystem.GetInventory()[_selectedItemId];

            // Draw panel background
            DrawRect(new Rect2(x, y, width, height), new Color(0.15f, 0.15f, 0.15f, 1.0f));
            DrawRect(new Rect2(x, y, width, height), BorderColor, false, BorderWidth);

            // Draw item name
            DrawPixelText(item.Name.ToUpper(), new Vector2(x + 5, y + 15), TextColor, 1);

            // Draw item quantity
            string quantityText = $"QTY: {item.Quantity}";
            DrawPixelText(quantityText, new Vector2(x + width - 5 - quantityText.Length * 6, y + 15), TextColor, 1);

            // Draw item category with appropriate color
            Color categoryColor = GetItemColor(item.Category);
            string categoryText = $"[{item.Category.ToUpper()}]";
            DrawPixelText(categoryText, new Vector2(x + 5, y + 30), categoryColor, 1);

            // Draw item properties
            float propY = y + 30;

            // Show if item is usable
            if (item.IsUsable)
            {
                propY += 15;
                DrawPixelText("USABLE", new Vector2(x + width - 60, propY), new Color(0.0f, 0.8f, 0.0f, 1.0f), 1);
            }

            // Show if item is consumable
            if (item.IsConsumable)
            {
                propY += 15;
                DrawPixelText("CONSUMABLE", new Vector2(x + width - 90, propY), new Color(0.8f, 0.4f, 0.0f, 1.0f), 1);
            }

            // Show if item is equippable
            if (item.IsEquippable)
            {
                propY += 15;
                DrawPixelText("EQUIPPABLE", new Vector2(x + width - 90, propY), new Color(0.0f, 0.4f, 0.8f, 1.0f), 1);
            }

            // Show if item is combineable
            if (item.IsCombineable)
            {
                propY += 15;
                DrawPixelText("COMBINEABLE", new Vector2(x + width - 90, propY), new Color(0.8f, 0.0f, 0.8f, 1.0f), 1);
            }
        }

        // Draw action buttons
        private void DrawActionButtons(float x, float y, float width, float height)
        {
            // Clear previous button rects
            _buttonRects.Clear();

            // Initialize button states if needed
            if (!_buttonHovered.ContainsKey("use"))
            {
                _buttonHovered["use"] = false;
                _buttonPressed["use"] = false;
            }

            if (!_buttonHovered.ContainsKey("drop"))
            {
                _buttonHovered["drop"] = false;
                _buttonPressed["drop"] = false;
            }

            if (!_buttonHovered.ContainsKey("close"))
            {
                _buttonHovered["close"] = false;
                _buttonPressed["close"] = false;
            }

            // Calculate button dimensions
            float buttonWidth = 80;
            float buttonHeight = 30;
            float buttonSpacing = 10;
            float buttonsX = x + width - buttonWidth * 3 - buttonSpacing * 2;
            float buttonsY = y + height - buttonHeight - 5;

            // Draw Use button
            Color useButtonColor = ButtonColor;
            if (_buttonHovered["use"])
            {
                useButtonColor = _buttonPressed["use"] ? ButtonPressColor : ButtonHoverColor;
            }

            bool canUseItem = _selectedItemId != null &&
                              _inventorySystem.GetInventory().ContainsKey(_selectedItemId) &&
                              _inventorySystem.GetInventory()[_selectedItemId].IsUsable;

            if (!canUseItem)
            {
                useButtonColor = new Color(useButtonColor.R, useButtonColor.G, useButtonColor.B, 0.5f);
            }

            DrawRect(new Rect2(buttonsX, buttonsY, buttonWidth, buttonHeight), useButtonColor);
            DrawRect(new Rect2(buttonsX, buttonsY, buttonWidth, buttonHeight), BorderColor, false, BorderWidth);
            DrawPixelText("USE", new Vector2(buttonsX + buttonWidth / 2 - 9, buttonsY + buttonHeight / 2 + 3), TextColor, 1);
            _buttonRects["use"] = new Rect2(buttonsX, buttonsY, buttonWidth, buttonHeight);

            // Draw Drop button
            Color dropButtonColor = ButtonColor;
            if (_buttonHovered["drop"])
            {
                dropButtonColor = _buttonPressed["drop"] ? ButtonPressColor : ButtonHoverColor;
            }

            bool canDropItem = _selectedItemId != null &&
                               _inventorySystem.GetInventory().ContainsKey(_selectedItemId);

            if (!canDropItem)
            {
                dropButtonColor = new Color(dropButtonColor.R, dropButtonColor.G, dropButtonColor.B, 0.5f);
            }

            DrawRect(new Rect2(buttonsX + buttonWidth + buttonSpacing, buttonsY, buttonWidth, buttonHeight), dropButtonColor);
            DrawRect(new Rect2(buttonsX + buttonWidth + buttonSpacing, buttonsY, buttonWidth, buttonHeight), BorderColor, false, BorderWidth);
            DrawPixelText("DROP", new Vector2(buttonsX + buttonWidth + buttonSpacing + buttonWidth / 2 - 12, buttonsY + buttonHeight / 2 + 3), TextColor, 1);
            _buttonRects["drop"] = new Rect2(buttonsX + buttonWidth + buttonSpacing, buttonsY, buttonWidth, buttonHeight);

            // Draw Close button
            Color closeButtonColor = ButtonColor;
            if (_buttonHovered["close"])
            {
                closeButtonColor = _buttonPressed["close"] ? ButtonPressColor : ButtonHoverColor;
            }

            DrawRect(new Rect2(buttonsX + (buttonWidth + buttonSpacing) * 2, buttonsY, buttonWidth, buttonHeight), closeButtonColor);
            DrawRect(new Rect2(buttonsX + (buttonWidth + buttonSpacing) * 2, buttonsY, buttonWidth, buttonHeight), BorderColor, false, BorderWidth);
            DrawPixelText("CLOSE", new Vector2(buttonsX + (buttonWidth + buttonSpacing) * 2 + buttonWidth / 2 - 15, buttonsY + buttonHeight / 2 + 3), TextColor, 1);
            _buttonRects["close"] = new Rect2(buttonsX + (buttonWidth + buttonSpacing) * 2, buttonsY, buttonWidth, buttonHeight);
        }

        // Draw the dragged item
        private void DrawDraggedItem()
        {
            if (_draggedItemId == null || !_inventorySystem.GetInventory().ContainsKey(_draggedItemId))
                return;

            var item = _inventorySystem.GetInventory()[_draggedItemId];

            // Calculate item position (centered on mouse)
            float itemSize = SlotSize;
            float itemX = _mousePosition.X - itemSize / 2;
            float itemY = _mousePosition.Y - itemSize / 2;

            // Draw item background
            DrawRect(new Rect2(itemX, itemY, itemSize, itemSize), new Color(0.15f, 0.15f, 0.15f, 0.8f));

            // Draw item icon from atlas if available
            var iconAtlas = GetNode<ItemIconAtlas>("/root/ItemIconAtlas");
            if (iconAtlas != null && iconAtlas.GetAtlasTexture() != null)
            {
                // Draw icon from atlas
                Rect2 iconRegion = iconAtlas.GetIconRegion(item.IconIndex);
                DrawTextureRectRegion(
                    iconAtlas.GetAtlasTexture(),
                    new Rect2(itemX + 4, itemY + 4, itemSize - 8, itemSize - 8),
                    iconRegion
                );
            }
            else
            {
                // Fallback to colored square
                Color itemColor = GetItemColor(item.Category);
                DrawRect(new Rect2(itemX + 4, itemY + 4, itemSize - 8, itemSize - 8), itemColor);

                // Draw item initial
                string initial = item.Name.Length > 0 ? item.Name[0].ToString() : "?";
                DrawPixelText(initial, new Vector2(itemX + itemSize / 2 - 3, itemY + itemSize / 2 + 3), TextColor, 1);
            }

            // Draw quantity if more than 1
            if (item.Quantity > 1)
            {
                string quantityText = item.Quantity.ToString();
                DrawPixelText(quantityText, new Vector2(itemX + itemSize - 4 - quantityText.Length * 6, itemY + itemSize - 10), TextColor, 1);
            }
        }

        // Draw tooltip
        private void DrawTooltip()
        {
            if (!_showTooltip || string.IsNullOrEmpty(_tooltipText))
                return;

            // Calculate tooltip dimensions
            float padding = 5;
            float lineHeight = 15;
            string[] lines = _tooltipText.Split('\n');
            float maxLineWidth = 0;

            foreach (var line in lines)
            {
                maxLineWidth = Mathf.Max(maxLineWidth, line.Length * 6);
            }

            float tooltipWidth = maxLineWidth + padding * 2;
            float tooltipHeight = lines.Length * lineHeight + padding * 2;

            // Adjust position to keep tooltip on screen
            Vector2 position = _tooltipPosition;
            if (position.X + tooltipWidth > Size.X)
            {
                position.X = Size.X - tooltipWidth;
            }

            if (position.Y + tooltipHeight > Size.Y)
            {
                position.Y = Size.Y - tooltipHeight;
            }

            // Draw tooltip background
            DrawRect(new Rect2(position.X, position.Y, tooltipWidth, tooltipHeight), new Color(0.1f, 0.1f, 0.1f, 0.9f));
            DrawRect(new Rect2(position.X, position.Y, tooltipWidth, tooltipHeight), BorderColor, false, BorderWidth);

            // Draw tooltip text
            for (int i = 0; i < lines.Length; i++)
            {
                DrawPixelText(lines[i], new Vector2(position.X + padding, position.Y + padding + i * lineHeight + 10), TextColor, 1);
            }
        }

        // Draw pixel text using the pixel font
        private void DrawPixelText(string text, Vector2 position, Color color, float scale = 1.0f)
        {
            if (string.IsNullOrEmpty(text))
                return;

            float x = position.X;
            float y = position.Y;

            foreach (char c in text)
            {
                if (c == ' ')
                {
                    x += 6 * scale;
                    continue;
                }

                var pattern = _pixelFont.GetCharacterPattern(c);

                for (int py = 0; py < pattern.GetLength(0); py++)
                {
                    for (int px = 0; px < pattern.GetLength(1); px++)
                    {
                        if (pattern[py, px])
                        {
                            DrawRect(new Rect2(
                                x + px * scale,
                                y + py * scale - 5 * scale,
                                scale,
                                scale),
                                color);
                        }
                    }
                }

                x += 6 * scale; // Character width + spacing
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
                default:
                    return new Color(0.5f, 0.5f, 0.5f, 1.0f); // Gray
            }
        }

        // Update tooltip based on mouse position
        private void UpdateTooltip()
        {
            _showTooltip = false;
            _tooltipText = "";

            if (!ShowTooltips)
                return;

            // Check if mouse is over an item slot
            foreach (var itemId in _itemSlotRects.Keys)
            {
                if (_itemSlotRects[itemId].HasPoint(_mousePosition))
                {
                    var item = _inventorySystem.GetInventory()[itemId];

                    // Build tooltip text with more details
                    _tooltipText = $"{item.Name}\n[{item.Category.ToUpper()}]\n{item.Description}";

                    // Add properties
                    var properties = new List<string>();
                    if (item.IsUsable) properties.Add("Usable");
                    if (item.IsConsumable) properties.Add("Consumable");
                    if (item.IsEquippable) properties.Add("Equippable");
                    if (item.IsCombineable) properties.Add("Combineable");

                    if (properties.Count > 0)
                    {
                        _tooltipText += $"\nProperties: {string.Join(", ", properties)}";
                    }

                    // Add effects if any
                    if (item.Effects.Count > 0)
                    {
                        _tooltipText += "\nEffects:";
                        foreach (var effect in item.Effects)
                        {
                            _tooltipText += $"\n- {effect.Key}: {effect.Value}";
                        }
                    }

                    // Add content if it's a document
                    if (item.Category == "document" && !string.IsNullOrEmpty(item.Content))
                    {
                        _tooltipText += $"\n\nContent: {item.Content}";
                    }

                    _showTooltip = true;
                    return;
                }
            }

            // Check if mouse is over a button
            if (_buttonRects.ContainsKey("use") && _buttonRects["use"].HasPoint(_mousePosition))
            {
                _tooltipText = "Use the selected item";
                _showTooltip = true;
            }
            else if (_buttonRects.ContainsKey("drop") && _buttonRects["drop"].HasPoint(_mousePosition))
            {
                _tooltipText = "Drop the selected item";
                _showTooltip = true;
            }
            else if (_buttonRects.ContainsKey("close") && _buttonRects["close"].HasPoint(_mousePosition))
            {
                _tooltipText = "Close the inventory";
                _showTooltip = true;
            }
        }

        // Handle button clicks
        private void HandleButtonClick(string buttonId)
        {
            switch (buttonId)
            {
                case "use":
                    UseItem(_selectedItemId);
                    break;
                case "drop":
                    DropItem(_selectedItemId);
                    break;
                case "close":
                    SetVisible(false);
                    break;
            }
        }

        // Handle item drop (when dragging and dropping)
        private void HandleItemDrop(string sourceItemId, string targetItemId)
        {
            // For now, just swap the items in the UI
            // In a real implementation, you might want to stack items or implement other logic
            _selectedItemId = targetItemId;
            QueueRedraw();
        }

        // Use an item
        private void UseItem(string itemId)
        {
            if (itemId == null || !_inventorySystem.GetInventory().ContainsKey(itemId))
                return;

            var item = _inventorySystem.GetInventory()[itemId];

            if (!item.IsUsable)
                return;

            _inventorySystem.UseItem(itemId);

            // If the item was consumed and no longer exists, clear selection
            if (!_inventorySystem.GetInventory().ContainsKey(itemId))
            {
                _selectedItemId = null;
            }
        }

        // Drop an item
        private void DropItem(string itemId)
        {
            if (itemId == null || !_inventorySystem.GetInventory().ContainsKey(itemId))
                return;

            _inventorySystem.RemoveItemFromInventory(itemId, 1);

            // If the item was completely removed, clear selection
            if (!_inventorySystem.GetInventory().ContainsKey(itemId))
            {
                _selectedItemId = null;
            }
        }

        // Called when the inventory changes
        private void OnInventoryChanged(string itemId, int quantity)
        {
            QueueRedraw();
        }

        // Called when the inventory changes (from GameState)
        private void OnInventoryChanged()
        {
            QueueRedraw();
        }
    }
}

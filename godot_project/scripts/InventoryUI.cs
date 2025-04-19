using Godot;

namespace SignalLost
{
    [GlobalClass]
    public partial class InventoryUI : Control
    {
        // References to UI elements
        private GridContainer _itemGrid;
        private Label _inventoryTitle;
        private Label _itemCount;
        private Panel _itemInfoPanel;
        private Label _itemName;
        private Label _itemDescription;
        private Label _itemCategory;
        private Label _itemQuantity;
        private Button _useButton;
        private Button _dropButton;
        private Button _closeButton;

        // References to game systems
        private InventorySystem _inventorySystem;
        private GameState _gameState;

        // Currently selected item
        private string _selectedItemId;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _itemGrid = GetNode<GridContainer>("ItemGrid");
            _inventoryTitle = GetNode<Label>("InventoryTitle");
            _itemCount = GetNode<Label>("ItemCount");
            _itemInfoPanel = GetNode<Panel>("ItemInfoPanel");
            _itemName = GetNode<Label>("ItemInfoPanel/ItemName");
            _itemDescription = GetNode<Label>("ItemInfoPanel/ItemDescription");
            _itemCategory = GetNode<Label>("ItemInfoPanel/ItemCategory");
            _itemQuantity = GetNode<Label>("ItemInfoPanel/ItemQuantity");
            _useButton = GetNode<Button>("ItemInfoPanel/UseButton");
            _dropButton = GetNode<Button>("ItemInfoPanel/DropButton");
            _closeButton = GetNode<Button>("CloseButton");

            // Get references to game systems
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_inventorySystem == null || _gameState == null)
            {
                GD.PrintErr("InventoryUI: Failed to get InventorySystem or GameState reference");
                return;
            }

            // Connect signals
            _closeButton.Pressed += OnCloseButtonPressed;
            _useButton.Pressed += OnUseButtonPressed;
            _dropButton.Pressed += OnDropButtonPressed;
            _inventorySystem.ItemAdded += (itemId, quantity) => OnInventoryChanged();
            _inventorySystem.ItemRemoved += (itemId, quantity) => OnInventoryChanged();
            _gameState.InventoryChanged += OnInventoryChanged;

            // Initialize the UI
            UpdateInventoryUI();
            ClearItemInfo();
        }

        // Update the inventory UI
        private void UpdateInventoryUI()
        {
            // Clear the item grid
            foreach (var child in _itemGrid.GetChildren())
            {
                child.QueueFree();
            }

            // Get the inventory
            var inventory = _inventorySystem.GetInventory();

            // Update the item count
            _itemCount.Text = $"Items: {_inventorySystem.GetTotalItemCount()} / {_inventorySystem.GetMaxInventorySize()}";

            // Add items to the grid
            foreach (var item in inventory.Values)
            {
                var itemButton = new Button();
                itemButton.Text = item.Name;
                itemButton.TooltipText = $"{item.Name} (x{item.Quantity})";
                itemButton.CustomMinimumSize = new Vector2(100, 100);
                itemButton.Pressed += () => OnItemButtonPressed(item.Id);

                // Add an icon if available
                if (!string.IsNullOrEmpty(item.IconPath) && ResourceLoader.Exists(item.IconPath))
                {
                    var texture = ResourceLoader.Load<Texture2D>(item.IconPath);
                    if (texture != null)
                    {
                        itemButton.Icon = texture;
                        itemButton.ExpandIcon = true;
                    }
                }

                _itemGrid.AddChild(itemButton);
            }
        }

        // Show item info
        private void ShowItemInfo(string itemId)
        {
            var item = _inventorySystem.GetInventory()[itemId];
            _selectedItemId = itemId;

            _itemName.Text = item.Name;
            _itemDescription.Text = item.Description;
            _itemCategory.Text = $"Category: {item.Category}";
            _itemQuantity.Text = $"Quantity: {item.Quantity}";

            _useButton.Disabled = !item.IsUsable;
            _itemInfoPanel.Visible = true;
        }

        // Clear item info
        private void ClearItemInfo()
        {
            _selectedItemId = null;
            _itemName.Text = "";
            _itemDescription.Text = "";
            _itemCategory.Text = "";
            _itemQuantity.Text = "";
            _itemInfoPanel.Visible = false;
        }

        // Event handlers
        private void OnCloseButtonPressed()
        {
            // Hide the inventory UI
            Visible = false;
        }

        private void OnItemButtonPressed(string itemId)
        {
            // Show item info
            ShowItemInfo(itemId);
        }

        private void OnUseButtonPressed()
        {
            if (_selectedItemId != null)
            {
                _inventorySystem.UseItem(_selectedItemId);
                UpdateInventoryUI();

                // If the item was consumed, clear the item info
                if (!_inventorySystem.GetInventory().ContainsKey(_selectedItemId))
                {
                    ClearItemInfo();
                }
                else
                {
                    // Otherwise, update the item info
                    ShowItemInfo(_selectedItemId);
                }
            }
        }

        private void OnDropButtonPressed()
        {
            if (_selectedItemId != null)
            {
                _inventorySystem.RemoveItemFromInventory(_selectedItemId, 1);
                UpdateInventoryUI();

                // If the item was completely removed, clear the item info
                if (!_inventorySystem.GetInventory().ContainsKey(_selectedItemId))
                {
                    ClearItemInfo();
                }
                else
                {
                    // Otherwise, update the item info
                    ShowItemInfo(_selectedItemId);
                }
            }
        }

        private void OnInventoryChanged()
        {
            UpdateInventoryUI();

            // If the selected item was changed, update the item info
            if (_selectedItemId != null)
            {
                if (_inventorySystem.GetInventory().ContainsKey(_selectedItemId))
                {
                    ShowItemInfo(_selectedItemId);
                }
                else
                {
                    ClearItemInfo();
                }
            }
        }
    }
}

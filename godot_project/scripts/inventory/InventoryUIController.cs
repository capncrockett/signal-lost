using Godot;
using System.Collections.Generic;
using System.Linq;
using SignalLost.Inventory.UI;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Controller for the inventory UI.
    /// </summary>
    [GlobalClass]
    public partial class InventoryUIController : Control
    {
        // References to UI elements
        private Label _itemNameLabel;
        private Label _categoryLabel;
        private Label _descriptionLabel;
        private VBoxContainer _propertiesContainer;
        private Button _useButton;
        private Button _equipButton;
        private Button _dropButton;
        private OptionButton _combineOptions;
        private Button _combineButton;
        private Button _closeButton;
        private Label _statusLabel;
        private GridContainer _inventoryGrid;

        // Reference to the inventory system
        private InventorySystemAdapter _inventorySystem;

        // Reference to the item effects system
        private ItemEffectsSystem _itemEffectsSystem;

        // Reference to the item combination system
        private ItemCombinationSystem _itemCombinationSystem;

        // Reference to the item icon atlas
        private ItemIconAtlas _itemIconAtlas;

        // Currently selected item
        private string _selectedItemId;

        // Dictionary of inventory slots
        private Dictionary<string, InventorySlotUI> _inventorySlots = new Dictionary<string, InventorySlotUI>();

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _itemNameLabel = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ItemDetails/ItemNameLabel");
            _categoryLabel = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ItemDetails/CategoryLabel");
            _descriptionLabel = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ItemDetails/DescriptionLabel");
            _propertiesContainer = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/HBoxContainer/ItemDetails/PropertiesContainer");
            _useButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemDetails/ActionButtons/UseButton");
            _equipButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemDetails/ActionButtons/EquipButton");
            _dropButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemDetails/ActionButtons/DropButton");
            _combineOptions = GetNode<OptionButton>("MarginContainer/VBoxContainer/HBoxContainer/ItemDetails/CombineContainer/CombineOptions");
            _combineButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemDetails/CombineContainer/CombineButton");
            _closeButton = GetNode<Button>("MarginContainer/VBoxContainer/FooterButtons/CloseButton");
            _statusLabel = GetNode<Label>("MarginContainer/VBoxContainer/StatusLabel");
            _inventoryGrid = GetNode<GridContainer>("MarginContainer/VBoxContainer/HBoxContainer/InventoryGrid");

            // Get references to systems
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");
            _itemEffectsSystem = GetNode<ItemEffectsSystem>("/root/ItemEffectsSystem");
            _itemCombinationSystem = GetNode<ItemCombinationSystem>("/root/ItemCombinationSystem");
            _itemIconAtlas = GetNode<ItemIconAtlas>("/root/ItemIconAtlas");

            // Connect signals
            _useButton.Pressed += OnUseButtonPressed;
            _equipButton.Pressed += OnEquipButtonPressed;
            _dropButton.Pressed += OnDropButtonPressed;
            _combineButton.Pressed += OnCombineButtonPressed;
            _closeButton.Pressed += OnCloseButtonPressed;

            // Connect inventory system signals
            _inventorySystem.ItemAdded += OnInventoryChanged;
            _inventorySystem.ItemRemoved += OnInventoryChanged;
            _inventorySystem.ItemUsed += OnItemUsed;
            _inventorySystem.ItemEquipped += OnItemEquipped;
            _inventorySystem.ItemUnequipped += OnItemUnequipped;
            _inventorySystem.ItemsCombined += OnItemsCombined;

            // Initialize UI
            UpdateInventoryGrid();
            UpdateItemDetails();
        }

        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect inventory system signals
            _inventorySystem.ItemAdded -= OnInventoryChanged;
            _inventorySystem.ItemRemoved -= OnInventoryChanged;
            _inventorySystem.ItemUsed -= OnItemUsed;
            _inventorySystem.ItemEquipped -= OnItemEquipped;
            _inventorySystem.ItemUnequipped -= OnItemUnequipped;
            _inventorySystem.ItemsCombined -= OnItemsCombined;
        }

        // Update the inventory grid
        private void UpdateInventoryGrid()
        {
            // Clear existing slots
            foreach (var slot in _inventorySlots.Values)
            {
                slot.QueueFree();
            }
            _inventorySlots.Clear();

            // Get inventory items
            var inventory = _inventorySystem.GetInventory();

            // Create slots for each item
            foreach (var item in inventory.Values)
            {
                // Create a new slot
                var slot = new InventorySlotUI();
                slot.SetItemId(item.Id);
                slot.CustomMinimumSize = new Vector2(50, 50);

                // Connect signals
                slot.SlotSelected += OnSlotSelected;
                slot.SlotClicked += OnSlotClicked;
                slot.SlotHovered += OnSlotHovered;

                // Add to grid
                _inventoryGrid.AddChild(slot);
                _inventorySlots[item.Id] = slot;
            }

            // Add empty slots to fill the grid
            int emptySlots = _inventorySystem.GetMaxInventorySize() - inventory.Count;
            for (int i = 0; i < emptySlots; i++)
            {
                // Create a new empty slot
                var slot = new InventorySlotUI();
                slot.CustomMinimumSize = new Vector2(50, 50);

                // Add to grid
                _inventoryGrid.AddChild(slot);
            }
        }

        // Update the item details panel
        private void UpdateItemDetails()
        {
            // Clear properties container
            foreach (var child in _propertiesContainer.GetChildren())
            {
                if (child != _propertiesContainer.GetNode("PropertiesLabel"))
                {
                    child.QueueFree();
                }
            }

            // Clear combine options
            _combineOptions.Clear();

            if (_selectedItemId == null || !_inventorySystem.HasItem(_selectedItemId))
            {
                // No item selected
                _itemNameLabel.Text = "No Item Selected";
                _categoryLabel.Text = "";
                _descriptionLabel.Text = "Select an item to view details";
                _useButton.Disabled = true;
                _equipButton.Disabled = true;
                _dropButton.Disabled = true;
                _combineOptions.Disabled = true;
                _combineButton.Disabled = true;
                return;
            }

            // Get the selected item
            var item = _inventorySystem.GetItem(_selectedItemId);

            // Update item details
            _itemNameLabel.Text = item.Name;
            _categoryLabel.Text = $"[{item.Category.ToUpper()}]";
            _descriptionLabel.Text = item.Description;

            // Update properties
            var properties = new List<string>();
            if (item.IsUsable) properties.Add("Usable");
            if (item.IsConsumable) properties.Add("Consumable");
            if (item.IsEquippable) properties.Add("Equippable");
            if (item.IsCombineable) properties.Add("Combineable");

            foreach (var property in properties)
            {
                var label = new Label();
                label.Text = $"- {property}";
                _propertiesContainer.AddChild(label);
            }

            // Update buttons
            _useButton.Disabled = !item.IsUsable;
            _equipButton.Disabled = !item.IsEquippable;
            _dropButton.Disabled = false;

            // Update combine options
            _combineOptions.Disabled = !item.IsCombineable;
            _combineButton.Disabled = !item.IsCombineable;

            if (item.IsCombineable)
            {
                // Add combine options
                _combineOptions.AddItem("Select an item...", 0);
                _combineOptions.Selected = 0;

                int index = 1;
                foreach (var inventoryItem in _inventorySystem.GetInventory().Values)
                {
                    if (inventoryItem.Id != _selectedItemId && inventoryItem.IsCombineable &&
                        _itemCombinationSystem.CanCombineItems(_selectedItemId, inventoryItem.Id))
                    {
                        _combineOptions.AddItem(inventoryItem.Name, index);
                        _combineOptions.SetItemMetadata(index, inventoryItem.Id);
                        index++;
                    }
                }

                _combineButton.Disabled = index == 1; // Disable if no compatible items
            }

            // Update status
            _statusLabel.Text = $"Selected: {item.Name}";
        }

        // Handle slot selection
        private void OnSlotSelected(string itemId)
        {
            _selectedItemId = itemId;
            UpdateItemDetails();

            // Update slot selection
            foreach (var slot in _inventorySlots.Values)
            {
                slot.SetSelected(slot.GetItemId() == _selectedItemId);
            }
        }

        // Handle slot click
        private void OnSlotClicked(string itemId, int buttonIndex)
        {
            if (buttonIndex == 1) // Left click
            {
                OnSlotSelected(itemId);
            }
            else if (buttonIndex == 2) // Right click
            {
                if (itemId != null && _inventorySystem.HasItem(itemId))
                {
                    var item = _inventorySystem.GetItem(itemId);
                    if (item.IsUsable)
                    {
                        _inventorySystem.UseItem(itemId);
                    }
                }
            }
        }

        // Handle slot hover
        private void OnSlotHovered(string itemId)
        {
            if (itemId != null && _inventorySystem.HasItem(itemId))
            {
                var item = _inventorySystem.GetItem(itemId);
                _statusLabel.Text = item.Name;
            }
            else
            {
                _statusLabel.Text = "Empty Slot";
            }
        }

        // Handle use button press
        private void OnUseButtonPressed()
        {
            if (_selectedItemId != null && _inventorySystem.HasItem(_selectedItemId))
            {
                _inventorySystem.UseItem(_selectedItemId);
            }
        }

        // Handle equip button press
        private void OnEquipButtonPressed()
        {
            if (_selectedItemId != null && _inventorySystem.HasItem(_selectedItemId))
            {
                if (_inventorySystem.IsItemEquipped(_selectedItemId))
                {
                    _inventorySystem.UnequipItem(_selectedItemId);
                    _equipButton.Text = "EQUIP";
                }
                else
                {
                    _inventorySystem.EquipItem(_selectedItemId);
                    _equipButton.Text = "UNEQUIP";
                }
            }
        }

        // Handle drop button press
        private void OnDropButtonPressed()
        {
            if (_selectedItemId != null && _inventorySystem.HasItem(_selectedItemId))
            {
                _inventorySystem.RemoveItemFromInventory(_selectedItemId, 1);
                if (!_inventorySystem.HasItem(_selectedItemId))
                {
                    _selectedItemId = null;
                }
                UpdateItemDetails();
            }
        }

        // Handle combine button press
        private void OnCombineButtonPressed()
        {
            if (_selectedItemId != null && _inventorySystem.HasItem(_selectedItemId) &&
                _combineOptions.Selected > 0)
            {
                string otherItemId = (string)_combineOptions.GetSelectedMetadata();
                if (_inventorySystem.HasItem(otherItemId))
                {
                    _inventorySystem.CombineItems(_selectedItemId, otherItemId);
                }
            }
        }

        // Handle close button press
        private void OnCloseButtonPressed()
        {
            Hide();
        }

        // Handle inventory changes
        private void OnInventoryChanged(string itemId, int quantity)
        {
            UpdateInventoryGrid();
            UpdateItemDetails();
        }

        // Handle item used
        private void OnItemUsed(string itemId)
        {
            if (itemId == _selectedItemId && !_inventorySystem.HasItem(itemId))
            {
                _selectedItemId = null;
            }
            UpdateItemDetails();
            _statusLabel.Text = $"Used: {_inventorySystem.GetItem(itemId)?.Name ?? itemId}";
        }

        // Handle item equipped
        private void OnItemEquipped(string itemId)
        {
            if (itemId == _selectedItemId)
            {
                _equipButton.Text = "UNEQUIP";
            }
            UpdateInventoryGrid(); // To update equipped indicator
            _statusLabel.Text = $"Equipped: {_inventorySystem.GetItem(itemId)?.Name ?? itemId}";
        }

        // Handle item unequipped
        private void OnItemUnequipped(string itemId)
        {
            if (itemId == _selectedItemId)
            {
                _equipButton.Text = "EQUIP";
            }
            UpdateInventoryGrid(); // To update equipped indicator
            _statusLabel.Text = $"Unequipped: {_inventorySystem.GetItem(itemId)?.Name ?? itemId}";
        }

        // Handle items combined
        private void OnItemsCombined(string item1Id, string item2Id, string resultId)
        {
            if (item1Id == _selectedItemId || item2Id == _selectedItemId)
            {
                _selectedItemId = resultId;
            }
            UpdateInventoryGrid();
            UpdateItemDetails();
            _statusLabel.Text = $"Combined items to create: {_inventorySystem.GetItem(resultId)?.Name ?? resultId}";
        }
    }
}

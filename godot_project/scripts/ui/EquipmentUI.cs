using Godot;
using System;
using System.Collections.Generic;
using SignalLost.Equipment;
using SignalLost.Inventory;
using SignalLost.Radio;

namespace SignalLost.UI
{
    /// <summary>
    /// UI for managing equipment.
    /// </summary>
    [GlobalClass]
    public partial class EquipmentUI : Control
    {
        // References to UI elements
        private Control _slotContainer;
        private Panel _itemInfoPanel;
        private Label _itemNameLabel;
        private Label _itemDescriptionLabel;
        private RichTextLabel _itemEffectsLabel;
        private Button _equipButton;
        private Button _unequipButton;
        private Button _closeButton;
        private Label _statusLabel;
        private TabContainer _tabContainer;
        private Control _radioUpgradesTab;
        private GridContainer _inventoryGrid;

        // Equipment slot controls
        private Dictionary<EquipmentSystem.EquipmentSlot, TextureRect> _slotControls = new Dictionary<EquipmentSystem.EquipmentSlot, TextureRect>();

        // Radio upgrade controls
        private Dictionary<RadioEquipmentManager.ComponentType, Button> _upgradeButtons = new Dictionary<RadioEquipmentManager.ComponentType, Button>();
        private Dictionary<RadioEquipmentManager.ComponentType, Label> _levelLabels = new Dictionary<RadioEquipmentManager.ComponentType, Label>();

        // References to systems
        private EquipmentSystem _equipmentSystem;
        private RadioEquipmentManager _radioEquipmentManager;
        private InventorySystemAdapter _inventorySystem;

        // State
        private string _selectedItemId = "";
        private EquipmentSystem.EquipmentSlot? _selectedSlot = null;

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references to UI elements
            _slotContainer = GetNode<Control>("SlotContainer");
            _itemInfoPanel = GetNode<Panel>("ItemInfoPanel");
            _itemNameLabel = GetNode<Label>("ItemInfoPanel/ItemName");
            _itemDescriptionLabel = GetNode<Label>("ItemInfoPanel/ItemDescription");
            _itemEffectsLabel = GetNode<RichTextLabel>("ItemInfoPanel/ItemEffects");
            _equipButton = GetNode<Button>("ItemInfoPanel/EquipButton");
            _unequipButton = GetNode<Button>("ItemInfoPanel/UnequipButton");
            _closeButton = GetNode<Button>("CloseButton");
            _statusLabel = GetNode<Label>("StatusLabel");
            _tabContainer = GetNode<TabContainer>("TabContainer");
            _radioUpgradesTab = GetNode<Control>("TabContainer/RadioUpgrades");
            _inventoryGrid = GetNode<GridContainer>("TabContainer/Inventory/ItemGrid");

            // Get references to systems
            _equipmentSystem = GetNode<EquipmentSystem>("/root/EquipmentSystem");
            _radioEquipmentManager = GetNode<RadioEquipmentManager>("/root/RadioEquipmentManager");
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");

            // Initialize equipment slots
            InitializeEquipmentSlots();

            // Initialize radio upgrade controls
            InitializeRadioUpgradeControls();

            // Connect signals
            _equipButton.Pressed += OnEquipButtonPressed;
            _unequipButton.Pressed += OnUnequipButtonPressed;
            _closeButton.Pressed += OnCloseButtonPressed;

            // Connect system signals
            if (_equipmentSystem != null)
            {
                _equipmentSystem.ItemEquipped += OnItemEquipped;
                _equipmentSystem.ItemUnequipped += OnItemUnequipped;
            }

            if (_radioEquipmentManager != null)
            {
                _radioEquipmentManager.ComponentUpgraded += OnComponentUpgraded;
                _radioEquipmentManager.RadioEquipmentChanged += OnRadioEquipmentChanged;
            }

            if (_inventorySystem != null)
            {
                _inventorySystem.ItemAdded += OnInventoryChanged;
                _inventorySystem.ItemRemoved += OnInventoryChanged;
            }

            // Initialize UI
            UpdateEquipmentSlots();
            UpdateRadioUpgradeControls();
            PopulateInventoryGrid();
            ClearItemInfo();

            GD.Print("EquipmentUI: Initialized");
        }

        /// <summary>
        /// Called when the node is about to be removed from the scene tree.
        /// </summary>
        public override void _ExitTree()
        {
            // Disconnect system signals
            if (_equipmentSystem != null)
            {
                _equipmentSystem.ItemEquipped -= OnItemEquipped;
                _equipmentSystem.ItemUnequipped -= OnItemUnequipped;
            }

            if (_radioEquipmentManager != null)
            {
                _radioEquipmentManager.ComponentUpgraded -= OnComponentUpgraded;
                _radioEquipmentManager.RadioEquipmentChanged -= OnRadioEquipmentChanged;
            }

            if (_inventorySystem != null)
            {
                _inventorySystem.ItemAdded -= OnInventoryChanged;
                _inventorySystem.ItemRemoved -= OnInventoryChanged;
            }
        }

        /// <summary>
        /// Initializes equipment slot controls.
        /// </summary>
        private void InitializeEquipmentSlots()
        {
            // Create a control for each equipment slot
            foreach (EquipmentSystem.EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSystem.EquipmentSlot)))
            {
                // Create a TextureRect for the slot
                TextureRect slotControl = new TextureRect();
                slotControl.Name = slot.ToString();
                slotControl.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                slotControl.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                slotControl.CustomMinimumSize = new Vector2(64, 64);
                slotControl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                slotControl.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
                slotControl.MouseFilter = Control.MouseFilterEnum.Stop;
                slotControl.TooltipText = slot.ToString();

                // Add a background panel
                Panel background = new Panel();
                background.Name = "Background";
                background.MouseFilter = Control.MouseFilterEnum.Ignore;
                background.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                background.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
                slotControl.AddChild(background);

                // Add a label for the slot name
                Label slotLabel = new Label();
                slotLabel.Name = "SlotLabel";
                slotLabel.Text = slot.ToString();
                slotLabel.HorizontalAlignment = HorizontalAlignment.Center;
                slotLabel.VerticalAlignment = VerticalAlignment.Bottom;
                slotLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
                slotControl.AddChild(slotLabel);

                // Add the slot control to the container
                _slotContainer.AddChild(slotControl);

                // Store the slot control
                _slotControls[slot] = slotControl;

                // Connect signals
                slotControl.GuiInput += (InputEvent @event) => OnSlotGuiInput(@event, slot);
            }
        }

        /// <summary>
        /// Initializes radio upgrade controls.
        /// </summary>
        private void InitializeRadioUpgradeControls()
        {
            // Create a control for each component type
            foreach (RadioEquipmentManager.ComponentType componentType in Enum.GetValues(typeof(RadioEquipmentManager.ComponentType)))
            {
                // Get the container for this component
                string containerName = $"{componentType}Container";
                Control container = _radioUpgradesTab.GetNodeOrNull<Control>(containerName);

                if (container == null)
                {
                    GD.PrintErr($"EquipmentUI: Container {containerName} not found");
                    continue;
                }

                // Get the level label
                Label levelLabel = container.GetNodeOrNull<Label>("LevelLabel");
                if (levelLabel != null)
                {
                    _levelLabels[componentType] = levelLabel;
                }

                // Get the upgrade button
                Button upgradeButton = container.GetNodeOrNull<Button>("UpgradeButton");
                if (upgradeButton != null)
                {
                    _upgradeButtons[componentType] = upgradeButton;

                    // Connect signal
                    upgradeButton.Pressed += () => OnUpgradeButtonPressed(componentType);
                }
            }
        }

        /// <summary>
        /// Updates the equipment slot controls to reflect the current equipment.
        /// </summary>
        private void UpdateEquipmentSlots()
        {
            // Get all equipped items
            Dictionary<EquipmentSystem.EquipmentSlot, string> equippedItems = _equipmentSystem.GetEquippedItems();

            // Update each slot control
            foreach (var slot in _slotControls.Keys)
            {
                TextureRect slotControl = _slotControls[slot];

                // Check if an item is equipped in this slot
                if (equippedItems.ContainsKey(slot))
                {
                    string itemId = equippedItems[slot];
                    ItemData item = _inventorySystem.GetItem(itemId);

                    if (item != null)
                    {
                        // Set the texture
                        slotControl.Texture = ResourceLoader.Load<Texture2D>(item.IconPath);

                        // Set the tooltip
                        slotControl.TooltipText = item.Name;
                    }
                    else
                    {
                        // Clear the texture
                        slotControl.Texture = null;

                        // Set the tooltip
                        slotControl.TooltipText = slot.ToString();
                    }
                }
                else
                {
                    // Clear the texture
                    slotControl.Texture = null;

                    // Set the tooltip
                    slotControl.TooltipText = slot.ToString();
                }
            }
        }

        /// <summary>
        /// Updates the radio upgrade controls to reflect the current radio equipment.
        /// </summary>
        private void UpdateRadioUpgradeControls()
        {
            // Check if a radio is equipped
            string radioItemId = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Radio);
            bool isCustomRadio = radioItemId == "radio_custom";

            // Enable/disable the radio upgrades tab
            _radioUpgradesTab.Visible = isCustomRadio;

            if (!isCustomRadio)
            {
                return;
            }

            // Update each component control
            foreach (RadioEquipmentManager.ComponentType componentType in Enum.GetValues(typeof(RadioEquipmentManager.ComponentType)))
            {
                // Get the current level
                int currentLevel = _radioEquipmentManager.GetComponentLevel(componentType);
                int maxLevel = _radioEquipmentManager.GetMaxComponentLevel(componentType);

                // Update the level label
                if (_levelLabels.ContainsKey(componentType))
                {
                    _levelLabels[componentType].Text = $"Level: {currentLevel}/{maxLevel}";
                }

                // Update the upgrade button
                if (_upgradeButtons.ContainsKey(componentType))
                {
                    _upgradeButtons[componentType].Disabled = currentLevel >= maxLevel;
                }
            }
        }

        /// <summary>
        /// Populates the inventory grid with items.
        /// </summary>
        private void PopulateInventoryGrid()
        {
            // Clear the grid
            foreach (Node child in _inventoryGrid.GetChildren())
            {
                child.QueueFree();
            }

            // Get all items in inventory
            var inventory = _inventorySystem.GetInventory();

            // Filter for equippable items
            List<ItemData> equippableItems = new List<ItemData>();
            foreach (var item in inventory.Values)
            {
                if (item.IsEquippable)
                {
                    equippableItems.Add(item);
                }
            }

            // Add each item to the grid
            foreach (ItemData item in equippableItems)
            {
                // Create a TextureRect for the item
                TextureRect itemControl = new TextureRect();
                itemControl.Name = item.Id;
                itemControl.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                itemControl.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                itemControl.CustomMinimumSize = new Vector2(64, 64);
                itemControl.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                itemControl.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
                itemControl.MouseFilter = Control.MouseFilterEnum.Stop;
                itemControl.TooltipText = item.Name;

                // Set the texture
                itemControl.Texture = ResourceLoader.Load<Texture2D>(item.IconPath);

                // Add a background panel
                Panel background = new Panel();
                background.Name = "Background";
                background.MouseFilter = Control.MouseFilterEnum.Ignore;
                background.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
                background.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
                itemControl.AddChild(background);

                // Add the item control to the grid
                _inventoryGrid.AddChild(itemControl);

                // Connect signals
                itemControl.GuiInput += (InputEvent @event) => OnItemGuiInput(@event, item.Id);
            }
        }

        /// <summary>
        /// Handles GUI input on a slot control.
        /// </summary>
        /// <param name="event">The input event</param>
        /// <param name="slot">The slot that received the input</param>
        private void OnSlotGuiInput(InputEvent @event, EquipmentSystem.EquipmentSlot slot)
        {
            // Check for left click
            if (@event is InputEventMouseButton mouseButton &&
                mouseButton.ButtonIndex == MouseButton.Left &&
                mouseButton.Pressed)
            {
                // Select the slot
                _selectedSlot = slot;
                _selectedItemId = _equipmentSystem.GetEquippedItem(slot);

                // Update item info
                if (!string.IsNullOrEmpty(_selectedItemId))
                {
                    ShowItemInfo(_selectedItemId);
                    _equipButton.Visible = false;
                    _unequipButton.Visible = true;
                }
                else
                {
                    ClearItemInfo();
                    _equipButton.Visible = false;
                    _unequipButton.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles GUI input on an item control.
        /// </summary>
        /// <param name="event">The input event</param>
        /// <param name="itemId">The ID of the item that received the input</param>
        private void OnItemGuiInput(InputEvent @event, string itemId)
        {
            // Check for left click
            if (@event is InputEventMouseButton mouseButton &&
                mouseButton.ButtonIndex == MouseButton.Left &&
                mouseButton.Pressed)
            {
                // Select the item
                _selectedItemId = itemId;
                _selectedSlot = null;

                // Update item info
                ShowItemInfo(itemId);
                _equipButton.Visible = true;
                _unequipButton.Visible = false;
            }
        }

        /// <summary>
        /// Shows information about an item.
        /// </summary>
        /// <param name="itemId">The ID of the item to show information for</param>
        private void ShowItemInfo(string itemId)
        {
            // Get the item
            ItemData item = _inventorySystem.GetItem(itemId);

            if (item == null)
            {
                ClearItemInfo();
                return;
            }

            // Set item info
            _itemNameLabel.Text = item.Name;
            _itemDescriptionLabel.Text = item.Description;

            // Set item effects
            Dictionary<string, float> effects = _equipmentSystem.GetItemEffects(itemId);
            string effectsText = "";

            foreach (var effect in effects)
            {
                string effectName = FormatEffectName(effect.Key);
                string effectValue = FormatEffectValue(effect.Value);
                string effectColor = effect.Value > 1.0f ? "green" : (effect.Value < 1.0f ? "red" : "white");

                effectsText += $"[color={effectColor}]{effectName}: {effectValue}[/color]\n";
            }

            _itemEffectsLabel.Text = effectsText;

            // Show the panel
            _itemInfoPanel.Visible = true;
        }

        /// <summary>
        /// Clears the item information panel.
        /// </summary>
        private void ClearItemInfo()
        {
            _itemNameLabel.Text = "";
            _itemDescriptionLabel.Text = "";
            _itemEffectsLabel.Text = "";
            _itemInfoPanel.Visible = false;
        }

        /// <summary>
        /// Formats an effect name for display.
        /// </summary>
        /// <param name="effectName">The effect name to format</param>
        /// <returns>The formatted effect name</returns>
        private string FormatEffectName(string effectName)
        {
            // Split by underscore and capitalize each word
            string[] words = effectName.Split('_');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
                }
            }

            return string.Join(" ", words);
        }

        /// <summary>
        /// Formats an effect value for display.
        /// </summary>
        /// <param name="effectValue">The effect value to format</param>
        /// <returns>The formatted effect value</returns>
        private string FormatEffectValue(float effectValue)
        {
            // For multiplicative effects
            if (effectValue != 1.0f)
            {
                if (effectValue > 1.0f)
                {
                    return $"+{(effectValue - 1.0f) * 100:F0}%";
                }
                else
                {
                    return $"{(effectValue - 1.0f) * 100:F0}%";
                }
            }

            // For boolean effects
            return "Yes";
        }

        /// <summary>
        /// Handles the equip button being pressed.
        /// </summary>
        private void OnEquipButtonPressed()
        {
            if (string.IsNullOrEmpty(_selectedItemId))
            {
                return;
            }

            // Get the valid slots for this item
            List<EquipmentSystem.EquipmentSlot> validSlots = _equipmentSystem.GetValidSlots(_selectedItemId);

            if (validSlots.Count == 0)
            {
                ShowStatus("This item cannot be equipped");
                return;
            }

            // If a slot is selected and it's valid, equip to that slot
            if (_selectedSlot.HasValue && validSlots.Contains(_selectedSlot.Value))
            {
                bool success = _equipmentSystem.EquipItem(_selectedItemId, _selectedSlot.Value);

                if (success)
                {
                    ShowStatus($"Equipped {_selectedItemId} to {_selectedSlot.Value}");
                }
                else
                {
                    ShowStatus($"Failed to equip {_selectedItemId}");
                }
            }
            // Otherwise, equip to the first valid slot
            else
            {
                bool success = _equipmentSystem.EquipItem(_selectedItemId, validSlots[0]);

                if (success)
                {
                    ShowStatus($"Equipped {_selectedItemId} to {validSlots[0]}");
                }
                else
                {
                    ShowStatus($"Failed to equip {_selectedItemId}");
                }
            }
        }

        /// <summary>
        /// Handles the unequip button being pressed.
        /// </summary>
        private void OnUnequipButtonPressed()
        {
            if (!_selectedSlot.HasValue)
            {
                return;
            }

            bool success = _equipmentSystem.UnequipItem(_selectedSlot.Value);

            if (success)
            {
                ShowStatus($"Unequipped item from {_selectedSlot.Value}");
                ClearItemInfo();
            }
            else
            {
                ShowStatus($"Failed to unequip item from {_selectedSlot.Value}");
            }
        }

        /// <summary>
        /// Handles the close button being pressed.
        /// </summary>
        private void OnCloseButtonPressed()
        {
            Visible = false;
        }

        /// <summary>
        /// Handles the upgrade button being pressed.
        /// </summary>
        /// <param name="componentType">The type of component to upgrade</param>
        private void OnUpgradeButtonPressed(RadioEquipmentManager.ComponentType componentType)
        {
            bool success = _radioEquipmentManager.UpgradeComponent(componentType);

            if (success)
            {
                ShowStatus($"Upgraded {componentType}");
            }
            else
            {
                ShowStatus($"Failed to upgrade {componentType}");
            }
        }

        /// <summary>
        /// Shows a status message.
        /// </summary>
        /// <param name="message">The message to show</param>
        private void ShowStatus(string message)
        {
            _statusLabel.Text = message;

            // Reset the message after a delay
            GetTree().CreateTimer(3.0f).Timeout += () => _statusLabel.Text = "";
        }

        /// <summary>
        /// Handles an item being equipped.
        /// </summary>
        /// <param name="itemId">The ID of the equipped item</param>
        /// <param name="slotIndex">The index of the slot the item was equipped to</param>
        private void OnItemEquipped(string itemId, int slotIndex)
        {
            UpdateEquipmentSlots();
            UpdateRadioUpgradeControls();
        }

        /// <summary>
        /// Handles an item being unequipped.
        /// </summary>
        /// <param name="itemId">The ID of the unequipped item</param>
        /// <param name="slotIndex">The index of the slot the item was unequipped from</param>
        private void OnItemUnequipped(string itemId, int slotIndex)
        {
            UpdateEquipmentSlots();
            UpdateRadioUpgradeControls();
        }

        /// <summary>
        /// Handles a component being upgraded.
        /// </summary>
        /// <param name="componentTypeIndex">The index of the component type that was upgraded</param>
        /// <param name="level">The new level of the component</param>
        private void OnComponentUpgraded(int componentTypeIndex, int level)
        {
            UpdateRadioUpgradeControls();
        }

        /// <summary>
        /// Handles the radio equipment changing.
        /// </summary>
        /// <param name="radioTypeIndex">The index of the new radio type</param>
        private void OnRadioEquipmentChanged(int radioTypeIndex)
        {
            UpdateRadioUpgradeControls();
        }

        /// <summary>
        /// Handles the inventory changing.
        /// </summary>
        /// <param name="itemId">The ID of the item that was added or removed</param>
        /// <param name="quantity">The quantity that was added or removed</param>
        private void OnInventoryChanged(string itemId, int quantity)
        {
            PopulateInventoryGrid();
        }
    }
}

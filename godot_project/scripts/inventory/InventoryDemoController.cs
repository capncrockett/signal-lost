using Godot;
using System;

namespace SignalLost.Inventory
{
    /// <summary>
    /// Controller for the inventory demo scene.
    /// </summary>
    [GlobalClass]
    public partial class InventoryDemoController : Control
    {
        // References to UI elements
        private Button _addFlashlightButton;
        private Button _addBatteryButton;
        private Button _addMedkitButton;
        private Button _addRadioBrokenButton;
        private Button _addRadioPartButton;
        private Button _addKeyButton;
        private Button _addDocumentButton;
        private Button _showInventoryButton;
        private RichTextLabel _logText;
        private Button _clearLogButton;
        private Control _inventoryUI;

        // Reference to the inventory system
        private InventorySystemAdapter _inventorySystem;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _addFlashlightButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddFlashlightButton");
            _addBatteryButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddBatteryButton");
            _addMedkitButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddMedkitButton");
            _addRadioBrokenButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddRadioBrokenButton");
            _addRadioPartButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddRadioPartButton");
            _addKeyButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddKeyButton");
            _addDocumentButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddDocumentButton");
            _showInventoryButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/ShowInventoryButton");
            _logText = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/HBoxContainer/LogPanel/MarginContainer/VBoxContainer/LogText");
            _clearLogButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/LogPanel/MarginContainer/VBoxContainer/ClearLogButton");
            _inventoryUI = GetNode<Control>("InventoryUI");

            // Get reference to the inventory system
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");

            // Connect signals
            _addFlashlightButton.Pressed += () => AddItem("flashlight");
            _addBatteryButton.Pressed += () => AddItem("battery");
            _addMedkitButton.Pressed += () => AddItem("medkit");
            _addRadioBrokenButton.Pressed += () => AddItem("radio_broken");
            _addRadioPartButton.Pressed += () => AddItem("radio_part");
            _addKeyButton.Pressed += () => AddItem("key_cabin");
            _addDocumentButton.Pressed += () => AddItem("research_notes");
            _showInventoryButton.Pressed += ToggleInventoryUI;
            _clearLogButton.Pressed += ClearLog;

            // Connect inventory system signals
            _inventorySystem.ItemAdded += OnItemAdded;
            _inventorySystem.ItemRemoved += OnItemRemoved;
            _inventorySystem.ItemUsed += OnItemUsed;
            _inventorySystem.ItemEquipped += OnItemEquipped;
            _inventorySystem.ItemUnequipped += OnItemUnequipped;
            _inventorySystem.ItemsCombined += OnItemsCombined;
            _inventorySystem.InventoryFull += OnInventoryFull;

            // Initialize
            LogMessage("Inventory Demo initialized");
            LogMessage("Click the buttons to add items to your inventory");
            LogMessage("Click 'Show Inventory' to open the inventory UI");
        }

        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect inventory system signals
            _inventorySystem.ItemAdded -= OnItemAdded;
            _inventorySystem.ItemRemoved -= OnItemRemoved;
            _inventorySystem.ItemUsed -= OnItemUsed;
            _inventorySystem.ItemEquipped -= OnItemEquipped;
            _inventorySystem.ItemUnequipped -= OnItemUnequipped;
            _inventorySystem.ItemsCombined -= OnItemsCombined;
            _inventorySystem.InventoryFull -= OnInventoryFull;
        }

        // Add an item to the inventory
        private void AddItem(string itemId)
        {
            bool result = _inventorySystem.AddItemToInventory(itemId);
            if (result)
            {
                var item = _inventorySystem.GetItem(itemId);
                LogMessage($"Added {item.Name} to inventory");
            }
            else
            {
                LogMessage($"Failed to add {itemId} to inventory", Colors.Red);
            }
        }

        // Toggle the inventory UI
        private void ToggleInventoryUI()
        {
            _inventoryUI.Visible = !_inventoryUI.Visible;
            if (_inventoryUI.Visible)
            {
                LogMessage("Opened inventory UI");
            }
            else
            {
                LogMessage("Closed inventory UI");
            }
        }

        // Clear the log
        private void ClearLog()
        {
            _logText.Clear();
            LogMessage("Log cleared");
        }

        // Log a message
        private void LogMessage(string message, Color color = default)
        {
            if (color == default)
            {
                color = Colors.White;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string colorHex = color.ToHtml();
            _logText.AppendText($"[{timestamp}] [color=#{colorHex}]{message}[/color]\n");
        }

        // Handle item added
        private void OnItemAdded(string itemId, int quantity)
        {
            var item = _inventorySystem.GetItem(itemId);
            LogMessage($"Item added: {item.Name} x{quantity}");
        }

        // Handle item removed
        private void OnItemRemoved(string itemId, int quantity)
        {
            var item = _inventorySystem.GetItem(itemId);
            string itemName = item?.Name ?? itemId;
            LogMessage($"Item removed: {itemName} x{quantity}");
        }

        // Handle item used
        private void OnItemUsed(string itemId)
        {
            var item = _inventorySystem.GetItem(itemId);
            string itemName = item?.Name ?? itemId;
            LogMessage($"Item used: {itemName}", Colors.Green);
        }

        // Handle item equipped
        private void OnItemEquipped(string itemId)
        {
            var item = _inventorySystem.GetItem(itemId);
            string itemName = item?.Name ?? itemId;
            LogMessage($"Item equipped: {itemName}", Colors.Cyan);
        }

        // Handle item unequipped
        private void OnItemUnequipped(string itemId)
        {
            var item = _inventorySystem.GetItem(itemId);
            string itemName = item?.Name ?? itemId;
            LogMessage($"Item unequipped: {itemName}", Colors.Cyan);
        }

        // Handle items combined
        private void OnItemsCombined(string item1Id, string item2Id, string resultId)
        {
            var item1 = _inventorySystem.GetItem(item1Id);
            var item2 = _inventorySystem.GetItem(item2Id);
            var result = _inventorySystem.GetItem(resultId);

            string item1Name = item1?.Name ?? item1Id;
            string item2Name = item2?.Name ?? item2Id;
            string resultName = result?.Name ?? resultId;

            LogMessage($"Items combined: {item1Name} + {item2Name} = {resultName}", Colors.Yellow);
        }

        // Handle inventory full
        private void OnInventoryFull()
        {
            LogMessage("Inventory is full!", Colors.Red);
        }
    }
}

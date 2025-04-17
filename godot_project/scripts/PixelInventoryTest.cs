using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelInventoryTest : Control
    {
        // References to UI elements
        private PixelInventoryUI _inventoryUI;
        private Button _addFlashlightButton;
        private Button _addBatteryButton;
        private Button _addMedkitButton;
        private Button _addKeyButton;
        private Button _clearInventoryButton;
        private Button _takeScreenshotButton;

        // References to game systems
        private InventorySystem _inventorySystem;
        private GameState _gameState;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _inventoryUI = GetNode<PixelInventoryUI>("PixelInventoryUI");
            _addFlashlightButton = GetNode<Button>("TestButtons/AddFlashlightButton");
            _addBatteryButton = GetNode<Button>("TestButtons/AddBatteryButton");
            _addMedkitButton = GetNode<Button>("TestButtons/AddMedkitButton");
            _addKeyButton = GetNode<Button>("TestButtons/AddKeyButton");
            _clearInventoryButton = GetNode<Button>("TestButtons/ClearInventoryButton");
            _takeScreenshotButton = GetNode<Button>("TestButtons/TakeScreenshotButton");

            // Get references to game systems
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_inventorySystem == null || _gameState == null)
            {
                GD.PrintErr("PixelInventoryTest: Failed to get InventorySystem or GameState reference");
                return;
            }

            // Connect button signals
            _addFlashlightButton.Pressed += () => AddItem("flashlight");
            _addBatteryButton.Pressed += () => AddItem("battery");
            _addMedkitButton.Pressed += () => AddItem("medkit");
            _addKeyButton.Pressed += () => AddItem("key_cabin");
            _clearInventoryButton.Pressed += ClearInventory;
            _takeScreenshotButton.Pressed += TakeScreenshot;

            // Hide inventory UI initially
            _inventoryUI.SetVisible(false);

            GD.Print("PixelInventoryTest ready!");
        }

        // Process input events
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Toggle inventory with I key
                if (keyEvent.Keycode == Key.I)
                {
                    _inventoryUI.ToggleVisibility();
                }
            }
        }

        // Add an item to the inventory
        private void AddItem(string itemId)
        {
            if (_inventorySystem.AddItemToInventory(itemId))
            {
                GD.Print($"Added {itemId} to inventory");
            }
            else
            {
                GD.Print($"Failed to add {itemId} to inventory");
            }
        }

        // Clear the inventory
        private void ClearInventory()
        {
            // Get all items in the inventory
            var inventory = _inventorySystem.GetInventory();
            
            // Remove each item
            foreach (var item in new System.Collections.Generic.Dictionary<string, InventorySystem.ItemData>(inventory))
            {
                _inventorySystem.RemoveItemFromInventory(item.Key, item.Value.Quantity);
            }
            
            GD.Print("Cleared inventory");
        }

        // Take a screenshot
        private void TakeScreenshot()
        {
            var screenshotTaker = GetNode<ScreenshotTaker>("/root/ScreenshotTaker");
            
            if (screenshotTaker != null)
            {
                screenshotTaker.TakeScreenshot("pixel_inventory_test");
                GD.Print("Screenshot taken");
            }
            else
            {
                // Fallback if ScreenshotTaker is not available
                var image = GetViewport().GetTexture().GetImage();
                image.SavePng("user://pixel_inventory_test.png");
                GD.Print("Screenshot saved to user://pixel_inventory_test.png");
            }
        }
    }
}

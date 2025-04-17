using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelInventoryDemo : Control
    {
        // References to UI elements
        private PixelInventoryUI _inventoryUI;
        private PixelMessageDisplay _messageDisplay;
        private Button _takeScreenshotButton;
        private Dictionary<string, Button> _itemButtons = new Dictionary<string, Button>();

        // References to game systems
        private InventorySystem _inventorySystem;
        private GameState _gameState;

        // Item mapping
        private Dictionary<Button, string> _buttonToItemId = new Dictionary<Button, string>();

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _inventoryUI = GetNode<PixelInventoryUI>("PixelInventoryUI");
            _messageDisplay = GetNode<PixelMessageDisplay>("PixelMessageDisplay");
            _takeScreenshotButton = GetNode<Button>("TakeScreenshotButton");

            // Get references to game systems
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_inventorySystem == null || _gameState == null)
            {
                GD.PrintErr("PixelInventoryDemo: Failed to get InventorySystem or GameState reference");
                return;
            }

            // Set up item buttons
            SetupItemButtons();

            // Connect signals
            _takeScreenshotButton.Pressed += TakeScreenshot;
            _inventorySystem.ItemUsed += OnItemUsed;

            // Hide inventory UI initially
            _inventoryUI.SetVisible(false);

            // Show welcome message
            _messageDisplay.SetMessage(
                "welcome",
                "SIGNAL LOST",
                "Welcome to the inventory demo.\n\nClick on items to pick them up.\nPress I to open your inventory.\nUse items to interact with the world.",
                true,
                0.1f
            );

            GD.Print("PixelInventoryDemo ready!");
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

                // Close message with Escape key
                if (keyEvent.Keycode == Key.Escape)
                {
                    _messageDisplay.SetVisible(false);
                }
            }
        }

        // Set up item buttons
        private void SetupItemButtons()
        {
            // Map buttons to item IDs
            var gridContainer = GetNode<GridContainer>("GameWorld/GridContainer");
            
            // Item 1: Flashlight
            var item1Button = gridContainer.GetNode<Button>("Item1");
            _buttonToItemId[item1Button] = "flashlight";
            item1Button.Pressed += () => PickupItem(item1Button);
            
            // Item 2: Battery
            var item2Button = gridContainer.GetNode<Button>("Item2");
            _buttonToItemId[item2Button] = "battery";
            item2Button.Pressed += () => PickupItem(item2Button);
            
            // Item 3: Medkit
            var item3Button = gridContainer.GetNode<Button>("Item3");
            _buttonToItemId[item3Button] = "medkit";
            item3Button.Pressed += () => PickupItem(item3Button);
            
            // Item 4: Cabin Key
            var item4Button = gridContainer.GetNode<Button>("Item4");
            _buttonToItemId[item4Button] = "key_cabin";
            item4Button.Pressed += () => PickupItem(item4Button);
            
            // Item 5: Map Fragment
            var item5Button = gridContainer.GetNode<Button>("Item5");
            _buttonToItemId[item5Button] = "map_fragment";
            item5Button.Pressed += () => PickupItem(item5Button);
            
            // Item 6: Radio Part
            var item6Button = gridContainer.GetNode<Button>("Item6");
            _buttonToItemId[item6Button] = "radio_part";
            item6Button.Pressed += () => PickupItem(item6Button);
            
            // Item 7: Water Bottle
            var item7Button = gridContainer.GetNode<Button>("Item7");
            _buttonToItemId[item7Button] = "water_bottle";
            item7Button.Pressed += () => PickupItem(item7Button);
            
            // Item 8: Canned Food
            var item8Button = gridContainer.GetNode<Button>("Item8");
            _buttonToItemId[item8Button] = "canned_food";
            item8Button.Pressed += () => PickupItem(item8Button);
            
            // Item 9: Empty (no item)
            var item9Button = gridContainer.GetNode<Button>("Item9");
            item9Button.Pressed += () => ShowMessage("Nothing here", "There's nothing to pick up here.");
        }

        // Pick up an item
        private void PickupItem(Button button)
        {
            if (!_buttonToItemId.ContainsKey(button))
                return;
                
            string itemId = _buttonToItemId[button];
            var item = _inventorySystem.GetItem(itemId);
            
            if (item == null)
                return;
                
            // Try to add the item to the inventory
            if (_inventorySystem.AddItemToInventory(itemId))
            {
                // Show success message
                ShowMessage($"Picked up {item.Name}", $"You picked up a {item.Name}.\n\n{item.Description}");
                
                // Disable the button
                button.Disabled = true;
                button.Text = "Taken";
            }
            else
            {
                // Show failure message (inventory full)
                ShowMessage("Inventory Full", "Your inventory is full. You can't carry any more items.");
            }
        }

        // Show a message
        private void ShowMessage(string title, string content)
        {
            _messageDisplay.SetMessage(
                "message_" + DateTime.Now.Ticks,
                title.ToUpper(),
                content,
                true,
                0.05f
            );
        }

        // Handle item usage
        private void OnItemUsed(string itemId)
        {
            var item = _inventorySystem.GetItem(itemId);
            
            if (item == null)
                return;
                
            // Handle different items
            switch (itemId)
            {
                case "flashlight":
                    ShowMessage("Used Flashlight", "You turn on the flashlight, illuminating the dark corners of the room.");
                    break;
                case "battery":
                    ShowMessage("Used Battery", "You use the battery to power a device.");
                    break;
                case "medkit":
                    ShowMessage("Used Medkit", "You apply first aid to your wounds. You feel better now.");
                    break;
                case "key_cabin":
                    ShowMessage("Used Cabin Key", "You use the key to unlock a door. It opens with a creak.");
                    break;
                case "map_fragment":
                    ShowMessage("Examined Map Fragment", "You examine the map fragment. It shows part of the area, but it's incomplete.");
                    break;
                case "radio_part":
                    ShowMessage("Examined Radio Part", "You examine the radio part. It could be used to repair or upgrade a radio.");
                    break;
                case "water_bottle":
                    ShowMessage("Drank Water", "You drink the water. It's refreshing and quenches your thirst.");
                    break;
                case "canned_food":
                    ShowMessage("Ate Food", "You eat the canned food. It's not tasty, but it fills your stomach.");
                    break;
                default:
                    ShowMessage($"Used {item.Name}", $"You use the {item.Name}.");
                    break;
            }
        }

        // Take a screenshot
        private void TakeScreenshot()
        {
            var screenshotTaker = GetNode<ScreenshotTaker>("/root/ScreenshotTaker");
            
            if (screenshotTaker != null)
            {
                screenshotTaker.TakeScreenshot("pixel_inventory_demo");
                GD.Print("Screenshot taken");
            }
            else
            {
                // Fallback if ScreenshotTaker is not available
                var image = GetViewport().GetTexture().GetImage();
                image.SavePng("user://pixel_inventory_demo.png");
                GD.Print("Screenshot saved to user://pixel_inventory_demo.png");
            }
        }
    }
}

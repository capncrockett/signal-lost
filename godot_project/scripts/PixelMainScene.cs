using Godot;
using SignalLost.Utils;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelMainScene : Node
    {
        // References to UI components
        private PixelRadioInterface _radioInterface;
        private PixelInventoryUI _inventoryUI;
        private PixelMapInterface _mapInterface;
        private PixelQuestUI _questUI;
        private SaveLoadMenu _saveLoadMenu;
        private ProgressionUI _progressionUI;

        // References to UI control buttons
        private Button _radioButton;
        private Button _inventoryButton;
        private Button _mapButton;
        private Button _questButton;
        private Button _saveLoadButton;
        private Button _progressionButton;

        // References to game systems
        private GameState _gameState;
        private RadioSystem _radioSystem;
        private InventorySystem _inventorySystem;
        private MapSystem _mapSystem;
        private SaveManager _saveManager;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Register utility autoloads
            UtilsAutoload.RegisterAutoloads();

            // Get references to UI components
            _radioInterface = GetNode<PixelRadioInterface>("PixelRadioInterface");
            _inventoryUI = GetNode<PixelInventoryUI>("PixelInventoryUI");
            _mapInterface = GetNode<PixelMapInterface>("PixelMapInterface");
            _questUI = GetNode<PixelQuestUI>("PixelQuestUI");
            _saveLoadMenu = GetNode<SaveLoadMenu>("SaveLoadMenu");
            _progressionUI = GetNode<ProgressionUI>("ProgressionUI");

            // Get references to UI control buttons
            _radioButton = GetNode<Button>("UIControls/RadioButton");
            _inventoryButton = GetNode<Button>("UIControls/InventoryButton");
            _mapButton = GetNode<Button>("UIControls/MapButton");
            _questButton = GetNode<Button>("UIControls/QuestButton");
            _saveLoadButton = GetNode<Button>("UIControls/SaveLoadButton");
            _progressionButton = GetNode<Button>("UIControls/ProgressionButton");

            // Get references to game systems
            _gameState = GetNode<GameState>("/root/GameState");
            _radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");
            _saveManager = GetNode<SaveManager>("/root/SaveManager");

            // Connect button signals
            _radioButton.Pressed += () => ShowInterface("radio");
            _inventoryButton.Pressed += () => ShowInterface("inventory");
            _mapButton.Pressed += () => ShowInterface("map");
            _questButton.Pressed += () => ShowInterface("quest");
            _saveLoadButton.Pressed += ToggleSaveLoadMenu;
            _progressionButton.Pressed += ToggleProgressionUI;

            // Set up keyboard shortcuts
            SetProcessInput(true);

            // Show radio interface by default
            ShowInterface("radio");
        }

        // Process input events
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.R)
                {
                    ShowInterface("radio");
                }
                else if (keyEvent.Keycode == Key.I)
                {
                    ShowInterface("inventory");
                }
                else if (keyEvent.Keycode == Key.M)
                {
                    ShowInterface("map");
                }
                else if (keyEvent.Keycode == Key.Q)
                {
                    ShowInterface("quest");
                }
                else if (keyEvent.Keycode == Key.S)
                {
                    ToggleSaveLoadMenu();
                }
                else if (keyEvent.Keycode == Key.P)
                {
                    ToggleProgressionUI();
                }
                else if (keyEvent.Keycode == Key.Escape)
                {
                    // If save/load menu is visible, hide it
                    if (_saveLoadMenu.Visible)
                    {
                        _saveLoadMenu.Visible = false;
                        return;
                    }

                    // If progression UI is visible, hide it
                    if (_progressionUI.Visible)
                    {
                        _progressionUI.Visible = false;
                        return;
                    }

                    // If any interface is visible, hide it
                    if (_inventoryUI.IsVisible() || _mapInterface.IsVisible() || _questUI.IsVisible())
                    {
                        ShowInterface("radio");
                    }
                }
            }
        }

        // Show the specified interface and hide others
        private void ShowInterface(string interfaceName)
        {
            // Hide all interfaces
            _radioInterface.SetVisible(false);
            _inventoryUI.SetVisible(false);
            _mapInterface.SetVisible(false);
            _questUI.SetVisible(false);
            _saveLoadMenu.Visible = false;
            _progressionUI.Visible = false;

            // Show the specified interface
            switch (interfaceName.ToLower())
            {
                case "radio":
                    _radioInterface.SetVisible(true);
                    break;
                case "inventory":
                    _inventoryUI.SetVisible(true);
                    break;
                case "map":
                    _mapInterface.SetVisible(true);
                    break;
                case "quest":
                    _questUI.SetVisible(true);
                    break;
            }

            // Update button states
            _radioButton.Disabled = string.Equals(interfaceName, "radio", StringComparison.OrdinalIgnoreCase);
            _inventoryButton.Disabled = string.Equals(interfaceName, "inventory", StringComparison.OrdinalIgnoreCase);
            _mapButton.Disabled = string.Equals(interfaceName, "map", StringComparison.OrdinalIgnoreCase);
            _questButton.Disabled = string.Equals(interfaceName, "quest", StringComparison.OrdinalIgnoreCase);
            _saveLoadButton.Disabled = false; // Save/load button is always enabled
            _progressionButton.Disabled = false; // Progression button is always enabled
        }

        // Toggle the save/load menu
        private void ToggleSaveLoadMenu()
        {
            // Toggle visibility
            _saveLoadMenu.Visible = !_saveLoadMenu.Visible;

            // If showing the menu, refresh the save slot list
            if (_saveLoadMenu.Visible)
            {
                _saveLoadMenu.RefreshSaveSlotList();
            }
        }

        // Toggle the progression UI
        private void ToggleProgressionUI()
        {
            // Toggle visibility
            _progressionUI.Visible = !_progressionUI.Visible;

            // If showing the UI, update it
            if (_progressionUI.Visible)
            {
                // The ProgressionUI will update itself in _Ready
            }
        }
    }
}

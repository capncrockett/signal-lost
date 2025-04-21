using Godot;
using System;
using SignalLost.UI;
using SignalLost.Audio;

namespace SignalLost
{
    /// <summary>
    /// Main controller for the game scene, handling UI navigation and system integration.
    /// </summary>
    [GlobalClass]
    public partial class MainGameController : Node
    {
        // References to UI components
        private PixelRadioInterface _radioInterface;
        private PixelInventoryUI _inventoryUI;
        private PixelMapInterface _mapInterface;
        private PixelQuestUI _questUI;
        private SaveLoadMenu _saveLoadMenu;
        private ProgressionUI _progressionUI;
        
        // References to managers
        private RadioInterfaceManager _radioInterfaceManager;
        private RadioAudioManager _radioAudioManager;
        
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
            GD.Print("MainGameController: Initializing...");
            
            try
            {
                // Get references to UI components
                _radioInterface = GetNode<PixelRadioInterface>("PixelRadioInterface");
                _inventoryUI = GetNode<PixelInventoryUI>("PixelInventoryUI");
                _mapInterface = GetNode<PixelMapInterface>("PixelMapInterface");
                _questUI = GetNode<PixelQuestUI>("PixelQuestUI");
                _saveLoadMenu = GetNode<SaveLoadMenu>("SaveLoadMenu");
                _progressionUI = GetNode<ProgressionUI>("ProgressionUI");
                
                // Get references to managers
                _radioInterfaceManager = GetNode<RadioInterfaceManager>("RadioInterfaceManager");
                _radioAudioManager = GetNode<RadioAudioManager>("RadioAudioManager");
                
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
                
                GD.Print("MainGameController: Initialization complete");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"MainGameController: Error during initialization: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
            }
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
            }
        }
        
        // Show the specified interface
        private void ShowInterface(string interfaceType)
        {
            // Hide all interfaces
            _radioInterface.Visible = false;
            _inventoryUI.Visible = false;
            _mapInterface.Visible = false;
            _questUI.Visible = false;
            _saveLoadMenu.Visible = false;
            _progressionUI.Visible = false;
            
            // Show the specified interface
            switch (interfaceType.ToLower())
            {
                case "radio":
                    _radioInterface.Visible = true;
                    break;
                case "inventory":
                    _inventoryUI.Visible = true;
                    break;
                case "map":
                    _mapInterface.Visible = true;
                    break;
                case "quest":
                    _questUI.Visible = true;
                    break;
                default:
                    // Default to radio interface
                    _radioInterface.Visible = true;
                    break;
            }
        }
        
        // Toggle the save/load menu
        private void ToggleSaveLoadMenu()
        {
            _saveLoadMenu.Visible = !_saveLoadMenu.Visible;
        }
        
        // Toggle the progression UI
        private void ToggleProgressionUI()
        {
            _progressionUI.Visible = !_progressionUI.Visible;
        }
    }
}

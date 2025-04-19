using Godot;
using SignalLost.Utils;

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

        // References to UI control buttons
        private Button _radioButton;
        private Button _inventoryButton;
        private Button _mapButton;
        private Button _questButton;

        // References to game systems
        private GameState _gameState;
        private RadioSystem _radioSystem;
        private InventorySystem _inventorySystem;
        private MapSystem _mapSystem;

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

            // Get references to UI control buttons
            _radioButton = GetNode<Button>("UIControls/RadioButton");
            _inventoryButton = GetNode<Button>("UIControls/InventoryButton");
            _mapButton = GetNode<Button>("UIControls/MapButton");
            _questButton = GetNode<Button>("UIControls/QuestButton");

            // Get references to game systems
            _gameState = GetNode<GameState>("/root/GameState");
            _radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");

            // Connect button signals
            _radioButton.Pressed += () => ShowInterface("radio");
            _inventoryButton.Pressed += () => ShowInterface("inventory");
            _mapButton.Pressed += () => ShowInterface("map");
            _questButton.Pressed += () => ShowInterface("quest");

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
                else if (keyEvent.Keycode == Key.Escape)
                {
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
            _radioButton.Disabled = interfaceName.ToLower() == "radio";
            _inventoryButton.Disabled = interfaceName.ToLower() == "inventory";
            _mapButton.Disabled = interfaceName.ToLower() == "map";
            _questButton.Disabled = interfaceName.ToLower() == "quest";
        }
    }
}

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
        private Button _backButton;
        private Button _exitButton;

        // UI transitions
        private ColorRect _transitionOverlay;
        private Timer _transitionTimer;

        // Confirmation dialog
        private ConfirmationDialog _confirmationDialog;

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

                // Create back button
                _backButton = new Button();
                _backButton.Text = "Back to Menu";
                _backButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
                _backButton.SizeFlagsVertical = Control.SizeFlags.ShrinkEnd;
                _backButton.Position = new Vector2(10, 10);
                _backButton.Pressed += OnBackButtonPressed;
                AddChild(_backButton);

                // Create exit button
                _exitButton = new Button();
                _exitButton.Text = "Exit Game";
                _exitButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
                _exitButton.SizeFlagsVertical = Control.SizeFlags.ShrinkEnd;
                _exitButton.Position = new Vector2(10, 50);
                _exitButton.Pressed += OnExitButtonPressed;
                AddChild(_exitButton);

                // Create transition overlay
                _transitionOverlay = new ColorRect();
                _transitionOverlay.Color = new Color(0, 0, 0, 0); // Start transparent
                _transitionOverlay.Size = GetViewport().GetVisibleRect().Size;
                _transitionOverlay.ZIndex = 100; // Ensure it's on top
                _transitionOverlay.Visible = false;
                AddChild(_transitionOverlay);

                // Create transition timer
                _transitionTimer = new Timer();
                _transitionTimer.OneShot = true;
                _transitionTimer.WaitTime = 0.3f; // Transition duration
                AddChild(_transitionTimer);

                // Create confirmation dialog
                _confirmationDialog = new ConfirmationDialog();
                _confirmationDialog.Title = "Confirmation";
                _confirmationDialog.DialogText = "Are you sure?";
                _confirmationDialog.MinSize = new Vector2(200, 100);
                _confirmationDialog.GetOkButton().Text = "Yes";
                _confirmationDialog.GetCancelButton().Text = "No";
                AddChild(_confirmationDialog);

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

        // Show the specified interface with transition
        private void ShowInterface(string interfaceType)
        {
            // Start fade out transition
            _transitionOverlay.Visible = true;
            var tween = CreateTween();
            tween.TweenProperty(_transitionOverlay, "color", new Color(0, 0, 0, 0.5f), 0.2f);
            tween.TweenCallback(Callable.From(() => {
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

                // Start fade in transition
                var fadeInTween = CreateTween();
                fadeInTween.TweenProperty(_transitionOverlay, "color", new Color(0, 0, 0, 0), 0.2f);
                fadeInTween.TweenCallback(Callable.From(() => {
                    _transitionOverlay.Visible = false;
                }));
            }));
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

        // Handle back button press
        private void OnBackButtonPressed()
        {
            _confirmationDialog.DialogText = "Return to main menu? Any unsaved progress will be lost.";
            _confirmationDialog.Confirmed += OnBackConfirmed;
            _confirmationDialog.Canceled += OnDialogCanceled;
            _confirmationDialog.PopupCentered();
        }

        // Handle exit button press
        private void OnExitButtonPressed()
        {
            _confirmationDialog.DialogText = "Exit the game? Any unsaved progress will be lost.";
            _confirmationDialog.Confirmed += OnExitConfirmed;
            _confirmationDialog.Canceled += OnDialogCanceled;
            _confirmationDialog.PopupCentered();
        }

        // Handle back confirmation
        private void OnBackConfirmed()
        {
            // Disconnect the signal to prevent multiple connections
            _confirmationDialog.Confirmed -= OnBackConfirmed;

            // Start transition to main menu
            _transitionOverlay.Visible = true;
            var tween = CreateTween();
            tween.TweenProperty(_transitionOverlay, "color", new Color(0, 0, 0, 1.0f), 0.5f);
            tween.TweenCallback(Callable.From(() => {
                // Change to main menu scene
                GetTree().ChangeSceneToFile("res://scenes/gameplay/MainMenu.tscn");
            }));
        }

        // Handle exit confirmation
        private void OnExitConfirmed()
        {
            // Disconnect the signal to prevent multiple connections
            _confirmationDialog.Confirmed -= OnExitConfirmed;

            // Start transition to exit
            _transitionOverlay.Visible = true;
            var tween = CreateTween();
            tween.TweenProperty(_transitionOverlay, "color", new Color(0, 0, 0, 1.0f), 0.5f);
            tween.TweenCallback(Callable.From(() => {
                // Exit the game
                GetTree().Quit();
            }));
        }

        // Handle dialog canceled
        private void OnDialogCanceled()
        {
            // Disconnect all signals to prevent multiple connections
            _confirmationDialog.Confirmed -= OnBackConfirmed;
            _confirmationDialog.Confirmed -= OnExitConfirmed;
            _confirmationDialog.Canceled -= OnDialogCanceled;
        }
    }
}

using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    /// <summary>
    /// A simple menu for saving and loading games.
    /// </summary>
    [GlobalClass]
    public partial class SaveLoadMenu : Control
    {
        // References
        private SaveManager _saveManager;
        private GameState _gameState;

        // UI elements
        private Button _saveButton;
        private Button _loadButton;
        private Button _closeButton;
        private ItemList _saveSlotList;
        private Label _statusLabel;
        private ConfirmationDialog _confirmationDialog;
        private ColorRect _feedbackOverlay;

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references
            _saveManager = GetNode<SaveManager>("/root/SaveManager");
            _gameState = GetNode<GameState>("/root/GameState");

            // Get UI elements
            _saveButton = GetNode<Button>("VBoxContainer/ButtonContainer/SaveButton");
            _loadButton = GetNode<Button>("VBoxContainer/ButtonContainer/LoadButton");
            _closeButton = GetNode<Button>("VBoxContainer/ButtonContainer/CloseButton");
            _saveSlotList = GetNode<ItemList>("VBoxContainer/SaveSlotList");
            _statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");

            // Create confirmation dialog
            _confirmationDialog = new ConfirmationDialog();
            _confirmationDialog.Title = "Confirmation";
            _confirmationDialog.DialogText = "Are you sure?";
            _confirmationDialog.MinSize = new Vector2I(200, 100);
            _confirmationDialog.GetOkButton().Text = "Yes";
            _confirmationDialog.GetCancelButton().Text = "No";
            AddChild(_confirmationDialog);

            // Create feedback overlay
            _feedbackOverlay = new ColorRect();
            _feedbackOverlay.Color = new Color(0, 0.7f, 0, 0.3f); // Green for success
            _feedbackOverlay.Size = Size;
            _feedbackOverlay.ZIndex = 100;
            _feedbackOverlay.Visible = false;
            AddChild(_feedbackOverlay);

            // Connect signals
            _saveButton.Pressed += OnSaveButtonPressed;
            _loadButton.Pressed += OnLoadButtonPressed;
            _closeButton.Pressed += OnCloseButtonPressed;
            _saveSlotList.ItemSelected += OnSaveSlotSelected;

            if (_saveManager != null)
            {
                _saveManager.SaveCompleted += OnSaveCompleted;
                _saveManager.LoadCompleted += OnLoadCompleted;
            }

            // Refresh save slot list
            RefreshSaveSlotList();

            // Hide status label initially
            _statusLabel.Text = "";
        }

        /// <summary>
        /// Called when the save button is pressed.
        /// </summary>
        private void OnSaveButtonPressed()
        {
            if (_saveManager != null)
            {
                // Get selected slot or use autosave
                string slotName = "autosave";
                if (_saveSlotList.GetSelectedItems().Length > 0)
                {
                    slotName = _saveSlotList.GetItemText(_saveSlotList.GetSelectedItems()[0]);
                }

                // Show confirmation dialog
                _confirmationDialog.DialogText = $"Save game to slot '{slotName}'?";
                // Store the action in a variable so we can disconnect it later
                Action saveAction = null;
                saveAction = () => {
                    // Save game
                    bool success = _saveManager.SaveGame(slotName);
                    if (success)
                    {
                        ShowFeedback(true, $"Game saved to slot '{slotName}'");
                    }
                    else
                    {
                        ShowFeedback(false, $"Failed to save game to slot '{slotName}'");
                    }

                    // Refresh save slot list
                    RefreshSaveSlotList();

                    // Disconnect the signal to prevent multiple connections
                    _confirmationDialog.Confirmed -= saveAction;
                };
                _confirmationDialog.Confirmed += saveAction;
                _confirmationDialog.PopupCentered();
            }
        }

        /// <summary>
        /// Called when the load button is pressed.
        /// </summary>
        private void OnLoadButtonPressed()
        {
            if (_saveManager != null && _saveSlotList.GetSelectedItems().Length > 0)
            {
                string slotName = _saveSlotList.GetItemText(_saveSlotList.GetSelectedItems()[0]);

                // Show confirmation dialog
                _confirmationDialog.DialogText = $"Load game from slot '{slotName}'? Any unsaved progress will be lost.";
                // Store the action in a variable so we can disconnect it later
                Action loadAction = null;
                loadAction = () => {
                    bool success = _saveManager.LoadGame(slotName);
                    if (success)
                    {
                        ShowFeedback(true, $"Game loaded from slot '{slotName}'");
                    }
                    else
                    {
                        ShowFeedback(false, $"Failed to load game from slot '{slotName}'");
                    }

                    // Disconnect the signal to prevent multiple connections
                    _confirmationDialog.Confirmed -= loadAction;
                };
                _confirmationDialog.Confirmed += loadAction;
                _confirmationDialog.PopupCentered();
            }
            else
            {
                ShowFeedback(false, "No save slot selected");
            }
        }

        /// <summary>
        /// Called when the close button is pressed.
        /// </summary>
        private void OnCloseButtonPressed()
        {
            // Hide the menu
            Visible = false;
        }

        /// <summary>
        /// Called when a save slot is selected.
        /// </summary>
        /// <param name="index">The index of the selected item</param>
        private void OnSaveSlotSelected(long index)
        {
            // Enable load button if a slot is selected
            _loadButton.Disabled = false;
        }

        /// <summary>
        /// Called when a save operation is completed.
        /// </summary>
        /// <param name="success">Whether the save was successful</param>
        /// <param name="saveName">The name of the save slot</param>
        private void OnSaveCompleted(bool success, string saveName)
        {
            if (success)
            {
                ShowFeedback(true, $"Game saved to slot '{saveName}'");
            }
            else
            {
                ShowFeedback(false, $"Failed to save game to slot '{saveName}'");
            }

            // Refresh save slot list
            RefreshSaveSlotList();
        }

        /// <summary>
        /// Called when a load operation is completed.
        /// </summary>
        /// <param name="success">Whether the load was successful</param>
        /// <param name="saveName">The name of the save slot</param>
        private void OnLoadCompleted(bool success, string saveName)
        {
            if (success)
            {
                ShowFeedback(true, $"Game loaded from slot '{saveName}'");

                // Hide the menu after loading with a delay
                GetTree().CreateTimer(1.0f).Timeout += () => {
                    Visible = false;
                };
            }
            else
            {
                ShowFeedback(false, $"Failed to load game from slot '{saveName}'");
            }
        }

        /// <summary>
        /// Refreshes the save slot list.
        /// </summary>
        public void RefreshSaveSlotList()
        {
            if (_saveManager != null)
            {
                // Clear the list
                _saveSlotList.Clear();

                // Get save slots
                List<string> saveSlots = _saveManager.GetSaveSlots();

                // Add slots to the list
                foreach (string slot in saveSlots)
                {
                    _saveSlotList.AddItem(slot);
                }

                // Add "New Save" option
                _saveSlotList.AddItem("New Save");

                // Disable load button if no slots are available
                _loadButton.Disabled = saveSlots.Count == 0;
            }
        }

        /// <summary>
        /// Shows the menu.
        /// </summary>
        public new void Show()
        {
            // Refresh save slot list
            RefreshSaveSlotList();

            // Show the menu
            Visible = true;
        }

        /// <summary>
        /// Shows feedback to the user.
        /// </summary>
        /// <param name="success">Whether the operation was successful</param>
        /// <param name="message">The message to display</param>
        private void ShowFeedback(bool success, string message)
        {
            // Update status label
            _statusLabel.Text = message;

            // Show feedback overlay
            _feedbackOverlay.Color = success ? new Color(0, 0.7f, 0, 0.3f) : new Color(0.7f, 0, 0, 0.3f);
            _feedbackOverlay.Visible = true;

            // Create tween for fade out
            var tween = CreateTween();
            tween.TweenProperty(_feedbackOverlay, "color:a", 0.0f, 1.0f);
            tween.TweenCallback(Callable.From(() => {
                _feedbackOverlay.Visible = false;
            }));
        }
    }
}

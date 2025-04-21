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

                // Save game
                bool success = _saveManager.SaveGame(slotName);
                if (success)
                {
                    _statusLabel.Text = $"Game saved to slot '{slotName}'";
                }
                else
                {
                    _statusLabel.Text = $"Failed to save game to slot '{slotName}'";
                }

                // Refresh save slot list
                RefreshSaveSlotList();
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
                bool success = _saveManager.LoadGame(slotName);
                if (success)
                {
                    _statusLabel.Text = $"Game loaded from slot '{slotName}'";
                }
                else
                {
                    _statusLabel.Text = $"Failed to load game from slot '{slotName}'";
                }
            }
            else
            {
                _statusLabel.Text = "No save slot selected";
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
                _statusLabel.Text = $"Game saved to slot '{saveName}'";
            }
            else
            {
                _statusLabel.Text = $"Failed to save game to slot '{saveName}'";
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
                _statusLabel.Text = $"Game loaded from slot '{saveName}'";

                // Hide the menu after loading
                Visible = false;
            }
            else
            {
                _statusLabel.Text = $"Failed to load game from slot '{saveName}'";
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
    }
}

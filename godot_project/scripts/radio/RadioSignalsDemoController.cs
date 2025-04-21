using Godot;
using System.Collections.Generic;

namespace SignalLost.Radio
{
    /// <summary>
    /// Controller for the radio signals demo scene.
    /// </summary>
    [GlobalClass]
    public partial class RadioSignalsDemoController : Control
    {
        // References to UI elements
        private Button _radioToggleButton;
        private Label _frequencyLabel;
        private HSlider _frequencySlider;
        private Button _addRadioButton;
        private Button _addEnhancedRadioButton;
        private Button _addCrystalButton;
        private ItemList _discoveredSignalsList;
        private Button _resetButton;
        private Button _closeButton;
        private Label _statusLabel;
        
        // Reference to the radio signals manager
        private RadioSignalsManager _radioSignalsManager;
        
        // Reference to the game state
        private GameState _gameState;
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _radioToggleButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ControlPanel/RadioControls/RadioToggleButton");
            _frequencyLabel = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/ControlPanel/RadioControls/FrequencyLabel");
            _frequencySlider = GetNode<HSlider>("MarginContainer/VBoxContainer/HBoxContainer/ControlPanel/RadioControls/FrequencySlider");
            _addRadioButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ControlPanel/EquipmentButtons/AddRadioButton");
            _addEnhancedRadioButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ControlPanel/EquipmentButtons/AddEnhancedRadioButton");
            _addCrystalButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ControlPanel/EquipmentButtons/AddCrystalButton");
            _discoveredSignalsList = GetNode<ItemList>("MarginContainer/VBoxContainer/HBoxContainer/ControlPanel/DiscoveredSignalsList");
            _resetButton = GetNode<Button>("MarginContainer/VBoxContainer/FooterButtons/ResetButton");
            _closeButton = GetNode<Button>("MarginContainer/VBoxContainer/FooterButtons/CloseButton");
            _statusLabel = GetNode<Label>("MarginContainer/VBoxContainer/StatusLabel");
            
            // Get references to systems
            _radioSignalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");
            _gameState = GetNode<GameState>("/root/GameState");
            
            if (_radioSignalsManager == null)
            {
                GD.PrintErr("RadioSignalsDemoController: RadioSignalsManager not found");
                return;
            }
            
            if (_gameState == null)
            {
                GD.PrintErr("RadioSignalsDemoController: GameState not found");
                return;
            }
            
            // Connect signals
            _radioToggleButton.Pressed += OnRadioToggleButtonPressed;
            _frequencySlider.ValueChanged += OnFrequencySliderValueChanged;
            _addRadioButton.Pressed += OnAddRadioButtonPressed;
            _addEnhancedRadioButton.Pressed += OnAddEnhancedRadioButtonPressed;
            _addCrystalButton.Pressed += OnAddCrystalButtonPressed;
            _discoveredSignalsList.ItemSelected += OnDiscoveredSignalsListItemSelected;
            _resetButton.Pressed += OnResetButtonPressed;
            _closeButton.Pressed += OnCloseButtonPressed;
            
            _radioSignalsManager.SignalDiscovered += OnSignalDiscovered;
            _radioSignalsManager.SignalLost += OnSignalLost;
            _gameState.RadioToggled += OnRadioToggled;
            _gameState.InventoryChanged += OnInventoryChanged;
            
            // Initialize
            UpdateFrequencyLabel();
            UpdateRadioToggleButton();
            UpdateEquipmentButtons();
            UpdateDiscoveredSignalsList();
        }
        
        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_radioSignalsManager != null)
            {
                _radioSignalsManager.SignalDiscovered -= OnSignalDiscovered;
                _radioSignalsManager.SignalLost -= OnSignalLost;
            }
            
            if (_gameState != null)
            {
                _gameState.RadioToggled -= OnRadioToggled;
                _gameState.InventoryChanged -= OnInventoryChanged;
            }
        }
        
        // Handle radio toggle button pressed
        private void OnRadioToggleButtonPressed()
        {
            // Toggle radio
            _gameState.ToggleRadio();
        }
        
        // Handle frequency slider value changed
        private void OnFrequencySliderValueChanged(double value)
        {
            // Update frequency label
            UpdateFrequencyLabel();
            
            // Update frequency in GameState
            _gameState.SetFrequency((float)value);
        }
        
        // Handle add radio button pressed
        private void OnAddRadioButtonPressed()
        {
            // Add radio to inventory if not already present
            if (!_gameState.Inventory.Contains("radio"))
            {
                _gameState.AddToInventory("radio");
                UpdateStatusLabel("Radio added to inventory");
            }
            else
            {
                // Remove radio from inventory
                _gameState.RemoveFromInventory("radio");
                UpdateStatusLabel("Radio removed from inventory");
            }
        }
        
        // Handle add enhanced radio button pressed
        private void OnAddEnhancedRadioButtonPressed()
        {
            // Add enhanced radio to inventory if not already present
            if (!_gameState.Inventory.Contains("radio_enhanced"))
            {
                _gameState.AddToInventory("radio_enhanced");
                UpdateStatusLabel("Enhanced Radio added to inventory");
            }
            else
            {
                // Remove enhanced radio from inventory
                _gameState.RemoveFromInventory("radio_enhanced");
                UpdateStatusLabel("Enhanced Radio removed from inventory");
            }
        }
        
        // Handle add crystal button pressed
        private void OnAddCrystalButtonPressed()
        {
            // Add crystal to inventory if not already present
            if (!_gameState.Inventory.Contains("strange_crystal"))
            {
                _gameState.AddToInventory("strange_crystal");
                UpdateStatusLabel("Strange Crystal added to inventory");
            }
            else
            {
                // Remove crystal from inventory
                _gameState.RemoveFromInventory("strange_crystal");
                UpdateStatusLabel("Strange Crystal removed from inventory");
            }
        }
        
        // Handle discovered signals list item selected
        private void OnDiscoveredSignalsListItemSelected(long index)
        {
            // Get the signal ID
            string signalId = (string)_discoveredSignalsList.GetItemMetadata((int)index);
            
            // Get the signal
            var signal = _radioSignalsManager.GetSignal(signalId);
            
            if (signal != null)
            {
                // Set the frequency to the signal's frequency
                _frequencySlider.Value = signal.Frequency;
                UpdateFrequencyLabel();
                _gameState.SetFrequency(signal.Frequency);
                
                // Make sure radio is on
                if (!_gameState.IsRadioOn)
                {
                    _gameState.ToggleRadio();
                    UpdateRadioToggleButton();
                }
                
                // Update status label
                UpdateStatusLabel($"Tuned to {signal.Name} at {signal.Frequency:F1} MHz");
            }
        }
        
        // Handle reset button pressed
        private void OnResetButtonPressed()
        {
            // Reset the demo
            ResetDemo();
        }
        
        // Handle close button pressed
        private void OnCloseButtonPressed()
        {
            // Close the demo
            Hide();
        }
        
        // Handle signal discovered
        private void OnSignalDiscovered(string signalId)
        {
            // Update discovered signals list
            UpdateDiscoveredSignalsList();
            
            // Update status label
            var signal = _radioSignalsManager.GetSignal(signalId);
            if (signal != null)
            {
                UpdateStatusLabel($"Signal discovered: {signal.Name}");
            }
        }
        
        // Handle signal lost
        private void OnSignalLost(string signalId)
        {
            // Update status label
            var signal = _radioSignalsManager.GetSignal(signalId);
            if (signal != null)
            {
                UpdateStatusLabel($"Signal lost: {signal.Name}");
            }
        }
        
        // Handle radio toggled
        private void OnRadioToggled(bool isOn)
        {
            // Update radio toggle button
            UpdateRadioToggleButton();
            
            // Update status label
            UpdateStatusLabel(isOn ? "Radio turned ON" : "Radio turned OFF");
        }
        
        // Handle inventory changed
        private void OnInventoryChanged()
        {
            // Update equipment buttons
            UpdateEquipmentButtons();
        }
        
        // Update the frequency label
        private void UpdateFrequencyLabel()
        {
            float frequency = (float)_frequencySlider.Value;
            _frequencyLabel.Text = $"Frequency: {frequency:F1} MHz";
        }
        
        // Update the radio toggle button
        private void UpdateRadioToggleButton()
        {
            _radioToggleButton.Text = _gameState.IsRadioOn ? "Turn Radio OFF" : "Turn Radio ON";
        }
        
        // Update equipment buttons
        private void UpdateEquipmentButtons()
        {
            // Update radio button
            _addRadioButton.Text = _gameState.Inventory.Contains("radio") ? "Remove Radio" : "Add Radio";
            
            // Update enhanced radio button
            _addEnhancedRadioButton.Text = _gameState.Inventory.Contains("radio_enhanced") ? "Remove Enhanced Radio" : "Add Enhanced Radio";
            
            // Update crystal button
            _addCrystalButton.Text = _gameState.Inventory.Contains("strange_crystal") ? "Remove Strange Crystal" : "Add Strange Crystal";
        }
        
        // Update the discovered signals list
        private void UpdateDiscoveredSignalsList()
        {
            // Clear the list
            _discoveredSignalsList.Clear();
            
            // Add discovered signals
            var discoveredSignals = _radioSignalsManager.GetDiscoveredSignals();
            foreach (var signal in discoveredSignals)
            {
                int index = _discoveredSignalsList.AddItem($"{signal.Name} ({signal.Frequency:F1} MHz)");
                _discoveredSignalsList.SetItemMetadata(index, signal.Id);
            }
        }
        
        // Update the status label
        private void UpdateStatusLabel(string text)
        {
            _statusLabel.Text = text;
        }
        
        // Reset the demo
        private void ResetDemo()
        {
            // Turn off radio
            if (_gameState.IsRadioOn)
            {
                _gameState.ToggleRadio();
            }
            
            // Reset frequency
            _frequencySlider.Value = 100.0;
            UpdateFrequencyLabel();
            _gameState.SetFrequency(100.0f);
            
            // Clear inventory
            List<string> itemsToRemove = new List<string>();
            foreach (var item in _gameState.Inventory)
            {
                if (item == "radio" || item == "radio_enhanced" || item == "strange_crystal")
                {
                    itemsToRemove.Add(item);
                }
            }
            
            foreach (var item in itemsToRemove)
            {
                _gameState.RemoveFromInventory(item);
            }
            
            // Update UI
            UpdateRadioToggleButton();
            UpdateEquipmentButtons();
            UpdateDiscoveredSignalsList();
            UpdateStatusLabel("Demo reset");
        }
    }
}

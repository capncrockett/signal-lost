using Godot;
using System;
using System.Collections.Generic;
using SignalLost.Utils;

namespace SignalLost
{
    /// <summary>
    /// A sandbox for visualizing and testing pixel-based rendering.
    /// </summary>
    public partial class PixelVisualizationSandbox : Control
    {
        // References to UI elements
        private PixelMessageDisplay _messageDisplay;
        private ScreenshotTaker _screenshotTaker;
        private Label _debugInfoLabel;
        private Button _takeScreenshotButton;
        private Button _toggleGridButton;
        private Button _toggleAsciiArtButton;
        private HSlider _interferenceSlider;
        private OptionButton _messageTypeDropdown;
        private TextEdit _contentInput;
        private TextEdit _asciiArtInput;
        private CheckBox _enableScanlinesCheckbox;
        private CheckBox _enableFlickerCheckbox;
        private CheckBox _enableTypewriterSoundCheckbox;
        private SpinBox _characterSizeSpinBox;
        private ColorPickerButton _textColorPicker;
        private ColorPickerButton _bgColorPicker;

        // State
        private bool _showGrid = false;
        private bool _showAsciiArt = false;
        private List<string> _asciiArt = new List<string>();

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _messageDisplay = GetNode<PixelMessageDisplay>("PixelMessageDisplay");
            _screenshotTaker = GetNode<ScreenshotTaker>("ScreenshotTaker");
            _debugInfoLabel = GetNode<Label>("ControlPanel/DebugInfo");
            _takeScreenshotButton = GetNode<Button>("ControlPanel/ScreenshotButton");
            _toggleGridButton = GetNode<Button>("ControlPanel/ToggleGridButton");
            _toggleAsciiArtButton = GetNode<Button>("ControlPanel/ToggleAsciiArtButton");
            _interferenceSlider = GetNode<HSlider>("ControlPanel/InterferenceSlider");
            _messageTypeDropdown = GetNode<OptionButton>("ControlPanel/MessageTypeDropdown");
            _contentInput = GetNode<TextEdit>("ControlPanel/ContentInput");
            _asciiArtInput = GetNode<TextEdit>("ControlPanel/AsciiArtInput");
            _enableScanlinesCheckbox = GetNode<CheckBox>("ControlPanel/EnableScanlinesCheckbox");
            _enableFlickerCheckbox = GetNode<CheckBox>("ControlPanel/EnableFlickerCheckbox");
            _enableTypewriterSoundCheckbox = GetNode<CheckBox>("ControlPanel/EnableTypewriterSoundCheckbox");
            _characterSizeSpinBox = GetNode<SpinBox>("ControlPanel/CharacterSizeSpinBox");
            _textColorPicker = GetNode<ColorPickerButton>("ControlPanel/TextColorPicker");
            _bgColorPicker = GetNode<ColorPickerButton>("ControlPanel/BgColorPicker");

            // Connect signals
            _takeScreenshotButton.Pressed += OnTakeScreenshotPressed;
            _toggleGridButton.Pressed += OnToggleGridPressed;
            _toggleAsciiArtButton.Pressed += OnToggleAsciiArtPressed;
            _interferenceSlider.ValueChanged += OnInterferenceChanged;
            _messageTypeDropdown.ItemSelected += OnMessageTypeSelected;
            _contentInput.TextChanged += OnContentChanged;
            _asciiArtInput.TextChanged += OnAsciiArtChanged;
            _enableScanlinesCheckbox.Toggled += OnScanlinesToggled;
            _enableFlickerCheckbox.Toggled += OnFlickerToggled;
            _enableTypewriterSoundCheckbox.Toggled += OnTypewriterSoundToggled;
            _characterSizeSpinBox.ValueChanged += OnCharacterSizeChanged;
            _textColorPicker.ColorChanged += OnTextColorChanged;
            _bgColorPicker.ColorChanged += OnBgColorChanged;

            // Initialize UI state
            InitializeUI();

            // Set initial message
            UpdateMessage();
        }

        // Initialize UI elements with default values
        private void InitializeUI()
        {
            // Set up message type dropdown
            _messageTypeDropdown.Clear();
            _messageTypeDropdown.AddItem("Terminal");
            _messageTypeDropdown.AddItem("Radio");
            _messageTypeDropdown.AddItem("Note");
            _messageTypeDropdown.AddItem("Computer");
            _messageTypeDropdown.Selected = 0;

            // Set up interference slider
            _interferenceSlider.MinValue = 0.0f;
            _interferenceSlider.MaxValue = 1.0f;
            _interferenceSlider.Step = 0.01f;
            _interferenceSlider.Value = 0.2f;

            // Set up checkboxes
            _enableScanlinesCheckbox.ButtonPressed = _messageDisplay.EnableScanlines;
            _enableFlickerCheckbox.ButtonPressed = _messageDisplay.EnableScreenFlicker;
            _enableTypewriterSoundCheckbox.ButtonPressed = _messageDisplay.EnableTypewriterSound;

            // Set up character size
            _characterSizeSpinBox.MinValue = 4;
            _characterSizeSpinBox.MaxValue = 16;
            _characterSizeSpinBox.Step = 1;
            _characterSizeSpinBox.Value = _messageDisplay.CharacterSize;

            // Set up color pickers
            _textColorPicker.Color = _messageDisplay.TextColor;
            _bgColorPicker.Color = _messageDisplay.BackgroundColor;

            // Set up content input
            _contentInput.Text = "Welcome to the Pixel Visualization Sandbox!\n\nThis tool allows you to test and visualize the pixel-based rendering system.\n\nTry adjusting the parameters to see how they affect the display.";

            // Set up ASCII art input
            _asciiArtInput.Text = "  _____  _          _ \n" +
                                 " |  __ \\(_)        | |\n" +
                                 " | |__) |___  _____| |\n" +
                                 " |  ___/| \\ \\/ / _ \\ |\n" +
                                 " | |    | |>  <  __/ |\n" +
                                 " |_|    |_/_/\\_\\___|_|";

            // Hide ASCII art input initially
            _asciiArtInput.Visible = false;
            _toggleAsciiArtButton.Text = "Show ASCII Art";
        }

        // Update the message display with current settings
        private void UpdateMessage()
        {
            // Get message type
            string messageType = _messageTypeDropdown.GetItemText(_messageTypeDropdown.Selected);
            _messageDisplay.MessageType = messageType;

            // Get content
            string content = _contentInput.Text;

            // Get interference level
            float interference = (float)_interferenceSlider.Value;

            // Update message display settings
            _messageDisplay.EnableScanlines = _enableScanlinesCheckbox.ButtonPressed;
            _messageDisplay.EnableScreenFlicker = _enableFlickerCheckbox.ButtonPressed;
            _messageDisplay.EnableTypewriterSound = _enableTypewriterSoundCheckbox.ButtonPressed;
            _messageDisplay.CharacterSize = (int)_characterSizeSpinBox.Value;
            _messageDisplay.TextColor = _textColorPicker.Color;
            _messageDisplay.BackgroundColor = _bgColorPicker.Color;

            // Set the message
            if (_showAsciiArt)
            {
                // Parse ASCII art from input
                _asciiArt = new List<string>(_asciiArtInput.Text.Split('\n'));
                _messageDisplay.SetMessageWithArt("sandbox_message", "Pixel Visualization Sandbox", content, _asciiArt, true, interference);
            }
            else
            {
                _messageDisplay.SetMessage("sandbox_message", "Pixel Visualization Sandbox", content, true, interference);
            }

            // Make sure the message is visible
            _messageDisplay.SetVisible(true);

            // Update debug info
            UpdateDebugInfo();
        }

        // Update the debug information display
        private void UpdateDebugInfo()
        {
            string debugInfo = $"Message Type: {_messageDisplay.MessageType}\n" +
                              $"Interference: {_interferenceSlider.Value:F2}\n" +
                              $"Character Size: {_messageDisplay.CharacterSize}\n" +
                              $"Scanlines: {_messageDisplay.EnableScanlines}\n" +
                              $"Flicker: {_messageDisplay.EnableScreenFlicker}\n" +
                              $"Typewriter Sound: {_messageDisplay.EnableTypewriterSound}\n" +
                              $"ASCII Art: {_showAsciiArt}\n" +
                              $"Grid: {_showGrid}";

            _debugInfoLabel.Text = debugInfo;
        }

        // Draw function for custom rendering
        public override void _Draw()
        {
            base._Draw();

            // Draw grid if enabled
            if (_showGrid)
            {
                DrawGrid();
            }
        }

        // Draw a pixel grid
        private void DrawGrid()
        {
            Vector2 size = Size;
            int gridSize = _messageDisplay.CharacterSize;
            Color gridColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);

            // Draw vertical lines
            for (int x = 0; x < size.X; x += gridSize)
            {
                DrawLine(new Vector2(x, 0), new Vector2(x, size.Y), gridColor);
            }

            // Draw horizontal lines
            for (int y = 0; y < size.Y; y += gridSize)
            {
                DrawLine(new Vector2(0, y), new Vector2(size.X, y), gridColor);
            }
        }

        // Event handlers
        private void OnTakeScreenshotPressed()
        {
            string screenshotPath = _screenshotTaker.TakeTimestampedScreenshot("pixel_sandbox");
            GD.Print($"Screenshot saved to: {screenshotPath}");
            _debugInfoLabel.Text = $"Screenshot saved to: {screenshotPath}\n\n{_debugInfoLabel.Text}";
        }

        private void OnToggleGridPressed()
        {
            _showGrid = !_showGrid;
            _toggleGridButton.Text = _showGrid ? "Hide Grid" : "Show Grid";
            QueueRedraw();
            UpdateDebugInfo();
        }

        private void OnToggleAsciiArtPressed()
        {
            _showAsciiArt = !_showAsciiArt;
            _toggleAsciiArtButton.Text = _showAsciiArt ? "Hide ASCII Art" : "Show ASCII Art";
            _asciiArtInput.Visible = _showAsciiArt;
            UpdateMessage();
        }

        private void OnInterferenceChanged(double value)
        {
            UpdateMessage();
        }

        private void OnMessageTypeSelected(long index)
        {
            UpdateMessage();
        }

        private void OnContentChanged()
        {
            UpdateMessage();
        }

        private void OnAsciiArtChanged()
        {
            if (_showAsciiArt)
            {
                UpdateMessage();
            }
        }

        private void OnScanlinesToggled(bool toggled)
        {
            UpdateMessage();
        }

        private void OnFlickerToggled(bool toggled)
        {
            UpdateMessage();
        }

        private void OnTypewriterSoundToggled(bool toggled)
        {
            UpdateMessage();
        }

        private void OnCharacterSizeChanged(double value)
        {
            UpdateMessage();
            if (_showGrid)
            {
                QueueRedraw();
            }
        }

        private void OnTextColorChanged(Color color)
        {
            UpdateMessage();
        }

        private void OnBgColorChanged(Color color)
        {
            UpdateMessage();
        }
    }
}

using Godot;
using System;

namespace SignalLost
{
    /// <summary>
    /// Demo scene for the pixel-styled UI elements.
    /// </summary>
    [GlobalClass]
    public partial class PixelUIDemo : Control
    {
        private PixelProgressBar _horizontalProgressBar;
        private Label _infoText;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _horizontalProgressBar = GetNode<PixelProgressBar>("MainPanel/ProgressBarsPanel/HorizontalProgressBar");
            _infoText = GetNode<Label>("MainPanel/InfoPanel/InfoText");
            
            // Set up initial state
            UpdateInfoText("Welcome to the Pixel UI Demo!");
        }
        
        // Update the info text
        private void UpdateInfoText(string text)
        {
            if (_infoText != null)
            {
                _infoText.Text = text;
            }
        }
        
        // Handle action button press
        private void _on_action_button_pressed()
        {
            UpdateInfoText("Button clicked! This demonstrates the button press event.");
            
            // Increment progress bar value
            if (_horizontalProgressBar != null)
            {
                float newValue = _horizontalProgressBar.Value + 0.1f;
                if (newValue > 1.0f)
                    newValue = 0.0f;
                    
                _horizontalProgressBar.SetValue(newValue);
            }
        }
        
        // Handle slider value change
        private void _on_horizontal_slider_value_changed(float value)
        {
            UpdateInfoText($"Slider value changed to: {value:F2}");
            
            // Update progress bar to match slider
            if (_horizontalProgressBar != null)
            {
                _horizontalProgressBar.SetValue(value);
            }
        }
        
        // Handle close button press
        private void _on_main_panel_close_requested()
        {
            UpdateInfoText("Close button clicked! In a real application, this would close the panel.");
        }
    }
}

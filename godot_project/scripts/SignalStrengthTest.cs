using Godot;
using System;

namespace SignalLost
{
    public partial class SignalStrengthTest : Control
    {
        // References to UI elements
        private SignalStrengthIndicator _standardIndicator;
        private PixelSignalStrengthIndicator _pixelIndicator;
        private HSlider _slider;
        private Label _sliderLabel;
        private Button _noSignalButton;
        private Button _weakButton;
        private Button _mediumButton;
        private Button _strongButton;
        private Button _perfectButton;
        private Button _screenshotButton;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _standardIndicator = GetNode<SignalStrengthIndicator>("VBoxContainer/StandardIndicator");
            _pixelIndicator = GetNode<PixelSignalStrengthIndicator>("VBoxContainer/PixelIndicator");
            _slider = GetNode<HSlider>("VBoxContainer/Slider");
            _sliderLabel = GetNode<Label>("VBoxContainer/SliderLabel");
            _noSignalButton = GetNode<Button>("VBoxContainer/TestButtons/NoSignalButton");
            _weakButton = GetNode<Button>("VBoxContainer/TestButtons/WeakButton");
            _mediumButton = GetNode<Button>("VBoxContainer/TestButtons/MediumButton");
            _strongButton = GetNode<Button>("VBoxContainer/TestButtons/StrongButton");
            _perfectButton = GetNode<Button>("VBoxContainer/TestButtons/PerfectButton");

            // Connect signals
            _slider.ValueChanged += OnSliderValueChanged;
            _noSignalButton.Pressed += () => SetSignalStrength(0.0f);
            _weakButton.Pressed += () => SetSignalStrength(0.25f);
            _mediumButton.Pressed += () => SetSignalStrength(0.5f);
            _strongButton.Pressed += () => SetSignalStrength(0.75f);
            _perfectButton.Pressed += () => SetSignalStrength(1.0f);

            // Set initial signal strength
            SetSignalStrength(_slider.Value / 100.0f);

            GD.Print("SignalStrengthTest ready!");
        }

        // Called when the slider value changes
        private void OnSliderValueChanged(double value)
        {
            float signalStrength = (float)value / 100.0f;
            SetSignalStrength(signalStrength);
        }

        // Set the signal strength for all indicators
        private void SetSignalStrength(float strength)
        {
            // Update slider if needed
            if (Math.Abs(_slider.Value - strength * 100.0f) > 0.01f)
            {
                _slider.Value = strength * 100.0f;
            }

            // Update label
            _sliderLabel.Text = $"Adjust Signal Strength: {strength * 100:F0}%";

            // Update indicators
            _standardIndicator.SetSignalStrength(strength);
            _pixelIndicator.SetSignalStrength(strength);

            GD.Print($"Signal strength set to {strength:F2}");
        }

        // Take a screenshot after a delay
        public void TakeScreenshot(float delay = 1.0f)
        {
            var timer = new Timer();
            AddChild(timer);
            timer.WaitTime = delay;
            timer.OneShot = true;
            timer.Timeout += () => {
                var image = GetViewport().GetTexture().GetImage();
                image.SavePng("user://signal_strength_test.png");
                GD.Print("Screenshot saved to user://signal_strength_test.png");
                timer.QueueFree();
            };
            timer.Start();
        }
    }
}

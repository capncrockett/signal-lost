using Godot;
using System.Collections.Generic;

namespace SignalLost.Gameplay
{
    /// <summary>
    /// Manages the loading screen.
    /// </summary>
    [GlobalClass]
    public partial class LoadingScreen : Control
    {
        // UI elements
        private Label _loadingLabel;
        private ProgressBar _progressBar;
        private Label _tipLabel;
        
        // Loading state
        private string _targetScene = "";
        private double _loadingTime = 0.0;
        private double _loadingDuration = 2.0; // Seconds to show loading screen
        
        // Tips
        private List<string> _tips = new List<string>
        {
            "Use your radio to find signals that might lead you to other survivors.",
            "Different frequencies can reveal different types of signals.",
            "Some signals are only available at certain times or locations.",
            "Keep an eye on your inventory - some items are essential for survival.",
            "Explore the area to find new locations and items.",
            "Complete quests to progress through the game.",
            "Some locations are only accessible with specific items.",
            "Pay attention to the content of radio messages - they might contain clues.",
            "The signal strength indicator shows how close you are to the exact frequency.",
            "Save your game regularly to avoid losing progress."
        };
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _loadingLabel = GetNode<Label>("MarginContainer/VBoxContainer/LoadingLabel");
            _progressBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/ProgressBar");
            _tipLabel = GetNode<Label>("MarginContainer/VBoxContainer/TipLabel");
            
            // Initialize loading state
            _progressBar.Value = 0;
            
            // Set random tip
            SetRandomTip();
        }
        
        // Called every frame
        public override void _Process(double delta)
        {
            // Update loading progress
            _loadingTime += delta;
            float progress = (float)(_loadingTime / _loadingDuration);
            _progressBar.Value = progress * 100;
            
            // Check if loading is complete
            if (_loadingTime >= _loadingDuration && !string.IsNullOrEmpty(_targetScene))
            {
                // Load the target scene
                GetTree().ChangeSceneToFile(_targetScene);
            }
        }
        
        // Set the target scene to load
        public void SetTargetScene(string scenePath)
        {
            _targetScene = scenePath;
        }
        
        // Set a random tip
        private void SetRandomTip()
        {
            if (_tips.Count > 0 && _tipLabel != null)
            {
                int index = new System.Random().Next(0, _tips.Count);
                _tipLabel.Text = _tips[index];
            }
        }
    }
}

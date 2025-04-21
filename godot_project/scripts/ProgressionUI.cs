using Godot;
using System;

namespace SignalLost
{
    /// <summary>
    /// UI for displaying game progression information.
    /// </summary>
    [GlobalClass]
    public partial class ProgressionUI : Control
    {
        // References
        private GameProgressionManager _progressionManager;
        private Label _stageLabel;
        private Label _descriptionLabel;
        private Label _objectiveLabel;
        private ProgressBar _progressBar;

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references
            _progressionManager = GetNode<GameProgressionManager>("/root/GameProgressionManager");
            _stageLabel = GetNode<Label>("VBoxContainer/StageLabel");
            _descriptionLabel = GetNode<Label>("VBoxContainer/DescriptionLabel");
            _objectiveLabel = GetNode<Label>("VBoxContainer/ObjectiveLabel");
            _progressBar = GetNode<ProgressBar>("VBoxContainer/ProgressBar");

            // Connect signals
            if (_progressionManager != null)
            {
                _progressionManager.ProgressionAdvanced += OnProgressionAdvanced;
            }

            // Update UI
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI with the current progression information.
        /// </summary>
        private void UpdateUI()
        {
            if (_progressionManager == null)
            {
                return;
            }

            // Update stage label
            _stageLabel.Text = $"Stage: {_progressionManager.CurrentStageName}";

            // Update description label
            _descriptionLabel.Text = _progressionManager.GetCurrentStageDescription();

            // Update objective label
            _objectiveLabel.Text = $"Next Objective: {_progressionManager.GetNextObjective()}";

            // Update progress bar
            int maxStages = Enum.GetValues(typeof(GameProgressionManager.ProgressionStage)).Length;
            _progressBar.MaxValue = maxStages - 1; // -1 because we start at 0
            _progressBar.Value = (int)_progressionManager.CurrentStage;
        }

        /// <summary>
        /// Called when the progression advances.
        /// </summary>
        /// <param name="newStage">The new progression stage</param>
        /// <param name="stageName">The name of the new stage</param>
        private void OnProgressionAdvanced(int newStage, string stageName)
        {
            // Update UI
            UpdateUI();

            // Show a notification
            var notification = new Label();
            notification.Text = $"Progression Advanced: {stageName}";
            notification.HorizontalAlignment = HorizontalAlignment.Center;
            notification.VerticalAlignment = VerticalAlignment.Center;
            notification.Position = new Vector2(GetViewportRect().Size.X / 2 - 100, 50);
            notification.Size = new Vector2(200, 50);
            notification.Modulate = new Color(1, 1, 0); // Yellow

            AddChild(notification);

            // Create a timer to remove the notification
            var timer = new Timer();
            timer.WaitTime = 3.0f;
            timer.OneShot = true;
            AddChild(timer);
            timer.Timeout += () =>
            {
                notification.QueueFree();
                timer.QueueFree();
            };
            timer.Start();
        }

        /// <summary>
        /// Set ProgressionManager reference (for testing)
        /// </summary>
        public void SetProgressionManager(GameProgressionManager progressionManager)
        {
            _progressionManager = progressionManager;
        }
    }
}

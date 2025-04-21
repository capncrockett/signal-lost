using Godot;
using System.Collections.Generic;
using SignalLost.Progression;

namespace SignalLost.UI
{
    /// <summary>
    /// UI component to display the game progression.
    /// </summary>
    [GlobalClass]
    public partial class GameProgressionDisplay : Control
    {
        // UI elements
        [Export] private Label _stageLabel;
        [Export] private Label _objectiveLabel;
        [Export] private ProgressBar _progressBar;
        [Export] private ItemList _milestonesList;
        
        // Reference to the game progression system
        private GameProgressionSystem _progressionSystem;
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get reference to the game progression system
            _progressionSystem = GetNode<GameProgressionSystem>("/root/GameProgressionSystem");
            
            if (_progressionSystem == null)
            {
                GD.PrintErr("GameProgressionDisplay: GameProgressionSystem not found");
                return;
            }
            
            // Connect signals
            _progressionSystem.StageChanged += OnStageChanged;
            _progressionSystem.ProgressChanged += OnProgressChanged;
            _progressionSystem.MilestoneCompleted += OnMilestoneCompleted;
            
            // Initialize UI
            UpdateUI();
        }
        
        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_progressionSystem != null)
            {
                _progressionSystem.StageChanged -= OnStageChanged;
                _progressionSystem.ProgressChanged -= OnProgressChanged;
                _progressionSystem.MilestoneCompleted -= OnMilestoneCompleted;
            }
        }
        
        // Handle stage changed
        private void OnStageChanged(int stageIndex)
        {
            // Update UI
            UpdateUI();
        }
        
        // Handle progress changed
        private void OnProgressChanged(int progress)
        {
            // Update progress bar
            if (_progressBar != null)
            {
                _progressBar.Value = progress;
            }
        }
        
        // Handle milestone completed
        private void OnMilestoneCompleted(string milestoneId)
        {
            // Update milestones list
            UpdateMilestonesList();
        }
        
        // Update the UI
        private void UpdateUI()
        {
            if (_progressionSystem == null)
                return;
                
            // Update stage label
            if (_stageLabel != null)
            {
                GameProgressionSystem.GameStage currentStage = _progressionSystem.GetCurrentStage();
                _stageLabel.Text = $"Stage: {currentStage}";
            }
            
            // Update objective label
            if (_objectiveLabel != null)
            {
                _objectiveLabel.Text = _progressionSystem.GetStageObjective();
            }
            
            // Update progress bar
            if (_progressBar != null)
            {
                _progressBar.Value = _progressionSystem.GetStageProgress();
            }
            
            // Update milestones list
            UpdateMilestonesList();
        }
        
        // Update the milestones list
        private void UpdateMilestonesList()
        {
            if (_milestonesList == null || _progressionSystem == null)
                return;
                
            // Clear the list
            _milestonesList.Clear();
            
            // Get current stage milestones
            List<StageMilestone> milestones = _progressionSystem.GetCurrentStageMilestones();
            
            // Add milestones to the list
            foreach (var milestone in milestones)
            {
                string status = milestone.IsCompleted ? "✓" : "□";
                _milestonesList.AddItem($"{status} {milestone.Description}");
            }
        }
    }
}

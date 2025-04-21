using Godot;
using SignalLost.Progression;

namespace SignalLost.Tests
{
    /// <summary>
    /// Test script for the game progression system.
    /// </summary>
    [GlobalClass]
    public partial class GameProgressionTest : Control
    {
        // Reference to the game progression system
        private GameProgressionSystem _progressionSystem;
        
        // UI elements
        private Button _findRadioPartsButton;
        private Button _repairRadioButton;
        private Button _discoverEmergencyBroadcastButton;
        private Button _discoverSurvivorSignalButton;
        private Button _visitOldMillButton;
        private Button _advanceStageButton;
        private Label _statusLabel;
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get reference to the game progression system
            _progressionSystem = GetNode<GameProgressionSystem>("GameProgressionSystem");
            
            if (_progressionSystem == null)
            {
                GD.PrintErr("GameProgressionTest: GameProgressionSystem not found");
                return;
            }
            
            // Get references to UI elements
            _findRadioPartsButton = GetNode<Button>("VBoxContainer/HBoxContainer/ControlPanel/MilestoneButtons/FindRadioPartsButton");
            _repairRadioButton = GetNode<Button>("VBoxContainer/HBoxContainer/ControlPanel/MilestoneButtons/RepairRadioButton");
            _discoverEmergencyBroadcastButton = GetNode<Button>("VBoxContainer/HBoxContainer/ControlPanel/MilestoneButtons/DiscoverEmergencyBroadcastButton");
            _discoverSurvivorSignalButton = GetNode<Button>("VBoxContainer/HBoxContainer/ControlPanel/MilestoneButtons/DiscoverSurvivorSignalButton");
            _visitOldMillButton = GetNode<Button>("VBoxContainer/HBoxContainer/ControlPanel/MilestoneButtons/VisitOldMillButton");
            _advanceStageButton = GetNode<Button>("VBoxContainer/HBoxContainer/ControlPanel/AdvanceStageButton");
            _statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");
            
            // Connect signals
            _findRadioPartsButton.Pressed += OnFindRadioPartsButtonPressed;
            _repairRadioButton.Pressed += OnRepairRadioButtonPressed;
            _discoverEmergencyBroadcastButton.Pressed += OnDiscoverEmergencyBroadcastButtonPressed;
            _discoverSurvivorSignalButton.Pressed += OnDiscoverSurvivorSignalButtonPressed;
            _visitOldMillButton.Pressed += OnVisitOldMillButtonPressed;
            _advanceStageButton.Pressed += OnAdvanceStageButtonPressed;
            
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
        
        // Handle find radio parts button pressed
        private void OnFindRadioPartsButtonPressed()
        {
            _progressionSystem.CompleteMilestone("find_radio_parts");
            UpdateStatusLabel("Milestone completed: Find Radio Parts");
        }
        
        // Handle repair radio button pressed
        private void OnRepairRadioButtonPressed()
        {
            _progressionSystem.CompleteMilestone("repair_radio");
            UpdateStatusLabel("Milestone completed: Repair Radio");
        }
        
        // Handle discover emergency broadcast button pressed
        private void OnDiscoverEmergencyBroadcastButtonPressed()
        {
            _progressionSystem.CompleteMilestone("discover_emergency_broadcast");
            UpdateStatusLabel("Milestone completed: Discover Emergency Broadcast");
        }
        
        // Handle discover survivor signal button pressed
        private void OnDiscoverSurvivorSignalButtonPressed()
        {
            _progressionSystem.CompleteMilestone("discover_survivor_signal");
            UpdateStatusLabel("Milestone completed: Discover Survivor Signal");
        }
        
        // Handle visit old mill button pressed
        private void OnVisitOldMillButtonPressed()
        {
            _progressionSystem.CompleteMilestone("visit_old_mill");
            UpdateStatusLabel("Milestone completed: Visit Old Mill");
        }
        
        // Handle advance stage button pressed
        private void OnAdvanceStageButtonPressed()
        {
            _progressionSystem.AdvanceStage();
            UpdateStatusLabel("Advanced to next stage");
        }
        
        // Handle stage changed
        private void OnStageChanged(int stageIndex)
        {
            UpdateUI();
            UpdateStatusLabel($"Stage changed to: {(GameProgressionSystem.GameStage)stageIndex}");
        }
        
        // Handle progress changed
        private void OnProgressChanged(int progress)
        {
            UpdateStatusLabel($"Progress changed to: {progress}%");
        }
        
        // Handle milestone completed
        private void OnMilestoneCompleted(string milestoneId)
        {
            UpdateStatusLabel($"Milestone completed: {milestoneId}");
        }
        
        // Update the UI
        private void UpdateUI()
        {
            if (_progressionSystem == null)
                return;
                
            // Update button states based on current stage
            GameProgressionSystem.GameStage currentStage = _progressionSystem.GetCurrentStage();
            
            // Beginning stage buttons
            _findRadioPartsButton.Disabled = currentStage != GameProgressionSystem.GameStage.Beginning || _progressionSystem.IsMilestoneCompleted("find_radio_parts");
            _repairRadioButton.Disabled = currentStage != GameProgressionSystem.GameStage.Beginning || _progressionSystem.IsMilestoneCompleted("repair_radio");
            
            // Exploration stage buttons
            _discoverEmergencyBroadcastButton.Disabled = currentStage != GameProgressionSystem.GameStage.Exploration || _progressionSystem.IsMilestoneCompleted("discover_emergency_broadcast");
            _discoverSurvivorSignalButton.Disabled = currentStage != GameProgressionSystem.GameStage.Exploration || _progressionSystem.IsMilestoneCompleted("discover_survivor_signal");
            _visitOldMillButton.Disabled = currentStage != GameProgressionSystem.GameStage.Exploration || _progressionSystem.IsMilestoneCompleted("visit_old_mill");
        }
        
        // Update the status label
        private void UpdateStatusLabel(string text)
        {
            if (_statusLabel != null)
            {
                _statusLabel.Text = text;
            }
        }
    }
}

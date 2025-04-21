using Godot;
using System.Collections.Generic;

namespace SignalLost.Gameplay
{
    /// <summary>
    /// Manages tutorial messages and guides the player through the game.
    /// </summary>
    [GlobalClass]
    public partial class TutorialSystem : Node
    {
        // UI elements
        [Export] private Control _tutorialPanel;
        [Export] private Label _tutorialLabel;
        [Export] private Button _tutorialDismissButton;
        
        // Tutorial state
        private Queue<string> _tutorialQueue = new Queue<string>();
        private bool _isTutorialActive = false;
        private double _tutorialTimer = 0.0;
        private double _tutorialDuration = 5.0; // Seconds to show tutorial message
        
        // Tutorial history
        private HashSet<string> _shownTutorials = new HashSet<string>();
        
        // References
        private GameplayManager _gameplayManager;
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references
            _gameplayManager = GetNode<GameplayManager>("/root/GameplayManager");
            
            if (_gameplayManager == null)
            {
                GD.PrintErr("TutorialSystem: Failed to get reference to GameplayManager");
                return;
            }
            
            // Connect signals
            _gameplayManager.TutorialMessage += OnTutorialMessage;
            _gameplayManager.GameStateChanged += OnGameStateChanged;
            
            if (_tutorialDismissButton != null)
            {
                _tutorialDismissButton.Pressed += OnTutorialDismissButtonPressed;
            }
            
            // Hide tutorial panel initially
            if (_tutorialPanel != null)
            {
                _tutorialPanel.Visible = false;
            }
        }
        
        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_gameplayManager != null)
            {
                _gameplayManager.TutorialMessage -= OnTutorialMessage;
                _gameplayManager.GameStateChanged -= OnGameStateChanged;
            }
            
            if (_tutorialDismissButton != null)
            {
                _tutorialDismissButton.Pressed -= OnTutorialDismissButtonPressed;
            }
        }
        
        // Called every frame
        public override void _Process(double delta)
        {
            // Update tutorial timer
            if (_isTutorialActive)
            {
                _tutorialTimer += delta;
                
                // Auto-dismiss tutorial after duration
                if (_tutorialTimer >= _tutorialDuration)
                {
                    DismissTutorial();
                }
            }
            else if (_tutorialQueue.Count > 0)
            {
                // Show next tutorial in queue
                ShowTutorial(_tutorialQueue.Dequeue());
            }
        }
        
        // Show a tutorial message
        private void ShowTutorial(string message)
        {
            if (_tutorialPanel == null || _tutorialLabel == null)
                return;
                
            // Set tutorial message
            _tutorialLabel.Text = message;
            
            // Show tutorial panel
            _tutorialPanel.Visible = true;
            
            // Reset tutorial timer
            _tutorialTimer = 0.0;
            _isTutorialActive = true;
            
            // Add to shown tutorials
            _shownTutorials.Add(message);
        }
        
        // Dismiss the current tutorial
        private void DismissTutorial()
        {
            if (_tutorialPanel == null)
                return;
                
            // Hide tutorial panel
            _tutorialPanel.Visible = false;
            
            // Reset tutorial state
            _isTutorialActive = false;
            _tutorialTimer = 0.0;
        }
        
        // Queue a tutorial message
        public void QueueTutorial(string message)
        {
            // Check if this tutorial has already been shown
            if (_shownTutorials.Contains(message))
                return;
                
            // Add to queue
            _tutorialQueue.Enqueue(message);
        }
        
        // Event handlers
        private void OnTutorialMessage(string message)
        {
            QueueTutorial(message);
        }
        
        private void OnGameStateChanged(string state)
        {
            // Show state-specific tutorials
            switch (state)
            {
                case "radio_repair":
                    QueueTutorial("Your radio is damaged and needs repair. Look for radio parts to fix it.");
                    break;
                    
                case "signal_discovery":
                    QueueTutorial("Your radio is working! Tune to different frequencies to find signals.");
                    break;
                    
                case "location_exploration":
                    QueueTutorial("You can now explore the area. Look for items and locations that might help you.");
                    break;
                    
                case "main_gameplay":
                    QueueTutorial("Complete quests to progress through the game. Use your radio to find clues.");
                    break;
            }
        }
        
        private void OnTutorialDismissButtonPressed()
        {
            DismissTutorial();
        }
    }
}

using Godot;

namespace SignalLost.Gameplay
{
    /// <summary>
    /// Manages the game over screen.
    /// </summary>
    [GlobalClass]
    public partial class GameOver : Control
    {
        // UI elements
        private Label _endingLabel;
        private Label _endingDescription;
        private Button _mainMenuButton;
        private Button _quitButton;
        
        // Ending data
        private string _endingType = "neutral";
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _endingLabel = GetNode<Label>("MarginContainer/VBoxContainer/EndingLabel");
            _endingDescription = GetNode<Label>("MarginContainer/VBoxContainer/EndingDescription");
            _mainMenuButton = GetNode<Button>("MarginContainer/VBoxContainer/MenuButtons/MainMenuButton");
            _quitButton = GetNode<Button>("MarginContainer/VBoxContainer/MenuButtons/QuitButton");
            
            // Connect signals
            _mainMenuButton.Pressed += OnMainMenuButtonPressed;
            _quitButton.Pressed += OnQuitButtonPressed;
            
            // Get ending type from game state
            var gameState = GetNode<GameState>("/root/GameState");
            if (gameState != null)
            {
                // In a real implementation, we would get the ending type from the game state
                // For now, we'll just use a default value
                _endingType = "neutral";
            }
            
            // Set ending text based on ending type
            SetEndingText(_endingType);
        }
        
        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_mainMenuButton != null)
                _mainMenuButton.Pressed -= OnMainMenuButtonPressed;
                
            if (_quitButton != null)
                _quitButton.Pressed -= OnQuitButtonPressed;
        }
        
        // Set the ending text based on the ending type
        private void SetEndingText(string endingType)
        {
            switch (endingType)
            {
                case "good":
                    _endingLabel.Text = "SIGNAL FOUND";
                    _endingDescription.Text = "You successfully found the source of the signal and made contact with the rescue team. " +
                        "Thanks to your efforts, you and the other survivors were rescued and brought to safety. " +
                        "The mysterious incident remains unexplained, but at least you made it out alive.";
                    break;
                    
                case "bad":
                    _endingLabel.Text = "SIGNAL LOST";
                    _endingDescription.Text = "You failed to find the source of the signal in time. " +
                        "As the last of your supplies ran out, you realized that no help was coming. " +
                        "The signal faded away, leaving you alone in the wilderness. " +
                        "Perhaps someone else will find your radio someday...";
                    break;
                    
                case "secret":
                    _endingLabel.Text = "THE TRUTH";
                    _endingDescription.Text = "You discovered the true source of the signal - a government experiment gone wrong. " +
                        "Armed with this knowledge, you managed to expose the truth to the world. " +
                        "The authorities weren't happy, but the public demanded answers. " +
                        "Your actions have changed the course of history, for better or worse.";
                    break;
                    
                case "neutral":
                default:
                    _endingLabel.Text = "JOURNEY'S END";
                    _endingDescription.Text = "After days of searching, you finally found a way out of the wilderness. " +
                        "The mystery of the signal remains unsolved, but at least you survived. " +
                        "As you look back at the forest one last time, you can't help but wonder what might have happened " +
                        "if you had made different choices along the way.";
                    break;
            }
        }
        
        // Event handlers
        private void OnMainMenuButtonPressed()
        {
            // Load the main menu scene
            GetTree().ChangeSceneToFile("res://scenes/gameplay/MainMenu.tscn");
        }
        
        private void OnQuitButtonPressed()
        {
            // Quit the game
            GetTree().Quit();
        }
    }
}

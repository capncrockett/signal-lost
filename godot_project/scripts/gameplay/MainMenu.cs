using Godot;

namespace SignalLost.Gameplay
{
    /// <summary>
    /// Manages the main menu of the game.
    /// </summary>
    [GlobalClass]
    public partial class MainMenu : Control
    {
        // UI elements
        private Button _newGameButton;
        private Button _loadGameButton;
        private Button _optionsButton;
        private Button _quitButton;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _newGameButton = GetNode<Button>("MarginContainer/VBoxContainer/MenuButtons/NewGameButton");
            _loadGameButton = GetNode<Button>("MarginContainer/VBoxContainer/MenuButtons/LoadGameButton");
            _optionsButton = GetNode<Button>("MarginContainer/VBoxContainer/MenuButtons/OptionsButton");
            _quitButton = GetNode<Button>("MarginContainer/VBoxContainer/MenuButtons/QuitButton");

            // Connect signals
            _newGameButton.Pressed += OnNewGameButtonPressed;
            _loadGameButton.Pressed += OnLoadGameButtonPressed;
            _optionsButton.Pressed += OnOptionsButtonPressed;
            _quitButton.Pressed += OnQuitButtonPressed;

            // Check if save game exists
            var saveGame = new SaveGame();
            _loadGameButton.Disabled = !saveGame.DoesSaveExist();
        }

        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_newGameButton != null)
                _newGameButton.Pressed -= OnNewGameButtonPressed;

            if (_loadGameButton != null)
                _loadGameButton.Pressed -= OnLoadGameButtonPressed;

            if (_optionsButton != null)
                _optionsButton.Pressed -= OnOptionsButtonPressed;

            if (_quitButton != null)
                _quitButton.Pressed -= OnQuitButtonPressed;
        }

        // Event handlers
        private void OnNewGameButtonPressed()
        {
            // Clear any existing game state
            var gameState = GetNode<GameState>("/root/GameState");
            if (gameState != null)
            {
                gameState.NewGame();
            }

            // Load the main gameplay scene
            GetTree().ChangeSceneToFile("res://scenes/MainGameScene.tscn");
        }

        private void OnLoadGameButtonPressed()
        {
            // Load the saved game
            var gameState = GetNode<GameState>("/root/GameState");
            if (gameState != null)
            {
                gameState.LoadGame();
            }

            // Load the main gameplay scene
            GetTree().ChangeSceneToFile("res://scenes/MainGameScene.tscn");
        }

        private void OnOptionsButtonPressed()
        {
            // Load the options scene
            // GetTree().ChangeSceneToFile("res://scenes/gameplay/Options.tscn");

            // For now, just print a message
            GD.Print("Options button pressed");
        }

        private void OnQuitButtonPressed()
        {
            // Quit the game
            GetTree().Quit();
        }
    }

    // Helper class for save game operations
    public class SaveGame
    {
        public bool DoesSaveExist()
        {
            return Godot.FileAccess.FileExists("user://savegame.json");
        }
    }
}

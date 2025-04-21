using Godot;

namespace SignalLost.Gameplay
{
    /// <summary>
    /// Main entry point for the game.
    /// </summary>
    [GlobalClass]
    public partial class Main : Node
    {
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Initialize game systems
            InitializeGameSystems();
            
            // Load the main menu scene
            GetTree().ChangeSceneToFile("res://scenes/gameplay/MainMenu.tscn");
        }
        
        // Initialize game systems
        private void InitializeGameSystems()
        {
            // Check if game systems are already initialized
            var gameState = GetNode<GameState>("/root/GameState");
            if (gameState == null)
            {
                GD.PrintErr("Main: GameState not found");
                return;
            }
            
            // Initialize game state
            gameState.Initialize();
            
            // Print initialization message
            GD.Print("Main: Game systems initialized");
        }
    }
}

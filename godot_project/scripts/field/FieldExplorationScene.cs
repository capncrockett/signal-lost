using Godot;
using System;

namespace SignalLost.Field
{
    /// <summary>
    /// Main scene for the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class FieldExplorationScene : Node2D
    {
        // References
        private GridSystem _gridSystem;
        private PlayerController _playerController;
        private Camera2D _camera;
        
        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references
            _gridSystem = GetNode<GridSystem>("GridSystem");
            _playerController = GetNode<PlayerController>("PlayerController");
            _camera = GetNode<Camera2D>("Camera2D");
            
            // Initialize camera
            InitializeCamera();
            
            GD.Print("FieldExplorationScene: Initialized");
        }
        
        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="delta">Time since the last frame</param>
        public override void _Process(double delta)
        {
            // Update camera position to follow player
            if (_camera != null && _playerController != null)
            {
                _camera.Position = _playerController.Position;
            }
        }
        
        /// <summary>
        /// Initializes the camera.
        /// </summary>
        private void InitializeCamera()
        {
            if (_camera != null && _playerController != null)
            {
                // Set initial camera position to follow player
                _camera.Position = _playerController.Position;
                
                // Set camera limits based on grid size
                int cellSize = _gridSystem.GetCellSize();
                int width = _gridSystem.GetWidth();
                int height = _gridSystem.GetHeight();
                
                _camera.LimitLeft = 0;
                _camera.LimitTop = 0;
                _camera.LimitRight = width * cellSize;
                _camera.LimitBottom = height * cellSize;
                
                GD.Print($"FieldExplorationScene: Set camera limits to {width * cellSize}x{height * cellSize}");
            }
        }
    }
}

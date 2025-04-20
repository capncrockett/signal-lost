using Godot;

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
        private SignalSourceManager _signalSourceManager;

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references
            _gridSystem = GetNode<GridSystem>("GridSystem");
            _playerController = GetNode<PlayerController>("PlayerController");
            _camera = GetNode<Camera2D>("Camera2D");
            _signalSourceManager = GetNode<SignalSourceManager>("SignalSourceManager");

            // Initialize camera
            InitializeCamera();

            // Create some test signal sources if they don't exist
            CreateTestSignalSources();

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

        /// <summary>
        /// Creates test signal sources in the scene.
        /// </summary>
        private void CreateTestSignalSources()
        {
            // Check if we already have signal sources
            var existingSources = GetTree().GetNodesInGroup("SignalSource");
            if (existingSources.Count > 0)
            {
                GD.Print($"FieldExplorationScene: Found {existingSources.Count} existing signal sources");
                return;
            }

            // Create some test signal sources at different locations
            CreateSignalSource(new Vector2I(5, 5), 91.5f, "signal_emergency", 1.0f, 8.0f);
            CreateSignalSource(new Vector2I(15, 10), 95.7f, "signal_test", 0.8f, 6.0f);
            CreateSignalSource(new Vector2I(3, 12), 103.2f, "signal_beacon", 0.9f, 7.0f);

            GD.Print("FieldExplorationScene: Created test signal sources");
        }

        /// <summary>
        /// Creates a signal source at the specified position.
        /// </summary>
        /// <param name="position">The grid position for the signal source</param>
        /// <param name="frequency">The frequency of the signal</param>
        /// <param name="messageId">The ID of the message associated with the signal</param>
        /// <param name="strength">The strength of the signal</param>
        /// <param name="range">The range of the signal in grid cells</param>
        private void CreateSignalSource(Vector2I position, float frequency, string messageId, float strength, float range)
        {
            // Create the signal source object
            var signalSource = new SignalSourceObject();
            signalSource.Frequency = frequency;
            signalSource.MessageId = messageId;
            signalSource.SignalStrength = strength;
            signalSource.SignalRange = range;

            // Set the position
            signalSource.Position = _gridSystem.GridToWorldPosition(position);

            // Add to the scene
            AddChild(signalSource);

            GD.Print($"FieldExplorationScene: Created signal source at position {position}, frequency {frequency} MHz");
        }
    }
}

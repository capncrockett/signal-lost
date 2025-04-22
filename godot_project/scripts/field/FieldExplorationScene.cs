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
        private DayNightCycleManager _dayNightCycleManager;
        private SignalLost.GameState _gameState;

        // UI elements
        private Label _timeDisplay;
        private Label _signalStrengthDisplay;
        private Label _interferenceDisplay;
        private Panel _hazardInfoPanel;
        private Label _hazardNameLabel;
        private Label _hazardDescriptionLabel;

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
            _dayNightCycleManager = GetNode<DayNightCycleManager>("DayNightCycleManager");
            _gameState = GetNode<SignalLost.GameState>("/root/GameState");

            // Get UI elements
            _timeDisplay = GetNode<Label>("UI/TimeDisplay");
            _signalStrengthDisplay = GetNode<Label>("UI/SignalStrengthDisplay");
            _interferenceDisplay = GetNode<Label>("UI/InterferenceDisplay");
            _hazardInfoPanel = GetNode<Panel>("UI/HazardInfoPanel");
            _hazardNameLabel = GetNode<Label>("UI/HazardInfoPanel/HazardNameLabel");
            _hazardDescriptionLabel = GetNode<Label>("UI/HazardInfoPanel/HazardDescriptionLabel");

            // Initialize camera
            InitializeCamera();

            // Create some test signal sources if they don't exist
            CreateTestSignalSources();

            // Connect signals
            if (_dayNightCycleManager != null)
            {
                _dayNightCycleManager.TimeChanged += OnTimeChanged;
                _dayNightCycleManager.DayNightChanged += OnDayNightChanged;
            }

            if (_gameState != null)
            {
                _gameState.SignalInterferenceChanged += OnSignalInterferenceChanged;
            }

            // Connect to environmental hazards
            ConnectToEnvironmentalHazards();

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

            // Handle keyboard input
            HandleKeyboardInput();

            // Update signal strength display
            UpdateSignalStrengthDisplay(CalculateCurrentSignalStrength());
        }

        /// <summary>
        /// Handles keyboard input for the field exploration scene.
        /// </summary>
        private void HandleKeyboardInput()
        {
            // Advance time by 1 hour when T is pressed
            if (Input.IsActionJustPressed("ui_focus_next") || Input.IsKeyJustPressed(Key.T))
            {
                if (_dayNightCycleManager != null)
                {
                    _dayNightCycleManager.AdvanceTime(1.0f);
                    GD.Print("Advanced time by 1 hour");
                }
            }

            // Toggle day/night cycle when D is pressed
            if (Input.IsKeyJustPressed(Key.D))
            {
                if (_dayNightCycleManager != null)
                {
                    bool automatic = !_dayNightCycleManager.AutomaticCycle;
                    _dayNightCycleManager.SetAutomaticCycle(automatic);
                    GD.Print($"Day/night cycle automatic mode: {automatic}");
                }
            }
        }

        /// <summary>
        /// Calculates the current signal strength based on player position and radio settings.
        /// </summary>
        /// <returns>The current signal strength (0.0 to 1.0)</returns>
        private float CalculateCurrentSignalStrength()
        {
            if (_gameState == null || !_gameState.IsRadioOn)
            {
                return 0.0f;
            }

            // Get current frequency
            float frequency = _gameState.CurrentFrequency;

            // Find signal at current frequency
            var signalData = _gameState.FindSignalAtFrequency(frequency);
            if (signalData == null)
            {
                return 0.0f;
            }

            // Calculate base signal strength
            float baseStrength = SignalLost.GameState.CalculateSignalStrength(frequency, signalData);

            // Apply interference
            float finalStrength = _gameState.ApplyInterferenceToSignal(baseStrength);

            return finalStrength;
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
            CreateSignalSource(new Vector2I(25, 5), 91.5f, "signal_emergency", 1.0f, 8.0f);
            CreateSignalSource(new Vector2I(15, 15), 95.7f, "signal_test", 0.8f, 6.0f);
            CreateSignalSource(new Vector2I(5, 10), 103.2f, "signal_beacon", 0.9f, 7.0f);

            GD.Print("FieldExplorationScene: Created test signal sources");
        }

        /// <summary>
        /// Connects to environmental hazards in the scene.
        /// </summary>
        private void ConnectToEnvironmentalHazards()
        {
            // Find all environmental hazards in the scene
            var hazards = GetTree().GetNodesInGroup("Hazard");
            foreach (var hazard in hazards)
            {
                if (hazard is EnvironmentalHazard environmentalHazard)
                {
                    // Connect signals
                    environmentalHazard.PlayerEnteredHazard += OnPlayerEnteredHazard;
                    environmentalHazard.PlayerExitedHazard += OnPlayerExitedHazard;
                    environmentalHazard.HazardDeactivated += OnHazardDeactivated;

                    GD.Print($"FieldExplorationScene: Connected to hazard {environmentalHazard.HazardName}");
                }
            }
        }

        /// <summary>
        /// Called when the player enters a hazard.
        /// </summary>
        /// <param name="hazardId">The ID of the hazard</param>
        private void OnPlayerEnteredHazard(string hazardId)
        {
            // Find the hazard
            var hazards = GetTree().GetNodesInGroup("Hazard");
            foreach (var hazard in hazards)
            {
                if (hazard is EnvironmentalHazard environmentalHazard && environmentalHazard.HazardId == hazardId)
                {
                    // Show hazard info
                    _hazardInfoPanel.Visible = true;
                    _hazardNameLabel.Text = environmentalHazard.HazardName;
                    _hazardDescriptionLabel.Text = environmentalHazard.HazardDescription;

                    GD.Print($"FieldExplorationScene: Player entered hazard {environmentalHazard.HazardName}");
                    break;
                }
            }
        }

        /// <summary>
        /// Called when the player exits a hazard.
        /// </summary>
        /// <param name="hazardId">The ID of the hazard</param>
        private void OnPlayerExitedHazard(string hazardId)
        {
            // Hide hazard info
            _hazardInfoPanel.Visible = false;

            GD.Print($"FieldExplorationScene: Player exited hazard {hazardId}");
        }

        /// <summary>
        /// Called when a hazard is deactivated.
        /// </summary>
        /// <param name="hazardId">The ID of the hazard</param>
        private void OnHazardDeactivated(string hazardId)
        {
            // Hide hazard info if it's the current hazard
            if (_hazardInfoPanel.Visible && _hazardNameLabel.Text == hazardId)
            {
                _hazardInfoPanel.Visible = false;
            }

            GD.Print($"FieldExplorationScene: Hazard deactivated {hazardId}");
        }

        /// <summary>
        /// Called when the time of day changes.
        /// </summary>
        /// <param name="time">The new time</param>
        private void OnTimeChanged(float time)
        {
            // Update time display
            if (_timeDisplay != null)
            {
                int hours = (int)time;
                int minutes = (int)((time - hours) * 60);
                _timeDisplay.Text = $"Time: {hours:D2}:{minutes:D2}";
            }
        }

        /// <summary>
        /// Called when day/night status changes.
        /// </summary>
        /// <param name="isDaytime">Whether it's daytime</param>
        private void OnDayNightChanged(bool isDaytime)
        {
            GD.Print($"FieldExplorationScene: Day/night changed to {(isDaytime ? "day" : "night")}");
        }

        /// <summary>
        /// Called when signal interference changes.
        /// </summary>
        /// <param name="interferenceLevel">The new interference level</param>
        private void OnSignalInterferenceChanged(float interferenceLevel)
        {
            // Update interference display
            if (_interferenceDisplay != null)
            {
                _interferenceDisplay.Text = $"Interference: {interferenceLevel * 100:F0}%";
            }
        }

        /// <summary>
        /// Updates the signal strength display.
        /// </summary>
        /// <param name="signalStrength">The signal strength</param>
        public void UpdateSignalStrengthDisplay(float signalStrength)
        {
            if (_signalStrengthDisplay != null)
            {
                _signalStrengthDisplay.Text = $"Signal: {signalStrength * 100:F0}%";
            }
        }

        /// <summary>
        /// Creates a signal source at the specified position.
        /// </summary>
        /// <param name="position">The grid position for the signal source</param>
        /// <param name="frequency">The frequency of the signal</param>
        /// <param name="messageId">The ID of the message associated with the signal</param>
        /// <param name="strength">The strength of the signal</param>
        /// <param name="range">The range of the signal in grid cells</param>
        public void CreateSignalSource(Vector2I position, float frequency, string messageId, float strength, float range)
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

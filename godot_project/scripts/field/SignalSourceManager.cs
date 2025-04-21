using Godot;
using System.Collections.Generic;

namespace SignalLost.Field
{
    /// <summary>
    /// Manages signal sources in the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class SignalSourceManager : Node
    {
        // List of signal sources in the field
        private List<SignalSourceObject> _signalSources = new List<SignalSourceObject>();

        // Reference to the radio system
        private RadioSystem _radioSystem;

        // Reference to the player controller
        private PlayerController _playerController;

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references
            _radioSystem = GetNode<RadioSystem>("/root/GameState/RadioSystem");
            _playerController = GetNode<PlayerController>("/root/FieldExplorationScene/PlayerController");

            if (_radioSystem == null)
            {
                GD.PrintErr("SignalSourceManager: Failed to find RadioSystem node");
            }

            if (_playerController == null)
            {
                GD.PrintErr("SignalSourceManager: Failed to find PlayerController node");
            }

            // Find all signal sources in the scene
            FindSignalSources();

            GD.Print($"SignalSourceManager: Initialized with {_signalSources.Count} signal sources");
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="delta">Time since the last frame</param>
        public override void _Process(double delta)
        {
            // Update signal strengths based on player position
            if (_playerController != null && _radioSystem != null)
            {
                UpdateSignalStrengths();
            }
        }

        /// <summary>
        /// Finds all signal sources in the scene.
        /// </summary>
        private void FindSignalSources()
        {
            // Clear the list
            _signalSources.Clear();

            // Find all signal sources in the scene
            var signalSources = GetTree().GetNodesInGroup("SignalSource");
            foreach (var source in signalSources)
            {
                if (source is SignalSourceObject signalSource)
                {
                    _signalSources.Add(signalSource);
                    GD.Print($"SignalSourceManager: Found signal source at frequency {signalSource.Frequency} MHz");
                }
            }
        }

        /// <summary>
        /// Updates signal strengths based on player position.
        /// </summary>
        private void UpdateSignalStrengths()
        {
            // Get player position
            Vector2I playerPosition = _playerController.GetGridPosition();

            // Update signal strengths for each signal source
            foreach (var source in _signalSources)
            {
                // Calculate signal strength based on distance
                float strength = source.CalculateSignalStrengthAtPosition(playerPosition);

                // Update the signal strength in the radio system
                _radioSystem.UpdateSignalSourceStrength(source.Frequency, strength);
            }
        }

        /// <summary>
        /// Adds a signal source to the manager.
        /// </summary>
        /// <param name="source">The signal source to add</param>
        public void AddSignalSource(SignalSourceObject source)
        {
            if (!_signalSources.Contains(source))
            {
                _signalSources.Add(source);
                GD.Print($"SignalSourceManager: Added signal source at frequency {source.Frequency} MHz");
            }
        }

        /// <summary>
        /// Removes a signal source from the manager.
        /// </summary>
        /// <param name="source">The signal source to remove</param>
        public void RemoveSignalSource(SignalSourceObject source)
        {
            if (_signalSources.Contains(source))
            {
                _signalSources.Remove(source);
                GD.Print($"SignalSourceManager: Removed signal source at frequency {source.Frequency} MHz");
            }
        }

        /// <summary>
        /// Gets all signal sources.
        /// </summary>
        /// <returns>The list of signal sources</returns>
        public List<SignalSourceObject> GetSignalSources()
        {
            return _signalSources;
        }

        /// <summary>
        /// Set RadioSystem reference (for testing)
        /// </summary>
        public void SetRadioSystem(RadioSystem radioSystem)
        {
            _radioSystem = radioSystem;
        }

        /// <summary>
        /// Set PlayerController reference (for testing)
        /// </summary>
        public void SetPlayerController(PlayerController playerController)
        {
            _playerController = playerController;
        }
    }
}

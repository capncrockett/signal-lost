using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class MessageManager : Node
    {
        // Reference to the message display
        private PixelMessageDisplay _messageDisplay;

        // Reference to game state
        private GameState _gameState;

        // Current message ID
        private string _currentMessageId;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("MessageManager._Ready() called");
            try
            {
                // Get references to singletons
                _gameState = GetNode<GameState>("/root/GameState");

                // Create the message display
                _messageDisplay = new PixelMessageDisplay();
                _messageDisplay.Name = "PixelMessageDisplay";
                _messageDisplay.AnchorRight = 1.0f;
                _messageDisplay.AnchorBottom = 1.0f;
                _messageDisplay.SizeFlagsHorizontal = Control.SizeFlags.Fill;
                _messageDisplay.SizeFlagsVertical = Control.SizeFlags.Fill;

                // Set initial visibility
                _messageDisplay.SetVisible(false);

                // Connect signals
                _messageDisplay.MessageClosed += OnMessageClosed;
                _messageDisplay.DecodeRequested += OnDecodeRequested;

                // Add to scene
                AddChild(_messageDisplay);

                GD.Print("MessageManager initialized successfully");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in MessageManager._Ready(): {ex.Message}");
                GD.PrintErr(ex.StackTrace);
            }
        }

        // Show a message by ID
        public void ShowMessage(string messageId)
        {
            if (_gameState == null) return;

            var message = _gameState.GetMessage(messageId);
            if (message != null)
            {
                _currentMessageId = messageId;

                // Calculate interference based on signal strength
                float interference = 0.0f;
                var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);
                if (signalData != null)
                {
                    float signalStrength = GameState.CalculateSignalStrength(_gameState.CurrentFrequency, signalData);
                    interference = 1.0f - signalStrength;
                }

                // Show the message
                _messageDisplay.SetMessage(
                    messageId,
                    message.Title,
                    message.Content,
                    message.Decoded,
                    interference
                );

                _messageDisplay.SetVisible(true);
            }
        }

        // Hide the current message
        public void HideMessage()
        {
            _messageDisplay.SetVisible(false);
            _currentMessageId = null;
        }

        // Check if a message is currently visible
        public bool IsMessageVisible()
        {
            return _messageDisplay != null && _messageDisplay.Visible;
        }

        // Signal handlers
        private void OnMessageClosed()
        {
            _currentMessageId = null;
        }

        private void OnDecodeRequested(string messageId)
        {
            if (_gameState != null)
            {
                bool decoded = _gameState.DecodeMessage(messageId);
                if (decoded)
                {
                    // Refresh the message display with the decoded message
                    ShowMessage(messageId);
                }
            }
        }
    }
}

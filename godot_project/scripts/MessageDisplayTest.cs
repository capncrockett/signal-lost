using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class MessageDisplayTest : Control
    {
        private Button _testButton;
        private PixelMessageDisplay _messageDisplay;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _testButton = GetNode<Button>("TestButton");
            _messageDisplay = GetNode<PixelMessageDisplay>("MessageDisplay");
            
            // Connect signals
            _testButton.Pressed += OnTestButtonPressed;
            
            if (_messageDisplay != null)
            {
                _messageDisplay.MessageClosed += OnMessageClosed;
                _messageDisplay.DecodeRequested += OnDecodeRequested;
            }
            else
            {
                GD.PrintErr("MessageDisplay not found!");
            }
        }
        
        // Show a test message
        private void OnTestButtonPressed()
        {
            if (_messageDisplay != null)
            {
                _messageDisplay.SetMessage(
                    "test_message",
                    "Test Message",
                    "This is a test message to demonstrate the pixel-based message display system. It includes a typewriter effect and visual noise to simulate interference.",
                    false,
                    0.3f
                );
                
                _messageDisplay.SetVisible(true);
            }
        }
        
        // Handle message closed event
        private void OnMessageClosed()
        {
            GD.Print("Message closed");
        }
        
        // Handle decode requested event
        private void OnDecodeRequested(string messageId)
        {
            GD.Print($"Decode requested for message: {messageId}");
            
            // Simulate decoding by showing the message again with less interference
            if (_messageDisplay != null)
            {
                _messageDisplay.SetMessage(
                    messageId,
                    "Decoded Message",
                    "This message has been decoded! The interference has been reduced, making it easier to read.",
                    true,
                    0.1f
                );
            }
        }
    }
}

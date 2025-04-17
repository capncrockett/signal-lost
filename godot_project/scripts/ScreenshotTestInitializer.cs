using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class ScreenshotTestInitializer : Node
    {
        public override void _Ready()
        {
            GD.Print("ScreenshotTestInitializer ready!");
            
            // Find the PixelMessageDisplay
            var messageDisplay = GetNode<PixelMessageDisplay>("../PixelMessageDisplay");
            
            if (messageDisplay != null)
            {
                // Set a test message
                messageDisplay.SetMessage(
                    "test_message_1",
                    "SIGNAL LOST - TEST MESSAGE",
                    "This is a test message to demonstrate the PixelMessageDisplay component. " +
                    "The game involves finding and decoding radio signals to uncover the story. " +
                    "This message would typically contain clues or narrative elements that the player discovers " +
                    "by tuning their radio to the correct frequency.",
                    false,
                    0.2f
                );
            }
            else
            {
                GD.PrintErr("Could not find PixelMessageDisplay node!");
            }
        }
    }
}

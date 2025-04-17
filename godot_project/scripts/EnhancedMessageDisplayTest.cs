using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class EnhancedMessageDisplayTest : Control
    {
        // UI references
        private Button _terminalButton;
        private Button _radioButton;
        private Button _noteButton;
        private Button _computerButton;
        private Button _asciiArtButton;
        private PixelMessageDisplay _messageDisplay;
        private ScreenshotTaker _screenshotTaker;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to UI elements
            _terminalButton = GetNode<Button>("%TerminalButton");
            _radioButton = GetNode<Button>("%RadioButton");
            _noteButton = GetNode<Button>("%NoteButton");
            _computerButton = GetNode<Button>("%ComputerButton");
            _asciiArtButton = GetNode<Button>("%AsciiArtButton");
            _messageDisplay = GetNode<PixelMessageDisplay>("%MessageDisplay");
            
            // Try to get screenshot taker
            _screenshotTaker = GetNode<ScreenshotTaker>("/root/ScreenshotTaker");
            if (_screenshotTaker == null)
            {
                _screenshotTaker = new ScreenshotTaker();
                AddChild(_screenshotTaker);
            }
            
            // Connect signals
            _terminalButton.Pressed += OnTerminalButtonPressed;
            _radioButton.Pressed += OnRadioButtonPressed;
            _noteButton.Pressed += OnNoteButtonPressed;
            _computerButton.Pressed += OnComputerButtonPressed;
            _asciiArtButton.Pressed += OnAsciiArtButtonPressed;
            
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
        
        // Show a terminal message
        private void OnTerminalButtonPressed()
        {
            if (_messageDisplay != null)
            {
                _messageDisplay.MessageType = "Terminal";
                _messageDisplay.TextColor = new Color(0.0f, 0.8f, 0.0f, 1.0f); // Green
                _messageDisplay.BackgroundColor = new Color(0.05f, 0.05f, 0.05f, 1.0f); // Dark gray
                _messageDisplay.EnableScanlines = true;
                _messageDisplay.EnableScreenFlicker = true;
                _messageDisplay.ShowTimestamp = true;
                
                _messageDisplay.SetMessage(
                    "terminal_message",
                    "TERMINAL ACCESS",
                    "System initialized. Terminal ready for input.\n\nWARNING: Unauthorized access to this terminal is prohibited. All activities are logged and monitored.\n\nEnter command sequence to proceed with system diagnostics.",
                    false,
                    0.2f
                );
            }
        }
        
        // Show a radio message
        private void OnRadioButtonPressed()
        {
            if (_messageDisplay != null)
            {
                _messageDisplay.MessageType = "Radio";
                _messageDisplay.TextColor = new Color(0.9f, 0.9f, 0.2f, 1.0f); // Yellow
                _messageDisplay.BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1.0f); // Dark gray
                _messageDisplay.EnableScanlines = false;
                _messageDisplay.EnableScreenFlicker = true;
                _messageDisplay.ShowTimestamp = false;
                
                _messageDisplay.SetMessage(
                    "radio_message",
                    "RADIO TRANSMISSION",
                    "...signal detected at frequency 87.5 MHz...\n\n*static*\n\n...coordinates received...location confirmed...send backup immediately...\n\n*static*\n\n...situation critical...require immediate extraction...",
                    false,
                    0.6f
                );
            }
        }
        
        // Show a note message
        private void OnNoteButtonPressed()
        {
            if (_messageDisplay != null)
            {
                _messageDisplay.MessageType = "Note";
                _messageDisplay.TextColor = new Color(0.1f, 0.1f, 0.1f, 1.0f); // Black
                _messageDisplay.BackgroundColor = new Color(0.95f, 0.95f, 0.9f, 1.0f); // Off-white
                _messageDisplay.EnableScanlines = false;
                _messageDisplay.EnableScreenFlicker = false;
                _messageDisplay.ShowTimestamp = false;
                
                _messageDisplay.SetMessage(
                    "note_message",
                    "HANDWRITTEN NOTE",
                    "If you're reading this, I'm probably gone. The facility has been compromised. Don't trust anyone who claims to be from management.\n\nThe access code to the lower levels is 3-5-8-9. Use it wisely.\n\nDestroy this note after reading.",
                    false,
                    0.0f
                );
            }
        }
        
        // Show a computer message
        private void OnComputerButtonPressed()
        {
            if (_messageDisplay != null)
            {
                _messageDisplay.MessageType = "Computer";
                _messageDisplay.TextColor = new Color(0.0f, 0.6f, 0.9f, 1.0f); // Blue
                _messageDisplay.BackgroundColor = new Color(0.0f, 0.0f, 0.2f, 1.0f); // Dark blue
                _messageDisplay.EnableScanlines = true;
                _messageDisplay.EnableScreenFlicker = true;
                _messageDisplay.ShowTimestamp = true;
                
                _messageDisplay.SetMessage(
                    "computer_message",
                    "SYSTEM ALERT",
                    "CRITICAL SYSTEM FAILURE DETECTED\n\nPrimary power systems offline\nBackup generators at 23% capacity\nEstimated time until complete shutdown: 47 minutes\n\nEvacuation protocols initiated\nAll personnel proceed to emergency exits immediately",
                    false,
                    0.3f
                );
            }
        }
        
        // Show a message with ASCII art
        private void OnAsciiArtButtonPressed()
        {
            if (_messageDisplay != null)
            {
                _messageDisplay.MessageType = "Terminal";
                _messageDisplay.TextColor = new Color(0.0f, 0.8f, 0.0f, 1.0f); // Green
                _messageDisplay.BackgroundColor = new Color(0.05f, 0.05f, 0.05f, 1.0f); // Dark gray
                _messageDisplay.EnableScanlines = true;
                _messageDisplay.EnableScreenFlicker = true;
                _messageDisplay.ShowTimestamp = true;
                
                // Create ASCII art
                List<string> asciiArt = new List<string>
                {
                    "    _____  _                   _   _                 _   ",
                    "   / ____|(_)                 | | | |               | |  ",
                    "  | (___   _   __ _  _ __   __ _ | | | |     ___   ___| |_ ",
                    "   \\___ \\ | | / _` || '_ \\ / _` || | | |    / _ \\ / __| __|",
                    "   ____) || || (_| || | | || (_| || | | |___| (_) |\\__ \\ |_ ",
                    "  |_____/ |_| \\__, ||_| |_| \\__,_||_| |______\\___/ |___/\\__|",
                    "               __/ |                                      ",
                    "              |___/                                       "
                };
                
                _messageDisplay.SetMessageWithArt(
                    "ascii_art_message",
                    "SYSTEM IDENTIFICATION",
                    "Welcome to the Signal Lost terminal system.\n\nThis system is designed for tracking and decoding radio signals in the field.\n\nPlease enter your credentials to continue.",
                    asciiArt,
                    false,
                    0.1f
                );
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
                switch (messageId)
                {
                    case "terminal_message":
                        _messageDisplay.SetMessage(
                            messageId,
                            "TERMINAL ACCESS - DECODED",
                            "System initialized. Terminal ready for input.\n\nWARNING: Unauthorized access to this terminal is prohibited. All activities are logged and monitored.\n\nEnter command sequence to proceed with system diagnostics.",
                            true,
                            0.0f
                        );
                        break;
                        
                    case "radio_message":
                        _messageDisplay.SetMessage(
                            messageId,
                            "RADIO TRANSMISSION - DECODED",
                            "Signal detected at frequency 87.5 MHz. Coordinates received: 42.3601° N, 71.0589° W. Location confirmed as Boston, Massachusetts.\n\nSend backup immediately. Situation critical. Require immediate extraction. Team compromised. Hostiles in vicinity.",
                            true,
                            0.1f
                        );
                        break;
                        
                    case "note_message":
                        _messageDisplay.SetMessage(
                            messageId,
                            "HANDWRITTEN NOTE - DECODED",
                            "If you're reading this, I'm probably gone. The facility has been compromised. Don't trust anyone who claims to be from management.\n\nThe access code to the lower levels is 3-5-8-9. Use it wisely.\n\nDestroy this note after reading.",
                            true,
                            0.0f
                        );
                        break;
                        
                    case "computer_message":
                        _messageDisplay.SetMessage(
                            messageId,
                            "SYSTEM ALERT - DECODED",
                            "CRITICAL SYSTEM FAILURE DETECTED\n\nPrimary power systems offline\nBackup generators at 23% capacity\nEstimated time until complete shutdown: 47 minutes\n\nEvacuation protocols initiated\nAll personnel proceed to emergency exits immediately\n\nSECURITY OVERRIDE: Use maintenance tunnels in sector B7 for safe exit.",
                            true,
                            0.0f
                        );
                        break;
                        
                    case "ascii_art_message":
                        // Keep the same ASCII art
                        List<string> asciiArt = new List<string>
                        {
                            "    _____  _                   _   _                 _   ",
                            "   / ____|(_)                 | | | |               | |  ",
                            "  | (___   _   __ _  _ __   __ _ | | | |     ___   ___| |_ ",
                            "   \\___ \\ | | / _` || '_ \\ / _` || | | |    / _ \\ / __| __|",
                            "   ____) || || (_| || | | || (_| || | | |___| (_) |\\__ \\ |_ ",
                            "  |_____/ |_| \\__, ||_| |_| \\__,_||_| |______\\___/ |___/\\__|",
                            "               __/ |                                      ",
                            "              |___/                                       "
                        };
                        
                        _messageDisplay.SetMessageWithArt(
                            messageId,
                            "SYSTEM IDENTIFICATION - DECODED",
                            "Welcome to the Signal Lost terminal system.\n\nThis system is designed for tracking and decoding radio signals in the field.\n\nAccess granted. User identified as: Dr. Sarah Chen, Research Division.",
                            asciiArt,
                            true,
                            0.0f
                        );
                        break;
                }
            }
        }
        
        // Take a screenshot when F12 is pressed
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F12)
            {
                if (_screenshotTaker != null)
                {
                    _screenshotTaker.TakeScreenshot("enhanced_message_display_test");
                    GD.Print("Screenshot taken");
                }
            }
        }
    }
}

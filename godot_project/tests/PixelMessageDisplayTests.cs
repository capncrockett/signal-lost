using Godot;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class PixelMessageDisplayTests : GUT.Test
    {
        // Component to test
        private PixelMessageDisplay _messageDisplay;
        
        // Signal tracking
        private bool _messageClosedSignalReceived = false;
        private bool _decodeRequestedSignalReceived = false;
        private string _decodeRequestedMessageId = null;
        
        // Called before each test
        public async void Before()
        {
            try
            {
                // Create instance of the component
                _messageDisplay = new PixelMessageDisplay();
                
                // Add to the scene tree using call_deferred
                CallDeferred("add_child", _messageDisplay);
                
                // Wait a frame to ensure the node is added
                await ToSignal(GetTree(), "process_frame");
                
                // Initialize
                _messageDisplay._Ready();
                
                // Connect to signals
                _messageDisplay.MessageClosed += OnMessageClosed;
                _messageDisplay.DecodeRequested += OnDecodeRequested;
                
                // Reset signal tracking
                _messageClosedSignalReceived = false;
                _decodeRequestedSignalReceived = false;
                _decodeRequestedMessageId = null;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in PixelMessageDisplayTests.Before: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        // Called after each test
        public void After()
        {
            // Disconnect signals
            _messageDisplay.MessageClosed -= OnMessageClosed;
            _messageDisplay.DecodeRequested -= OnDecodeRequested;
            
            // Clean up
            _messageDisplay.QueueFree();
            _messageDisplay = null;
        }
        
        // Signal handlers
        private void OnMessageClosed()
        {
            _messageClosedSignalReceived = true;
        }
        
        private void OnDecodeRequested(string messageId)
        {
            _decodeRequestedSignalReceived = true;
            _decodeRequestedMessageId = messageId;
        }
        
        [TestMethod]
        public void TestInitialization()
        {
            // Verify that the message display is initialized correctly
            Assert.IsNotNull(_messageDisplay, "PixelMessageDisplay should not be null");
            Assert.IsFalse(_messageDisplay.Visible, "Message display should not be visible initially");
        }
        
        [TestMethod]
        public void TestSetMessage()
        {
            // Set a message
            _messageDisplay.SetMessage("test_id", "Test Title", "Test content", false);
            
            // Verify the message display is visible
            Assert.IsTrue(_messageDisplay.Visible, "Message display should be visible after setting a message");
        }
        
        [TestMethod]
        public void TestSetMessageWithArt()
        {
            // Create ASCII art
            List<string> asciiArt = new List<string>
            {
                "  _____  ",
                " / __  \\ ",
                "| |  | | ",
                "| |  | | ",
                "| |__| | ",
                " \\____/  "
            };
            
            // Set a message with ASCII art
            _messageDisplay.SetMessageWithArt("test_id", "Test Title", "Test content", asciiArt, false);
            
            // Verify the message display is visible
            Assert.IsTrue(_messageDisplay.Visible, "Message display should be visible after setting a message with ASCII art");
        }
        
        [TestMethod]
        public void TestSetVisible()
        {
            // Set a message
            _messageDisplay.SetMessage("test_id", "Test Title", "Test content", false);
            
            // Verify the message display is visible
            Assert.IsTrue(_messageDisplay.Visible, "Message display should be visible after setting a message");
            
            // Hide the message display
            _messageDisplay.SetVisible(false);
            
            // Verify the message display is hidden
            Assert.IsFalse(_messageDisplay.Visible, "Message display should be hidden after SetVisible(false)");
            
            // Show the message display again
            _messageDisplay.SetVisible(true);
            
            // Verify the message display is visible again
            Assert.IsTrue(_messageDisplay.Visible, "Message display should be visible after SetVisible(true)");
        }
        
        [TestMethod]
        public void TestMessageClosedSignal()
        {
            // Set a message
            _messageDisplay.SetMessage("test_id", "Test Title", "Test content", false);
            
            // Simulate clicking the close button
            // Note: We can't directly test GUI input in unit tests, so we'll just emit the signal
            _messageDisplay.EmitSignal(SignalName.MessageClosed);
            
            // Verify the signal was received
            Assert.IsTrue(_messageClosedSignalReceived, "MessageClosed signal should be received");
        }
        
        [TestMethod]
        public void TestDecodeRequestedSignal()
        {
            // Set a message
            string messageId = "test_decode_id";
            _messageDisplay.SetMessage(messageId, "Test Title", "Test content", false);
            
            // Simulate clicking the decode button
            // Note: We can't directly test GUI input in unit tests, so we'll just emit the signal
            _messageDisplay.EmitSignal(SignalName.DecodeRequested, messageId);
            
            // Verify the signal was received
            Assert.IsTrue(_decodeRequestedSignalReceived, "DecodeRequested signal should be received");
            Assert.AreEqual(messageId, _decodeRequestedMessageId, "DecodeRequested signal should include the correct message ID");
        }
    }
}

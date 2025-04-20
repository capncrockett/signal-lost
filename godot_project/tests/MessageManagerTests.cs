using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class MessageManagerTests : GUT.Test
    {
        // Components to test
        private MessageManager _messageManager;
        private GameState _gameState;
        
        // Called before each test
        public async void Before()
        {
            try
            {
                // Create instances of the components
                _gameState = new GameState();
                _messageManager = new MessageManager();
                
                // Add them to the scene tree using call_deferred
                CallDeferred("add_child", _gameState);
                CallDeferred("add_child", _messageManager);
                
                // Wait a frame to ensure all nodes are added
                await ToSignal(GetTree(), "process_frame");
                
                // Initialize them
                _gameState._Ready();
                _messageManager._Ready();
                
                // Set up test data
                _gameState.AddMessage("test_message", new RadioMessage
                {
                    Id = "test_message",
                    Title = "Test Message",
                    Content = "This is a test message content.",
                    Frequency = 90.0f,
                    Decoded = false
                });
                
                _gameState.AddMessage("decoded_message", new RadioMessage
                {
                    Id = "decoded_message",
                    Title = "Decoded Message",
                    Content = "This message is already decoded.",
                    Frequency = 91.5f,
                    Decoded = true
                });
                
                // Add a test signal
                _gameState.AddSignal(new RadioSignal
                {
                    Id = "test_signal",
                    Frequency = 90.0f,
                    Message = "Test signal message",
                    Type = RadioSignalType.Morse,
                    Strength = 0.8f
                });
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in MessageManagerTests.Before: {ex.Message}");
                GD.PrintErr(ex.StackTrace);
                throw; // Re-throw to fail the test
            }
        }
        
        // Called after each test
        public void After()
        {
            // Clean up
            _messageManager.QueueFree();
            _gameState.QueueFree();
            
            _messageManager = null;
            _gameState = null;
        }
        
        [TestMethod]
        public void TestInitialization()
        {
            // Verify that the message manager is initialized correctly
            Assert.IsNotNull(_messageManager, "MessageManager should not be null");
            Assert.IsFalse(_messageManager.IsMessageVisible(), "No message should be visible initially");
        }
        
        [TestMethod]
        public void TestShowMessage()
        {
            // Show a message
            _messageManager.ShowMessage("test_message");
            
            // Verify the message is visible
            Assert.IsTrue(_messageManager.IsMessageVisible(), "Message should be visible after ShowMessage");
        }
        
        [TestMethod]
        public void TestHideMessage()
        {
            // Show a message
            _messageManager.ShowMessage("test_message");
            
            // Hide the message
            _messageManager.HideMessage();
            
            // Verify the message is hidden
            Assert.IsFalse(_messageManager.IsMessageVisible(), "Message should be hidden after HideMessage");
        }
        
        [TestMethod]
        public void TestShowNonExistentMessage()
        {
            // Try to show a non-existent message
            _messageManager.ShowMessage("non_existent_message");
            
            // Verify no message is shown
            Assert.IsFalse(_messageManager.IsMessageVisible(), "No message should be visible when showing non-existent message");
        }
        
        [TestMethod]
        public void TestMessageInterference()
        {
            // Set the current frequency to match the signal
            _gameState.SetFrequency(90.0f);
            
            // Show a message
            _messageManager.ShowMessage("test_message");
            
            // Verify the message is visible
            Assert.IsTrue(_messageManager.IsMessageVisible(), "Message should be visible");
            
            // Change to a frequency with no signal
            _gameState.SetFrequency(95.0f);
            
            // Show the message again
            _messageManager.ShowMessage("test_message");
            
            // Verify the message is still visible but with interference
            Assert.IsTrue(_messageManager.IsMessageVisible(), "Message should be visible even with interference");
        }
    }
}

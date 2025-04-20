using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// Base class for all tests in the Signal Lost game.
    /// Provides common functionality and utilities for testing.
    /// </summary>
    [TestClass]
    public abstract partial class TestBase : GUT.Test
    {
        // Flag to indicate if the test is running on a Mac
        protected static readonly bool IsMacOS = OperatingSystem.IsMacOS();
        
        // Flag to indicate if the test is running on Windows
        protected static readonly bool IsWindows = OperatingSystem.IsWindows();
        
        /// <summary>
        /// Called before each test. Override in derived classes.
        /// </summary>
        public virtual void Before()
        {
            // Base implementation does nothing
        }
        
        /// <summary>
        /// Called after each test. Override in derived classes.
        /// </summary>
        public virtual void After()
        {
            // Base implementation does nothing
        }
        
        /// <summary>
        /// Safely adds a child node using call_deferred to avoid threading issues.
        /// </summary>
        /// <param name="node">The node to add as a child</param>
        protected void SafeAddChild(Node node)
        {
            if (node == null)
            {
                GD.PrintErr("Attempted to add null node as child");
                return;
            }
            
            CallDeferred("add_child", node);
        }
        
        /// <summary>
        /// Safely removes a child node using call_deferred to avoid threading issues.
        /// </summary>
        /// <param name="node">The node to remove</param>
        protected void SafeRemoveChild(Node node)
        {
            if (node == null)
            {
                GD.PrintErr("Attempted to remove null node");
                return;
            }
            
            if (!IsInstanceValid(node))
            {
                GD.PrintErr("Attempted to remove invalid node");
                return;
            }
            
            if (node.GetParent() != this)
            {
                GD.PrintErr("Attempted to remove node that is not a child of this node");
                return;
            }
            
            CallDeferred("remove_child", node);
        }
        
        /// <summary>
        /// Safely frees a node using queue_free to avoid threading issues.
        /// </summary>
        /// <param name="node">The node to free</param>
        protected void SafeFreeNode(Node node)
        {
            if (node == null)
            {
                GD.PrintErr("Attempted to free null node");
                return;
            }
            
            if (!IsInstanceValid(node))
            {
                GD.PrintErr("Attempted to free invalid node");
                return;
            }
            
            node.QueueFree();
        }
        
        /// <summary>
        /// Waits for a signal to be emitted.
        /// </summary>
        /// <param name="source">The source object that will emit the signal</param>
        /// <param name="signalName">The name of the signal to wait for</param>
        /// <returns>A GodotObject representing the signal</returns>
        protected async Godot.GodotObject WaitForSignal(Godot.GodotObject source, string signalName)
        {
            if (source == null)
            {
                GD.PrintErr($"Cannot wait for signal '{signalName}' from null source");
                return null;
            }
            
            try
            {
                return await ToSignal(source, signalName);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error waiting for signal '{signalName}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Waits for the next frame to be processed.
        /// </summary>
        protected async void WaitForNextFrame()
        {
            await ToSignal(GetTree(), "process_frame");
        }
        
        /// <summary>
        /// Skips the test if running on Mac OS.
        /// </summary>
        /// <param name="message">Optional message explaining why the test is skipped on Mac</param>
        protected void SkipOnMac(string message = "Test skipped on Mac platform")
        {
            if (IsMacOS)
            {
                GD.Print($"Skipping test on Mac: {message}");
                Assert.IsTrue(true, message);
                throw new SkipTestException(message);
            }
        }
        
        /// <summary>
        /// Skips the test if running on Windows.
        /// </summary>
        /// <param name="message">Optional message explaining why the test is skipped on Windows</param>
        protected void SkipOnWindows(string message = "Test skipped on Windows platform")
        {
            if (IsWindows)
            {
                GD.Print($"Skipping test on Windows: {message}");
                Assert.IsTrue(true, message);
                throw new SkipTestException(message);
            }
        }
        
        /// <summary>
        /// Skips the test with the given message.
        /// </summary>
        /// <param name="message">Message explaining why the test is skipped</param>
        protected void SkipTest(string message)
        {
            GD.Print($"Skipping test: {message}");
            Assert.IsTrue(true, message);
            throw new SkipTestException(message);
        }
        
        /// <summary>
        /// Logs a message to the console with a timestamp.
        /// </summary>
        /// <param name="message">The message to log</param>
        protected void LogMessage(string message)
        {
            GD.Print($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }
        
        /// <summary>
        /// Logs an error message to the console with a timestamp.
        /// </summary>
        /// <param name="message">The error message to log</param>
        protected void LogError(string message)
        {
            GD.PrintErr($"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {message}");
        }
    }
    
    /// <summary>
    /// Exception used to skip a test.
    /// </summary>
    public class SkipTestException : Exception
    {
        public SkipTestException(string message) : base(message) { }
    }
}

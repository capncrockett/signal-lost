using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalLost
{
    /// <summary>
    /// Test script for demonstrating memory leak detection.
    /// </summary>
    [GlobalClass]
    public partial class MemoryLeakTest : Control
    {
        // UI references
        private Button _createObjectsButton;
        private Button _createLeakButton;
        private Button _createResourcesButton;
        private Button _cleanupButton;
        private Button _toggleUIButton;
        private Label _statusLabel;
        private MemoryLeakDetectorUI _memoryUI;
        
        // References to other components
        private MemoryProfiler _memoryProfiler;
        private ResourceTracker _resourceTracker;
        private DisposableResourceManager _disposableManager;
        
        // Test objects
        private List<object> _managedObjects = new List<object>();
        private List<Resource> _managedResources = new List<Resource>();
        
        // Leak simulation
        private static List<object> _leakedObjects = new List<object>();
        private Timer _leakTimer;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get UI references
            _createObjectsButton = GetNode<Button>("%CreateObjectsButton");
            _createLeakButton = GetNode<Button>("%CreateLeakButton");
            _createResourcesButton = GetNode<Button>("%CreateResourcesButton");
            _cleanupButton = GetNode<Button>("%CleanupButton");
            _toggleUIButton = GetNode<Button>("%ToggleUIButton");
            _statusLabel = GetNode<Label>("%StatusLabel");
            _memoryUI = GetNode<MemoryLeakDetectorUI>("%MemoryLeakDetectorUI");
            
            // Get references to other components
            _memoryProfiler = GetNode<MemoryProfiler>("/root/MemoryProfiler");
            _resourceTracker = GetNode<ResourceTracker>("/root/ResourceTracker");
            _disposableManager = DisposableResourceManager.Instance;
            
            // Connect signals
            _createObjectsButton.Pressed += OnCreateObjectsButtonPressed;
            _createLeakButton.Pressed += OnCreateLeakButtonPressed;
            _createResourcesButton.Pressed += OnCreateResourcesButtonPressed;
            _cleanupButton.Pressed += OnCleanupButtonPressed;
            _toggleUIButton.Pressed += OnToggleUIButtonPressed;
            
            // Create leak timer
            _leakTimer = new Timer();
            _leakTimer.WaitTime = 0.5;
            _leakTimer.OneShot = false;
            _leakTimer.Timeout += OnLeakTimerTimeout;
            AddChild(_leakTimer);
            
            // Update status
            UpdateStatus("Ready");
        }

        // Called when the create objects button is pressed
        private void OnCreateObjectsButtonPressed()
        {
            UpdateStatus("Creating objects...");
            
            // Create a bunch of managed objects
            for (int i = 0; i < 100; i++)
            {
                var obj = new TestObject($"Test_{i}");
                _managedObjects.Add(obj);
                
                // Track with memory profiler
                if (_memoryProfiler != null)
                {
                    _memoryProfiler.TrackObject(obj, "TestObject");
                }
            }
            
            UpdateStatus($"Created {_managedObjects.Count} managed objects");
        }

        // Called when the create leak button is pressed
        private void OnCreateLeakButtonPressed()
        {
            if (_leakTimer.IsStopped())
            {
                UpdateStatus("Starting memory leak simulation...");
                _leakTimer.Start();
            }
            else
            {
                UpdateStatus("Stopping memory leak simulation...");
                _leakTimer.Stop();
            }
        }

        // Called when the create resources button is pressed
        private void OnCreateResourcesButtonPressed()
        {
            UpdateStatus("Creating resources...");
            
            // Create a bunch of resources
            for (int i = 0; i < 10; i++)
            {
                // Create image
                var image = new Image();
                image.Create(64, 64, false, Image.Format.Rgba8);
                
                // Create texture from image
                var texture = ImageTexture.CreateFromImage(image);
                
                // Track resources
                if (_resourceTracker != null)
                {
                    _resourceTracker.TrackResource(texture, $"TestTexture_{i}");
                }
                
                _managedResources.Add(texture);
            }
            
            UpdateStatus($"Created {_managedResources.Count} resources");
        }

        // Called when the cleanup button is pressed
        private void OnCleanupButtonPressed()
        {
            UpdateStatus("Cleaning up...");
            
            // Clear managed objects
            _managedObjects.Clear();
            
            // Clear managed resources
            _managedResources.Clear();
            
            // Force garbage collection
            if (_memoryProfiler != null)
            {
                _memoryProfiler.ForceGarbageCollection();
            }
            else
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            
            UpdateStatus("Cleanup complete");
        }

        // Called when the toggle UI button is pressed
        private void OnToggleUIButtonPressed()
        {
            _memoryUI.ToggleVisibility();
            UpdateStatus(_memoryUI.Visible ? "Memory UI visible" : "Memory UI hidden");
        }

        // Called when the leak timer times out
        private void OnLeakTimerTimeout()
        {
            // Create objects that will never be garbage collected (simulating a leak)
            for (int i = 0; i < 10; i++)
            {
                var obj = new LeakyObject($"Leak_{_leakedObjects.Count}");
                _leakedObjects.Add(obj);
                
                // Track with memory profiler
                if (_memoryProfiler != null)
                {
                    _memoryProfiler.TrackObject(obj, "LeakyObject");
                }
            }
            
            UpdateStatus($"Leaking objects... ({_leakedObjects.Count} total)");
        }

        // Updates the status label
        private void UpdateStatus(string status)
        {
            _statusLabel.Text = $"Status: {status}";
        }

        // Test object class
        private class TestObject
        {
            public string Name { get; private set; }
            public byte[] Data { get; private set; }
            
            public TestObject(string name)
            {
                Name = name;
                Data = new byte[1024]; // 1KB of data
            }
        }

        // Leaky object class (static reference prevents garbage collection)
        private class LeakyObject
        {
            public string Name { get; private set; }
            public byte[] Data { get; private set; }
            
            public LeakyObject(string name)
            {
                Name = name;
                Data = new byte[1024 * 10]; // 10KB of data
            }
        }
    }
}

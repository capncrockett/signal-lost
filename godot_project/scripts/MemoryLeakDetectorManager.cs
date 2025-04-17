using Godot;
using System;

namespace SignalLost
{
    /// <summary>
    /// Manages the memory leak detector UI.
    /// This script should be attached to the main scene.
    /// </summary>
    [GlobalClass]
    public partial class MemoryLeakDetectorManager : Node
    {
        // Configuration
        [Export]
        public bool EnableMemoryLeakDetection { get; set; } = true;

        [Export]
        public bool ShowUIOnStartup { get; set; } = false;

        // References
        private MemoryLeakDetectorUI _memoryUI;
        private PackedScene _memoryUIScene;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            if (!EnableMemoryLeakDetection)
                return;

            GD.Print("MemoryLeakDetectorManager: Initializing...");
            
            // Load the memory UI scene
            _memoryUIScene = ResourceLoader.Load<PackedScene>("res://scenes/MemoryLeakDetectorUI.tscn");
            
            if (_memoryUIScene == null)
            {
                GD.PrintErr("MemoryLeakDetectorManager: Failed to load MemoryLeakDetectorUI.tscn");
                return;
            }
            
            // Create the UI
            _memoryUI = _memoryUIScene.Instantiate<MemoryLeakDetectorUI>();
            _memoryUI.ShowOnStartup = ShowUIOnStartup;
            
            // Add to the scene
            GetTree().Root.AddChild(_memoryUI);
            
            GD.Print("MemoryLeakDetectorManager: Memory leak detector UI initialized");
        }

        // Called when input is received
        public override void _Input(InputEvent @event)
        {
            if (!EnableMemoryLeakDetection || _memoryUI == null)
                return;

            // Toggle memory UI with F8 key
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F8)
            {
                _memoryUI.ToggleVisibility();
            }
        }

        // Called when the node is about to be removed from the scene
        public override void _ExitTree()
        {
            if (_memoryUI != null)
            {
                _memoryUI.QueueFree();
                _memoryUI = null;
            }
        }
    }
}

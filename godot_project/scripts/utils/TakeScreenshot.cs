using Godot;
using System;
using System.IO;

namespace SignalLost.Utils
{
    public partial class TakeScreenshot : GodotObject
    {
        public static void Main()
        {
            GD.Print("Starting screenshot capture...");
            
            // Create a new instance of the game scene
            var mainScene = ResourceLoader.Load<PackedScene>("res://PixelMainScene_Enhanced.tscn");
            if (mainScene == null)
            {
                GD.PrintErr("Failed to load main scene. Make sure the path is correct.");
                return;
            }
            
            var mainInstance = mainScene.Instantiate();
            
            // Add the scene to the root
            var root = Engine.GetMainLoop() as SceneTree;
            if (root == null)
            {
                GD.PrintErr("Failed to get SceneTree.");
                return;
            }
            
            root.Root.AddChild(mainInstance);
            
            // Wait a few frames for the scene to initialize
            for (int i = 0; i < 5; i++)
            {
                // Process a frame manually
                root.Process(0.016f); // Simulate a frame at 60fps
            }
            
            // Take the screenshot
            var screenshotTaker = new ScreenshotTaker();
            string screenshotPath = screenshotTaker.TakeTimestampedScreenshot("game_screenshot");
            
            GD.Print($"Screenshot saved to: {screenshotPath}");
            
            // Clean up
            mainInstance.QueueFree();
            
            // Exit the application
            root.Quit();
        }
    }
}

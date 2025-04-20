using Godot;
using System;
using System.IO;
using System.Threading;

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
            
            // Wait a moment for the scene to initialize
            Thread.Sleep(500); // Wait 500ms
            
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

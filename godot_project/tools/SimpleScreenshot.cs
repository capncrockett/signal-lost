using Godot;
using System;
using SignalLost.Utils;

/// <summary>
/// Simple command-line tool for taking screenshots.
/// Usage: godot --script tools/SimpleScreenshot.cs [filename]
/// </summary>
public partial class SimpleScreenshot : SceneTree
{
    public override void _Initialize()
    {
        GD.Print("Simple Screenshot tool initializing...");
        
        // Get command line arguments
        string[] args = OS.GetCmdlineArgs();
        string filename = "screenshot";
        
        // Check if a filename was provided
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--script" && i + 2 < args.Length)
            {
                filename = args[i + 2];
                break;
            }
        }
        
        // Create a timestamp
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fullFilename = $"{filename}_{timestamp}.png";
        
        GD.Print($"Taking screenshot with filename: {fullFilename}");
        
        // Create the screenshot manager
        var screenshotManager = new ScreenshotManager();
        
        // Add it to the scene
        var root = GetRoot();
        root.AddChild(screenshotManager);
        
        // Wait a moment for the scene to initialize
        System.Threading.Thread.Sleep(500);
        
        // Take the screenshot
        string path = screenshotManager.TakeScreenshot(fullFilename);
        
        GD.Print($"Screenshot saved to: {path}");
        
        // Exit
        Quit(0);
    }
}

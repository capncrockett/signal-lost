using Godot;
using SignalLost.Utils;

/// <summary>
/// Command-line tool for taking screenshots.
/// Usage: godot --headless --script tools/TakeScreenshot.cs [filename]
/// </summary>
public partial class TakeScreenshot : SceneTree
{
    public override void _Initialize()
    {
        GD.Print("Screenshot tool initializing...");
        
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
        
        // Create a screenshot taker
        var screenshotTaker = new ScreenshotTaker();
        GetRoot().AddChild(screenshotTaker);
        
        // Take a screenshot
        string path = screenshotTaker.TakeTimestampedScreenshot(filename);
        
        if (!string.IsNullOrEmpty(path))
        {
            GD.Print($"Screenshot saved to: {path}");
        }
        else
        {
            GD.PrintErr("Failed to take screenshot.");
        }
        
        // Exit
        Quit(0);
    }
}

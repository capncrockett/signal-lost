using Godot;
using System;
using System.IO;
using Environment = System.Environment;

namespace SignalLost
{
    [GlobalClass]
    public partial class ScreenshotTaker : Node
    {
        [Export]
        public bool AutoTakeScreenshot { get; set; } = true;

        [Export]
        public float AutoScreenshotDelay { get; set; } = 3.0f;

        [Export]
        public string ScreenshotDirectory { get; set; } = "screenshots";

        private bool _screenshotTaken = false;
        private float _timer = 0;

        public override void _Ready()
        {
            GD.Print("ScreenshotTaker ready!");

            // Create screenshots directory if it doesn't exist
            EnsureScreenshotDirectoryExists();
        }

        public override void _Process(double delta)
        {
            if (AutoTakeScreenshot)
            {
                // Wait a few seconds to let the UI fully load
                _timer += (float)delta;

                if (!_screenshotTaken && _timer > AutoScreenshotDelay)
                {
                    TakeScreenshot();
                    _screenshotTaken = true;
                }
            }
        }

        // Take a screenshot with a custom filename
        public void TakeScreenshot(string filename = "")
        {
            GD.Print("Taking screenshot...");

            // Get the viewport image
            var image = GetViewport().GetTexture().GetImage();

            // Generate a filename if none provided
            if (string.IsNullOrEmpty(filename))
            {
                filename = $"screenshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            }

            // Determine the save path based on platform
            string savePath = GetScreenshotPath(filename);

            // Save the screenshot
            Error err = image.SavePng(savePath);

            if (err == Error.Ok)
            {
                GD.Print($"Screenshot saved to: {savePath}");
            }
            else
            {
                GD.PrintErr($"Failed to save screenshot: {err}");
            }
        }

        // Get the appropriate screenshot path based on platform
        private string GetScreenshotPath(string filename)
        {
            string basePath;

            if (OS.GetName() == "macOS")
            {
                // On Mac, save to the user's Pictures folder
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                basePath = Path.Combine(homeDir, "Pictures", ScreenshotDirectory);

                // Create directory if it doesn't exist
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                return Path.Combine(basePath, filename);
            }
            else if (OS.GetName() == "Windows")
            {
                // On Windows, save to the user's Pictures folder
                string picturesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                basePath = Path.Combine(picturesDir, ScreenshotDirectory);

                // Create directory if it doesn't exist
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }

                return Path.Combine(basePath, filename);
            }
            else
            {
                // For other platforms, use the user:// directory
                string userDir = OS.GetUserDataDir();
                return Path.Combine(userDir, filename);
            }
        }

        // Ensure the screenshot directory exists
        private void EnsureScreenshotDirectoryExists()
        {
            if (OS.GetName() == "macOS")
            {
                string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string dirPath = Path.Combine(homeDir, "Pictures", ScreenshotDirectory);

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }
            else if (OS.GetName() == "Windows")
            {
                string picturesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                string dirPath = Path.Combine(picturesDir, ScreenshotDirectory);

                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
            }
        }

        // Take a screenshot when F12 is pressed
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F12)
            {
                TakeScreenshot();
            }
        }
    }
}

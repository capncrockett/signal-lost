using Godot;
using System;
using System.IO;
using System.Diagnostics;
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

        [Export]
        public bool CleanupAfterScreenshot { get; set; } = true;

        [Signal]
        public delegate void ScreenshotTakenEventHandler(string path);

        private bool _screenshotTaken = false;
        private float _timer = 0;
        private DisposableResourceManager _resourceManager;
        private MemoryProfiler _memoryProfiler;

        public override void _Ready()
        {
            GD.Print("ScreenshotTaker ready!");

            // Create screenshots directory if it doesn't exist
            EnsureScreenshotDirectoryExists();

            // Get references to other components
            _resourceManager = DisposableResourceManager.Instance;
            _memoryProfiler = GetNode<MemoryProfiler>("/root/MemoryProfiler");
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

            // Track the image with memory profiler if available
            if (_memoryProfiler != null)
            {
                _memoryProfiler.TrackObject(image, "Screenshot");
            }

            // Generate a filename if none provided
            if (string.IsNullOrEmpty(filename))
            {
                filename = $"screenshot_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
            }
            else if (!filename.EndsWith(".png"))
            {
                filename = $"{filename}.png";
            }

            // Determine the save path based on platform
            string savePath = GetScreenshotPath(filename);

            // Save the screenshot
            Error err = image.SavePng(savePath);

            if (err == Error.Ok)
            {
                GD.Print($"Screenshot saved to: {savePath}");

                // Emit signal
                EmitSignal(SignalName.ScreenshotTaken, savePath);
            }
            else
            {
                GD.PrintErr($"Failed to save screenshot: {err}");
            }

            // Clean up resources
            if (CleanupAfterScreenshot)
            {
                CleanupScreenshotResources(image);
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

        // Clean up resources after taking a screenshot
        private void CleanupScreenshotResources(Image image)
        {
            // Force garbage collection
            if (_memoryProfiler != null)
            {
                _memoryProfiler.UntrackObject(image, "Screenshot");
                _memoryProfiler.ForceGarbageCollection();
            }
            else
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            // On macOS, we need to kill the Preview process that might have been launched
            if (OS.GetName() == "macOS")
            {
                KillPreviewProcess();
            }
        }

        // Kill the Preview process on macOS
        private void KillPreviewProcess()
        {
            try
            {
                if (OS.GetName() == "macOS")
                {
                    // Check if Preview is running
                    Process[] previewProcesses = Process.GetProcessesByName("Preview");

                    if (previewProcesses.Length > 0)
                    {
                        GD.Print("ScreenshotTaker: Preview process detected, not killing to avoid disrupting user.");
                        // We don't actually kill Preview as it might disrupt the user
                        // This is just a check to be aware of potential resource usage
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"ScreenshotTaker: Error checking Preview process: {ex.Message}");
            }
        }
    }
}

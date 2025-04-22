using Godot;
using System;
using System.IO;
using System.Diagnostics;

namespace SignalLost.Utils
{
    /// <summary>
    /// Manages taking and saving screenshots from within Godot.
    /// Can also be used to capture screenshots using external tools.
    /// </summary>
    public partial class ScreenshotManager : Node
    {
        [Export]
        public string ScreenshotDirectory { get; set; } = "screenshots";

        [Export]
        public bool UseExternalTool { get; set; } = false;

        [Export]
        public string ExternalToolPath { get; set; } = "capture_screen.bat";

        private static ScreenshotManager _instance;
        public static ScreenshotManager Instance => _instance;

        public override void _Ready()
        {
            if (_instance != null)
            {
                QueueFree();
                return;
            }

            _instance = this;
            
            // Make this node persistent so it doesn't get destroyed when changing scenes
            ProcessMode = ProcessModeEnum.Always;
            
            // Create the screenshot directory if it doesn't exist
            EnsureDirectoryExists();
        }

        /// <summary>
        /// Takes a screenshot and saves it to the specified directory.
        /// </summary>
        /// <param name="filename">Optional filename. If not provided, a timestamp will be used.</param>
        /// <returns>The full path to the saved screenshot.</returns>
        public string TakeScreenshot(string filename = null)
        {
            if (UseExternalTool)
            {
                return TakeScreenshotWithExternalTool(filename);
            }
            else
            {
                return TakeScreenshotWithGodot(filename);
            }
        }

        /// <summary>
        /// Takes a screenshot using Godot's built-in functionality.
        /// </summary>
        /// <param name="filename">Optional filename. If not provided, a timestamp will be used.</param>
        /// <returns>The full path to the saved screenshot.</returns>
        private string TakeScreenshotWithGodot(string filename = null)
        {
            // Generate a filename if none is provided
            if (string.IsNullOrEmpty(filename))
            {
                filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            }
            
            // Ensure the filename has a .png extension
            if (!filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".png";
            }
            
            // Get the full path
            string projectDir = ProjectSettings.GlobalizePath("res://").GetBaseDir();
            string fullDir = Path.Combine(projectDir, ScreenshotDirectory);
            string fullPath = Path.Combine(fullDir, filename);
            
            // Take the screenshot
            var image = GetViewport().GetTexture().GetImage();
            Error error = image.SavePng(fullPath);
            
            if (error != Error.Ok)
            {
                GD.PrintErr($"Failed to save screenshot: {error}");
                return null;
            }
            
            GD.Print($"Screenshot saved to: {fullPath}");
            return fullPath;
        }

        /// <summary>
        /// Takes a screenshot using an external tool.
        /// </summary>
        /// <param name="filename">Optional filename. If not provided, a timestamp will be used.</param>
        /// <returns>The full path to the saved screenshot.</returns>
        private string TakeScreenshotWithExternalTool(string filename = null)
        {
            // Generate a filename if none is provided
            if (string.IsNullOrEmpty(filename))
            {
                filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            }
            
            // Ensure the filename has a .png extension
            if (!filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".png";
            }
            
            // Get the full path
            string projectDir = ProjectSettings.GlobalizePath("res://").GetBaseDir();
            string fullDir = Path.Combine(projectDir, ScreenshotDirectory);
            string fullPath = Path.Combine(fullDir, filename);
            
            try
            {
                // Create the process start info
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ExternalToolPath,
                    Arguments = $"\"{fullPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                // Start the process
                using (Process process = Process.Start(startInfo))
                {
                    // Wait for the process to exit
                    process.WaitForExit();
                    
                    // Check if the screenshot was created
                    if (File.Exists(fullPath))
                    {
                        GD.Print($"Screenshot saved to: {fullPath}");
                        return fullPath;
                    }
                    else
                    {
                        GD.PrintErr($"Failed to save screenshot using external tool. Output: {process.StandardOutput.ReadToEnd()}");
                        GD.PrintErr($"Error: {process.StandardError.ReadToEnd()}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error taking screenshot with external tool: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ensures the screenshot directory exists.
        /// </summary>
        private void EnsureDirectoryExists()
        {
            try
            {
                string projectDir = ProjectSettings.GlobalizePath("res://").GetBaseDir();
                string fullDir = Path.Combine(projectDir, ScreenshotDirectory);
                
                if (!Directory.Exists(fullDir))
                {
                    Directory.CreateDirectory(fullDir);
                    GD.Print($"Created screenshot directory: {fullDir}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error creating screenshot directory: {ex.Message}");
            }
        }
    }
}

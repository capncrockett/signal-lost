using Godot;
using System;
using System.IO;
using Environment = System.Environment;

namespace SignalLost.Utils
{
    /// <summary>
    /// Utility class for taking screenshots with cross-platform support.
    /// </summary>
    [GlobalClass]
    public partial class ScreenshotTaker : Node
    {
        /// <summary>
        /// Whether to automatically clean up resources after taking a screenshot.
        /// </summary>
        [Export]
        public bool CleanupAfterScreenshot { get; set; } = true;

        /// <summary>
        /// The default directory name for screenshots.
        /// </summary>
        [Export]
        public string ScreenshotDirectoryName { get; set; } = "Screenshots";

        /// <summary>
        /// Takes a screenshot and saves it to the platform-specific screenshots directory.
        /// </summary>
        /// <param name="filename">The filename for the screenshot (without extension).</param>
        /// <returns>The full path to the saved screenshot.</returns>
        public string TakeScreenshot(string filename)
        {
            // Ensure filename has .png extension
            if (!filename.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                filename = $"{filename}.png";
            }

            // Get platform-specific directory
            string directory = GetScreenshotDirectory();
            string fullPath = Path.Combine(directory, filename);

            // Ensure directory exists
            if (!Directory.Exists(directory))
            {
                try
                {
                    Directory.CreateDirectory(directory);
                    GD.Print($"Created screenshot directory: {directory}");
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to create screenshot directory: {ex.Message}");
                    return string.Empty;
                }
            }

            try
            {
                // Take screenshot
                var viewport = GetViewport();
                var image = viewport.GetTexture().GetImage();
                Error error = image.SavePng(fullPath);

                if (error != Error.Ok)
                {
                    GD.PrintErr($"Failed to save screenshot: {error}");
                    return string.Empty;
                }

                GD.Print($"Screenshot saved to: {fullPath}");

                // Clean up resources if needed
                if (CleanupAfterScreenshot)
                {
                    image.Dispose();
                }

                return fullPath;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error taking screenshot: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the directory for saving screenshots, using the user:// directory for cross-platform compatibility.
        /// </summary>
        /// <returns>The full path to the screenshots directory.</returns>
        private string GetScreenshotDirectory()
        {
            // Use user:// directory for cross-platform compatibility
            string userDir = OS.GetUserDataDir();
            string basePath = Path.Combine(userDir, ScreenshotDirectoryName);

            return basePath;
        }

        /// <summary>
        /// Takes a screenshot with a timestamp in the filename.
        /// </summary>
        /// <param name="prefix">Optional prefix for the filename.</param>
        /// <returns>The full path to the saved screenshot.</returns>
        public string TakeTimestampedScreenshot(string prefix = "screenshot")
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"{prefix}_{timestamp}.png";
            return TakeScreenshot(filename);
        }
    }
}

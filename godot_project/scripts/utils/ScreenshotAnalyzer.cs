using Godot;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SignalLost.Utils
{
    /// <summary>
    /// Analyzes screenshots and provides textual descriptions that can be understood by Augment.
    /// </summary>
    public partial class ScreenshotAnalyzer : Node
    {
        [Export]
        public string ScreenshotDirectory { get; set; } = "screenshots";
        
        private static ScreenshotAnalyzer _instance;
        public static ScreenshotAnalyzer Instance => _instance;
        
        public override void _Ready()
        {
            if (_instance != null)
            {
                QueueFree();
                return;
            }
            
            _instance = this;
            
            // Make this node persistent
            ProcessMode = ProcessModeEnum.Always;
        }
        
        /// <summary>
        /// Analyzes a screenshot and returns a textual description.
        /// </summary>
        /// <param name="screenshotPath">Path to the screenshot file</param>
        /// <returns>A textual description of the screenshot</returns>
        public string AnalyzeScreenshot(string screenshotPath)
        {
            try
            {
                // Load the image
                var image = Image.LoadFromFile(screenshotPath);
                if (image == null)
                {
                    GD.PrintErr($"Failed to load image: {screenshotPath}");
                    return $"Failed to load image: {screenshotPath}";
                }
                
                // Get basic image information
                int width = image.GetWidth();
                int height = image.GetHeight();
                
                // Analyze colors
                var colorAnalysis = AnalyzeColors(image);
                
                // Generate a description
                StringBuilder description = new StringBuilder();
                description.AppendLine($"Screenshot Analysis: {Path.GetFileName(screenshotPath)}");
                description.AppendLine($"Resolution: {width}x{height}");
                description.AppendLine($"Average Color: R={colorAnalysis.AverageColor.R}, G={colorAnalysis.AverageColor.G}, B={colorAnalysis.AverageColor.B}");
                description.AppendLine($"Brightness: {colorAnalysis.Brightness:F2}");
                description.AppendLine($"Is Dark Screen: {colorAnalysis.IsDark}");
                description.AppendLine($"Has Red Elements: {colorAnalysis.HasRed}");
                description.AppendLine($"Has Green Elements: {colorAnalysis.HasGreen}");
                description.AppendLine($"Has Blue Elements: {colorAnalysis.HasBlue}");
                
                // Save the analysis to a file
                string analysisPath = Path.ChangeExtension(screenshotPath, ".analysis.txt");
                File.WriteAllText(analysisPath, description.ToString());
                
                GD.Print($"Analysis saved to: {analysisPath}");
                
                return description.ToString();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error analyzing screenshot: {ex.Message}");
                return $"Error analyzing screenshot: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Analyzes the colors in an image.
        /// </summary>
        private (Color AverageColor, float Brightness, bool IsDark, bool HasRed, bool HasGreen, bool HasBlue) AnalyzeColors(Image image)
        {
            // Resize the image to speed up processing
            image.Resize(100, 100, Image.Interpolation.Bilinear);
            var resizedImage = image;
            
            // Calculate average color
            int r = 0, g = 0, b = 0;
            int pixelCount = 0;
            
            for (int y = 0; y < resizedImage.GetHeight(); y++)
            {
                for (int x = 0; x < resizedImage.GetWidth(); x++)
                {
                    Color color = resizedImage.GetPixel(x, y);
                    r += (int)(color.R * 255);
                    g += (int)(color.G * 255);
                    b += (int)(color.B * 255);
                    pixelCount++;
                }
            }
            
            r /= pixelCount;
            g /= pixelCount;
            b /= pixelCount;
            
            Color averageColor = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
            
            // Calculate brightness
            float brightness = (r + g + b) / (3.0f * 255.0f);
            
            // Determine if the screen is dark
            bool isDark = brightness < 0.5f;
            
            // Check for dominant colors
            bool hasRed = r > 150 && r > g + 50 && r > b + 50;
            bool hasGreen = g > 150 && g > r + 50 && g > b + 50;
            bool hasBlue = b > 150 && b > r + 50 && b > g + 50;
            
            return (averageColor, brightness, isDark, hasRed, hasGreen, hasBlue);
        }
        
        /// <summary>
        /// Command-line entry point for analyzing screenshots.
        /// </summary>
        public static void Main()
        {
            GD.Print("Screenshot Analyzer starting...");
            
            // Get command line arguments
            string[] args = OS.GetCmdlineArgs();
            string screenshotPath = null;
            
            // Parse command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--screenshot" && i + 1 < args.Length)
                {
                    screenshotPath = args[i + 1];
                }
            }
            
            // Check if a screenshot path was provided
            if (string.IsNullOrEmpty(screenshotPath))
            {
                GD.PrintErr("No screenshot path provided. Use --screenshot <path> to specify a screenshot.");
                return;
            }
            
            // Create a new scene with the analyzer
            var scene = new Node();
            var analyzer = new ScreenshotAnalyzer();
            scene.AddChild(analyzer);
            
            // Add the scene to the root
            var root = Engine.GetMainLoop() as SceneTree;
            if (root == null)
            {
                GD.PrintErr("Failed to get SceneTree.");
                return;
            }
            
            root.Root.AddChild(scene);
            
            // Wait a moment for the scene to initialize
            System.Threading.Thread.Sleep(500);
            
            // Analyze the screenshot
            string analysis = analyzer.AnalyzeScreenshot(screenshotPath);
            
            // Print the analysis
            GD.Print(analysis);
            
            // Exit after a short delay
            System.Threading.Thread.Sleep(500);
            root.Quit();
        }
    }
}

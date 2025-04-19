using Godot;
using System;

namespace SignalLost.Utils
{
    /// <summary>
    /// Utility class to register utility autoloads.
    /// This class is not meant to be instantiated directly.
    /// </summary>
    public static class UtilsAutoload
    {
        // Track if autoloads have been registered
        private static bool _registered = false;

        /// <summary>
        /// Registers all utility autoloads.
        /// Call this method from the main scene's _Ready method.
        /// </summary>
        public static void RegisterAutoloads()
        {
            // Only register once
            if (_registered)
            {
                return;
            }

            try
            {
                // Register ScreenshotTaker
                RegisterScreenshotTaker();

                _registered = true;
                GD.Print("Utility autoloads registered successfully.");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to register utility autoloads: {ex.Message}");
            }
        }

        /// <summary>
        /// Registers the ScreenshotTaker as an autoload singleton.
        /// </summary>
        private static void RegisterScreenshotTaker()
        {
            try
            {
                var screenshotTaker = new ScreenshotTaker();
                screenshotTaker.Name = "ScreenshotTaker";

                // Add to the scene tree using SceneTree.Root
                var sceneTree = Engine.GetMainLoop() as SceneTree;
                if (sceneTree != null)
                {
                    var root = sceneTree.Root;
                    root.AddChild(screenshotTaker);
                }
                else
                {
                    GD.PrintErr("Failed to get SceneTree");
                }

                GD.Print("ScreenshotTaker registered as autoload.");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to register ScreenshotTaker: {ex.Message}");
            }
        }
    }
}

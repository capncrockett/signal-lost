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
        /// <summary>
        /// Registers all utility autoloads.
        /// Call this method from the main scene's _Ready method.
        /// </summary>
        public static void RegisterAutoloads()
        {
            // Register ScreenshotTaker
            RegisterScreenshotTaker();
            
            GD.Print("Utility autoloads registered successfully.");
        }
        
        /// <summary>
        /// Registers the ScreenshotTaker as an autoload singleton.
        /// </summary>
        private static void RegisterScreenshotTaker()
        {
            var screenshotTaker = new ScreenshotTaker();
            screenshotTaker.Name = "ScreenshotTaker";
            
            // Add to the scene tree
            Engine.GetSingleton("SceneTree").Call("get_root").Call("add_child", screenshotTaker);
            
            GD.Print("ScreenshotTaker registered as autoload.");
        }
    }
}

using Godot;
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalLost.Utils;

namespace SignalLost.Tests
{
    /// <summary>
    /// Tests for the ScreenshotTaker class.
    /// </summary>
    [GlobalClass]
    public partial class ScreenshotTakerTests : GUT.Test
    {
        private ScreenshotTaker _screenshotTaker;
        private string _testScreenshotPath;

        /// <summary>
        /// Set up the test environment.
        /// </summary>
        public void Before()
        {
            // Create a new ScreenshotTaker instance
            _screenshotTaker = new ScreenshotTaker();
            _screenshotTaker.ScreenshotDirectoryName = "TestScreenshots";
            AddChild(_screenshotTaker);
        }

        /// <summary>
        /// Clean up after tests.
        /// </summary>
        public void After()
        {
            // Clean up any test screenshots
            if (!string.IsNullOrEmpty(_testScreenshotPath) && File.Exists(_testScreenshotPath))
            {
                try
                {
                    File.Delete(_testScreenshotPath);
                    GD.Print($"Deleted test screenshot: {_testScreenshotPath}");
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to delete test screenshot: {ex.Message}");
                }
            }

            // Remove the ScreenshotTaker
            if (_screenshotTaker != null)
            {
                _screenshotTaker.QueueFree();
                _screenshotTaker = null;
            }
        }

        /// <summary>
        /// Test that screenshots can be taken and saved.
        /// </summary>
        [TestMethod]
        public void TestTakeScreenshot()
        {
            // Take a screenshot
            string filename = "test_screenshot.png";
            _testScreenshotPath = _screenshotTaker.TakeScreenshot(filename);

            // Verify the screenshot was created
            Assert.IsFalse(string.IsNullOrEmpty(_testScreenshotPath), "Screenshot path should not be empty");
            Assert.IsTrue(File.Exists(_testScreenshotPath), "Screenshot file should exist");
        }

        /// <summary>
        /// Test that timestamped screenshots can be taken and saved.
        /// </summary>
        [TestMethod]
        public void TestTakeTimestampedScreenshot()
        {
            // Take a timestamped screenshot
            _testScreenshotPath = _screenshotTaker.TakeTimestampedScreenshot("test");

            // Verify the screenshot was created
            Assert.IsFalse(string.IsNullOrEmpty(_testScreenshotPath), "Screenshot path should not be empty");
            Assert.IsTrue(File.Exists(_testScreenshotPath), "Screenshot file should exist");
            
            // Verify the filename contains a timestamp
            string filename = Path.GetFileName(_testScreenshotPath);
            Assert.IsTrue(filename.StartsWith("test_"), "Filename should start with the prefix");
            Assert.IsTrue(filename.EndsWith(".png"), "Filename should end with .png");
            
            // The filename should be in the format: test_YYYYMMDD_HHMMSS.png
            Assert.AreEqual(24, filename.Length, "Filename should be in the format: test_YYYYMMDD_HHMMSS.png");
        }

        /// <summary>
        /// Test that the screenshot directory is created if it doesn't exist.
        /// </summary>
        [TestMethod]
        public void TestScreenshotDirectoryCreation()
        {
            // Set a unique directory name
            string uniqueDirName = $"TestScreenshots_{Guid.NewGuid().ToString().Substring(0, 8)}";
            _screenshotTaker.ScreenshotDirectoryName = uniqueDirName;

            // Take a screenshot
            string filename = "directory_test.png";
            _testScreenshotPath = _screenshotTaker.TakeScreenshot(filename);

            // Verify the screenshot was created
            Assert.IsFalse(string.IsNullOrEmpty(_testScreenshotPath), "Screenshot path should not be empty");
            Assert.IsTrue(File.Exists(_testScreenshotPath), "Screenshot file should exist");

            // Verify the directory was created
            string directory = Path.GetDirectoryName(_testScreenshotPath);
            Assert.IsTrue(Directory.Exists(directory), "Screenshot directory should exist");
            
            // Clean up the test directory
            try
            {
                Directory.Delete(directory, true);
                GD.Print($"Deleted test directory: {directory}");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to delete test directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Test that the screenshot path is platform-specific.
        /// </summary>
        [TestMethod]
        public void TestPlatformSpecificPath()
        {
            // Take a screenshot
            string filename = "platform_test.png";
            _testScreenshotPath = _screenshotTaker.TakeScreenshot(filename);

            // Verify the screenshot was created
            Assert.IsFalse(string.IsNullOrEmpty(_testScreenshotPath), "Screenshot path should not be empty");
            
            // Verify the path is platform-specific
            if (OS.GetName() == "macOS")
            {
                // Mac path should contain Documents
                Assert.IsTrue(_testScreenshotPath.Contains("Documents"), 
                    "macOS screenshot path should contain Documents directory");
            }
            else if (OS.GetName() == "Windows")
            {
                // Windows path should contain Pictures or My Pictures
                bool containsPictures = _testScreenshotPath.Contains("Pictures") || 
                                        _testScreenshotPath.Contains("My Pictures");
                Assert.IsTrue(containsPictures, 
                    "Windows screenshot path should contain Pictures directory");
            }
            else
            {
                // Other platforms should use user data directory
                string userDir = OS.GetUserDataDir();
                Assert.IsTrue(_testScreenshotPath.StartsWith(userDir), 
                    "Other platforms should use user data directory");
            }
        }
    }
}

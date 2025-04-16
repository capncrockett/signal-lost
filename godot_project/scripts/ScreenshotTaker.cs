using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class ScreenshotTaker : Node
    {
        private bool _screenshotTaken = false;

        public override void _Ready()
        {
            GD.Print("ScreenshotTaker ready!");
        }

        private float _timer = 0;

        public override void _Process(double delta)
        {
            // Wait a few seconds to let the UI fully load
            _timer += (float)delta;

            if (!_screenshotTaken && _timer > 3.0f)
            {
                TakeScreenshot();
                _screenshotTaken = true;
            }
        }

        private void TakeScreenshot()
        {
            GD.Print("Taking screenshot...");

            // Get the viewport image
            var image = GetViewport().GetTexture().GetImage();

            // Save the screenshot
            string path = "user://screenshot.png";
            Error err = image.SavePng(path);

            if (err == Error.Ok)
            {
                GD.Print($"Screenshot saved to: {OS.GetUserDataDir()}/screenshot.png");
            }
            else
            {
                GD.PrintErr($"Failed to save screenshot: {err}");
            }
        }
    }
}

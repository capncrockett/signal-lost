using Godot;
using System;

namespace SignalLost.UI
{
    /// <summary>
    /// Defines a consistent visual theme for the game's pixel-based UI.
    /// This theme system provides a centralized way to manage colors, dimensions, and visual effects.
    /// </summary>
    [GlobalClass]
    public partial class PixelTheme : Resource
    {
        // Color palette
        [Export] public Color PrimaryColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);      // Main UI color (green)
        [Export] public Color SecondaryColor { get; set; } = new Color(0.0f, 0.5f, 0.8f, 1.0f);    // Secondary UI color (blue)
        [Export] public Color AccentColor { get; set; } = new Color(0.8f, 0.8f, 0.0f, 1.0f);       // Accent color (yellow)
        [Export] public Color AlertColor { get; set; } = new Color(0.8f, 0.0f, 0.0f, 1.0f);        // Alert color (red)
        [Export] public Color BackgroundColor { get; set; } = new Color(0.05f, 0.05f, 0.05f, 1.0f); // Background color (dark gray)
        [Export] public Color BorderColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);       // Border color (gray)
        [Export] public Color TextColor { get; set; } = new Color(0.9f, 0.9f, 0.9f, 1.0f);         // Text color (white)
        [Export] public Color DisabledColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 1.0f);     // Disabled color (gray)

        // UI element colors
        [Export] public Color ButtonColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        [Export] public Color ButtonHighlightColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        [Export] public Color ButtonTextColor { get; set; } = new Color(0.0f, 0.8f, 0.0f, 1.0f);
        [Export] public Color PanelColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [Export] public Color SliderColor { get; set; } = new Color(0.0f, 0.6f, 0.0f, 1.0f);
        [Export] public Color ProgressBarColor { get; set; } = new Color(0.0f, 0.7f, 0.0f, 1.0f);
        [Export] public Color ProgressBarBgColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1.0f);

        // Visual effects
        [Export] public Color ScanlineColor { get; set; } = new Color(0.0f, 0.0f, 0.0f, 0.2f);
        [Export] public float NoiseIntensity { get; set; } = 0.05f;
        [Export] public bool EnableScanlines { get; set; } = true;
        [Export] public bool EnableScreenFlicker { get; set; } = true;
        [Export] public bool EnablePixelSnapping { get; set; } = true;

        // Dimensions
        [Export] public int BorderWidth { get; set; } = 1;
        [Export] public int CornerRadius { get; set; } = 0; // 0 for sharp corners, >0 for rounded
        [Export] public int CharacterSize { get; set; } = 8;
        [Export] public int LineSpacing { get; set; } = 2;
        [Export] public int ButtonPadding { get; set; } = 10;
        [Export] public int PanelPadding { get; set; } = 20;

        // Animation
        [Export] public float TypewriterSpeed { get; set; } = 0.05f;
        [Export] public float TypewriterSpeedVariation { get; set; } = 0.02f;
        [Export] public float CursorBlinkSpeed { get; set; } = 0.5f;
        [Export] public float TransitionSpeed { get; set; } = 0.3f;

        // Singleton instance
        private static PixelTheme _instance;

        /// <summary>
        /// Gets the singleton instance of the PixelTheme.
        /// </summary>
        public static PixelTheme Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to load from resources
                    _instance = ResourceLoader.Load<PixelTheme>("res://resources/pixel_theme.tres");

                    // If not found, create a new instance
                    if (_instance == null)
                    {
                        _instance = new PixelTheme();
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// Applies a color variation to simulate screen flicker.
        /// </summary>
        /// <param name="baseColor">The base color to modify.</param>
        /// <param name="intensity">The intensity of the flicker (0-1).</param>
        /// <returns>The modified color with flicker applied.</returns>
        public Color ApplyFlicker(Color baseColor, float intensity = 0.1f)
        {
            if (!EnableScreenFlicker)
                return baseColor;

            Random random = new Random();
            float flickerAmount = (float)random.NextDouble() * intensity;

            if (random.NextDouble() < 0.5)
                flickerAmount = -flickerAmount;

            return new Color(
                Mathf.Clamp(baseColor.R + flickerAmount, 0, 1),
                Mathf.Clamp(baseColor.G + flickerAmount, 0, 1),
                Mathf.Clamp(baseColor.B + flickerAmount, 0, 1),
                baseColor.A
            );
        }

        /// <summary>
        /// Draws scanlines on a Control node.
        /// </summary>
        /// <param name="control">The Control node to draw on.</param>
        /// <param name="offset">The vertical offset for the scanlines.</param>
        public void DrawScanlines(Control control, float offset = 0)
        {
            if (!EnableScanlines)
                return;

            int scanlineSpacing = 4; // Space between scanlines
            Vector2 size = control.Size;

            for (int y = (int)offset % scanlineSpacing; y < size.Y; y += scanlineSpacing)
            {
                control.DrawRect(new Rect2(0, y, size.X, 1), ScanlineColor);
            }
        }

        /// <summary>
        /// Draws a pixel-style button.
        /// </summary>
        /// <param name="control">The Control node to draw on.</param>
        /// <param name="rect">The rectangle defining the button area.</param>
        /// <param name="text">The button text.</param>
        /// <param name="isHighlighted">Whether the button is highlighted.</param>
        /// <param name="isDisabled">Whether the button is disabled.</param>
        public void DrawButton(Control control, Rect2 rect, string text, bool isHighlighted = false, bool isDisabled = false)
        {
            // Determine button colors based on state
            Color bgColor = isDisabled ? DisabledColor : (isHighlighted ? ButtonHighlightColor : ButtonColor);
            Color textCol = isDisabled ? DisabledColor : ButtonTextColor;

            // Draw button background
            control.DrawRect(rect, bgColor);

            // Draw button border
            control.DrawRect(rect, BorderColor, false, BorderWidth);

            // Draw button text
            control.DrawString(ThemeDB.FallbackFont,
                new Vector2(rect.Position.X + rect.Size.X / 2, rect.Position.Y + rect.Size.Y / 2 + 6),
                text, HorizontalAlignment.Center, -1, 14, textCol);
        }

        /// <summary>
        /// Draws a pixel-style panel.
        /// </summary>
        /// <param name="control">The Control node to draw on.</param>
        /// <param name="rect">The rectangle defining the panel area.</param>
        public void DrawPanel(Control control, Rect2 rect)
        {
            // Draw panel background
            control.DrawRect(rect, PanelColor);

            // Draw panel border
            control.DrawRect(rect, BorderColor, false, BorderWidth);
        }

        /// <summary>
        /// Draws a pixel-style progress bar.
        /// </summary>
        /// <param name="control">The Control node to draw on.</param>
        /// <param name="rect">The rectangle defining the progress bar area.</param>
        /// <param name="value">The current value (0-1).</param>
        /// <param name="vertical">Whether the progress bar is vertical.</param>
        public void DrawProgressBar(Control control, Rect2 rect, float value, bool vertical = false)
        {
            // Clamp value to 0-1 range
            value = Mathf.Clamp(value, 0, 1);

            // Draw background
            control.DrawRect(rect, ProgressBarBgColor);

            // Draw progress
            if (vertical)
            {
                float height = rect.Size.Y * value;
                control.DrawRect(new Rect2(
                    rect.Position.X,
                    rect.Position.Y + rect.Size.Y - height,
                    rect.Size.X,
                    height
                ), ProgressBarColor);
            }
            else
            {
                float width = rect.Size.X * value;
                control.DrawRect(new Rect2(
                    rect.Position.X,
                    rect.Position.Y,
                    width,
                    rect.Size.Y
                ), ProgressBarColor);
            }

            // Draw border
            control.DrawRect(rect, BorderColor, false, BorderWidth);
        }

        /// <summary>
        /// Draws a pixel-style slider.
        /// </summary>
        /// <param name="control">The Control node to draw on.</param>
        /// <param name="rect">The rectangle defining the slider area.</param>
        /// <param name="value">The current value (0-1).</param>
        /// <param name="vertical">Whether the slider is vertical.</param>
        public void DrawSlider(Control control, Rect2 rect, float value, bool vertical = false)
        {
            // Clamp value to 0-1 range
            value = Mathf.Clamp(value, 0, 1);

            // Draw background
            control.DrawRect(rect, ProgressBarBgColor);

            // Calculate handle position
            float handleSize = vertical ? rect.Size.X : rect.Size.Y;
            Vector2 handlePos;

            if (vertical)
            {
                float availableHeight = rect.Size.Y - handleSize;
                float yPos = rect.Position.Y + availableHeight * (1 - value);
                handlePos = new Vector2(rect.Position.X, yPos);
            }
            else
            {
                float availableWidth = rect.Size.X - handleSize;
                float xPos = rect.Position.X + availableWidth * value;
                handlePos = new Vector2(xPos, rect.Position.Y);
            }

            // Draw handle
            control.DrawRect(new Rect2(
                handlePos,
                new Vector2(vertical ? rect.Size.X : handleSize, vertical ? handleSize : rect.Size.Y)
            ), SliderColor);

            // Draw border
            control.DrawRect(rect, BorderColor, false, BorderWidth);
        }
    }
}

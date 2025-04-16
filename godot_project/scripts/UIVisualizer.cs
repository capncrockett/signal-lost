using Godot;
using System;
using System.Text;

namespace SignalLost
{
    [GlobalClass]
    public partial class UIVisualizer : Node
    {
        // Grid size for ASCII visualization
        private const int GRID_WIDTH = 80;
        private const int GRID_HEIGHT = 30;
        
        // Characters for visualization
        private const char EMPTY = ' ';
        private const char BORDER = '#';
        private const char BUTTON = 'B';
        private const char TEXT = 'T';
        private const char HIGHLIGHT = '*';
        
        // Create a text-based visualization of the UI
        public static void VisualizeUI(Control control)
        {
            GD.Print("=== UI VISUALIZATION ===");
            
            // Log basic information
            Vector2 size = control.Size;
            GD.Print($"Window Size: {size.X}x{size.Y}");
            
            // Create a grid for visualization
            char[,] grid = new char[GRID_HEIGHT, GRID_WIDTH];
            for (int y = 0; y < GRID_HEIGHT; y++)
                for (int x = 0; x < GRID_WIDTH; x++)
                    grid[y, x] = EMPTY;
            
            // If this is a PixelUITest, visualize its specific elements
            if (control is PixelUITest pixelUI)
            {
                VisualizePixelUI(pixelUI, grid, size);
            }
            
            // Print the grid
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            
            // Add top border
            for (int x = 0; x < GRID_WIDTH; x++)
                sb.Append('-');
            sb.AppendLine();
            
            // Add grid content with side borders
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                sb.Append('|');
                for (int x = 0; x < GRID_WIDTH; x++)
                    sb.Append(grid[y, x]);
                sb.AppendLine("|");
            }
            
            // Add bottom border
            for (int x = 0; x < GRID_WIDTH; x++)
                sb.Append('-');
            sb.AppendLine();
            
            GD.Print(sb.ToString());
            GD.Print("=== END VISUALIZATION ===");
        }
        
        // Visualize PixelUITest specific elements
        private static void VisualizePixelUI(PixelUITest pixelUI, char[,] grid, Vector2 windowSize)
        {
            // Get the UI elements through reflection (since they're private)
            bool showMessage = (bool)GetPrivateField(pixelUI, "_showMessage");
            Rect2 showMessageButtonRect = (Rect2)GetPrivateField(pixelUI, "_showMessageButtonRect");
            Rect2 messageRect = (Rect2)GetPrivateField(pixelUI, "_messageRect");
            Rect2 closeButtonRect = (Rect2)GetPrivateField(pixelUI, "_closeButtonRect");
            bool showButtonHovered = (bool)GetPrivateField(pixelUI, "_showButtonHovered");
            bool closeButtonHovered = (bool)GetPrivateField(pixelUI, "_closeButtonHovered");
            
            // Log the UI state
            GD.Print($"UI State: {(showMessage ? "Showing Message" : "Showing Button")}");
            GD.Print($"Show Button: Position({showMessageButtonRect.Position.X},{showMessageButtonRect.Position.Y}), Size({showMessageButtonRect.Size.X},{showMessageButtonRect.Size.Y}), Hovered: {showButtonHovered}");
            
            if (showMessage)
            {
                GD.Print($"Message Box: Position({messageRect.Position.X},{messageRect.Position.Y}), Size({messageRect.Size.X},{messageRect.Size.Y})");
                GD.Print($"Close Button: Position({closeButtonRect.Position.X},{closeButtonRect.Position.Y}), Size({closeButtonRect.Size.X},{closeButtonRect.Size.Y}), Hovered: {closeButtonHovered}");
            }
            
            // Map the UI elements to the grid
            if (showMessage)
            {
                // Draw message box
                DrawRectToGrid(grid, messageRect, windowSize, BORDER);
                
                // Draw close button
                DrawRectToGrid(grid, closeButtonRect, windowSize, closeButtonHovered ? HIGHLIGHT : BUTTON);
                
                // Add some text representation in the message box
                int textX = MapX(messageRect.Position.X + 20, windowSize.X);
                int textY = MapY(messageRect.Position.Y + 60, windowSize.Y);
                
                if (textY >= 0 && textY < GRID_HEIGHT && textX >= 0 && textX < GRID_WIDTH)
                {
                    grid[textY, textX] = TEXT;
                    
                    // Add some more text characters to represent content
                    for (int i = 1; i < 10 && textX + i < GRID_WIDTH; i++)
                    {
                        grid[textY, textX + i] = TEXT;
                    }
                    
                    // Add a second line of text
                    if (textY + 1 < GRID_HEIGHT)
                    {
                        for (int i = 0; i < 15 && textX + i < GRID_WIDTH; i++)
                        {
                            grid[textY + 1, textX + i] = TEXT;
                        }
                    }
                }
            }
            else
            {
                // Draw show message button
                DrawRectToGrid(grid, showMessageButtonRect, windowSize, showButtonHovered ? HIGHLIGHT : BUTTON);
            }
        }
        
        // Helper to draw a rectangle to the grid
        private static void DrawRectToGrid(char[,] grid, Rect2 rect, Vector2 windowSize, char character)
        {
            int startX = MapX(rect.Position.X, windowSize.X);
            int startY = MapY(rect.Position.Y, windowSize.Y);
            int endX = MapX(rect.Position.X + rect.Size.X, windowSize.X);
            int endY = MapY(rect.Position.Y + rect.Size.Y, windowSize.Y);
            
            // Clamp to grid bounds
            startX = Math.Max(0, Math.Min(startX, GRID_WIDTH - 1));
            startY = Math.Max(0, Math.Min(startY, GRID_HEIGHT - 1));
            endX = Math.Max(0, Math.Min(endX, GRID_WIDTH - 1));
            endY = Math.Max(0, Math.Min(endY, GRID_HEIGHT - 1));
            
            // Draw the rectangle outline
            for (int x = startX; x <= endX; x++)
            {
                if (startY >= 0 && startY < GRID_HEIGHT)
                    grid[startY, x] = character;
                if (endY >= 0 && endY < GRID_HEIGHT && endY != startY)
                    grid[endY, x] = character;
            }
            
            for (int y = startY + 1; y < endY; y++)
            {
                if (startX >= 0 && startX < GRID_WIDTH)
                    grid[y, startX] = character;
                if (endX >= 0 && endX < GRID_WIDTH && endX != startX)
                    grid[y, endX] = character;
            }
        }
        
        // Map X coordinate from window space to grid space
        private static int MapX(float x, float windowWidth)
        {
            return (int)(x / windowWidth * GRID_WIDTH);
        }
        
        // Map Y coordinate from window space to grid space
        private static int MapY(float y, float windowHeight)
        {
            return (int)(y / windowHeight * GRID_HEIGHT);
        }
        
        // Helper to get private field value using reflection
        private static object GetPrivateField(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (field != null)
            {
                return field.GetValue(instance);
            }
            
            return null;
        }
    }
}

using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class SimpleTest : Control
    {
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("SimpleTest scene loaded successfully!");
        }
        
        // Process function called every frame
        public override void _Process(double delta)
        {
            // Draw a simple pixel pattern
            QueueRedraw();
        }
        
        // Custom drawing function
        public override void _Draw()
        {
            // Draw a simple pixel pattern
            for (int x = 0; x < 10; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    Color color = new Color(
                        (float)x / 10.0f,
                        (float)y / 10.0f,
                        0.5f,
                        1.0f
                    );
                    
                    DrawRect(new Rect2(100 + x * 20, 100 + y * 20, 18, 18), color);
                }
            }
        }
    }
}

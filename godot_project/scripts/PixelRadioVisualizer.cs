using Godot;
using System;
using System.Text;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelRadioVisualizer : Node
    {
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("PixelRadioVisualizer ready!");
            
            // Find the PixelRadioInterface
            var radioInterface = GetParent().GetNode<PixelRadioInterface>("RadioTuner/PixelRadioInterface");
            
            if (radioInterface != null)
            {
                // Visualize the radio interface
                UIVisualizer.VisualizeUI(radioInterface);
            }
            else
            {
                GD.PrintErr("PixelRadioVisualizer: Could not find PixelRadioInterface!");
            }
        }
        
        // Called every frame
        public override void _Process(double delta)
        {
            // Periodically visualize the radio interface
            if (Time.GetTicksMsec() % 5000 < 100) // Every 5 seconds
            {
                var radioInterface = GetParent().GetNode<PixelRadioInterface>("RadioTuner/PixelRadioInterface");
                
                if (radioInterface != null)
                {
                    UIVisualizer.VisualizeUI(radioInterface);
                }
            }
        }
    }
}

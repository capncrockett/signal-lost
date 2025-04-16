using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class EnhancedRadioVisualizer : Node
    {
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("EnhancedRadioVisualizer ready!");
            
            // Find the EnhancedRadioInterface
            var radioInterface = GetParent().GetNode<EnhancedRadioInterface>("RadioTuner/EnhancedRadioInterface");
            
            if (radioInterface != null)
            {
                // Visualize the radio interface
                UIVisualizer.VisualizeUI(radioInterface);
            }
            else
            {
                GD.PrintErr("EnhancedRadioVisualizer: Could not find EnhancedRadioInterface!");
            }
        }
        
        // Called every frame
        public override void _Process(double delta)
        {
            // Periodically visualize the radio interface
            if (Time.GetTicksMsec() % 5000 < 100) // Every 5 seconds
            {
                var radioInterface = GetParent().GetNode<EnhancedRadioInterface>("RadioTuner/EnhancedRadioInterface");
                
                if (radioInterface != null)
                {
                    UIVisualizer.VisualizeUI(radioInterface);
                }
            }
        }
    }
}

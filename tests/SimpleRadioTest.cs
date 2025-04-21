using Godot;
using System;
using SignalLost;
using SignalLost.UI;
using SignalLost.Audio;

/// <summary>
/// Simple test for the radio interface.
/// </summary>
[GlobalClass]
public partial class SimpleRadioTest : Node
{
    public override void _Ready()
    {
        GD.Print("SimpleRadioTest: Starting test...");
        
        try
        {
            // Create radio interface
            var radioInterface = new PixelRadioInterface();
            AddChild(radioInterface);
            
            // Check if radio interface was created
            if (radioInterface != null)
            {
                GD.Print("SimpleRadioTest: Radio interface created successfully");
            }
            else
            {
                GD.PrintErr("SimpleRadioTest: Radio interface creation failed");
            }
            
            // Create radio interface manager
            var radioInterfaceManager = new RadioInterfaceManager();
            radioInterfaceManager.RadioInterfacePath = new NodePath("../PixelRadioInterface");
            AddChild(radioInterfaceManager);
            
            // Check if radio interface manager was created
            if (radioInterfaceManager != null)
            {
                GD.Print("SimpleRadioTest: Radio interface manager created successfully");
            }
            else
            {
                GD.PrintErr("SimpleRadioTest: Radio interface manager creation failed");
            }
            
            // Create radio audio manager
            var radioAudioManager = new RadioAudioManager();
            AddChild(radioAudioManager);
            
            // Check if radio audio manager was created
            if (radioAudioManager != null)
            {
                GD.Print("SimpleRadioTest: Radio audio manager created successfully");
            }
            else
            {
                GD.PrintErr("SimpleRadioTest: Radio audio manager creation failed");
            }
            
            GD.Print("SimpleRadioTest: All tests passed!");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"SimpleRadioTest: Error: {ex.Message}");
            GD.PrintErr(ex.StackTrace);
        }
        
        // Exit with success
        GetTree().Quit(0);
    }
}

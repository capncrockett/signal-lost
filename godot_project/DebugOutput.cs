using Godot;
using System;

namespace SignalLost
{
    public partial class DebugOutput : SceneTree
    {
        public override void _Initialize()
        {
            GD.Print("C# Debug output script running...");

            // Print some system information
            GD.Print("OS: " + OS.GetName());
            GD.Print("Engine version: " + Engine.GetVersionInfo());

            // Try to access the audio system
            GD.Print("Audio drivers: " + string.Join(", ", AudioServer.GetOutputDeviceList()));
            GD.Print("Current audio driver: " + AudioServer.GetOutputDevice());

            // Print any errors
            GD.Print("Last error check");

            // Print warning about audio output device
            GD.Print("WARNING: Current output_device invalidated, taking output_device");

            // Exit after printing
            Quit(0);
        }
    }
}

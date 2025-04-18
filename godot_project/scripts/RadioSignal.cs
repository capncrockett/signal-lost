using Godot;
using System;

namespace SignalLost
{
    public enum RadioSignalType
    {
        Voice,
        Morse,
        Data,
        Beacon
    }

    public class RadioSignal
    {
        public string Id { get; set; }
        public float Frequency { get; set; }
        public string Message { get; set; }
        public RadioSignalType Type { get; set; }
        public float Strength { get; set; } = 1.0f;
        public bool IsActive { get; set; } = true;
    }
}

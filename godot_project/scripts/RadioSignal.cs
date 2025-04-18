using Godot;
using System;

namespace SignalLost
{
    public enum RadioSignalType
    {
        Morse,
        Voice,
        Data,
        Noise
    }

    public class RadioSignal
    {
        public string Id { get; set; }
        public float Frequency { get; set; }
        public string Message { get; set; }
        public RadioSignalType Type { get; set; }
        public float Strength { get; set; } = 1.0f;
        public bool IsDiscovered { get; set; } = false;
        public bool IsDecoded { get; set; } = false;
    }
}

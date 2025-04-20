using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SignalLost
{
    /// <summary>
    /// Represents the data that is saved to disk.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Basic game state
        public float CurrentFrequency { get; set; } = 90.0f;
        public bool IsRadioOn { get; set; } = false;
        public List<float> DiscoveredFrequencies { get; set; } = new List<float>();
        public string CurrentLocation { get; set; } = "bunker";
        public List<string> Inventory { get; set; } = new List<string>();
        public int GameProgress { get; set; } = 0;
        
        // Messages
        public Dictionary<string, MessageInfo> Messages { get; set; } = new Dictionary<string, MessageInfo>();
        
        // Field exploration state
        public Vector2I PlayerPosition { get; set; } = new Vector2I(1, 1);
        public Vector2I PlayerFacingDirection { get; set; } = new Vector2I(0, 1);
        public List<SignalSourceInfo> SignalSources { get; set; } = new List<SignalSourceInfo>();
        
        // Metadata
        public string Timestamp { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        public string GameVersion { get; set; } = "1.0.0";
        
        /// <summary>
        /// Represents a message in the save data.
        /// </summary>
        [Serializable]
        public class MessageInfo
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public bool Decoded { get; set; }
        }
        
        /// <summary>
        /// Represents a signal source in the save data.
        /// </summary>
        [Serializable]
        public class SignalSourceInfo
        {
            public Vector2I Position { get; set; }
            public float Frequency { get; set; }
            public string MessageId { get; set; }
            public float SignalStrength { get; set; }
            public float SignalRange { get; set; }
        }
    }
}

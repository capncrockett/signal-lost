using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost.Radio
{
    /// <summary>
    /// Enhanced signal data with more detailed information and content types.
    /// </summary>
    [GlobalClass]
    public partial class EnhancedSignalData : Resource
    {
        // Basic signal properties
        [Export] public string Id { get; set; } = "";
        [Export] public string Name { get; set; } = "";
        [Export] public string Description { get; set; } = "";
        [Export] public float Frequency { get; set; } = 90.0f;
        [Export] public SignalType Type { get; set; } = SignalType.Voice;
        [Export] public string Content { get; set; } = "";
        [Export] public string LocationId { get; set; } = "";
        [Export] public bool IsStatic { get; set; } = false;
        [Export] public float Bandwidth { get; set; } = 0.3f;
        [Export] public float MinSignalStrength { get; set; } = 0.5f;
        [Export] public bool IsHidden { get; set; } = false;
        [Export] public bool IsDecoded { get; set; } = false;
        [Export] public string RequiredItemToUnlock { get; set; } = "";

        // Related signal IDs (not exported due to Godot C# limitations with List<string>)
        public List<string> RelatedSignalIds { get; set; } = new List<string>();
        [Export] public string NextSignalId { get; set; } = "";
        [Export] public int StoryProgressRequired { get; set; } = 0;
        [Export] public int StoryProgressUnlocked { get; set; } = 0;
        [Export] public string AudioPath { get; set; } = "";
        [Export] public string ImagePath { get; set; } = "";

        // Narrative properties
        [Export] public string NarrativeThreadId { get; set; } = ""; // ID of the narrative thread this signal belongs to
        [Export] public int NarrativeSequence { get; set; } = 0; // Order in the narrative thread (0 = not part of a sequence)
        [Export] public string NarrativeTrigger { get; set; } = ""; // Event to trigger when this signal is decoded
        [Export] public bool IsKeyNarrativeSignal { get; set; } = false; // Whether this signal is key to the main story
        [Export] public string CharacterId { get; set; } = ""; // ID of the character associated with this signal
        [Export] public string SignalTimestampStr { get; set; } = ""; // Exported string representation of timestamp
        public DateTime SignalTimestamp { get => string.IsNullOrEmpty(SignalTimestampStr) ? DateTime.MinValue : DateTime.Parse(SignalTimestampStr); set => SignalTimestampStr = value.ToString("yyyy-MM-dd HH:mm:ss"); } // In-game timestamp of when the signal was sent

        // Enhanced content properties
        [Export] public string EncodedContent { get; set; } = ""; // Content before decoding
        [Export] public string DecodedContent { get; set; } = ""; // Content after decoding
        [Export] public string AsciiArt { get; set; } = ""; // ASCII art to display with the signal
        [Export] public float InterferenceLevel { get; set; } = 0.0f; // Level of interference (0.0-1.0)
        [Export] public bool HasVisualComponent { get; set; } = false; // Whether the signal has a visual component

        // Signal strength calculation
        public float CalculateSignalStrength(float currentFrequency)
        {
            float distance = Mathf.Abs(currentFrequency - Frequency);

            // If we're outside the bandwidth, no signal
            if (distance > Bandwidth)
                return 0.0f;

            // Calculate strength based on proximity to exact frequency
            return 1.0f - (distance / Bandwidth);
        }

        // Check if the signal can be detected based on strength and requirements
        public bool CanBeDetected(float signalStrength, int currentProgress, List<string> inventory)
        {
            // Check signal strength
            if (signalStrength < MinSignalStrength)
                return false;

            // Check story progress requirement
            if (currentProgress < StoryProgressRequired)
                return false;

            // Check if required item is in inventory
            if (!string.IsNullOrEmpty(RequiredItemToUnlock) && !inventory.Contains(RequiredItemToUnlock))
                return false;

            return true;
        }

        // Get formatted content based on signal type and strength
        public string GetFormattedContent(float signalStrength)
        {
            if (!IsDecoded)
            {
                // Return encoded content if available, otherwise a generic message
                return !string.IsNullOrEmpty(EncodedContent) ? EncodedContent : "[ENCRYPTED SIGNAL]";
            }

            if (signalStrength < MinSignalStrength)
                return "[SIGNAL TOO WEAK]";

            // Use decoded content if available, otherwise fall back to content
            string formattedContent = !string.IsNullOrEmpty(DecodedContent) ? DecodedContent : Content;

            // Apply interference effect based on signal strength and interference level
            float effectiveInterference = InterferenceLevel + (1.0f - signalStrength) * 0.5f;
            if (effectiveInterference > 0.1f)
            {
                formattedContent = ApplyStaticEffect(formattedContent, 1.0f - effectiveInterference);
            }

            // Format based on signal type
            switch (Type)
            {
                case SignalType.Morse:
                    formattedContent = FormatMorseCode(formattedContent);
                    break;

                case SignalType.Data:
                    formattedContent = FormatDataSignal(formattedContent);
                    break;

                case SignalType.Voice:
                    formattedContent = FormatVoiceSignal(formattedContent);
                    break;
            }

            // Add timestamp if available
            if (SignalTimestamp != DateTime.MinValue)
            {
                formattedContent = $"[{SignalTimestamp:yyyy-MM-dd HH:mm:ss}]\n{formattedContent}";
            }

            // Add character attribution if available
            if (!string.IsNullOrEmpty(CharacterId))
            {
                formattedContent = $"[From: {CharacterId}]\n{formattedContent}";
            }

            return formattedContent;
        }

        // Apply static effect to text based on signal strength
        private string ApplyStaticEffect(string text, float signalStrength)
        {
            // If signal is strong, return the text as is
            if (signalStrength > 0.8f)
                return text;

            // Calculate how much static to add
            float staticAmount = 1.0f - signalStrength;
            int staticChars = (int)(text.Length * staticAmount * 0.5f);

            // Create a copy of the text
            char[] chars = text.ToCharArray();

            // Replace random characters with static
            System.Random random = new System.Random();
            for (int i = 0; i < staticChars; i++)
            {
                int index = random.Next(chars.Length);
                chars[index] = '*';
            }

            return new string(chars);
        }

        // Format Morse code
        private string FormatMorseCode(string morseCode)
        {
            // Add spacing and formatting to Morse code
            return "[color=#00FFFF]" + morseCode.Replace("/", "/\n") + "[/color]";
        }

        // Format data signal
        private string FormatDataSignal(string data)
        {
            // Add formatting to data signal
            return "[color=#00FF00][font=monospace]" + data + "[/font][/color]";
        }

        // Format voice signal
        private string FormatVoiceSignal(string voiceContent)
        {
            // Add formatting to voice signal (quotation marks and styling)
            return "[color=#FFFFFF][i]\"" + voiceContent + "\"[/i][/color]";
        }
    }

    /// <summary>
    /// Types of radio signals.
    /// </summary>
    public enum SignalType
    {
        Voice,
        Morse,
        Data
    }
}

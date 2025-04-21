using Godot;
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
                return "[ENCRYPTED SIGNAL]";

            if (signalStrength < MinSignalStrength)
                return "[SIGNAL TOO WEAK]";

            string formattedContent = Content;

            // Apply static effect based on signal strength
            if (signalStrength < 0.8f)
            {
                formattedContent = ApplyStaticEffect(formattedContent, signalStrength);
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

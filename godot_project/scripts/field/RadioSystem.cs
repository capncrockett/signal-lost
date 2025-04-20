using Godot;
using System.Collections.Generic;

namespace SignalLost.Field
{
    /// <summary>
    /// Stub implementation of the RadioSystem class for the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class RadioSystem : Node
    {
        // Signal sources
        private Dictionary<float, SignalSourceData> _signalSources = new Dictionary<float, SignalSourceData>();

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Initialize
        }

        /// <summary>
        /// Registers a signal source with the radio system.
        /// </summary>
        /// <param name="frequency">The frequency of the signal</param>
        /// <param name="strength">The strength of the signal</param>
        /// <param name="messageId">The ID of the message associated with the signal</param>
        /// <param name="source">The signal source object</param>
        public void RegisterSignalSource(float frequency, float strength, string messageId, SignalSourceObject source)
        {
            _signalSources[frequency] = new SignalSourceData
            {
                Frequency = frequency,
                Strength = strength,
                MessageId = messageId,
                Source = source
            };

            GD.Print($"RadioSystem: Registered signal source at {frequency} MHz");
        }

        /// <summary>
        /// Updates the strength of a signal source.
        /// </summary>
        /// <param name="frequency">The frequency of the signal</param>
        /// <param name="strength">The new strength of the signal</param>
        public void UpdateSignalSourceStrength(float frequency, float strength)
        {
            if (_signalSources.ContainsKey(frequency))
            {
                _signalSources[frequency].Strength = strength;
            }
        }

        /// <summary>
        /// Gets the strength of a signal at the specified frequency.
        /// </summary>
        /// <param name="frequency">The frequency to check</param>
        /// <returns>The signal strength (0.0 to 1.0), or 0.0 if no signal is found</returns>
        public float GetSignalStrength(float frequency)
        {
            if (_signalSources.ContainsKey(frequency))
            {
                return _signalSources[frequency].Strength;
            }
            return 0.0f;
        }

        /// <summary>
        /// Gets the message ID associated with a signal at the specified frequency.
        /// </summary>
        /// <param name="frequency">The frequency to check</param>
        /// <returns>The message ID, or an empty string if no signal is found</returns>
        public string GetSignalMessageId(float frequency)
        {
            if (_signalSources.ContainsKey(frequency))
            {
                return _signalSources[frequency].MessageId;
            }
            return "";
        }
    }

    /// <summary>
    /// Data for a signal source.
    /// </summary>
    public class SignalSourceData
    {
        public float Frequency { get; set; }
        public float Strength { get; set; }
        public string MessageId { get; set; }
        public SignalSourceObject Source { get; set; }
    }
}

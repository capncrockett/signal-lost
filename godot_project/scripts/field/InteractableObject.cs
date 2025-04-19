using Godot;
using System;

namespace SignalLost.Field
{
    /// <summary>
    /// Base class for all interactable objects in the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class InteractableObject : Node2D
    {
        // Interactable properties
        [Export]
        public string ObjectId { get; set; } = "";

        [Export]
        public string DisplayName { get; set; } = "Object";

        [Export]
        public string Description { get; set; } = "An interactable object.";

        [Export]
        public bool IsOneTimeInteraction { get; set; } = false;

        // State
        protected bool _hasBeenInteracted = false;

        // Signals
        [Signal]
        public delegate void InteractionCompletedEventHandler(string objectId);

        /// <summary>
        /// Called when the player interacts with this object.
        /// </summary>
        /// <returns>True if interaction was successful, false otherwise</returns>
        public virtual bool Interact()
        {
            if (IsOneTimeInteraction && _hasBeenInteracted)
            {
                // Already interacted with this object
                return false;
            }

            // Mark as interacted
            _hasBeenInteracted = true;

            // Emit signal
            EmitSignal(SignalName.InteractionCompleted, ObjectId);

            return true;
        }

        /// <summary>
        /// Gets the interaction prompt text for this object.
        /// </summary>
        /// <returns>The interaction prompt text</returns>
        public virtual string GetInteractionPrompt()
        {
            return $"Press E to interact with {DisplayName}";
        }

        /// <summary>
        /// Resets the interaction state of this object.
        /// </summary>
        public virtual void ResetInteraction()
        {
            _hasBeenInteracted = false;
        }
    }
}

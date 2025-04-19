using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost.Field
{
    /// <summary>
    /// Represents a single cell in the grid-based field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class Cell : Node
    {
        // Cell position in the grid
        public Vector2I Position { get; private set; }

        // Cell properties
        public bool IsBlocked { get; set; } = false;
        public bool HasInteractable { get; set; } = false;
        public float InterferenceMultiplier { get; set; } = 1.0f;
        
        // Reference to interactable object in this cell
        private InteractableObject _interactableObject;

        /// <summary>
        /// Creates a new cell at the specified position.
        /// </summary>
        /// <param name="x">X coordinate in the grid</param>
        /// <param name="y">Y coordinate in the grid</param>
        public Cell(int x, int y)
        {
            Position = new Vector2I(x, y);
        }

        /// <summary>
        /// Sets an interactable object in this cell.
        /// </summary>
        /// <param name="interactable">The interactable object to place in this cell</param>
        public void SetInteractable(InteractableObject interactable)
        {
            _interactableObject = interactable;
            HasInteractable = interactable != null;
        }

        /// <summary>
        /// Gets the interactable object in this cell.
        /// </summary>
        /// <returns>The interactable object, or null if none exists</returns>
        public InteractableObject GetInteractable()
        {
            return _interactableObject;
        }

        /// <summary>
        /// Interacts with the object in this cell.
        /// </summary>
        /// <returns>True if interaction was successful, false otherwise</returns>
        public bool Interact()
        {
            if (HasInteractable && _interactableObject != null)
            {
                return _interactableObject.Interact();
            }
            return false;
        }
    }
}

using Godot;

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
        
        /// <summary>
        /// Creates a new cell at the specified position.
        /// </summary>
        /// <param name="x">X coordinate in the grid</param>
        /// <param name="y">Y coordinate in the grid</param>
        public Cell(int x, int y)
        {
            Position = new Vector2I(x, y);
        }
    }
}

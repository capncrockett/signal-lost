using Godot;
using System;

namespace SignalLost.Field
{
    /// <summary>
    /// Manages the grid-based world for the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class GridSystem : Node
    {
        // Grid dimensions
        [Export]
        private int _width = 20;

        [Export]
        private int _height = 15;

        [Export]
        private int _cellSize = 32; // Size of each cell in pixels

        // Cell data
        private Cell[,] _grid;

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            Initialize(_width, _height);
        }

        /// <summary>
        /// Initializes the grid with the specified dimensions.
        /// </summary>
        /// <param name="width">Width of the grid in cells</param>
        /// <param name="height">Height of the grid in cells</param>
        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;
            _grid = new Cell[width, height];
            
            // Initialize cells
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _grid[x, y] = new Cell(x, y);
                }
            }
            
            GD.Print($"GridSystem: Initialized grid with dimensions {width}x{height}");
        }

        /// <summary>
        /// Checks if a position is valid (within bounds and not blocked).
        /// </summary>
        /// <param name="position">The position to check</param>
        /// <returns>True if the position is valid, false otherwise</returns>
        public bool IsValidPosition(Vector2I position)
        {
            return position.X >= 0 && position.X < _width &&
                   position.Y >= 0 && position.Y < _height &&
                   !_grid[position.X, position.Y].IsBlocked;
        }

        /// <summary>
        /// Gets the cell at the specified position.
        /// </summary>
        /// <param name="position">The position to get the cell from</param>
        /// <returns>The cell at the position, or null if the position is invalid</returns>
        public Cell GetCell(Vector2I position)
        {
            if (position.X >= 0 && position.X < _width &&
                position.Y >= 0 && position.Y < _height)
            {
                return _grid[position.X, position.Y];
            }
            return null;
        }

        /// <summary>
        /// Sets whether a cell is blocked.
        /// </summary>
        /// <param name="position">The position of the cell</param>
        /// <param name="isBlocked">Whether the cell is blocked</param>
        public void SetCellBlocked(Vector2I position, bool isBlocked)
        {
            if (position.X >= 0 && position.X < _width &&
                position.Y >= 0 && position.Y < _height)
            {
                _grid[position.X, position.Y].IsBlocked = isBlocked;
            }
        }

        /// <summary>
        /// Converts a grid position to a world position.
        /// </summary>
        /// <param name="gridPosition">The grid position</param>
        /// <returns>The corresponding world position</returns>
        public Vector2 GridToWorldPosition(Vector2I gridPosition)
        {
            return new Vector2(gridPosition.X * _cellSize, gridPosition.Y * _cellSize);
        }

        /// <summary>
        /// Converts a world position to a grid position.
        /// </summary>
        /// <param name="worldPosition">The world position</param>
        /// <returns>The corresponding grid position</returns>
        public Vector2I WorldToGridPosition(Vector2 worldPosition)
        {
            return new Vector2I(
                Mathf.FloorToInt(worldPosition.X / _cellSize),
                Mathf.FloorToInt(worldPosition.Y / _cellSize)
            );
        }

        /// <summary>
        /// Gets the width of the grid.
        /// </summary>
        public int GetWidth()
        {
            return _width;
        }

        /// <summary>
        /// Gets the height of the grid.
        /// </summary>
        public int GetHeight()
        {
            return _height;
        }

        /// <summary>
        /// Gets the size of each cell in pixels.
        /// </summary>
        public int GetCellSize()
        {
            return _cellSize;
        }
    }
}

using Godot;
using GUT;
using SignalLost.Field;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [GlobalClass]
    [TestClass]
    public partial class GridSystemTest : Test
    {
        private GridSystem _gridSystem;
        
        public new void Before()
        {
            _gridSystem = new GridSystem();
            AddChild(_gridSystem);
            
            // Initialize with a small test grid
            _gridSystem.Initialize(10, 10);
        }
        
        public new void After()
        {
            _gridSystem.QueueFree();
            _gridSystem = null;
        }
        
        [TestMethod]
        public void TestGridInitialization()
        {
            // Verify grid dimensions
            Assert.AreEqual(10, _gridSystem.GetWidth());
            Assert.AreEqual(10, _gridSystem.GetHeight());
            
            // Verify cell size
            Assert.AreEqual(32, _gridSystem.GetCellSize());
        }
        
        [TestMethod]
        public void TestIsValidPosition()
        {
            // Valid positions
            Assert.IsTrue(_gridSystem.IsValidPosition(new Vector2I(0, 0)));
            Assert.IsTrue(_gridSystem.IsValidPosition(new Vector2I(5, 5)));
            Assert.IsTrue(_gridSystem.IsValidPosition(new Vector2I(9, 9)));
            
            // Invalid positions (out of bounds)
            Assert.IsFalse(_gridSystem.IsValidPosition(new Vector2I(-1, 0)));
            Assert.IsFalse(_gridSystem.IsValidPosition(new Vector2I(0, -1)));
            Assert.IsFalse(_gridSystem.IsValidPosition(new Vector2I(10, 0)));
            Assert.IsFalse(_gridSystem.IsValidPosition(new Vector2I(0, 10)));
            
            // Block a cell and verify it's invalid
            _gridSystem.SetCellBlocked(new Vector2I(5, 5), true);
            Assert.IsFalse(_gridSystem.IsValidPosition(new Vector2I(5, 5)));
            
            // Unblock the cell and verify it's valid again
            _gridSystem.SetCellBlocked(new Vector2I(5, 5), false);
            Assert.IsTrue(_gridSystem.IsValidPosition(new Vector2I(5, 5)));
        }
        
        [TestMethod]
        public void TestGetCell()
        {
            // Get a valid cell
            Cell cell = _gridSystem.GetCell(new Vector2I(5, 5));
            Assert.IsNotNull(cell);
            Assert.AreEqual(new Vector2I(5, 5), cell.Position);
            Assert.IsFalse(cell.IsBlocked);
            
            // Get an invalid cell (out of bounds)
            Cell invalidCell = _gridSystem.GetCell(new Vector2I(10, 10));
            Assert.IsNull(invalidCell);
        }
        
        [TestMethod]
        public void TestGridToWorldPosition()
        {
            // Convert grid positions to world positions
            Vector2 worldPos1 = _gridSystem.GridToWorldPosition(new Vector2I(0, 0));
            Vector2 worldPos2 = _gridSystem.GridToWorldPosition(new Vector2I(1, 1));
            Vector2 worldPos3 = _gridSystem.GridToWorldPosition(new Vector2I(5, 5));
            
            // Verify conversions
            Assert.AreEqual(new Vector2(0, 0), worldPos1);
            Assert.AreEqual(new Vector2(32, 32), worldPos2);
            Assert.AreEqual(new Vector2(160, 160), worldPos3);
        }
        
        [TestMethod]
        public void TestWorldToGridPosition()
        {
            // Convert world positions to grid positions
            Vector2I gridPos1 = _gridSystem.WorldToGridPosition(new Vector2(0, 0));
            Vector2I gridPos2 = _gridSystem.WorldToGridPosition(new Vector2(32, 32));
            Vector2I gridPos3 = _gridSystem.WorldToGridPosition(new Vector2(160, 160));
            Vector2I gridPos4 = _gridSystem.WorldToGridPosition(new Vector2(31, 31));
            
            // Verify conversions
            Assert.AreEqual(new Vector2I(0, 0), gridPos1);
            Assert.AreEqual(new Vector2I(1, 1), gridPos2);
            Assert.AreEqual(new Vector2I(5, 5), gridPos3);
            Assert.AreEqual(new Vector2I(0, 0), gridPos4); // Should floor to 0,0
        }
    }
}

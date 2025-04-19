using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalLost.Field;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class GridSystemTests : Test
    {
        private GridSystem _gridSystem;
        
        [TestInitialize]
        public void Setup()
        {
            _gridSystem = new GridSystem();
            AddChild(_gridSystem);
            
            // Initialize with a small test grid
            _gridSystem.Initialize(10, 10);
        }
        
        [TestCleanup]
        public void Teardown()
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
            Assert.IsFalse(cell.HasInteractable);
            
            // Get an invalid cell (out of bounds)
            Cell invalidCell = _gridSystem.GetCell(new Vector2I(10, 10));
            Assert.IsNull(invalidCell);
        }
        
        [TestMethod]
        public void TestSetCellBlocked()
        {
            // Block a cell
            _gridSystem.SetCellBlocked(new Vector2I(5, 5), true);
            Cell cell = _gridSystem.GetCell(new Vector2I(5, 5));
            Assert.IsTrue(cell.IsBlocked);
            
            // Unblock the cell
            _gridSystem.SetCellBlocked(new Vector2I(5, 5), false);
            cell = _gridSystem.GetCell(new Vector2I(5, 5));
            Assert.IsFalse(cell.IsBlocked);
        }
        
        [TestMethod]
        public void TestPlaceInteractable()
        {
            // Create an interactable object
            var interactable = new InteractableObject();
            interactable.ObjectId = "test_object";
            interactable.DisplayName = "Test Object";
            
            // Place the interactable on the grid
            _gridSystem.PlaceInteractable(new Vector2I(5, 5), interactable);
            
            // Verify the cell has an interactable
            Cell cell = _gridSystem.GetCell(new Vector2I(5, 5));
            Assert.IsTrue(cell.HasInteractable);
            
            // Verify the interactable is the one we placed
            InteractableObject retrievedInteractable = cell.GetInteractable();
            Assert.IsNotNull(retrievedInteractable);
            Assert.AreEqual("test_object", retrievedInteractable.ObjectId);
            Assert.AreEqual("Test Object", retrievedInteractable.DisplayName);
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

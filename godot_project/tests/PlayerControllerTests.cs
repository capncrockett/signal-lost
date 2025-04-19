using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalLost.Field;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class PlayerControllerTests : Test
    {
        private GridSystem _gridSystem;
        private PlayerController _playerController;
        private Node2D _testScene;
        
        [TestInitialize]
        public void Setup()
        {
            // Create a test scene
            _testScene = new Node2D();
            _testScene.Name = "FieldExplorationScene";
            AddChild(_testScene);
            
            // Create a grid system
            _gridSystem = new GridSystem();
            _gridSystem.Name = "GridSystem";
            _testScene.AddChild(_gridSystem);
            
            // Initialize with a small test grid
            _gridSystem.Initialize(10, 10);
            
            // Create a player controller
            _playerController = new PlayerController();
            _testScene.AddChild(_playerController);
            
            // Wait for _Ready to be called
            ProcessFrame();
        }
        
        [TestCleanup]
        public void Teardown()
        {
            _testScene.QueueFree();
            _testScene = null;
            _gridSystem = null;
            _playerController = null;
        }
        
        [TestMethod]
        public void TestInitialPosition()
        {
            // Verify initial grid position
            Assert.AreEqual(new Vector2I(1, 1), _playerController.GetGridPosition());
            
            // Verify initial world position
            Assert.AreEqual(_gridSystem.GridToWorldPosition(new Vector2I(1, 1)), _playerController.Position);
        }
        
        [TestMethod]
        public void TestMovement()
        {
            // Initial position
            Vector2I initialPosition = _playerController.GetGridPosition();
            
            // Move right
            bool moveRightResult = _playerController.TryMove(new Vector2I(1, 0));
            Assert.IsTrue(moveRightResult);
            Assert.AreEqual(initialPosition + new Vector2I(1, 0), _playerController.GetGridPosition());
            
            // Move down
            bool moveDownResult = _playerController.TryMove(new Vector2I(0, 1));
            Assert.IsTrue(moveDownResult);
            Assert.AreEqual(initialPosition + new Vector2I(1, 1), _playerController.GetGridPosition());
            
            // Move left
            bool moveLeftResult = _playerController.TryMove(new Vector2I(-1, 0));
            Assert.IsTrue(moveLeftResult);
            Assert.AreEqual(initialPosition + new Vector2I(0, 1), _playerController.GetGridPosition());
            
            // Move up
            bool moveUpResult = _playerController.TryMove(new Vector2I(0, -1));
            Assert.IsTrue(moveUpResult);
            Assert.AreEqual(initialPosition, _playerController.GetGridPosition());
        }
        
        [TestMethod]
        public void TestMovementBlocked()
        {
            // Initial position
            Vector2I initialPosition = _playerController.GetGridPosition();
            
            // Block the cell to the right
            Vector2I blockedPosition = initialPosition + new Vector2I(1, 0);
            _gridSystem.SetCellBlocked(blockedPosition, true);
            
            // Try to move right (should fail)
            bool moveRightResult = _playerController.TryMove(new Vector2I(1, 0));
            Assert.IsFalse(moveRightResult);
            Assert.AreEqual(initialPosition, _playerController.GetGridPosition());
        }
        
        [TestMethod]
        public void TestSetGridPosition()
        {
            // Set grid position
            Vector2I newPosition = new Vector2I(5, 5);
            _playerController.SetGridPosition(newPosition);
            
            // Verify grid position
            Assert.AreEqual(newPosition, _playerController.GetGridPosition());
            
            // Verify world position
            Assert.AreEqual(_gridSystem.GridToWorldPosition(newPosition), _playerController.Position);
        }
        
        [TestMethod]
        public void TestInteraction()
        {
            // Create an interactable object
            var interactable = new InteractableObject();
            interactable.ObjectId = "test_object";
            interactable.DisplayName = "Test Object";
            
            // Place the interactable in front of the player
            Vector2I playerPosition = _playerController.GetGridPosition();
            Vector2I interactablePosition = playerPosition + new Vector2I(0, 1); // Place below the player
            _gridSystem.PlaceInteractable(interactablePosition, interactable);
            
            // Move the player to face the interactable
            _playerController.TryMove(new Vector2I(0, 1));
            _playerController.TryMove(new Vector2I(0, -1)); // Move back but still facing down
            
            // Interact with the object
            bool interactionResult = _playerController.Interact();
            Assert.IsTrue(interactionResult);
            
            // Verify the object has been interacted with
            Assert.IsTrue(interactable._hasBeenInteracted);
        }
    }
}

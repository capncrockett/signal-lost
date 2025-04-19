using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalLost.Field;

namespace SignalLost.Tests
{
    [TestClass]
    public partial class InteractableObjectTests : Test
    {
        private InteractableObject _interactableObject;
        private ItemObject _itemObject;
        private SignalSourceObject _signalSourceObject;
        
        [TestInitialize]
        public void Setup()
        {
            // Create a basic interactable object
            _interactableObject = new InteractableObject();
            _interactableObject.ObjectId = "test_object";
            _interactableObject.DisplayName = "Test Object";
            _interactableObject.Description = "A test object.";
            AddChild(_interactableObject);
            
            // Create an item object
            _itemObject = new ItemObject();
            _itemObject.ObjectId = "test_item";
            _itemObject.ItemId = "test_item";
            _itemObject.DisplayName = "Test Item";
            _itemObject.Description = "A test item.";
            _itemObject.Quantity = 1;
            AddChild(_itemObject);
            
            // Create a signal source object
            _signalSourceObject = new SignalSourceObject();
            _signalSourceObject.ObjectId = "test_signal";
            _signalSourceObject.DisplayName = "Test Signal";
            _signalSourceObject.Description = "A test signal source.";
            _signalSourceObject.Frequency = 90.0f;
            _signalSourceObject.MessageId = "test_message";
            _signalSourceObject.SignalStrength = 1.0f;
            AddChild(_signalSourceObject);
            
            // Wait for _Ready to be called
            ProcessFrame();
        }
        
        [TestCleanup]
        public void Teardown()
        {
            _interactableObject.QueueFree();
            _interactableObject = null;
            
            _itemObject.QueueFree();
            _itemObject = null;
            
            _signalSourceObject.QueueFree();
            _signalSourceObject = null;
        }
        
        [TestMethod]
        public void TestBasicInteractableObject()
        {
            // Verify properties
            Assert.AreEqual("test_object", _interactableObject.ObjectId);
            Assert.AreEqual("Test Object", _interactableObject.DisplayName);
            Assert.AreEqual("A test object.", _interactableObject.Description);
            Assert.IsFalse(_interactableObject.IsOneTimeInteraction);
            
            // Verify interaction
            bool interactionResult = _interactableObject.Interact();
            Assert.IsTrue(interactionResult);
            
            // Verify interaction prompt
            string prompt = _interactableObject.GetInteractionPrompt();
            Assert.AreEqual("Press E to interact with Test Object", prompt);
        }
        
        [TestMethod]
        public void TestOneTimeInteraction()
        {
            // Set as one-time interaction
            _interactableObject.IsOneTimeInteraction = true;
            
            // First interaction should succeed
            bool firstInteractionResult = _interactableObject.Interact();
            Assert.IsTrue(firstInteractionResult);
            
            // Second interaction should fail
            bool secondInteractionResult = _interactableObject.Interact();
            Assert.IsFalse(secondInteractionResult);
            
            // Reset interaction
            _interactableObject.ResetInteraction();
            
            // Interaction should succeed again
            bool resetInteractionResult = _interactableObject.Interact();
            Assert.IsTrue(resetInteractionResult);
        }
        
        [TestMethod]
        public void TestItemObject()
        {
            // Verify properties
            Assert.AreEqual("test_item", _itemObject.ObjectId);
            Assert.AreEqual("test_item", _itemObject.ItemId);
            Assert.AreEqual("Test Item", _itemObject.DisplayName);
            Assert.AreEqual("A test item.", _itemObject.Description);
            Assert.AreEqual(1, _itemObject.Quantity);
            Assert.IsTrue(_itemObject.IsOneTimeInteraction);
            
            // Verify interaction prompt
            string prompt = _itemObject.GetInteractionPrompt();
            Assert.AreEqual("Press E to collect Test Item", prompt);
        }
        
        [TestMethod]
        public void TestSignalSourceObject()
        {
            // Verify properties
            Assert.AreEqual("test_signal", _signalSourceObject.ObjectId);
            Assert.AreEqual("Test Signal", _signalSourceObject.DisplayName);
            Assert.AreEqual("A test signal source.", _signalSourceObject.Description);
            Assert.AreEqual(90.0f, _signalSourceObject.Frequency);
            Assert.AreEqual("test_message", _signalSourceObject.MessageId);
            Assert.AreEqual(1.0f, _signalSourceObject.SignalStrength);
            
            // Verify interaction prompt
            string prompt = _signalSourceObject.GetInteractionPrompt();
            Assert.AreEqual("Press E to examine Test Signal", prompt);
        }
    }
}

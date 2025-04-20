using Godot;
using GUT;
using SignalLost.Field;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [GlobalClass]
    [TestClass]
    public partial class SignalSourceManagerTest : Test
    {
        private SignalSourceManager _signalSourceManager;
        private SignalSourceObject _testSignalSource;
        private GridSystem _gridSystem;

        public void Before()
        {
            // Create a grid system
            _gridSystem = new GridSystem();
            AddChild(_gridSystem);
            _gridSystem.Initialize(10, 10);

            // Create a signal source manager
            _signalSourceManager = new SignalSourceManager();
            AddChild(_signalSourceManager);

            // Create a test signal source
            _testSignalSource = new SignalSourceObject();
            _testSignalSource.Frequency = 91.5f;
            _testSignalSource.MessageId = "test_signal";
            _testSignalSource.SignalStrength = 1.0f;
            _testSignalSource.SignalRange = 5.0f;
            _testSignalSource.Position = new Vector2(160, 160); // Position at grid 5,5
            AddChild(_testSignalSource);
        }

        public void After()
        {
            _gridSystem.QueueFree();
            _gridSystem = null;

            _signalSourceManager.QueueFree();
            _signalSourceManager = null;

            _testSignalSource.QueueFree();
            _testSignalSource = null;
        }

        [TestMethod]
        public void TestAddSignalSource()
        {
            // Add the signal source to the manager
            _signalSourceManager.AddSignalSource(_testSignalSource);

            // Verify it was added
            var sources = _signalSourceManager.GetSignalSources();
            Assert.AreEqual(1, sources.Count);
            Assert.IsTrue(sources.Contains(_testSignalSource));
        }

        [TestMethod]
        public void TestRemoveSignalSource()
        {
            // Add the signal source to the manager
            _signalSourceManager.AddSignalSource(_testSignalSource);

            // Verify it was added
            var sources = _signalSourceManager.GetSignalSources();
            Assert.AreEqual(1, sources.Count);

            // Remove the signal source
            _signalSourceManager.RemoveSignalSource(_testSignalSource);

            // Verify it was removed
            sources = _signalSourceManager.GetSignalSources();
            Assert.AreEqual(0, sources.Count);
        }

        [TestMethod]
        public void TestSignalStrengthCalculation()
        {
            // Test signal strength calculation directly
            // Signal source is at position 5,5 (160, 160)

            // Test position at 0,0 (far from signal source)
            Vector2I farPosition = new Vector2I(0, 0);
            float strength = _testSignalSource.CalculateSignalStrengthAtPosition(farPosition);

            // Signal source is at 5,5, test position is at 0,0, distance is about 7.07 cells
            // With a range of 5.0, the strength should be 0
            Assert.AreEqual(0.0f, strength);

            // Test position at 3,3 (medium distance from signal source)
            Vector2I mediumPosition = new Vector2I(3, 3);
            strength = _testSignalSource.CalculateSignalStrengthAtPosition(mediumPosition);

            // Signal source is at 5,5, test position is at 3,3, distance is about 2.83 cells
            // With a range of 5.0, the strength should be about 0.43
            Assert.IsTrue(strength > 0.4f && strength < 0.5f);

            // Test position at 5,5 (same as signal source)
            Vector2I samePosition = new Vector2I(5, 5);
            strength = _testSignalSource.CalculateSignalStrengthAtPosition(samePosition);

            // Signal source is at 5,5, test position is at 5,5, distance is 0
            // With a range of 5.0, the strength should be 1.0
            Assert.AreEqual(1.0f, strength);
        }
    }
}

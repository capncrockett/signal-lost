using Godot;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// Tests for the GameState class.
    /// </summary>
    [TestClass]
    public partial class GameStateTests : UnitTestBase
    {
        private GameState _gameState;
        
        /// <summary>
        /// Called before each test.
        /// </summary>
        public override void Before()
        {
            base.Before();
            
            // Create a new instance of the GameState
            _gameState = CreateMockGameState();
        }
        
        /// <summary>
        /// Tests setting the radio frequency.
        /// </summary>
        [TestMethod]
        public void TestSetFrequency()
        {
            // Arrange
            float initialFrequency = _gameState.CurrentFrequency;
            float newFrequency = 95.5f;
            
            // Act
            _gameState.SetFrequency(newFrequency);
            
            // Assert
            Assert.AreEqual(newFrequency, _gameState.CurrentFrequency,
                "Frequency should be updated to the new value");
        }
        
        /// <summary>
        /// Tests frequency limits.
        /// </summary>
        [TestMethod]
        public void TestFrequencyLimits()
        {
            // Arrange
            float belowMin = 87.0f;
            float aboveMax = 109.0f;
            
            // Act
            _gameState.SetFrequency(belowMin);
            
            // Assert
            Assert.AreEqual(TestData.Frequencies.Min, _gameState.CurrentFrequency,
                "Frequency should be clamped to minimum value");
            
            // Act
            _gameState.SetFrequency(aboveMax);
            
            // Assert
            Assert.AreEqual(TestData.Frequencies.Max, _gameState.CurrentFrequency,
                "Frequency should be clamped to maximum value");
        }
        
        /// <summary>
        /// Tests toggling the radio on and off.
        /// </summary>
        [TestMethod]
        public void TestToggleRadio()
        {
            // Arrange
            bool initialState = _gameState.IsRadioOn;
            
            // Act
            _gameState.ToggleRadio();
            
            // Assert
            Assert.AreEqual(!initialState, _gameState.IsRadioOn,
                "Radio state should be toggled");
            
            // Act
            _gameState.ToggleRadio();
            
            // Assert
            Assert.AreEqual(initialState, _gameState.IsRadioOn,
                "Radio state should be toggled back to initial state");
        }
        
        /// <summary>
        /// Tests finding a signal at a specific frequency.
        /// </summary>
        [TestMethod]
        public void TestFindSignalAtFrequency()
        {
            // Arrange
            float signalFrequency = TestData.Frequencies.Signal1; // This should match a signal in GameState.Signals
            float nonSignalFrequency = TestData.Frequencies.NoSignal1; // This should not match any signal
            
            // Act
            var signal = _gameState.FindSignalAtFrequency(signalFrequency);
            var noSignal = _gameState.FindSignalAtFrequency(nonSignalFrequency);
            
            // Assert
            Assert.IsNotNull(signal, $"Signal should be found at frequency {TestData.Frequencies.Signal1}");
            Assert.AreEqual(signalFrequency, signal.Frequency, "Signal frequency should match");
            Assert.IsNull(noSignal, $"No signal should be found at frequency {TestData.Frequencies.NoSignal1}");
        }
        
        /// <summary>
        /// Tests calculating signal strength.
        /// </summary>
        [TestMethod]
        public void TestCalculateSignalStrength()
        {
            // Arrange
            float signalFrequency = TestData.Frequencies.Signal1;
            var signal = _gameState.FindSignalAtFrequency(signalFrequency);
            
            // Act
            float exactStrength = GameState.CalculateSignalStrength(signalFrequency, signal);
            float offsetStrength = GameState.CalculateSignalStrength(signalFrequency + 0.1f, signal);
            float farStrength = GameState.CalculateSignalStrength(signalFrequency + 1.0f, signal);
            
            // Assert
            Assert.AreEqual(1.0f, exactStrength, "Signal strength should be maximum when tuned exactly");
            Assert.IsTrue(offsetStrength < 1.0f, "Signal strength should be less than maximum when slightly off-tune");
            Assert.IsTrue(offsetStrength > 0.0f, "Signal strength should be greater than zero when slightly off-tune");
            Assert.AreEqual(0.0f, farStrength, "Signal strength should be zero when far off-tune");
        }
        
        /// <summary>
        /// Tests adding discovered frequencies.
        /// </summary>
        [TestMethod]
        public void TestAddDiscoveredFrequency()
        {
            // Arrange
            float newFrequency = 95.5f;
            int initialCount = _gameState.DiscoveredFrequencies.Count;
            
            // Act
            _gameState.AddDiscoveredFrequency(newFrequency);
            
            // Assert
            Assert.AreEqual(initialCount + 1, _gameState.DiscoveredFrequencies.Count,
                "Discovered frequencies count should increase");
            Assert.IsTrue(_gameState.DiscoveredFrequencies.Contains(newFrequency),
                "Discovered frequencies should contain the new frequency");
            
            // Act - Add the same frequency again
            _gameState.AddDiscoveredFrequency(newFrequency);
            
            // Assert
            Assert.AreEqual(initialCount + 1, _gameState.DiscoveredFrequencies.Count,
                "Discovered frequencies count should not increase when adding a duplicate");
        }
        
        /// <summary>
        /// Tests decoding messages.
        /// </summary>
        [TestMethod]
        public void TestDecodeMessage()
        {
            // Arrange
            string messageId = TestData.MessageIds.Message1;
            
            // Act
            bool result = _gameState.DecodeMessage(messageId);
            
            // Assert
            Assert.IsTrue(result, "Message should be successfully decoded");
            Assert.IsTrue(_gameState.Messages[messageId].Decoded, "Message should be marked as decoded");
            
            // Act - Try to decode the same message again
            bool secondResult = _gameState.DecodeMessage(messageId);
            
            // Assert
            Assert.IsFalse(secondResult, "Already decoded message should not be decoded again");
        }
        
        /// <summary>
        /// Tests getting static intensity.
        /// </summary>
        [TestMethod]
        public void TestGetStaticIntensity()
        {
            // Arrange
            float frequency1 = TestData.Frequencies.NoSignal1;
            float frequency2 = TestData.Frequencies.NoSignal2;
            
            // Act
            float intensity1 = GameState.GetStaticIntensity(frequency1);
            float intensity2 = GameState.GetStaticIntensity(frequency2);
            
            // Assert
            Assert.IsTrue(intensity1 > 0.0f, "Static intensity should be greater than zero");
            Assert.IsTrue(intensity1 < 1.0f, "Static intensity should be less than one");
            Assert.AreNotEqual(intensity1, intensity2,
                "Static intensity should be different for different frequencies");
        }
    }
}

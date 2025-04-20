using Godot;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    [TestClass]
    public class RadioSignalTests
    {
        [TestMethod]
        public void TestRadioSignalCreation()
        {
            // Create a radio signal
            RadioSignal signal = new RadioSignal
            {
                Id = "test_signal",
                Frequency = 90.5f,
                Message = "Test message",
                Type = RadioSignalType.Morse,
                Strength = 0.8f,
                IsActive = true
            };
            
            // Verify properties
            Assert.AreEqual("test_signal", signal.Id, "ID should match");
            Assert.AreEqual(90.5f, signal.Frequency, "Frequency should match");
            Assert.AreEqual("Test message", signal.Message, "Message should match");
            Assert.AreEqual(RadioSignalType.Morse, signal.Type, "Type should match");
            Assert.AreEqual(0.8f, signal.Strength, "Strength should match");
            Assert.IsTrue(signal.IsActive, "IsActive should match");
        }
        
        [TestMethod]
        public void TestRadioSignalDefaultValues()
        {
            // Create a radio signal with minimal properties
            RadioSignal signal = new RadioSignal
            {
                Id = "minimal_signal",
                Frequency = 91.0f,
                Message = "Minimal message"
            };
            
            // Verify default values
            Assert.AreEqual(RadioSignalType.Voice, signal.Type, "Default type should be Voice");
            Assert.AreEqual(1.0f, signal.Strength, "Default strength should be 1.0");
            Assert.IsTrue(signal.IsActive, "Default IsActive should be true");
        }
        
        [TestMethod]
        public void TestRadioSignalTypes()
        {
            // Test each signal type
            RadioSignal voiceSignal = new RadioSignal { Type = RadioSignalType.Voice };
            RadioSignal morseSignal = new RadioSignal { Type = RadioSignalType.Morse };
            RadioSignal dataSignal = new RadioSignal { Type = RadioSignalType.Data };
            RadioSignal beaconSignal = new RadioSignal { Type = RadioSignalType.Beacon };
            
            // Verify types
            Assert.AreEqual(RadioSignalType.Voice, voiceSignal.Type, "Voice type should match");
            Assert.AreEqual(RadioSignalType.Morse, morseSignal.Type, "Morse type should match");
            Assert.AreEqual(RadioSignalType.Data, dataSignal.Type, "Data type should match");
            Assert.AreEqual(RadioSignalType.Beacon, beaconSignal.Type, "Beacon type should match");
        }
    }
}

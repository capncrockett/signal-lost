using Godot;
using System;
using System.Collections.Generic;
using GUT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[GlobalClass]
namespace SignalLost.Tests
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public partial class GameStateTests : Test
    {
        private GameState _gameState = null;

        // Called before each test
        public override void Before()
        {
            // Create a new instance of the GameState
            _gameState = new GameState();
            AddChild(_gameState);
            _gameState._Ready(); // Call _Ready manually since we're not using the scene tree
        }

        // Called after each test
        public override void After()
        {
            // Clean up
            _gameState.QueueFree();
            _gameState = null;
        }

        // Test frequency setting
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestSetFrequency()
        {
            // Arrange
            float initialFrequency = _gameState.CurrentFrequency;
            float newFrequency = 95.5f;

            // Act
            _gameState.SetFrequency(newFrequency);

            // Assert
            AssertEqual(_gameState.CurrentFrequency, newFrequency,
                "Frequency should be updated to the new value");
        }

        // Test frequency limits
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestFrequencyLimits()
        {
            // Arrange
            float belowMin = 87.0f;
            float aboveMax = 109.0f;

            // Act
            _gameState.SetFrequency(belowMin);

            // Assert
            AssertEqual(_gameState.CurrentFrequency, 88.0f,
                "Frequency should be clamped to minimum value");

            // Act
            _gameState.SetFrequency(aboveMax);

            // Assert
            AssertEqual(_gameState.CurrentFrequency, 108.0f,
                "Frequency should be clamped to maximum value");
        }

        // Test radio toggle
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestToggleRadio()
        {
            // Arrange
            bool initialState = _gameState.IsRadioOn;

            // Act
            _gameState.ToggleRadio();

            // Assert
            AssertEqual(_gameState.IsRadioOn, !initialState,
                "Radio state should be toggled");

            // Act
            _gameState.ToggleRadio();

            // Assert
            AssertEqual(_gameState.IsRadioOn, initialState,
                "Radio state should be toggled back to initial state");
        }

        // Test signal detection
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestFindSignalAtFrequency()
        {
            // Arrange
            float signalFrequency = 91.5f; // This should match a signal in GameState.Signals
            float nonSignalFrequency = 92.5f; // This should not match any signal

            // Act
            var signal = _gameState.FindSignalAtFrequency(signalFrequency);
            var noSignal = _gameState.FindSignalAtFrequency(nonSignalFrequency);

            // Assert
            AssertNotNull(signal, "Signal should be found at frequency 91.5");
            AssertEqual(signal.Frequency, signalFrequency, "Signal frequency should match");
            AssertNull(noSignal, "No signal should be found at frequency 92.5");
        }

        // Test signal strength calculation
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestCalculateSignalStrength()
        {
            // Arrange
            float signalFrequency = 91.5f;
            var signal = _gameState.FindSignalAtFrequency(signalFrequency);

            // Act
            float exactStrength = _gameState.CalculateSignalStrength(signalFrequency, signal);
            float offsetStrength = _gameState.CalculateSignalStrength(signalFrequency + 0.1f, signal);
            float farStrength = _gameState.CalculateSignalStrength(signalFrequency + 1.0f, signal);

            // Assert
            AssertEqual(exactStrength, 1.0f, "Signal strength should be maximum when tuned exactly");
            AssertLess(offsetStrength, 1.0f, "Signal strength should be less than maximum when slightly off-tune");
            AssertGreater(offsetStrength, 0.0f, "Signal strength should be greater than zero when slightly off-tune");
            AssertEqual(farStrength, 0.0f, "Signal strength should be zero when far off-tune");
        }

        // Test discovered frequencies
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestAddDiscoveredFrequency()
        {
            // Arrange
            float newFrequency = 95.5f;
            int initialCount = _gameState.DiscoveredFrequencies.Count;

            // Act
            _gameState.AddDiscoveredFrequency(newFrequency);

            // Assert
            AssertEqual(_gameState.DiscoveredFrequencies.Count, initialCount + 1,
                "Discovered frequencies count should increase");
            AssertTrue(_gameState.DiscoveredFrequencies.Contains(newFrequency),
                "Discovered frequencies should contain the new frequency");

            // Act - Add the same frequency again
            _gameState.AddDiscoveredFrequency(newFrequency);

            // Assert
            AssertEqual(_gameState.DiscoveredFrequencies.Count, initialCount + 1,
                "Discovered frequencies count should not increase when adding a duplicate");
        }

        // Test message decoding
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestDecodeMessage()
        {
            // Arrange
            string messageId = "msg_001";

            // Act
            bool result = _gameState.DecodeMessage(messageId);

            // Assert
            AssertTrue(result, "Message should be successfully decoded");
            AssertTrue(_gameState.Messages[messageId].Decoded, "Message should be marked as decoded");

            // Act - Try to decode the same message again
            bool secondResult = _gameState.DecodeMessage(messageId);

            // Assert
            AssertFalse(secondResult, "Already decoded message should not be decoded again");
        }

        // Test static intensity
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void TestGetStaticIntensity()
        {
            // Arrange
            float frequency1 = 90.0f;
            float frequency2 = 95.0f;

            // Act
            float intensity1 = _gameState.GetStaticIntensity(frequency1);
            float intensity2 = _gameState.GetStaticIntensity(frequency2);

            // Assert
            AssertGreater(intensity1, 0.0f, "Static intensity should be greater than zero");
            AssertLess(intensity1, 1.0f, "Static intensity should be less than one");
            AssertNotEqual(intensity1, intensity2,
                "Static intensity should be different for different frequencies");
        }
    }
}

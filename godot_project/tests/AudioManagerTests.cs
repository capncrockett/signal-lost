using Godot;
using System;
using GUT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
	[Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
	public partial class AudioManagerTests : Test
	{
		private AudioManager _audioManager = null;

		// Called before each test
		public override void Before()
		{
			// Create a new instance of the AudioManager
			_audioManager = new AudioManager();
			AddChild(_audioManager);
			_audioManager._Ready(); // Call _Ready manually since we're not using the scene tree
		}

		// Called after each test
		public override void After()
		{
			// Clean up
			_audioManager.QueueFree();
			_audioManager = null;
		}

		// Test volume setting
		[Test]
		public void TestSetVolume()
		{
			// Arrange
			float newVolume = 0.5f;

			// Act
			_audioManager.SetVolume(newVolume);

			// Assert
			AssertEqual(_audioManager.Get("_volume"), newVolume,
				"Volume should be updated to the new value");
		}

		// Test volume limits
		[Test]
		public void TestVolumeLimits()
		{
			// Arrange
			float belowMin = -0.5f;
			float aboveMax = 1.5f;

			// Act
			_audioManager.SetVolume(belowMin);

			// Assert
			AssertEqual(_audioManager.Get("_volume"), 0.0f,
				"Volume should be clamped to minimum value");

			// Act
			_audioManager.SetVolume(aboveMax);

			// Assert
			AssertEqual(_audioManager.Get("_volume"), 1.0f,
				"Volume should be clamped to maximum value");
		}

		// Test mute toggle
		[Test]
		public void TestToggleMute()
		{
			// Arrange
			bool initialState = (bool)_audioManager.Get("_isMuted");

			// Act
			_audioManager.ToggleMute();

			// Assert
			AssertEqual(_audioManager.Get("_isMuted"), !initialState,
				"Mute state should be toggled");

			// Act
			_audioManager.ToggleMute();

			// Assert
			AssertEqual(_audioManager.Get("_isMuted"), initialState,
				"Mute state should be toggled back to initial state");
		}

		// Test noise type setting
		[Test]
		public void TestSetNoiseType()
		{
			// Arrange
			AudioManager.NoiseType initialType = (AudioManager.NoiseType)_audioManager.Get("_currentNoiseType");
			AudioManager.NoiseType newType = AudioManager.NoiseType.Pink;

			// Act
			_audioManager.SetNoiseType(newType);

			// Assert
			AssertEqual(_audioManager.Get("_currentNoiseType"), newType,
				"Noise type should be updated to the new value");
			AssertNotEqual(_audioManager.Get("_currentNoiseType"), initialType,
				"Noise type should be different from initial value");
		}

		// Test static noise playback
		[Test]
		public void TestPlayStaticNoise()
		{
			// Arrange
			float intensity = 0.7f;

			// Act
			_audioManager.PlayStaticNoise(intensity);

			// Assert
			var staticPlayer = (AudioStreamPlayer)_audioManager.Get("_staticPlayer");
			AssertTrue(staticPlayer.Playing, "Static player should be playing");

			// Act
			_audioManager.StopStaticNoise();

			// Assert
			AssertFalse(staticPlayer.Playing, "Static player should not be playing after stopping");
		}

		// Test signal playback
		[Test]
		public void TestPlaySignal()
		{
			// Arrange
			float frequency = 440.0f;
			float volumeScale = 0.8f;

			// Act
			var generator = _audioManager.PlaySignal(frequency, volumeScale);

			// Assert
			var signalPlayer = (AudioStreamPlayer)_audioManager.Get("_signalPlayer");
			AssertTrue(signalPlayer.Playing, "Signal player should be playing");
			AssertNotNull(generator, "Generator should not be null");

			// Act
			_audioManager.StopSignal();

			// Assert
			AssertFalse(signalPlayer.Playing, "Signal player should not be playing after stopping");
		}

		// Test different waveforms
		[Test]
		public void TestDifferentWaveforms()
		{
			// Arrange
			float frequency = 440.0f;

			// Act - Test sine wave
			var sineGenerator = _audioManager.PlaySignal(frequency, 1.0f, "sine");
			_audioManager.StopSignal();

			// Act - Test square wave
			var squareGenerator = _audioManager.PlaySignal(frequency, 1.0f, "square");
			_audioManager.StopSignal();

			// Act - Test triangle wave
			var triangleGenerator = _audioManager.PlaySignal(frequency, 1.0f, "triangle");
			_audioManager.StopSignal();

			// Act - Test sawtooth wave
			var sawtoothGenerator = _audioManager.PlaySignal(frequency, 1.0f, "sawtooth");
			_audioManager.StopSignal();

			// Assert
			AssertNotNull(sineGenerator, "Sine generator should not be null");
			AssertNotNull(squareGenerator, "Square generator should not be null");
			AssertNotNull(triangleGenerator, "Triangle generator should not be null");
			AssertNotNull(sawtoothGenerator, "Sawtooth generator should not be null");
		}

		// Test effect playback
		[Test]
		public void TestPlayEffect()
		{
			// This test is more complex because it requires actual audio files
			// For now, we'll just test that the method doesn't crash

			// Act
			_audioManager.PlayEffect("test_effect");

			// Assert
			var effectPlayer = (AudioStreamPlayer)_audioManager.Get("_effectPlayer");

			// We can't assert that it's playing because the file might not exist
			// But we can assert that the method didn't crash
			Pass("PlayEffect method did not crash");
		}
	}
}

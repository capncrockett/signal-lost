using Godot;

namespace SignalLost.Tests
{
	[TestClass]
	public class RadioTunerTests : Test
	{
		// Path to the scene we want to test
		private PackedScene _radioTunerScene;
		private RadioTuner _radioTuner = null;
		private GameState _gameState;
		
		// Constants for frequency limits
		private const float MinFrequency = 88.0f;
		private const float MaxFrequency = 108.0f;

		// Called before each test
		public override void Before()
		{
			// Load the scene
			_radioTunerScene = GD.Load<PackedScene>("res://scenes/radio/RadioTuner.tscn");
			
			// Create a new instance of the RadioTuner scene
			_radioTuner = _radioTunerScene.Instantiate<RadioTuner>();
			AddChild(_radioTuner);
			
			// Get reference to GameState
			_gameState = GetNode<GameState>("/root/GameState");
			
			// Reset GameState to default values
			_gameState.SetFrequency(90.0f);
			_gameState.ToggleRadio(); // Ensure it's off
			if (_gameState.IsRadioOn)
				_gameState.ToggleRadio();
				
			_gameState.DiscoveredFrequencies.Clear();
		}

		// Called after each test
		public override void After()
		{
			// Clean up
			_radioTuner.QueueFree();
			_radioTuner = null;
		}

		// Test power button functionality
		[Test]
		public void TestPowerButton()
		{
			// Initially radio should be off
			AssertFalse(_gameState.IsRadioOn, "Radio should start in OFF state");
			
			// Simulate clicking the power button
			_radioTuner.GetNode<Button>("PowerButton").EmitSignal("pressed");
			
			// Radio should now be on
			AssertTrue(_gameState.IsRadioOn, "Radio should be ON after clicking power button");
			
			// Simulate clicking the power button again
			_radioTuner.GetNode<Button>("PowerButton").EmitSignal("pressed");
			
			// Radio should now be off again
			AssertFalse(_gameState.IsRadioOn, "Radio should be OFF after clicking power button again");
		}

		// Test frequency change
		[Test]
		public void TestFrequencyChange()
		{
			// Set initial frequency
			_gameState.SetFrequency(90.0f);
			
			// Call the method directly
			_radioTuner.Call("ChangeFrequency", 0.1f);
			
			// Check if frequency was updated
			AssertEqual(_gameState.CurrentFrequency, 90.1f, "Frequency should be 90.1 after increasing by 0.1");
			
			// Test frequency limits
			_gameState.SetFrequency(MinFrequency);
			_radioTuner.Call("ChangeFrequency", -0.1f);
			AssertEqual(_gameState.CurrentFrequency, MinFrequency, "Frequency should not go below minimum");
			
			_gameState.SetFrequency(MaxFrequency);
			_radioTuner.Call("ChangeFrequency", 0.1f);
			AssertEqual(_gameState.CurrentFrequency, MaxFrequency, "Frequency should not go above maximum");
		}

		// Test signal detection
		[Test]
		public void TestSignalDetection()
		{
			// Turn radio on
			_gameState.ToggleRadio();
			
			// Set frequency to a known signal
			_gameState.SetFrequency(91.5f);  // This should match a signal in GameState.Signals
			
			// Process the frequency
			_radioTuner.Call("ProcessFrequency");
			
			// Check if signal was detected
			AssertNotNull(_radioTuner.Get("_currentSignalId"), "Signal should be detected at frequency 91.5");
			AssertTrue((float)_radioTuner.Get("_signalStrength") > 0.5f, "Signal strength should be high when tuned correctly");
			
			// Check if frequency was added to discovered frequencies
			AssertTrue(_gameState.DiscoveredFrequencies.Contains(91.5f), "Frequency should be added to discovered frequencies");
			
			// Set frequency to a non-signal area
			_gameState.SetFrequency(92.5f);  // This should not match any signal
			
			// Process the frequency
			_radioTuner.Call("ProcessFrequency");
			
			// Check that no signal was detected
			AssertNull(_radioTuner.Get("_currentSignalId"), "No signal should be detected at frequency 92.5");
			AssertTrue((float)_radioTuner.Get("_signalStrength") < 0.2f, "Signal strength should be low when no signal is present");
		}

		// Test scanning functionality
		[Test]
		public void TestScanning()
		{
			// Turn radio on
			_gameState.ToggleRadio();
			
			// Start with a known frequency
			_gameState.SetFrequency(90.0f);
			
			// Start scanning
			_radioTuner.Call("ToggleScanning");
			
			// Verify scanning state
			AssertTrue((bool)_radioTuner.Get("_isScanning"), "Radio should be in scanning mode");
			
			// Simulate scan timer timeout
			_radioTuner.Call("OnScanTimerTimeout");
			
			// Frequency should have increased
			AssertEqual(_gameState.CurrentFrequency, 90.1f, "Frequency should increase after scan timer timeout");
			
			// Stop scanning
			_radioTuner.Call("ToggleScanning");
			
			// Verify scanning state
			AssertFalse((bool)_radioTuner.Get("_isScanning"), "Radio should not be in scanning mode after toggling");
		}

		// Test message display
		[Test]
		public void TestMessageDisplay()
		{
			// Turn radio on
			_gameState.ToggleRadio();
			
			// Set frequency to a known signal
			_gameState.SetFrequency(91.5f);  // This should match a signal in GameState.Signals
			
			// Process the frequency
			_radioTuner.Call("ProcessFrequency");
			
			// Check if message button is enabled
			AssertFalse(_radioTuner.GetNode<Button>("MessageContainer/MessageButton").Disabled, 
				"Message button should be enabled when signal is detected");
			
			// Toggle message display
			_radioTuner.Call("ToggleMessage");
			
			// Check if message is displayed
			AssertTrue((bool)_radioTuner.Get("_showMessage"), "Message should be displayed after toggling");
			AssertTrue(_radioTuner.GetNode<Control>("MessageContainer/MessageDisplay").Visible, 
				"Message display should be visible");
			
			// Toggle message display again
			_radioTuner.Call("ToggleMessage");
			
			// Check if message is hidden
			AssertFalse((bool)_radioTuner.Get("_showMessage"), "Message should be hidden after toggling again");
			AssertFalse(_radioTuner.GetNode<Control>("MessageContainer/MessageDisplay").Visible, 
				"Message display should be hidden");
		}

		// Test radio behavior when turned off
		[Test]
		public void TestRadioOffBehavior()
		{
			// Turn radio on initially
			_gameState.ToggleRadio();
			
			// Start scanning
			_radioTuner.Call("ToggleScanning");
			
			// Turn radio off
			_radioTuner.Call("TogglePower");
			
			// Check if scanning stopped
			AssertFalse((bool)_radioTuner.Get("_isScanning"), "Scanning should stop when radio is turned off");
			
			// Try to change frequency when radio is off
			float initialFreq = _gameState.CurrentFrequency;
			_radioTuner.Call("ChangeFrequency", 0.1f);
			
			// Frequency should still change even when radio is off
			AssertEqual(_gameState.CurrentFrequency, initialFreq + 0.1f, "Frequency should change even when radio is off");
		}
	}
}

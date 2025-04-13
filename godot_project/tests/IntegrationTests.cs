using Godot;
using System;

namespace SignalLost.Tests
{
    [TestClass]
    public class IntegrationTests : Test
    {
        // Components to test
        private GameState _gameState = null;
        private AudioManager _audioManager = null;
        private RadioTuner _radioTuner = null;
        private PackedScene _radioTunerScene;
        
        // Called before each test
        public override void Before()
        {
            // Create instances of the components
            _gameState = new GameState();
            _audioManager = new AudioManager();
            
            // Add them to the scene tree
            AddChild(_gameState);
            AddChild(_audioManager);
            
            // Initialize them
            _gameState._Ready();
            _audioManager._Ready();
            
            // Load the RadioTuner scene
            _radioTunerScene = GD.Load<PackedScene>("res://scenes/radio/RadioTuner.tscn");
            
            // Create a new instance of the RadioTuner scene
            _radioTuner = _radioTunerScene.Instantiate<RadioTuner>();
            AddChild(_radioTuner);
            
            // Reset GameState to default values
            _gameState.SetFrequency(90.0f);
            if (_gameState.IsRadioOn)
                _gameState.ToggleRadio(); // Ensure it's off
                
            _gameState.DiscoveredFrequencies.Clear();
        }
        
        // Called after each test
        public override void After()
        {
            // Clean up
            _radioTuner.QueueFree();
            _audioManager.QueueFree();
            _gameState.QueueFree();
            
            _radioTuner = null;
            _audioManager = null;
            _gameState = null;
        }
        
        // Test GameState and AudioManager integration
        [Test]
        public void TestGameStateAudioManagerIntegration()
        {
            // Turn radio on
            _gameState.ToggleRadio();
            
            // Set frequency to a known signal
            _gameState.SetFrequency(91.5f);  // This should match a signal in GameState.Signals
            
            // Process the frequency (normally done by RadioTuner)
            var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);
            
            // Verify signal was found
            AssertNotNull(signalData, "Signal should be found at frequency 91.5");
            
            // Calculate signal strength
            float signalStrength = _gameState.CalculateSignalStrength(_gameState.CurrentFrequency, signalData);
            
            // Verify signal strength is high when tuned correctly
            AssertGreater(signalStrength, 0.9f, "Signal strength should be high when tuned correctly");
            
            // Play audio based on signal data (normally done by RadioTuner)
            if (signalData.IsStatic)
            {
                float staticIntensity = 1.0f - signalStrength;
                _audioManager.PlayStaticNoise(staticIntensity);
                _audioManager.PlaySignal(signalData.Frequency * 10, signalStrength * 0.5f);
            }
            else
            {
                _audioManager.StopStaticNoise();
                _audioManager.PlaySignal(signalData.Frequency * 10);
            }
            
            // Verify audio is playing
            var staticPlayer = (AudioStreamPlayer)_audioManager.Get("_staticPlayer");
            var signalPlayer = (AudioStreamPlayer)_audioManager.Get("_signalPlayer");
            
            if (signalData.IsStatic)
            {
                AssertTrue(staticPlayer.Playing, "Static player should be playing for static signals");
            }
            
            AssertTrue(signalPlayer.Playing, "Signal player should be playing");
            
            // Clean up audio
            _audioManager.StopStaticNoise();
            _audioManager.StopSignal();
        }
        
        // Test RadioTuner and GameState integration
        [Test]
        public void TestRadioTunerGameStateIntegration()
        {
            // Turn radio on
            _gameState.ToggleRadio();
            
            // Verify RadioTuner updates when GameState changes
            AssertEqual(_radioTuner.GetNode<Button>("PowerButton").Text, "ON", 
                "Power button text should update when radio is turned on");
            
            // Change frequency via GameState
            _gameState.SetFrequency(95.5f);
            
            // Verify RadioTuner frequency display updates
            AssertEqual(_radioTuner.GetNode<Label>("FrequencyDisplay").Text, "95.5 MHz", 
                "Frequency display should update when frequency changes");
            
            // Verify slider position updates
            float sliderValue = (float)_radioTuner.GetNode<Slider>("FrequencySlider").Value;
            float expectedValue = (_gameState.CurrentFrequency - 88.0f) / (108.0f - 88.0f) * 100;
            AssertEqual(sliderValue, expectedValue, 0.1f, 
                "Slider position should reflect current frequency");
            
            // Change frequency via RadioTuner
            _radioTuner.Call("ChangeFrequency", 0.5f);
            
            // Verify GameState frequency updates
            AssertEqual(_gameState.CurrentFrequency, 96.0f, 
                "GameState frequency should update when changed via RadioTuner");
            
            // Toggle radio via RadioTuner
            _radioTuner.Call("TogglePower");
            
            // Verify GameState radio state updates
            AssertFalse(_gameState.IsRadioOn, 
                "GameState radio state should update when toggled via RadioTuner");
            
            // Verify RadioTuner UI updates
            AssertEqual(_radioTuner.GetNode<Button>("PowerButton").Text, "OFF", 
                "Power button text should update when radio is turned off");
        }
        
        // Test signal discovery and message decoding
        [Test]
        public void TestSignalDiscoveryAndMessageDecoding()
        {
            // Turn radio on
            _gameState.ToggleRadio();
            
            // Set frequency to a known signal
            _gameState.SetFrequency(91.5f);  // This should match a signal in GameState.Signals
            
            // Process the frequency via RadioTuner
            _radioTuner.Call("ProcessFrequency");
            
            // Verify signal was detected
            AssertNotNull(_radioTuner.Get("_currentSignalId"), 
                "Signal should be detected at frequency 91.5");
            
            // Verify frequency was added to discovered frequencies
            AssertTrue(_gameState.DiscoveredFrequencies.Contains(91.5f), 
                "Frequency should be added to discovered frequencies");
            
            // Get the message ID
            string messageId = (string)_radioTuner.Get("_currentSignalId");
            
            // Verify message exists
            var message = _gameState.GetMessage(messageId);
            AssertNotNull(message, "Message should exist for the detected signal");
            
            // Verify message is not decoded yet
            AssertFalse(message.Decoded, "Message should not be decoded initially");
            
            // Decode the message
            bool decodeResult = _gameState.DecodeMessage(messageId);
            
            // Verify decode was successful
            AssertTrue(decodeResult, "Message decoding should be successful");
            
            // Verify message is now decoded
            AssertTrue(message.Decoded, "Message should be marked as decoded after decoding");
            
            // Try to decode again
            bool secondDecodeResult = _gameState.DecodeMessage(messageId);
            
            // Verify second decode fails
            AssertFalse(secondDecodeResult, "Second decode attempt should fail");
        }
        
        // Test scanning functionality with signal discovery
        [Test]
        public void TestScanningWithSignalDiscovery()
        {
            // Turn radio on
            _gameState.ToggleRadio();
            
            // Start with a frequency away from signals
            _gameState.SetFrequency(90.0f);
            
            // Start scanning
            _radioTuner.Call("ToggleScanning");
            
            // Verify scanning state
            AssertTrue((bool)_radioTuner.Get("_isScanning"), 
                "Radio should be in scanning mode");
            
            // Initial discovered frequencies count
            int initialCount = _gameState.DiscoveredFrequencies.Count;
            
            // Simulate scanning until we find a signal
            // We know there's a signal at 91.5f, so we'll scan until we reach it
            float currentFreq = _gameState.CurrentFrequency;
            while (currentFreq < 91.5f)
            {
                // Simulate scan timer timeout
                _radioTuner.Call("OnScanTimerTimeout");
                
                // Process the frequency
                _radioTuner.Call("ProcessFrequency");
                
                // Update current frequency
                currentFreq = _gameState.CurrentFrequency;
                
                // Avoid infinite loop
                if (currentFreq >= 108.0f)
                    break;
            }
            
            // Verify we found the signal
            AssertEqual(_gameState.CurrentFrequency, 91.5f, 0.1f, 
                "Scanning should stop at or near the signal frequency");
            
            // Verify the signal was discovered
            AssertGreater(_gameState.DiscoveredFrequencies.Count, initialCount, 
                "Discovered frequencies count should increase after finding a signal");
            
            // Stop scanning
            _radioTuner.Call("ToggleScanning");
            
            // Verify scanning state
            AssertFalse((bool)_radioTuner.Get("_isScanning"), 
                "Radio should not be in scanning mode after toggling");
        }
        
        // Test edge case: radio behavior at frequency boundaries
        [Test]
        public void TestFrequencyBoundaries()
        {
            // Turn radio on
            _gameState.ToggleRadio();
            
            // Test lower boundary
            _gameState.SetFrequency(88.0f);
            
            // Try to go below minimum
            _radioTuner.Call("ChangeFrequency", -0.1f);
            
            // Verify frequency is clamped
            AssertEqual(_gameState.CurrentFrequency, 88.0f, 
                "Frequency should be clamped to minimum value");
            
            // Process the frequency
            _radioTuner.Call("ProcessFrequency");
            
            // Verify radio still functions at boundary
            var staticIntensity = (float)_radioTuner.Get("_staticIntensity");
            AssertGreater(staticIntensity, 0.0f, 
                "Static intensity should be greater than zero at lower boundary");
            
            // Test upper boundary
            _gameState.SetFrequency(108.0f);
            
            // Try to go above maximum
            _radioTuner.Call("ChangeFrequency", 0.1f);
            
            // Verify frequency is clamped
            AssertEqual(_gameState.CurrentFrequency, 108.0f, 
                "Frequency should be clamped to maximum value");
            
            // Process the frequency
            _radioTuner.Call("ProcessFrequency");
            
            // Verify radio still functions at boundary
            staticIntensity = (float)_radioTuner.Get("_staticIntensity");
            AssertGreater(staticIntensity, 0.0f, 
                "Static intensity should be greater than zero at upper boundary");
        }
        
        // Test edge case: radio behavior when turning on/off while scanning
        [Test]
        public void TestRadioToggleDuringScanning()
        {
            // Turn radio on
            _gameState.ToggleRadio();
            
            // Start scanning
            _radioTuner.Call("ToggleScanning");
            
            // Verify scanning state
            AssertTrue((bool)_radioTuner.Get("_isScanning"), 
                "Radio should be in scanning mode");
            
            // Turn radio off
            _gameState.ToggleRadio();
            
            // Verify scanning stops when radio is turned off
            AssertFalse((bool)_radioTuner.Get("_isScanning"), 
                "Scanning should stop when radio is turned off");
            
            // Turn radio back on
            _gameState.ToggleRadio();
            
            // Verify scanning remains off when radio is turned back on
            AssertFalse((bool)_radioTuner.Get("_isScanning"), 
                "Scanning should remain off when radio is turned back on");
            
            // Start scanning again
            _radioTuner.Call("ToggleScanning");
            
            // Verify scanning state
            AssertTrue((bool)_radioTuner.Get("_isScanning"), 
                "Radio should be in scanning mode after toggling");
            
            // Turn radio off via RadioTuner
            _radioTuner.Call("TogglePower");
            
            // Verify scanning stops when radio is turned off
            AssertFalse((bool)_radioTuner.Get("_isScanning"), 
                "Scanning should stop when radio is turned off via RadioTuner");
        }
        
        // Test edge case: audio behavior when rapidly changing frequency
        [Test]
        public void TestRapidFrequencyChanges()
        {
            // Turn radio on
            _gameState.ToggleRadio();
            
            // Set initial frequency
            _gameState.SetFrequency(90.0f);
            
            // Process the frequency
            _radioTuner.Call("ProcessFrequency");
            
            // Rapidly change frequency multiple times
            for (int i = 0; i < 10; i++)
            {
                _gameState.SetFrequency(90.0f + i * 0.5f);
                _radioTuner.Call("ProcessFrequency");
            }
            
            // Verify audio players are in a consistent state
            var staticPlayer = (AudioStreamPlayer)_audioManager.Get("_staticPlayer");
            var signalPlayer = (AudioStreamPlayer)_audioManager.Get("_signalPlayer");
            
            // We can't assert specific playing states because it depends on the final frequency
            // But we can verify the system doesn't crash during rapid changes
            Pass("System handled rapid frequency changes without crashing");
            
            // Clean up audio
            _audioManager.StopStaticNoise();
            _audioManager.StopSignal();
        }
        
        // Test full radio tuning workflow
        [Test]
        public void TestFullRadioTuningWorkflow()
        {
            // 1. Turn radio on
            _gameState.ToggleRadio();
            AssertTrue(_gameState.IsRadioOn, "Radio should be on");
            
            // 2. Start at a non-signal frequency
            _gameState.SetFrequency(90.0f);
            _radioTuner.Call("ProcessFrequency");
            
            // Verify no signal is detected
            AssertNull(_radioTuner.Get("_currentSignalId"), 
                "No signal should be detected at frequency 90.0");
            
            // 3. Start scanning
            _radioTuner.Call("ToggleScanning");
            AssertTrue((bool)_radioTuner.Get("_isScanning"), 
                "Radio should be in scanning mode");
            
            // 4. Simulate scanning until we find a signal
            float currentFreq = _gameState.CurrentFrequency;
            while (currentFreq < 91.5f)
            {
                _radioTuner.Call("OnScanTimerTimeout");
                _radioTuner.Call("ProcessFrequency");
                currentFreq = _gameState.CurrentFrequency;
                if (currentFreq >= 108.0f) break;
            }
            
            // 5. Stop scanning when we find a signal
            _radioTuner.Call("ToggleScanning");
            AssertFalse((bool)_radioTuner.Get("_isScanning"), 
                "Scanning should be stopped");
            
            // 6. Fine-tune the frequency
            _radioTuner.Call("ChangeFrequency", 0.1f);
            _radioTuner.Call("ProcessFrequency");
            
            // 7. Check signal strength
            float signalStrength = (float)_radioTuner.Get("_signalStrength");
            AssertGreater(signalStrength, 0.0f, 
                "Signal strength should be greater than zero");
            
            // 8. View the message
            _radioTuner.Call("ToggleMessage");
            AssertTrue((bool)_radioTuner.Get("_showMessage"), 
                "Message should be displayed");
            
            // 9. Decode the message
            string messageId = (string)_radioTuner.Get("_currentSignalId");
            if (messageId != null)
            {
                bool decodeResult = _gameState.DecodeMessage(messageId);
                AssertTrue(decodeResult, "Message decoding should be successful");
            }
            
            // 10. Hide the message
            _radioTuner.Call("ToggleMessage");
            AssertFalse((bool)_radioTuner.Get("_showMessage"), 
                "Message should be hidden");
            
            // 11. Turn radio off
            _gameState.ToggleRadio();
            AssertFalse(_gameState.IsRadioOn, "Radio should be off");
            
            // Verify audio stops when radio is turned off
            var staticPlayer = (AudioStreamPlayer)_audioManager.Get("_staticPlayer");
            var signalPlayer = (AudioStreamPlayer)_audioManager.Get("_signalPlayer");
            
            AssertFalse(staticPlayer.Playing, "Static player should not be playing when radio is off");
            AssertFalse(signalPlayer.Playing, "Signal player should not be playing when radio is off");
        }
    }
}

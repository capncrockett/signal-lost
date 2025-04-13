extends "res://addons/gut/test.gd"

# Path to the scene we want to test
var RadioTunerScene = load("res://scenes/radio/RadioTuner.tscn")
var radio_tuner = null

# Called before each test
func before_each():
    # Create a new instance of the RadioTuner scene
    radio_tuner = RadioTunerScene.instance()
    add_child(radio_tuner)
    
    # Reset GameState to default values
    GameState.current_frequency = 90.0
    GameState.is_radio_on = false
    GameState.discovered_frequencies = []

# Called after each test
func after_each():
    # Clean up
    radio_tuner.queue_free()
    radio_tuner = null

# Test power button functionality
func test_power_button():
    # Initially radio should be off
    assert_false(GameState.is_radio_on, "Radio should start in OFF state")
    
    # Simulate clicking the power button
    radio_tuner.power_button.emit_signal("pressed")
    
    # Radio should now be on
    assert_true(GameState.is_radio_on, "Radio should be ON after clicking power button")
    
    # Simulate clicking the power button again
    radio_tuner.power_button.emit_signal("pressed")
    
    # Radio should now be off again
    assert_false(GameState.is_radio_on, "Radio should be OFF after clicking power button again")

# Test frequency change
func test_frequency_change():
    # Set initial frequency
    GameState.set_frequency(90.0)
    
    # Simulate changing frequency
    radio_tuner.change_frequency(0.1)
    
    # Check if frequency was updated
    assert_eq(GameState.current_frequency, 90.1, "Frequency should be 90.1 after increasing by 0.1")
    
    # Test frequency limits
    GameState.set_frequency(min_frequency)
    radio_tuner.change_frequency(-0.1)
    assert_eq(GameState.current_frequency, min_frequency, "Frequency should not go below minimum")
    
    GameState.set_frequency(max_frequency)
    radio_tuner.change_frequency(0.1)
    assert_eq(GameState.current_frequency, max_frequency, "Frequency should not go above maximum")

# Test signal detection
func test_signal_detection():
    # Turn radio on
    GameState.is_radio_on = true
    
    # Set frequency to a known signal
    GameState.set_frequency(91.5)  # This should match a signal in GameState.signals
    
    # Process the frequency
    radio_tuner.process_frequency()
    
    # Check if signal was detected
    assert_not_null(radio_tuner.current_signal_id, "Signal should be detected at frequency 91.5")
    assert_true(radio_tuner.signal_strength > 0.5, "Signal strength should be high when tuned correctly")
    
    # Check if frequency was added to discovered frequencies
    assert_true(91.5 in GameState.discovered_frequencies, "Frequency should be added to discovered frequencies")
    
    # Set frequency to a non-signal area
    GameState.set_frequency(92.5)  # This should not match any signal
    
    # Process the frequency
    radio_tuner.process_frequency()
    
    # Check that no signal was detected
    assert_null(radio_tuner.current_signal_id, "No signal should be detected at frequency 92.5")
    assert_true(radio_tuner.signal_strength < 0.2, "Signal strength should be low when no signal is present")

# Test scanning functionality
func test_scanning():
    # Turn radio on
    GameState.is_radio_on = true
    
    # Start with a known frequency
    GameState.set_frequency(90.0)
    
    # Start scanning
    radio_tuner.toggle_scanning()
    
    # Verify scanning state
    assert_true(radio_tuner.is_scanning, "Radio should be in scanning mode")
    
    # Simulate scan timer timeout
    radio_tuner._on_scan_timer_timeout()
    
    # Frequency should have increased
    assert_eq(GameState.current_frequency, 90.1, "Frequency should increase after scan timer timeout")
    
    # Stop scanning
    radio_tuner.toggle_scanning()
    
    # Verify scanning state
    assert_false(radio_tuner.is_scanning, "Radio should not be in scanning mode after toggling")

# Test message display
func test_message_display():
    # Turn radio on
    GameState.is_radio_on = true
    
    # Set frequency to a known signal
    GameState.set_frequency(91.5)  # This should match a signal in GameState.signals
    
    # Process the frequency
    radio_tuner.process_frequency()
    
    # Check if message button is enabled
    assert_false(radio_tuner.message_button.disabled, "Message button should be enabled when signal is detected")
    
    # Toggle message display
    radio_tuner.toggle_message()
    
    # Check if message is displayed
    assert_true(radio_tuner.show_message, "Message should be displayed after toggling")
    assert_true(radio_tuner.message_display.visible, "Message display should be visible")
    
    # Toggle message display again
    radio_tuner.toggle_message()
    
    # Check if message is hidden
    assert_false(radio_tuner.show_message, "Message should be hidden after toggling again")
    assert_false(radio_tuner.message_display.visible, "Message display should be hidden")

# Test radio behavior when turned off
func test_radio_off_behavior():
    # Turn radio on initially
    GameState.is_radio_on = true
    
    # Start scanning
    radio_tuner.toggle_scanning()
    
    # Turn radio off
    radio_tuner.toggle_power()
    
    # Check if scanning stopped
    assert_false(radio_tuner.is_scanning, "Scanning should stop when radio is turned off")
    
    # Try to change frequency when radio is off
    var initial_freq = GameState.current_frequency
    radio_tuner.change_frequency(0.1)
    
    # Frequency should still change even when radio is off
    assert_eq(GameState.current_frequency, initial_freq + 0.1, "Frequency should change even when radio is off")

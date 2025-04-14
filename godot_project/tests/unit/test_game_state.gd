extends BaseTest

# Unit tests for the GameState singleton

func test_frequency_setting():
	# Test setting frequency
	var initial_frequency = _game_state.get_current_frequency()
	var new_frequency = 95.5
	
	_game_state.set_frequency(new_frequency)
	assert_eq(_game_state.get_current_frequency(), new_frequency, "Frequency should be set correctly")
	
	# Test frequency clamping
	_game_state.set_frequency(87.0)
	assert_eq(_game_state.get_current_frequency(), 88.0, "Frequency should be clamped to minimum value")
	
	_game_state.set_frequency(109.0)
	assert_eq(_game_state.get_current_frequency(), 108.0, "Frequency should be clamped to maximum value")
	
	# Reset to initial frequency
	_game_state.set_frequency(initial_frequency)

func test_radio_toggle():
	# Test toggling radio
	var initial_radio_state = _game_state.is_radio_on()
	
	_game_state.toggle_radio()
	assert_eq(_game_state.is_radio_on(), !initial_radio_state, "Radio state should be toggled")
	
	# Toggle back to original state
	_game_state.toggle_radio()
	assert_eq(_game_state.is_radio_on(), initial_radio_state, "Radio state should be toggled back")

func test_signal_detection():
	# Test signal detection at known frequency
	_game_state.set_frequency(91.5)
	var signal_data = _game_state.find_signal_at_frequency(_game_state.get_current_frequency())
	
	assert_not_null(signal_data, "Signal should be detected at frequency 91.5")
	
	# Test signal detection at frequency without signal
	_game_state.set_frequency(92.5)
	signal_data = _game_state.find_signal_at_frequency(_game_state.get_current_frequency())
	
	assert_null(signal_data, "No signal should be detected at frequency 92.5")

func test_signal_strength_calculation():
	# Test signal strength calculation
	_game_state.set_frequency(91.5)
	var signal_data = _game_state.find_signal_at_frequency(_game_state.get_current_frequency())
	
	assert_not_null(signal_data, "Signal should be detected at frequency 91.5")
	
	var signal_strength = _game_state.calculate_signal_strength(_game_state.get_current_frequency(), signal_data)
	assert_gt(signal_strength, 0.9, "Signal strength should be high when tuned correctly")
	
	# Test signal strength at slightly off frequency
	var off_frequency = 91.6
	signal_strength = _game_state.calculate_signal_strength(off_frequency, signal_data)
	assert_lt(signal_strength, 0.9, "Signal strength should be lower when slightly off frequency")

func test_discovered_frequencies():
	# Test adding discovered frequencies
	var initial_count = _game_state.get_discovered_frequencies().size()
	
	_game_state.add_discovered_frequency(91.5)
	assert_eq(_game_state.get_discovered_frequencies().size(), initial_count + 1, "Discovered frequency should be added")
	
	# Test adding the same frequency again (should not increase count)
	_game_state.add_discovered_frequency(91.5)
	assert_eq(_game_state.get_discovered_frequencies().size(), initial_count + 1, "Adding the same frequency should not increase count")

func test_message_handling():
	# Test getting a message
	_game_state.set_frequency(91.5)
	var signal_data = _game_state.find_signal_at_frequency(_game_state.get_current_frequency())
	
	assert_not_null(signal_data, "Signal should be detected at frequency 91.5")
	assert_not_null(signal_data.MessageId, "Signal should have a message ID")
	
	var message = _game_state.get_message(signal_data.MessageId)
	assert_not_null(message, "Message should be found for signal")
	
	# Test message decoding
	var decode_result = _game_state.decode_message(signal_data.MessageId)
	assert_true(decode_result, "Message should be decoded successfully")
	
	# Test decoding the same message again
	decode_result = _game_state.decode_message(signal_data.MessageId)
	assert_false(decode_result, "Already decoded message should not be decoded again")
	
	# Reset message state
	_game_state.reset_message_state(signal_data.MessageId)
	
	# Test decoding after reset
	decode_result = _game_state.decode_message(signal_data.MessageId)
	assert_true(decode_result, "Message should be decoded successfully after reset")

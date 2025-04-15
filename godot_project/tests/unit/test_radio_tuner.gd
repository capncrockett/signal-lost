extends BaseTest

# Unit tests for the RadioTuner scene

var _radio_tuner = null

func before_each():
	# Call parent before_each
	super.before_each()
	
	# Load and instantiate the RadioTuner scene
	_radio_tuner = instantiate_scene("res://Scenes/Radio/RadioTuner.tscn")
	assert_not_null(_radio_tuner, "RadioTuner scene should be instantiated")

func after_each():
	# Call parent after_each
	super.after_each()
	
	# Clean up
	if _radio_tuner:
		_radio_tuner.queue_free()
		_radio_tuner = null

func test_radio_tuner_initialization():
	# Test that the RadioTuner has all required nodes
	assert_true(_radio_tuner.has_node("FrequencyDisplay"), "RadioTuner should have FrequencyDisplay node")
	assert_true(_radio_tuner.has_node("FrequencySlider"), "RadioTuner should have FrequencySlider node")
	assert_true(_radio_tuner.has_node("PowerButton"), "RadioTuner should have PowerButton node")
	assert_true(_radio_tuner.has_node("SignalStrengthMeter"), "RadioTuner should have SignalStrengthMeter node")
	assert_true(_radio_tuner.has_node("StaticVisualization"), "RadioTuner should have StaticVisualization node")
	assert_true(_radio_tuner.has_node("MessageContainer"), "RadioTuner should have MessageContainer node")
	assert_true(_radio_tuner.has_node("ScanButton"), "RadioTuner should have ScanButton node")
	assert_true(_radio_tuner.has_node("TuneDownButton"), "RadioTuner should have TuneDownButton node")
	assert_true(_radio_tuner.has_node("TuneUpButton"), "RadioTuner should have TuneUpButton node")

func test_frequency_display():
	# Test that the frequency display shows the correct frequency
	var frequency_display = _radio_tuner.get_node("FrequencyDisplay")
	assert_not_null(frequency_display, "FrequencyDisplay node should exist")
	
	var current_frequency = _game_state.get_current_frequency()
	assert_eq(frequency_display.text, "%.1f MHz" % current_frequency, "Frequency display should show current frequency")
	
	# Change frequency and check if display updates
	_game_state.set_frequency(95.5)
	await get_tree().process_frame
	assert_eq(frequency_display.text, "95.5 MHz", "Frequency display should update when frequency changes")

func test_power_button():
	# Test that the power button shows the correct state
	var power_button = _radio_tuner.get_node("PowerButton")
	assert_not_null(power_button, "PowerButton node should exist")
	
	var initial_radio_state = _game_state.is_radio_on()
	assert_eq(power_button.text, "ON" if initial_radio_state else "OFF", "Power button should show correct state")
	
	# Toggle radio and check if button updates
	_game_state.toggle_radio()
	await get_tree().process_frame
	assert_eq(power_button.text, "ON" if _game_state.is_radio_on() else "OFF", "Power button should update when radio state changes")
	
	# Test button click
	power_button.pressed.emit()
	await get_tree().process_frame
	assert_eq(_game_state.is_radio_on(), initial_radio_state, "Radio state should toggle when power button is clicked")

func test_frequency_slider():
	# Test that the frequency slider shows the correct value
	var frequency_slider = _radio_tuner.get_node("FrequencySlider")
	assert_not_null(frequency_slider, "FrequencySlider node should exist")
	
	var current_frequency = _game_state.get_current_frequency()
	var expected_value = (current_frequency - _radio_tuner.min_frequency) / (_radio_tuner.max_frequency - _radio_tuner.min_frequency) * 100
	assert_almost_eq(frequency_slider.value, expected_value, 0.1, "Frequency slider should show correct value")
	
	# Change frequency and check if slider updates
	_game_state.set_frequency(95.5)
	await get_tree().process_frame
	expected_value = (95.5 - _radio_tuner.min_frequency) / (_radio_tuner.max_frequency - _radio_tuner.min_frequency) * 100
	assert_almost_eq(frequency_slider.value, expected_value, 0.1, "Frequency slider should update when frequency changes")
	
	# Test slider change
	frequency_slider.value = 50.0
	frequency_slider.value_changed.emit(50.0)
	await get_tree().process_frame
	var expected_frequency = _radio_tuner.min_frequency + (50.0 / 100.0) * (_radio_tuner.max_frequency - _radio_tuner.min_frequency)
	assert_almost_eq(_game_state.get_current_frequency(), expected_frequency, 0.1, "Frequency should change when slider is moved")

func test_tune_buttons():
	# Test that the tune buttons change the frequency
	var tune_down_button = _radio_tuner.get_node("TuneDownButton")
	var tune_up_button = _radio_tuner.get_node("TuneUpButton")
	assert_not_null(tune_down_button, "TuneDownButton node should exist")
	assert_not_null(tune_up_button, "TuneUpButton node should exist")
	
	# Set initial frequency
	_game_state.set_frequency(95.5)
	await get_tree().process_frame
	
	# Test tune down button
	tune_down_button.pressed.emit()
	await get_tree().process_frame
	assert_almost_eq(_game_state.get_current_frequency(), 95.4, 0.01, "Frequency should decrease when tune down button is clicked")
	
	# Test tune up button
	tune_up_button.pressed.emit()
	await get_tree().process_frame
	assert_almost_eq(_game_state.get_current_frequency(), 95.5, 0.01, "Frequency should increase when tune up button is clicked")

func test_scan_button():
	# Test that the scan button starts and stops scanning
	var scan_button = _radio_tuner.get_node("ScanButton")
	assert_not_null(scan_button, "ScanButton node should exist")
	
	# Ensure radio is on
	if not _game_state.is_radio_on():
		_game_state.toggle_radio()
		await get_tree().process_frame
	
	# Test scan button click
	scan_button.pressed.emit()
	await get_tree().process_frame
	assert_eq(scan_button.text, "Stop Scan", "Scan button should show 'Stop Scan' when scanning")
	
	# Stop scanning
	scan_button.pressed.emit()
	await get_tree().process_frame
	assert_eq(scan_button.text, "Scan", "Scan button should show 'Scan' when not scanning")

func test_message_button():
	# Test that the message button shows and hides the message
	var message_container = _radio_tuner.get_node("MessageContainer")
	var message_button = _radio_tuner.get_node("MessageContainer/MessageButton")
	var message_display = _radio_tuner.get_node("MessageContainer/MessageDisplay")
	assert_not_null(message_container, "MessageContainer node should exist")
	assert_not_null(message_button, "MessageButton node should exist")
	assert_not_null(message_display, "MessageDisplay node should exist")
	
	# Ensure radio is on
	if not _game_state.is_radio_on():
		_game_state.toggle_radio()
		await get_tree().process_frame
	
	# Tune to a frequency with a signal
	_game_state.set_frequency(91.5)
	await get_tree().process_frame
	await get_tree().process_frame
	
	# Check if message container is visible
	assert_true(message_container.visible, "Message container should be visible when tuned to a frequency with a signal")
	
	# Check if message button is enabled
	assert_false(message_button.disabled, "Message button should be enabled when tuned to a frequency with a signal")
	
	# Test message button click
	message_button.pressed.emit()
	await get_tree().process_frame
	assert_true(message_display.visible, "Message display should be visible when message button is clicked")
	assert_eq(message_button.text, "Hide Message", "Message button should show 'Hide Message' when message is visible")
	
	# Test message button click again
	message_button.pressed.emit()
	await get_tree().process_frame
	assert_false(message_display.visible, "Message display should be hidden when message button is clicked again")
	assert_eq(message_button.text, "Show Message", "Message button should show 'Show Message' when message is hidden")

extends BaseTest

# Integration tests for the RadioTuner with GameState and AudioManager

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

func test_radio_tuner_responds_to_game_state_changes():
	# Test that the RadioTuner UI updates when GameState changes
	
	# Ensure radio is off
	if _game_state.is_radio_on():
		_game_state.toggle_radio()
		await get_tree().process_frame
	
	# Check initial state
	var power_button = _radio_tuner.get_node("PowerButton")
	assert_eq(power_button.text, "OFF", "Power button should show 'OFF' when radio is off")
	
	# Turn radio on
	_game_state.toggle_radio()
	await get_tree().process_frame
	assert_eq(power_button.text, "ON", "Power button should show 'ON' when radio is turned on")
	
	# Change frequency
	_game_state.set_frequency(95.5)
	await get_tree().process_frame
	var frequency_display = _radio_tuner.get_node("FrequencyDisplay")
	assert_eq(frequency_display.text, "95.5 MHz", "Frequency display should update when frequency changes")

func test_game_state_responds_to_radio_tuner_actions():
	# Test that the GameState updates when RadioTuner actions are performed
	
	# Ensure radio is off
	if _game_state.is_radio_on():
		_game_state.toggle_radio()
		await get_tree().process_frame
	
	# Click power button
	var power_button = _radio_tuner.get_node("PowerButton")
	power_button.pressed.emit()
	await get_tree().process_frame
	assert_true(_game_state.is_radio_on(), "Radio should be turned on when power button is clicked")
	
	# Move frequency slider
	var frequency_slider = _radio_tuner.get_node("FrequencySlider")
	frequency_slider.value = 50.0
	frequency_slider.value_changed.emit(50.0)
	await get_tree().process_frame
	var expected_frequency = _radio_tuner.min_frequency + (50.0 / 100.0) * (_radio_tuner.max_frequency - _radio_tuner.min_frequency)
	assert_almost_eq(_game_state.get_current_frequency(), expected_frequency, 0.1, "Frequency should change when slider is moved")

func test_signal_detection_in_radio_tuner():
	# Test that the RadioTuner detects signals correctly
	
	# Ensure radio is on
	if not _game_state.is_radio_on():
		_game_state.toggle_radio()
		await get_tree().process_frame
	
	# Tune to a frequency with a signal
	_game_state.set_frequency(91.5)
	await get_tree().process_frame
	await get_tree().process_frame
	
	# Check if signal strength meter shows a high value
	var signal_strength_meter = _radio_tuner.get_node("SignalStrengthMeter")
	assert_gt(signal_strength_meter.value, 90.0, "Signal strength meter should show a high value when tuned to a signal")
	
	# Check if message container is visible
	var message_container = _radio_tuner.get_node("MessageContainer")
	assert_true(message_container.visible, "Message container should be visible when tuned to a frequency with a signal")
	
	# Tune to a frequency without a signal
	_game_state.set_frequency(92.5)
	await get_tree().process_frame
	await get_tree().process_frame
	
	# Check if signal strength meter shows a low value
	assert_lt(signal_strength_meter.value, 20.0, "Signal strength meter should show a low value when not tuned to a signal")
	
	# Check if message container is hidden
	assert_false(message_container.visible, "Message container should be hidden when not tuned to a frequency with a signal")

func test_audio_visualization():
	# Test that the audio visualizer responds to signal strength and static intensity
	
	# Ensure radio is on
	if not _game_state.is_radio_on():
		_game_state.toggle_radio()
		await get_tree().process_frame
	
	# Get the audio visualizer
	var audio_visualizer = _radio_tuner.get_node("StaticVisualization")
	assert_not_null(audio_visualizer, "StaticVisualization node should exist")
	assert_true(audio_visualizer.has_method("set_signal_strength"), "StaticVisualization should have set_signal_strength method")
	
	# Tune to a frequency with a signal
	_game_state.set_frequency(91.5)
	await get_tree().process_frame
	await get_tree().process_frame
	
	# Check if audio visualizer has high signal strength and low static intensity
	assert_gt(audio_visualizer._signal_strength, 0.9, "Audio visualizer should have high signal strength when tuned to a signal")
	assert_lt(audio_visualizer._static_intensity, 0.1, "Audio visualizer should have low static intensity when tuned to a signal")
	
	# Tune to a frequency without a signal
	_game_state.set_frequency(92.5)
	await get_tree().process_frame
	await get_tree().process_frame
	
	# Check if audio visualizer has low signal strength and high static intensity
	assert_lt(audio_visualizer._signal_strength, 0.2, "Audio visualizer should have low signal strength when not tuned to a signal")
	assert_gt(audio_visualizer._static_intensity, 0.8, "Audio visualizer should have high static intensity when not tuned to a signal")

func test_full_radio_workflow():
	# Test the full radio workflow
	
	# 1. Start with radio off
	if _game_state.is_radio_on():
		_game_state.toggle_radio()
		await get_tree().process_frame
	
	# 2. Turn radio on
	var power_button = _radio_tuner.get_node("PowerButton")
	power_button.pressed.emit()
	await get_tree().process_frame
	assert_true(_game_state.is_radio_on(), "Radio should be turned on")
	
	# 3. Set initial frequency
	_game_state.set_frequency(90.0)
	await get_tree().process_frame
	
	# 4. Scan for signals
	var scan_button = _radio_tuner.get_node("ScanButton")
	scan_button.pressed.emit()
	await get_tree().process_frame
	
	# Wait for scanning to find a signal (simulated)
	_game_state.set_frequency(91.5)
	await get_tree().process_frame
	
	# Stop scanning
	scan_button.pressed.emit()
	await get_tree().process_frame
	
	# 5. Check signal strength
	var signal_strength_meter = _radio_tuner.get_node("SignalStrengthMeter")
	assert_gt(signal_strength_meter.value, 90.0, "Signal strength meter should show a high value when tuned to a signal")
	
	# 6. Show message
	var message_button = _radio_tuner.get_node("MessageContainer/MessageButton")
	message_button.pressed.emit()
	await get_tree().process_frame
	
	# Check if message is displayed
	var message_display = _radio_tuner.get_node("MessageContainer/MessageDisplay")
	assert_true(message_display.visible, "Message display should be visible")
	assert_not_eq(message_display.text, "", "Message display should not be empty")
	
	# 7. Hide message
	message_button.pressed.emit()
	await get_tree().process_frame
	assert_false(message_display.visible, "Message display should be hidden")
	
	# 8. Turn radio off
	power_button.pressed.emit()
	await get_tree().process_frame
	assert_false(_game_state.is_radio_on(), "Radio should be turned off")

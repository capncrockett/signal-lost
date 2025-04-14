extends Node

# This is a simple test runner that doesn't rely on inheritance
# It will run basic tests on the core components

func _ready():
	print("Starting simple test runner...")

	# Run the tests
	run_tests()

	# Exit the application
	if not OS.has_feature("editor"):
		get_tree().quit()

func run_tests():
	print("\n===== RUNNING TESTS =====")

	# Get references to singletons
	var game_state = get_node("/root/GameState")
	var audio_manager = get_node("/root/AudioManager")

	if game_state == null or audio_manager == null:
		print("ERROR: Could not find GameState or AudioManager singletons")
		return

	print("Found GameState and AudioManager singletons")

	# Test GameState
	test_game_state(game_state)

	# Test AudioManager
	test_audio_manager(audio_manager)

	# Test integration
	test_integration(game_state, audio_manager)

	print("\n===== TESTS COMPLETED =====")

func test_game_state(game_state):
	print("\n----- Testing GameState -----")

	# Setup
	game_state.set_frequency(90.0)
	if game_state.is_radio_on():
		game_state.toggle_radio() # Ensure it's off

	# Test frequency setting
	var initial_freq = game_state.get_current_frequency()
	game_state.set_frequency(95.5)
	var new_freq = game_state.get_current_frequency()

	if new_freq == 95.5:
		print("PASS: Frequency setting works")
	else:
		print("FAIL: Frequency setting doesn't work. Expected 95.5, got " + str(new_freq))

	# Test radio toggle
	var initial_state = game_state.is_radio_on()
	game_state.toggle_radio()
	var new_state = game_state.is_radio_on()

	if new_state != initial_state:
		print("PASS: Radio toggle works")
	else:
		print("FAIL: Radio toggle doesn't work")

	# Test signal detection
	var signal_data = game_state.find_signal_at_frequency(91.5)

	if signal_data != null:
		print("PASS: Signal detection works")
	else:
		print("FAIL: Signal detection doesn't work")

	# Test signal strength calculation
	if signal_data != null:
		var strength = game_state.calculate_signal_strength(91.5, signal_data)

		if strength > 0.9:
			print("PASS: Signal strength calculation works")
		else:
			print("FAIL: Signal strength calculation doesn't work. Expected > 0.9, got " + str(strength))

	# Reset state
	game_state.set_frequency(initial_freq)
	if game_state.is_radio_on() != initial_state:
		game_state.toggle_radio()

func test_audio_manager(audio_manager):
	print("\n----- Testing AudioManager -----")

	# Test volume setting
	audio_manager.set_volume(0.5)
	print("PASS: Volume setting doesn't crash")

	# Test mute toggle
	audio_manager.toggle_mute()
	print("PASS: Mute toggle doesn't crash")
	audio_manager.toggle_mute() # Toggle back

	# Test static noise playback
	audio_manager.play_static_noise(0.5)
	print("PASS: Static noise playback doesn't crash")
	audio_manager.stop_static_noise()

	# Test signal playback
	audio_manager.play_signal(440.0)
	print("PASS: Signal playback doesn't crash")
	audio_manager.stop_signal()

func test_integration(game_state, audio_manager):
	print("\n----- Testing Integration -----")

	# Load the RadioTuner scene
	var radio_tuner_scene = load("res://Scenes/Radio/RadioTuner.tscn")

	if radio_tuner_scene == null:
		print("FAIL: Could not load RadioTuner scene")
		return

	print("PASS: RadioTuner scene loaded")

	# Create an instance of the RadioTuner
	var radio_tuner = radio_tuner_scene.instantiate()
	add_child(radio_tuner)

	print("PASS: RadioTuner instantiated")

	# Test basic functionality
	var initial_state = game_state.is_radio_on()

	# Turn radio on if it's off
	if !initial_state:
		game_state.toggle_radio()

	# Set frequency to a known signal
	game_state.set_frequency(91.5)

	# Process the frequency
	radio_tuner.call("_process_frequency")

	# Check if signal was detected
	var current_signal_id = radio_tuner.get("_current_signal_id")

	if current_signal_id != null:
		print("PASS: Signal detection in RadioTuner works")
	else:
		print("FAIL: Signal detection in RadioTuner doesn't work")

	# Clean up
	radio_tuner.queue_free()

	# Reset state
	if game_state.is_radio_on() != initial_state:
		game_state.toggle_radio()

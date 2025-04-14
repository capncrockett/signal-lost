extends GutTest
class_name BaseTest

# Base class for all tests
# Provides common functionality and utilities

# References to singletons
var _game_state = null
var _audio_manager = null

# Called before each test
func before_each():
	# Get references to singletons
	_game_state = get_node_or_null("/root/GameState")
	if _game_state == null:
		_game_state = get_node_or_null("/root/GameStateWrapper")
	
	_audio_manager = get_node_or_null("/root/AudioManager")
	if _audio_manager == null:
		_audio_manager = get_node_or_null("/root/AudioManagerWrapper")
	
	# Verify that singletons are available
	assert_not_null(_game_state, "GameState singleton should be available")
	assert_not_null(_audio_manager, "AudioManager singleton should be available")

# Called after each test
func after_each():
	# Reset game state
	if _game_state:
		# Reset frequency
		_game_state.set_frequency(90.0)
		
		# Turn radio off
		if _game_state.is_radio_on():
			_game_state.toggle_radio()
	
	# Reset audio manager
	if _audio_manager:
		# Stop all audio
		_audio_manager.stop_signal()
		_audio_manager.stop_static_noise()
		
		# Reset volume
		_audio_manager.set_volume(1.0)
		
		# Unmute
		if _audio_manager.is_muted():
			_audio_manager.toggle_mute()

# Helper method to load a scene
func load_scene(path: String):
	var scene = load(path)
	assert_not_null(scene, "Scene should load: %s" % path)
	return scene

# Helper method to instantiate a scene
func instantiate_scene(path: String):
	var scene = load_scene(path)
	var instance = scene.instantiate()
	assert_not_null(instance, "Scene should instantiate: %s" % path)
	add_child_autofree(instance)
	return instance

# Helper method to find a signal at a specific frequency
func find_signal_at_frequency(frequency: float):
	assert_not_null(_game_state, "GameState should be available")
	return _game_state.find_signal_at_frequency(frequency)

# Helper method to check if a frequency has a signal
func assert_has_signal_at_frequency(frequency: float, message: String = ""):
	var signal_data = find_signal_at_frequency(frequency)
	assert_not_null(signal_data, message if message else "Should have a signal at frequency %.1f" % frequency)
	return signal_data

# Helper method to check if a frequency does not have a signal
func assert_no_signal_at_frequency(frequency: float, message: String = ""):
	var signal_data = find_signal_at_frequency(frequency)
	assert_null(signal_data, message if message else "Should not have a signal at frequency %.1f" % frequency)

extends BaseTest

# Unit tests for the AudioManager singleton

func test_volume_setting():
	# Test setting volume
	var initial_volume = _audio_manager.get_volume()
	
	_audio_manager.set_volume(0.5)
	assert_eq(_audio_manager.get_volume(), 0.5, "Volume should be set correctly")
	
	# Test volume clamping
	_audio_manager.set_volume(1.5)
	assert_eq(_audio_manager.get_volume(), 1.0, "Volume should be clamped to maximum value")
	
	_audio_manager.set_volume(-0.5)
	assert_eq(_audio_manager.get_volume(), 0.0, "Volume should be clamped to minimum value")
	
	# Reset to initial volume
	_audio_manager.set_volume(initial_volume)

func test_mute_toggle():
	# Test toggling mute
	var initial_mute_state = _audio_manager.is_muted()
	
	_audio_manager.toggle_mute()
	assert_eq(_audio_manager.is_muted(), !initial_mute_state, "Mute state should be toggled")
	
	# Toggle back to original state
	_audio_manager.toggle_mute()
	assert_eq(_audio_manager.is_muted(), initial_mute_state, "Mute state should be toggled back")

func test_static_noise_playback():
	# Test playing static noise
	_audio_manager.play_static_noise(0.5)
	
	# We can't easily test audio output, so we'll just check that it doesn't crash
	assert_true(true, "Static noise playback should not crash")
	
	# Test stopping static noise
	_audio_manager.stop_static_noise()
	assert_true(true, "Stopping static noise should not crash")

func test_signal_playback():
	# Test playing signal
	_audio_manager.play_signal(915.0, 0.8)
	
	# We can't easily test audio output, so we'll just check that it doesn't crash
	assert_true(true, "Signal playback should not crash")
	
	# Test stopping signal
	_audio_manager.stop_signal()
	assert_true(true, "Stopping signal should not crash")

func test_audio_bus_manipulation():
	# Test getting master bus index
	var master_bus_index = _audio_manager.get_master_bus_index()
	assert_eq(master_bus_index, 0, "Master bus index should be 0")
	
	# Test setting volume on master bus
	var initial_volume_db = AudioServer.get_bus_volume_db(master_bus_index)
	
	# Convert linear volume to dB
	var test_volume = 0.5
	var test_volume_db = linear_to_db(test_volume)
	
	_audio_manager.set_volume(test_volume)
	assert_almost_eq(AudioServer.get_bus_volume_db(master_bus_index), test_volume_db, 0.1, "Master bus volume should be set correctly")
	
	# Reset to initial volume
	AudioServer.set_bus_volume_db(master_bus_index, initial_volume_db)

extends Node

func _ready():
	print("Starting comprehensive test runner...")
	
	# Run basic tests
	run_basic_tests()
	
	# Run GameState tests
	run_game_state_tests()
	
	# Run AudioManager tests
	run_audio_manager_tests()
	
	print("All tests completed!")
	
	# Exit the application
	get_tree().quit()

func run_basic_tests():
	print("\nRunning basic tests...")
	
	# Test math operations
	var a = 5
	var b = 10
	var c = a + b
	
	if c == 15:
		print("PASS: Basic math operations work")
	else:
		print("FAIL: Basic math operations don't work")
	
	# Test string operations
	var str1 = "Hello"
	var str2 = "World"
	var str3 = str1 + " " + str2
	
	if str3 == "Hello World":
		print("PASS: String operations work")
	else:
		print("FAIL: String operations don't work")

func run_game_state_tests():
	print("\nRunning GameState tests...")
	
	# Get the GameState autoload
	var game_state = get_node("/root/GameState")
	
	if game_state:
		print("GameState autoload found")
		
		# Test frequency setting
		var initial_frequency = game_state.get_current_frequency()
		var new_frequency = 95.5
		
		game_state.set_frequency(new_frequency)
		
		if game_state.get_current_frequency() == new_frequency:
			print("PASS: Frequency was set correctly")
		else:
			print("FAIL: Frequency was not set correctly")
		
		# Test radio toggle
		var initial_radio_state = game_state.is_radio_on()
		
		game_state.toggle_radio()
		
		if game_state.is_radio_on() != initial_radio_state:
			print("PASS: Radio state was toggled correctly")
		else:
			print("FAIL: Radio state was not toggled correctly")
		
		# Toggle back to original state
		game_state.toggle_radio()
	else:
		print("ERROR: GameState autoload not found")

func run_audio_manager_tests():
	print("\nRunning AudioManager tests...")
	
	# Get the AudioManager autoload
	var audio_manager = get_node("/root/AudioManager")
	
	if audio_manager:
		print("AudioManager autoload found")
		
		# Test volume setting
		audio_manager.set_volume(0.5)
		print("PASS: Volume was set without errors")
		
		# Test mute toggle
		audio_manager.toggle_mute()
		print("PASS: Mute was toggled without errors")
		
		# Toggle back to original state
		audio_manager.toggle_mute()
	else:
		print("ERROR: AudioManager autoload not found")

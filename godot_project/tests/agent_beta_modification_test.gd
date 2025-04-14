extends Node

func _ready():
	print("Agent Beta modification test is running!")
	
	# Get the GameState wrapper
	var game_state_wrapper = get_node_or_null("/root/GameState")
	if game_state_wrapper:
		print("GameState wrapper found")
		
		# Test our new function
		if game_state_wrapper.has_method("get_agent_beta_test_info"):
			var test_info = game_state_wrapper.get_agent_beta_test_info()
			print("Successfully called our new function!")
			print("Agent: ", test_info.agent)
			print("Role: ", test_info.role)
			print("Test time: ", test_info.test_time)
		else:
			print("ERROR: Our new function was not found!")
	else:
		print("ERROR: GameState wrapper not found!")
	
	# Test radio functionality
	test_radio_functionality(game_state_wrapper)
	
	print("Agent Beta modification test completed!")
	
	# Exit after tests complete
	get_tree().quit()

func test_radio_functionality(game_state):
	if not game_state:
		print("ERROR: Cannot test radio functionality without GameState")
		return
		
	print("\nTesting radio functionality...")
	
	# Make sure radio is off
	if game_state.is_radio_on():
		game_state.toggle_radio()
		print("Radio turned off for testing")
	
	# Turn radio on
	game_state.toggle_radio()
	print("Radio turned on: ", game_state.is_radio_on())
	
	# Set frequency to a known signal
	game_state.set_frequency(91.5)
	print("Frequency set to 91.5")
	
	# Check for signal
	var signal_data = game_state.find_signal_at_frequency(91.5)
	if signal_data:
		print("Signal found: ", signal_data.Name)
		
		# Calculate signal strength
		var strength = game_state.calculate_signal_strength(91.5, signal_data)
		print("Signal strength: ", strength)
		
		# Get message
		var message = game_state.get_message(signal_data.MessageId)
		if message:
			print("Message found: ", message.Content)
		else:
			print("No message found")
	else:
		print("No signal found at 91.5")
	
	# Turn radio off
	game_state.toggle_radio()
	print("Radio turned off: ", not game_state.is_radio_on())
	
	print("Radio functionality test completed")

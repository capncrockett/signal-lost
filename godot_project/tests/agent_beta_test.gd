extends Node

func _ready():
	print("Agent Beta test script is running!")
	print("Testing console output capabilities...")
	
	# Test basic output
	print("Basic output test: SUCCESS")
	
	# Test error output
	push_error("This is a test error message - can be ignored")
	
	# Test warning output
	push_warning("This is a test warning message - can be ignored")
	
	# Test array/dictionary output
	var test_array = [1, 2, 3, 4, 5]
	var test_dict = {"name": "Agent Beta", "role": "QA Developer"}
	print("Array output test:", test_array)
	print("Dictionary output test:", test_dict)
	
	# Test accessing game state
	var game_state = get_node_or_null("/root/GameState")
	if game_state:
		print("GameState found:", game_state)
		print("Current frequency:", game_state.get_current_frequency())
	else:
		print("GameState not found!")
	
	# Test accessing audio manager
	var audio_manager = get_node_or_null("/root/AudioManager")
	if audio_manager:
		print("AudioManager found:", audio_manager)
	else:
		print("AudioManager not found!")
	
	print("Agent Beta test completed successfully!")
	
	# Exit after tests complete
	get_tree().quit()

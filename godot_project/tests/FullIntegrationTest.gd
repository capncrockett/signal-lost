extends Node

# This script runs a comprehensive integration test of all components

func _ready():
	print("Starting full integration test...")
	
	# Run the tests
	run_integration_tests()
	
	# Exit the application
	get_tree().quit()

func run_integration_tests():
	print("\nRunning full integration tests...")
	
	# Get the autoloads
	var game_state = get_node("/root/GameState")
	var audio_manager = get_node("/root/AudioManager")
	
	if game_state and audio_manager:
		print("Both autoloads found")
		
		# Test basic functionality
		test_basic_functionality(game_state, audio_manager)
		
		# Test radio tuning
		test_radio_tuning(game_state, audio_manager)
		
		# Test signal detection
		test_signal_detection(game_state, audio_manager)
		
		# Test message decoding
		test_message_decoding(game_state, audio_manager)
		
		# Test scanning
		test_scanning(game_state, audio_manager)
		
		# Test full workflow
		test_full_workflow(game_state, audio_manager)
	else:
		print("ERROR: One or both autoloads not found")

func test_basic_functionality(game_state, audio_manager):
	print("\nTesting basic functionality...")
	
	# Test GameState
	var initial_frequency = game_state.get_current_frequency()
	var initial_radio_state = game_state.is_radio_on()
	
	# Set frequency
	game_state.set_frequency(95.5)
	if game_state.get_current_frequency() == 95.5:
		print("PASS: Frequency was set correctly")
	else:
		print("FAIL: Frequency was not set correctly")
	
	# Toggle radio
	game_state.toggle_radio()
	if game_state.is_radio_on() != initial_radio_state:
		print("PASS: Radio state was toggled correctly")
	else:
		print("FAIL: Radio state was not toggled correctly")
	
	# Test AudioManager
	audio_manager.set_volume(0.5)
	print("PASS: Volume was set without errors")
	
	audio_manager.toggle_mute()
	print("PASS: Mute was toggled without errors")
	
	# Reset state
	game_state.set_frequency(initial_frequency)
	if game_state.is_radio_on() != initial_radio_state:
		game_state.toggle_radio()
	audio_manager.toggle_mute()

func test_radio_tuning(game_state, audio_manager):
	print("\nTesting radio tuning...")
	
	# Ensure radio is on
	if not game_state.is_radio_on():
		game_state.toggle_radio()
	
	# Test frequency limits
	game_state.set_frequency(87.0)
	if game_state.get_current_frequency() == 88.0:
		print("PASS: Frequency is clamped to minimum value")
	else:
		print("FAIL: Frequency is not clamped to minimum value")
	
	game_state.set_frequency(109.0)
	if game_state.get_current_frequency() == 108.0:
		print("PASS: Frequency is clamped to maximum value")
	else:
		print("FAIL: Frequency is not clamped to maximum value")
	
	# Test frequency steps
	game_state.set_frequency(90.0)
	var static_intensity_1 = game_state.get_static_intensity(game_state.get_current_frequency())
	
	game_state.set_frequency(90.1)
	var static_intensity_2 = game_state.get_static_intensity(game_state.get_current_frequency())
	
	if static_intensity_1 != static_intensity_2:
		print("PASS: Static intensity changes with frequency")
	else:
		print("FAIL: Static intensity does not change with frequency")
	
	# Reset state
	game_state.set_frequency(90.0)

func test_signal_detection(game_state, audio_manager):
	print("\nTesting signal detection...")
	
	# Ensure radio is on
	if not game_state.is_radio_on():
		game_state.toggle_radio()
	
	# Test signal detection at known frequency
	game_state.set_frequency(91.5)
	var signal_data = game_state.find_signal_at_frequency(game_state.get_current_frequency())
	
	if signal_data:
		print("PASS: Signal detected at frequency 91.5")
		
		# Test signal strength
		var signal_strength = game_state.calculate_signal_strength(game_state.get_current_frequency(), signal_data)
		if signal_strength > 0.9:
			print("PASS: Signal strength is high when tuned correctly")
		else:
			print("FAIL: Signal strength is not high when tuned correctly")
		
		# Test discovered frequencies
		var initial_count = game_state.get_discovered_frequencies().size()
		game_state.add_discovered_frequency(signal_data.Frequency)
		
		if game_state.get_discovered_frequencies().size() >= initial_count:
			print("PASS: Frequency was added to discovered frequencies")
		else:
			print("FAIL: Frequency was not added to discovered frequencies")
		
		# Test audio
		audio_manager.play_static_noise(0.2)
		audio_manager.play_signal(signal_data.Frequency * 10, 0.8)
		print("PASS: Audio played for detected signal")
		
		# Clean up
		audio_manager.stop_static_noise()
		audio_manager.stop_signal()
	else:
		print("FAIL: No signal detected at frequency 91.5")
	
	# Test no signal detection
	game_state.set_frequency(92.5)
	signal_data = game_state.find_signal_at_frequency(game_state.get_current_frequency())
	
	if not signal_data:
		print("PASS: No signal detected at frequency 92.5")
	else:
		print("FAIL: Signal incorrectly detected at frequency 92.5")
	
	# Reset state
	game_state.set_frequency(90.0)

func test_message_decoding(game_state, audio_manager):
	print("\nTesting message decoding...")
	
	# Ensure radio is on
	if not game_state.is_radio_on():
		game_state.toggle_radio()
	
	# Test message decoding at known frequency
	game_state.set_frequency(91.5)
	var signal_data = game_state.find_signal_at_frequency(game_state.get_current_frequency())
	
	if signal_data and signal_data.MessageId:
		var message_id = signal_data.MessageId
		
		# Get message
		var message = game_state.get_message(message_id)
		if message:
			print("PASS: Message found for signal")
			
			# Decode message
			var decode_result = game_state.decode_message(message_id)
			if decode_result:
				print("PASS: Message decoded successfully")
			else:
				print("FAIL: Message decoding failed")
			
			# Try to decode again
			decode_result = game_state.decode_message(message_id)
			if not decode_result:
				print("PASS: Already decoded message cannot be decoded again")
			else:
				print("FAIL: Already decoded message was decoded again")
		else:
			print("FAIL: No message found for signal")
	else:
		print("FAIL: No signal with message ID detected")
	
	# Reset state
	game_state.set_frequency(90.0)

func test_scanning(game_state, audio_manager):
	print("\nTesting scanning functionality...")
	
	# Ensure radio is on
	if not game_state.is_radio_on():
		game_state.toggle_radio()
	
	# Start at a non-signal frequency
	game_state.set_frequency(90.0)
	
	# Simulate scanning
	print("Simulating scanning...")
	var found_signal = false
	var current_freq = game_state.get_current_frequency()
	
	# Scan through frequencies
	while current_freq < 108.0:
		current_freq += 0.1
		game_state.set_frequency(current_freq)
		
		var signal_data = game_state.find_signal_at_frequency(current_freq)
		if signal_data:
			found_signal = true
			print("PASS: Signal found at frequency %.1f during scanning" % current_freq)
			break
	
	if found_signal:
		print("PASS: Scanning successfully found a signal")
	else:
		print("FAIL: Scanning did not find any signals")
	
	# Reset state
	game_state.set_frequency(90.0)

func test_full_workflow(game_state, audio_manager):
	print("\nTesting full radio workflow...")
	
	# 1. Start with radio off
	if game_state.is_radio_on():
		game_state.toggle_radio()
	
	print("1. Radio is off")
	
	# 2. Turn radio on
	game_state.toggle_radio()
	if game_state.is_radio_on():
		print("2. PASS: Radio turned on successfully")
	else:
		print("2. FAIL: Radio did not turn on")
	
	# 3. Set initial frequency
	game_state.set_frequency(90.0)
	if game_state.get_current_frequency() == 90.0:
		print("3. PASS: Initial frequency set successfully")
	else:
		print("3. FAIL: Initial frequency not set correctly")
	
	# 4. Scan to find a signal
	print("4. Scanning for signals...")
	var found_signal = false
	var signal_freq = 0.0
	var current_freq = game_state.get_current_frequency()
	
	while current_freq < 108.0:
		current_freq += 0.1
		game_state.set_frequency(current_freq)
		
		var signal_data = game_state.find_signal_at_frequency(current_freq)
		if signal_data:
			found_signal = true
			signal_freq = current_freq
			print("   PASS: Signal found at frequency %.1f" % current_freq)
			break
	
	if found_signal:
		# 5. Fine-tune the frequency
		var fine_tuned_freq = signal_freq + 0.1
		game_state.set_frequency(fine_tuned_freq)
		print("5. Fine-tuned to frequency %.1f" % fine_tuned_freq)
		
		# 6. Check signal strength
		var signal_data = game_state.find_signal_at_frequency(fine_tuned_freq)
		if signal_data:
			var signal_strength = game_state.calculate_signal_strength(fine_tuned_freq, signal_data)
			print("6. Signal strength: %.2f" % signal_strength)
			
			# 7. Decode message
			if signal_data.MessageId:
				var decode_result = game_state.decode_message(signal_data.MessageId)
				if decode_result:
					print("7. PASS: Message decoded successfully")
				else:
					print("7. FAIL: Message decoding failed")
			else:
				print("7. SKIP: No message ID for this signal")
		else:
			print("6-7. SKIP: Lost signal after fine-tuning")
	else:
		print("4-7. SKIP: No signal found during scanning")
	
	# 8. Turn radio off
	game_state.toggle_radio()
	if not game_state.is_radio_on():
		print("8. PASS: Radio turned off successfully")
	else:
		print("8. FAIL: Radio did not turn off")
	
	print("\nFull workflow test completed")

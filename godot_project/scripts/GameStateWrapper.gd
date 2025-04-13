extends Node

# This script is a wrapper for the GameState C# class
# It will be used as an autoload

var _game_state = null

func _ready():
	print("GameStateWrapper is initializing...")
	
	# Try to load the C# script
	var game_state_script = load("res://scripts/GameState.cs")
	if game_state_script:
		print("GameState script loaded successfully")
		
		# Try to instantiate GameState
		_game_state = game_state_script.new()
		if _game_state:
			print("GameState instantiated successfully")
			add_child(_game_state)
		else:
			print("ERROR: Failed to instantiate GameState")
	else:
		print("ERROR: Failed to load GameState script")

# Forward properties and methods to the C# instance
func get_current_frequency():
	return _game_state.CurrentFrequency if _game_state else 90.0

func is_radio_on():
	return _game_state.IsRadioOn if _game_state else false

func get_discovered_frequencies():
	return _game_state.DiscoveredFrequencies if _game_state else []

func set_frequency(freq):
	if _game_state:
		_game_state.SetFrequency(freq)
		return true
	return false

func toggle_radio():
	if _game_state:
		_game_state.ToggleRadio()
		return true
	return false

func find_signal_at_frequency(freq):
	return _game_state.FindSignalAtFrequency(freq) if _game_state else null

func calculate_signal_strength(freq, signal_data):
	return _game_state.CalculateSignalStrength(freq, signal_data) if _game_state else 0.0

func get_static_intensity(freq):
	return _game_state.GetStaticIntensity(freq) if _game_state else 0.5

func add_discovered_frequency(freq):
	if _game_state:
		_game_state.AddDiscoveredFrequency(freq)
		return true
	return false

func decode_message(message_id):
	return _game_state.DecodeMessage(message_id) if _game_state else false

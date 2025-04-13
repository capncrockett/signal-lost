extends Node

# This script is a wrapper for the GameState C# class
# It will be used as an autoload

var _game_state = null

func _ready():
	print("GameStateWrapper is initializing...")

	# Since we can't directly instantiate C# scripts from GDScript,
	# we'll create a dummy implementation for testing

	# Create a basic object to simulate GameState
	_game_state = Node.new()
	_game_state.name = "GameState"
	add_child(_game_state)

	# Add properties
	_game_state.set("CurrentFrequency", 90.0)
	_game_state.set("IsRadioOn", false)
	_game_state.set("DiscoveredFrequencies", [])

	# Add methods
	_game_state.set_script(create_script_with_methods())

	print("Dummy GameState created for testing")

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

func get_message(message_id):
	return _game_state.GetMessage(message_id) if _game_state else null

func reset_message_state(message_id):
	if _game_state and _game_state.has_method("ResetMessageState"):
		_game_state.ResetMessageState(message_id)
		return true
	return false

# Create a script with methods to simulate the C# GameState class
func create_script_with_methods():
	var script = GDScript.new()
	script.source_code = """
extends Node

# Properties
var CurrentFrequency = 90.0
var IsRadioOn = false
var DiscoveredFrequencies = []
var Messages = {}

# Methods
func SetFrequency(freq):
	CurrentFrequency = clamp(freq, 88.0, 108.0)


func ToggleRadio():
	IsRadioOn = !IsRadioOn


func FindSignalAtFrequency(freq):
	# Simulate a signal at 91.5 MHz
	if abs(freq - 91.5) < 0.2:
		return {
			"Frequency": 91.5,
			"Name": "Test Signal",
			"IsStatic": false,
			"MessageId": "msg_001"
		}
	return null


func CalculateSignalStrength(freq, signal_data):
	if signal_data == null:
		return 0.0

	var distance = abs(freq - signal_data.Frequency)
	if distance > 0.5:
		return 0.0

	return 1.0 - (distance * 2.0)


func GetStaticIntensity(freq):
	# Generate a pseudo-random value based on frequency
	var hash = hash(str(freq))
	return (hash % 100) / 100.0


func AddDiscoveredFrequency(freq):
	if !DiscoveredFrequencies.has(freq):
		DiscoveredFrequencies.append(freq)


func DecodeMessage(message_id):
	# Initialize the message if it doesn't exist
	if !Messages.has(message_id):
		Messages[message_id] = {
			"Decoded": false,
			"Content": "Test message content"
		}

	# Check if already decoded
	if Messages[message_id].Decoded:
		return false

	# Decode the message
	Messages[message_id].Decoded = true
	return true


func ResetMessageState(message_id):
	# Initialize the message if it doesn't exist
	if !Messages.has(message_id):
		Messages[message_id] = {
			"Decoded": false,
			"Content": "Test message content"
		}
	else:
		# Reset the decoded state
		Messages[message_id].Decoded = false

	return true


func GetMessage(message_id):
	# Initialize the message if it doesn't exist
	if !Messages.has(message_id):
		Messages[message_id] = {
			"Decoded": false,
			"Content": "Test message content"
		}

	return Messages[message_id]
"""
	script.reload()
	return script

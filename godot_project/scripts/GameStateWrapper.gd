extends Node

# This script is a wrapper for the GameState C# class
# It will be used as an autoload

var _game_state = null

func _ready():
	print("GameStateWrapper is initializing...")

	# Try to get the C# GameState if it exists as an autoload
	var game_state_autoload = get_node_or_null("/root/GameState")
	if game_state_autoload:
		print("Found GameState autoload, using it")
		_game_state = game_state_autoload
		return

	# Try to instantiate the C# GameState class
	if ClassDB.class_exists("SignalLost.GameState"):
		print("Found GameState class, instantiating it")
		_game_state = Node.new()
		_game_state.set_script(load("res://scripts/GameState.cs"))
		add_child(_game_state)
		return

	# Fallback to dummy implementation for testing
	print("Creating dummy GameState for testing")
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
	# Use a simple fallback approach
	var default_value = 90.0
	if _game_state == null:
		return default_value

	# Use a direct property access with fallback
	var current_freq = _game_state.get("CurrentFrequency")
	if current_freq != null:
		return current_freq

	return default_value

func is_radio_on():
	# Use a simple fallback approach
	var default_value = false
	if _game_state == null:
		return default_value

	# Use a direct property access with fallback
	var is_on = _game_state.get("IsRadioOn")
	if is_on != null:
		return is_on

	return default_value

func get_discovered_frequencies():
	# Use a simple fallback approach
	var default_value = []
	if _game_state == null:
		return default_value

	# Use a direct property access with fallback
	var freqs = _game_state.get("DiscoveredFrequencies")
	if freqs != null:
		return freqs

	return default_value

func set_frequency(freq):
	# Use a simple fallback approach
	if _game_state == null:
		return false

	# Try direct method call
	if _game_state.has_method("SetFrequency"):
		_game_state.call("SetFrequency", freq)
		return true

	# Fallback to property setting
	_game_state.set("CurrentFrequency", freq)

	# Emit signal to notify listeners
	if _game_state.has_signal("FrequencyChanged"):
		_game_state.emit_signal("FrequencyChanged", freq)

	return true

func toggle_radio():
	# Use a simple fallback approach
	if _game_state == null:
		return false

	# Try direct method call
	if _game_state.has_method("ToggleRadio"):
		_game_state.call("ToggleRadio")
		return true

	# Fallback to property toggling
	var is_on = is_radio_on()
	_game_state.set("IsRadioOn", !is_on)

	# Emit signal to notify listeners
	if _game_state.has_signal("RadioToggled"):
		_game_state.emit_signal("RadioToggled", !is_on)

	return true

func find_signal_at_frequency(freq):
	# Use a simple fallback approach
	if _game_state == null:
		return null

	# Try direct method call
	if _game_state.has_method("FindSignalAtFrequency"):
		return _game_state.call("FindSignalAtFrequency", freq)

	# Fallback - simulate signals at specific frequencies
	if abs(freq - 91.5) < 0.3:
		print("Found signal at 91.5")
		return {
			"Frequency": 91.5,
			"Name": "Test Signal 1",
			"IsStatic": true,
			"MessageId": "msg_001",
			"Bandwidth": 0.3
		}

	if abs(freq - 95.7) < 0.2:
		print("Found signal at 95.7")
		return {
			"Frequency": 95.7,
			"Name": "Test Signal 2",
			"IsStatic": false,
			"MessageId": "msg_002",
			"Bandwidth": 0.2
		}

	if abs(freq - 103.2) < 0.4:
		print("Found signal at 103.2")
		return {
			"Frequency": 103.2,
			"Name": "Test Signal 3",
			"IsStatic": true,
			"MessageId": "msg_003",
			"Bandwidth": 0.4
		}

	return null

func calculate_signal_strength(freq, signal_data):
	# Use a simple fallback approach
	if _game_state == null or signal_data == null:
		return 0.0

	# Try direct method call
	if _game_state.has_method("CalculateSignalStrength"):
		return _game_state.call("CalculateSignalStrength", freq, signal_data)

	# Fallback - calculate based on distance
	var distance = abs(freq - signal_data.Frequency)
	var max_distance = signal_data.Bandwidth if "Bandwidth" in signal_data else 0.5

	# Calculate strength based on how close we are to the exact frequency
	# 1.0 = perfect signal, 0.0 = no signal
	if distance <= max_distance:
		return 1.0 - (distance / max_distance)
	else:
		return 0.0

func get_static_intensity(freq):
	# Use a simple fallback approach
	if _game_state == null:
		return 0.5

	# Try direct method call
	if _game_state.has_method("GetStaticIntensity"):
		return _game_state.call("GetStaticIntensity", freq)

	# Fallback - generate a pseudo-random value
	var hash_val = hash(str(freq))
	return (hash_val % 100) / 100.0

func add_discovered_frequency(freq):
	# Use a simple fallback approach
	if _game_state == null:
		return false

	# Try direct method call
	if _game_state.has_method("AddDiscoveredFrequency"):
		_game_state.call("AddDiscoveredFrequency", freq)
		return true

	# Fallback - directly modify the property
	var freqs = get_discovered_frequencies()
	if not freq in freqs:
		freqs.append(freq)
		_game_state.set("DiscoveredFrequencies", freqs)
	return true

func decode_message(message_id):
	# Use a simple fallback approach
	if _game_state == null:
		return false

	# Try direct method call
	if _game_state.has_method("DecodeMessage"):
		return _game_state.call("DecodeMessage", message_id)

	# Fallback - simulate message decoding
	var messages = _game_state.get("Messages")
	if messages == null:
		messages = {}
		_game_state.set("Messages", messages)

	if not message_id in messages:
		messages[message_id] = {"Decoded": false, "Content": "Test message content"}

	if messages[message_id].Decoded:
		return false

	messages[message_id].Decoded = true
	return true

func get_message(message_id):
	# Use a simple fallback approach
	if _game_state == null:
		return null

	# Try direct method call
	if _game_state.has_method("GetMessage"):
		return _game_state.call("GetMessage", message_id)

	# Fallback - simulate message retrieval
	var messages = _game_state.get("Messages")
	if messages == null:
		messages = {}
		_game_state.set("Messages", messages)

	if not message_id in messages:
		messages[message_id] = {"Decoded": false, "Content": "Test message content"}

	return messages[message_id]

func reset_message_state(message_id):
	# Use a simple fallback approach
	if _game_state == null:
		return false

	# Try direct method call
	if _game_state.has_method("ResetMessageState"):
		_game_state.call("ResetMessageState", message_id)
		return true

	# Fallback - simulate message reset
	var messages = _game_state.get("Messages")
	if messages == null:
		messages = {}
		_game_state.set("Messages", messages)

	if not message_id in messages:
		messages[message_id] = {"Decoded": false, "Content": "Test message content"}
	else:
		messages[message_id].Decoded = false

	return true

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

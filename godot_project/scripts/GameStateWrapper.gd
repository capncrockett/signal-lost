extends Node

# This script is a wrapper for the GameState C# class
# It will be used as an autoload
# Modified by Agent Beta for testing

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

	# Define all available signals
	var signals = [
		{
			"Frequency": 91.5,
			"Name": "Emergency Broadcast",
			"IsStatic": true,
			"MessageId": "msg_001",
			"Bandwidth": 0.3,
			"Content": "EMERGENCY ALERT: This is an emergency broadcast. All citizens must evacuate to designated shelters immediately. This is not a drill."
		},
		{
			"Frequency": 95.7,
			"Name": "Military Communication",
			"IsStatic": false,
			"MessageId": "msg_002",
			"Bandwidth": 0.2,
			"Content": "MILITARY TRANSMISSION: Perimeter breach in sector 7. All units converge on coordinates 35.12, -106.54. Containment protocol alpha in effect."
		},
		{
			"Frequency": 103.2,
			"Name": "Survivor Message",
			"IsStatic": true,
			"MessageId": "msg_003",
			"Bandwidth": 0.4,
			"Content": "If anyone can hear this... we're holed up in the old mining facility. Food and water running low. The things outside... they're getting closer. Please send help."
		},
		{
			"Frequency": 88.3,
			"Name": "Research Facility",
			"IsStatic": true,
			"MessageId": "msg_004",
			"Bandwidth": 0.25,
			"Content": "Lab log 37: The specimens are showing increased aggression. Containment protocols failing. If you're receiving this, stay away from the research facility. I repeat, stay away!"
		},
		{
			"Frequency": 107.1,
			"Name": "Automated Weather System",
			"IsStatic": false,
			"MessageId": "msg_005",
			"Bandwidth": 0.35,
			"Content": "AUTOMATED WEATHER ALERT: Severe atmospheric disturbance detected. Unusual radiation levels. Seek shelter immediately. This message will repeat."
		}
	]

	# Check each signal to see if we're within its bandwidth
	for i in range(signals.size()):
		var signal_data = signals[i]
		var distance = abs(freq - signal_data.Frequency)
		if distance < signal_data.Bandwidth:
			# Only print if we're very close to the exact frequency
			if distance < 0.05:
				print("Found signal at %s: %s" % [signal_data.Frequency, signal_data.Name])
			return signal_data

	return null

func calculate_signal_strength(freq, signal_data):
	# Use a simple fallback approach
	if _game_state == null or signal_data == null:
		return 0.0

	# Try direct method call
	if _game_state.has_method("CalculateSignalStrength"):
		return _game_state.call("CalculateSignalStrength", freq, signal_data)

	# Fallback - calculate based on distance with a more realistic curve
	var distance = abs(freq - signal_data.Frequency)
	var max_distance = signal_data.Bandwidth if "Bandwidth" in signal_data else 0.5

	# No signal if we're outside the bandwidth
	if distance > max_distance:
		return 0.0

	# Calculate strength based on how close we are to the exact frequency
	# using a quadratic curve for more realistic falloff
	# 1.0 = perfect signal, 0.0 = no signal
	var normalized_distance = distance / max_distance

	# Add some randomness to simulate atmospheric interference
	var interference = randf() * 0.1 # Up to 10% interference

	# Quadratic falloff (1 - x²) gives a more natural curve
	var strength = 1.0 - (normalized_distance * normalized_distance)

	# Apply interference
	strength = max(0.0, strength - interference)

	return strength

func get_static_intensity(freq):
	# Use a simple fallback approach
	if _game_state == null:
		return 0.5

	# Try direct method call
	if _game_state.has_method("GetStaticIntensity"):
		return _game_state.call("GetStaticIntensity", freq)

	# Check if we're near any signal
	var min_static = 1.0
	var signals = [
		{"Frequency": 91.5, "Bandwidth": 0.3},
		{"Frequency": 95.7, "Bandwidth": 0.2},
		{"Frequency": 103.2, "Bandwidth": 0.4},
		{"Frequency": 88.3, "Bandwidth": 0.25},
		{"Frequency": 107.1, "Bandwidth": 0.35}
	]

	# Check each signal to find the minimum static level
	for i in range(signals.size()):
		var signal_data = signals[i]
		var distance = abs(freq - signal_data.Frequency)

		# If we're within the bandwidth, calculate static based on distance
		if distance < signal_data.Bandwidth:
			var normalized_distance = distance / signal_data.Bandwidth
			var signal_static = normalized_distance * normalized_distance
			min_static = min(min_static, signal_static)

	# Add some randomness to the static
	var base_static = 0.3 # Minimum static level even with perfect tuning
	var random_factor = randf() * 0.2 # Up to 20% random variation

	# Combine base static, signal-based static, and random factor
	return base_static + (min_static * 0.7) + random_factor

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

	# If message doesn't exist, create it with content from the signal data
	if not message_id in messages:
		# Find the signal with this message ID
		var content = "No message found"
		var signals = [
			{"MessageId": "msg_001", "Content": "EMERGENCY ALERT: This is an emergency broadcast. All citizens must evacuate to designated shelters immediately. This is not a drill."},
			{"MessageId": "msg_002", "Content": "MILITARY TRANSMISSION: Perimeter breach in sector 7. All units converge on coordinates 35.12, -106.54. Containment protocol alpha in effect."},
			{"MessageId": "msg_003", "Content": "If anyone can hear this... we're holed up in the old mining facility. Food and water running low. The things outside... they're getting closer. Please send help."},
			{"MessageId": "msg_004", "Content": "Lab log 37: The specimens are showing increased aggression. Containment protocols failing. If you're receiving this, stay away from the research facility. I repeat, stay away!"},
			{"MessageId": "msg_005", "Content": "AUTOMATED WEATHER ALERT: Severe atmospheric disturbance detected. Unusual radiation levels. Seek shelter immediately. This message will repeat."}
		]

		for i in range(signals.size()):
			if signals[i].MessageId == message_id:
				content = signals[i].Content
				break

		messages[message_id] = {"Decoded": false, "Content": content}

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

# Added by Agent Beta for testing
func get_agent_beta_test_info():
	print("Agent Beta test function called!")
	var info = {
		"agent": "Beta",
		"role": "QA Developer",
		"test_time": Time.get_datetime_string_from_system()
	}
	print("Test info: ", info)
	return info

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

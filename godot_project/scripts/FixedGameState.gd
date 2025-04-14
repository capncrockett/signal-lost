extends Node

# This script is a fixed version of the GameState class
# It will be used as an autoload
# Modified by Agent Beta for testing

# Game state variables
var _current_frequency: float = 90.0
var _is_radio_on: bool = false
var _discovered_frequencies: Array = []
var _current_location: String = "bunker"
var _inventory: Array = []
var _game_progress: int = 0

# Signal data
var _signals = [
    {
        "Id": "signal_1",
        "Frequency": 91.5,
        "MessageId": "msg_001",
        "IsStatic": true,
        "Bandwidth": 0.3
    },
    {
        "Id": "signal_2",
        "Frequency": 95.7,
        "MessageId": "msg_002",
        "IsStatic": false,
        "Bandwidth": 0.2
    },
    {
        "Id": "signal_3",
        "Frequency": 103.2,
        "MessageId": "msg_003",
        "IsStatic": true,
        "Bandwidth": 0.4
    }
]

# Messages data
var _messages = {
    "msg_001": {
        "Title": "Emergency Broadcast",
        "Content": "This is an emergency broadcast. All citizens must evacuate to designated shelters immediately.",
        "Decoded": false
    },
    "msg_002": {
        "Title": "Military Communication",
        "Content": "Alpha team, proceed to sector 7. Bravo team, hold position. Await further instructions.",
        "Decoded": false
    },
    "msg_003": {
        "Title": "Survivor Message",
        "Content": "If anyone can hear this, we're at the old factory. We have supplies and shelter. Please respond.",
        "Decoded": false
    }
}

# Signals (Godot's events, not radio signals)
signal FrequencyChanged(new_frequency)
signal RadioToggled(is_on)
signal FrequencyDiscovered(frequency)
signal MessageDecoded(message_id)

func _ready():
    print("FixedGameState is ready")

# Getters
func get_current_frequency() -> float:
    return _current_frequency

func is_radio_on() -> bool:
    return _is_radio_on

func get_discovered_frequencies() -> Array:
    return _discovered_frequencies

# Functions to modify state
func set_frequency(freq: float) -> void:
    _current_frequency = clamp(freq, 88.0, 108.0)
    print("FixedGameState: Frequency set to " + str(_current_frequency))
    emit_signal("FrequencyChanged", _current_frequency)

func toggle_radio() -> void:
    _is_radio_on = !_is_radio_on
    print("FixedGameState: Radio toggled to " + str(_is_radio_on))
    emit_signal("RadioToggled", _is_radio_on)

func add_discovered_frequency(freq: float) -> void:
    if !_discovered_frequencies.has(freq):
        _discovered_frequencies.append(freq)
        emit_signal("FrequencyDiscovered", freq)

func decode_message(message_id: String) -> bool:
    if _messages.has(message_id) and !_messages[message_id].Decoded:
        _messages[message_id].Decoded = true
        emit_signal("MessageDecoded", message_id)
        return true
    return false

# Signal functions
func find_signal_at_frequency(freq: float):
    for signal_data in _signals:
        var distance = abs(freq - signal_data.Frequency)
        if distance <= signal_data.Bandwidth:
            return signal_data
    return null

func calculate_signal_strength(freq: float, signal_data) -> float:
    if signal_data == null:
        return 0.0

    var distance = abs(freq - signal_data.Frequency)
    var max_distance = signal_data.Bandwidth

    # Calculate strength based on how close we are to the exact frequency
    # 1.0 = perfect signal, 0.0 = no signal
    if distance <= max_distance:
        return 1.0 - (distance / max_distance)
    else:
        return 0.0

func get_static_intensity(freq: float) -> float:
    # Generate a static intensity based on the frequency
    # This creates "dead zones" and "noisy areas" on the radio spectrum
    var base_noise = 0.3
    var noise_factor = sin(freq * 0.5) * 0.3 + 0.5
    return base_noise + noise_factor * 0.7

func get_message(message_id: String):
    if _messages.has(message_id):
        return _messages[message_id]
    return null

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

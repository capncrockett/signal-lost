extends Node

# Game state variables
var current_frequency = 90.0
var is_radio_on = false
var discovered_frequencies = []
var current_location = "bunker"
var inventory = []
var game_progress = 0

# Signal data
var signals = [
    {
        "id": "signal_1",
        "frequency": 91.5,
        "message_id": "msg_001",
        "is_static": true,
        "bandwidth": 0.3  # How close you need to be to the frequency to detect it
    },
    {
        "id": "signal_2",
        "frequency": 95.7,
        "message_id": "msg_002",
        "is_static": false,
        "bandwidth": 0.2
    },
    {
        "id": "signal_3",
        "frequency": 103.2,
        "message_id": "msg_003",
        "is_static": true,
        "bandwidth": 0.4
    }
]

# Messages data
var messages = {
    "msg_001": {
        "title": "Emergency Broadcast",
        "content": "This is an emergency broadcast. All citizens must evacuate to designated shelters immediately.",
        "decoded": false
    },
    "msg_002": {
        "title": "Military Communication",
        "content": "Alpha team, proceed to sector 7. Bravo team, hold position. Await further instructions.",
        "decoded": false
    },
    "msg_003": {
        "title": "Survivor Message",
        "content": "If anyone can hear this, we're at the old factory. We have supplies and shelter. Please respond.",
        "decoded": false
    }
}

# Signal functions
func find_signal_at_frequency(freq):
    for signal_data in signals:
        var distance = abs(freq - signal_data.frequency)
        if distance <= signal_data.bandwidth:
            return signal_data
    return null

func calculate_signal_strength(freq, signal_data):
    var distance = abs(freq - signal_data.frequency)
    var max_distance = signal_data.bandwidth
    
    # Calculate strength based on how close we are to the exact frequency
    # 1.0 = perfect signal, 0.0 = no signal
    if distance <= max_distance:
        return 1.0 - (distance / max_distance)
    else:
        return 0.0

func get_static_intensity(freq):
    # Generate a static intensity based on the frequency
    # This creates "dead zones" and "noisy areas" on the radio spectrum
    var base_noise = 0.3
    var noise_factor = sin(freq * 0.5) * 0.3 + 0.5
    return base_noise + noise_factor * 0.7

func get_message(message_id):
    if messages.has(message_id):
        return messages[message_id]
    return null

func add_discovered_frequency(freq):
    if not freq in discovered_frequencies:
        discovered_frequencies.append(freq)
        emit_signal("frequency_discovered", freq)

# Signals (Godot's events, not radio signals)
signal frequency_changed(new_frequency)
signal radio_toggled(is_on)
signal frequency_discovered(frequency)
signal message_decoded(message_id)

# Functions to modify state
func set_frequency(freq):
    current_frequency = clamp(freq, 88.0, 108.0)
    emit_signal("frequency_changed", current_frequency)

func toggle_radio():
    is_radio_on = !is_radio_on
    emit_signal("radio_toggled", is_radio_on)

func decode_message(message_id):
    if messages.has(message_id) and not messages[message_id].decoded:
        messages[message_id].decoded = true
        emit_signal("message_decoded", message_id)
        return true
    return false

# Save and load functions
func save_game():
    var save_data = {
        "current_frequency": current_frequency,
        "is_radio_on": is_radio_on,
        "discovered_frequencies": discovered_frequencies,
        "current_location": current_location,
        "inventory": inventory,
        "game_progress": game_progress,
        "messages": messages
    }
    
    var save_file = File.new()
    save_file.open("user://savegame.save", File.WRITE)
    save_file.store_line(to_json(save_data))
    save_file.close()
    return true

func load_game():
    var save_file = File.new()
    if not save_file.file_exists("user://savegame.save"):
        return false
    
    save_file.open("user://savegame.save", File.READ)
    var save_data = parse_json(save_file.get_line())
    save_file.close()
    
    current_frequency = save_data.current_frequency
    is_radio_on = save_data.is_radio_on
    discovered_frequencies = save_data.discovered_frequencies
    current_location = save_data.current_location
    inventory = save_data.inventory
    game_progress = save_data.game_progress
    messages = save_data.messages
    
    return true

extends Control

## Radio tuner properties
@export var min_frequency: float = 88.0
@export var max_frequency: float = 108.0
@export var frequency_step: float = 0.1
@export var scan_speed: float = 0.3  # Scan speed in seconds
@export var process_frequency_interval: int = 60  # Process frequency every 60 frames

## Local state
var _is_scanning: bool = false
var _show_message: bool = false
var _current_signal_id = null
var _signal_strength: float = 0.0
var _static_intensity: float = 0.5
var _scan_timer: Timer = null
var _last_process_time: int = 0

## References to singletons
var _game_state = null
var _audio_manager = null

## Cached node references
var _frequency_display: Label = null
var _frequency_slider: HSlider = null
var _power_button: Button = null
var _signal_strength_meter: ProgressBar = null
var _static_visualization: ColorRect = null
var _message_container: Panel = null
var _message_button: Button = null
var _message_display: Label = null
var _scan_button: Button = null
var _tune_down_button: Button = null
var _tune_up_button: Button = null

## Called when the node enters the scene tree
func _ready() -> void:
	# Initialize singleton references
	_initialize_singletons()

	# Cache node references for better performance
	_cache_node_references()

	# Create scan timer
	_create_scan_timer()

	# Connect UI signals
	_connect_signals()

	# Initialize UI
	_update_ui()

## Initialize references to singleton nodes
func _initialize_singletons() -> void:
	_game_state = get_node_or_null("/root/GameState")
	if _game_state == null:
		_game_state = get_node_or_null("/root/GameStateWrapper")

	_audio_manager = get_node_or_null("/root/AudioManager")
	if _audio_manager == null:
		_audio_manager = get_node_or_null("/root/AudioManagerWrapper")

	print("RadioTuner: GameState = %s, AudioManager = %s" % [_game_state, _audio_manager])

## Cache node references for better performance
func _cache_node_references() -> void:
	_frequency_display = get_node_or_null("FrequencyDisplay")
	_frequency_slider = get_node_or_null("FrequencySlider")
	_power_button = get_node_or_null("PowerButton")
	_signal_strength_meter = get_node_or_null("SignalStrengthMeter")
	_static_visualization = get_node_or_null("StaticVisualization")
	_message_container = get_node_or_null("MessageContainer")
	_message_button = get_node_or_null("MessageContainer/MessageButton") if _message_container else null
	_message_display = get_node_or_null("MessageContainer/MessageDisplay") if _message_container else null
	_scan_button = get_node_or_null("ScanButton")
	_tune_down_button = get_node_or_null("TuneDownButton")
	_tune_up_button = get_node_or_null("TuneUpButton")

## Create and configure the scan timer
func _create_scan_timer() -> void:
	_scan_timer = Timer.new()
	_scan_timer.wait_time = scan_speed
	_scan_timer.one_shot = false
	_scan_timer.timeout.connect(_on_scan_timer_timeout)
	add_child(_scan_timer)

## Connect UI signals to their handlers
func _connect_signals() -> void:
	if _power_button:
		_power_button.pressed.connect(_on_power_button_pressed)
	else:
		$PowerButton.pressed.connect(_on_power_button_pressed)

	if _frequency_slider:
		_frequency_slider.value_changed.connect(_on_frequency_slider_changed)
	else:
		$FrequencySlider.value_changed.connect(_on_frequency_slider_changed)

	if _message_button:
		_message_button.pressed.connect(_on_message_button_pressed)
	else:
		$MessageContainer/MessageButton.pressed.connect(_on_message_button_pressed)

	if _scan_button:
		_scan_button.pressed.connect(_on_scan_button_pressed)
	else:
		$ScanButton.pressed.connect(_on_scan_button_pressed)

	if _tune_down_button:
		_tune_down_button.pressed.connect(_on_tune_down_button_pressed)
	else:
		$TuneDownButton.pressed.connect(_on_tune_down_button_pressed)

	if _tune_up_button:
		_tune_up_button.pressed.connect(_on_tune_up_button_pressed)
	else:
		$TuneUpButton.pressed.connect(_on_tune_up_button_pressed)

# Process function called every frame
func _process(delta):
	if _game_state and _game_state.is_radio_on():
		# Process frequency - only once per second to avoid excessive processing
		if Engine.get_process_frames() % 60 == 0:
			_process_frequency()
		_update_static_visualization(delta)

# Process the current frequency
func _process_frequency():
	if not _game_state:
		return

	var signal_data = _game_state.find_signal_at_frequency(_game_state.get_current_frequency())

	if signal_data:
		# Calculate signal strength
		_signal_strength = _game_state.calculate_signal_strength(_game_state.get_current_frequency(), signal_data)

		# Calculate static intensity
		_static_intensity = 1.0 - _signal_strength

		# Update UI
		$SignalStrengthMeter.value = _signal_strength * 100
		_current_signal_id = signal_data.MessageId

		# Add to discovered frequencies
		_game_state.add_discovered_frequency(signal_data.Frequency)

		# Play audio
		if _audio_manager:
			if signal_data.IsStatic:
				_audio_manager.play_static_noise(_static_intensity)
				_audio_manager.play_signal(signal_data.Frequency * 10, _signal_strength * 0.5)
			else:
				_audio_manager.stop_static_noise()
				_audio_manager.play_signal(signal_data.Frequency * 10)
	else:
		# No signal found
		_static_intensity = _game_state.get_static_intensity(_game_state.get_current_frequency())
		_signal_strength = 0.1
		_current_signal_id = null

		# Update UI
		$SignalStrengthMeter.value = _signal_strength * 100

		# Play audio
		if _audio_manager:
			_audio_manager.stop_signal()
			_audio_manager.play_static_noise(_static_intensity)

	# Update message button state
	_update_message_button()

# Update the static visualization
func _update_static_visualization(_delta: float) -> void:
	# Update the audio visualizer
	if _static_visualization and _static_visualization.has_method("set_signal_strength"):
		_static_visualization.set_signal_strength(_signal_strength)
		_static_visualization.set_static_intensity(_static_intensity)
	else:
		# Fallback to simple visualization if the audio visualizer is not available
		var mod_color = $StaticVisualization.modulate
		mod_color.a = _static_intensity
		$StaticVisualization.modulate = mod_color

# Change the frequency by a specific amount
func change_frequency(amount):
	if not _game_state:
		return

	var new_freq = _game_state.get_current_frequency() + amount
	new_freq = clamp(new_freq, min_frequency, max_frequency)

	_game_state.set_frequency(new_freq)

## Toggle the radio power
func toggle_power() -> void:
	if not _game_state:
		return

	_game_state.toggle_radio()

	if not _game_state.is_radio_on():
		# Stop all audio when radio is turned off
		if _audio_manager:
			_audio_manager.stop_signal()
			_audio_manager.stop_static_noise()

		# Stop scanning if active
		if _is_scanning:
			toggle_scanning()

	_update_ui()

# Toggle frequency scanning
func toggle_scanning():
	_is_scanning = !_is_scanning

	if _is_scanning and _game_state and _game_state.is_radio_on():
		_scan_timer.start()
	else:
		_scan_timer.stop()

	# Update UI
	$ScanButton.text = "Stop Scan" if _is_scanning else "Scan"

## Toggle message display
func toggle_message() -> void:
	_show_message = !_show_message

	# Update UI - handle both cached and direct references
	if _message_display:
		_message_display.visible = _show_message
	else:
		$MessageContainer/MessageDisplay.visible = _show_message

	if _message_button:
		_message_button.text = "Hide Message" if _show_message else "Show Message"
	else:
		$MessageContainer/MessageButton.text = "Hide Message" if _show_message else "Show Message"

	if _show_message and _current_signal_id != null and _game_state:
		var message = _game_state.get_message(_current_signal_id)
		if message:
			if _message_display:
				_message_display.text = message.Content
			else:
				$MessageContainer/MessageDisplay.text = message.Content

# Update the UI based on current state
func _update_ui():
	if not _game_state:
		return

	# Update frequency display
	$FrequencyDisplay.text = "%.1f MHz" % _game_state.get_current_frequency()

	# Update power button
	$PowerButton.text = "ON" if _game_state.is_radio_on() else "OFF"

	# Update frequency slider
	var percentage = (_game_state.get_current_frequency() - min_frequency) / (max_frequency - min_frequency)
	$FrequencySlider.value = percentage * 100

	# Update message button
	_update_message_button()

	# Update scan button
	_scan_button.text = "Stop Scan" if _is_scanning else "Scan"

	# Update tune buttons
	_tune_down_button.disabled = !_game_state.is_radio_on() or _is_scanning
	_tune_up_button.disabled = !_game_state.is_radio_on() or _is_scanning

## Update the message button state
func _update_message_button() -> void:
	if not _game_state:
		return

	var has_message = _current_signal_id != null and _game_state.get_message(_current_signal_id) != null
	var radio_on = _game_state.is_radio_on()

	# Handle both cached and direct references
	if _message_button:
		_message_button.disabled = !radio_on or !has_message
	else:
		$MessageContainer/MessageButton.disabled = !radio_on or !has_message

	if _message_container:
		_message_container.visible = has_message
	else:
		$MessageContainer.visible = has_message

# Signal handlers
func _on_power_button_pressed():
	toggle_power()

func _on_frequency_slider_changed(value):
	if not _game_state:
		return

	var freq = min_frequency + (value / 100.0) * (max_frequency - min_frequency)
	_game_state.set_frequency(freq)

func _on_message_button_pressed():
	toggle_message()

func _on_tune_down_button_pressed():
	change_frequency(-frequency_step)

func _on_tune_up_button_pressed():
	change_frequency(frequency_step)

func _on_scan_button_pressed():
	toggle_scanning()

func _on_scan_timer_timeout():
	if _is_scanning and _game_state and _game_state.is_radio_on():
		# Increment frequency by step
		var new_freq = _game_state.get_current_frequency() + frequency_step

		# If we reach the max frequency, loop back to min
		if new_freq > max_frequency:
			new_freq = min_frequency

		_game_state.set_frequency(new_freq)

extends Control

# Radio tuner properties
export var min_frequency = 88.0
export var max_frequency = 108.0
export var frequency_step = 0.1

# UI references (will be set in _ready)
onready var frequency_display = $FrequencyDisplay
onready var power_button = $PowerButton
onready var frequency_slider = $FrequencySlider
onready var signal_strength_meter = $SignalStrengthMeter
onready var static_visualization = $StaticVisualization
onready var message_container = $MessageContainer
onready var message_button = $MessageContainer/MessageButton
onready var message_display = $MessageContainer/MessageDisplay

# Local state
var is_scanning = false
var show_message = false
var current_signal_id = null
var signal_strength = 0.0
var static_intensity = 0.5
var scan_timer = null

# Called when the node enters the scene tree
func _ready():
    # Initialize UI
    update_ui()
    
    # Connect signals
    power_button.connect("pressed", self, "_on_power_button_pressed")
    frequency_slider.connect("value_changed", self, "_on_frequency_slider_changed")
    message_button.connect("pressed", self, "_on_message_button_pressed")
    
    # Connect to GameState signals
    GameState.connect("frequency_changed", self, "_on_frequency_changed")
    GameState.connect("radio_toggled", self, "_on_radio_toggled")
    
    # Create scan timer
    scan_timer = Timer.new()
    scan_timer.wait_time = 0.3  # Scan speed in seconds
    scan_timer.one_shot = false
    scan_timer.connect("timeout", self, "_on_scan_timer_timeout")
    add_child(scan_timer)

# Process function called every frame
func _process(delta):
    if GameState.is_radio_on:
        # Process input
        if Input.is_action_just_pressed("tune_up"):
            change_frequency(frequency_step)
        elif Input.is_action_just_pressed("tune_down"):
            change_frequency(-frequency_step)
        elif Input.is_action_just_pressed("toggle_power"):
            toggle_power()
        
        # Process frequency and update audio/visuals
        process_frequency()
        update_static_visualization(delta)

# Process the current frequency
func process_frequency():
    var signal_data = GameState.find_signal_at_frequency(GameState.current_frequency)
    
    if signal_data:
        # Calculate signal strength based on how close we are to the exact frequency
        signal_strength = GameState.calculate_signal_strength(GameState.current_frequency, signal_data)
        
        # Calculate static intensity based on signal strength
        static_intensity = signal_data.is_static ? 1.0 - signal_strength : (1.0 - signal_strength) * 0.5
        
        # Update UI
        signal_strength_meter.value = signal_strength * 100
        current_signal_id = signal_data.message_id
        
        # If this is a new signal discovery, add it to discovered frequencies
        if not signal_data.frequency in GameState.discovered_frequencies:
            GameState.add_discovered_frequency(signal_data.frequency)
        
        # Play appropriate audio
        if signal_data.is_static:
            # Play static with the signal mixed in
            AudioManager.play_static_noise(static_intensity)
            AudioManager.play_signal(signal_data.frequency * 10, signal_strength * 0.5)  # Scale up for audible range
        else:
            # Play a clear signal
            AudioManager.stop_static_noise()
            AudioManager.play_signal(signal_data.frequency * 10)  # Scale up for audible range
    else:
        # No signal found, just play static
        var intensity = GameState.get_static_intensity(GameState.current_frequency)
        
        # Update state
        static_intensity = intensity
        signal_strength = 0.1  # Low signal strength
        current_signal_id = null
        
        # Update UI
        signal_strength_meter.value = signal_strength * 100
        
        # Play audio
        AudioManager.stop_signal()
        AudioManager.play_static_noise(intensity)
    
    # Update message button state
    update_message_button()

# Update the static visualization
func update_static_visualization(delta):
    # This would normally update a shader or material
    # For now, we'll just update a property
    static_visualization.modulate.a = static_intensity

# Change the frequency by a specific amount
func change_frequency(amount):
    var new_freq = GameState.current_frequency + amount
    new_freq = clamp(new_freq, min_frequency, max_frequency)
    new_freq = stepify(new_freq, frequency_step)  # Round to nearest step
    
    GameState.set_frequency(new_freq)

# Toggle the radio power
func toggle_power():
    GameState.toggle_radio()

# Toggle frequency scanning
func toggle_scanning():
    is_scanning = !is_scanning
    
    if is_scanning and GameState.is_radio_on:
        scan_timer.start()
    else:
        scan_timer.stop()
    
    # Update UI
    $ScanButton.text = "Stop Scan" if is_scanning else "Scan"

# Toggle message display
func toggle_message():
    show_message = !show_message
    
    # Update UI
    message_display.visible = show_message
    message_button.text = "Hide Message" if show_message else "Show Message"
    
    if show_message and current_signal_id:
        var message = GameState.get_message(current_signal_id)
        if message:
            message_display.set_message(message)

# Update the UI based on current state
func update_ui():
    # Update frequency display
    frequency_display.text = "%.1f MHz" % GameState.current_frequency
    
    # Update power button
    power_button.text = "ON" if GameState.is_radio_on else "OFF"
    
    # Update frequency slider
    var percentage = (GameState.current_frequency - min_frequency) / (max_frequency - min_frequency)
    frequency_slider.value = percentage * 100
    
    # Update message button
    update_message_button()
    
    # Update scan button
    $ScanButton.text = "Stop Scan" if is_scanning else "Scan"
    
    # Update tune buttons
    $TuneDownButton.disabled = !GameState.is_radio_on or is_scanning
    $TuneUpButton.disabled = !GameState.is_radio_on or is_scanning

# Update the message button state
func update_message_button():
    var has_message = current_signal_id != null and GameState.get_message(current_signal_id) != null
    message_button.disabled = !GameState.is_radio_on or !has_message
    message_container.visible = has_message

# Signal handlers
func _on_power_button_pressed():
    toggle_power()

func _on_frequency_slider_changed(value):
    var freq = min_frequency + (value / 100.0) * (max_frequency - min_frequency)
    freq = stepify(freq, frequency_step)  # Round to nearest step
    GameState.set_frequency(freq)

func _on_message_button_pressed():
    toggle_message()

func _on_tune_down_button_pressed():
    change_frequency(-frequency_step)

func _on_tune_up_button_pressed():
    change_frequency(frequency_step)

func _on_scan_button_pressed():
    toggle_scanning()

func _on_scan_timer_timeout():
    if is_scanning and GameState.is_radio_on:
        # Increment frequency by step
        var new_freq = GameState.current_frequency + frequency_step
        
        # If we reach the max frequency, loop back to min
        if new_freq > max_frequency:
            new_freq = min_frequency
        
        GameState.set_frequency(new_freq)

func _on_frequency_changed(new_frequency):
    update_ui()

func _on_radio_toggled(is_on):
    if !is_on:
        # Stop all audio when radio is turned off
        AudioManager.stop_signal()
        AudioManager.stop_static_noise()
        
        # Stop scanning if active
        if is_scanning:
            toggle_scanning()
    
    update_ui()

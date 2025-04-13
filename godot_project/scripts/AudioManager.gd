extends Node

# Audio properties
var volume = 0.8
var is_muted = false

# Audio nodes (will be created at runtime)
var static_player
var signal_player
var effect_player

# Noise types
enum NoiseType {
    WHITE,
    PINK,
    BROWN,
    DIGITAL
}

var current_noise_type = NoiseType.WHITE

# Audio bus indices
var master_bus_idx = 0
var static_bus_idx = 1
var signal_bus_idx = 2

# Initialize audio system
func _ready():
    # Create audio players
    static_player = AudioStreamPlayer.new()
    signal_player = AudioStreamPlayer.new()
    effect_player = AudioStreamPlayer.new()
    
    # Add to scene tree
    add_child(static_player)
    add_child(signal_player)
    add_child(effect_player)
    
    # Set up audio buses
    master_bus_idx = AudioServer.get_bus_index("Master")
    
    # Create static bus
    AudioServer.add_bus()
    static_bus_idx = AudioServer.get_bus_count() - 1
    AudioServer.set_bus_name(static_bus_idx, "Static")
    AudioServer.set_bus_send(static_bus_idx, "Master")
    
    # Create signal bus
    AudioServer.add_bus()
    signal_bus_idx = AudioServer.get_bus_count() - 1
    AudioServer.set_bus_name(signal_bus_idx, "Signal")
    AudioServer.set_bus_send(signal_bus_idx, "Master")
    
    # Set up effects
    _setup_audio_effects()
    
    # Set initial volume
    set_volume(volume)

# Set up audio effects for buses
func _setup_audio_effects():
    # Add EQ to static bus
    var eq = AudioEffectEQ.new()
    eq.set_band_gain_db(0, -5.0)  # Reduce low frequencies
    eq.set_band_gain_db(1, 2.0)   # Boost mid-low frequencies
    AudioServer.add_bus_effect(static_bus_idx, eq)
    
    # Add distortion to static bus
    var distortion = AudioEffectDistortion.new()
    distortion.mode = AudioEffectDistortion.MODE_BITCRUSH
    distortion.drive = 0.2
    AudioServer.add_bus_effect(static_bus_idx, distortion)
    
    # Add reverb to signal bus
    var reverb = AudioEffectReverb.new()
    reverb.wet = 0.1
    reverb.dry = 0.9
    AudioServer.add_bus_effect(signal_bus_idx, reverb)

# Generate noise based on the current noise type
func _generate_noise_stream(intensity):
    var noise_generator = AudioStreamGenerator.new()
    noise_generator.mix_rate = 44100
    noise_generator.buffer_length = 0.1  # 100ms buffer
    
    var playback = AudioStreamGeneratorPlayback.new()
    static_player.stream = noise_generator
    static_player.play()
    playback = static_player.get_stream_playback()
    
    # Fill the buffer with noise
    var buffer_size = noise_generator.buffer_length * noise_generator.mix_rate
    for i in range(buffer_size):
        var sample = 0.0
        
        match current_noise_type:
            NoiseType.WHITE:
                sample = randf() * 2.0 - 1.0
            NoiseType.PINK:
                # Simple approximation of pink noise
                sample = (randf() * 2.0 - 1.0) * 0.7 + (randf() * 2.0 - 1.0) * 0.2 + (randf() * 2.0 - 1.0) * 0.1
            NoiseType.BROWN:
                # Simple approximation of brown noise
                sample = (randf() * 2.0 - 1.0) * 0.5 + (randf() * 2.0 - 1.0) * 0.3 + (randf() * 2.0 - 1.0) * 0.2
            NoiseType.DIGITAL:
                # Digital noise (more harsh)
                sample = round(randf()) * 2.0 - 1.0
        
        # Apply intensity
        sample *= intensity
        
        playback.push_frame(Vector2(sample, sample))
    
    return noise_generator

# Play static noise with given intensity
func play_static_noise(intensity):
    if static_player.playing:
        static_player.stop()
    
    var noise_stream = _generate_noise_stream(intensity)
    static_player.stream = noise_stream
    static_player.bus = "Static"
    static_player.volume_db = linear2db(intensity * volume)
    static_player.play()

# Stop static noise
func stop_static_noise():
    static_player.stop()

# Play signal tone at specified frequency
func play_signal(frequency, volume_scale = 1.0, waveform = "sine"):
    if signal_player.playing:
        signal_player.stop()
    
    var generator = AudioStreamGenerator.new()
    generator.mix_rate = 44100
    generator.buffer_length = 0.1  # 100ms buffer
    
    signal_player.stream = generator
    signal_player.bus = "Signal"
    signal_player.volume_db = linear2db(volume_scale * volume)
    signal_player.play()
    
    var playback = signal_player.get_stream_playback()
    
    # Fill the buffer with the waveform
    var buffer_size = generator.buffer_length * generator.mix_rate
    var phase = 0.0
    var increment = frequency / generator.mix_rate
    
    for i in range(buffer_size):
        var sample = 0.0
        
        # Generate different waveforms
        match waveform:
            "sine":
                sample = sin(phase * 2.0 * PI)
            "square":
                sample = 1.0 if fmod(phase, 1.0) < 0.5 else -1.0
            "triangle":
                var t = fmod(phase, 1.0)
                sample = 2.0 * (t if t < 0.5 else 1.0 - t) - 1.0
            "sawtooth":
                sample = 2.0 * fmod(phase, 1.0) - 1.0
        
        # Apply volume scale
        sample *= volume_scale
        
        playback.push_frame(Vector2(sample, sample))
        phase += increment
    
    return generator

# Stop signal tone
func stop_signal():
    signal_player.stop()

# Play a sound effect
func play_effect(effect_name):
    var effect_path = "res://assets/audio/" + effect_name + ".wav"
    var effect = load(effect_path)
    
    if effect:
        effect_player.stream = effect
        effect_player.play()

# Set the volume (0.0 to 1.0)
func set_volume(vol):
    volume = clamp(vol, 0.0, 1.0)
    AudioServer.set_bus_volume_db(master_bus_idx, linear2db(volume))

# Toggle mute
func toggle_mute():
    is_muted = !is_muted
    AudioServer.set_bus_mute(master_bus_idx, is_muted)

# Set the noise type
func set_noise_type(type):
    current_noise_type = type

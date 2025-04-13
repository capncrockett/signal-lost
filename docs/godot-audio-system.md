# Signal Lost - Godot Audio System Architecture

This document describes the audio system architecture used in the Godot implementation of Signal Lost.

## Overview

The audio system in Signal Lost is built using Godot's built-in audio capabilities, which provide powerful tools for audio processing, synthesis, and effects. Godot's audio system is well-suited for the radio tuning mechanics that are central to the game.

## Key Components

### 1. AudioManager Singleton

The `AudioManager` singleton is responsible for managing all audio in the game. It handles:

- Generating and playing static noise
- Generating and playing signal tones
- Managing audio effects and mixing
- Controlling volume and mute state
- Handling audio bus configuration

```gdscript
# Example usage
AudioManager.play_static_noise(0.5)  # Play static at 50% intensity
AudioManager.play_signal(91.5, 0.8)  # Play signal at 91.5 MHz with 80% volume
AudioManager.set_volume(0.7)         # Set master volume to 70%
```

### 2. RadioTuner Component

The `RadioTuner` scene is the core of the game's audio experience. It simulates a radio tuning experience with:

- Frequency tuning controls
- Signal strength visualization
- Static visualization
- Message display for decoded signals

```gdscript
# Example usage
radio_tuner.set_frequency(91.5)  # Tune to 91.5 MHz
radio_tuner.toggle_power()       # Turn radio on/off
radio_tuner.toggle_scanning()    # Start/stop automatic scanning
```

### 3. Audio Bus System

Godot's audio bus system is used to organize and process audio:

- **Master Bus**: Controls overall game volume
- **Static Bus**: Processes static noise with EQ and distortion effects
- **Signal Bus**: Processes signal tones with reverb and filtering
- **Music Bus**: Handles background music with dynamic mixing
- **SFX Bus**: Manages sound effects

## Audio Generation

### Static Noise Generation

Static noise is generated procedurally using Godot's `AudioStreamGenerator` class:

```gdscript
func _generate_noise_stream(intensity: float) -> AudioStreamGenerator:
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
        var sample = randf() * 2.0 - 1.0  # White noise
        
        # Apply intensity
        sample *= intensity
        
        playback.push_frame(Vector2(sample, sample))
    
    return noise_generator
```

### Signal Tone Generation

Signal tones are generated using oscillators with various waveforms:

```gdscript
func play_signal(frequency: float, volume_scale: float = 1.0, waveform: String = "sine") -> AudioStreamGenerator:
    var generator = AudioStreamGenerator.new()
    generator.mix_rate = 44100
    generator.buffer_length = 0.1  # 100ms buffer
    
    signal_player.stream = generator
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
```

## Signal Processing

### Signal Detection

The game simulates radio signal detection by checking if the current frequency is close to a predefined signal frequency:

```gdscript
func find_signal_at_frequency(freq: float) -> Dictionary:
    for signal_data in signals:
        var distance = abs(freq - signal_data.frequency)
        if distance <= signal_data.bandwidth:
            return signal_data
    return {}

func calculate_signal_strength(freq: float, signal_data: Dictionary) -> float:
    var distance = abs(freq - signal_data.frequency)
    var max_distance = signal_data.bandwidth
    
    # Calculate strength based on how close we are to the exact frequency
    # 1.0 = perfect signal, 0.0 = no signal
    if distance <= max_distance:
        return 1.0 - (distance / max_distance)
    else:
        return 0.0
```

### Audio Effects

Godot's audio effects are used to enhance the radio experience:

- **EQ**: Shapes the frequency response of static and signals
- **Distortion**: Adds grit and texture to static
- **Reverb**: Adds space and depth to signals
- **Filters**: Simulates radio bandwidth limitations
- **Compression**: Controls dynamic range

```gdscript
func _setup_audio_effects() -> void:
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
```

## Noise Types

The system supports multiple types of noise for different radio effects:

- **White Noise**: Equal energy across all frequencies
- **Pink Noise**: Energy decreases as frequency increases (1/f)
- **Brown Noise**: Energy decreases more rapidly with frequency (1/f²)
- **Digital Noise**: Harsh, digital-sounding noise

```gdscript
enum NoiseType {
    WHITE,
    PINK,
    BROWN,
    DIGITAL
}

func generate_noise(type: int, intensity: float) -> float:
    var sample = 0.0
    
    match type:
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
    
    return sample * intensity
```

## Audio Visualization

The radio tuner includes visual feedback for audio:

- **Signal Strength Meter**: Shows the strength of the detected signal
- **Static Visualization**: Displays static as visual noise
- **Frequency Display**: Shows the current tuned frequency
- **Spectrum Analyzer**: Visualizes the frequency content of the audio

```gdscript
func update_static_visualization(delta: float) -> void:
    # Update the static visualization shader
    static_visualization.material.set_shader_param("intensity", static_intensity)
    static_visualization.material.set_shader_param("time", OS.get_ticks_msec() / 1000.0)
```

## Testing

The audio system includes comprehensive testing:

- **Unit Tests**: Test individual audio components
- **Integration Tests**: Test audio system as a whole
- **Performance Tests**: Ensure audio processing is efficient

```gdscript
# Example test for radio tuner audio
func test_radio_tuner_audio():
    # Arrange
    radio_tuner.is_on = true
    
    # Act
    radio_tuner.set_frequency(91.5)  # Known signal frequency
    radio_tuner.process_frequency()
    
    # Assert
    assert_true(AudioManager.signal_player.playing, "Signal player should be playing")
    assert_eq(radio_tuner.signal_strength, 1.0, "Signal strength should be maximum")
```

## Conclusion

The Godot audio system provides a robust foundation for the radio tuning mechanics in Signal Lost. By leveraging Godot's built-in audio capabilities, we can create a more immersive and performant audio experience than was possible with the browser-based implementation.

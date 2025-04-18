# Signal Lost Audio System

## Overview

The audio system in Signal Lost is built using Godot's audio capabilities, providing realistic radio tuning mechanics with static noise, signal processing, and audio effects.

## Key Components

### 1. AudioManager

The `AudioManager` singleton handles all audio in the game:

- Generating and playing static noise
- Generating and playing signal tones
- Managing audio effects and mixing
- Controlling volume and mute state

```csharp
// Example usage
AudioManager.PlayStaticNoise(0.5f);  // Play static at 50% intensity
AudioManager.PlaySignal(91.5f, 0.8f); // Play signal at 91.5 MHz with 80% strength
AudioManager.SetVolume(0.7f);        // Set master volume to 70%
```

### 2. RadioSystem

The `RadioSystem` singleton manages radio signals and frequencies:

- Defining available signals
- Detecting signals at specific frequencies
- Calculating signal strength
- Managing signal discovery

```csharp
// Example usage
var signal = RadioSystem.FindSignalAtFrequency(91.5f);
float strength = RadioSystem.CalculateSignalStrength(91.5f);
bool isDiscovered = RadioSystem.IsSignalDiscovered(signal.Id);
```

### 3. PixelRadioInterface

The `PixelRadioInterface` provides the visual interface for the radio:

- Frequency tuning controls
- Signal strength visualization
- Static visualization
- Message display for decoded signals

## Audio Generation

### Static Noise Generation

Static noise is generated procedurally using Godot's audio generation capabilities:

```csharp
private void GenerateStaticNoise(float intensity)
{
    // Create noise buffer
    var buffer = new float[BUFFER_SIZE];
    
    // Fill buffer with random values
    for (int i = 0; i < BUFFER_SIZE; i++)
    {
        buffer[i] = (float)GD.RandRange(-1.0, 1.0) * intensity;
    }
    
    // Apply to audio stream
    _staticStreamPlayer.Stream = CreateAudioStreamFromBuffer(buffer);
    _staticStreamPlayer.Play();
}
```

### Signal Tone Generation

Signal tones are generated using oscillators with various waveforms:

```csharp
private void GenerateSignalTone(float frequency, float volume, string waveform = "sine")
{
    var buffer = new float[BUFFER_SIZE];
    float phase = 0.0f;
    float increment = frequency / SAMPLE_RATE;
    
    for (int i = 0; i < BUFFER_SIZE; i++)
    {
        float sample = 0.0f;
        
        // Generate different waveforms
        switch (waveform)
        {
            case "sine":
                sample = Mathf.Sin(phase * Mathf.Pi * 2.0f);
                break;
            case "square":
                sample = phase % 1.0f < 0.5f ? 1.0f : -1.0f;
                break;
            case "triangle":
                float t = phase % 1.0f;
                sample = 2.0f * (t < 0.5f ? t : 1.0f - t) - 1.0f;
                break;
            case "sawtooth":
                sample = 2.0f * (phase % 1.0f) - 1.0f;
                break;
        }
        
        buffer[i] = sample * volume;
        phase += increment;
    }
    
    _signalStreamPlayer.Stream = CreateAudioStreamFromBuffer(buffer);
    _signalStreamPlayer.Play();
}
```

## Signal Processing

### Signal Detection

The game simulates radio signal detection by checking if the current frequency is close to a predefined signal frequency:

```csharp
public RadioSignal FindSignalAtFrequency(float frequency)
{
    foreach (var signal in _signals)
    {
        float distance = Mathf.Abs(frequency - signal.Frequency);
        if (distance <= signal.Bandwidth)
        {
            return signal;
        }
    }
    return null;
}

public float CalculateSignalStrength(float frequency, RadioSignal signal)
{
    float distance = Mathf.Abs(frequency - signal.Frequency);
    float maxDistance = signal.Bandwidth;
    
    // Calculate strength based on how close we are to the exact frequency
    // 1.0 = perfect signal, 0.0 = no signal
    if (distance <= maxDistance)
    {
        return 1.0f - (distance / maxDistance);
    }
    else
    {
        return 0.0f;
    }
}
```

### Audio Effects

Godot's audio effects enhance the radio experience:

- **EQ**: Shapes the frequency response of static and signals
- **Distortion**: Adds grit and texture to static
- **Reverb**: Adds space and depth to signals
- **Filters**: Simulates radio bandwidth limitations

## Radio UI Features

The radio interface includes:

- **Tuning Knob**: Allows precise frequency adjustment
- **Signal Strength Meter**: Shows the strength of the detected signal
- **Static Visualization**: Displays static as visual noise
- **Frequency Display**: Shows the current tuned frequency
- **Power Button**: Toggles the radio on/off
- **Scan Button**: Automatically scans for signals

## Realistic Radio Behavior

The system simulates realistic radio behavior:

- **Signal Fading**: Signals fade in and out based on tuning precision
- **Static Interference**: Static increases as signal strength decreases
- **Squelch Effect**: Suppresses audio below a certain signal threshold
- **Tuning Sensitivity**: More precise tuning required for weaker signals
- **Atmospheric Effects**: Time-based variations in signal strength

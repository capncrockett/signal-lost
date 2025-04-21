# Audio System Implementation Plan

## Current Issues

1. Audio feedback is missing (no static, beeps, or radio sounds)
2. The AudioManager is implemented but not properly connected to the UI
3. Signal detection lacks appropriate audio cues
4. The overall audio experience is incomplete

## Implementation Plan

### 1. Verify AudioManager Implementation

First, we need to ensure the AudioManager is properly implemented and connected:

1. Check that the AudioManager singleton is registered in the project
2. Verify that it's being properly initialized
3. Ensure it has the necessary methods for generating and playing sounds
4. Connect it to the RadioSystem and PixelRadioInterface

### 2. Implement Static Noise Generation

Static noise is a core element of the radio experience:

```csharp
public void PlayStaticNoise(float intensity)
{
    // Stop any existing static playback
    if (_staticPlayer.Playing)
    {
        _staticPlayer.Stop();
    }
    
    // If intensity is too low, don't play anything
    if (intensity < 0.05f)
    {
        return;
    }
    
    // Create noise buffer
    var buffer = new float[BUFFER_SIZE];
    
    // Fill buffer with random values
    for (int i = 0; i < BUFFER_SIZE; i++)
    {
        buffer[i] = (float)GD.RandRange(-1.0, 1.0) * intensity;
    }
    
    // Apply to audio stream
    var stream = new AudioStreamGenerator();
    stream.MixRate = SAMPLE_RATE;
    _staticPlayer.Stream = stream;
    _staticPlayer.Play();
    
    // Fill the buffer
    var playbackGenerator = _staticPlayer.GetStreamPlayback() as AudioStreamGeneratorPlayback;
    if (playbackGenerator != null)
    {
        playbackGenerator.PushBuffer(buffer);
    }
    
    // Set volume based on intensity
    _staticPlayer.VolumeDb = Mathf.LinearToDb(intensity);
}
```

### 3. Implement Signal Tone Generation

Different types of signals should have different audio characteristics:

```csharp
public void PlaySignalTone(float frequency, float volume, string waveform = "sine")
{
    // Stop any existing signal playback
    if (_signalPlayer.Playing)
    {
        _signalPlayer.Stop();
    }
    
    // If volume is too low, don't play anything
    if (volume < 0.05f)
    {
        return;
    }
    
    // Create buffer
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
    
    // Apply to audio stream
    var stream = new AudioStreamGenerator();
    stream.MixRate = SAMPLE_RATE;
    _signalPlayer.Stream = stream;
    _signalPlayer.Play();
    
    // Fill the buffer
    var playbackGenerator = _signalPlayer.GetStreamPlayback() as AudioStreamGeneratorPlayback;
    if (playbackGenerator != null)
    {
        playbackGenerator.PushBuffer(buffer);
    }
    
    // Set volume based on provided volume
    _signalPlayer.VolumeDb = Mathf.LinearToDb(volume);
}
```

### 4. Implement Morse Code Audio

For Morse code signals, we need to generate the characteristic dots and dashes:

```csharp
public void PlayMorseCode(string message, float volume, float speed = 1.0f)
{
    // Convert message to Morse code
    string morseCode = ConvertToMorse(message);
    
    // Create sequence of tones and silences
    List<(bool isOn, float duration)> sequence = new List<(bool, float)>();
    
    foreach (char c in morseCode)
    {
        switch (c)
        {
            case '.': // Dot
                sequence.Add((true, 1.0f / speed));
                sequence.Add((false, 1.0f / speed));
                break;
            case '-': // Dash
                sequence.Add((true, 3.0f / speed));
                sequence.Add((false, 1.0f / speed));
                break;
            case ' ': // Space between letters
                sequence.Add((false, 3.0f / speed));
                break;
            case '/': // Space between words
                sequence.Add((false, 7.0f / speed));
                break;
        }
    }
    
    // Play the sequence
    PlayMorseSequence(sequence, volume);
}

private string ConvertToMorse(string message)
{
    // Dictionary mapping characters to Morse code
    Dictionary<char, string> morseMap = new Dictionary<char, string>
    {
        {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."},
        {'F', "..-."}, {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"},
        {'K', "-.-"}, {'L', ".-.."}, {'M', "--"}, {'N', "-."}, {'O', "---"},
        {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."}, {'S', "..."}, {'T', "-"},
        {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"}, {'Y', "-.--"},
        {'Z', "--.."}, {'0', "-----"}, {'1', ".----"}, {'2', "..---"},
        {'3', "...--"}, {'4', "....-"}, {'5', "....."}, {'6', "-...."},
        {'7', "--..."}, {'8', "---.."}, {'9', "----."},
        {' ', "/"} // Space between words
    };
    
    // Convert message to uppercase
    message = message.ToUpper();
    
    // Build Morse code string
    string morse = "";
    foreach (char c in message)
    {
        if (morseMap.ContainsKey(c))
        {
            morse += morseMap[c] + " ";
        }
    }
    
    return morse.Trim();
}
```

### 5. Implement UI Sound Effects

Add sound effects for UI interactions:

```csharp
public void PlayButtonClick()
{
    _uiPlayer.Stream = _buttonClickSound;
    _uiPlayer.Play();
}

public void PlayTuningSound()
{
    _uiPlayer.Stream = _tuningSound;
    _uiPlayer.Play();
}

public void PlayPowerOnSound()
{
    _uiPlayer.Stream = _powerOnSound;
    _uiPlayer.Play();
}

public void PlayPowerOffSound()
{
    _uiPlayer.Stream = _powerOffSound;
    _uiPlayer.Play();
}
```

### 6. Implement Signal Processing Effects

Add audio effects to enhance the radio experience:

```csharp
public void ApplyStaticEffect(float intensity)
{
    // Apply low-pass filter
    _lowPassFilter.CutoffHz = Mathf.Lerp(10000, 1000, intensity);
    
    // Apply distortion
    _distortionEffect.Drive = Mathf.Lerp(0, 0.5f, intensity);
    
    // Apply EQ
    _eqEffect.GainDb = Mathf.Lerp(0, -10, intensity);
}
```

### 7. Connect Audio System to Radio Interface

Ensure the audio system is properly connected to the radio interface:

```csharp
// In PixelRadioInterface.cs, _Process method
public override void _Process(double delta)
{
    // Update state based on game systems
    if (_radioSystem != null && _gameState != null)
    {
        _currentFrequency = _gameState.CurrentFrequency;
        _isPowerOn = _gameState.IsRadioOn;
        _isScanning = false; // TODO: Add scanning state to RadioSystem
        _signalStrength = _radioSystem.GetSignalStrength();
        
        // Update audio based on state
        if (_isPowerOn)
        {
            // Get the audio manager
            var audioManager = GetNode<AudioManager>("/root/AudioManager");
            if (audioManager != null)
            {
                if (_signalStrength > 0.1f)
                {
                    // Play signal with appropriate volume
                    audioManager.PlaySignalTone(_currentFrequency * 10, _signalStrength);
                    audioManager.PlayStaticNoise(1.0f - _signalStrength);
                }
                else
                {
                    // Just play static
                    audioManager.StopSignal();
                    audioManager.PlayStaticNoise(1.0f);
                }
            }
        }
        else
        {
            // Stop all audio when radio is off
            var audioManager = GetNode<AudioManager>("/root/AudioManager");
            if (audioManager != null)
            {
                audioManager.StopAll();
            }
        }
    }
    
    // Calculate knob rotation based on frequency
    float frequencyRange = _maxFrequency - _minFrequency;
    float frequencyPercentage = (_currentFrequency - _minFrequency) / frequencyRange;
    _knobRotation = frequencyPercentage * 270.0f; // 270 degrees of rotation
    
    // Redraw
    QueueRedraw();
}
```

### 8. Implement Audio Visualization

Add visual representation of audio:

```csharp
// In DrawVisualizationArea method
private void DrawVisualizationArea()
{
    // ... existing code ...
    
    // Draw visualization content if radio is on
    if (_isPowerOn)
    {
        // Draw signal visualization (simple bars)
        int numBars = 32;
        float barWidth = (vizWidth - (numBars - 1) * 2) / numBars;
        float maxBarHeight = vizHeight - 10;
        
        for (int i = 0; i < numBars; i++)
        {
            // Calculate bar height based on signal strength and some randomness
            float noise = (float)GD.RandRange(-0.2, 0.2);
            float signalFactor = _signalStrength > 0.1f ? _signalStrength : 0;
            float staticFactor = 1.0f - signalFactor;
            
            // Signal component - smooth sine wave
            float signalHeight = 0;
            if (signalFactor > 0)
            {
                float phase = (float)Time.GetTicksMsec() / 200.0f;
                signalHeight = Mathf.Sin(phase + i * 0.2f) * signalFactor * maxBarHeight * 0.5f;
                signalHeight = Mathf.Abs(signalHeight) + (maxBarHeight * 0.1f * signalFactor);
            }
            
            // Static component - random noise
            float staticHeight = (float)GD.RandRange(5, maxBarHeight * 0.7f) * staticFactor;
            
            // Combine signal and static
            float barHeight = Mathf.Max(signalHeight, staticHeight);
            barHeight = Mathf.Min(barHeight, maxBarHeight);
            
            // Calculate bar position
            float barX = vizX + i * (barWidth + 2);
            float barY = vizY + vizHeight - barHeight - 5;
            
            // Draw the bar
            Color barColor = SignalMeterColor.Lerp(new Color(0.7f, 0.7f, 0.7f, 1.0f), staticFactor);
            DrawRect(new Rect2(barX, barY, barWidth, barHeight), barColor);
        }
    }
}
```

## Testing Plan

1. Test static noise generation at different intensities
2. Test signal tone generation at different frequencies
3. Test Morse code playback with different messages
4. Test UI sound effects
5. Test audio visualization
6. Test audio effects
7. Test integration with the radio interface
8. Test on both Windows and Mac platforms

## Implementation Steps

1. Verify AudioManager implementation
2. Implement static noise generation
3. Implement signal tone generation
4. Implement Morse code audio
5. Implement UI sound effects
6. Implement signal processing effects
7. Connect audio system to radio interface
8. Implement audio visualization
9. Test and refine

## Success Criteria

1. Static noise is generated and played at appropriate times
2. Signal tones are generated and played when signals are detected
3. Morse code is properly converted and played
4. UI sound effects enhance the user experience
5. Audio visualization correlates with audio playback
6. The overall audio experience is immersive and enhances gameplay

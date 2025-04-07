# Signal Lost - Audio System Architecture

This document describes the audio system architecture used in the Signal Lost game.

## Overview

The audio system in Signal Lost is built using the Web Audio API, which provides low-level control over audio processing and synthesis. This approach was chosen over Phaser's built-in audio system to enable more sophisticated audio effects, particularly for the radio tuning mechanics that are central to the game.

## Key Components

### 1. SoundscapeManager

The `SoundscapeManager` class is responsible for managing ambient sounds and background music. It handles:

- Loading and playing ambient audio loops
- Crossfading between different soundscapes
- Managing global volume levels
- Handling audio context initialization

```typescript
// Example usage
const soundscapeManager = new SoundscapeManager(scene);
soundscapeManager.playAmbience('forest');
soundscapeManager.crossfadeTo('ruins', 3000);
```

### 2. RadioTuner

The `RadioTuner` class is the core of the game's audio experience. It simulates a radio tuning experience with:

- Dynamic static/noise generation
- Signal detection based on frequency
- Interference and signal clarity effects
- Audio visualization

```typescript
// Example usage
const radioTuner = new RadioTuner(scene);
radioTuner.setFrequency(91.5); // Tune to 91.5 MHz
radioTuner.detectSignal(); // Check for signals at current frequency
```

### 3. VolumeControl

The `VolumeControl` class provides a UI for adjusting volume levels:

- Master volume control
- Individual volume controls for different audio types
- Volume persistence using SaveManager

```typescript
// Example usage
const volumeControl = new VolumeControl(scene);
volumeControl.setMasterVolume(0.8);
```

## Audio Context and Nodes

The audio system uses a shared AudioContext and a graph of AudioNodes:

```
AudioContext
    |
    ├── OscillatorNode (static generator)
    |       └── GainNode (static volume)
    |               └── AnalyserNode (visualization)
    |                       └── AudioDestinationNode
    |
    ├── AudioBufferSourceNode (signal audio)
    |       └── GainNode (signal volume)
    |               └── AudioDestinationNode
    |
    └── AudioBufferSourceNode (ambient audio)
            └── GainNode (ambient volume)
                    └── AudioDestinationNode
```

## Signal Generation

### Static Noise

Static noise is generated using an OscillatorNode with noise-like waveforms:

```typescript
// Pink noise generation (example)
const bufferSize = 4096;
const noiseBuffer = audioContext.createBuffer(1, bufferSize, audioContext.sampleRate);
const output = noiseBuffer.getChannelData(0);

// Fill with pink noise
let b0 = 0, b1 = 0, b2 = 0, b3 = 0, b4 = 0, b5 = 0, b6 = 0;
for (let i = 0; i < bufferSize; i++) {
  const white = Math.random() * 2 - 1;
  b0 = 0.99886 * b0 + white * 0.0555179;
  b1 = 0.99332 * b1 + white * 0.0750759;
  b2 = 0.96900 * b2 + white * 0.1538520;
  b3 = 0.86650 * b3 + white * 0.3104856;
  b4 = 0.55000 * b4 + white * 0.5329522;
  b5 = -0.7616 * b5 - white * 0.0168980;
  output[i] = b0 + b1 + b2 + b3 + b4 + b5 + b6 + white * 0.5362;
  output[i] *= 0.11; // (roughly) compensate for gain
  b6 = white * 0.115926;
}
```

### Signal Clarity

Signal clarity is simulated by adjusting the ratio between the static noise and the signal audio:

```typescript
// Example of adjusting signal clarity
const signalStrength = this.getSignalStrength(frequency);
this.staticGain.gain.value = 1.0 - signalStrength;
this.signalGain.gain.value = signalStrength;
```

## Frequency Mapping

The game maps frequencies to signals using a configuration system:

```typescript
// Example frequency mapping
const signals = [
  {
    id: 'signal1',
    frequency: 91.5,
    range: 0.5,
    type: 'location',
    data: {
      x: 10,
      y: 8,
      name: 'Radio Tower'
    }
  },
  // More signals...
];
```

## Audio Visualization

The audio system includes visualization using AnalyserNode:

```typescript
// Example of audio visualization
const analyser = audioContext.createAnalyser();
analyser.fftSize = 256;
const bufferLength = analyser.frequencyBinCount;
const dataArray = new Uint8Array(bufferLength);

// In render loop
analyser.getByteFrequencyData(dataArray);
// Draw visualization using dataArray
```

## Performance Considerations

The audio system is designed with performance in mind:

1. **Resource Management**: Audio nodes are created only when needed and properly disposed
2. **Buffer Reuse**: Audio buffers are reused when possible
3. **Throttling**: Audio processing is throttled in non-critical sections
4. **Lazy Loading**: Audio files are loaded on demand

## Browser Compatibility

The audio system includes fallbacks for browser compatibility:

```typescript
// Example of browser compatibility check
if (window.AudioContext || window.webkitAudioContext) {
  const AudioContextClass = window.AudioContext || window.webkitAudioContext;
  this.audioContext = new AudioContextClass();
} else {
  console.error('Web Audio API not supported in this browser');
  // Fallback to basic audio
}
```

## Future Improvements

Planned improvements to the audio system:

1. **Spatial Audio**: Implement 3D audio for field exploration
2. **Audio Filters**: Add more sophisticated filters for signal processing
3. **Procedural Audio**: Generate more audio content procedurally
4. **Audio Caching**: Improve caching for faster loading
5. **Compression**: Optimize audio file sizes

## Testing the Audio System

The audio system includes comprehensive tests:

```typescript
// Example of audio system test
test('should adjust volume correctly', () => {
  const soundManager = new SoundscapeManager(mockScene);
  soundManager.setVolume(0.5);
  expect(soundManager.getVolume()).toBe(0.5);
});
```

E2E tests also verify audio functionality by checking for audio context creation and monitoring console logs for audio-related messages.

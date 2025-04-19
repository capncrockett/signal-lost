using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class AudioManager : Node
    {
        // Audio properties
        private float _volume = 0.8f;
        private bool _isMuted = false;

        // Audio nodes (will be created at runtime)
        private AudioStreamPlayer _staticPlayer;
        private AudioStreamPlayer _signalPlayer;
        private AudioStreamPlayer _effectPlayer;

        // Noise types
        public enum NoiseType
        {
            White,
            Pink,
            Brown,
            Digital
        }

        private NoiseType _currentNoiseType = NoiseType.White;

        // Audio bus indices
        private int _masterBusIdx = 0;
        private int _staticBusIdx = 1;
        private int _signalBusIdx = 2;

        // Initialize audio system
        public override void _Ready()
        {
            // Create audio players
            _staticPlayer = new AudioStreamPlayer();
            _signalPlayer = new AudioStreamPlayer();
            _effectPlayer = new AudioStreamPlayer();

            // Add to scene tree
            AddChild(_staticPlayer);
            AddChild(_signalPlayer);
            AddChild(_effectPlayer);

            // Set up audio buses
            _masterBusIdx = AudioServer.GetBusIndex("Master");

            // Create static bus
            AudioServer.AddBus();
            _staticBusIdx = AudioServer.GetBusCount() - 1;
            AudioServer.SetBusName(_staticBusIdx, "Static");
            AudioServer.SetBusSend(_staticBusIdx, "Master");

            // Create signal bus
            AudioServer.AddBus();
            _signalBusIdx = AudioServer.GetBusCount() - 1;
            AudioServer.SetBusName(_signalBusIdx, "Signal");
            AudioServer.SetBusSend(_signalBusIdx, "Master");

            // Set up effects
            SetupAudioEffects();

            // Set initial volume
            SetVolume(_volume);
        }

        // Set up audio effects for buses
        private void SetupAudioEffects()
        {
            // Add EQ to static bus
            var staticEq = new AudioEffectEQ();
            staticEq.SetBandGainDb(0, -10.0f);  // Cut very low frequencies
            staticEq.SetBandGainDb(1, -5.0f);   // Reduce low frequencies
            staticEq.SetBandGainDb(2, 0.0f);    // Keep low-mid frequencies
            staticEq.SetBandGainDb(3, 3.0f);    // Boost mid frequencies
            staticEq.SetBandGainDb(4, 5.0f);    // Boost mid-high frequencies
            staticEq.SetBandGainDb(5, 4.0f);    // Boost high-mid frequencies
            AudioServer.AddBusEffect(_staticBusIdx, staticEq);

            // Add distortion to static bus
            var distortion = new AudioEffectDistortion();
            distortion.Mode = AudioEffectDistortion.ModeEnum.Lofi;
            distortion.Drive = 0.2f;
            distortion.PreGain = 0.4f;
            AudioServer.AddBusEffect(_staticBusIdx, distortion);

            // Add bandpass filter to static bus for radio-like sound
            var bandpass = new AudioEffectBandPassFilter();
            bandpass.CutoffHz = 2500.0f;
            bandpass.Resonance = 0.5f;
            bandpass.Gain = 1.2f;
            AudioServer.AddBusEffect(_staticBusIdx, bandpass);

            // Add EQ to signal bus
            var signalEq = new AudioEffectEQ();
            signalEq.SetBandGainDb(0, -15.0f);  // Cut very low frequencies
            signalEq.SetBandGainDb(1, -5.0f);   // Reduce low frequencies
            signalEq.SetBandGainDb(2, 3.0f);    // Boost mid-low frequencies
            signalEq.SetBandGainDb(3, 5.0f);    // Boost mid-high frequencies
            signalEq.SetBandGainDb(4, 0.0f);    // Normal high frequencies
            signalEq.SetBandGainDb(5, -8.0f);   // Cut very high frequencies
            AudioServer.AddBusEffect(_signalBusIdx, signalEq);

            // Add bandpass filter to signal bus for radio-like sound
            var signalBandpass = new AudioEffectBandPassFilter();
            signalBandpass.CutoffHz = 1800.0f;
            signalBandpass.Resonance = 0.7f;
            signalBandpass.Gain = 1.5f;
            AudioServer.AddBusEffect(_signalBusIdx, signalBandpass);

            // Add slight reverb to signal bus
            var reverb = new AudioEffectReverb();
            reverb.Wet = 0.1f;
            reverb.Dry = 0.9f;
            reverb.Damping = 0.6f;
            reverb.Spread = 0.2f;
            AudioServer.AddBusEffect(_signalBusIdx, reverb);
        }

        // Noise generation state
        private Random _random = new Random();
        private float[] _pinkNoiseBuffer = new float[7];
        private float _brownNoiseLastValue = 0.0f;

        // Current static noise intensity for continuous playback
        private float _currentStaticIntensity = 0.0f;
        private float _lastSample = 0.0f;
        private AudioStreamGeneratorPlayback _staticPlayback;
        private bool _isStaticInitialized = false;

        // Create a looping noise stream with given intensity
        private AudioStream CreateLoopingNoiseStream(float intensity)
        {
            try
            {
                // Store the current intensity for continuous playback
                _currentStaticIntensity = intensity;

                // Create a simple noise sample with a longer buffer for better continuity
                var audioStreamGenerator = new AudioStreamGenerator();
                audioStreamGenerator.MixRate = 44100;
                audioStreamGenerator.BufferLength = 2.0f; // 2 second buffer for better continuity

                // Set up the stream in the player
                _staticPlayer.Stream = audioStreamGenerator;
                _staticPlayer.Play();

                // Get the playback instance and store it for continuous filling
                _staticPlayback = (AudioStreamGeneratorPlayback)_staticPlayer.GetStreamPlayback();
                if (_staticPlayback == null)
                {
                    GD.PrintErr("Failed to get audio playback instance");
                    return audioStreamGenerator;
                }

                // Mark as initialized
                _isStaticInitialized = true;

                // Fill the buffer initially
                FillStaticNoiseBuffer();

                return audioStreamGenerator;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error creating looping noise stream: {ex.Message}");
                return null;
            }
        }

        // Fill the static noise buffer - called continuously to keep audio playing
        private void FillStaticNoiseBuffer()
        {
            if (!_isStaticInitialized || _staticPlayback == null) return;

            try
            {
                // Check if we need to fill the buffer
                int availableFrames = _staticPlayback.GetFramesAvailable();
                if (availableFrames < 1000) return; // Skip if buffer is mostly full

                // Reset noise generation state for consistency if buffer is nearly empty
                if (availableFrames > 80000)
                {
                    Array.Clear(_pinkNoiseBuffer, 0, _pinkNoiseBuffer.Length);
                    _brownNoiseLastValue = 0.0f;
                }

                // Generate noise samples to fill the buffer
                for (int i = 0; i < availableFrames; i++)
                {
                    float sample = 0.0f;

                    // Generate different types of noise
                    switch (_currentNoiseType)
                    {
                        case NoiseType.White:
                            sample = GenerateWhiteNoise();
                            break;
                        case NoiseType.Pink:
                            sample = GeneratePinkNoise();
                            break;
                        case NoiseType.Brown:
                            sample = GenerateBrownNoise();
                            break;
                        case NoiseType.Digital:
                            sample = GenerateDigitalNoise();
                            break;
                    }

                    // Apply intensity with a gentle curve for more natural fading
                    float adjustedIntensity = Mathf.Pow(_currentStaticIntensity, 1.2f);
                    sample *= adjustedIntensity;

                    // Add occasional crackle effect for realism (less frequent)
                    if (_random.NextDouble() < 0.005f * _currentStaticIntensity)
                    {
                        sample += (float)(_random.NextDouble() * 0.3f - 0.15f) * _currentStaticIntensity;
                    }

                    // Apply a very subtle low-pass filter to smooth out harsh frequencies
                    _lastSample = _lastSample * 0.2f + sample * 0.8f;
                    sample = _lastSample;

                    // Push the frame to the audio buffer
                    _staticPlayback.PushFrame(new Vector2(sample, sample));
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error filling static noise buffer: {ex.Message}");
            }
        }

        // Generate white noise (equal energy per frequency)
        private float GenerateWhiteNoise()
        {
            return (float)_random.NextDouble() * 2.0f - 1.0f;
        }

        // Generate pink noise (1/f spectrum - more natural sounding)
        private float GeneratePinkNoise()
        {
            return GeneratePinkNoiseFromSample(GenerateWhiteNoise());
        }

        // Generate pink noise from a provided white noise sample
        private float GeneratePinkNoiseFromSample(float white)
        {
            // Paul Kellet's refined method for pink noise generation
            _pinkNoiseBuffer[0] = 0.99886f * _pinkNoiseBuffer[0] + white * 0.0555179f;
            _pinkNoiseBuffer[1] = 0.99332f * _pinkNoiseBuffer[1] + white * 0.0750759f;
            _pinkNoiseBuffer[2] = 0.96900f * _pinkNoiseBuffer[2] + white * 0.1538520f;
            _pinkNoiseBuffer[3] = 0.86650f * _pinkNoiseBuffer[3] + white * 0.3104856f;
            _pinkNoiseBuffer[4] = 0.55000f * _pinkNoiseBuffer[4] + white * 0.5329522f;
            _pinkNoiseBuffer[5] = -0.7616f * _pinkNoiseBuffer[5] - white * 0.0168980f;

            float pink = _pinkNoiseBuffer[0] + _pinkNoiseBuffer[1] + _pinkNoiseBuffer[2] + _pinkNoiseBuffer[3] + _pinkNoiseBuffer[4] + _pinkNoiseBuffer[5] + _pinkNoiseBuffer[6] + white * 0.5362f;
            _pinkNoiseBuffer[6] = white * 0.115926f;

            // Normalize to -1.0 to 1.0 range
            return pink * 0.11f;
        }

        // Generate brown noise (1/f² spectrum - deeper rumble)
        private float GenerateBrownNoise()
        {
            return GenerateBrownNoiseFromSample(GenerateWhiteNoise());
        }

        // Generate brown noise from a provided white noise sample
        private float GenerateBrownNoiseFromSample(float white)
        {
            // Simple first-order low-pass filter
            _brownNoiseLastValue = (_brownNoiseLastValue + (0.02f * white)) / 1.02f;

            // Normalize to -1.0 to 1.0 range
            return _brownNoiseLastValue * 3.5f;
        }

        // Generate digital noise (harsh, bit-crushed sound)
        private float GenerateDigitalNoise()
        {
            return GenerateDigitalNoiseFromSample(GenerateWhiteNoise());
        }

        // Generate digital noise from a provided white noise sample
        private float GenerateDigitalNoiseFromSample(float raw)
        {
            // Quantize to fewer steps for digital sound
            int steps = 8;
            return Mathf.Round(raw * steps) / steps;
        }

        // Timer for continuous static noise generation
        private Timer _staticNoiseTimer;

        // Play static noise with given intensity
        public void PlayStaticNoise(float intensity)
        {
            try
            {
                // If intensity is very low, just reduce volume but don't stop
                if (intensity < 0.05f)
                {
                    intensity = 0.05f; // Keep a minimum level of static
                }

                // Update the current intensity for continuous playback
                _currentStaticIntensity = intensity;

                // Set up the static player
                _staticPlayer.Bus = "Static";

                // If already playing, just adjust volume for smoother transition
                if (_staticPlayer.Playing)
                {
                    // Smoothly adjust volume using interpolation for more natural transitions
                    float currentDb = _staticPlayer.VolumeDb;
                    float targetDb = Mathf.LinearToDb(intensity * _volume);
                    _staticPlayer.VolumeDb = Mathf.Lerp(currentDb, targetDb, 0.3f);

                    // Make sure the buffer stays filled
                    FillStaticNoiseBuffer();
                    return;
                }

                // Set volume before playing
                _staticPlayer.VolumeDb = Mathf.LinearToDb(intensity * _volume);

                // Create a simple looping noise stream - this will set up the stream and start playback
                CreateLoopingNoiseStream(intensity);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error in PlayStaticNoise: {ex.Message}");
            }
        }

        // Stop static noise
        public void StopStaticNoise()
        {
            // Instead of stopping, just set volume very low
            _staticPlayer.VolumeDb = -80.0f; // Nearly silent
            _currentStaticIntensity = 0.0f;
        }

        // Signal playback state
        private AudioStreamGeneratorPlayback _signalPlayback;
        private bool _isSignalInitialized = false;
        private float _currentSignalVolume = 1.0f;
        private string _currentWaveform = "sine";
        private bool _isBeepMode = false;
        private float _beepPhase = 0.0f;
        private float _morseTimer = 0.0f;
        private int _morseIndex = 0;

        // Morse code for "TEST": - (T) . (E) ... (S) - (T)
        private readonly bool[] _morseTest = new bool[] {
            // T: -
            true, false,  // Dash, then symbol space

            // Letter space between T and E
            false,

            // E: .
            true, false,  // Dot, then symbol space

            // Letter space between E and S
            false,

            // S: ...
            true, false,  // Dot, then symbol space
            true, false,  // Dot, then symbol space
            true, false,  // Dot, then symbol space

            // Letter space between S and T
            false,

            // T: -
            true, false,  // Dash, then symbol space

            // Word space at the end before repeating
            false, false
        };

        // Morse timing for each element (in seconds)
        private readonly float[] _morseTiming = new float[] {
            // T: -
            0.3f, 0.1f,  // Dash duration, symbol space

            // Letter space between T and E
            0.3f,

            // E: .
            0.1f, 0.1f,  // Dot duration, symbol space

            // Letter space between E and S
            0.3f,

            // S: ...
            0.1f, 0.1f,  // Dot duration, symbol space
            0.1f, 0.1f,  // Dot duration, symbol space
            0.1f, 0.1f,  // Dot duration, symbol space

            // Letter space between S and T
            0.3f,

            // T: -
            0.3f, 0.1f,  // Dash duration, symbol space

            // Word space at the end before repeating
            0.7f, 0.7f
        };

        // Play signal tone at specified frequency
        public AudioStreamGenerator PlaySignal(float frequency, float volumeScale = 1.0f, string waveform = "sine", bool beepMode = false)
        {
            try
            {
                // If volume scale is very low, just stop the player
                if (volumeScale < 0.05f)
                {
                    StopSignal();
                    return null;
                }

                // Store current parameters for continuous playback
                _currentSignalVolume = volumeScale;
                _currentWaveform = waveform;
                _isBeepMode = beepMode;

                // If already playing and frequency is close, just adjust volume for smoother transition
                if (_signalPlayer.Playing && _currentSignalFrequency > 0 &&
                    Mathf.Abs(_currentSignalFrequency - frequency) < 0.5f)
                {
                    // Smoothly adjust volume
                    float currentDb = _signalPlayer.VolumeDb;
                    float targetDb = Mathf.LinearToDb(volumeScale * _volume);
                    _signalPlayer.VolumeDb = Mathf.Lerp(currentDb, targetDb, 0.3f);

                    // Play squelch on effect if we're newly detecting a strong signal
                    if (beepMode && !_isBeepMode)
                    {
                        PlaySquelchOn();
                    }

                    _isBeepMode = beepMode;
                    return (AudioStreamGenerator)_signalPlayer.Stream;
                }

                // If we're starting a new signal and it's a strong one, play squelch on
                if (beepMode && (!_signalPlayer.Playing || _currentSignalFrequency == 0))
                {
                    PlaySquelchOn();
                }

                // Stop if playing
                if (_signalPlayer.Playing)
                {
                    _signalPlayer.Stop();
                }

                // Store current frequency for future reference
                _currentSignalFrequency = frequency;

                // Create new generator with longer buffer for better continuity
                var generator = new AudioStreamGenerator();
                generator.MixRate = 44100;
                generator.BufferLength = 1.0f;  // 1 second buffer for better continuity

                // Set up the player
                _signalPlayer.Stream = generator;
                _signalPlayer.Bus = "Signal";
                _signalPlayer.VolumeDb = Mathf.LinearToDb(volumeScale * _volume);
                _signalPlayer.Play();

                // Get the playback instance and store it for continuous filling
                _signalPlayback = (AudioStreamGeneratorPlayback)_signalPlayer.GetStreamPlayback();
                if (_signalPlayback == null)
                {
                    GD.PrintErr("Failed to get signal playback instance");
                    return generator;
                }

                // Mark as initialized
                _isSignalInitialized = true;

                // Reset morse code state
                _beepPhase = 0.0f;
                _morseTimer = 0.0f;
                _morseIndex = 0;

                // Fill the buffer initially
                FillSignalBuffer();

                return generator;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error playing signal: {ex.Message}");
                return null;
            }
        }

        // Fill the signal buffer - called continuously to keep audio playing
        private void FillSignalBuffer()
        {
            if (!_isSignalInitialized || _signalPlayback == null) return;

            try
            {
                // Check if we need to fill the buffer
                int availableFrames = _signalPlayback.GetFramesAvailable();
                if (availableFrames < 1000) return; // Skip if buffer is mostly full

                // Fill the buffer with the waveform
                float phase = _beepPhase; // Continue from last phase for smooth waveform
                float baseIncrement = _currentSignalFrequency / 44100.0f;
                float morseTimer = _morseTimer; // For morse code timing
                int morseIndex = _morseIndex; // Current position in morse code

                // Note: Morse code timing is now defined in the _morseTiming array

                for (int i = 0; i < availableFrames; i++)
                {
                    float sample = 0.0f;

                    // Add slight frequency modulation for more realistic radio sound
                    float modulation = 1.0f + (float)Math.Sin(i * 0.0001f) * 0.001f;
                    float increment = baseIncrement * modulation;

                    // Determine if we should generate a tone based on morse code
                    bool generateTone = true;

                    if (_isBeepMode)
                    {
                        // Get the current morse code element
                        bool isOn = _morseTest[morseIndex];

                        // Get the timing for this element
                        float elementDuration = _morseTiming[morseIndex];

                        // Only generate tone when the element is "on"
                        generateTone = isOn;

                        // Update the timer and move to next element if needed
                        morseTimer += 1.0f / 44100.0f; // Increment by sample duration
                        if (morseTimer >= elementDuration)
                        {
                            morseTimer = 0.0f;
                            morseIndex = (morseIndex + 1) % _morseTest.Length;
                        }
                    }

                    if (generateTone)
                    {
                        // Generate different waveforms
                        switch (_currentWaveform)
                        {
                            case "sine":
                                sample = Mathf.Sin(phase * 2.0f * Mathf.Pi);
                                break;
                            case "square":
                                sample = (phase % 1.0f) < 0.5f ? 1.0f : -1.0f;
                                break;
                            case "triangle":
                                float t = phase % 1.0f;
                                sample = 2.0f * (t < 0.5f ? t : 1.0f - t) - 1.0f;
                                break;
                            case "sawtooth":
                                sample = 2.0f * (phase % 1.0f) - 1.0f;
                                break;
                        }

                        // Apply volume scale with slight random variation for realism
                        float randomVariation = 1.0f + ((float)_random.NextDouble() * 0.02f - 0.01f);
                        sample *= _currentSignalVolume * randomVariation;
                    }

                    // Push the frame to the audio buffer
                    _signalPlayback.PushFrame(new Vector2(sample, sample));

                    // Always update phase for continuous waveform
                    phase += increment;
                }

                // Store the current state for next buffer fill
                _beepPhase = phase % 1.0f; // Keep phase in 0-1 range
                _morseTimer = morseTimer;
                _morseIndex = morseIndex;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error filling signal buffer: {ex.Message}");
            }
        }

        // Current signal frequency (for smooth transitions)
        private float _currentSignalFrequency = 0.0f;

        // Stop signal tone
        public void StopSignal()
        {
            // Play squelch off effect if we were in beep mode
            if (_signalPlayer.Playing && _isBeepMode)
            {
                PlaySquelchOff();
            }

            // Reset signal state
            _signalPlayer.Stop();
            _isSignalInitialized = false;
            _currentSignalFrequency = 0.0f;
            _isBeepMode = false;
        }

        // Play a sound effect
        public void PlayEffect(string effectName)
        {
            var effectPath = "res://assets/audio/" + effectName + ".wav";
            var effect = GD.Load<AudioStream>(effectPath);

            if (effect != null)
            {
                _effectPlayer.Stream = effect;
                _effectPlayer.Play();
            }
        }

        // Play squelch on effect (chirp when signal starts)
        public void PlaySquelchOn()
        {
            try
            {
                // Create a short chirp sound
                var generator = new AudioStreamGenerator();
                generator.MixRate = 44100;
                generator.BufferLength = 0.15f; // 150ms chirp

                _effectPlayer.Stream = generator;
                _effectPlayer.Bus = "Signal";
                _effectPlayer.VolumeDb = Mathf.LinearToDb(0.8f * _volume);
                _effectPlayer.Play();

                var playback = (AudioStreamGeneratorPlayback)_effectPlayer.GetStreamPlayback();
                if (playback == null) return;

                // Generate a more subtle rising tone (squelch on effect)
                int bufferSize = (int)(generator.BufferLength * generator.MixRate);
                float phase = 0.0f;

                for (int i = 0; i < bufferSize; i++)
                {
                    // Frequency rises more subtly from 600Hz to 800Hz
                    float progress = (float)i / bufferSize;
                    float freq = 600.0f + (200.0f * progress);
                    float increment = freq / generator.MixRate;

                    // Generate sine wave
                    float sample = Mathf.Sin(phase * 2.0f * Mathf.Pi);

                    // Apply smoother envelope
                    float envelope;
                    if (progress < 0.15f) // Attack (first 15%)
                        envelope = progress / 0.15f;
                    else if (progress > 0.6f) // Decay (last 40%)
                        envelope = (1.0f - progress) / 0.4f;
                    else // Sustain (middle 45%)
                        envelope = 1.0f;

                    // Reduce overall volume
                    sample *= envelope * 0.7f;

                    // Add a bit of noise for realism, but less than before
                    if (progress > 0.5f)
                        sample += GenerateWhiteNoise() * 0.03f * (progress - 0.5f) / 0.5f;

                    playback.PushFrame(new Vector2(sample, sample));
                    phase += increment;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error playing squelch on: {ex.Message}");
            }
        }

        // Play squelch off effect (chirp when signal ends)
        public void PlaySquelchOff()
        {
            try
            {
                // Create a short chirp sound
                var generator = new AudioStreamGenerator();
                generator.MixRate = 44100;
                generator.BufferLength = 0.15f; // 150ms chirp

                _effectPlayer.Stream = generator;
                _effectPlayer.Bus = "Signal";
                _effectPlayer.VolumeDb = Mathf.LinearToDb(0.8f * _volume);
                _effectPlayer.Play();

                var playback = (AudioStreamGeneratorPlayback)_effectPlayer.GetStreamPlayback();
                if (playback == null) return;

                // Generate a more subtle falling tone (squelch off effect)
                int bufferSize = (int)(generator.BufferLength * generator.MixRate);
                float phase = 0.0f;

                for (int i = 0; i < bufferSize; i++)
                {
                    // Frequency falls more subtly from 800Hz to 600Hz
                    float progress = (float)i / bufferSize;
                    float freq = 800.0f - (200.0f * progress);
                    float increment = freq / generator.MixRate;

                    // Generate sine wave
                    float sample = Mathf.Sin(phase * 2.0f * Mathf.Pi);

                    // Apply smoother envelope with faster decay
                    float envelope;
                    if (progress < 0.1f) // Attack (first 10%)
                        envelope = progress / 0.1f;
                    else // Faster decay (remaining 90%)
                        envelope = (1.0f - progress) / 0.6f;

                    // Reduce overall volume
                    sample *= envelope * 0.6f;

                    // Add less noise for realism
                    sample += GenerateWhiteNoise() * 0.05f * progress;

                    playback.PushFrame(new Vector2(sample, sample));
                    phase += increment;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error playing squelch off: {ex.Message}");
            }
        }

        // Set the volume (0.0 to 1.0)
        public void SetVolume(float vol)
        {
            _volume = Mathf.Clamp(vol, 0.0f, 1.0f);
            AudioServer.SetBusVolumeDb(_masterBusIdx, Mathf.LinearToDb(_volume));
        }

        // Toggle mute
        public void ToggleMute()
        {
            _isMuted = !_isMuted;
            AudioServer.SetBusMute(_masterBusIdx, _isMuted);
        }

        // Set mute state directly
        public void SetMuted(bool isMuted)
        {
            if (_isMuted != isMuted)
            {
                _isMuted = isMuted;
                AudioServer.SetBusMute(_masterBusIdx, _isMuted);
            }
        }

        // Set the noise type
        public void SetNoiseType(NoiseType type)
        {
            _currentNoiseType = type;
        }

        // Process method called every frame
        public override void _Process(double delta)
        {
            // Keep the static noise buffer filled
            if (_isStaticInitialized && _staticPlayer.Playing)
            {
                FillStaticNoiseBuffer();
            }

            // Keep the signal buffer filled
            if (_isSignalInitialized && _signalPlayer.Playing)
            {
                FillSignalBuffer();
            }
        }
    }
}

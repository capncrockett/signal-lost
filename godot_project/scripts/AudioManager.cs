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

        // Generate a noise stream with given intensity
        private AudioStreamGenerator GenerateNoiseStream(float intensity)
        {
            try
            {
                // Create a new generator with a longer buffer for better continuity
                var noiseGenerator = new AudioStreamGenerator();
                noiseGenerator.MixRate = 44100;
                noiseGenerator.BufferLength = 1.0f;  // 1 second buffer for better continuity

                // Set up the stream in the player
                _staticPlayer.Stream = noiseGenerator;
                _staticPlayer.Play();

                // Get the playback instance
                var playback = (AudioStreamGeneratorPlayback)_staticPlayer.GetStreamPlayback();
                if (playback == null)
                {
                    GD.PrintErr("Failed to get audio playback instance");
                    return noiseGenerator;
                }

                // Fill the buffer with noise
                var bufferSize = (int)(noiseGenerator.BufferLength * noiseGenerator.MixRate);

                // Reset noise generation state for consistency
                Array.Clear(_pinkNoiseBuffer, 0, _pinkNoiseBuffer.Length);
                _brownNoiseLastValue = 0.0f;

                // Pre-generate white noise for efficiency
                float[] whiteNoise = new float[bufferSize];
                for (int i = 0; i < bufferSize; i++)
                {
                    whiteNoise[i] = (float)_random.NextDouble() * 2.0f - 1.0f;
                }

                // Process the noise based on the selected type
                for (int i = 0; i < bufferSize; i++)
                {
                    float sample = 0.0f;

                    // Generate different types of noise
                    switch (_currentNoiseType)
                    {
                        case NoiseType.White:
                            sample = whiteNoise[i];
                            break;
                        case NoiseType.Pink:
                            // Use pre-generated white noise
                            sample = GeneratePinkNoiseFromSample(whiteNoise[i]);
                            break;
                        case NoiseType.Brown:
                            // Use pre-generated white noise
                            sample = GenerateBrownNoiseFromSample(whiteNoise[i]);
                            break;
                        case NoiseType.Digital:
                            // Use pre-generated white noise
                            sample = GenerateDigitalNoiseFromSample(whiteNoise[i]);
                            break;
                    }

                    // Apply intensity with a gentle curve for more natural fading
                    float adjustedIntensity = Mathf.Pow(intensity, 1.2f);
                    sample *= adjustedIntensity;

                    // Add occasional crackle effect for realism (less frequent)
                    if (_random.NextDouble() < 0.005f * intensity)
                    {
                        sample += (float)(_random.NextDouble() * 0.3f - 0.15f) * intensity;
                    }

                    // Apply a very subtle low-pass filter to smooth out harsh frequencies
                    _lastSample = _lastSample * 0.2f + sample * 0.8f;
                    sample = _lastSample;

                    // Push the frame to the audio buffer
                    playback.PushFrame(new Vector2(sample, sample));
                }

                // Set the player to loop for continuous playback
                _staticPlayer.Autoplay = true;

                return noiseGenerator;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error generating noise stream: {ex.Message}");
                return null;
            }
        }

        // Last sample for simple low-pass filtering
        private float _lastSample = 0.0f;

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
        private float _currentStaticIntensity = 0.0f;

        // Play static noise with given intensity
        public void PlayStaticNoise(float intensity)
        {
            // Store the current intensity for use in the timer callback
            _currentStaticIntensity = intensity;

            // If intensity is very low, just stop the player
            if (intensity < 0.05f)
            {
                StopStaticNoise();
                return;
            }

            // Set up the static player
            _staticPlayer.Bus = "Static";
            _staticPlayer.VolumeDb = Mathf.LinearToDb(intensity * _volume);

            // Create and start the timer if it doesn't exist
            if (_staticNoiseTimer == null)
            {
                _staticNoiseTimer = new Timer();
                _staticNoiseTimer.WaitTime = 0.2f; // Refresh noise every 200ms
                _staticNoiseTimer.Timeout += OnStaticNoiseTimerTimeout;
                _staticNoiseTimer.Autostart = true;
                AddChild(_staticNoiseTimer);
            }

            // Generate and play new noise stream immediately
            GenerateNoiseStream(intensity);
        }

        // Timer callback to continuously generate static noise
        private void OnStaticNoiseTimerTimeout()
        {
            if (_currentStaticIntensity >= 0.05f && !_staticPlayer.Playing)
            {
                // Regenerate the noise to keep it continuous
                GenerateNoiseStream(_currentStaticIntensity);
            }
        }

        // Stop static noise
        public void StopStaticNoise()
        {
            // Stop the player
            _staticPlayer.Stop();

            // Reset the current intensity
            _currentStaticIntensity = 0.0f;

            // Stop and remove the timer if it exists
            if (_staticNoiseTimer != null)
            {
                _staticNoiseTimer.Stop();
            }
        }

        // Play signal tone at specified frequency
        public AudioStreamGenerator PlaySignal(float frequency, float volumeScale = 1.0f, string waveform = "sine")
        {
            // If volume scale is very low, just stop the player
            if (volumeScale < 0.05f)
            {
                StopSignal();
                return null;
            }

            // If already playing and frequency is close, just adjust volume for smoother transition
            if (_signalPlayer.Playing && _currentSignalFrequency > 0 &&
                Mathf.Abs(_currentSignalFrequency - frequency) < 0.5f)
            {
                // Smoothly adjust volume
                _signalPlayer.VolumeDb = Mathf.LinearToDb(volumeScale * _volume);
                return (AudioStreamGenerator)_signalPlayer.Stream;
            }

            // Stop if playing
            if (_signalPlayer.Playing)
            {
                _signalPlayer.Stop();
            }

            // Store current frequency for future reference
            _currentSignalFrequency = frequency;

            // Create new generator
            var generator = new AudioStreamGenerator();
            generator.MixRate = 44100;
            generator.BufferLength = 0.2f;  // 200ms buffer for smoother looping

            // Set up the player
            _signalPlayer.Stream = generator;
            _signalPlayer.Bus = "Signal";
            _signalPlayer.VolumeDb = Mathf.LinearToDb(volumeScale * _volume);
            _signalPlayer.Play();

            // Get the playback instance
            var playback = (AudioStreamGeneratorPlayback)_signalPlayer.GetStreamPlayback();

            // Fill the buffer with the waveform
            var bufferSize = (int)(generator.BufferLength * generator.MixRate);
            float phase = 0.0f;
            float baseIncrement = frequency / generator.MixRate;

            for (int i = 0; i < bufferSize; i++)
            {
                float sample = 0.0f;

                // Add slight frequency modulation for more realistic radio sound
                float modulation = 1.0f + (float)Math.Sin(i * 0.0001f) * 0.001f;
                float increment = baseIncrement * modulation;

                // Generate different waveforms
                switch (waveform)
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
                sample *= volumeScale * randomVariation;

                playback.PushFrame(new Vector2(sample, sample));
                phase += increment;
            }

            return generator;
        }

        // Current signal frequency (for smooth transitions)
        private float _currentSignalFrequency = 0.0f;

        // Stop signal tone
        public void StopSignal()
        {
            _signalPlayer.Stop();
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

        // Set the noise type
        public void SetNoiseType(NoiseType type)
        {
            _currentNoiseType = type;
        }
    }
}

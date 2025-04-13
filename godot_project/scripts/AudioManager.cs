using Godot;
using System;

namespace SignalLost
{
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
            var eq = new AudioEffectEQ();
            eq.SetBandGainDb(0, -5.0f);  // Reduce low frequencies
            eq.SetBandGainDb(1, 2.0f);   // Boost mid-low frequencies
            AudioServer.AddBusEffect(_staticBusIdx, eq);
            
            // Add distortion to static bus
            var distortion = new AudioEffectDistortion();
            distortion.Mode = AudioEffectDistortion.ModeEnum.Bitcrush;
            distortion.Drive = 0.2f;
            AudioServer.AddBusEffect(_staticBusIdx, distortion);
            
            // Add reverb to signal bus
            var reverb = new AudioEffectReverb();
            reverb.Wet = 0.1f;
            reverb.Dry = 0.9f;
            AudioServer.AddBusEffect(_signalBusIdx, reverb);
        }

        // Generate noise based on the current noise type
        private AudioStreamGenerator GenerateNoiseStream(float intensity)
        {
            var noiseGenerator = new AudioStreamGenerator();
            noiseGenerator.MixRate = 44100;
            noiseGenerator.BufferLength = 0.1f;  // 100ms buffer
            
            AudioStreamGeneratorPlayback playback;
            _staticPlayer.Stream = noiseGenerator;
            _staticPlayer.Play();
            playback = (AudioStreamGeneratorPlayback)_staticPlayer.GetStreamPlayback();
            
            // Fill the buffer with noise
            var bufferSize = (int)(noiseGenerator.BufferLength * noiseGenerator.MixRate);
            var random = new Random();
            
            for (int i = 0; i < bufferSize; i++)
            {
                float sample = 0.0f;
                
                switch (_currentNoiseType)
                {
                    case NoiseType.White:
                        sample = (float)random.NextDouble() * 2.0f - 1.0f;
                        break;
                    case NoiseType.Pink:
                        // Simple approximation of pink noise
                        sample = (float)((random.NextDouble() * 2.0 - 1.0) * 0.7 + 
                                        (random.NextDouble() * 2.0 - 1.0) * 0.2 + 
                                        (random.NextDouble() * 2.0 - 1.0) * 0.1);
                        break;
                    case NoiseType.Brown:
                        // Simple approximation of brown noise
                        sample = (float)((random.NextDouble() * 2.0 - 1.0) * 0.5 + 
                                        (random.NextDouble() * 2.0 - 1.0) * 0.3 + 
                                        (random.NextDouble() * 2.0 - 1.0) * 0.2);
                        break;
                    case NoiseType.Digital:
                        // Digital noise (more harsh)
                        sample = (float)(Math.Round(random.NextDouble()) * 2.0 - 1.0);
                        break;
                }
                
                // Apply intensity
                sample *= intensity;
                
                playback.PushFrame(new Vector2(sample, sample));
            }
            
            return noiseGenerator;
        }

        // Play static noise with given intensity
        public void PlayStaticNoise(float intensity)
        {
            if (_staticPlayer.Playing)
            {
                _staticPlayer.Stop();
            }
            
            var noiseStream = GenerateNoiseStream(intensity);
            _staticPlayer.Stream = noiseStream;
            _staticPlayer.Bus = "Static";
            _staticPlayer.VolumeDb = Mathf.LinearToDb(intensity * _volume);
            _staticPlayer.Play();
        }

        // Stop static noise
        public void StopStaticNoise()
        {
            _staticPlayer.Stop();
        }

        // Play signal tone at specified frequency
        public AudioStreamGenerator PlaySignal(float frequency, float volumeScale = 1.0f, string waveform = "sine")
        {
            if (_signalPlayer.Playing)
            {
                _signalPlayer.Stop();
            }
            
            var generator = new AudioStreamGenerator();
            generator.MixRate = 44100;
            generator.BufferLength = 0.1f;  // 100ms buffer
            
            _signalPlayer.Stream = generator;
            _signalPlayer.Bus = "Signal";
            _signalPlayer.VolumeDb = Mathf.LinearToDb(volumeScale * _volume);
            _signalPlayer.Play();
            
            var playback = (AudioStreamGeneratorPlayback)_signalPlayer.GetStreamPlayback();
            
            // Fill the buffer with the waveform
            var bufferSize = (int)(generator.BufferLength * generator.MixRate);
            float phase = 0.0f;
            float increment = frequency / generator.MixRate;
            
            for (int i = 0; i < bufferSize; i++)
            {
                float sample = 0.0f;
                
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
                
                // Apply volume scale
                sample *= volumeScale;
                
                playback.PushFrame(new Vector2(sample, sample));
                phase += increment;
            }
            
            return generator;
        }

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

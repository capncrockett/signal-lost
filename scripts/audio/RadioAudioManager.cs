using Godot;
using System;
using SignalLost;

namespace SignalLost.Audio
{
    /// <summary>
    /// Manages audio effects and playback for the radio interface.
    /// </summary>
    [GlobalClass]
    public partial class RadioAudioManager : Node
    {
        // References to game systems
        private AudioManager _audioManager;
        private GameState _gameState;
        private RadioSystem _radioSystem;
        
        // Audio state
        private float _currentFrequency = 90.0f;
        private float _signalStrength = 0.0f;
        private bool _isRadioOn = false;
        private bool _isTuning = false;
        
        // Audio parameters
        [Export] public float TuningNoiseIntensity { get; set; } = 0.3f;
        [Export] public float StaticBaseIntensity { get; set; } = 0.1f;
        [Export] public float SignalQualityThreshold { get; set; } = 0.7f;
        [Export] public float UIVolume { get; set; } = 0.8f;
        
        // Audio effects
        private AudioStreamPlayer _uiSoundPlayer;
        private AudioStreamPlayer _tuningNoisePlayer;
        private Timer _tuningNoiseTimer;
        
        // Audio resources
        private AudioStream _clickSound;
        private AudioStream _knobSound;
        private AudioStream _sliderSound;
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to game systems
            _audioManager = GetNode<AudioManager>("/root/AudioManager");
            _gameState = GetNode<GameState>("/root/GameState");
            _radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            
            if (_audioManager == null)
            {
                GD.PrintErr("RadioAudioManager: AudioManager not found");
                return;
            }
            
            // Create audio players
            _uiSoundPlayer = new AudioStreamPlayer();
            _uiSoundPlayer.Name = "UISoundPlayer";
            _uiSoundPlayer.VolumeDb = Mathf.LinearToDb(UIVolume);
            AddChild(_uiSoundPlayer);
            
            _tuningNoisePlayer = new AudioStreamPlayer();
            _tuningNoisePlayer.Name = "TuningNoisePlayer";
            _tuningNoisePlayer.VolumeDb = Mathf.LinearToDb(TuningNoiseIntensity);
            AddChild(_tuningNoisePlayer);
            
            // Create tuning noise timer
            _tuningNoiseTimer = new Timer();
            _tuningNoiseTimer.Name = "TuningNoiseTimer";
            _tuningNoiseTimer.WaitTime = 0.1f;
            _tuningNoiseTimer.OneShot = false;
            _tuningNoiseTimer.Timeout += OnTuningNoiseTimerTimeout;
            AddChild(_tuningNoiseTimer);
            
            // Load audio resources
            LoadAudioResources();
            
            // Connect to game state signals if available
            if (_gameState != null)
            {
                _gameState.FrequencyChanged += OnFrequencyChanged;
                _gameState.RadioToggled += OnRadioToggled;
            }
            
            GD.Print("RadioAudioManager initialized");
        }
        
        // Load audio resources
        private void LoadAudioResources()
        {
            try
            {
                _clickSound = GD.Load<AudioStream>("res://assets/audio/click.wav");
                _knobSound = GD.Load<AudioStream>("res://assets/audio/knob.wav");
                _sliderSound = GD.Load<AudioStream>("res://assets/audio/slider.wav");
                
                if (_clickSound == null) GD.PrintErr("RadioAudioManager: Failed to load click.wav");
                if (_knobSound == null) GD.PrintErr("RadioAudioManager: Failed to load knob.wav");
                if (_sliderSound == null) GD.PrintErr("RadioAudioManager: Failed to load slider.wav");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"RadioAudioManager: Error loading audio resources: {ex.Message}");
            }
        }
        
        // Play UI sound effect
        public void PlayUISound(string soundType)
        {
            if (_uiSoundPlayer == null) return;
            
            AudioStream sound = null;
            
            switch (soundType.ToLower())
            {
                case "click":
                    sound = _clickSound;
                    break;
                case "knob":
                    sound = _knobSound;
                    break;
                case "slider":
                    sound = _sliderSound;
                    break;
                default:
                    GD.PrintErr($"RadioAudioManager: Unknown sound type: {soundType}");
                    return;
            }
            
            if (sound != null)
            {
                _uiSoundPlayer.Stream = sound;
                _uiSoundPlayer.Play();
            }
        }
        
        // Start tuning noise
        public void StartTuning()
        {
            if (!_isRadioOn) return;
            
            _isTuning = true;
            
            // Start the tuning noise timer
            _tuningNoiseTimer.Start();
            
            // Play initial tuning noise
            PlayTuningNoise();
        }
        
        // Stop tuning noise
        public void StopTuning()
        {
            _isTuning = false;
            
            // Stop the tuning noise timer
            _tuningNoiseTimer.Stop();
            
            // Update audio based on current state
            UpdateAudio();
        }
        
        // Play tuning noise
        private void PlayTuningNoise()
        {
            if (_audioManager == null) return;
            
            // Increase static noise during tuning
            float staticIntensity = 1.0f - _signalStrength;
            staticIntensity = Mathf.Clamp(staticIntensity + TuningNoiseIntensity, 0.0f, 1.0f);
            
            _audioManager.PlayStaticNoise(staticIntensity);
            
            // Reduce signal volume during tuning
            if (_signalStrength > 0.0f)
            {
                float signalVolume = _signalStrength * (1.0f - TuningNoiseIntensity);
                _audioManager.PlaySignal(_currentFrequency * 10, signalVolume, "sine", _signalStrength > SignalQualityThreshold);
            }
        }
        
        // Update audio based on current state
        private void UpdateAudio()
        {
            if (_audioManager == null) return;
            
            if (_isRadioOn)
            {
                if (_isTuning)
                {
                    // Already handled in PlayTuningNoise
                    PlayTuningNoise();
                }
                else
                {
                    // Normal audio playback
                    if (_signalStrength > 0.1f)
                    {
                        // Play signal with appropriate volume
                        _audioManager.PlaySignal(_currentFrequency * 10, _signalStrength, "sine", _signalStrength > SignalQualityThreshold);
                        _audioManager.PlayStaticNoise(1.0f - _signalStrength);
                    }
                    else
                    {
                        // Just play static
                        _audioManager.StopSignal();
                        _audioManager.PlayStaticNoise(1.0f);
                    }
                }
            }
            else
            {
                // Stop all audio when radio is off
                _audioManager.StopSignal();
                _audioManager.StopStaticNoise();
            }
        }
        
        // Handle frequency changed
        private void OnFrequencyChanged(float frequency)
        {
            _currentFrequency = frequency;
            
            // Update signal strength
            if (_radioSystem != null)
            {
                _signalStrength = _radioSystem.GetSignalStrength();
            }
            else if (_gameState != null)
            {
                var signalData = _gameState.FindSignalAtFrequency(frequency);
                if (signalData != null)
                {
                    _signalStrength = GameState.CalculateSignalStrength(frequency, signalData);
                }
                else
                {
                    _signalStrength = 0.0f;
                }
            }
            
            // Play knob sound for frequency change
            PlayUISound("knob");
            
            // Update audio
            UpdateAudio();
        }
        
        // Handle radio toggled
        private void OnRadioToggled(bool isOn)
        {
            _isRadioOn = isOn;
            
            // Play power toggle sound
            PlayUISound("click");
            
            if (isOn)
            {
                // Play power on squelch
                if (_audioManager != null)
                {
                    _audioManager.PlaySquelchOn();
                }
            }
            else
            {
                // Play power off squelch
                if (_audioManager != null)
                {
                    _audioManager.PlaySquelchOff();
                }
            }
            
            // Update audio
            UpdateAudio();
        }
        
        // Handle tuning noise timer timeout
        private void OnTuningNoiseTimerTimeout()
        {
            if (_isTuning)
            {
                PlayTuningNoise();
            }
        }
        
        // Set the current frequency and signal strength directly
        public void SetFrequencyAndSignal(float frequency, float signalStrength)
        {
            _currentFrequency = frequency;
            _signalStrength = signalStrength;
            
            // Update audio
            UpdateAudio();
        }
        
        // Set radio power state directly
        public void SetRadioPower(bool isOn)
        {
            if (_isRadioOn != isOn)
            {
                _isRadioOn = isOn;
                
                // Update audio
                UpdateAudio();
            }
        }
        
        // Play button click sound
        public void PlayButtonClick()
        {
            PlayUISound("click");
        }
        
        // Play dial turn sound
        public void PlayDialTurn()
        {
            PlayUISound("knob");
        }
        
        // Play slider sound
        public void PlaySliderSound()
        {
            PlayUISound("slider");
        }
    }
}

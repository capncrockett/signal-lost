import { AudioManager } from './AudioManager';

/**
 * SoundscapeManager
 *
 * Manages ambient audio layers using Web Audio API
 * - Static: background noise that increases with distance from signal
 * - Drone: continuous ambient tone that sets the mood
 * - Blip: periodic signal sounds that increase with proximity to signal
 */
export class SoundscapeManager {
  private audioContext: AudioContext | null = null;
  private isInitialized: boolean = false;

  // Audio nodes
  private staticSource: AudioBufferSourceNode | null = null;
  private droneSource: OscillatorNode | null = null;
  private blipSchedulerId: number | null = null;

  // Gain nodes for volume control
  private staticGain: GainNode | null = null;
  private droneGain: GainNode | null = null;
  private blipGain: GainNode | null = null;

  // Master gain for overall volume
  private masterGain: GainNode | null = null;

  // Panning nodes
  private staticPanner: StereoPannerNode | null = null;
  private dronePanner: StereoPannerNode | null = null;
  private blipPanner: StereoPannerNode | null = null;

  // Configuration
  private config = {
    droneFrequency: 55, // Base frequency in Hz
    blipInterval: 2000, // Time between blips in ms
    blipDuration: 200,  // Duration of each blip in ms
  };

  // Reference to the scene
  private scene?: Phaser.Scene;

  // Reference to the audio manager
  private audioManager: AudioManager;

  // Volume change listener
  private volumeChangeListener: ((volume: number) => void) | null = null;

  constructor(scene?: Phaser.Scene) {
    // Audio context will be initialized on first user interaction
    this.scene = scene;

    // Get the audio manager
    this.audioManager = AudioManager.getInstance();
  }

  /**
   * Initialize the audio context and set up audio nodes
   * Must be called after a user interaction due to browser autoplay policies
   */
  public initialize(): boolean {
    if (this.isInitialized) return true;

    try {
      // Create audio context
      this.audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();

      // Create master gain
      this.masterGain = this.audioContext.createGain();
      this.masterGain.gain.value = 0.5; // Overall volume at 50% (reduced from 70%)
      this.masterGain.connect(this.audioContext.destination);

      // Set up volume change listener
      this.volumeChangeListener = (volume: number) => {
        if (this.masterGain) {
          this.masterGain.gain.value = volume * 0.5; // Apply volume scaling
        }
      };

      // Add listener to audio manager
      this.audioManager.addVolumeChangeListener(this.volumeChangeListener);

      // Set up static noise layer
      this.initializeStaticLayer();

      // Set up drone layer
      this.initializeDroneLayer();

      // Set up blip layer
      this.initializeBlipLayer();

      this.isInitialized = true;
      return true;
    } catch (error) {
      console.error('Failed to initialize SoundscapeManager:', error);
      return false;
    }
  }

  /**
   * Initialize the static noise layer
   */
  private initializeStaticLayer(): void {
    if (!this.audioContext) return;

    // Create gain node for static volume
    this.staticGain = this.audioContext.createGain();
    this.staticGain.gain.value = 0.225; // Start at 22.5% volume (reduced by 25%)

    // Create panner for static
    this.staticPanner = this.audioContext.createStereoPanner();
    this.staticPanner.pan.value = 0; // Center panning

    // Connect nodes
    this.staticGain.connect(this.staticPanner);
    this.staticPanner.connect(this.masterGain!);

    // Create white noise
    this.createStaticNoise();
  }

  /**
   * Initialize the drone layer
   */
  private initializeDroneLayer(): void {
    if (!this.audioContext) return;

    // Create oscillator for drone
    this.droneSource = this.audioContext.createOscillator();
    this.droneSource.type = 'sine';
    this.droneSource.frequency.value = this.config.droneFrequency;

    // Create gain node for drone volume
    this.droneGain = this.audioContext.createGain();
    this.droneGain.gain.value = 0.2; // Start at 20% volume

    // Create panner for drone
    this.dronePanner = this.audioContext.createStereoPanner();
    this.dronePanner.pan.value = -0.2; // Slightly left

    // Connect nodes
    this.droneSource.connect(this.droneGain);
    this.droneGain.connect(this.dronePanner);
    this.dronePanner.connect(this.masterGain!);

    // Start oscillator
    this.droneSource.start();
  }

  /**
   * Initialize the blip layer
   */
  private initializeBlipLayer(): void {
    if (!this.audioContext) return;

    // Create gain node for blip volume
    this.blipGain = this.audioContext.createGain();
    this.blipGain.gain.value = 0.0; // Start with no blips

    // Create panner for blip
    this.blipPanner = this.audioContext.createStereoPanner();
    this.blipPanner.pan.value = 0.3; // Slightly right

    // Connect nodes
    this.blipGain.connect(this.blipPanner);
    this.blipPanner.connect(this.masterGain!);

    // Start scheduling blips
    this.scheduleBlips();
  }

  /**
   * Create white noise for the static layer
   */
  private createStaticNoise(): void {
    if (!this.audioContext) return;

    // Create buffer for white noise
    const bufferSize = 2 * this.audioContext.sampleRate;
    const noiseBuffer = this.audioContext.createBuffer(
      1,
      bufferSize,
      this.audioContext.sampleRate
    );

    // Fill buffer with white noise
    const data = noiseBuffer.getChannelData(0);
    for (let i = 0; i < bufferSize; i++) {
      data[i] = Math.random() * 2 - 1;
    }

    // Create source node
    this.staticSource = this.audioContext.createBufferSource();
    this.staticSource.buffer = noiseBuffer;
    this.staticSource.loop = true;

    // Connect to gain node
    this.staticSource.connect(this.staticGain!);

    // Start playback
    this.staticSource.start();
  }

  /**
   * Schedule periodic blips
   */
  private scheduleBlips(): void {
    if (!this.audioContext) return;

    const scheduleNextBlip = () => {
      this.createBlip();
      this.blipSchedulerId = window.setTimeout(
        scheduleNextBlip,
        this.config.blipInterval
      );
    };

    // Start scheduling
    scheduleNextBlip();
  }

  /**
   * Create a single blip sound
   */
  private createBlip(): void {
    if (!this.audioContext || !this.blipGain) return;

    // Create oscillator for blip
    const blipOsc = this.audioContext.createOscillator();
    blipOsc.type = 'sine';
    blipOsc.frequency.value = 880; // A5 note

    // Create gain node for blip envelope
    const blipEnvelope = this.audioContext.createGain();
    blipEnvelope.gain.value = 0;

    // Connect nodes
    blipOsc.connect(blipEnvelope);
    blipEnvelope.connect(this.blipGain);

    // Schedule envelope
    const now = this.audioContext.currentTime;
    blipEnvelope.gain.setValueAtTime(0, now);
    blipEnvelope.gain.linearRampToValueAtTime(1, now + 0.01);
    blipEnvelope.gain.linearRampToValueAtTime(0, now + this.config.blipDuration / 1000);

    // Start and stop oscillator
    blipOsc.start(now);
    blipOsc.stop(now + this.config.blipDuration / 1000 + 0.01);
  }

  /**
   * Play a signal lock sound
   */
  public playSignalSound(): void {
    if (!this.isInitialized) {
      // Try to use Phaser's sound system if available
      if (this.scene && this.scene.sound) {
        try {
          // Play a beep sound using Phaser's sound system
          const beep = this.scene.sound.add('static', { volume: 0.3, loop: false });
          beep.play();
          return;
        } catch (error) {
          console.warn('Failed to play signal sound with Phaser:', error);
        }
      }

      // If we can't use Phaser or Web Audio API, return
      if (!this.audioContext) return;
    }

    try {
      if (!this.audioContext || !this.masterGain) return;

      // Create oscillator for signal sound
      const signalOsc = this.audioContext.createOscillator();
      signalOsc.type = 'sine';
      signalOsc.frequency.value = 1760; // A6 note

      // Create gain node for envelope
      const signalGain = this.audioContext.createGain();
      signalGain.gain.value = 0;

      // Connect to master gain
      signalOsc.connect(signalGain);
      signalGain.connect(this.masterGain);

      // Schedule envelope
      const now = this.audioContext.currentTime;
      signalGain.gain.setValueAtTime(0, now);
      signalGain.gain.linearRampToValueAtTime(0.5, now + 0.01);
      signalGain.gain.linearRampToValueAtTime(0, now + 0.5);

      // Start and stop oscillator
      signalOsc.start(now);
      signalOsc.stop(now + 0.6);
    } catch (error) {
      console.error('Failed to play signal sound:', error);
    }
  }

  /**
   * Update audio layers based on signal strength
   * @param signalStrength Value between 0 (no signal) and 1 (perfect signal)
   */
  public updateLayers(signalStrength: number): void {
    if (!this.isInitialized) return;

    // Clamp signal strength between 0 and 1
    const strength = Math.max(0, Math.min(1, signalStrength));

    // Adjust static volume (inverse relationship with signal strength)
    if (this.staticGain) {
      this.staticGain.gain.value = 0.375 * (1 - strength); // Reduced by 25% from 0.5
    }

    // Adjust drone characteristics
    if (this.droneSource && this.droneGain) {
      // Increase drone volume with signal strength
      this.droneGain.gain.value = 0.1 + (0.3 * strength);

      // Adjust drone frequency based on signal strength
      this.droneSource.frequency.value = this.config.droneFrequency + (strength * 20);
    }

    // Adjust blip volume and rate
    if (this.blipGain) {
      // Blips get louder with stronger signal
      this.blipGain.gain.value = strength * 0.4;

      // Adjust blip interval (faster with stronger signal)
      this.config.blipInterval = 2000 - (strength * 1500);
    }

    // Adjust panning based on signal strength
    // As signal gets stronger, sounds converge to center
    if (this.staticPanner && this.dronePanner && this.blipPanner) {
      this.staticPanner.pan.value = -0.5 * (1 - strength);
      this.dronePanner.pan.value = -0.3 * (1 - strength);
      this.blipPanner.pan.value = 0.4 * (1 - strength);
    }
  }

  /**
   * Adjust panning of all layers based on player position
   * @param position Value between -1 (far left) and 1 (far right)
   */
  public adjustPanning(position: number): void {
    if (!this.isInitialized) return;

    // Clamp position between -1 and 1
    const pos = Math.max(-1, Math.min(1, position));

    // Adjust panning for all layers
    if (this.staticPanner) this.staticPanner.pan.value = pos * -0.3; // Opposite to player
    if (this.dronePanner) this.dronePanner.pan.value = pos * -0.5; // Stronger opposite to player
    if (this.blipPanner) this.blipPanner.pan.value = pos * 0.7; // Same direction as player
  }

  /**
   * Set master volume
   * @param volume Value between 0 (silent) and 1 (full volume)
   */
  public setVolume(volume: number): void {
    if (!this.masterGain) return;

    // Clamp volume between 0 and 1
    const vol = Math.max(0, Math.min(1, volume));

    // Set master gain
    this.masterGain.gain.value = vol;
  }

  /**
   * Clean up resources
   */
  public dispose(): void {
    // Stop all audio sources
    if (this.staticSource) {
      this.staticSource.stop();
      this.staticSource.disconnect();
    }

    if (this.droneSource) {
      this.droneSource.stop();
      this.droneSource.disconnect();
    }

    // Clear blip scheduler
    if (this.blipSchedulerId !== null) {
      window.clearTimeout(this.blipSchedulerId);
      this.blipSchedulerId = null;
    }

    // Disconnect all nodes
    if (this.staticGain) this.staticGain.disconnect();
    if (this.droneGain) this.droneGain.disconnect();
    if (this.blipGain) this.blipGain.disconnect();
    if (this.staticPanner) this.staticPanner.disconnect();
    if (this.dronePanner) this.dronePanner.disconnect();
    if (this.blipPanner) this.blipPanner.disconnect();
    if (this.masterGain) this.masterGain.disconnect();

    // Close audio context
    if (this.audioContext && this.audioContext.state !== 'closed') {
      this.audioContext.close();
    }

    this.isInitialized = false;
  }
}

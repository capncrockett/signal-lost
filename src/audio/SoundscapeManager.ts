import { AudioManager } from './AudioManager';
import { createNoise } from './NoiseGenerator';
import { NoiseType } from './NoiseType';
import * as Tone from 'tone';

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

  // Tone.js nodes
  private noiseGenerator: Tone.Noise | null = null;
  private noiseGain: Tone.Gain<'gain'> | null = null;

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
    blipDuration: 200, // Duration of each blip in ms
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
      // Use a type assertion to handle the webkitAudioContext
      const AudioContextClass =
        window.AudioContext ||
        (
          window as unknown as {
            webkitAudioContext: typeof AudioContext;
          }
        ).webkitAudioContext;
      this.audioContext = new AudioContextClass();

      // Create master gain
      this.masterGain = this.audioContext.createGain();
      this.masterGain.gain.value = 0.5; // Base volume at 50%
      this.masterGain.connect(this.audioContext.destination);

      // Set up volume change listener
      this.volumeChangeListener = (volume: number) => {
        if (this.masterGain) {
          // Apply a normalized volume scale
          // Map the 0-1 range to a more appropriate gain range for Web Audio API
          // Max gain of 0.5 prevents distortion while still allowing good volume range
          const normalizedGain = volume * 0.5;

          // Set the gain value with a small time constant for smooth transitions
          if (this.audioContext) {
            this.masterGain.gain.setTargetAtTime(
              normalizedGain,
              this.audioContext.currentTime,
              0.01
            );
          } else {
            this.masterGain.gain.value = normalizedGain;
          }
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

    try {
      // Try to use Tone.js for better quality noise
      const result = createNoise(NoiseType.Pink, 0.1125); // Half of the original 0.225
      if (result) {
        this.noiseGenerator = result.noise;
        this.noiseGain = result.gain;
        console.log('Static noise initialized with Tone.js (pink noise)');
        return;
      }
    } catch (error) {
      console.error('Failed to initialize Tone.js noise:', error);
    }

    // Fallback to Web Audio API if Tone.js fails
    console.log('Falling back to Web Audio API for noise generation');

    // Create gain node for static volume
    this.staticGain = this.audioContext.createGain();
    this.staticGain.gain.value = 0.1125; // Half of the original 0.225

    // Create panner for static
    this.staticPanner = this.audioContext.createStereoPanner();
    this.staticPanner.pan.value = 0; // Center panning

    // Connect nodes
    this.staticGain.connect(this.staticPanner);
    if (this.masterGain) {
      this.staticPanner.connect(this.masterGain);
    }

    // Create noise using Web Audio API
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
    if (this.masterGain) {
      this.dronePanner.connect(this.masterGain);
    }

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
    if (this.masterGain) {
      this.blipPanner.connect(this.masterGain);
    }

    // Start scheduling blips
    this.scheduleBlips();
  }

  /**
   * Create pink noise for the static layer (fallback method)
   */
  private createStaticNoise(): void {
    if (!this.audioContext) return;

    try {
      // Create buffer for noise
      const bufferSize = 2 * this.audioContext.sampleRate;
      const noiseBuffer = this.audioContext.createBuffer(
        1,
        bufferSize,
        this.audioContext.sampleRate
      );
      const data = noiseBuffer.getChannelData(0);

      // Generate pink noise using the Voss algorithm
      // Pink noise has a frequency spectrum that falls off at 3dB per octave
      // This sounds more natural and less harsh than white noise
      let b0 = 0,
        b1 = 0,
        b2 = 0,
        b3 = 0,
        b4 = 0,
        b5 = 0;
      const b6 = 0;

      for (let i = 0; i < bufferSize; i++) {
        // White noise
        const white = Math.random() * 2 - 1;

        // Pink noise calculation
        b0 = 0.99886 * b0 + white * 0.0555179;
        b1 = 0.99332 * b1 + white * 0.0750759;
        b2 = 0.969 * b2 + white * 0.153852;
        b3 = 0.8665 * b3 + white * 0.3104856;
        b4 = 0.55 * b4 + white * 0.5329522;
        b5 = -0.7616 * b5 - white * 0.016898;

        // Combine components
        data[i] = b0 + b1 + b2 + b3 + b4 + b5 + b6 + white * 0.5362;

        // Normalize to [-1, 1]
        data[i] *= 0.11; // Adjust amplitude

        // Clamp to prevent clipping
        data[i] = Math.max(-1, Math.min(1, data[i]));
      }

      // Create source node
      this.staticSource = this.audioContext.createBufferSource();
      this.staticSource.buffer = noiseBuffer;
      this.staticSource.loop = true;

      // Connect to gain node
      if (this.staticGain) {
        this.staticSource.connect(this.staticGain);
      }

      // Start playback
      this.staticSource.start();

      console.log('Pink noise generated using Web Audio API (fallback)');
    } catch (error) {
      console.error('Failed to create pink noise:', error);
    }
  }

  /**
   * Schedule periodic blips
   */
  private scheduleBlips(): void {
    if (!this.audioContext) return;

    const scheduleNextBlip = () => {
      this.createBlip();
      this.blipSchedulerId = window.setTimeout(scheduleNextBlip, this.config.blipInterval);
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
   * Calculate the static volume based on signal strength
   * @param signalStrength The current signal strength (0-1)
   * @returns The static volume value before master volume scaling
   */
  private getStaticVolume(signalStrength: number): number {
    // Calculate the static volume (inverse of signal strength)
    // 0.1875 = reduced static (no signal), 0.0 = no static (perfect signal)
    // This is half of the original 0.375 value
    return 0.1875 * (1 - signalStrength);
  }

  /**
   * Update audio layers based on signal strength
   * @param signalStrength Value between 0 (no signal) and 1 (perfect signal)
   */
  public updateLayers(signalStrength: number): void {
    if (!this.isInitialized) return;

    // Clamp signal strength between 0 and 1
    const strength = Math.max(0, Math.min(1, signalStrength));

    // Get the base static volume based on signal strength
    const baseStaticVolume = this.getStaticVolume(strength);

    // Apply master volume scaling
    const masterVolume = this.audioManager.getMasterVolume();
    const finalStaticVolume = baseStaticVolume * masterVolume;

    // Use Tone.js noise generator if available
    if (this.noiseGain) {
      this.noiseGain.gain.value = finalStaticVolume;
    }
    // Fallback to Web Audio API
    else if (this.staticGain) {
      this.staticGain.gain.value = finalStaticVolume;
    }

    // Adjust drone characteristics
    if (this.droneSource && this.droneGain) {
      // Increase drone volume with signal strength
      this.droneGain.gain.value = 0.1 + 0.3 * strength;

      // Adjust drone frequency based on signal strength
      this.droneSource.frequency.value = this.config.droneFrequency + strength * 20;
    }

    // Adjust blip volume and rate
    if (this.blipGain) {
      // Blips get louder with stronger signal
      this.blipGain.gain.value = strength * 0.4;

      // Adjust blip interval (faster with stronger signal)
      this.config.blipInterval = 2000 - strength * 1500;
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
    // Stop Tone.js noise generator
    if (this.noiseGenerator) {
      this.noiseGenerator.stop();
      this.noiseGenerator = null;
    }

    // Clean up Tone.js gain node
    if (this.noiseGain) {
      this.noiseGain = null;
    }

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
      void this.audioContext.close(); // Use void operator to explicitly ignore the promise
    }

    this.isInitialized = false;
  }
}

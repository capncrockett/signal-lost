import Phaser from 'phaser';
import { AudioManager } from '../audio/AudioManager';
import * as Tone from 'tone';
import { createNoise } from '../audio/NoiseGenerator';
import { NoiseType } from '../audio/NoiseType';

interface RadioTunerConfig {
  width?: number;
  height?: number;
  minFrequency?: number;
  maxFrequency?: number;
  signalFrequencies?: number[];
  signalTolerance?: number;
  backgroundColor?: number;
  sliderColor?: number;
  knobColor?: number;
  radioImageKey?: string;
  radioImageKeyAlt?: string;
}

/**
 * RadioTuner component for Phaser 3
 * Simulates analog radio tuning with frequency slider and static audio
 */
export class RadioTuner extends Phaser.GameObjects.Container {
  private config: Required<RadioTunerConfig>;
  private background!: Phaser.GameObjects.Graphics;
  private slider!: Phaser.GameObjects.Graphics;
  private knob!: Phaser.GameObjects.Graphics;
  private frequencyText!: Phaser.GameObjects.Text;
  private currentFrequency: number;
  private isDragging: boolean = false;
  private audioContext: AudioContext | null = null;
  private staticGain: GainNode | null = null;
  private masterGain: GainNode | null = null;
  private staticSource: AudioBufferSourceNode | null = null;
  private isAudioInitialized: boolean = false;
  private audioManager: AudioManager;
  private volumeChangeListener: ((volume: number) => void) | null = null;

  // Tone.js specific properties
  private noiseGenerator: Tone.Noise | null = null;
  private noiseGain: Tone.Gain<'gain'> | null = null;

  constructor(scene: Phaser.Scene, x: number, y: number, config: RadioTunerConfig = {}) {
    super(scene, x, y);

    // Default configuration
    this.config = {
      width: config.width || 300,
      height: config.height || 100,
      minFrequency: config.minFrequency || 88.0,
      maxFrequency: config.maxFrequency || 108.0,
      signalFrequencies: config.signalFrequencies || [91.5, 96.3, 103.7],
      signalTolerance: config.signalTolerance || 0.3,
      backgroundColor: config.backgroundColor || 0x333333,
      sliderColor: config.sliderColor || 0x666666,
      knobColor: config.knobColor || 0xcccccc,
      radioImageKey: config.radioImageKey || 'radio',
      radioImageKeyAlt: config.radioImageKeyAlt || 'radio_alt',
    };

    // Get the audio manager
    this.audioManager = AudioManager.getInstance();

    this.currentFrequency = (this.config.minFrequency + this.config.maxFrequency) / 2;

    this.createVisuals();
    this.setupInteraction();
    this.updateDisplay();
  }

  private createVisuals(): void {
    // Create background with border
    this.background = this.scene.add.graphics();

    // Add a border
    this.background.lineStyle(4, 0xffffff, 1); // White border
    this.background.strokeRoundedRect(
      -this.config.width / 2,
      -this.config.height / 2,
      this.config.width,
      this.config.height,
      10
    );

    // Fill background
    this.background.fillStyle(this.config.backgroundColor, 1);
    this.background.fillRoundedRect(
      -this.config.width / 2,
      -this.config.height / 2,
      this.config.width,
      this.config.height,
      10
    );
    this.add(this.background);

    // Create slider track with better visibility
    this.slider = this.scene.add.graphics();

    // Add a border to the slider
    this.slider.lineStyle(2, 0xffffff, 1);
    this.slider.strokeRect(-this.config.width / 2 + 20, -8, this.config.width - 40, 16);

    // Fill slider
    this.slider.fillStyle(this.config.sliderColor, 1);
    this.slider.fillRect(-this.config.width / 2 + 20, -8, this.config.width - 40, 16);
    this.add(this.slider);

    // Create knob with better visibility
    this.knob = this.scene.add.graphics();

    // Add a border to the knob
    this.knob.lineStyle(3, 0xffffff, 1);
    this.knob.strokeCircle(0, 0, 20);

    // Fill knob
    this.knob.fillStyle(this.config.knobColor, 1);
    this.knob.fillCircle(0, 0, 20);
    this.add(this.knob);

    // Add frequency markers
    const markerGraphics = this.scene.add.graphics();
    markerGraphics.fillStyle(0xffffff, 1);

    // Add frequency labels
    for (
      let freq = Math.ceil(this.config.minFrequency);
      freq <= this.config.maxFrequency;
      freq += 2
    ) {
      const t =
        (freq - this.config.minFrequency) / (this.config.maxFrequency - this.config.minFrequency);
      const x = -this.config.width / 2 + 20 + t * (this.config.width - 40);

      // Draw marker
      markerGraphics.fillRect(x - 1, -15, 2, 10);

      // Add label
      const label = this.scene.add.text(x, -30, `${freq}`, {
        fontSize: '12px',
        color: '#ffffff',
      });
      label.setOrigin(0.5, 0.5);
      this.add(label);
    }
    this.add(markerGraphics);

    // Create frequency text with better visibility
    this.frequencyText = this.scene.add.text(
      0,
      this.config.height / 2 - 40,
      `${this.currentFrequency.toFixed(1)} MHz`,
      {
        fontSize: '24px',
        fontStyle: 'bold',
        color: '#ffffff',
        backgroundColor: '#000000',
        padding: { x: 10, y: 5 },
      }
    );
    this.frequencyText.setOrigin(0.5, 0.5);
    this.add(this.frequencyText);
  }

  private setupInteraction(): void {
    // Make knob interactive
    this.knob.setInteractive(new Phaser.Geom.Circle(0, 0, 15), Phaser.Geom.Circle.Contains);

    // Setup drag events
    this.scene.input.setDraggable(this.knob);

    this.scene.input.on(
      'dragstart',
      (_pointer: Phaser.Input.Pointer, gameObject: Phaser.GameObjects.GameObject) => {
        if (gameObject === this.knob) {
          this.isDragging = true;
          this.initializeAudio();
        }
      }
    );

    this.scene.input.on(
      'drag',
      (
        _pointer: Phaser.Input.Pointer,
        gameObject: Phaser.GameObjects.GameObject,
        dragX: number,
        _dragY: number
      ) => {
        if (gameObject === this.knob && this.isDragging) {
          // Constrain to slider bounds
          const minX = -this.config.width / 2 + 20;
          const maxX = this.config.width / 2 - 20;
          const clampedX = Phaser.Math.Clamp(dragX, minX, maxX);

          // Update knob position
          this.knob.x = clampedX;

          // Calculate frequency based on position
          const t = (clampedX - minX) / (maxX - minX);
          this.currentFrequency =
            this.config.minFrequency + t * (this.config.maxFrequency - this.config.minFrequency);

          // Update display
          this.updateDisplay();

          // Update audio
          this.updateAudio();

          // Check for signal lock
          this.checkSignalLock();
        }
      }
    );

    this.scene.input.on(
      'dragend',
      (_pointer: Phaser.Input.Pointer, gameObject: Phaser.GameObjects.GameObject) => {
        if (gameObject === this.knob) {
          this.isDragging = false;
        }
      }
    );

    // Click on slider to jump
    this.slider.setInteractive(
      new Phaser.Geom.Rectangle(-this.config.width / 2 + 20, -5, this.config.width - 40, 10),
      Phaser.Geom.Rectangle.Contains
    );

    this.slider.on('pointerdown', (pointer: Phaser.Input.Pointer) => {
      const localX = pointer.x - this.x - this.scene.cameras.main.scrollX;
      const minX = -this.config.width / 2 + 20;
      const maxX = this.config.width / 2 - 20;
      const clampedX = Phaser.Math.Clamp(localX, minX, maxX);

      // Update knob position
      this.knob.x = clampedX;

      // Calculate frequency
      const t = (clampedX - minX) / (maxX - minX);
      this.currentFrequency =
        this.config.minFrequency + t * (this.config.maxFrequency - this.config.minFrequency);

      // Update display
      this.updateDisplay();

      // Initialize and update audio
      this.initializeAudio();
      this.updateAudio();

      // Check for signal lock
      this.checkSignalLock();
    });
  }

  private updateDisplay(): void {
    // Update frequency text
    this.frequencyText.setText(`${this.currentFrequency.toFixed(1)} MHz`);

    // Update knob position based on frequency
    const t =
      (this.currentFrequency - this.config.minFrequency) /
      (this.config.maxFrequency - this.config.minFrequency);
    const minX = -this.config.width / 2 + 20;
    const maxX = this.config.width / 2 - 20;
    this.knob.x = minX + t * (maxX - minX);
  }

  // Initialize audio system

  private initializeAudio(): void {
    if (this.isAudioInitialized) return;

    try {
      // Create audio context
      // Use a type assertion to handle the webkitAudioContext
      // This is a common pattern for cross-browser compatibility
      const AudioContextClass =
        window.AudioContext ||
        (
          window as unknown as {
            webkitAudioContext: typeof AudioContext;
          }
        ).webkitAudioContext;
      this.audioContext = new AudioContextClass();

      // Create master gain node (reduced to 50% volume)
      this.masterGain = this.audioContext.createGain();
      this.masterGain.gain.value = 0.5; // 50% volume
      this.masterGain.connect(this.audioContext.destination);

      // Create gain node for static volume
      this.staticGain = this.audioContext.createGain();
      this.staticGain.connect(this.masterGain);

      // We'll simulate static with white noise
      // Generate white noise programmatically instead of loading an MP3 file
      this.createStaticNoise();

      // Set up volume change listener
      this.volumeChangeListener = (volume: number) => {
        // Scale volume so that 50% in UI is maximum (0.25 gain)
        // This makes the overall volume much lower
        const scaledVolume = Math.min(0.25, volume * 0.5);

        // Update master gain
        if (this.masterGain) {
          this.masterGain.gain.value = scaledVolume;
        }

        // Update noise gain (for static)
        if (this.noiseGain) {
          // Apply the same volume scaling to the noise
          this.noiseGain.gain.value = this.getStaticVolume(this.getSignalStrength()) * volume;
        }
      };

      // Add listener to audio manager
      this.audioManager.addVolumeChangeListener(this.volumeChangeListener);

      this.isAudioInitialized = true;

      // Emit an event that audio is initialized (for testing)
      this.emit('audioInitialized');
    } catch (error) {
      console.error('Failed to initialize audio:', error);
    }
  }

  private createStaticNoise(): void {
    try {
      // Use our NoiseGenerator utility to create pink noise with reduced volume
      const result = createNoise(NoiseType.Pink, 0.25); // Half of the original 0.5

      if (result) {
        // Store references for later use
        this.noiseGenerator = result.noise;
        this.noiseGain = result.gain;

        console.log('Pink noise generator initialized with Tone.js');
        return;
      }
    } catch (error) {
      console.error('Failed to create pink noise with Tone.js:', error);
    }

    // Fallback to Web Audio API if Tone.js fails
    this.createStaticNoiseFallback();
  }

  private createStaticNoiseFallback(): void {
    if (!this.audioContext) return;

    try {
      // Create buffer for pink noise
      const bufferSize = 2 * this.audioContext.sampleRate;
      const noiseBuffer = this.audioContext.createBuffer(
        1,
        bufferSize,
        this.audioContext.sampleRate
      );
      const data = noiseBuffer.getChannelData(0);

      // Generate pink noise using the Voss algorithm
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

      // Connect to gain node with reduced volume (half of original)
      this.staticGain!.gain.value = 0.25; // Half of the original 0.5
      this.staticSource.connect(this.staticGain!);

      // Start playback
      this.staticSource.start();

      console.log('Pink noise generator initialized with Web Audio API (fallback)');
    } catch (error) {
      console.error('Failed to create pink noise with fallback method:', error);
    }
  }

  private updateAudio(): void {
    // Calculate signal strength based on proximity to valid frequencies
    const signalStrength = this.getSignalStrength();

    // Update static volume based on signal strength
    this.updateStaticVolume(signalStrength);
  }

  /**
   * Calculate the static volume based on signal strength
   * @param signalStrength The current signal strength (0-1)
   * @returns The static volume value before master volume scaling
   */
  private getStaticVolume(signalStrength: number): number {
    // Calculate the static volume (inverse of signal strength)
    // 0.375 = reduced static (no signal), 0.0 = no static (perfect signal)
    // This is half of the original 0.75 value
    return 0.375 * (1.0 - signalStrength);
  }

  /**
   * Update the static volume based on signal strength and master volume
   * @param signalStrength The current signal strength (default: current value)
   */
  private updateStaticVolume(signalStrength: number = this.getSignalStrength()): void {
    // Get the base static volume
    const staticVolume = this.getStaticVolume(signalStrength);

    // Get the master volume for scaling
    const masterVolume = this.audioManager.getMasterVolume();

    // Calculate the final volume with master volume scaling
    const finalVolume = staticVolume * masterVolume;

    // Update Tone.js noise generator if available
    if (this.noiseGain) {
      this.noiseGain.gain.value = finalVolume;
      return;
    }

    // Fallback to Web Audio API
    if (this.staticGain) {
      this.staticGain.gain.value = finalVolume;
    }
  }

  private getSignalStrength(): number {
    // Calculate signal strength based on proximity to valid frequencies
    let closestDistance = Number.MAX_VALUE;

    for (const frequency of this.config.signalFrequencies) {
      const distance = Math.abs(this.currentFrequency - frequency);
      closestDistance = Math.min(closestDistance, distance);
    }

    // Normalize distance to signal strength
    // 1.0 = perfect signal, 0.0 = no signal
    const normalizedStrength = 1.0 - Math.min(closestDistance / this.config.signalTolerance, 1.0);
    return Math.max(0, normalizedStrength);
  }

  private checkSignalLock(): void {
    const signalStrength = this.getSignalStrength();

    // If signal strength is above threshold, emit signal lock event
    if (signalStrength > 0.8) {
      // Find which signal was locked onto
      const lockedSignal = this.findLockedSignal();

      // Emit signal lock event with frequency, signal strength, and signal ID
      this.emit('signalLock', {
        frequency: this.currentFrequency,
        signalStrength: signalStrength,
        signalId: lockedSignal?.id || 'unknown',
        signalType: lockedSignal?.type || 'unknown',
        signalData: lockedSignal?.data || null
      });

      // Log the signal lock for testing purposes
      console.log(
        `Signal locked at frequency: ${this.currentFrequency.toFixed(1)} MHz with strength ${signalStrength.toFixed(2)}, ` +
        `signal ID: ${lockedSignal?.id || 'unknown'}, type: ${lockedSignal?.type || 'unknown'}`
      );
    }
  }

  /**
   * Find which predefined signal was locked onto
   * @returns The signal data or undefined if no match
   */
  private findLockedSignal(): { id: string; frequency: number; type: string; data: any } | undefined {
    // Define signal data with IDs, types, and additional data
    const signals = [
      { id: 'signal1', frequency: 91.5, type: 'location', data: { locationId: 'tower1', coordinates: { x: 10, y: 8 } } },
      { id: 'signal2', frequency: 96.3, type: 'message', data: { message: 'Help us... coordinates... 15, 12...' } },
      { id: 'signal3', frequency: 103.7, type: 'location', data: { locationId: 'ruins1', coordinates: { x: 15, y: 12 } } },
    ];

    // Find the closest signal
    let closestSignal;
    let closestDistance = Number.MAX_VALUE;

    for (const signal of signals) {
      const distance = Math.abs(this.currentFrequency - signal.frequency);
      if (distance < closestDistance && distance < this.config.signalTolerance) {
        closestDistance = distance;
        closestSignal = signal;
      }
    }

    return closestSignal;
  }

  /**
   * Set the current frequency directly
   */
  public setFrequency(frequency: number): void {
    this.currentFrequency = Phaser.Math.Clamp(
      frequency,
      this.config.minFrequency,
      this.config.maxFrequency
    );
    this.updateDisplay();
    this.updateAudio();
    this.checkSignalLock();
  }

  /**
   * Get the current frequency
   */
  public getFrequency(): number {
    return this.currentFrequency;
  }

  /**
   * Get the current signal strength (0.0 to 1.0)
   */
  public getSignalStrengthValue(): number {
    return this.getSignalStrength();
  }

  /**
   * Clean up resources when component is destroyed
   */
  public destroy(fromScene?: boolean): void {
    // Clean up Tone.js resources
    if (this.noiseGenerator) {
      try {
        this.noiseGenerator.stop();
        this.noiseGenerator.dispose();
        this.noiseGenerator = null;
      } catch (error) {
        console.error('Error disposing Tone.js noise generator:', error);
      }
    }

    if (this.noiseGain) {
      try {
        this.noiseGain.dispose();
        this.noiseGain = null;
      } catch (error) {
        console.error('Error disposing Tone.js gain node:', error);
      }
    }

    // Clean up Web Audio API resources (fallback)
    if (this.staticSource) {
      try {
        this.staticSource.stop();
        this.staticSource.disconnect();
        this.staticSource = null;
      } catch (error) {
        console.error('Error cleaning up static source:', error);
      }
    }

    if (this.staticGain) {
      try {
        this.staticGain.disconnect();
        this.staticGain = null;
      } catch (error) {
        console.error('Error cleaning up static gain:', error);
      }
    }

    if (this.masterGain) {
      try {
        this.masterGain.disconnect();
        this.masterGain = null;
      } catch (error) {
        console.error('Error cleaning up master gain:', error);
      }
    }

    // Remove volume change listener
    if (this.volumeChangeListener) {
      this.audioManager.removeVolumeChangeListener(this.volumeChangeListener);
      this.volumeChangeListener = null;
    }

    // Close audio context
    if (this.audioContext && this.audioContext.state !== 'closed') {
      try {
        this.audioContext.close();
        this.audioContext = null;
      } catch (error) {
        console.error('Error closing audio context:', error);
      }
    }

    // Call parent destroy method
    super.destroy(fromScene);
  }
}

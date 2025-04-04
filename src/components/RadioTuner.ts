import Phaser from 'phaser';
import { AudioManager } from '../audio/AudioManager';

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
    };

    // Get the audio manager
    this.audioManager = AudioManager.getInstance();

    this.currentFrequency = (this.config.minFrequency + this.config.maxFrequency) / 2;

    this.createVisuals();
    this.setupInteraction();
    this.updateDisplay();
  }

  private createVisuals(): void {
    // Create background
    this.background = this.scene.add.graphics();
    this.background.fillStyle(this.config.backgroundColor, 1);
    this.background.fillRoundedRect(
      -this.config.width / 2,
      -this.config.height / 2,
      this.config.width,
      this.config.height,
      10
    );
    this.add(this.background);

    // Create slider track
    this.slider = this.scene.add.graphics();
    this.slider.fillStyle(this.config.sliderColor, 1);
    this.slider.fillRect(-this.config.width / 2 + 20, -5, this.config.width - 40, 10);
    this.add(this.slider);

    // Create knob
    this.knob = this.scene.add.graphics();
    this.knob.fillStyle(this.config.knobColor, 1);
    this.knob.fillCircle(0, 0, 15);
    this.add(this.knob);

    // Create frequency text
    this.frequencyText = this.scene.add.text(
      0,
      this.config.height / 2 - 30,
      `${this.currentFrequency.toFixed(1)} MHz`,
      {
        fontSize: '18px',
        color: '#ffffff',
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
      this.audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();

      // Create master gain node (reduced to 50% volume)
      this.masterGain = this.audioContext.createGain();
      this.masterGain.gain.value = 0.5; // 50% volume
      this.masterGain.connect(this.audioContext.destination);

      // Create gain node for static volume
      this.staticGain = this.audioContext.createGain();
      this.staticGain.connect(this.masterGain);

      // We'll simulate static with white noise
      // In a real implementation, you would load an actual static sound
      this.createStaticNoise();

      // Set up volume change listener
      this.volumeChangeListener = (volume: number) => {
        if (this.masterGain) {
          // Scale volume so that 50% in UI is maximum (0.25 gain)
          // This makes the overall volume much lower
          this.masterGain.gain.value = Math.min(0.25, volume * 0.5);
        }
      };

      // Add listener to audio manager
      this.audioManager.addVolumeChangeListener(this.volumeChangeListener);

      this.isAudioInitialized = true;
    } catch (error) {
      console.error('Failed to initialize audio:', error);
    }
  }

  private createStaticNoise(): void {
    if (!this.audioContext) return;

    // Create buffer for white noise
    const bufferSize = 2 * this.audioContext.sampleRate;
    const noiseBuffer = this.audioContext.createBuffer(1, bufferSize, this.audioContext.sampleRate);

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

  private updateAudio(): void {
    if (!this.staticGain) return;

    // Calculate signal strength based on proximity to valid frequencies
    const signalStrength = this.getSignalStrength();

    // Adjust static volume based on signal strength
    // 0.75 = reduced static (no signal), 0.0 = no static (perfect signal)
    // Reduced from 1.0 to 0.75 (25% reduction)
    this.staticGain.gain.value = 0.75 * (1.0 - signalStrength);
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
      this.emit('signalLock', this.currentFrequency);
    }
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
    // Stop and clean up audio
    if (this.staticSource) {
      this.staticSource.stop();
      this.staticSource.disconnect();
    }

    if (this.staticGain) {
      this.staticGain.disconnect();
    }

    if (this.masterGain) {
      this.masterGain.disconnect();
    }

    // Remove volume change listener
    if (this.volumeChangeListener) {
      this.audioManager.removeVolumeChangeListener(this.volumeChangeListener);
      this.volumeChangeListener = null;
    }

    if (this.audioContext && this.audioContext.state !== 'closed') {
      this.audioContext.close();
    }

    // Call parent destroy method
    super.destroy(fromScene);
  }
}

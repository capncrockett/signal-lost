import Phaser from 'phaser';
import { AudioManager } from '../audio/AudioManager';

interface VolumeControlConfig {
  width?: number;
  height?: number;
  backgroundColor?: number;
  sliderColor?: number;
  knobColor?: number;
  initialVolume?: number;
}

/**
 * VolumeControl component for Phaser 3
 * Provides a UI slider to control the game's master volume
 */
export class VolumeControl extends Phaser.GameObjects.Container {
  private config: Required<VolumeControlConfig>;
  private background!: Phaser.GameObjects.Graphics;
  private slider!: Phaser.GameObjects.Graphics;
  private knob!: Phaser.GameObjects.Graphics;
  private volumeText!: Phaser.GameObjects.Text;
  private isDragging: boolean = false;
  private audioManager: AudioManager;

  constructor(scene: Phaser.Scene, x: number, y: number, config: VolumeControlConfig = {}) {
    super(scene, x, y);

    // Default configuration
    this.config = {
      width: config.width || 150,
      height: config.height || 30,
      backgroundColor: config.backgroundColor || 0x333333,
      sliderColor: config.sliderColor || 0x666666,
      knobColor: config.knobColor || 0xcccccc,
      initialVolume: config.initialVolume !== undefined ? config.initialVolume : 0.8, // Default to 80% volume
    };

    // Get the audio manager
    this.audioManager = AudioManager.getInstance();

    // Set initial volume
    this.audioManager.setMasterVolume(this.config.initialVolume);

    this.createVisuals();
    this.setupInteraction();
    this.updateDisplay();
  }

  /**
   * Create the visual elements of the volume control
   */
  private createVisuals(): void {
    // Create background
    this.background = this.scene.add.graphics();
    this.background.fillStyle(this.config.backgroundColor, 1);
    this.background.fillRoundedRect(
      -this.config.width / 2,
      -this.config.height / 2,
      this.config.width,
      this.config.height,
      5
    );
    this.add(this.background);

    // Create slider track
    this.slider = this.scene.add.graphics();
    this.slider.fillStyle(this.config.sliderColor, 1);
    this.slider.fillRoundedRect(
      -this.config.width / 2 + 10,
      -this.config.height / 4,
      this.config.width - 20,
      this.config.height / 2,
      3
    );
    this.add(this.slider);

    // Create knob
    this.knob = this.scene.add.graphics();
    this.knob.fillStyle(this.config.knobColor, 1);
    this.knob.fillCircle(0, 0, this.config.height / 2 - 5);
    this.add(this.knob);

    // Create volume text
    this.volumeText = this.scene.add.text(0, -this.config.height / 2 - 15, 'Volume: 50%', {
      fontSize: '14px',
      color: '#ffffff',
    });
    this.volumeText.setOrigin(0.5, 0.5);
    this.add(this.volumeText);

    // Make the entire control interactive
    this.background.setInteractive(
      new Phaser.Geom.Rectangle(
        -this.config.width / 2,
        -this.config.height / 2,
        this.config.width,
        this.config.height
      ),
      Phaser.Geom.Rectangle.Contains
    );

    // Make the knob interactive and draggable
    this.knob.setInteractive(
      new Phaser.Geom.Circle(0, 0, this.config.height / 2 - 5),
      Phaser.Geom.Circle.Contains
    );
  }

  /**
   * Set up interaction handlers
   */
  private setupInteraction(): void {
    // Setup drag events
    this.scene.input.setDraggable(this.knob);

    this.scene.input.on(
      'dragstart',
      (_pointer: Phaser.Input.Pointer, gameObject: Phaser.GameObjects.GameObject) => {
        if (gameObject === this.knob) {
          this.isDragging = true;
        }
      }
    );

    this.scene.input.on(
      'drag',
      (
        _pointer: Phaser.Input.Pointer,
        gameObject: Phaser.GameObjects.GameObject,
        dragX: number
      ) => {
        if (gameObject === this.knob && this.isDragging) {
          // Constrain to slider bounds
          const minX = -this.config.width / 2 + 10;
          const maxX = this.config.width / 2 - 10;
          const clampedX = Phaser.Math.Clamp(dragX, minX, maxX);

          // Update knob position
          this.knob.x = clampedX;

          // Calculate linear volume (0-1)
          const linearVolume = (clampedX - minX) / (maxX - minX);

          // Apply volume curve for more natural adjustment
          const curvedVolume = this.applyVolumeCurve(linearVolume);

          // Update audio manager
          this.audioManager.setMasterVolume(curvedVolume);

          // Update display
          this.updateDisplay();
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

    // Allow clicking on the slider to set volume directly
    this.background.on('pointerdown', (pointer: Phaser.Input.Pointer) => {
      const localX = pointer.x - this.x - this.scene.cameras.main.scrollX;
      const minX = -this.config.width / 2 + 10;
      const maxX = this.config.width / 2 - 10;
      const clampedX = Phaser.Math.Clamp(localX, minX, maxX);

      // Calculate linear volume (0-1)
      const linearVolume = (clampedX - minX) / (maxX - minX);

      // Apply volume curve for more natural adjustment
      const curvedVolume = this.applyVolumeCurve(linearVolume);

      // Update audio manager
      this.audioManager.setMasterVolume(curvedVolume);

      // Update display - this will also update the knob position
      // based on the actual volume, preventing jumps
      this.updateDisplay();

      // Start dragging the knob from this position
      this.isDragging = true;
      this.scene.input.setDraggable(this.knob, true);
    });
  }

  /**
   * Update the visual display based on current volume
   */
  private updateDisplay(): void {
    // Get current volume
    const volume = this.audioManager.getMasterVolume();

    // Calculate effective volume (50% UI = 100% actual max volume)
    // const effectiveVolume = Math.min(1, volume * 2);
    // Not currently used, but kept for future reference

    // Update volume text
    this.volumeText.setText(`Volume: ${Math.round(volume * 100)}%`);

    // Update knob position
    const minX = -this.config.width / 2 + 10;
    const maxX = this.config.width / 2 - 10;
    this.knob.x = minX + volume * (maxX - minX);
  }

  /**
   * Apply a volume curve to make volume adjustment feel more natural
   * Uses a cubic curve that gives better control at lower volumes
   * @param linearVolume Linear volume value between 0 and 1
   * @returns Curved volume value between 0 and 1
   */
  private applyVolumeCurve(linearVolume: number): number {
    // Use a cubic curve (x^3) for more natural volume control
    // This gives finer control at lower volumes where human hearing is more sensitive
    return Phaser.Math.Easing.Cubic.Out(linearVolume);
  }

  /**
   * Handle scene resize events to maintain proper positioning
   * @param width New width of the scene
   * @param height New height of the scene
   */
  public onResize(width: number, _height: number): void {
    // Adjust position based on new dimensions
    // Keep the volume control in the top-right corner
    this.x = width - this.config.width / 2 - 20;
    this.y = 50; // Fixed distance from top

    // Update display
    this.updateDisplay();
  }

  /**
   * Set the volume directly
   * @param volume Value between 0 (silent) and 1 (full volume)
   */
  public setVolume(volume: number): void {
    // Apply volume curve for more natural adjustment
    const curvedVolume = this.applyVolumeCurve(volume);

    // Update audio manager
    this.audioManager.setMasterVolume(curvedVolume);

    // Update display
    this.updateDisplay();
  }

  /**
   * Get the current volume
   * @returns Current volume (0-1)
   */
  public getVolume(): number {
    return this.audioManager.getMasterVolume();
  }
}

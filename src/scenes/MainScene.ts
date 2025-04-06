import Phaser from 'phaser';
import { RadioTuner } from '../components/RadioTuner';
import { SoundscapeManager } from '../audio/SoundscapeManager';
import { VolumeControl } from '../components/VolumeControl';
import { TestOverlay } from '../utils/TestOverlay';

export class MainScene extends Phaser.Scene {
  private radioTuner!: RadioTuner;
  private soundscapeManager!: SoundscapeManager;
  private volumeControl!: VolumeControl;

  constructor() {
    super({ key: 'MainScene' });
  }

  preload() {
    // Load assets
    this.load.image('radio', 'assets/images/radio.png');
    this.load.image('signalDetector', 'assets/images/signalDetector.png');
    this.load.image('menuBackground', 'assets/images/menuBackground.png');
    // Static noise is generated programmatically, no need to load an audio file
  }

  create() {
    // Add background
    const bg = this.add.image(400, 300, 'menuBackground');
    bg.setDisplaySize(800, 600);
    bg.setName('menuBackground'); // Add name for resize handler

    // Set up resize handler
    this.scale.on('resize', this.handleResize, this);

    // Initial resize to set correct positions
    this.handleResize(this.scale.width, this.scale.height);

    // Initialize soundscape manager
    this.soundscapeManager = new SoundscapeManager(this);

    // Create the radio tuner component with larger size
    this.radioTuner = new RadioTuner(this, 400, 300, {
      width: 500, // Increase width
      height: 200, // Increase height
      backgroundColor: 0x444444, // Darker background for better visibility
      knobColor: 0xffff00, // Yellow knob for better visibility
      sliderColor: 0x888888, // Lighter slider for better visibility
    });
    this.add.existing(this.radioTuner);

    // Add test overlay for the radio tuner
    TestOverlay.createOverlay(this, this.radioTuner, 'radio-tuner');

    // Add a text label above the radio
    const radioLabel = this.add.text(400, 150, 'RADIO TUNER', {
      fontSize: '32px',
      fontStyle: 'bold',
      color: '#ffffff',
      backgroundColor: '#000000',
      padding: { x: 10, y: 5 },
    });
    radioLabel.setOrigin(0.5, 0.5);

    // Define the type for signal lock data
    interface SignalLockData {
      frequency: number;
      signalStrength: number;
    }

    // Listen for signal lock events
    this.radioTuner.on('signalLock', (data: SignalLockData) => {
      console.log(`Signal locked at frequency: ${data.frequency}`);
      // Play a signal sound when locked
      this.soundscapeManager.playSignalSound();
    });

    // Initialize audio on first interaction
    this.input.once('pointerdown', () => {
      this.soundscapeManager.initialize();
      console.log('Audio initialized');
    });

    // Add volume control
    this.volumeControl = new VolumeControl(this, 700, 50, {
      width: 150,
      height: 30,
      initialVolume: 0.1, // Start at 10% volume
    });
    this.add.existing(this.volumeControl);

    // Add test overlay for the volume control
    TestOverlay.createOverlay(this, this.volumeControl, 'volume-control');

    // Add button to navigate to FieldScene
    const fieldButton = this.add.text(400, 500, 'Go to Field', {
      fontSize: '32px',
      fontStyle: 'bold',
      color: '#ffffff',
      backgroundColor: '#333333',
      padding: { x: 20, y: 10 },
      shadow: { offsetX: 2, offsetY: 2, color: '#000000', blur: 2, stroke: true, fill: true },
    });
    fieldButton.setOrigin(0.5, 0.5);
    fieldButton.setInteractive({ useHandCursor: true });
    fieldButton.setName('fieldButton'); // Add name for resize handler

    // Add a glow effect to make it more visible
    fieldButton.preFX?.addGlow(0x00ffff, 0, 0, false, 0.1, 16);

    // Make the button pulse to draw attention
    this.tweens.add({
      targets: fieldButton,
      scaleX: 1.1,
      scaleY: 1.1,
      duration: 1000,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut',
    });

    // Add click handler
    fieldButton.on('pointerdown', () => {
      console.log('Go to Field button clicked');
      this.scene.start('FieldScene');
    });

    // Add test overlay for E2E testing
    TestOverlay.createOverlay(this, fieldButton, 'go-to-field-button', () => {
      console.log('Go to Field button clicked via test overlay');
      this.scene.start('FieldScene');
    });

    // Note: DOM button removed to avoid duplication with the Phaser Text button

    // Add instructions
    const instructions = this.add.text(400, 400, 'Click and drag the radio tuner to find signals', {
      fontSize: '18px',
      color: '#ffffff',
      backgroundColor: '#000000',
      padding: { x: 10, y: 5 },
    });
    instructions.setOrigin(0.5, 0.5);
    instructions.setName('instructions'); // Add name for resize handler

    // Add volume instructions
    const volumeInstructions = this.add.text(700, 80, 'Adjust volume', {
      fontSize: '14px',
      color: '#ffffff',
      backgroundColor: '#000000',
      padding: { x: 5, y: 3 },
    });
    volumeInstructions.setOrigin(0.5, 0.5);
    volumeInstructions.setName('volumeInstructions'); // Add name for resize handler
  }

  update(_time: number, _delta: number) {
    // Update game objects

    // Update soundscape based on radio tuner signal strength
    if (this.radioTuner && this.soundscapeManager) {
      const signalStrength = this.radioTuner.getSignalStrengthValue();
      this.soundscapeManager.updateLayers(signalStrength);
    }
  }

  /**
   * Handle resize events
   * @param width New width of the scene
   * @param height New height of the scene
   */
  private handleResize(width: number, height: number): void {
    // Resize and reposition background
    const bg = this.children.getByName('menuBackground') as Phaser.GameObjects.Image;
    if (bg) {
      bg.setPosition(width / 2, height / 2);
      // Use setScale instead of setDisplaySize to maintain aspect ratio
      const scaleX = width / 800;
      const scaleY = height / 600;
      const scale = Math.max(scaleX, scaleY);
      bg.setScale(scale);
    }

    // Reposition volume control
    if (this.volumeControl) {
      this.volumeControl.onResize(width, height);
    }

    // Reposition radio tuner
    if (this.radioTuner) {
      this.radioTuner.x = width / 2;
      this.radioTuner.y = height / 2;
    }

    // Reposition field button
    const fieldButton = this.children.getByName('fieldButton') as Phaser.GameObjects.Text;
    if (fieldButton) {
      fieldButton.x = width / 2;
      fieldButton.y = height - 100;
    }

    // Reposition instructions
    const instructions = this.children.getByName('instructions') as Phaser.GameObjects.Text;
    if (instructions) {
      instructions.x = width / 2;
      instructions.y = height / 2 + 100;
    }

    // Reposition volume instructions
    const volumeInstructions = this.children.getByName(
      'volumeInstructions'
    ) as Phaser.GameObjects.Text;
    if (volumeInstructions) {
      volumeInstructions.x = width / 2;
      volumeInstructions.y = height / 2 + 130;
    }
  }
}

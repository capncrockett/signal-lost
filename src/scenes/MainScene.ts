import Phaser from 'phaser';
import { RadioTuner } from '../components/RadioTuner';
import { SoundscapeManager } from '../audio/SoundscapeManager';
import { VolumeControl } from '../components/VolumeControl';

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
    this.load.audio('static', 'assets/audio/static.mp3');
  }

  create() {
    // Add background
    const bg = this.add.image(400, 300, 'menuBackground');
    bg.setDisplaySize(800, 600);

    // Initialize soundscape manager
    this.soundscapeManager = new SoundscapeManager(this);

    // Create the radio tuner component
    this.radioTuner = new RadioTuner(this, 400, 300);
    this.add.existing(this.radioTuner);

    // Listen for signal lock events
    this.radioTuner.on('signalLock', (data: any) => {
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

    // Add button to navigate to FieldScene
    const fieldButton = this.add.text(400, 500, 'Go to Field', {
      fontSize: '24px',
      color: '#ffffff',
      backgroundColor: '#333333',
      padding: { x: 10, y: 5 },
    });
    fieldButton.setOrigin(0.5, 0.5);
    fieldButton.setInteractive({ useHandCursor: true });
    fieldButton.on('pointerdown', () => {
      this.scene.start('FieldScene');
    });

    // Add instructions
    const instructions = this.add.text(400, 400, 'Click and drag the radio tuner to find signals', {
      fontSize: '18px',
      color: '#ffffff',
      backgroundColor: '#000000',
      padding: { x: 10, y: 5 },
    });
    instructions.setOrigin(0.5, 0.5);

    // Add volume instructions
    const volumeInstructions = this.add.text(700, 80, 'Adjust volume', {
      fontSize: '14px',
      color: '#ffffff',
      backgroundColor: '#000000',
      padding: { x: 5, y: 3 },
    });
    volumeInstructions.setOrigin(0.5, 0.5);
  }

  update(_time: number, _delta: number) {
    // Update game objects

    // Update soundscape based on radio tuner signal strength
    if (this.radioTuner && this.soundscapeManager) {
      const signalStrength = this.radioTuner.getSignalStrengthValue();
      this.soundscapeManager.updateLayers(signalStrength);
    }
  }
}

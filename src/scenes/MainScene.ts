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
    // Static noise is generated programmatically, no need to load an audio file
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

    // Add a DOM button as a fallback for E2E tests
    const domButton = document.createElement('button');
    domButton.innerText = 'Go to Field';
    domButton.style.position = 'absolute';
    domButton.style.bottom = '100px';
    domButton.style.left = '50%';
    domButton.style.transform = 'translateX(-50%)';
    domButton.style.padding = '10px 20px';
    domButton.style.fontSize = '24px';
    domButton.style.backgroundColor = '#333';
    domButton.style.color = '#fff';
    domButton.style.border = '2px solid #fff';
    domButton.style.borderRadius = '5px';
    domButton.style.cursor = 'pointer';
    domButton.style.zIndex = '1000';
    domButton.onclick = () => {
      console.log('DOM Go to Field button clicked');
      this.scene.start('FieldScene');
    };
    document.getElementById('game')?.appendChild(domButton);

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

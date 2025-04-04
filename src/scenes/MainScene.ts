import Phaser from 'phaser';
import { RadioTuner } from '../components/RadioTuner';
import { SoundscapeManager } from '../audio/SoundscapeManager';

export class MainScene extends Phaser.Scene {
  private radioTuner!: RadioTuner;
  private soundscapeManager!: SoundscapeManager;

  constructor() {
    super({ key: 'MainScene' });
  }

  preload() {
    // Load assets here
    // Will add asset loading later
  }

  create() {
    // Initialize soundscape manager
    this.soundscapeManager = new SoundscapeManager();

    // Create the radio tuner component
    this.radioTuner = new RadioTuner(this, 400, 300);
    this.add.existing(this.radioTuner);

    // Listen for signal lock events
    this.radioTuner.on('signalLock', (frequency: number) => {
      console.log(`Signal locked at frequency: ${frequency}`);
      // Handle signal lock event
    });

    // Initialize audio on first interaction
    this.input.once('pointerdown', () => {
      this.soundscapeManager.initialize();
      console.log('Audio initialized');
    });
  }

  update(time: number, delta: number) {
    // Update game objects

    // Update soundscape based on radio tuner signal strength
    if (this.radioTuner && this.soundscapeManager) {
      const signalStrength = this.radioTuner.getSignalStrengthValue();
      this.soundscapeManager.adjustLayers(signalStrength);
    }
  }
}

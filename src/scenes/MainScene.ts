import Phaser from 'phaser';
import { RadioTuner } from '../components/RadioTuner';

export class MainScene extends Phaser.Scene {
  private radioTuner!: RadioTuner;

  constructor() {
    super({ key: 'MainScene' });
  }

  preload() {
    // Load assets here
    // Will add asset loading later
  }

  create() {
    // Create the radio tuner component
    this.radioTuner = new RadioTuner(this, 400, 300);
    this.add.existing(this.radioTuner);

    // Listen for signal lock events
    this.radioTuner.on('signalLock', (frequency: number) => {
      console.log(`Signal locked at frequency: ${frequency}`);
      // Handle signal lock event
    });
  }

  update(time: number, delta: number) {
    // Update game objects
  }
}

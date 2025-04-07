import Phaser from 'phaser';

// Simple test scene
class TestScene extends Phaser.Scene {
  constructor() {
    super({ key: 'TestScene' });
  }

  preload() {
    // Load assets
    this.load.image('radio', 'assets/images/radio.png');
    this.load.image('background', 'assets/images/menuBackground.png');
  }

  create() {
    console.log('TestScene created');

    // Add background
    const bg = this.add.image(400, 300, 'background');
    bg.setDisplaySize(800, 600);

    // Add radio
    const radio = this.add.image(400, 300, 'radio');
    radio.setScale(0.5);

    // Add text
    this.add
      .text(400, 100, 'Signal Lost', {
        fontFamily: 'Arial',
        fontSize: '32px',
        color: '#ffffff',
      })
      .setOrigin(0.5);
  }
}

// Game configuration
const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: 800,
  height: 600,
  backgroundColor: '#000000',
  parent: 'game',
  scene: [TestScene],
};

// Add global error handler
window.addEventListener('error', (event) => {
  console.error('Global error:', event.message, event.filename, event.lineno, event.error);
});

// Initialize the game
window.addEventListener('load', () => {
  try {
    console.log('Window loaded, initializing Phaser game...');
    console.log('Game container exists:', !!document.getElementById('game'));

    // Create the game instance
    new Phaser.Game(config);
    console.log('Phaser game initialized');
  } catch (error) {
    console.error('Error initializing game:', error);
  }
});

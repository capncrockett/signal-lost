import Phaser from 'phaser';
import { MainScene } from './scenes/MainScene';
import { FieldScene } from './scenes/field/FieldScene';

// Debug scene to test asset loading
class LoadingScene extends Phaser.Scene {
  constructor() {
    super({ key: 'LoadingScene' });
  }

  preload() {
    // Set up loading bar
    const progressBar = this.add.graphics();
    const progressBox = this.add.graphics();
    progressBox.fillStyle(0x222222, 0.8);
    progressBox.fillRect(240, 270, 320, 50);

    // Loading text
    const width = this.cameras.main.width;
    const height = this.cameras.main.height;
    const loadingText = this.add.text(width / 2, height / 2 - 50, 'Loading...', {
      fontFamily: 'Arial',
      fontSize: '20px',
      color: '#ffffff',
    });
    loadingText.setOrigin(0.5, 0.5);

    // Asset text
    const assetText = this.add.text(width / 2, height / 2 + 50, '', {
      fontFamily: 'Arial',
      fontSize: '18px',
      color: '#ffffff',
    });
    assetText.setOrigin(0.5, 0.5);

    // Log asset loading
    this.load.on('progress', (value: number) => {
      progressBar.clear();
      progressBar.fillStyle(0xffffff, 1);
      progressBar.fillRect(250, 280, 300 * value, 30);
    });

    this.load.on('fileprogress', (file: Phaser.Loader.File) => {
      assetText.setText(`Loading asset: ${file.key}`);
    });

    this.load.on('filecomplete', (key: string, type: string) => {
      console.log(`Asset loaded: ${key} (${type})`);
    });

    this.load.on('loaderror', (file: Phaser.Loader.File) => {
      // Ensure url is a string
      const url = typeof file.url === 'string' ? file.url : String(file.url);
      console.error(`Error loading asset: ${file.key} from ${url}`);
      // Try alternative path
      if (url.startsWith('/')) {
        const newUrl = url.substring(1); // Remove leading slash
        console.log(`Retrying with alternative path: ${newUrl}`);
        this.load.image(`${file.key}_alt`, newUrl);
      }
    });

    this.load.on('complete', () => {
      progressBar.destroy();
      progressBox.destroy();
      loadingText.destroy();
      assetText.destroy();
      this.scene.start('MainScene');
    });

    // Load essential assets
    // Try multiple path formats to handle different environments
    this.load.image('radio', '/assets/images/radio.png');
    this.load.image('radio_alt', 'assets/images/radio.png'); // Alternative path

    this.load.image('background', '/assets/images/menuBackground.png');
    this.load.image('background_alt', 'assets/images/menuBackground.png'); // Alternative path
  }

  create() {
    console.log('LoadingScene completed');
  }
}

// Game configuration
const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: 800,
  height: 600,
  backgroundColor: '#000000',
  parent: 'game',
  scene: [LoadingScene, MainScene, FieldScene],
  // Add debug flags
  banner: true,
  fps: {
    min: 10,
    target: 60,
    forceSetTimeOut: true,
    deltaHistory: 10,
  },
  render: {
    pixelArt: true,
    antialias: false,
    antialiasGL: false,
  },
  // Add physics configuration
  physics: {
    default: 'arcade',
    arcade: {
      gravity: { y: 0 },
      debug: false,
    },
  },
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

    // Create a fallback canvas for testing
    const gameContainer = document.getElementById('game');
    if (gameContainer) {
      const fallbackCanvas = document.createElement('canvas');
      fallbackCanvas.id = 'fallback-canvas';
      fallbackCanvas.width = 800;
      fallbackCanvas.height = 600;
      fallbackCanvas.style.display = 'none'; // Hide it by default
      gameContainer.appendChild(fallbackCanvas);
    }

    // Create the game instance
    const game = new Phaser.Game(config);
    console.log('Phaser game initialized');

    // Define interface for window with game property
    interface WindowWithGame {
      game: Phaser.Game;
    }

    // Add the game instance to window for debugging
    (window as unknown as WindowWithGame).game = game;
  } catch (error) {
    console.error('Error initializing game:', error);
  }
});

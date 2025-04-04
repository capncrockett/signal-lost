import Phaser from 'phaser';
import { MainScene } from './scenes/MainScene';

// Game configuration
const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: 800,
  height: 600,
  backgroundColor: '#000000',
  scene: [MainScene],
  physics: {
    default: 'arcade',
    arcade: {
      gravity: { y: 0 },
      debug: false
    }
  },
  pixelArt: true
};

// Initialize the game
window.addEventListener('load', () => {
  new Phaser.Game(config);
});

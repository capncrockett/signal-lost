import Phaser from 'phaser';
import { MainScene } from './scenes/MainScene';
import { FieldScene } from './scenes/field/FieldScene';

// Game configuration
const config: Phaser.Types.Core.GameConfig = {
  type: Phaser.AUTO,
  width: 800,
  height: 600,
  backgroundColor: '#000000',
  parent: 'game', // Specify the parent container
  scene: [MainScene, FieldScene],
  physics: {
    default: 'arcade',
    arcade: {
      gravity: { y: 0 },
      debug: false
    }
  },
  pixelArt: true,
  canvasStyle: 'display: block; margin: 0 auto;' // Ensure canvas is visible
};

// Initialize the game
window.addEventListener('load', () => {
  console.log('Window loaded, initializing Phaser game...');
  const game = new Phaser.Game(config);
  console.log('Phaser game initialized');

  // Log when the canvas is created
  const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      if (mutation.addedNodes.length) {
        mutation.addedNodes.forEach((node) => {
          if (node.nodeName === 'CANVAS') {
            console.log('Canvas element created');
          }
        });
      }
    });
  });

  // Start observing the document body for changes
  observer.observe(document.body, { childList: true, subtree: true });
});

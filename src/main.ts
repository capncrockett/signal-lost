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
      gravity: { x: 0, y: 0 },
      debug: false,
    },
  },
  pixelArt: true,
  scale: {
    mode: Phaser.Scale.NONE, // Use NONE and handle scaling manually for better control
    parent: 'game',
    width: 800,
    height: 600,
  },
  dom: {
    createContainer: true,
  },
  canvasStyle: 'display: block;',
  autoRound: true, // Round pixel positions to avoid blurring
};

// Add global error handler
window.addEventListener('error', (event) => {
  console.error('Global error:', event.message, event.filename, event.lineno, event.error);
});

// Add unhandled promise rejection handler
window.addEventListener('unhandledrejection', (event) => {
  console.error('Unhandled promise rejection:', event.reason);
});

// Log DOM ready state
document.addEventListener('DOMContentLoaded', () => {
  console.log('DOM content loaded');
  console.log('Game container exists:', !!document.getElementById('game'));
  console.log('Fallback canvas exists:', !!document.getElementById('fallback-canvas'));
});

// Initialize the game
window.addEventListener('load', () => {
  try {
    console.log('Window loaded, initializing Phaser game...');
    console.log('Window dimensions:', window.innerWidth, 'x', window.innerHeight);
    console.log('Game container exists:', !!document.getElementById('game'));
    console.log(
      'Game container dimensions:',
      document.getElementById('game')?.getBoundingClientRect()
    );
    console.log('Fallback canvas exists:', !!document.getElementById('fallback-canvas'));
    console.log(
      'Fallback canvas dimensions:',
      document.getElementById('fallback-canvas')?.getBoundingClientRect()
    );
    console.log('Phaser config:', JSON.stringify(config));

    // Create the game instance
    // Game instance is created but not stored in a variable
    // as it's managed internally by Phaser
    new Phaser.Game(config);
    console.log('Phaser game initialized');

    // Log when the canvas is created
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.addedNodes.length) {
          mutation.addedNodes.forEach((node) => {
            if (node.nodeName === 'CANVAS') {
              console.log('Canvas element created:', node);
              console.log('Canvas dimensions:', (node as HTMLElement).getBoundingClientRect());
            }
          });
        }
      });
    });

    // Start observing the document body for changes
    observer.observe(document.body, { childList: true, subtree: true });

    // Also observe the game container
    const gameContainer = document.getElementById('game');
    if (gameContainer) {
      observer.observe(gameContainer, { childList: true, subtree: true });
    }

    // Log all canvas elements after a delay
    setTimeout(() => {
      const canvasElements = document.querySelectorAll('canvas');
      console.log('Canvas elements after delay:', canvasElements.length);
      canvasElements.forEach((canvas, index) => {
        console.log(`Canvas ${index}:`, canvas);
        console.log(`Canvas ${index} dimensions:`, canvas.getBoundingClientRect());
        console.log(`Canvas ${index} visibility:`, window.getComputedStyle(canvas).visibility);
        console.log(`Canvas ${index} display:`, window.getComputedStyle(canvas).display);
      });
    }, 2000);
  } catch (error) {
    console.error('Error initializing game:', error);
  }
});

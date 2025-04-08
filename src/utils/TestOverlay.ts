import * as Phaser from 'phaser';

// Define types for game objects to avoid 'any' warnings
type PhaserGameObject = Phaser.GameObjects.GameObject;
type PhaserEmitter = { emit: (event: string, ...args: unknown[]) => void };

/**
 * TestOverlay utility for adding DOM elements with test IDs that overlay Phaser game objects
 * This makes E2E testing more reliable by providing stable selectors
 */
export class TestOverlay {
  /**
   * Create a transparent clickable overlay for a Phaser game object
   * @param scene The Phaser scene
   * @param gameObject The Phaser game object to overlay
   * @param testId The test ID to assign to the overlay element
   * @param clickHandler Optional click handler function
   * @returns The created DOM element
   */
  static createOverlay(
    scene: Phaser.Scene,
    gameObject: PhaserGameObject | null,
    testId: string,
    clickHandler?: () => void
  ): HTMLElement {
    // Get the game canvas
    const canvas = scene.sys.game.canvas;
    if (!canvas || !canvas.parentElement) {
      console.error('Canvas or parent element not found');
      return document.createElement('div');
    }

    // Handle null gameObject
    if (!gameObject) {
      console.warn(`Creating test overlay for null gameObject with testId: ${testId}`);
      // Create a fallback element in the center of the screen
      const fallbackObject = scene.add.rectangle(400, 300, 100, 50, 0x000000, 0);
      fallbackObject.setName(`fallback-${testId}`);
      gameObject = fallbackObject;
    }

    // Create overlay element
    const overlay = document.createElement('div');
    overlay.setAttribute('data-testid', testId);
    overlay.style.position = 'absolute';
    overlay.style.backgroundColor = 'rgba(0, 0, 0, 0.01)'; // Very slight background to help with stability
    overlay.style.cursor = 'pointer';
    overlay.style.zIndex = '100';
    overlay.style.pointerEvents = 'all';
    overlay.style.userSelect = 'none';
    overlay.style.touchAction = 'none';
    // Using unknown as an intermediate step to avoid any
    const extendedStyle = overlay.style as unknown as { webkitTapHighlightColor: string };
    extendedStyle.webkitTapHighlightColor = 'transparent';

    // Add a small indicator in development mode to show the test overlay
    if (process.env.NODE_ENV !== 'production') {
      overlay.innerHTML = `<div style="font-size: 10px; color: rgba(255,255,255,0.3); text-align: center;">${testId}</div>`;
    }

    // Add to the game container
    canvas.parentElement.appendChild(overlay);

    // Position the overlay
    this.updateOverlayPosition(scene, gameObject, overlay);

    // Add click handler with multiple event types for better reliability
    const handleClick = (e: Event): void => {
      e.preventDefault();
      e.stopPropagation();

      if (clickHandler) {
        clickHandler();
      } else {
        // Emit a pointer down event on the game object if it has listeners
        const emitter = gameObject as unknown as PhaserEmitter;
        if (emitter.emit) {
          emitter.emit('pointerdown');
        }
      }
    };

    // Add multiple event listeners for better test reliability
    overlay.addEventListener('click', handleClick);
    overlay.addEventListener('mousedown', handleClick);
    overlay.addEventListener('touchstart', handleClick);

    // Set up an update listener to reposition the overlay when the camera moves
    scene.events.on('postupdate', () => {
      this.updateOverlayPosition(scene, gameObject, overlay);
    });

    // Clean up when the scene is shut down
    scene.events.once('shutdown', () => {
      if (overlay.parentElement) {
        overlay.parentElement.removeChild(overlay);
      }
    });

    return overlay;
  }

  /**
   * Update the position and size of an overlay element to match a game object
   * @param scene The Phaser scene
   * @param gameObject The Phaser game object
   * @param overlay The overlay DOM element
   */
  private static updateOverlayPosition(
    scene: Phaser.Scene,
    gameObject: PhaserGameObject | null,
    overlay: HTMLElement
  ): void {
    // Get the game canvas
    const canvas = scene.sys.game.canvas;
    if (!canvas || !gameObject) return;

    // Get the bounds of the game object
    let bounds: Phaser.Geom.Rectangle;

    // Define interfaces for different types of game objects
    interface WithBounds {
      getBounds: () => Phaser.Geom.Rectangle;
    }
    interface WithDimensions {
      x: number;
      y: number;
      width: number;
      height: number;
      originX?: number;
      originY?: number;
    }
    interface WithPosition {
      x: number;
      y: number;
    }

    // Check if the object has getBounds method
    if ('getBounds' in gameObject) {
      // Use getBounds if available
      bounds = (gameObject as WithBounds).getBounds();
    } else if ('width' in gameObject && 'height' in gameObject) {
      // For objects with width and height properties
      const obj = gameObject as unknown as WithDimensions;
      const x = obj.x - obj.width * (obj.originX || 0);
      const y = obj.y - obj.height * (obj.originY || 0);
      bounds = new Phaser.Geom.Rectangle(x, y, obj.width, obj.height);
    } else {
      // Fallback for other objects - create a small clickable area
      const obj = gameObject as unknown as WithPosition;
      const x = obj.x || 0;
      const y = obj.y || 0;
      bounds = new Phaser.Geom.Rectangle(x - 25, y - 25, 50, 50);
    }

    // Get the canvas bounds
    const canvasBounds = canvas.getBoundingClientRect();

    // Use fixed dimensions for our game
    const gameWidth = 800;
    const gameHeight = 600;

    // Calculate the scale factor between the game world and the canvas
    const scaleX = canvasBounds.width / gameWidth;
    const scaleY = canvasBounds.height / gameHeight;

    // Apply camera scroll offset
    const cameraX = scene.cameras.main.scrollX;
    const cameraY = scene.cameras.main.scrollY;

    // Calculate the position and size in screen coordinates
    const x = (bounds.x - cameraX) * scaleX + canvasBounds.left;
    const y = (bounds.y - cameraY) * scaleY + canvasBounds.top;
    const width = bounds.width * scaleX;
    const height = bounds.height * scaleY;

    // Update the overlay position and size
    overlay.style.left = `${x}px`;
    overlay.style.top = `${y}px`;
    overlay.style.width = `${width}px`;
    overlay.style.height = `${height}px`;
  }

  /**
   * Remove all test overlays from the DOM
   */
  static removeAllOverlays(): void {
    const overlays = document.querySelectorAll('[data-testid]');
    overlays.forEach((overlay) => {
      if (overlay.parentElement) {
        overlay.parentElement.removeChild(overlay);
      }
    });
  }
}

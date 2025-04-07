import Phaser from 'phaser';

/**
 * Interactable
 *
 * An object that can be interacted with in the grid-based scene
 */
export class Interactable extends Phaser.GameObjects.Sprite {
  // Unique identifier
  private id: string;

  // Type of interactable (tower, ruins, etc.)
  private type: string;

  // Grid position
  private gridX: number;
  private gridY: number;

  // Distance at which the interactable triggers
  private triggerDistance: number;

  // Whether the interactable has been triggered
  private triggered: boolean = false;

  // Whether the interactable has been discovered via radio signal
  private discovered: boolean = false;

  // Visual indicator for discovered interactables
  private discoveryMarker: Phaser.GameObjects.Graphics | null = null;

  /**
   * Create a new interactable
   * @param scene Reference to the scene
   * @param id Unique identifier
   * @param type Type of interactable
   * @param gridX Grid X coordinate
   * @param gridY Grid Y coordinate
   * @param triggerDistance Distance at which the interactable triggers
   */
  constructor(
    scene: Phaser.Scene,
    id: string,
    type: string,
    gridX: number,
    gridY: number,
    triggerDistance: number
  ) {
    // Calculate world position from grid position
    const tileSize = 32; // Assuming 32x32 tiles
    const worldX = gridX * tileSize + tileSize / 2;
    const worldY = gridY * tileSize + tileSize / 2;

    super(scene, worldX, worldY, type);

    this.id = id;
    this.type = type;
    this.gridX = gridX;
    this.gridY = gridY;
    this.triggerDistance = triggerDistance;

    // Set origin to center
    this.setOrigin(0.5, 0.5);

    // Add pulsing effect
    this.createPulseEffect();

    // Set depth to ensure visibility
    this.setDepth(5);
  }

  /**
   * Create a pulsing effect for the interactable
   */
  private createPulseEffect(): void {
    // Add a slight pulsing effect
    this.scene.tweens.add({
      targets: this,
      scaleX: 1.1,
      scaleY: 1.1,
      duration: 1000,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut',
    });
  }

  /**
   * Trigger the interactable
   */
  trigger(): void {
    if (this.triggered) {
      return;
    }

    this.triggered = true;

    // Add visual effect
    this.scene.tweens.add({
      targets: this,
      alpha: 0.7,
      duration: 500,
      ease: 'Power2',
    });

    // Add particle effect
    const particles = this.scene.add.particles(this.type);

    particles.createEmitter({
      x: this.x,
      y: this.y,
      speed: { min: 20, max: 50 },
      angle: { min: 0, max: 360 },
      scale: { start: 0.5, end: 0 },
      lifespan: 1000,
      quantity: 20,
      blendMode: 'ADD',
    });

    // Remove particles after animation
    this.scene.time.delayedCall(1000, () => {
      particles.destroy();
    });
  }

  /**
   * Check if the interactable has been triggered
   */
  isTriggered(): boolean {
    return this.triggered;
  }

  /**
   * Get the interactable's ID
   */
  getId(): string {
    return this.id;
  }

  /**
   * Get the interactable's type
   */
  getType(): string {
    return this.type;
  }

  /**
   * Get the interactable's grid X coordinate
   */
  getGridX(): number {
    return this.gridX;
  }

  /**
   * Get the interactable's grid Y coordinate
   */
  getGridY(): number {
    return this.gridY;
  }

  /**
   * Get the interactable's trigger distance
   */
  getTriggerDistance(): number {
    return this.triggerDistance;
  }

  /**
   * Set whether the interactable has been discovered via radio signal
   * @param discovered Whether the interactable has been discovered
   */
  setDiscovered(discovered: boolean): void {
    this.discovered = discovered;

    if (discovered) {
      this.createDiscoveryMarker();
    } else if (this.discoveryMarker) {
      this.discoveryMarker.destroy();
      this.discoveryMarker = null;
    }
  }

  /**
   * Check if the interactable has been discovered via radio signal
   */
  isDiscovered(): boolean {
    return this.discovered;
  }

  /**
   * Create a visual marker to indicate that this location was discovered via radio signal
   */
  private createDiscoveryMarker(): void {
    // Remove existing marker if any
    if (this.discoveryMarker) {
      this.discoveryMarker.destroy();
    }

    // Create a new marker
    this.discoveryMarker = this.scene.add.graphics();
    this.discoveryMarker.setDepth(4); // Below the interactable

    // Draw a circle
    this.discoveryMarker.lineStyle(2, 0xffff00, 1);
    this.discoveryMarker.strokeCircle(this.x, this.y, 20);

    // Add a pulsing effect
    this.scene.tweens.add({
      targets: this.discoveryMarker,
      alpha: 0.5,
      duration: 1000,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut',
    });
  }
}

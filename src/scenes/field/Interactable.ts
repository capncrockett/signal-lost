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
}

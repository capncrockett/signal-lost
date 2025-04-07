import * as Phaser from 'phaser';

/**
 * Player
 *
 * Player character with grid-based movement
 */
export class Player extends Phaser.GameObjects.Sprite {
  // Grid position
  private gridX: number;
  private gridY: number;

  // Target position for movement
  private targetX: number | null = null;
  private targetY: number | null = null;

  // Movement speed in pixels per second
  private moveSpeed: number = 200;

  /**
   * Create a new player
   * @param scene Reference to the scene
   * @param gridX Initial grid X coordinate
   * @param gridY Initial grid Y coordinate
   * @param texture Texture key
   */
  constructor(scene: Phaser.Scene, gridX: number, gridY: number, texture: string) {
    // Calculate world position from grid position
    const tileSize = 32; // Assuming 32x32 tiles
    const worldX = gridX * tileSize + tileSize / 2;
    const worldY = gridY * tileSize + tileSize / 2;

    super(scene, worldX, worldY, texture);

    this.gridX = gridX;
    this.gridY = gridY;

    // Set up animations
    this.createAnimations();

    // Set origin to center
    this.setOrigin(0.5, 0.5);

    // Play idle animation
    this.play('player-idle-down');
  }

  /**
   * Create player animations
   */
  private createAnimations(): void {
    // Create animations if they don't exist
    const anims = this.scene.anims;

    if (!anims.exists('player-idle-down')) {
      anims.create({
        key: 'player-idle-down',
        frames: anims.generateFrameNumbers('player', { start: 0, end: 0 }),
        frameRate: 10,
        repeat: -1,
      });
    }

    if (!anims.exists('player-idle-up')) {
      anims.create({
        key: 'player-idle-up',
        frames: anims.generateFrameNumbers('player', { start: 1, end: 1 }),
        frameRate: 10,
        repeat: -1,
      });
    }

    if (!anims.exists('player-idle-left')) {
      anims.create({
        key: 'player-idle-left',
        frames: anims.generateFrameNumbers('player', { start: 2, end: 2 }),
        frameRate: 10,
        repeat: -1,
      });
    }

    if (!anims.exists('player-idle-right')) {
      anims.create({
        key: 'player-idle-right',
        frames: anims.generateFrameNumbers('player', { start: 3, end: 3 }),
        frameRate: 10,
        repeat: -1,
      });
    }

    if (!anims.exists('player-walk-down')) {
      anims.create({
        key: 'player-walk-down',
        frames: anims.generateFrameNumbers('player', { start: 4, end: 7 }),
        frameRate: 10,
        repeat: -1,
      });
    }

    if (!anims.exists('player-walk-up')) {
      anims.create({
        key: 'player-walk-up',
        frames: anims.generateFrameNumbers('player', { start: 8, end: 11 }),
        frameRate: 10,
        repeat: -1,
      });
    }

    if (!anims.exists('player-walk-left')) {
      anims.create({
        key: 'player-walk-left',
        frames: anims.generateFrameNumbers('player', { start: 12, end: 15 }),
        frameRate: 10,
        repeat: -1,
      });
    }

    if (!anims.exists('player-walk-right')) {
      anims.create({
        key: 'player-walk-right',
        frames: anims.generateFrameNumbers('player', { start: 16, end: 19 }),
        frameRate: 10,
        repeat: -1,
      });
    }
  }

  /**
   * Update the player
   * @param time Current time
   * @param delta Time since last update
   */
  update(time: number, delta: number): void {
    // Skip if not moving
    if (this.targetX === null || this.targetY === null) {
      return;
    }

    // Calculate distance to move this frame
    const distance = (this.moveSpeed * delta) / 1000;

    // Calculate direction to target
    const dx = this.targetX - this.x;
    const dy = this.targetY - this.y;
    const length = Math.sqrt(dx * dx + dy * dy);

    // Check if we've reached the target
    if (length <= distance) {
      // Snap to target position
      this.x = this.targetX;
      this.y = this.targetY;

      // Clear target
      this.targetX = null;
      this.targetY = null;

      // Play idle animation
      if (this.anims.currentAnim) {
        const direction = this.anims.currentAnim.key.split('-')[2];
        this.play(`player-idle-${direction}`);
      }

      return;
    }

    // Normalize direction
    const dirX = dx / length;
    const dirY = dy / length;

    // Move towards target
    this.x += dirX * distance;
    this.y += dirY * distance;
  }

  /**
   * Move to a grid position
   * @param gridX Grid X coordinate
   * @param gridY Grid Y coordinate
   */
  moveTo(gridX: number, gridY: number): void {
    // Update grid position
    this.gridX = gridX;
    this.gridY = gridY;

    // Calculate world position
    const tileSize = 32; // Assuming 32x32 tiles
    const worldX = gridX * tileSize + tileSize / 2;
    const worldY = gridY * tileSize + tileSize / 2;

    // Set target position
    this.targetX = worldX;
    this.targetY = worldY;

    // Determine direction
    const dx = gridX - this.gridX;
    const dy = gridY - this.gridY;

    // Play walk animation
    if (Math.abs(dx) > Math.abs(dy)) {
      // Horizontal movement
      if (dx > 0) {
        this.play('player-walk-right');
      } else {
        this.play('player-walk-left');
      }
    } else {
      // Vertical movement
      if (dy > 0) {
        this.play('player-walk-down');
      } else {
        this.play('player-walk-up');
      }
    }
  }

  /**
   * Check if the player is currently moving
   * @returns True if the player is moving
   */
  isMoving(): boolean {
    return this.targetX !== null && this.targetY !== null;
  }

  /**
   * Get the player's grid X coordinate
   */
  getGridX(): number {
    return this.gridX;
  }

  /**
   * Get the player's grid Y coordinate
   */
  getGridY(): number {
    return this.gridY;
  }
}

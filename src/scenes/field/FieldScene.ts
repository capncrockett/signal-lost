import Phaser from 'phaser';
import { GridSystem } from './GridSystem';
import { Player } from './Player';
import { Interactable } from './Interactable';
import { SaveManager } from '../../utils/SaveManager';

/**
 * FieldScene
 * 
 * Main exploration scene with grid-based movement and interactable objects
 */
export class FieldScene extends Phaser.Scene {
  // Grid system for managing movement and positions
  private gridSystem!: GridSystem;
  
  // Player character
  private player!: Player;
  
  // Interactable objects in the scene
  private interactables: Interactable[] = [];
  
  // Map of interactable IDs to their objects
  private interactableMap: Map<string, Interactable> = new Map();
  
  // Event emitter for scene events
  private eventEmitter: Phaser.Events.EventEmitter;
  
  constructor() {
    super({ key: 'FieldScene' });
    this.eventEmitter = new Phaser.Events.EventEmitter();
  }
  
  /**
   * Preload assets for the scene
   */
  preload(): void {
    // Load tilemap
    this.load.image('tiles', 'assets/images/tiles.png');
    this.load.tilemapTiledJSON('field', 'assets/maps/field.json');
    
    // Load player sprite
    this.load.spritesheet('player', 'assets/images/player.png', {
      frameWidth: 32,
      frameHeight: 32
    });
    
    // Load interactable sprites
    this.load.image('tower', 'assets/images/tower.png');
    this.load.image('ruins', 'assets/images/ruins.png');
  }
  
  /**
   * Create the scene
   */
  create(): void {
    // Create tilemap
    const map = this.make.tilemap({ key: 'field' });
    const tileset = map.addTilesetImage('tiles', 'tiles');
    
    if (!tileset) {
      console.error('Failed to load tileset');
      return;
    }
    
    // Create layers
    const groundLayer = map.createLayer('Ground', tileset);
    const obstaclesLayer = map.createLayer('Obstacles', tileset);
    
    if (!groundLayer || !obstaclesLayer) {
      console.error('Failed to create layers');
      return;
    }
    
    // Set collision on obstacles layer
    obstaclesLayer.setCollisionByProperty({ collides: true });
    
    // Initialize grid system
    const tileSize = 32;
    const gridWidth = map.width;
    const gridHeight = map.height;
    this.gridSystem = new GridSystem(this, gridWidth, gridHeight, tileSize);
    
    // Add collision from tilemap to grid system
    for (let y = 0; y < gridHeight; y++) {
      for (let x = 0; x < gridWidth; x++) {
        const tile = obstaclesLayer.getTileAt(x, y);
        if (tile && tile.properties.collides) {
          this.gridSystem.setTileCollision(x, y, true);
        }
      }
    }
    
    // Create player
    const playerStartX = 5;
    const playerStartY = 5;
    this.player = new Player(this, playerStartX, playerStartY, 'player');
    this.add.existing(this.player);
    
    // Set up camera to follow player
    this.cameras.main.startFollow(this.player);
    this.cameras.main.setBounds(0, 0, map.widthInPixels, map.heightInPixels);
    
    // Create interactables
    this.createInteractables();
    
    // Set up input
    this.setupInput();
    
    // Initialize SaveManager
    SaveManager.initialize();
  }
  
  /**
   * Create interactable objects in the scene
   */
  private createInteractables(): void {
    // Define interactable objects
    const interactableData = [
      { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
      { id: 'tower2', type: 'tower', x: 20, y: 15, triggerDistance: 2 },
      { id: 'ruins1', type: 'ruins', x: 15, y: 12, triggerDistance: 1 }
    ];
    
    // Create interactable objects
    for (const data of interactableData) {
      // Skip if already discovered
      if (SaveManager.getFlag(`found${data.id}`)) {
        continue;
      }
      
      // Create interactable
      const interactable = new Interactable(
        this,
        data.id,
        data.type,
        data.x,
        data.y,
        data.triggerDistance
      );
      
      // Add to scene
      this.add.existing(interactable);
      
      // Add to collections
      this.interactables.push(interactable);
      this.interactableMap.set(data.id, interactable);
    }
  }
  
  /**
   * Set up input handlers
   */
  private setupInput(): void {
    // Create cursor keys
    const cursors = this.input.keyboard?.createCursorKeys();
    
    if (!cursors) {
      console.error('Failed to create cursor keys');
      return;
    }
    
    // Handle key down events
    this.input.keyboard?.on('keydown', (event: KeyboardEvent) => {
      // Skip if player is already moving
      if (this.player.isMoving()) {
        return;
      }
      
      // Handle movement
      switch (event.code) {
        case 'ArrowUp':
        case 'KeyW':
          this.movePlayer(0, -1);
          break;
        case 'ArrowDown':
        case 'KeyS':
          this.movePlayer(0, 1);
          break;
        case 'ArrowLeft':
        case 'KeyA':
          this.movePlayer(-1, 0);
          break;
        case 'ArrowRight':
        case 'KeyD':
          this.movePlayer(1, 0);
          break;
        case 'Space':
        case 'KeyE':
          this.interact();
          break;
      }
    });
  }
  
  /**
   * Move the player in the specified direction
   * @param dx X direction (-1, 0, 1)
   * @param dy Y direction (-1, 0, 1)
   * @returns True if the player moved successfully
   */
  movePlayer(dx: number, dy: number): boolean {
    // Get current player position
    const currentX = this.player.getGridX();
    const currentY = this.player.getGridY();
    
    // Calculate new position
    const newX = currentX + dx;
    const newY = currentY + dy;
    
    // Check if the new position is valid
    if (!this.gridSystem.isValidPosition(newX, newY)) {
      return false;
    }
    
    // Check if the new position is blocked
    if (this.gridSystem.isCollision(newX, newY)) {
      return false;
    }
    
    // Move the player
    this.player.moveTo(newX, newY);
    
    // Check for nearby interactables
    this.checkInteractables();
    
    return true;
  }
  
  /**
   * Check for interactables near the player
   */
  private checkInteractables(): void {
    const playerX = this.player.getGridX();
    const playerY = this.player.getGridY();
    
    // Check each interactable
    for (const interactable of this.interactables) {
      // Skip if already triggered
      if (interactable.isTriggered()) {
        continue;
      }
      
      // Check if player is within trigger distance
      const distance = this.gridSystem.getDistance(
        playerX,
        playerY,
        interactable.getGridX(),
        interactable.getGridY()
      );
      
      if (distance <= interactable.getTriggerDistance()) {
        // Trigger the interactable
        interactable.trigger();
        
        // Emit event
        this.eventEmitter.emit('interactableTriggered', interactable.getId(), interactable.getType());
        
        // Save to game state
        SaveManager.setFlag(`found${interactable.getId()}`, true);
      }
    }
  }
  
  /**
   * Interact with the nearest interactable
   */
  private interact(): void {
    const playerX = this.player.getGridX();
    const playerY = this.player.getGridY();
    
    // Find the nearest interactable
    let nearestInteractable: Interactable | null = null;
    let nearestDistance = Number.MAX_VALUE;
    
    for (const interactable of this.interactables) {
      const distance = this.gridSystem.getDistance(
        playerX,
        playerY,
        interactable.getGridX(),
        interactable.getGridY()
      );
      
      if (distance <= 1 && distance < nearestDistance) {
        nearestInteractable = interactable;
        nearestDistance = distance;
      }
    }
    
    // Interact with the nearest interactable
    if (nearestInteractable) {
      // Emit interaction event
      this.eventEmitter.emit('interactableInteracted', nearestInteractable.getId(), nearestInteractable.getType());
    }
  }
  
  /**
   * Update the scene
   * @param time Current time
   * @param delta Time since last update
   */
  update(time: number, delta: number): void {
    // Update player
    this.player.update(time, delta);
    
    // Update interactables
    for (const interactable of this.interactables) {
      interactable.update(time, delta);
    }
  }
  
  /**
   * Add an event listener
   * @param event Event name
   * @param fn Callback function
   * @param context Context for the callback
   */
  on(event: string, fn: Function, context?: any): this {
    this.eventEmitter.on(event, fn, context);
    return this;
  }
  
  /**
   * Remove an event listener
   * @param event Event name
   * @param fn Callback function
   * @param context Context for the callback
   */
  off(event: string, fn?: Function, context?: any): this {
    this.eventEmitter.off(event, fn, context);
    return this;
  }
  
  /**
   * Get the grid system
   */
  getGridSystem(): GridSystem {
    return this.gridSystem;
  }
  
  /**
   * Get the player
   */
  getPlayer(): Player {
    return this.player;
  }
  
  /**
   * Get an interactable by ID
   * @param id Interactable ID
   */
  getInteractable(id: string): Interactable | undefined {
    return this.interactableMap.get(id);
  }
  
  /**
   * Get all interactables
   */
  getInteractables(): Interactable[] {
    return [...this.interactables];
  }
}

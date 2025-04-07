import Phaser from 'phaser';
import { GridSystem } from './GridSystem';
import { Player } from './Player';
import { Interactable } from './Interactable';
import { SaveManager } from '../../utils/SaveManager';
import { NarrativeEngine } from '../../narrative/NarrativeEngine';
import { NarrativeRenderer } from '../../narrative/NarrativeRenderer';
import { interactableData } from './InteractableConfig';
import { Inventory } from '../../inventory/Inventory';
import { InventoryUI } from '../../inventory/InventoryUI';
import { itemsData } from '../../inventory/ItemsConfig';
import { Item } from '../../inventory/Item';

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

  // Narrative engine
  private narrativeEngine!: NarrativeEngine;

  // Narrative renderer
  private narrativeRenderer!: NarrativeRenderer;

  // Inventory
  private inventory!: Inventory;

  // Inventory UI
  private inventoryUI!: InventoryUI;

  constructor() {
    super({ key: 'FieldScene' });
    this.eventEmitter = new Phaser.Events.EventEmitter();
  }

  /**
   * Preload assets for the scene
   */
  preload(): void {
    // Load tilemap with multiple path formats
    this.load.image('tiles', '/assets/images/tiles.png');
    this.load.tilemapTiledJSON('field', '/assets/maps/field.json');

    // Alternative paths without leading slash
    this.load.image('tiles_alt', 'assets/images/tiles.png');
    this.load.tilemapTiledJSON('field_alt', 'assets/maps/field.json');

    // Load player sprite
    this.load.spritesheet('player', '/assets/images/player.png', {
      frameWidth: 32,
      frameHeight: 32,
    });
    this.load.spritesheet('player_alt', 'assets/images/player.png', {
      frameWidth: 32,
      frameHeight: 32,
    });

    // Load interactable sprites
    this.load.image('tower', '/assets/images/tower.png');
    this.load.image('ruins', '/assets/images/ruins.png');
    this.load.image('tower_alt', 'assets/images/tower.png');
    this.load.image('ruins_alt', 'assets/images/ruins.png');

    // Load narrative events
    this.load.json('narrative_events', '/assets/narrative/events.json');
    this.load.json('narrative_events_alt', 'assets/narrative/events.json');

    // Log asset loading events
    this.load.on('filecomplete', (key: string, type: string) => {
      console.log(`FieldScene asset loaded: ${key} (${type})`);
    });

    this.load.on('loaderror', (file: Phaser.Loader.File) => {
      // Ensure url is a string
      const url = typeof file.url === 'string' ? file.url : String(file.url);
      console.error(`FieldScene error loading asset: ${file.key} from ${url}`);
      // If this is not an alternative asset, try loading with the alternative path
      if (!file.key.endsWith('_alt')) {
        if (url.startsWith('/')) {
          const newUrl = url.substring(1);
          console.log(`FieldScene retrying with alternative path: ${newUrl}`);
        }
      }
    });
  }

  /**
   * Create the scene
   */
  create(): void {
    try {
      // Create tilemap and layers
      const mapData = this.createTilemap();
      if (!mapData) return;

      const { map, obstaclesLayer } = mapData;

      // Initialize grid system and set up collisions
      this.setupGridSystem(map, obstaclesLayer);

      // Create player
      this.createPlayer();

      // Set up camera to follow player
      this.setupCamera(map);

      // Create interactables
      this.createInteractables();

      // Set up resize handler
      this.scale.on(
        'resize',
        (width: number, height: number) => this.handleResize(width, height),
        this
      );

      // Initial resize to set correct positions
      this.handleResize(this.scale.width, this.scale.height);

      // Set up input
      this.setupInput();

      // Initialize SaveManager
      SaveManager.initialize();

      // Initialize narrative engine
      this.initializeNarrativeEngine();

      // Initialize inventory system
      this.initializeInventorySystem();
    } catch (error) {
      console.error('Error creating field scene:', error);
    }
  }

  /**
   * Create tilemap and layers
   * @returns Map data including the tilemap and layers, or null if creation failed
   */
  private createTilemap(): {
    map: Phaser.Tilemaps.Tilemap;
    groundLayer: Phaser.Tilemaps.TilemapLayer;
    obstaclesLayer: Phaser.Tilemaps.TilemapLayer;
  } | null {
    try {
      // Try to create tilemap with primary key
      let map = this.make.tilemap({ key: 'field' });

      // If that fails, try the alternative key
      if (!map) {
        console.log('Trying alternative tilemap key');
        map = this.make.tilemap({ key: 'field_alt' });
      }

      if (!map) {
        console.error('Failed to create tilemap with both keys');
        return null;
      }

      // Add orientation property to fix the error
      if (map.orientation === undefined) {
        // @ts-expect-error - Adding missing property
        map.orientation = 'orthogonal';
      }

      // Try primary tileset key first
      let tileset = map.addTilesetImage('tiles', 'tiles');

      // If that fails, try the alternative key
      if (!tileset) {
        console.log('Trying alternative tileset key');
        tileset = map.addTilesetImage('tiles', 'tiles_alt');
      }

      if (!tileset) {
        console.error('Failed to load tileset with both keys');
        return null;
      }

      // Create layers
      const groundLayer = map.createLayer('Ground', tileset);
      const obstaclesLayer = map.createLayer('Obstacles', tileset);

      if (!groundLayer || !obstaclesLayer) {
        console.error('Failed to create layers');
        return null;
      }

      // Set collision on obstacles layer
      obstaclesLayer.setCollisionByProperty({ collides: true });

      return { map, groundLayer, obstaclesLayer };
    } catch (error) {
      console.error('Error creating tilemap:', error);
      return null;
    }
  }

  /**
   * Set up grid system and collisions
   * @param map The tilemap
   * @param obstaclesLayer The obstacles layer
   */
  private setupGridSystem(
    map: Phaser.Tilemaps.Tilemap,
    obstaclesLayer: Phaser.Tilemaps.TilemapLayer
  ): void {
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
  }

  /**
   * Create player character
   */
  private createPlayer(): void {
    const playerStartX = 5;
    const playerStartY = 5;
    this.player = new Player(this, playerStartX, playerStartY, 'player');
    this.add.existing(this.player);
  }

  /**
   * Set up camera to follow player
   * @param map The tilemap
   */
  private setupCamera(map: Phaser.Tilemaps.Tilemap): void {
    this.cameras.main.startFollow(this.player);
    this.cameras.main.setBounds(0, 0, map.widthInPixels, map.heightInPixels);
  }

  /**
   * Create interactable objects in the scene
   */
  private createInteractables(): void {
    // Create interactable objects from configuration
    for (const data of interactableData) {
      // Skip if already discovered and collected
      if (SaveManager.getFlag(`collected${data.id}`)) {
        continue;
      }

      // Create interactable
      const interactable = this.createInteractable(data);

      // If the location was discovered via radio signal, mark it as found
      if (SaveManager.getFlag(`discovered_${data.id}`)) {
        interactable.setDiscovered(true);
      }

      // Add to interactables array and map
      this.interactables.push(interactable);
      this.interactableMap.set(data.id, interactable);
    }

    // Check for signal-discovered locations
    this.checkSignalDiscoveredLocations();
  }

  /**
   * Create a single interactable object
   * @param data The interactable configuration
   * @returns The created interactable
   */
  private createInteractable(data: {
    id: string;
    type: string;
    x: number;
    y: number;
    triggerDistance: number;
  }): Interactable {
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

    return interactable;
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
    this.input.keyboard?.on('keydown', this.handleKeyDown.bind(this));
  }

  /**
   * Handle keyboard input
   * @param event The keyboard event
   */
  private handleKeyDown(event: KeyboardEvent): void {
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
      case 'KeyI':
        this.toggleInventory();
        break;
    }
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
        this.eventEmitter.emit(
          'interactableTriggered',
          interactable.getId(),
          interactable.getType()
        );

        // Save to game state
        SaveManager.setFlag(`found${interactable.getId()}`, true);
      }
    }
  }

  /**
   * Toggle the inventory UI
   */
  private toggleInventory(): void {
    if (this.inventoryUI) {
      this.inventoryUI.toggle();
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
      this.eventEmitter.emit(
        'interactableInteracted',
        nearestInteractable.getId(),
        nearestInteractable.getType()
      );
    }
  }

  /**
   * Initialize the inventory system
   */
  private initializeInventorySystem(): void {
    // Create inventory
    this.inventory = new Inventory(20);

    // Load item definitions
    this.inventory.loadItemDefinitions(itemsData);

    // Load saved inventory
    this.inventory.loadInventory();

    // Create inventory UI
    this.inventoryUI = new InventoryUI(this, this.inventory, {
      x: this.cameras.main.width / 2,
      y: this.cameras.main.height / 2,
    });
    this.add.existing(this.inventoryUI);

    // Add starting items if this is a new game
    this.addStartingItems();
  }

  /**
   * Add starting items to the inventory
   */
  private addStartingItems(): void {
    // Check if this is the first time loading the scene
    if (!SaveManager.getFlag('field_scene_visited')) {
      // Add radio to inventory
      const radioItem = itemsData.find(item => item.id === 'radio');
      if (radioItem) {
        this.inventory.addItem(new Item(radioItem));
      }

      // Add journal to inventory
      const journalItem = itemsData.find(item => item.id === 'journal');
      if (journalItem) {
        this.inventory.addItem(new Item(journalItem));
      }

      // Mark scene as visited
      SaveManager.setFlag('field_scene_visited', true);
    }
  }

  /**
   * Set up inventory event listeners
   */
  private setupInventoryEventListeners(): void {
    // Listen for item use events
    this.inventory.on('itemUsed', (item: Item) => {
      console.log(`Item used: ${item.getName()}`);

      // Handle item effects
      const effects = item.getEffects();
      const action = effects.action as string | undefined;

      if (action) {
        switch (action) {
          case 'open_radio':
            // Return to main scene to use radio
            this.scene.start('MainScene');
            break;

          case 'open_map':
            // Show map overlay
            console.log('Opening map...');
            // TODO: Implement map overlay
            break;

          case 'read_note':
            // Show note content
            const content = effects.content as string | undefined;
            if (content) {
              // Trigger a narrative event to display the note content
              this.narrativeEngine.addEvent({
                id: `note_${Date.now()}`,
                message: content,
                choices: [
                  {
                    text: 'Close',
                    outcome: '',
                  },
                ],
              });
              this.narrativeEngine.triggerEvent(`note_${Date.now()}`);
            }
            break;

          default:
            console.log(`Unknown action: ${action}`);
            break;
        }
      }
    });

    // Listen for interactable events that give items
    this.eventEmitter.on('interactableTriggered', (id: string, type: string) => {
      // Check if this interactable gives an item
      if (type === 'item') {
        // Find the corresponding item in the item definitions
        const itemData = itemsData.find(item => item.id === id);
        if (itemData) {
          // Add the item to the inventory
          const item = new Item(itemData);
          const added = this.inventory.addItem(item);

          if (added) {
            console.log(`Added item to inventory: ${item.getName()}`);

            // Mark the item as collected
            SaveManager.setFlag(`collected_${id}`, true);
          } else {
            console.log('Inventory is full!');
            // TODO: Show inventory full message
          }
        }
      }
    });
  }

  /**
   * Initialize the narrative engine
   */
  private initializeNarrativeEngine(): void {
    // Create and initialize narrative engine
    this.createNarrativeEngine();

    // Create narrative renderer
    this.createNarrativeRenderer();

    // Set up event listeners
    this.setupNarrativeEventListeners();
  }

  /**
   * Create and initialize the narrative engine
   */
  private createNarrativeEngine(): void {
    // Create narrative engine
    this.narrativeEngine = new NarrativeEngine();

    // Load narrative events
    const eventsData = this.cache.json.get('narrative_events');
    if (eventsData) {
      this.narrativeEngine.loadEvents(JSON.stringify(eventsData));
    }
  }

  /**
   * Create the narrative renderer
   */
  private createNarrativeRenderer(): void {
    // Create narrative renderer
    this.narrativeRenderer = new NarrativeRenderer(
      this,
      this.cameras.main.width / 2,
      this.cameras.main.height / 2,
      this.narrativeEngine
    );
    this.add.existing(this.narrativeRenderer);
  }

  /**
   * Set up narrative event listeners
   */
  private setupNarrativeEventListeners(): void {
    // Listen for narrative events
    this.narrativeEngine.on('narrativeEvent', (event) => {
      console.log(`Narrative event triggered: ${event.id}`);
    });

    this.narrativeEngine.on('narrativeChoice', (data) => {
      console.log(`Choice made: ${data.choice.text}`);
    });

    // Listen for interactable events
    this.eventEmitter.on('interactableTriggered', (id: string, type: string) => {
      // Trigger corresponding narrative event
      if (type === 'tower') {
        this.narrativeEngine.triggerEvent('tower_discovery');
      } else if (type === 'ruins') {
        this.narrativeEngine.triggerEvent('ruins_discovery');
      }
    });
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

    // Update narrative renderer
    if (this.narrativeRenderer) {
      this.narrativeRenderer.update(time, delta);
    }
  }

  /**
   * Add an event listener
   * @param event Event name
   * @param fn Callback function
   * @param context Context for the callback
   */
  on(event: string, fn: (...args: unknown[]) => void, context?: unknown): this {
    this.eventEmitter.on(event, fn, context);
    return this;
  }

  /**
   * Remove an event listener
   * @param event Event name
   * @param fn Callback function
   * @param context Context for the callback
   */
  off(event: string, fn?: (...args: unknown[]) => void, context?: unknown): this {
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

  /**
   * Check for locations discovered via radio signals
   */
  private checkSignalDiscoveredLocations(): void {
    // Get all discovered locations from SaveManager
    const discoveredLocations: string[] = [];

    // Check for tower1 location
    if (SaveManager.getFlag('discovered_tower1')) {
      discoveredLocations.push('tower1');
    }

    // Check for ruins1 location
    if (SaveManager.getFlag('discovered_ruins1')) {
      discoveredLocations.push('ruins1');
    }

    // Add visual indicators for discovered locations
    for (const locationId of discoveredLocations) {
      const locationData = SaveManager.getData(`location_${locationId}`);
      if (
        locationData &&
        typeof locationData === 'object' &&
        'x' in locationData &&
        'y' in locationData &&
        typeof locationData.x === 'number' &&
        typeof locationData.y === 'number'
      ) {
        // Convert to numbers to ensure type safety
        const x = Number(locationData.x);
        const y = Number(locationData.y);
        this.addLocationMarker(locationId, x, y);
      }
    }
  }

  /**
   * Add a visual marker for a discovered location
   * @param locationId The location ID
   * @param gridX The grid X coordinate
   * @param gridY The grid Y coordinate
   */
  private addLocationMarker(locationId: string, gridX: number, gridY: number): void {
    // Convert grid coordinates to pixel coordinates
    const pixelX = gridX * this.gridSystem.getTileSize() + this.gridSystem.getTileSize() / 2;
    const pixelY = gridY * this.gridSystem.getTileSize() + this.gridSystem.getTileSize() / 2;

    // Create a marker sprite
    const marker = this.add.circle(pixelX, pixelY, 16, 0xffff00, 0.8);
    marker.setDepth(10);

    // Add a pulsing effect
    this.tweens.add({
      targets: marker,
      scaleX: 1.5,
      scaleY: 1.5,
      alpha: 0.4,
      duration: 1000,
      yoyo: true,
      repeat: -1,
      ease: 'Sine.easeInOut',
    });

    // Add a label
    const label = this.add.text(pixelX, pixelY - 30, locationId.toUpperCase(), {
      fontSize: '14px',
      color: '#ffffff',
      backgroundColor: '#000000',
      padding: { x: 5, y: 3 },
    });
    label.setOrigin(0.5, 0.5);
    label.setDepth(11);
  }

  /**
   * Handle resize events
   * @param width New width of the scene
   * @param height New height of the scene
   */
  private handleResize(width: number, height: number): void {
    // Adjust camera bounds if needed
    if (this.cameras.main && this.gridSystem) {
      // Get the map dimensions in pixels
      const mapWidth = this.gridSystem.getMapWidth() * this.gridSystem.getTileSize();
      const mapHeight = this.gridSystem.getMapHeight() * this.gridSystem.getTileSize();

      // Set camera bounds to ensure the map is fully visible
      this.cameras.main.setBounds(0, 0, mapWidth, mapHeight);

      // Adjust zoom level to ensure the map fits well on screen
      const zoomX = width / mapWidth;
      const zoomY = height / mapHeight;
      const zoom = Math.min(zoomX, zoomY) * 0.9; // 90% to leave some margin

      // Don't zoom out too far or in too close
      const clampedZoom = Phaser.Math.Clamp(Number(zoom), 0.5, 2);
      this.cameras.main.setZoom(clampedZoom);

      // Make sure the camera is still following the player
      if (this.player) {
        const playerSprite = this.player.getSprite();
        if (playerSprite && playerSprite instanceof Phaser.GameObjects.GameObject) {
          this.cameras.main.startFollow(playerSprite);
        }
      }
    }

    // Adjust UI elements if needed
    if (this.narrativeRenderer) {
      this.narrativeRenderer.onResize(width, height);
    }
  }
}

import { FieldScene } from '../../../src/scenes/field/FieldScene';
import { GridSystem } from '../../../src/scenes/field/GridSystem';
import { Player } from '../../../src/scenes/field/Player';
import { Interactable } from '../../../src/scenes/field/Interactable';
import { SaveManager } from '../../../src/utils/SaveManager';

// Mock dependencies
jest.mock('../../../src/scenes/field/GridSystem');
jest.mock('../../../src/scenes/field/Player');
jest.mock('../../../src/scenes/field/Interactable');
jest.mock('../../../src/utils/SaveManager');

// Mock Phaser
const mockTilemap = {
  width: 20,
  height: 15,
  widthInPixels: 640,
  heightInPixels: 480,
  addTilesetImage: jest.fn().mockReturnValue({}),
  createLayer: jest.fn().mockReturnValue({
    setCollisionByProperty: jest.fn(),
    getTileAt: jest.fn().mockReturnValue({ properties: { collides: true } })
  })
};

const mockScene = {
  add: {
    existing: jest.fn(),
  },
  make: {
    tilemap: jest.fn().mockReturnValue(mockTilemap)
  },
  load: {
    image: jest.fn(),
    spritesheet: jest.fn(),
    tilemapTiledJSON: jest.fn()
  },
  input: {
    keyboard: {
      createCursorKeys: jest.fn().mockReturnValue({}),
      on: jest.fn()
    }
  },
  cameras: {
    main: {
      startFollow: jest.fn(),
      setBounds: jest.fn()
    }
  }
};

describe('FieldScene', () => {
  let fieldScene: FieldScene;
  
  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();
    
    // Create a field scene
    fieldScene = new FieldScene();
    
    // Mock the scene methods
    Object.assign(fieldScene, mockScene);
  });
  
  describe('preload', () => {
    test('should load assets', () => {
      fieldScene.preload();
      
      expect(mockScene.load.image).toHaveBeenCalledWith('tiles', 'assets/images/tiles.png');
      expect(mockScene.load.tilemapTiledJSON).toHaveBeenCalledWith('field', 'assets/maps/field.json');
      expect(mockScene.load.spritesheet).toHaveBeenCalledWith('player', 'assets/images/player.png', {
        frameWidth: 32,
        frameHeight: 32
      });
      expect(mockScene.load.image).toHaveBeenCalledWith('tower', 'assets/images/tower.png');
      expect(mockScene.load.image).toHaveBeenCalledWith('ruins', 'assets/images/ruins.png');
    });
  });
  
  describe('create', () => {
    test('should create the scene', () => {
      // Mock console.error to prevent output during tests
      const originalConsoleError = console.error;
      console.error = jest.fn();
      
      fieldScene.create();
      
      // Should create tilemap
      expect(mockScene.make.tilemap).toHaveBeenCalledWith({ key: 'field' });
      expect(mockTilemap.addTilesetImage).toHaveBeenCalledWith('tiles', 'tiles');
      expect(mockTilemap.createLayer).toHaveBeenCalledTimes(2);
      
      // Should create grid system
      expect(GridSystem).toHaveBeenCalledWith(fieldScene, 20, 15, 32);
      
      // Should create player
      expect(Player).toHaveBeenCalledWith(fieldScene, 5, 5, 'player');
      expect(mockScene.add.existing).toHaveBeenCalled();
      
      // Should set up camera
      expect(mockScene.cameras.main.startFollow).toHaveBeenCalled();
      expect(mockScene.cameras.main.setBounds).toHaveBeenCalledWith(0, 0, 640, 480);
      
      // Should initialize SaveManager
      expect(SaveManager.initialize).toHaveBeenCalled();
      
      // Restore console.error
      console.error = originalConsoleError;
    });
  });
  
  describe('movePlayer', () => {
    beforeEach(() => {
      // Set up the scene
      fieldScene.create();
      
      // Mock GridSystem methods
      const mockGridSystem = GridSystem.mock.instances[0];
      mockGridSystem.isValidPosition = jest.fn().mockReturnValue(true);
      mockGridSystem.isCollision = jest.fn().mockReturnValue(false);
      
      // Mock Player methods
      const mockPlayer = Player.mock.instances[0];
      mockPlayer.getGridX = jest.fn().mockReturnValue(5);
      mockPlayer.getGridY = jest.fn().mockReturnValue(5);
      mockPlayer.moveTo = jest.fn();
    });
    
    test('should move the player in the specified direction', () => {
      // Move right
      fieldScene.movePlayer(1, 0);
      
      // Should check if the new position is valid
      expect(GridSystem.mock.instances[0].isValidPosition).toHaveBeenCalledWith(6, 5);
      
      // Should check if the new position has a collision
      expect(GridSystem.mock.instances[0].isCollision).toHaveBeenCalledWith(6, 5);
      
      // Should move the player
      expect(Player.mock.instances[0].moveTo).toHaveBeenCalledWith(6, 5);
    });
    
    test('should not move the player to invalid positions', () => {
      // Mock isValidPosition to return false
      GridSystem.mock.instances[0].isValidPosition = jest.fn().mockReturnValue(false);
      
      // Try to move
      const result = fieldScene.movePlayer(1, 0);
      
      // Should return false
      expect(result).toBe(false);
      
      // Should not move the player
      expect(Player.mock.instances[0].moveTo).not.toHaveBeenCalled();
    });
    
    test('should not move the player to positions with collisions', () => {
      // Mock isCollision to return true
      GridSystem.mock.instances[0].isCollision = jest.fn().mockReturnValue(true);
      
      // Try to move
      const result = fieldScene.movePlayer(1, 0);
      
      // Should return false
      expect(result).toBe(false);
      
      // Should not move the player
      expect(Player.mock.instances[0].moveTo).not.toHaveBeenCalled();
    });
  });
  
  describe('checkInteractables', () => {
    beforeEach(() => {
      // Set up the scene
      fieldScene.create();
      
      // Mock GridSystem methods
      const mockGridSystem = GridSystem.mock.instances[0];
      mockGridSystem.getDistance = jest.fn().mockReturnValue(1);
      
      // Mock Player methods
      const mockPlayer = Player.mock.instances[0];
      mockPlayer.getGridX = jest.fn().mockReturnValue(5);
      mockPlayer.getGridY = jest.fn().mockReturnValue(5);
      
      // Create a mock interactable
      const mockInteractable = new Interactable(fieldScene as any, 'tower1', 'tower', 6, 5, 2);
      mockInteractable.getGridX = jest.fn().mockReturnValue(6);
      mockInteractable.getGridY = jest.fn().mockReturnValue(5);
      mockInteractable.getTriggerDistance = jest.fn().mockReturnValue(2);
      mockInteractable.isTriggered = jest.fn().mockReturnValue(false);
      mockInteractable.trigger = jest.fn();
      
      // Add the interactable to the scene
      // @ts-ignore - Accessing private property for testing
      fieldScene.interactables = [mockInteractable];
    });
    
    test('should trigger interactables within range', () => {
      // @ts-ignore - Accessing private method for testing
      fieldScene.checkInteractables();
      
      // Should check the distance
      expect(GridSystem.mock.instances[0].getDistance).toHaveBeenCalledWith(5, 5, 6, 5);
      
      // Should trigger the interactable
      expect(Interactable.mock.instances[0].trigger).toHaveBeenCalled();
      
      // Should save to game state
      expect(SaveManager.setFlag).toHaveBeenCalledWith('foundtower1', true);
    });
    
    test('should not trigger interactables that are already triggered', () => {
      // Mock isTriggered to return true
      Interactable.mock.instances[0].isTriggered = jest.fn().mockReturnValue(true);
      
      // @ts-ignore - Accessing private method for testing
      fieldScene.checkInteractables();
      
      // Should not check the distance
      expect(GridSystem.mock.instances[0].getDistance).not.toHaveBeenCalled();
      
      // Should not trigger the interactable
      expect(Interactable.mock.instances[0].trigger).not.toHaveBeenCalled();
    });
    
    test('should not trigger interactables that are out of range', () => {
      // Mock getDistance to return a value greater than the trigger distance
      GridSystem.mock.instances[0].getDistance = jest.fn().mockReturnValue(3);
      
      // @ts-ignore - Accessing private method for testing
      fieldScene.checkInteractables();
      
      // Should check the distance
      expect(GridSystem.mock.instances[0].getDistance).toHaveBeenCalledWith(5, 5, 6, 5);
      
      // Should not trigger the interactable
      expect(Interactable.mock.instances[0].trigger).not.toHaveBeenCalled();
    });
  });
  
  describe('interact', () => {
    beforeEach(() => {
      // Set up the scene
      fieldScene.create();
      
      // Mock GridSystem methods
      const mockGridSystem = GridSystem.mock.instances[0];
      mockGridSystem.getDistance = jest.fn().mockReturnValue(1);
      
      // Mock Player methods
      const mockPlayer = Player.mock.instances[0];
      mockPlayer.getGridX = jest.fn().mockReturnValue(5);
      mockPlayer.getGridY = jest.fn().mockReturnValue(5);
      
      // Create a mock interactable
      const mockInteractable = new Interactable(fieldScene as any, 'tower1', 'tower', 6, 5, 2);
      mockInteractable.getGridX = jest.fn().mockReturnValue(6);
      mockInteractable.getGridY = jest.fn().mockReturnValue(5);
      mockInteractable.getId = jest.fn().mockReturnValue('tower1');
      mockInteractable.getType = jest.fn().mockReturnValue('tower');
      
      // Add the interactable to the scene
      // @ts-ignore - Accessing private property for testing
      fieldScene.interactables = [mockInteractable];
      
      // Mock the event emitter
      // @ts-ignore - Accessing private property for testing
      fieldScene.eventEmitter = {
        emit: jest.fn(),
        on: jest.fn(),
        off: jest.fn()
      };
    });
    
    test('should interact with the nearest interactable', () => {
      // @ts-ignore - Accessing private method for testing
      fieldScene.interact();
      
      // Should check the distance
      expect(GridSystem.mock.instances[0].getDistance).toHaveBeenCalledWith(5, 5, 6, 5);
      
      // Should emit an event
      // @ts-ignore - Accessing private property for testing
      expect(fieldScene.eventEmitter.emit).toHaveBeenCalledWith(
        'interactableInteracted',
        'tower1',
        'tower'
      );
    });
    
    test('should not interact with interactables that are too far away', () => {
      // Mock getDistance to return a value greater than 1
      GridSystem.mock.instances[0].getDistance = jest.fn().mockReturnValue(2);
      
      // @ts-ignore - Accessing private method for testing
      fieldScene.interact();
      
      // Should check the distance
      expect(GridSystem.mock.instances[0].getDistance).toHaveBeenCalledWith(5, 5, 6, 5);
      
      // Should not emit an event
      // @ts-ignore - Accessing private property for testing
      expect(fieldScene.eventEmitter.emit).not.toHaveBeenCalled();
    });
  });
  
  describe('update', () => {
    test('should update player and interactables', () => {
      // Set up the scene
      fieldScene.create();
      
      // Create a mock interactable
      const mockInteractable = new Interactable(fieldScene as any, 'tower1', 'tower', 6, 5, 2);
      mockInteractable.update = jest.fn();
      
      // Add the interactable to the scene
      // @ts-ignore - Accessing private property for testing
      fieldScene.interactables = [mockInteractable];
      
      // Update the scene
      fieldScene.update(0, 16);
      
      // Should update the player
      expect(Player.mock.instances[0].update).toHaveBeenCalledWith(0, 16);
      
      // Should update the interactables
      expect(mockInteractable.update).toHaveBeenCalledWith(0, 16);
    });
  });
  
  describe('event handling', () => {
    test('should add and remove event listeners', () => {
      // Set up the scene
      fieldScene.create();
      
      // Mock the event emitter
      // @ts-ignore - Accessing private property for testing
      fieldScene.eventEmitter = {
        emit: jest.fn(),
        on: jest.fn(),
        off: jest.fn()
      };
      
      // Add an event listener
      const callback = jest.fn();
      fieldScene.on('test', callback);
      
      // Should call the event emitter's on method
      // @ts-ignore - Accessing private property for testing
      expect(fieldScene.eventEmitter.on).toHaveBeenCalledWith('test', callback, undefined);
      
      // Remove the event listener
      fieldScene.off('test', callback);
      
      // Should call the event emitter's off method
      // @ts-ignore - Accessing private property for testing
      expect(fieldScene.eventEmitter.off).toHaveBeenCalledWith('test', callback, undefined);
    });
  });
  
  describe('getters', () => {
    test('should return the correct values', () => {
      // Set up the scene
      fieldScene.create();
      
      // Should return the grid system
      expect(fieldScene.getGridSystem()).toBe(GridSystem.mock.instances[0]);
      
      // Should return the player
      expect(fieldScene.getPlayer()).toBe(Player.mock.instances[0]);
      
      // Should return interactables
      expect(fieldScene.getInteractables()).toEqual([]);
      
      // Should return undefined for non-existent interactable
      expect(fieldScene.getInteractable('nonexistent')).toBeUndefined();
    });
  });
});

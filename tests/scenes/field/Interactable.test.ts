import { Interactable } from '../../../src/scenes/field/Interactable';

// Mock Phaser.Scene
const mockScene = {
  add: {
    graphics: jest.fn(),
    image: jest.fn(),
    sprite: jest.fn(),
    text: jest.fn(),
    particles: jest.fn().mockReturnValue({
      createEmitter: jest.fn(),
      destroy: jest.fn()
    })
  },
  time: {
    delayedCall: jest.fn((delay: number, callback: Function) => {
      // Call the callback immediately for testing
      callback();
    })
  },
  tweens: {
    add: jest.fn()
  }
};

describe('Interactable', () => {
  let interactable: Interactable;
  
  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();
    
    // Create an interactable at grid position (10, 8)
    interactable = new Interactable(
      mockScene as any,
      'tower1',
      'tower',
      10,
      8,
      2
    );
  });
  
  describe('constructor', () => {
    test('should initialize with the correct properties', () => {
      expect(interactable.getId()).toBe('tower1');
      expect(interactable.getType()).toBe('tower');
      expect(interactable.getGridX()).toBe(10);
      expect(interactable.getGridY()).toBe(8);
      expect(interactable.getTriggerDistance()).toBe(2);
      expect(interactable.isTriggered()).toBe(false);
    });
    
    test('should initialize at the correct world position', () => {
      // For a 32px tile, the center of tile (10,8) should be at (336,272)
      expect(interactable.x).toBe(336);
      expect(interactable.y).toBe(272);
    });
    
    test('should create a pulse effect', () => {
      expect(mockScene.tweens.add).toHaveBeenCalled();
      expect(mockScene.tweens.add.mock.calls[0][0].targets).toBe(interactable);
    });
  });
  
  describe('trigger', () => {
    test('should set triggered to true', () => {
      interactable.trigger();
      expect(interactable.isTriggered()).toBe(true);
    });
    
    test('should add visual effects', () => {
      interactable.trigger();
      
      // Should add a tween
      expect(mockScene.tweens.add).toHaveBeenCalledTimes(2); // Once in constructor, once in trigger
      expect(mockScene.tweens.add.mock.calls[1][0].targets).toBe(interactable);
      
      // Should add particles
      expect(mockScene.add.particles).toHaveBeenCalledWith('tower');
      expect(mockScene.add.particles().createEmitter).toHaveBeenCalled();
      
      // Should set up a delayed call to destroy particles
      expect(mockScene.time.delayedCall).toHaveBeenCalled();
      expect(mockScene.add.particles().destroy).toHaveBeenCalled();
    });
    
    test('should do nothing if already triggered', () => {
      // Trigger once
      interactable.trigger();
      
      // Reset mocks
      jest.clearAllMocks();
      
      // Trigger again
      interactable.trigger();
      
      // Should not add effects again
      expect(mockScene.tweens.add).not.toHaveBeenCalled();
      expect(mockScene.add.particles).not.toHaveBeenCalled();
    });
  });
  
  describe('getters', () => {
    test('should return the correct values', () => {
      expect(interactable.getId()).toBe('tower1');
      expect(interactable.getType()).toBe('tower');
      expect(interactable.getGridX()).toBe(10);
      expect(interactable.getGridY()).toBe(8);
      expect(interactable.getTriggerDistance()).toBe(2);
      expect(interactable.isTriggered()).toBe(false);
    });
  });
});

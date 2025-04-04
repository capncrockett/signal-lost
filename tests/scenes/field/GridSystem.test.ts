import { GridSystem } from '../../../src/scenes/field/GridSystem';

// Mock Phaser.Scene
const mockScene = {
  add: {
    graphics: jest.fn(),
    image: jest.fn(),
    sprite: jest.fn(),
    text: jest.fn()
  },
  input: {
    keyboard: {
      createCursorKeys: jest.fn()
    }
  }
};

describe('GridSystem', () => {
  let gridSystem: GridSystem;

  beforeEach(() => {
    // Create a 10x10 grid with 32px tiles
    gridSystem = new GridSystem(mockScene as any, 10, 10, 32);
  });

  describe('isValidPosition', () => {
    test('should return true for valid positions', () => {
      expect(gridSystem.isValidPosition(0, 0)).toBe(true);
      expect(gridSystem.isValidPosition(5, 5)).toBe(true);
      expect(gridSystem.isValidPosition(9, 9)).toBe(true);
    });

    test('should return false for invalid positions', () => {
      expect(gridSystem.isValidPosition(-1, 0)).toBe(false);
      expect(gridSystem.isValidPosition(0, -1)).toBe(false);
      expect(gridSystem.isValidPosition(10, 0)).toBe(false);
      expect(gridSystem.isValidPosition(0, 10)).toBe(false);
    });
  });

  describe('isCollision', () => {
    test('should return false for positions without collision', () => {
      expect(gridSystem.isCollision(0, 0)).toBe(false);
      expect(gridSystem.isCollision(5, 5)).toBe(false);
    });

    test('should return true for positions with collision', () => {
      // Set up a collision
      gridSystem.setTileCollision(5, 5, true);

      expect(gridSystem.isCollision(5, 5)).toBe(true);
    });

    test('should return true for invalid positions', () => {
      expect(gridSystem.isCollision(-1, 0)).toBe(true);
      expect(gridSystem.isCollision(0, -1)).toBe(true);
      expect(gridSystem.isCollision(10, 0)).toBe(true);
      expect(gridSystem.isCollision(0, 10)).toBe(true);
    });
  });

  describe('setTileCollision', () => {
    test('should set collision for valid positions', () => {
      gridSystem.setTileCollision(5, 5, true);
      expect(gridSystem.isCollision(5, 5)).toBe(true);

      gridSystem.setTileCollision(5, 5, false);
      expect(gridSystem.isCollision(5, 5)).toBe(false);
    });

    test('should ignore invalid positions', () => {
      // This should not throw an error
      gridSystem.setTileCollision(-1, 0, true);
      gridSystem.setTileCollision(0, -1, true);
      gridSystem.setTileCollision(10, 0, true);
      gridSystem.setTileCollision(0, 10, true);
    });
  });

  describe('gridToWorld', () => {
    test('should convert grid coordinates to world coordinates', () => {
      // For a 32px tile, the center of tile (0,0) should be at (16,16)
      expect(gridSystem.gridToWorld(0, 0)).toEqual({ x: 16, y: 16 });

      // For a 32px tile, the center of tile (1,1) should be at (48,48)
      expect(gridSystem.gridToWorld(1, 1)).toEqual({ x: 48, y: 48 });
    });
  });

  describe('worldToGrid', () => {
    test('should convert world coordinates to grid coordinates', () => {
      // World coordinates (16,16) should be in tile (0,0)
      expect(gridSystem.worldToGrid(16, 16)).toEqual({ x: 0, y: 0 });

      // World coordinates (48,48) should be in tile (1,1)
      expect(gridSystem.worldToGrid(48, 48)).toEqual({ x: 1, y: 1 });

      // World coordinates (31,31) should still be in tile (0,0)
      expect(gridSystem.worldToGrid(31, 31)).toEqual({ x: 0, y: 0 });
    });
  });

  describe('getDistance', () => {
    test('should calculate Manhattan distance between two positions', () => {
      expect(gridSystem.getDistance(0, 0, 0, 0)).toBe(0);
      expect(gridSystem.getDistance(0, 0, 3, 0)).toBe(3);
      expect(gridSystem.getDistance(0, 0, 0, 3)).toBe(3);
      expect(gridSystem.getDistance(0, 0, 3, 4)).toBe(7);
      expect(gridSystem.getDistance(2, 3, 5, 7)).toBe(7);
    });
  });

  describe('findPath', () => {
    test('should find a path between two positions', () => {
      const path = gridSystem.findPath(0, 0, 2, 2);

      // Path should exist
      expect(path).not.toBeNull();

      // Path should end at (2,2)
      if (path) {
        expect(path.length).toBeGreaterThan(0);

        // Check last step
        const lastStep = path[path.length - 1];
        expect(lastStep).toEqual({ x: 2, y: 2 });
      }
    });

    test('should return null for invalid positions', () => {
      expect(gridSystem.findPath(-1, 0, 2, 2)).toBeNull();
      expect(gridSystem.findPath(0, -1, 2, 2)).toBeNull();
      expect(gridSystem.findPath(0, 0, -1, 2)).toBeNull();
      expect(gridSystem.findPath(0, 0, 2, -1)).toBeNull();
      expect(gridSystem.findPath(10, 0, 2, 2)).toBeNull();
      expect(gridSystem.findPath(0, 10, 2, 2)).toBeNull();
      expect(gridSystem.findPath(0, 0, 10, 2)).toBeNull();
      expect(gridSystem.findPath(0, 0, 2, 10)).toBeNull();
    });

    test('should return null for positions with collision', () => {
      // Set up collisions to block all paths
      gridSystem.setTileCollision(1, 0, true);
      gridSystem.setTileCollision(0, 1, true);

      expect(gridSystem.findPath(0, 0, 2, 2)).toBeNull();
    });

    test('should find a path around obstacles', () => {
      // Set up some obstacles
      gridSystem.setTileCollision(1, 1, true);
      gridSystem.setTileCollision(2, 1, true);

      const path = gridSystem.findPath(0, 0, 3, 3);

      // Path should exist
      expect(path).not.toBeNull();

      // Path should go around the obstacles
      if (path) {
        // Check that the path doesn't go through obstacles
        for (const step of path) {
          expect(gridSystem.isCollision(step.x, step.y)).toBe(false);
        }
      }
    });
  });

  describe('getters', () => {
    test('should return grid dimensions', () => {
      expect(gridSystem.getWidth()).toBe(10);
      expect(gridSystem.getHeight()).toBe(10);
    });

    test('should return tile size', () => {
      expect(gridSystem.getTileSize()).toBe(32);
    });

    test('should return collision grid', () => {
      // Set up some collisions
      gridSystem.setTileCollision(1, 1, true);
      gridSystem.setTileCollision(2, 2, true);

      const collisionGrid = gridSystem.getCollisionGrid();

      // Check dimensions
      expect(collisionGrid.length).toBe(10);
      expect(collisionGrid[0].length).toBe(10);

      // Check collisions
      expect(collisionGrid[1][1]).toBe(true);
      expect(collisionGrid[2][2]).toBe(true);
      expect(collisionGrid[0][0]).toBe(false);
    });

    test('should return a copy of the collision grid', () => {
      // Get the collision grid
      const collisionGrid = gridSystem.getCollisionGrid();

      // Modify the copy
      collisionGrid[0][0] = true;

      // The original should not be modified
      expect(gridSystem.isCollision(0, 0)).toBe(false);
    });
  });
});

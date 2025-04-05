import Phaser from 'phaser';

/**
 * GridSystem
 *
 * Manages a grid-based system for movement and collision detection
 */
export class GridSystem {
  // Reference to the scene
  private scene: Phaser.Scene;

  // Grid dimensions
  private width: number;
  private height: number;

  // Tile size in pixels
  private tileSize: number;

  // Collision grid (true = collision, false = no collision)
  private collisionGrid: boolean[][];

  /**
   * Create a new grid system
   * @param scene Reference to the scene
   * @param width Grid width in tiles
   * @param height Grid height in tiles
   * @param tileSize Tile size in pixels
   */
  constructor(scene: Phaser.Scene, width: number, height: number, tileSize: number) {
    this.scene = scene;
    this.width = width;
    this.height = height;
    this.tileSize = tileSize;

    // Initialize collision grid
    this.collisionGrid = Array(height)
      .fill(null)
      .map(() => Array(width).fill(false));
  }

  /**
   * Check if a position is valid (within grid bounds)
   * @param x Grid X coordinate
   * @param y Grid Y coordinate
   * @returns True if the position is valid
   */
  isValidPosition(x: number, y: number): boolean {
    return x >= 0 && x < this.width && y >= 0 && y < this.height;
  }

  /**
   * Check if a position has a collision
   * @param x Grid X coordinate
   * @param y Grid Y coordinate
   * @returns True if the position has a collision
   */
  isCollision(x: number, y: number): boolean {
    // Check if position is valid
    if (!this.isValidPosition(x, y)) {
      return true; // Treat out-of-bounds as collision
    }

    return this.collisionGrid[y][x];
  }

  /**
   * Set collision for a tile
   * @param x Grid X coordinate
   * @param y Grid Y coordinate
   * @param collision True to set collision, false to clear collision
   */
  setTileCollision(x: number, y: number, collision: boolean): void {
    // Check if position is valid
    if (!this.isValidPosition(x, y)) {
      return;
    }

    this.collisionGrid[y][x] = collision;
  }

  /**
   * Convert grid coordinates to world coordinates
   * @param x Grid X coordinate
   * @param y Grid Y coordinate
   * @returns World coordinates {x, y}
   */
  gridToWorld(x: number, y: number): { x: number; y: number } {
    return {
      x: x * this.tileSize + this.tileSize / 2,
      y: y * this.tileSize + this.tileSize / 2,
    };
  }

  /**
   * Convert world coordinates to grid coordinates
   * @param x World X coordinate
   * @param y World Y coordinate
   * @returns Grid coordinates {x, y}
   */
  worldToGrid(x: number, y: number): { x: number; y: number } {
    return {
      x: Math.floor(x / this.tileSize),
      y: Math.floor(y / this.tileSize),
    };
  }

  /**
   * Get the distance between two grid positions
   * @param x1 First position X
   * @param y1 First position Y
   * @param x2 Second position X
   * @param y2 Second position Y
   * @returns Manhattan distance between the positions
   */
  getDistance(x1: number, y1: number, x2: number, y2: number): number {
    return Math.abs(x1 - x2) + Math.abs(y1 - y2);
  }

  /**
   * Find a path between two grid positions
   * @param startX Start position X
   * @param startY Start position Y
   * @param endX End position X
   * @param endY End position Y
   * @returns Array of positions {x, y} or null if no path found
   */
  findPath(
    startX: number,
    startY: number,
    endX: number,
    endY: number
  ): { x: number; y: number }[] | null {
    // Check if start and end positions are valid
    if (!this.isValidPosition(startX, startY) || !this.isValidPosition(endX, endY)) {
      return null;
    }

    // Check if end position has collision
    if (this.isCollision(endX, endY)) {
      return null;
    }

    // Simple implementation of A* pathfinding
    const openSet: {
      x: number;
      y: number;
      g: number;
      h: number;
      f: number;
      parent: { x: number; y: number } | null;
    }[] = [];
    const closedSet: Set<string> = new Set();

    // Add start position to open set
    openSet.push({
      x: startX,
      y: startY,
      g: 0,
      h: this.getDistance(startX, startY, endX, endY),
      f: this.getDistance(startX, startY, endX, endY),
      parent: null,
    });

    // Loop until open set is empty
    while (openSet.length > 0) {
      // Sort open set by f value
      openSet.sort((a, b) => a.f - b.f);

      // Get the node with the lowest f value
      const current = openSet.shift()!;

      // Check if we reached the end
      if (current.x === endX && current.y === endY) {
        // Reconstruct path
        const path: { x: number; y: number }[] = [];
        let node = current;

        while (node.parent) {
          path.unshift({ x: node.x, y: node.y });
          // Define the type for the node
          type GridNode = {
            x: number;
            y: number;
            parent: GridNode | null;
            f?: number;
            g?: number;
            h?: number;
          };

          // Create a new node without circular reference
          node = { ...node.parent, parent: null } as GridNode;
        }

        return path;
      }

      // Add current node to closed set
      closedSet.add(`${current.x},${current.y}`);

      // Check neighbors
      const neighbors = [
        { x: current.x - 1, y: current.y },
        { x: current.x + 1, y: current.y },
        { x: current.x, y: current.y - 1 },
        { x: current.x, y: current.y + 1 },
      ];

      for (const neighbor of neighbors) {
        // Skip if not valid or has collision
        if (
          !this.isValidPosition(neighbor.x, neighbor.y) ||
          this.isCollision(neighbor.x, neighbor.y)
        ) {
          continue;
        }

        // Skip if in closed set
        if (closedSet.has(`${neighbor.x},${neighbor.y}`)) {
          continue;
        }

        // Calculate g, h, and f values
        const g = current.g + 1;
        const h = this.getDistance(neighbor.x, neighbor.y, endX, endY);
        const f = g + h;

        // Check if neighbor is already in open set
        const existingNeighbor = openSet.find(
          (node) => node.x === neighbor.x && node.y === neighbor.y
        );

        if (existingNeighbor) {
          // Update if new path is better
          if (g < existingNeighbor.g) {
            existingNeighbor.g = g;
            existingNeighbor.f = g + existingNeighbor.h;
            existingNeighbor.parent = { x: current.x, y: current.y };
          }
        } else {
          // Add to open set
          openSet.push({
            x: neighbor.x,
            y: neighbor.y,
            g,
            h,
            f,
            parent: { x: current.x, y: current.y },
          });
        }
      }
    }

    // No path found
    return null;
  }

  /**
   * Get the grid width
   */
  getWidth(): number {
    return this.width;
  }

  /**
   * Get the grid height
   */
  getHeight(): number {
    return this.height;
  }

  /**
   * Get the tile size
   */
  getTileSize(): number {
    return this.tileSize;
  }

  /**
   * Get the collision grid
   */
  getCollisionGrid(): boolean[][] {
    return this.collisionGrid.map((row) => [...row]);
  }
}

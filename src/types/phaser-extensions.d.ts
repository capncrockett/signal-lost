/**
 * Extensions to Phaser types
 */

declare namespace Phaser {
  namespace Tilemaps {
    interface Tile {
      properties: {
        collides?: boolean;
        [key: string]: unknown;
      };
    }
  }
}

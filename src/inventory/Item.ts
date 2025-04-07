import Phaser from 'phaser';

/**
 * Item types
 */
export enum ItemType {
  TOOL = 'tool',
  DOCUMENT = 'document',
  KEY = 'key',
  RADIO_PART = 'radio_part',
  CONSUMABLE = 'consumable',
}

/**
 * Interface for item data
 */
export interface ItemData {
  id: string;
  name: string;
  description: string;
  type: ItemType;
  icon: string;
  usable: boolean;
  stackable: boolean;
  maxStack?: number;
  effects?: Record<string, unknown>;
}

/**
 * Item class
 *
 * Represents an item in the game that can be collected and used
 */
export class Item {
  private id: string;
  private name: string;
  private description: string;
  private type: ItemType;
  private icon: string;
  private usable: boolean;
  private stackable: boolean;
  private maxStack: number;
  private quantity: number;
  private effects: Record<string, unknown>;

  /**
   * Create a new item
   * @param data Item data
   * @param quantity Initial quantity
   */
  constructor(data: ItemData, quantity: number = 1) {
    this.id = data.id;
    this.name = data.name;
    this.description = data.description;
    this.type = data.type;
    this.icon = data.icon;
    this.usable = data.usable;
    this.stackable = data.stackable;
    this.maxStack = data.maxStack || 99;
    this.quantity = Math.min(quantity, this.maxStack);
    this.effects = data.effects || {};
  }

  /**
   * Get the item ID
   */
  getId(): string {
    return this.id;
  }

  /**
   * Get the item name
   */
  getName(): string {
    return this.name;
  }

  /**
   * Get the item description
   */
  getDescription(): string {
    return this.description;
  }

  /**
   * Get the item type
   */
  getType(): ItemType {
    return this.type;
  }

  /**
   * Get the item icon
   */
  getIcon(): string {
    return this.icon;
  }

  /**
   * Check if the item is usable
   */
  isUsable(): boolean {
    return this.usable;
  }

  /**
   * Check if the item is stackable
   */
  isStackable(): boolean {
    return this.stackable;
  }

  /**
   * Get the maximum stack size
   */
  getMaxStack(): number {
    return this.maxStack;
  }

  /**
   * Get the current quantity
   */
  getQuantity(): number {
    return this.quantity;
  }

  /**
   * Set the quantity
   * @param quantity New quantity
   * @returns Actual quantity set (may be clamped to max stack)
   */
  setQuantity(quantity: number): number {
    this.quantity = Math.min(Math.max(0, quantity), this.maxStack);
    return this.quantity;
  }

  /**
   * Add to the quantity
   * @param amount Amount to add
   * @returns New quantity
   */
  addQuantity(amount: number): number {
    return this.setQuantity(this.quantity + amount);
  }

  /**
   * Remove from the quantity
   * @param amount Amount to remove
   * @returns New quantity
   */
  removeQuantity(amount: number): number {
    return this.setQuantity(this.quantity - amount);
  }

  /**
   * Get the item effects
   */
  getEffects(): Record<string, unknown> {
    return { ...this.effects };
  }

  /**
   * Get a specific effect
   * @param key Effect key
   * @returns Effect value or undefined if not found
   */
  getEffect(key: string): unknown {
    return this.effects[key];
  }

  /**
   * Create a sprite for this item
   * @param scene Scene to add the sprite to
   * @param x X position
   * @param y Y position
   * @returns Sprite object
   */
  createSprite(scene: Phaser.Scene, x: number, y: number): Phaser.GameObjects.Sprite {
    const sprite = scene.add.sprite(x, y, this.icon);

    // Add quantity text if stackable and quantity > 1
    if (this.stackable && this.quantity > 1) {
      const quantityText = scene.add.text(x + 10, y + 10, this.quantity.toString(), {
        fontSize: '12px',
        color: '#ffffff',
        backgroundColor: '#000000',
        padding: { x: 2, y: 2 },
      });
      quantityText.setOrigin(1, 1);

      // Group the sprite and text
      const container = scene.add.container(x, y, [sprite, quantityText]);
      container.setSize(sprite.width, sprite.height);

      // Return the sprite for consistency
      return sprite;
    }

    return sprite;
  }

  /**
   * Convert the item to a serializable object
   */
  toJSON(): Record<string, unknown> {
    return {
      id: this.id,
      quantity: this.quantity,
    };
  }

  /**
   * Create an item from a serialized object
   * @param data Serialized item data
   * @param itemsData Item definitions
   * @returns New item instance or null if not found
   */
  static fromJSON(data: Record<string, unknown>, itemsData: ItemData[]): Item | null {
    // Find the item definition
    const itemData = itemsData.find((item) => item.id === data.id);
    if (!itemData) {
      console.error(`Item definition not found for ID: ${data.id}`);
      return null;
    }

    // Create a new item with the saved quantity
    const quantity = typeof data.quantity === 'number' ? data.quantity : 1;
    return new Item(itemData, quantity);
  }
}

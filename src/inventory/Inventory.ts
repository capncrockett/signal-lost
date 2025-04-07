import Phaser from 'phaser';
import { Item, ItemData, ItemType } from './Item';
import { SaveManager } from '../utils/SaveManager';

/**
 * Inventory class
 *
 * Manages the player's inventory of items
 */
export class Inventory {
  // Event emitter for inventory events
  private eventEmitter: Phaser.Events.EventEmitter;

  // Items in the inventory
  private items: Item[] = [];

  // Maximum number of items in the inventory
  private maxItems: number;

  // Item definitions
  private itemsData: ItemData[] = [];

  /**
   * Create a new inventory
   * @param maxItems Maximum number of items in the inventory
   */
  constructor(maxItems: number = 20) {
    this.eventEmitter = new Phaser.Events.EventEmitter();
    this.maxItems = maxItems;
  }

  /**
   * Load item definitions
   * @param itemsData Array of item definitions
   */
  loadItemDefinitions(itemsData: ItemData[]): void {
    this.itemsData = itemsData;
  }

  /**
   * Add an item to the inventory
   * @param item Item to add
   * @returns True if the item was added successfully
   */
  addItem(item: Item): boolean {
    // Check if inventory is full
    if (this.items.length >= this.maxItems && !this.canStack(item)) {
      this.eventEmitter.emit('inventoryFull', item);
      return false;
    }

    // Check if the item can be stacked with an existing item
    if (item.isStackable()) {
      const existingItem = this.findItemById(item.getId());
      if (existingItem) {
        // Add to existing stack
        existingItem.addQuantity(item.getQuantity());

        // Emit event
        this.eventEmitter.emit('itemUpdated', existingItem);

        // Save inventory state
        this.saveInventory();

        return true;
      }
    }

    // Add as a new item
    this.items.push(item);

    // Emit event
    this.eventEmitter.emit('itemAdded', item);

    // Save inventory state
    this.saveInventory();

    return true;
  }

  /**
   * Remove an item from the inventory
   * @param index Index of the item to remove
   * @param quantity Quantity to remove (default: all)
   * @returns True if the item was removed successfully
   */
  removeItem(index: number, quantity?: number): boolean {
    // Check if index is valid
    if (index < 0 || index >= this.items.length) {
      return false;
    }

    const item = this.items[index];

    // If quantity is specified and the item is stackable
    if (quantity !== undefined && item.isStackable()) {
      // Remove the specified quantity
      const newQuantity = item.removeQuantity(quantity);

      // If the quantity is now 0, remove the item
      if (newQuantity <= 0) {
        this.items.splice(index, 1);
        this.eventEmitter.emit('itemRemoved', item);
      } else {
        this.eventEmitter.emit('itemUpdated', item);
      }
    } else {
      // Remove the entire item
      this.items.splice(index, 1);
      this.eventEmitter.emit('itemRemoved', item);
    }

    // Save inventory state
    this.saveInventory();

    return true;
  }

  /**
   * Use an item
   * @param index Index of the item to use
   * @returns True if the item was used successfully
   */
  useItem(index: number): boolean {
    // Check if index is valid
    if (index < 0 || index >= this.items.length) {
      return false;
    }

    const item = this.items[index];

    // Check if the item is usable
    if (!item.isUsable()) {
      return false;
    }

    // Emit event
    this.eventEmitter.emit('itemUsed', item);

    // If the item is consumable, reduce quantity
    if (item.getType() === ItemType.CONSUMABLE) {
      const newQuantity = item.removeQuantity(1);

      // If the quantity is now 0, remove the item
      if (newQuantity <= 0) {
        this.items.splice(index, 1);
        this.eventEmitter.emit('itemRemoved', item);
      } else {
        this.eventEmitter.emit('itemUpdated', item);
      }

      // Save inventory state
      this.saveInventory();
    }

    return true;
  }

  /**
   * Get all items in the inventory
   */
  getItems(): Item[] {
    return [...this.items];
  }

  /**
   * Get the number of items in the inventory
   */
  getItemCount(): number {
    return this.items.length;
  }

  /**
   * Get the maximum number of items in the inventory
   */
  getMaxItems(): number {
    return this.maxItems;
  }

  /**
   * Check if the inventory is full
   */
  isFull(): boolean {
    return this.items.length >= this.maxItems;
  }

  /**
   * Check if an item can be stacked with existing items
   * @param item Item to check
   * @returns True if the item can be stacked
   */
  private canStack(item: Item): boolean {
    if (!item.isStackable()) {
      return false;
    }

    const existingItem = this.findItemById(item.getId());
    if (!existingItem) {
      return false;
    }

    return existingItem.getQuantity() < existingItem.getMaxStack();
  }

  /**
   * Find an item by ID
   * @param id Item ID
   * @returns Item or undefined if not found
   */
  findItemById(id: string): Item | undefined {
    return this.items.find((item) => item.getId() === id);
  }

  /**
   * Find items by type
   * @param type Item type
   * @returns Array of items of the specified type
   */
  findItemsByType(type: ItemType): Item[] {
    return this.items.filter((item) => item.getType() === type);
  }

  /**
   * Check if the inventory contains an item
   * @param id Item ID
   * @returns True if the inventory contains the item
   */
  hasItem(id: string): boolean {
    return this.findItemById(id) !== undefined;
  }

  /**
   * Get the quantity of an item
   * @param id Item ID
   * @returns Quantity of the item or 0 if not found
   */
  getItemQuantity(id: string): number {
    const item = this.findItemById(id);
    return item ? item.getQuantity() : 0;
  }

  /**
   * Clear the inventory
   */
  clear(): void {
    this.items = [];
    this.eventEmitter.emit('inventoryCleared');

    // Save inventory state
    this.saveInventory();
  }

  /**
   * Save the inventory state
   */
  saveInventory(): void {
    // Convert items to JSON
    const itemsData = this.items.map((item) => item.toJSON());

    // Save to game state
    SaveManager.setData('inventory', itemsData);
  }

  /**
   * Load the inventory state
   * @returns True if the inventory was loaded successfully
   */
  loadInventory(): boolean {
    // Clear current inventory
    this.items = [];

    // Get saved inventory data
    const savedData = SaveManager.getData('inventory');
    if (!savedData || !Array.isArray(savedData)) {
      return false;
    }

    // Convert JSON to items
    for (const itemData of savedData) {
      if (typeof itemData === 'object' && itemData !== null) {
        const item = Item.fromJSON(itemData as Record<string, unknown>, this.itemsData);
        if (item) {
          this.items.push(item);
        }
      }
    }

    // Emit event
    this.eventEmitter.emit('inventoryLoaded');

    return true;
  }

  /**
   * Add an event listener
   * @param event Event name
   * @param fn Callback function
   * @param context Context for the callback
   */
  on(event: string, fn: (...args: unknown[]) => void, context?: unknown): void {
    this.eventEmitter.on(event, fn, context);
  }

  /**
   * Remove an event listener
   * @param event Event name
   * @param fn Callback function
   * @param context Context for the callback
   */
  off(event: string, fn?: (...args: unknown[]) => void, context?: unknown): void {
    this.eventEmitter.off(event, fn, context);
  }

  /**
   * Remove all event listeners
   */
  removeAllListeners(): void {
    this.eventEmitter.removeAllListeners();
  }
}

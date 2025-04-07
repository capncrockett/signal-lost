import Phaser from 'phaser';
import { Inventory } from './Inventory';

/**
 * Configuration for the inventory UI
 */
interface InventoryUIConfig {
  x: number;
  y: number;
  width: number;
  height: number;
  columns: number;
  padding: number;
  backgroundColor: number;
  backgroundAlpha: number;
  slotColor: number;
  slotAlpha: number;
  slotSize: number;
  slotSpacing: number;
  selectedSlotColor: number;
  selectedSlotAlpha: number;
}

/**
 * InventoryUI class
 *
 * UI component for displaying and interacting with the inventory
 */
export class InventoryUI extends Phaser.GameObjects.Container {
  // Reference to the inventory
  private inventory: Inventory;

  // UI configuration
  private config: InventoryUIConfig;

  // UI elements
  private background!: Phaser.GameObjects.Rectangle;
  private slots: Phaser.GameObjects.Rectangle[] = [];
  private itemSprites: Phaser.GameObjects.Sprite[] = [];
  private itemTexts: Phaser.GameObjects.Text[] = [];
  private titleText!: Phaser.GameObjects.Text;
  private descriptionText!: Phaser.GameObjects.Text;
  private closeButton!: Phaser.GameObjects.Text;
  private useButton!: Phaser.GameObjects.Text;
  private dropButton!: Phaser.GameObjects.Text;

  // Selected slot index
  private selectedSlot: number = -1;

  // Visibility state
  private isVisible: boolean = false;

  /**
   * Create a new inventory UI
   * @param scene Reference to the scene
   * @param inventory Inventory to display
   * @param config UI configuration
   */
  constructor(
    scene: Phaser.Scene,
    inventory: Inventory,
    config?: Partial<InventoryUIConfig>
  ) {
    // Default position
    const x = config?.x ?? 400;
    const y = config?.y ?? 300;

    super(scene, x, y);

    this.inventory = inventory;

    // Default configuration
    this.config = {
      x,
      y,
      width: 600,
      height: 400,
      columns: 5,
      padding: 20,
      backgroundColor: 0x000000,
      backgroundAlpha: 0.8,
      slotColor: 0x333333,
      slotAlpha: 0.8,
      slotSize: 64,
      slotSpacing: 10,
      selectedSlotColor: 0x666666,
      selectedSlotAlpha: 0.8,
      ...config
    };

    // Create UI elements
    this.createBackground();
    this.createTitle();
    this.createCloseButton();
    this.createSlots();
    this.createItemInfo();
    this.createActionButtons();

    // Set up event listeners
    this.setupEventListeners();

    // Add to scene
    scene.add.existing(this);

    // Hide initially
    this.setVisible(false);
    this.isVisible = false;
  }

  /**
   * Create the background
   */
  private createBackground(): void {
    this.background = this.scene.add.rectangle(
      0,
      0,
      this.config.width,
      this.config.height,
      this.config.backgroundColor,
      this.config.backgroundAlpha
    );
    this.add(this.background);
  }

  /**
   * Create the title
   */
  private createTitle(): void {
    this.titleText = this.scene.add.text(
      0,
      -this.config.height / 2 + this.config.padding,
      'Inventory',
      {
        fontSize: '24px',
        color: '#ffffff',
        fontStyle: 'bold'
      }
    );
    this.titleText.setOrigin(0.5, 0);
    this.add(this.titleText);
  }

  /**
   * Create the close button
   */
  private createCloseButton(): void {
    this.closeButton = this.scene.add.text(
      this.config.width / 2 - this.config.padding,
      -this.config.height / 2 + this.config.padding,
      'X',
      {
        fontSize: '24px',
        color: '#ff0000',
        fontStyle: 'bold'
      }
    );
    this.closeButton.setOrigin(1, 0);
    this.closeButton.setInteractive({ useHandCursor: true });
    this.closeButton.on('pointerdown', () => {
      this.hide();
    });
    this.add(this.closeButton);
  }

  /**
   * Create inventory slots
   */
  private createSlots(): void {
    const { columns, slotSize, slotSpacing } = this.config;
    const rows = Math.ceil(this.inventory.getMaxItems() / columns);

    // Calculate total width and height of the slots grid
    const gridWidth = columns * slotSize + (columns - 1) * slotSpacing;
    const gridHeight = rows * slotSize + (rows - 1) * slotSpacing;

    // Calculate starting position (top-left of the grid)
    const startX = -gridWidth / 2;
    const startY = -gridHeight / 2 + 50; // Offset for title

    // Create slots
    for (let i = 0; i < this.inventory.getMaxItems(); i++) {
      const row = Math.floor(i / columns);
      const col = i % columns;

      const x = startX + col * (slotSize + slotSpacing) + slotSize / 2;
      const y = startY + row * (slotSize + slotSpacing) + slotSize / 2;

      // Create slot background
      const slot = this.scene.add.rectangle(
        x,
        y,
        slotSize,
        slotSize,
        this.config.slotColor,
        this.config.slotAlpha
      );
      slot.setStrokeStyle(2, 0xffffff, 0.5);
      slot.setInteractive({ useHandCursor: true });

      // Add click handler
      slot.on('pointerdown', () => {
        this.selectSlot(i);
      });

      this.slots.push(slot);
      this.add(slot);

      // Create placeholder for item sprite
      const sprite = this.scene.add.sprite(x, y, '');
      sprite.setVisible(false);
      this.itemSprites.push(sprite);
      this.add(sprite);

      // Create placeholder for item quantity text
      const text = this.scene.add.text(
        x + slotSize / 2 - 5,
        y + slotSize / 2 - 5,
        '',
        {
          fontSize: '14px',
          color: '#ffffff',
          backgroundColor: '#000000',
          padding: { x: 2, y: 2 }
        }
      );
      text.setOrigin(1, 1);
      text.setVisible(false);
      this.itemTexts.push(text);
      this.add(text);
    }
  }

  /**
   * Create item information display
   */
  private createItemInfo(): void {
    // Create description text
    this.descriptionText = this.scene.add.text(
      0,
      this.config.height / 2 - this.config.padding - 60,
      '',
      {
        fontSize: '16px',
        color: '#ffffff',
        wordWrap: { width: this.config.width - this.config.padding * 2 }
      }
    );
    this.descriptionText.setOrigin(0.5, 1);
    this.add(this.descriptionText);
  }

  /**
   * Create action buttons
   */
  private createActionButtons(): void {
    // Use button
    this.useButton = this.scene.add.text(
      -80,
      this.config.height / 2 - this.config.padding - 20,
      'Use',
      {
        fontSize: '18px',
        color: '#ffffff',
        backgroundColor: '#006600',
        padding: { x: 10, y: 5 }
      }
    );
    this.useButton.setOrigin(0.5, 0.5);
    this.useButton.setInteractive({ useHandCursor: true });
    this.useButton.on('pointerdown', () => {
      this.useSelectedItem();
    });
    this.useButton.setVisible(false);
    this.add(this.useButton);

    // Drop button
    this.dropButton = this.scene.add.text(
      80,
      this.config.height / 2 - this.config.padding - 20,
      'Drop',
      {
        fontSize: '18px',
        color: '#ffffff',
        backgroundColor: '#660000',
        padding: { x: 10, y: 5 }
      }
    );
    this.dropButton.setOrigin(0.5, 0.5);
    this.dropButton.setInteractive({ useHandCursor: true });
    this.dropButton.on('pointerdown', () => {
      this.dropSelectedItem();
    });
    this.dropButton.setVisible(false);
    this.add(this.dropButton);
  }

  /**
   * Set up event listeners
   */
  private setupEventListeners(): void {
    // Listen for inventory events
    const refreshItems = (): void => this.refreshItems();
    this.inventory.on('itemAdded', refreshItems, this);
    this.inventory.on('itemRemoved', refreshItems, this);
    this.inventory.on('itemUpdated', refreshItems, this);
    this.inventory.on('inventoryLoaded', refreshItems, this);
    this.inventory.on('inventoryCleared', refreshItems, this);

    // Listen for keyboard events
    this.scene.input.keyboard?.on('keydown-I', () => {
      this.toggle();
    });

    this.scene.input.keyboard?.on('keydown-ESC', () => {
      if (this.isVisible) {
        this.hide();
      }
    });
  }

  /**
   * Refresh the displayed items
   */
  refreshItems(): void {
    const items = this.inventory.getItems();

    // Reset all slots
    for (let i = 0; i < this.inventory.getMaxItems(); i++) {
      // Hide item sprite and text
      this.itemSprites[i].setVisible(false);
      this.itemTexts[i].setVisible(false);

      // Reset slot appearance
      this.slots[i].setFillStyle(
        this.config.slotColor,
        this.config.slotAlpha
      );
    }

    // Update slots with items
    for (let i = 0; i < items.length; i++) {
      const item = items[i];

      // Set item sprite
      this.itemSprites[i].setTexture(item.getIcon());
      this.itemSprites[i].setVisible(true);

      // Set quantity text for stackable items
      if (item.isStackable() && item.getQuantity() > 1) {
        this.itemTexts[i].setText(item.getQuantity().toString());
        this.itemTexts[i].setVisible(true);
      }
    }

    // Update selected slot
    if (this.selectedSlot >= 0 && this.selectedSlot < items.length) {
      this.selectSlot(this.selectedSlot);
    } else {
      this.clearSelection();
    }
  }

  /**
   * Select an inventory slot
   * @param index Slot index
   */
  selectSlot(index: number): void {
    // Clear previous selection
    if (this.selectedSlot >= 0 && this.selectedSlot < this.slots.length) {
      this.slots[this.selectedSlot].setFillStyle(
        this.config.slotColor,
        this.config.slotAlpha
      );
    }

    // Set new selection
    this.selectedSlot = index;

    // Get the item at this slot
    const items = this.inventory.getItems();
    const item = items[index];

    if (item) {
      // Highlight the slot
      this.slots[index].setFillStyle(
        this.config.selectedSlotColor,
        this.config.selectedSlotAlpha
      );

      // Update description
      this.descriptionText.setText(`${item.getName()}\n\n${item.getDescription()}`);

      // Show action buttons
      this.useButton.setVisible(item.isUsable());
      this.dropButton.setVisible(true);
    } else {
      this.clearSelection();
    }
  }

  /**
   * Clear the current selection
   */
  clearSelection(): void {
    this.selectedSlot = -1;
    this.descriptionText.setText('');
    this.useButton.setVisible(false);
    this.dropButton.setVisible(false);
  }

  /**
   * Use the selected item
   */
  useSelectedItem(): void {
    if (this.selectedSlot >= 0) {
      this.inventory.useItem(this.selectedSlot);
    }
  }

  /**
   * Drop the selected item
   */
  dropSelectedItem(): void {
    if (this.selectedSlot >= 0) {
      this.inventory.removeItem(this.selectedSlot);
    }
  }

  /**
   * Show the inventory UI
   */
  show(): void {
    if (this.isVisible) return;

    this.setVisible(true);
    this.isVisible = true;

    // Refresh items
    this.refreshItems();
  }

  /**
   * Hide the inventory UI
   */
  hide(): void {
    if (!this.isVisible) return;

    this.setVisible(false);
    this.isVisible = false;

    // Clear selection
    this.clearSelection();
  }

  /**
   * Toggle the inventory UI visibility
   */
  toggle(): void {
    if (this.isVisible) {
      this.hide();
    } else {
      this.show();
    }
  }

  /**
   * Check if the inventory UI is visible
   */
  getIsVisible(): boolean {
    return this.isVisible;
  }

  /**
   * Clean up resources
   */
  destroy(fromScene?: boolean): void {
    // Remove event listeners
    const refreshItems = (): void => this.refreshItems();
    this.inventory.off('itemAdded', refreshItems, this);
    this.inventory.off('itemRemoved', refreshItems, this);
    this.inventory.off('itemUpdated', refreshItems, this);
    this.inventory.off('inventoryLoaded', refreshItems, this);
    this.inventory.off('inventoryCleared', refreshItems, this);

    super.destroy(fromScene);
  }
}

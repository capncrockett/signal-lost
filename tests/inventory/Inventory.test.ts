// Mock Phaser before importing Inventory
// eslint-disable-next-line @typescript-eslint/no-var-requires
jest.mock('phaser', () => require('../mocks/PhaserMock').default);

// Mock SaveManager
jest.mock('../../src/utils/SaveManager', () => ({
  SaveManager: {
    setData: jest.fn(),
    getData: jest.fn(),
    setFlag: jest.fn(),
    getFlag: jest.fn(),
  },
}));

// Import after mocking
import { Inventory } from '../../src/inventory/Inventory';
import { Item, ItemData, ItemType } from '../../src/inventory/Item';
import { SaveManager } from '../../src/utils/SaveManager';

describe('Inventory', () => {
  let inventory: Inventory;
  let mockItemData: ItemData;
  let mockItem: Item;

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Create inventory
    inventory = new Inventory(10);

    // Create mock item data
    mockItemData = {
      id: 'test_item',
      name: 'Test Item',
      description: 'A test item',
      type: ItemType.TOOL,
      icon: 'test_icon',
      usable: true,
      stackable: false,
    };

    // Create mock item
    mockItem = new Item(mockItemData);

    // Mock event emitter methods
    inventory['eventEmitter'] = {
      on: jest.fn(),
      off: jest.fn(),
      emit: jest.fn(),
      removeAllListeners: jest.fn(),
    };
  });

  describe('addItem', () => {
    test('should add an item to the inventory', () => {
      const result = inventory.addItem(mockItem);

      expect(result).toBe(true);
      expect(inventory.getItems().length).toBe(1);
      expect(inventory.getItems()[0]).toBe(mockItem);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemAdded', mockItem);
      expect(SaveManager.setData).toHaveBeenCalled();
    });

    test('should not add an item if inventory is full', () => {
      // Fill inventory
      for (let i = 0; i < 10; i++) {
        inventory.addItem(new Item(mockItemData));
      }

      // Try to add one more
      const result = inventory.addItem(mockItem);

      expect(result).toBe(false);
      expect(inventory.getItems().length).toBe(10);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('inventoryFull', mockItem);
    });

    test('should stack items if they are stackable', () => {
      // Create stackable item
      const stackableItemData: ItemData = {
        ...mockItemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem1 = new Item(stackableItemData, 2);
      const stackableItem2 = new Item(stackableItemData, 2);

      // Add first item
      inventory.addItem(stackableItem1);

      // Add second item (should stack)
      const result = inventory.addItem(stackableItem2);

      expect(result).toBe(true);
      expect(inventory.getItems().length).toBe(1);
      expect(inventory.getItems()[0].getQuantity()).toBe(4);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemUpdated', stackableItem1);
    });
  });

  describe('removeItem', () => {
    test('should remove an item from the inventory', () => {
      // Add item
      inventory.addItem(mockItem);

      // Remove item
      const result = inventory.removeItem(0);

      expect(result).toBe(true);
      expect(inventory.getItems().length).toBe(0);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemRemoved', mockItem);
      expect(SaveManager.setData).toHaveBeenCalled();
    });

    test('should remove a specific quantity from a stackable item', () => {
      // Create stackable item
      const stackableItemData: ItemData = {
        ...mockItemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      // Add item
      inventory.addItem(stackableItem);

      // Remove 2 from the stack
      const result = inventory.removeItem(0, 2);

      expect(result).toBe(true);
      expect(inventory.getItems().length).toBe(1);
      expect(inventory.getItems()[0].getQuantity()).toBe(1);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemUpdated', stackableItem);
    });

    test('should remove the item entirely if quantity reaches 0', () => {
      // Create stackable item
      const stackableItemData: ItemData = {
        ...mockItemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      // Add item
      inventory.addItem(stackableItem);

      // Remove 3 from the stack (all of it)
      const result = inventory.removeItem(0, 3);

      expect(result).toBe(true);
      expect(inventory.getItems().length).toBe(0);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemRemoved', stackableItem);
    });

    test('should return false if index is invalid', () => {
      const result = inventory.removeItem(0);

      expect(result).toBe(false);
      expect(inventory['eventEmitter'].emit).not.toHaveBeenCalled();
    });
  });

  describe('useItem', () => {
    test('should use an item', () => {
      // Add item
      inventory.addItem(mockItem);

      // Use item
      const result = inventory.useItem(0);

      expect(result).toBe(true);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemUsed', mockItem);
    });

    test('should consume a consumable item', () => {
      // Create consumable item
      const consumableItemData: ItemData = {
        ...mockItemData,
        type: ItemType.CONSUMABLE,
        stackable: true,
        maxStack: 5,
      };
      const consumableItem = new Item(consumableItemData, 3);

      // Add item
      inventory.addItem(consumableItem);

      // Use item
      const result = inventory.useItem(0);

      expect(result).toBe(true);
      expect(inventory.getItems()[0].getQuantity()).toBe(2);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemUsed', consumableItem);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemUpdated', consumableItem);
    });

    test('should remove a consumable item if quantity reaches 0', () => {
      // Create consumable item
      const consumableItemData: ItemData = {
        ...mockItemData,
        type: ItemType.CONSUMABLE,
        stackable: true,
        maxStack: 5,
      };
      const consumableItem = new Item(consumableItemData, 1);

      // Add item
      inventory.addItem(consumableItem);

      // Use item
      const result = inventory.useItem(0);

      expect(result).toBe(true);
      expect(inventory.getItems().length).toBe(0);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemUsed', consumableItem);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('itemRemoved', consumableItem);
    });

    test('should return false if item is not usable', () => {
      // Create non-usable item
      const nonUsableItemData: ItemData = {
        ...mockItemData,
        usable: false,
      };
      const nonUsableItem = new Item(nonUsableItemData);

      // Add item
      inventory.addItem(nonUsableItem);

      // Reset mock to clear the itemAdded event
      jest.clearAllMocks();

      // Try to use item
      const result = inventory.useItem(0);

      expect(result).toBe(false);
      expect(inventory['eventEmitter'].emit).not.toHaveBeenCalled();
    });

    test('should return false if index is invalid', () => {
      const result = inventory.useItem(0);

      expect(result).toBe(false);
      expect(inventory['eventEmitter'].emit).not.toHaveBeenCalled();
    });
  });

  describe('findItemById', () => {
    test('should find an item by ID', () => {
      // Add item
      inventory.addItem(mockItem);

      // Find item
      const result = inventory.findItemById('test_item');

      expect(result).toBe(mockItem);
    });

    test('should return undefined if item is not found', () => {
      const result = inventory.findItemById('nonexistent_item');

      expect(result).toBeUndefined();
    });
  });

  describe('findItemsByType', () => {
    test('should find items by type', () => {
      // Create items of different types
      const toolItem = new Item(mockItemData);
      const documentItemData: ItemData = {
        ...mockItemData,
        id: 'document_item',
        type: ItemType.DOCUMENT,
      };
      const documentItem = new Item(documentItemData);

      // Add items
      inventory.addItem(toolItem);
      inventory.addItem(documentItem);

      // Find items by type
      const toolItems = inventory.findItemsByType(ItemType.TOOL);
      const documentItems = inventory.findItemsByType(ItemType.DOCUMENT);

      expect(toolItems.length).toBe(1);
      expect(toolItems[0]).toBe(toolItem);
      expect(documentItems.length).toBe(1);
      expect(documentItems[0]).toBe(documentItem);
    });
  });

  describe('hasItem', () => {
    test('should return true if inventory has the item', () => {
      // Add item
      inventory.addItem(mockItem);

      // Check if inventory has the item
      const result = inventory.hasItem('test_item');

      expect(result).toBe(true);
    });

    test('should return false if inventory does not have the item', () => {
      const result = inventory.hasItem('nonexistent_item');

      expect(result).toBe(false);
    });
  });

  describe('getItemQuantity', () => {
    test('should return the quantity of an item', () => {
      // Create stackable item
      const stackableItemData: ItemData = {
        ...mockItemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      // Add item
      inventory.addItem(stackableItem);

      // Get quantity
      const result = inventory.getItemQuantity('test_item');

      expect(result).toBe(3);
    });

    test('should return 0 if item is not found', () => {
      const result = inventory.getItemQuantity('nonexistent_item');

      expect(result).toBe(0);
    });
  });

  describe('clear', () => {
    test('should clear the inventory', () => {
      // Add items
      inventory.addItem(mockItem);
      inventory.addItem(new Item({ ...mockItemData, id: 'another_item' }));

      // Clear inventory
      inventory.clear();

      expect(inventory.getItems().length).toBe(0);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('inventoryCleared');
      expect(SaveManager.setData).toHaveBeenCalled();
    });
  });

  describe('saveInventory', () => {
    test('should save the inventory state', () => {
      // Add item
      inventory.addItem(mockItem);

      // Save inventory
      inventory.saveInventory();

      expect(SaveManager.setData).toHaveBeenCalledWith('inventory', [
        { id: 'test_item', quantity: 1 },
      ]);
    });
  });

  describe('loadInventory', () => {
    test('should load the inventory state', () => {
      // Mock saved data
      (SaveManager.getData as jest.Mock).mockReturnValue([{ id: 'test_item', quantity: 2 }]);

      // Load item definitions
      inventory.loadItemDefinitions([mockItemData]);

      // Load inventory
      const result = inventory.loadInventory();

      expect(result).toBe(true);
      expect(inventory.getItems().length).toBe(1);
      expect(inventory.getItems()[0].getId()).toBe('test_item');
      expect(inventory.getItems()[0].getQuantity()).toBe(2);
      expect(inventory['eventEmitter'].emit).toHaveBeenCalledWith('inventoryLoaded');
    });

    test('should return false if no saved data', () => {
      // Mock no saved data
      (SaveManager.getData as jest.Mock).mockReturnValue(null);

      // Load inventory
      const result = inventory.loadInventory();

      expect(result).toBe(false);
      expect(inventory.getItems().length).toBe(0);
      expect(inventory['eventEmitter'].emit).not.toHaveBeenCalled();
    });
  });

  describe('event listeners', () => {
    test('should add event listeners', () => {
      const callback = jest.fn();
      inventory.on('itemAdded', callback);

      expect(inventory['eventEmitter'].on).toHaveBeenCalledWith('itemAdded', callback, undefined);
    });

    test('should remove event listeners', () => {
      const callback = jest.fn();
      inventory.off('itemAdded', callback);

      expect(inventory['eventEmitter'].off).toHaveBeenCalledWith('itemAdded', callback, undefined);
    });

    test('should remove all event listeners', () => {
      inventory.removeAllListeners();

      expect(inventory['eventEmitter'].removeAllListeners).toHaveBeenCalled();
    });
  });
});

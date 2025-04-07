// Mock Phaser before importing Item
// eslint-disable-next-line @typescript-eslint/no-var-requires
jest.mock('phaser', () => require('../mocks/PhaserMock').default);

// Import after mocking
import { Item, ItemData, ItemType } from '../../src/inventory/Item';

describe('Item', () => {
  let itemData: ItemData;
  let item: Item;

  beforeEach(() => {
    // Create item data
    itemData = {
      id: 'test_item',
      name: 'Test Item',
      description: 'A test item',
      type: ItemType.TOOL,
      icon: 'test_icon',
      usable: true,
      stackable: false,
    };

    // Create item
    item = new Item(itemData);
  });

  describe('constructor', () => {
    test('should create an item with the provided data', () => {
      expect(item.getId()).toBe('test_item');
      expect(item.getName()).toBe('Test Item');
      expect(item.getDescription()).toBe('A test item');
      expect(item.getType()).toBe(ItemType.TOOL);
      expect(item.getIcon()).toBe('test_icon');
      expect(item.isUsable()).toBe(true);
      expect(item.isStackable()).toBe(false);
      expect(item.getQuantity()).toBe(1);
    });

    test('should create a stackable item with the provided quantity', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      expect(stackableItem.isStackable()).toBe(true);
      expect(stackableItem.getMaxStack()).toBe(5);
      expect(stackableItem.getQuantity()).toBe(3);
    });

    test('should clamp quantity to max stack', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 10);

      expect(stackableItem.getQuantity()).toBe(5);
    });

    test('should use default max stack if not provided', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
      };
      const stackableItem = new Item(stackableItemData);

      expect(stackableItem.getMaxStack()).toBe(99);
    });

    test('should use empty effects if not provided', () => {
      expect(item.getEffects()).toEqual({});
    });

    test('should use provided effects', () => {
      const itemWithEffects = new Item({
        ...itemData,
        effects: { action: 'test_action' },
      });

      expect(itemWithEffects.getEffects()).toEqual({ action: 'test_action' });
      expect(itemWithEffects.getEffect('action')).toBe('test_action');
    });
  });

  describe('quantity management', () => {
    test('should set quantity', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      const result = stackableItem.setQuantity(4);

      expect(result).toBe(4);
      expect(stackableItem.getQuantity()).toBe(4);
    });

    test('should clamp quantity to max stack when setting', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      const result = stackableItem.setQuantity(10);

      expect(result).toBe(5);
      expect(stackableItem.getQuantity()).toBe(5);
    });

    test('should clamp quantity to 0 when setting negative', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      const result = stackableItem.setQuantity(-1);

      expect(result).toBe(0);
      expect(stackableItem.getQuantity()).toBe(0);
    });

    test('should add to quantity', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      const result = stackableItem.addQuantity(1);

      expect(result).toBe(4);
      expect(stackableItem.getQuantity()).toBe(4);
    });

    test('should clamp when adding beyond max stack', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      const result = stackableItem.addQuantity(3);

      expect(result).toBe(5);
      expect(stackableItem.getQuantity()).toBe(5);
    });

    test('should remove from quantity', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      const result = stackableItem.removeQuantity(1);

      expect(result).toBe(2);
      expect(stackableItem.getQuantity()).toBe(2);
    });

    test('should clamp to 0 when removing too much', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      const result = stackableItem.removeQuantity(5);

      expect(result).toBe(0);
      expect(stackableItem.getQuantity()).toBe(0);
    });
  });

  describe('createSprite', () => {
    test('should create a sprite for the item', () => {
      const mockScene = {
        add: {
          sprite: jest.fn().mockReturnValue({
            width: 32,
            height: 32,
          }),
          text: jest.fn().mockReturnValue({
            setOrigin: jest.fn().mockReturnThis(),
          }),
          container: jest.fn().mockReturnValue({
            setSize: jest.fn(),
          }),
        },
      };

      const sprite = item.createSprite(mockScene as any, 100, 100);

      expect(mockScene.add.sprite).toHaveBeenCalledWith(100, 100, 'test_icon');
      expect(sprite).toBeDefined();
    });

    test('should add quantity text for stackable items with quantity > 1', () => {
      const stackableItemData: ItemData = {
        ...itemData,
        stackable: true,
        maxStack: 5,
      };
      const stackableItem = new Item(stackableItemData, 3);

      const mockScene = {
        add: {
          sprite: jest.fn().mockReturnValue({
            width: 32,
            height: 32,
          }),
          text: jest.fn().mockReturnValue({
            setOrigin: jest.fn().mockReturnThis(),
          }),
          container: jest.fn().mockReturnValue({
            setSize: jest.fn(),
          }),
        },
      };

      const sprite = stackableItem.createSprite(mockScene as any, 100, 100);

      expect(mockScene.add.sprite).toHaveBeenCalledWith(100, 100, 'test_icon');
      expect(mockScene.add.text).toHaveBeenCalledWith(110, 110, '3', expect.any(Object));
      expect(mockScene.add.container).toHaveBeenCalled();
      expect(sprite).toBeDefined();
    });
  });

  describe('serialization', () => {
    test('should convert to JSON', () => {
      const json = item.toJSON();

      expect(json).toEqual({
        id: 'test_item',
        quantity: 1,
      });
    });

    test('should create from JSON', () => {
      const json = {
        id: 'test_item',
        quantity: 3,
      };

      const newItem = Item.fromJSON(json, [itemData]);

      expect(newItem).not.toBeNull();
      expect(newItem?.getId()).toBe('test_item');
      expect(newItem?.getQuantity()).toBe(3);
    });

    test('should return null if item definition not found', () => {
      const json = {
        id: 'nonexistent_item',
        quantity: 1,
      };

      const newItem = Item.fromJSON(json, [itemData]);

      expect(newItem).toBeNull();
    });
  });
});

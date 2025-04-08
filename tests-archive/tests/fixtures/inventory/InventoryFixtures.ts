import { FixtureCollection } from '../types';

/**
 * Inventory fixtures
 */
export const InventoryFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'inventory',
  fixtures: [
    {
      id: 'emptyInventory',
      data: {
        items: [],
        capacity: 20,
        selectedItemIndex: -1,
      },
      metadata: {
        description: 'Empty inventory',
        tags: ['inventory', 'empty'],
      },
    },
    {
      id: 'startingInventory',
      data: {
        items: [
          {
            id: 'radio',
            quantity: 1,
          },
        ],
        capacity: 20,
        selectedItemIndex: 0,
      },
      metadata: {
        description: 'Starting inventory with just a radio',
        tags: ['inventory', 'starting'],
      },
    },
    {
      id: 'midGameInventory',
      data: {
        items: [
          {
            id: 'radio',
            quantity: 1,
          },
          {
            id: 'map',
            quantity: 1,
          },
          {
            id: 'battery',
            quantity: 3,
          },
          {
            id: 'note_tower',
            quantity: 1,
          },
        ],
        capacity: 20,
        selectedItemIndex: 0,
      },
      metadata: {
        description: 'Mid-game inventory with several items',
        tags: ['inventory', 'midGame'],
      },
    },
    {
      id: 'lateGameInventory',
      data: {
        items: [
          {
            id: 'radio',
            quantity: 1,
          },
          {
            id: 'map',
            quantity: 1,
          },
          {
            id: 'flashlight',
            quantity: 1,
          },
          {
            id: 'radio_enhancer',
            quantity: 1,
          },
          {
            id: 'radio_filter',
            quantity: 1,
          },
          {
            id: 'key_bunker',
            quantity: 1,
          },
          {
            id: 'key_lab',
            quantity: 1,
          },
          {
            id: 'battery',
            quantity: 3,
          },
          {
            id: 'medkit',
            quantity: 2,
          },
          {
            id: 'note_tower',
            quantity: 1,
          },
          {
            id: 'note_ruins',
            quantity: 1,
          },
        ],
        capacity: 20,
        selectedItemIndex: 0,
      },
      metadata: {
        description: 'Late-game inventory with many items',
        tags: ['inventory', 'lateGame'],
      },
    },
    {
      id: 'fullInventory',
      data: {
        items: Array(20)
          .fill(null)
          .map((_, i) => ({
            id: i < 10 ? `item_${i + 1}` : 'battery',
            quantity: i < 10 ? 1 : 2,
          })),
        capacity: 20,
        selectedItemIndex: 0,
      },
      metadata: {
        description: 'Full inventory at capacity',
        tags: ['inventory', 'full'],
      },
    },
    {
      id: 'stackedInventory',
      data: {
        items: [
          {
            id: 'radio',
            quantity: 1,
          },
          {
            id: 'map',
            quantity: 1,
          },
          {
            id: 'battery',
            quantity: 10,
          },
          {
            id: 'medkit',
            quantity: 5,
          },
        ],
        capacity: 20,
        selectedItemIndex: 0,
      },
      metadata: {
        description: 'Inventory with stacked items',
        tags: ['inventory', 'stacked'],
      },
    },
    {
      id: 'upgradedInventory',
      data: {
        items: [
          {
            id: 'radio',
            quantity: 1,
            effects: {
              enhanced: true,
              filtered: true,
            },
          },
          {
            id: 'map',
            quantity: 1,
            effects: {
              expanded: true,
            },
          },
          {
            id: 'flashlight',
            quantity: 1,
            effects: {
              brightness: 2,
            },
          },
        ],
        capacity: 20,
        selectedItemIndex: 0,
      },
      metadata: {
        description: 'Inventory with upgraded items',
        tags: ['inventory', 'upgraded'],
      },
    },
  ],
  metadata: {
    description: 'Inventory fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};

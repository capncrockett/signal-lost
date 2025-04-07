import { SaveDataFixture, FixtureCollection } from '../types';

/**
 * Save data fixtures
 */
export const SaveDataFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'saveData',
  fixtures: [
    {
      id: 'emptySave',
      data: {
        version: '1.0.0',
        timestamp: Date.now(),
        gameState: {
          flags: {},
          data: {},
        },
        settings: {
          volume: 0.8,
          musicVolume: 0.5,
          sfxVolume: 0.8,
        },
      },
      metadata: {
        description: 'Empty save data',
        tags: ['empty', 'newGame'],
      },
    },
    {
      id: 'midGameSave',
      data: {
        version: '1.0.0',
        timestamp: Date.now() - 86400000, // 1 day ago
        gameState: {
          flags: {
            discovered_tower1: true,
            discovered_ruins1: true,
            has_radio: true,
            knows_frequency: 91.5,
          },
          data: {
            lastFrequency: 91.5,
            visitedLocations: ['tower1', 'ruins1'],
            playerPosition: { x: 10, y: 8 },
            inventory: [
              { id: 'radio', quantity: 1 },
              { id: 'battery', quantity: 2 },
            ],
          },
        },
        settings: {
          volume: 0.7,
          musicVolume: 0.4,
          sfxVolume: 0.8,
        },
      },
      metadata: {
        description: 'Mid-game save data',
        tags: ['midGame', 'progress'],
      },
    },
    {
      id: 'corruptedSave',
      data: {
        version: '1.0.0',
        timestamp: Date.now() - 172800000, // 2 days ago
        gameState: {
          flags: {
            discovered_tower1: true,
            // Corrupted flag
            discovered_ruins1: null,
          },
          data: {
            // Missing lastFrequency
            visitedLocations: ['tower1', 'ruins1'],
            // Invalid player position
            playerPosition: { x: 'invalid', y: 8 },
          },
        },
        // Missing settings
      },
      metadata: {
        description: 'Corrupted save data for testing error handling',
        tags: ['corrupted', 'error'],
      },
    },
    {
      id: 'incompatibleVersionSave',
      data: {
        version: '0.5.0', // Older version
        timestamp: Date.now() - 2592000000, // 30 days ago
        gameState: {
          // Old format
          discoveredLocations: ['tower1'],
          playerPos: { x: 5, y: 5 },
          inventory: ['radio'],
        },
        settings: {
          volume: 0.5,
        },
      },
      metadata: {
        description: 'Save data with incompatible version for testing migration',
        tags: ['incompatible', 'migration'],
      },
    },
    {
      id: 'multiSlotSave',
      data: {
        slots: [
          {
            id: 'slot1',
            name: 'Slot 1',
            version: '1.0.0',
            timestamp: Date.now() - 86400000, // 1 day ago
            gameState: {
              flags: {
                discovered_tower1: true,
              },
              data: {
                lastFrequency: 91.5,
                visitedLocations: ['tower1'],
                playerPosition: { x: 10, y: 8 },
              },
            },
          },
          {
            id: 'slot2',
            name: 'Slot 2',
            version: '1.0.0',
            timestamp: Date.now() - 172800000, // 2 days ago
            gameState: {
              flags: {
                discovered_tower1: true,
                discovered_ruins1: true,
              },
              data: {
                lastFrequency: 96.3,
                visitedLocations: ['tower1', 'ruins1'],
                playerPosition: { x: 15, y: 12 },
              },
            },
          },
          {
            id: 'slot3',
            name: 'Empty Slot',
            version: '1.0.0',
            timestamp: null,
            gameState: null,
          },
        ],
        settings: {
          volume: 0.8,
          musicVolume: 0.5,
          sfxVolume: 0.8,
        },
      },
      metadata: {
        description: 'Save data with multiple slots',
        tags: ['multiSlot', 'slots'],
      },
    },
  ],
  metadata: {
    description: 'Save data fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};

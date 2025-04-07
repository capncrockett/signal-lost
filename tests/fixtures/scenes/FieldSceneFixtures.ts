import { FieldSceneFixture, FixtureCollection } from '../types';

/**
 * Field scene fixtures
 */
export const FieldSceneFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'fieldScene',
  fixtures: [
    {
      id: 'initialState',
      data: {
        playerPosition: { x: 5, y: 5 },
        playerDirection: 'down',
        playerMoving: false,
        interactables: [
          { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
          { id: 'tower2', type: 'tower', x: 20, y: 15, triggerDistance: 2 },
          { id: 'ruins1', type: 'ruins', x: 15, y: 12, triggerDistance: 1 },
        ],
        nearbyInteractable: null,
        narrativeActive: false,
        currentNarrativeEvent: null,
      },
      metadata: {
        description: 'Initial state of the field scene',
        tags: ['initial', 'field'],
      },
    },
    {
      id: 'nearTower',
      data: {
        playerPosition: { x: 9, y: 8 },
        playerDirection: 'right',
        playerMoving: false,
        interactables: [
          { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
          { id: 'tower2', type: 'tower', x: 20, y: 15, triggerDistance: 2 },
          { id: 'ruins1', type: 'ruins', x: 15, y: 12, triggerDistance: 1 },
        ],
        nearbyInteractable: { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
        narrativeActive: false,
        currentNarrativeEvent: null,
      },
      metadata: {
        description: 'Field scene with player near a tower',
        tags: ['field', 'tower', 'interaction'],
      },
    },
    {
      id: 'narrativeActive',
      data: {
        playerPosition: { x: 9, y: 8 },
        playerDirection: 'right',
        playerMoving: false,
        interactables: [
          { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
          { id: 'tower2', type: 'tower', x: 20, y: 15, triggerDistance: 2 },
          { id: 'ruins1', type: 'ruins', x: 15, y: 12, triggerDistance: 1 },
        ],
        nearbyInteractable: { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
        narrativeActive: true,
        currentNarrativeEvent: {
          id: 'tower_discovery',
          message: 'You hear whispers in the static. The radio tower looms above you, its red light blinking in the fog.',
          choices: [
            {
              text: 'Investigate the tower',
              outcome: 'trigger_tower_investigation',
            },
            {
              text: 'Keep your distance',
              outcome: 'trigger_tower_observe',
            },
          ],
        },
      },
      metadata: {
        description: 'Field scene with active narrative dialog',
        tags: ['field', 'narrative', 'dialog'],
      },
    },
    {
      id: 'inventoryOpen',
      data: {
        playerPosition: { x: 5, y: 5 },
        playerDirection: 'down',
        playerMoving: false,
        interactables: [
          { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
          { id: 'tower2', type: 'tower', x: 20, y: 15, triggerDistance: 2 },
          { id: 'ruins1', type: 'ruins', x: 15, y: 12, triggerDistance: 1 },
        ],
        nearbyInteractable: null,
        narrativeActive: false,
        currentNarrativeEvent: null,
        inventoryOpen: true,
        inventory: {
          items: [
            { id: 'radio', type: 'tool', name: 'Radio', quantity: 1 },
            { id: 'map', type: 'tool', name: 'Map', quantity: 1 },
            { id: 'battery', type: 'consumable', name: 'Battery', quantity: 3 },
          ],
          selectedItemIndex: 0,
        },
      },
      metadata: {
        description: 'Field scene with inventory open',
        tags: ['field', 'inventory', 'ui'],
      },
    },
    {
      id: 'itemPickup',
      data: {
        playerPosition: { x: 11, y: 8 },
        playerDirection: 'down',
        playerMoving: false,
        interactables: [
          { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
          { id: 'tower2', type: 'tower', x: 20, y: 15, triggerDistance: 2 },
          { id: 'ruins1', type: 'ruins', x: 15, y: 12, triggerDistance: 1 },
          { id: 'note_tower', type: 'item', x: 11, y: 8, triggerDistance: 1 },
        ],
        nearbyInteractable: { id: 'note_tower', type: 'item', x: 11, y: 8, triggerDistance: 1 },
        narrativeActive: false,
        currentNarrativeEvent: null,
      },
      metadata: {
        description: 'Field scene with player near an item pickup',
        tags: ['field', 'item', 'interaction'],
      },
    },
  ],
  metadata: {
    description: 'Field scene fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};

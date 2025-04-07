import { FixtureCollection } from '../types';

/**
 * Main scene fixtures
 */
export const MainSceneFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'mainScene',
  fixtures: [
    {
      id: 'initialState',
      data: {
        frequency: 88.0,
        signalStrength: 0,
        activeSignal: null,
        radioInitialized: false,
        audioInitialized: false,
      },
      metadata: {
        description: 'Initial state of the main scene',
        tags: ['initial', 'radio'],
      },
    },
    {
      id: 'signalDetected',
      data: {
        frequency: 91.5,
        signalStrength: 0.95,
        activeSignal: {
          id: 'signal1_discovery',
          frequency: 91.5,
          range: 0.5,
          type: 'location',
          data: {
            x: 10,
            y: 8,
            name: 'Radio Tower',
          },
        },
        radioInitialized: true,
        audioInitialized: true,
      },
      metadata: {
        description: 'Main scene with a detected signal',
        tags: ['signal', 'radio'],
      },
    },
    {
      id: 'weakSignal',
      data: {
        frequency: 91.3,
        signalStrength: 0.3,
        activeSignal: {
          id: 'signal1_discovery',
          frequency: 91.5,
          range: 0.5,
          type: 'location',
          data: {
            x: 10,
            y: 8,
            name: 'Radio Tower',
          },
        },
        radioInitialized: true,
        audioInitialized: true,
      },
      metadata: {
        description: 'Main scene with a weak signal',
        tags: ['signal', 'radio', 'weak'],
      },
    },
    {
      id: 'noSignal',
      data: {
        frequency: 100.0,
        signalStrength: 0,
        activeSignal: null,
        radioInitialized: true,
        audioInitialized: true,
      },
      metadata: {
        description: 'Main scene with no signal',
        tags: ['noSignal', 'radio'],
      },
    },
    {
      id: 'enhancedRadio',
      data: {
        frequency: 91.5,
        signalStrength: 1.0,
        activeSignal: {
          id: 'signal1_discovery',
          frequency: 91.5,
          range: 0.5,
          type: 'location',
          data: {
            x: 10,
            y: 8,
            name: 'Radio Tower',
          },
        },
        radioInitialized: true,
        audioInitialized: true,
        enhancedRadio: true,
        signalFilter: true,
      },
      metadata: {
        description: 'Main scene with enhanced radio capabilities',
        tags: ['signal', 'radio', 'enhanced'],
      },
    },
    {
      id: 'saveMenuOpen',
      data: {
        frequency: 88.0,
        signalStrength: 0,
        activeSignal: null,
        radioInitialized: true,
        audioInitialized: true,
        saveMenuOpen: true,
        saveSlots: [
          {
            id: 'slot1',
            name: 'Slot 1',
            timestamp: Date.now() - 86400000,
            hasData: true,
          },
          {
            id: 'slot2',
            name: 'Slot 2',
            timestamp: null,
            hasData: false,
          },
          {
            id: 'slot3',
            name: 'Slot 3',
            timestamp: null,
            hasData: false,
          },
        ],
      },
      metadata: {
        description: 'Main scene with save menu open',
        tags: ['saveMenu', 'ui'],
      },
    },
  ],
  metadata: {
    description: 'Main scene fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};

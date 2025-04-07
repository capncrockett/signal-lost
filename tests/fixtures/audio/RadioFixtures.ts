import { FixtureCollection } from '../types';

/**
 * Radio fixtures
 */
export const RadioFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'radio',
  fixtures: [
    {
      id: 'defaultRadio',
      data: {
        frequency: 88.0,
        minFrequency: 88.0,
        maxFrequency: 108.0,
        signalStrength: 0,
        tuningSpeed: 0.1,
        signalFrequencies: [
          { frequency: 91.5, range: 0.5, id: 'signal1' },
          { frequency: 96.3, range: 0.5, id: 'signal2' },
          { frequency: 103.7, range: 0.5, id: 'signal3' },
          { frequency: 105.1, range: 0.5, id: 'signal4' },
        ],
        signalTolerance: 0.3,
        enhanced: false,
        filtered: false,
      },
      metadata: {
        description: 'Default radio settings',
        tags: ['radio', 'default'],
      },
    },
    {
      id: 'enhancedRadio',
      data: {
        frequency: 88.0,
        minFrequency: 88.0,
        maxFrequency: 108.0,
        signalStrength: 0,
        tuningSpeed: 0.1,
        signalFrequencies: [
          { frequency: 91.5, range: 0.8, id: 'signal1' },
          { frequency: 96.3, range: 0.8, id: 'signal2' },
          { frequency: 103.7, range: 0.8, id: 'signal3' },
          { frequency: 105.1, range: 0.8, id: 'signal4' },
        ],
        signalTolerance: 0.5,
        enhanced: true,
        filtered: false,
        rangeBoost: 1.5,
      },
      metadata: {
        description: 'Enhanced radio with better range',
        tags: ['radio', 'enhanced'],
      },
    },
    {
      id: 'filteredRadio',
      data: {
        frequency: 88.0,
        minFrequency: 88.0,
        maxFrequency: 108.0,
        signalStrength: 0,
        tuningSpeed: 0.1,
        signalFrequencies: [
          { frequency: 91.5, range: 0.5, id: 'signal1' },
          { frequency: 96.3, range: 0.5, id: 'signal2' },
          { frequency: 103.7, range: 0.5, id: 'signal3' },
          { frequency: 105.1, range: 0.5, id: 'signal4' },
        ],
        signalTolerance: 0.3,
        enhanced: false,
        filtered: true,
        clarityBoost: 2.0,
        staticReduction: 0.5,
      },
      metadata: {
        description: 'Filtered radio with reduced static',
        tags: ['radio', 'filtered'],
      },
    },
    {
      id: 'fullyUpgradedRadio',
      data: {
        frequency: 88.0,
        minFrequency: 88.0,
        maxFrequency: 108.0,
        signalStrength: 0,
        tuningSpeed: 0.1,
        signalFrequencies: [
          { frequency: 91.5, range: 0.8, id: 'signal1' },
          { frequency: 96.3, range: 0.8, id: 'signal2' },
          { frequency: 103.7, range: 0.8, id: 'signal3' },
          { frequency: 105.1, range: 0.8, id: 'signal4' },
        ],
        signalTolerance: 0.5,
        enhanced: true,
        filtered: true,
        rangeBoost: 1.5,
        clarityBoost: 2.0,
        staticReduction: 0.5,
      },
      metadata: {
        description: 'Fully upgraded radio with all enhancements',
        tags: ['radio', 'enhanced', 'filtered', 'upgraded'],
      },
    },
    {
      id: 'tunedToSignal',
      data: {
        frequency: 91.5,
        minFrequency: 88.0,
        maxFrequency: 108.0,
        signalStrength: 0.95,
        tuningSpeed: 0.1,
        signalFrequencies: [
          { frequency: 91.5, range: 0.5, id: 'signal1' },
          { frequency: 96.3, range: 0.5, id: 'signal2' },
          { frequency: 103.7, range: 0.5, id: 'signal3' },
          { frequency: 105.1, range: 0.5, id: 'signal4' },
        ],
        signalTolerance: 0.3,
        enhanced: false,
        filtered: false,
        activeSignal: {
          id: 'signal1',
          frequency: 91.5,
          range: 0.5,
          type: 'location',
          data: {
            x: 10,
            y: 8,
            name: 'Radio Tower',
          },
        },
      },
      metadata: {
        description: 'Radio tuned to a signal',
        tags: ['radio', 'signal', 'tuned'],
      },
    },
    {
      id: 'weakSignal',
      data: {
        frequency: 91.3,
        minFrequency: 88.0,
        maxFrequency: 108.0,
        signalStrength: 0.3,
        tuningSpeed: 0.1,
        signalFrequencies: [
          { frequency: 91.5, range: 0.5, id: 'signal1' },
          { frequency: 96.3, range: 0.5, id: 'signal2' },
          { frequency: 103.7, range: 0.5, id: 'signal3' },
          { frequency: 105.1, range: 0.5, id: 'signal4' },
        ],
        signalTolerance: 0.3,
        enhanced: false,
        filtered: false,
        activeSignal: {
          id: 'signal1',
          frequency: 91.5,
          range: 0.5,
          type: 'location',
          data: {
            x: 10,
            y: 8,
            name: 'Radio Tower',
          },
        },
      },
      metadata: {
        description: 'Radio with weak signal reception',
        tags: ['radio', 'signal', 'weak'],
      },
    },
  ],
  metadata: {
    description: 'Radio fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};

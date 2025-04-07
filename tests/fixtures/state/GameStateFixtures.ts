import { GameStateFixture, FixtureCollection } from '../types';

/**
 * Game state fixtures
 */
export const GameStateFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'gameState',
  fixtures: [
    {
      id: 'newGame',
      data: {
        flags: {},
        data: {},
      },
      metadata: {
        description: 'New game state with no progress',
        tags: ['newGame', 'empty'],
      },
    },
    {
      id: 'midGame',
      data: {
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
        },
      },
      metadata: {
        description: 'Mid-game state with some progress',
        tags: ['midGame', 'progress'],
      },
    },
    {
      id: 'lateGame',
      data: {
        flags: {
          discovered_tower1: true,
          discovered_tower2: true,
          discovered_ruins1: true,
          discovered_bunker1: true,
          has_radio: true,
          has_map: true,
          has_flashlight: true,
          has_radio_enhancer: true,
          has_signal_filter: true,
          knows_frequency: 91.5,
          knows_frequency_sequence: true,
          knows_security_code: true,
        },
        data: {
          lastFrequency: 103.7,
          visitedLocations: ['tower1', 'tower2', 'ruins1', 'bunker1'],
          playerPosition: { x: 5, y: 20 },
          inventory: [
            { id: 'radio', quantity: 1 },
            { id: 'map', quantity: 1 },
            { id: 'flashlight', quantity: 1 },
            { id: 'radio_enhancer', quantity: 1 },
            { id: 'radio_filter', quantity: 1 },
            { id: 'key_bunker', quantity: 1 },
            { id: 'key_lab', quantity: 1 },
            { id: 'battery', quantity: 3 },
            { id: 'medkit', quantity: 2 },
          ],
        },
      },
      metadata: {
        description: 'Late-game state with significant progress',
        tags: ['lateGame', 'progress'],
      },
    },
    {
      id: 'completedGame',
      data: {
        flags: {
          discovered_tower1: true,
          discovered_tower2: true,
          discovered_ruins1: true,
          discovered_bunker1: true,
          has_radio: true,
          has_map: true,
          has_flashlight: true,
          has_radio_enhancer: true,
          has_signal_filter: true,
          knows_frequency: 91.5,
          knows_frequency_sequence: true,
          knows_security_code: true,
          completed_signal_protocol: true,
          game_completed: true,
        },
        data: {
          lastFrequency: 105.1,
          visitedLocations: ['tower1', 'tower2', 'ruins1', 'bunker1'],
          playerPosition: { x: 15, y: 15 },
          inventory: [
            { id: 'radio', quantity: 1 },
            { id: 'map', quantity: 1 },
            { id: 'flashlight', quantity: 1 },
            { id: 'radio_enhancer', quantity: 1 },
            { id: 'radio_filter', quantity: 1 },
            { id: 'key_bunker', quantity: 1 },
            { id: 'key_lab', quantity: 1 },
            { id: 'battery', quantity: 5 },
            { id: 'medkit', quantity: 3 },
            { id: 'journal', quantity: 1 },
            { id: 'note_tower', quantity: 1 },
            { id: 'note_ruins', quantity: 1 },
          ],
          completedSequence: [91.5, 96.3, 103.7, 105.1],
          completionTime: 3600000, // 1 hour in milliseconds
        },
      },
      metadata: {
        description: 'Completed game state',
        tags: ['completedGame', 'endGame'],
      },
    },
  ],
  metadata: {
    description: 'Game state fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};

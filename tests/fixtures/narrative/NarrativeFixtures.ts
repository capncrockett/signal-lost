import { NarrativeFixture, FixtureCollection } from '../types';

/**
 * Narrative fixtures
 */
export const NarrativeFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'narrative',
  fixtures: [
    {
      id: 'emptyEngine',
      data: {
        events: {},
        flags: {},
        history: [],
        currentEvent: null,
        activeEvent: null,
      },
      metadata: {
        description: 'Empty narrative engine state',
        tags: ['narrative', 'empty'],
      },
    },
    {
      id: 'basicEngine',
      data: {
        events: {
          'intro': {
            id: 'intro',
            message: 'Welcome to Signal Lost. Your journey begins now.',
            choices: [
              {
                text: 'Continue',
                outcome: 'trigger_tutorial',
              },
            ],
          },
          'tutorial': {
            id: 'tutorial',
            message: 'Use the radio to find signals and discover locations.',
            choices: [
              {
                text: 'Got it',
                outcome: 'set_tutorial_complete=true',
              },
            ],
          },
        },
        flags: {
          'tutorial_complete': false,
        },
        history: ['intro'],
        currentEvent: 'intro',
        activeEvent: {
          id: 'intro',
          message: 'Welcome to Signal Lost. Your journey begins now.',
          choices: [
            {
              text: 'Continue',
              outcome: 'trigger_tutorial',
            },
          ],
        },
      },
      metadata: {
        description: 'Basic narrative engine state with intro and tutorial',
        tags: ['narrative', 'basic'],
      },
    },
    {
      id: 'midGameEngine',
      data: {
        events: {
          'intro': {
            id: 'intro',
            message: 'Welcome to Signal Lost. Your journey begins now.',
            choices: [
              {
                text: 'Continue',
                outcome: 'trigger_tutorial',
              },
            ],
          },
          'tutorial': {
            id: 'tutorial',
            message: 'Use the radio to find signals and discover locations.',
            choices: [
              {
                text: 'Got it',
                outcome: 'set_tutorial_complete=true',
              },
            ],
          },
          'tower_discovery': {
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
          'tower_investigation': {
            id: 'tower_investigation',
            message: 'As you approach the tower, the static on your radio intensifies. You notice a small door at the base of the structure.',
            choices: [
              {
                text: 'Try to open the door',
                outcome: 'trigger_tower_door',
              },
              {
                text: 'Circle around the tower',
                outcome: 'trigger_tower_circle',
              },
              {
                text: 'Back away',
                outcome: 'trigger_tower_observe',
              },
            ],
          },
        },
        flags: {
          'tutorial_complete': true,
          'discovered_tower1': true,
        },
        history: ['intro', 'tutorial', 'tower_discovery'],
        currentEvent: 'tower_discovery',
        activeEvent: {
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
        description: 'Mid-game narrative engine state',
        tags: ['narrative', 'midGame'],
      },
    },
    {
      id: 'conditionalEvents',
      data: {
        events: {
          'bunker_discovery': {
            id: 'bunker_discovery',
            message: 'You\'ve found a bunker entrance partially hidden by vegetation. The heavy metal door is locked.',
            choices: [
              {
                text: 'Use the bunker key',
                outcome: 'trigger_bunker_enter',
                condition: 'has_bunker_key=true',
              },
              {
                text: 'Try to force the door',
                outcome: 'trigger_bunker_force',
              },
              {
                text: 'Leave the bunker',
                outcome: 'trigger_new_area',
              },
            ],
          },
          'bunker_force': {
            id: 'bunker_force',
            message: 'You try to force the door, but it\'s too sturdy. You\'ll need the proper key to open it.',
            choices: [
              {
                text: 'Look for another way in',
                outcome: 'trigger_bunker_search',
              },
              {
                text: 'Leave the bunker',
                outcome: 'trigger_new_area',
              },
            ],
          },
        },
        flags: {
          'discovered_bunker1': true,
          'has_bunker_key': false,
        },
        history: ['bunker_discovery', 'bunker_force'],
        currentEvent: 'bunker_force',
        activeEvent: {
          id: 'bunker_force',
          message: 'You try to force the door, but it\'s too sturdy. You\'ll need the proper key to open it.',
          choices: [
            {
              text: 'Look for another way in',
              outcome: 'trigger_bunker_search',
            },
            {
              text: 'Leave the bunker',
              outcome: 'trigger_new_area',
            },
          ],
        },
      },
      metadata: {
        description: 'Narrative engine with conditional events',
        tags: ['narrative', 'conditional'],
      },
    },
    {
      id: 'complexBranching',
      data: {
        events: {
          'signal_protocol_decision': {
            id: 'signal_protocol_decision',
            message: 'You\'ve discovered all the frequencies for the Signal Protocol. Do you want to activate it?',
            choices: [
              {
                text: 'Activate the protocol',
                outcome: 'trigger_protocol_activation',
              },
              {
                text: 'Wait and gather more information',
                outcome: 'trigger_protocol_delay',
              },
              {
                text: 'Destroy the protocol documentation',
                outcome: 'trigger_protocol_destroy',
              },
            ],
          },
          'protocol_activation': {
            id: 'protocol_activation',
            message: 'You tune your radio to each frequency in sequence: 91.5, 96.3, 103.7, 105.1. The static clears, and a voice speaks clearly through your radio...',
            choices: [
              {
                text: 'Listen carefully',
                outcome: 'trigger_protocol_message',
              },
            ],
          },
          'protocol_delay': {
            id: 'protocol_delay',
            message: 'You decide to wait and gather more information before activating the protocol. Perhaps there\'s more to discover.',
            choices: [
              {
                text: 'Continue exploring',
                outcome: 'trigger_new_area',
              },
            ],
          },
          'protocol_destroy': {
            id: 'protocol_destroy',
            message: 'You destroy the protocol documentation, ensuring that whatever it might activate remains dormant. Perhaps some things are better left undiscovered.',
            choices: [
              {
                text: 'Continue exploring',
                outcome: 'trigger_new_area',
              },
            ],
          },
        },
        flags: {
          'knows_frequency_sequence': true,
          'has_all_frequencies': true,
        },
        history: ['signal_protocol_decision'],
        currentEvent: 'signal_protocol_decision',
        activeEvent: {
          id: 'signal_protocol_decision',
          message: 'You\'ve discovered all the frequencies for the Signal Protocol. Do you want to activate it?',
          choices: [
            {
              text: 'Activate the protocol',
              outcome: 'trigger_protocol_activation',
            },
            {
              text: 'Wait and gather more information',
              outcome: 'trigger_protocol_delay',
            },
            {
              text: 'Destroy the protocol documentation',
              outcome: 'trigger_protocol_destroy',
            },
          ],
        },
      },
      metadata: {
        description: 'Narrative engine with complex branching choices',
        tags: ['narrative', 'branching', 'complex'],
      },
    },
  ],
  metadata: {
    description: 'Narrative engine fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};

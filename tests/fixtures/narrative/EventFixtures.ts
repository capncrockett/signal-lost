import { EventFixture, FixtureCollection } from '../types';

/**
 * Event fixtures
 */
export const EventFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'events',
  fixtures: [
    {
      id: 'signalEvents',
      data: {
        events: [
          {
            id: 'signal1_discovery',
            message: 'You\'ve tuned into the emergency broadcast frequency. Through the static, you hear coordinates being repeated: \'...survivors at coordinates 10, 8... medical supplies available... approach with caution...\'',
            choices: [
              {
                text: 'Mark the coordinates',
                outcome: 'set_discovered_tower1=true',
              },
              {
                text: 'Keep listening',
                outcome: 'trigger_signal1_more',
              },
            ],
            interference: 0.2,
          },
          {
            id: 'signal1_more',
            message: 'The message continues: \'...repeat, survivors at tower coordinates 10, 8... we have limited medical supplies... radio frequency 91.5 MHz for contact...\'',
            choices: [
              {
                text: 'Mark the coordinates',
                outcome: 'set_discovered_tower1=true',
              },
              {
                text: 'Note the frequency',
                outcome: 'set_knows_frequency=91.5',
              },
            ],
            interference: 0.3,
          },
          {
            id: 'signal2_discovery',
            message: 'You\'ve tuned into a distress signal. A panicked voice speaks through bursts of static: \'If anyone can hear this... we\'re trapped... coordinates 15, 12... need immediate assistance...\'',
            choices: [
              {
                text: 'Mark the coordinates',
                outcome: 'set_discovered_ruins1=true',
              },
              {
                text: 'Continue searching for signals',
                outcome: 'trigger_radio_tuning',
              },
            ],
            interference: 0.4,
          },
        ],
      },
      metadata: {
        description: 'Signal discovery events',
        tags: ['events', 'signal', 'discovery'],
      },
    },
    {
      id: 'locationEvents',
      data: {
        events: [
          {
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
            interference: 0.3,
          },
          {
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
            interference: 0.5,
          },
          {
            id: 'ruins_discovery',
            message: 'The ruins appear to be an old research facility. Most of the building has collapsed, but one section remains intact.',
            choices: [
              {
                text: 'Enter the intact section',
                outcome: 'trigger_ruins_enter',
              },
              {
                text: 'Search around the ruins',
                outcome: 'trigger_ruins_search',
              },
              {
                text: 'Leave the ruins',
                outcome: 'trigger_new_area',
              },
            ],
            interference: 0.2,
          },
        ],
      },
      metadata: {
        description: 'Location discovery events',
        tags: ['events', 'location', 'discovery'],
      },
    },
    {
      id: 'itemEvents',
      data: {
        events: [
          {
            id: 'item_note_tower',
            message: 'You found a note. It appears to be a page torn from a journal.',
            choices: [
              {
                text: 'Read the note',
                outcome: 'trigger_note_tower_read',
              },
              {
                text: 'Put it away',
                outcome: 'set_has_note_tower=true',
              },
            ],
          },
          {
            id: 'note_tower_read',
            message: 'The note reads: "The signal protocol requires three specific frequencies to be activated in sequence. The first is at 91.5 MHz. Find the others by exploring the ruins."',
            choices: [
              {
                text: 'Take the note',
                outcome: 'set_has_note_tower=true',
              },
              {
                text: 'Leave it',
                outcome: '',
              },
            ],
          },
          {
            id: 'item_key_bunker',
            message: 'You found a key with a label that reads "Bunker Access".',
            choices: [
              {
                text: 'Take the key',
                outcome: 'set_has_bunker_key=true',
              },
              {
                text: 'Leave it',
                outcome: '',
              },
            ],
          },
        ],
      },
      metadata: {
        description: 'Item discovery events',
        tags: ['events', 'item', 'discovery'],
      },
    },
    {
      id: 'endgameEvents',
      data: {
        events: [
          {
            id: 'protocol_activation',
            message: 'You tune your radio to each frequency in sequence: 91.5, 96.3, 103.7, 105.1. The static clears, and a voice speaks clearly through your radio...',
            choices: [
              {
                text: 'Listen carefully',
                outcome: 'trigger_protocol_message',
              },
            ],
          },
          {
            id: 'protocol_message',
            message: '"Thank you for activating the Signal Protocol. We have been waiting for someone to find us. Our signal has been broadcasting for decades, hoping someone would discover the truth. Now, at last, we can tell our story..."',
            choices: [
              {
                text: 'Continue listening',
                outcome: 'trigger_protocol_revelation',
              },
            ],
          },
          {
            id: 'protocol_revelation',
            message: 'The voice reveals the truth about what happened to the research facility and the surrounding area. The experiments with radio frequencies opened something that should have remained closed. The voice thanks you for listening and fades away, leaving you alone with the knowledge of what happened here.',
            choices: [
              {
                text: 'End',
                outcome: 'set_game_completed=true',
              },
            ],
          },
        ],
      },
      metadata: {
        description: 'Endgame events',
        tags: ['events', 'endgame', 'finale'],
      },
    },
    {
      id: 'tutorialEvents',
      data: {
        events: [
          {
            id: 'tutorial_intro',
            message: 'Welcome to Signal Lost. In this game, you\'ll use your radio to find signals and explore mysterious locations.',
            choices: [
              {
                text: 'Continue',
                outcome: 'trigger_tutorial_radio',
              },
            ],
          },
          {
            id: 'tutorial_radio',
            message: 'Your radio is your most important tool. Use it to tune into different frequencies and discover signals. Try tuning to 91.5 MHz to find your first signal.',
            choices: [
              {
                text: 'Got it',
                outcome: 'trigger_tutorial_movement',
              },
            ],
          },
          {
            id: 'tutorial_movement',
            message: 'Use WASD or arrow keys to move around the field. Press E or Space to interact with objects.',
            choices: [
              {
                text: 'Got it',
                outcome: 'trigger_tutorial_inventory',
              },
            ],
          },
          {
            id: 'tutorial_inventory',
            message: 'Press I to open your inventory. You\'ll collect items as you explore, which can help you progress through the game.',
            choices: [
              {
                text: 'Got it',
                outcome: 'set_tutorial_complete=true',
              },
            ],
          },
        ],
      },
      metadata: {
        description: 'Tutorial events',
        tags: ['events', 'tutorial'],
      },
    },
  ],
  metadata: {
    description: 'Event fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};

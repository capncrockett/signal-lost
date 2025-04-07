import { ItemFixture, FixtureCollection } from '../types';

// Define ItemType enum locally to avoid circular dependencies
enum ItemType {
  TOOL = 'tool',
  COMPONENT = 'component',
  CONSUMABLE = 'consumable',
  KEY = 'key',
  DOCUMENT = 'document',
}

/**
 * Item fixtures
 */
export const ItemFixtures: FixtureCollection<Record<string, unknown>> = {
  id: 'items',
  fixtures: [
    {
      id: 'allItems',
      data: [
        {
          id: 'radio',
          name: 'Radio',
          description: 'A portable radio that can tune into different frequencies.',
          type: ItemType.TOOL,
          icon: 'radio',
          usable: true,
          stackable: false,
          effects: {
            action: 'open_radio',
          },
        },
        {
          id: 'map',
          name: 'Map',
          description: 'A map of the area with discovered locations marked.',
          type: ItemType.TOOL,
          icon: 'map',
          usable: true,
          stackable: false,
          effects: {
            action: 'open_map',
          },
        },
        {
          id: 'flashlight',
          name: 'Flashlight',
          description: 'A flashlight for illuminating dark areas.',
          type: ItemType.TOOL,
          icon: 'flashlight',
          usable: true,
          stackable: false,
          effects: {
            action: 'toggle_light',
          },
        },
        {
          id: 'radio_enhancer',
          name: 'Signal Enhancer',
          description: 'Enhances radio reception range.',
          type: ItemType.COMPONENT,
          icon: 'radio_enhancer',
          usable: true,
          stackable: false,
          effects: {
            action: 'enhance_radio',
          },
        },
        {
          id: 'radio_filter',
          name: 'Signal Filter',
          description: 'Filters out static and improves signal clarity.',
          type: ItemType.COMPONENT,
          icon: 'radio_filter',
          usable: true,
          stackable: false,
          effects: {
            action: 'filter_radio',
          },
        },
        {
          id: 'key_bunker',
          name: 'Bunker Key',
          description: 'A key to the bunker entrance.',
          type: ItemType.KEY,
          icon: 'key_bunker',
          usable: false,
          stackable: false,
        },
        {
          id: 'key_lab',
          name: 'Lab Key',
          description: 'A key to the laboratory section of the ruins.',
          type: ItemType.KEY,
          icon: 'key_lab',
          usable: false,
          stackable: false,
        },
        {
          id: 'battery',
          name: 'Battery',
          description: 'A battery for powering electronic devices.',
          type: ItemType.CONSUMABLE,
          icon: 'battery',
          usable: true,
          stackable: true,
          maxStack: 10,
          effects: {
            action: 'recharge',
            amount: 25,
          },
        },
        {
          id: 'medkit',
          name: 'Medkit',
          description: 'A medical kit for treating injuries.',
          type: ItemType.CONSUMABLE,
          icon: 'medkit',
          usable: true,
          stackable: true,
          maxStack: 5,
          effects: {
            action: 'heal',
            amount: 50,
          },
        },
        {
          id: 'journal',
          name: 'Journal',
          description: 'A journal with notes about the area and the signal protocol.',
          type: ItemType.DOCUMENT,
          icon: 'journal',
          usable: true,
          stackable: false,
          effects: {
            action: 'read_note',
            content: 'The journal contains notes about the Signal Protocol. It mentions that the protocol requires four specific frequencies to be tuned in sequence: 91.5, 96.3, 103.7, and 105.1 MHz. The purpose of the protocol is unclear, but it seems to be related to communication with something beyond our understanding.',
          },
        },
        {
          id: 'note_tower',
          name: 'Tower Note',
          description: 'A note found at the radio tower.',
          type: ItemType.DOCUMENT,
          icon: 'note',
          usable: true,
          stackable: false,
          effects: {
            action: 'read_note',
            content: 'The note reads: "Something\'s happening at the research facility. They were experimenting with those frequencies again. I\'m going to investigate."',
          },
        },
        {
          id: 'note_ruins',
          name: 'Ruins Note',
          description: 'A note found at the ruins.',
          type: ItemType.DOCUMENT,
          icon: 'note',
          usable: true,
          stackable: false,
          effects: {
            action: 'read_note',
            content: 'The note reads: "The experiments with the Signal Protocol have yielded unexpected results. We\'ve made contact with something, but I\'m not sure we should continue. The frequencies seem to be opening a channel to... somewhere else."',
          },
        },
      ],
      metadata: {
        description: 'All item definitions',
        tags: ['items', 'all'],
      },
    },
    {
      id: 'toolItems',
      data: [
        {
          id: 'radio',
          name: 'Radio',
          description: 'A portable radio that can tune into different frequencies.',
          type: ItemType.TOOL,
          icon: 'radio',
          usable: true,
          stackable: false,
          effects: {
            action: 'open_radio',
          },
        },
        {
          id: 'map',
          name: 'Map',
          description: 'A map of the area with discovered locations marked.',
          type: ItemType.TOOL,
          icon: 'map',
          usable: true,
          stackable: false,
          effects: {
            action: 'open_map',
          },
        },
        {
          id: 'flashlight',
          name: 'Flashlight',
          description: 'A flashlight for illuminating dark areas.',
          type: ItemType.TOOL,
          icon: 'flashlight',
          usable: true,
          stackable: false,
          effects: {
            action: 'toggle_light',
          },
        },
      ],
      metadata: {
        description: 'Tool items',
        tags: ['items', 'tools'],
      },
    },
    {
      id: 'consumableItems',
      data: [
        {
          id: 'battery',
          name: 'Battery',
          description: 'A battery for powering electronic devices.',
          type: ItemType.CONSUMABLE,
          icon: 'battery',
          usable: true,
          stackable: true,
          maxStack: 10,
          effects: {
            action: 'recharge',
            amount: 25,
          },
        },
        {
          id: 'medkit',
          name: 'Medkit',
          description: 'A medical kit for treating injuries.',
          type: ItemType.CONSUMABLE,
          icon: 'medkit',
          usable: true,
          stackable: true,
          maxStack: 5,
          effects: {
            action: 'heal',
            amount: 50,
          },
        },
      ],
      metadata: {
        description: 'Consumable items',
        tags: ['items', 'consumables'],
      },
    },
    {
      id: 'documentItems',
      data: [
        {
          id: 'journal',
          name: 'Journal',
          description: 'A journal with notes about the area and the signal protocol.',
          type: ItemType.DOCUMENT,
          icon: 'journal',
          usable: true,
          stackable: false,
          effects: {
            action: 'read_note',
            content: 'The journal contains notes about the Signal Protocol. It mentions that the protocol requires four specific frequencies to be tuned in sequence: 91.5, 96.3, 103.7, and 105.1 MHz. The purpose of the protocol is unclear, but it seems to be related to communication with something beyond our understanding.',
          },
        },
        {
          id: 'note_tower',
          name: 'Tower Note',
          description: 'A note found at the radio tower.',
          type: ItemType.DOCUMENT,
          icon: 'note',
          usable: true,
          stackable: false,
          effects: {
            action: 'read_note',
            content: 'The note reads: "Something\'s happening at the research facility. They were experimenting with those frequencies again. I\'m going to investigate."',
          },
        },
        {
          id: 'note_ruins',
          name: 'Ruins Note',
          description: 'A note found at the ruins.',
          type: ItemType.DOCUMENT,
          icon: 'note',
          usable: true,
          stackable: false,
          effects: {
            action: 'read_note',
            content: 'The note reads: "The experiments with the Signal Protocol have yielded unexpected results. We\'ve made contact with something, but I\'m not sure we should continue. The frequencies seem to be opening a channel to... somewhere else."',
          },
        },
      ],
      metadata: {
        description: 'Document items',
        tags: ['items', 'documents'],
      },
    },
    {
      id: 'keyItems',
      data: [
        {
          id: 'key_bunker',
          name: 'Bunker Key',
          description: 'A key to the bunker entrance.',
          type: ItemType.KEY,
          icon: 'key_bunker',
          usable: false,
          stackable: false,
        },
        {
          id: 'key_lab',
          name: 'Lab Key',
          description: 'A key to the laboratory section of the ruins.',
          type: ItemType.KEY,
          icon: 'key_lab',
          usable: false,
          stackable: false,
        },
      ],
      metadata: {
        description: 'Key items',
        tags: ['items', 'keys'],
      },
    },
    {
      id: 'componentItems',
      data: [
        {
          id: 'radio_enhancer',
          name: 'Signal Enhancer',
          description: 'Enhances radio reception range.',
          type: ItemType.COMPONENT,
          icon: 'radio_enhancer',
          usable: true,
          stackable: false,
          effects: {
            action: 'enhance_radio',
          },
        },
        {
          id: 'radio_filter',
          name: 'Signal Filter',
          description: 'Filters out static and improves signal clarity.',
          type: ItemType.COMPONENT,
          icon: 'radio_filter',
          usable: true,
          stackable: false,
          effects: {
            action: 'filter_radio',
          },
        },
      ],
      metadata: {
        description: 'Component items',
        tags: ['items', 'components'],
      },
    },
  ],
  metadata: {
    description: 'Item fixtures for testing',
    author: 'Signal Lost Team',
    createdAt: '2023-06-01',
    updatedAt: '2023-06-15',
  },
};

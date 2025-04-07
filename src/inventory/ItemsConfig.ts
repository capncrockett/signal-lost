import { ItemData, ItemType } from './Item';

/**
 * Item definitions for the game
 */
export const itemsData: ItemData[] = [
  // Tools
  {
    id: 'radio',
    name: 'Radio',
    description: 'A portable radio that can pick up signals. Essential for finding locations.',
    type: ItemType.TOOL,
    icon: 'radio_item',
    usable: true,
    stackable: false,
    effects: {
      action: 'open_radio',
    },
  },
  {
    id: 'map',
    name: 'Map',
    description: 'A map of the area. Shows discovered locations.',
    type: ItemType.TOOL,
    icon: 'map_item',
    usable: true,
    stackable: false,
    effects: {
      action: 'open_map',
    },
  },
  {
    id: 'flashlight',
    name: 'Flashlight',
    description: 'A flashlight for exploring dark areas.',
    type: ItemType.TOOL,
    icon: 'flashlight_item',
    usable: true,
    stackable: false,
    effects: {
      action: 'toggle_light',
    },
  },
  
  // Documents
  {
    id: 'journal',
    name: 'Journal',
    description: 'A journal containing notes about your discoveries.',
    type: ItemType.DOCUMENT,
    icon: 'journal_item',
    usable: true,
    stackable: false,
    effects: {
      action: 'open_journal',
    },
  },
  {
    id: 'note_tower',
    name: 'Tower Note',
    description: 'A note found at the radio tower. It mentions something about a "signal protocol".',
    type: ItemType.DOCUMENT,
    icon: 'note_item',
    usable: true,
    stackable: false,
    effects: {
      action: 'read_note',
      content: 'The signal protocol requires three specific frequencies to be activated in sequence. The first is at 91.5 MHz. Find the others by exploring the ruins.',
    },
  },
  {
    id: 'note_ruins',
    name: 'Ruins Note',
    description: 'A note found in the ruins. It contains a partial frequency sequence.',
    type: ItemType.DOCUMENT,
    icon: 'note_item',
    usable: true,
    stackable: false,
    effects: {
      action: 'read_note',
      content: 'Second frequency: 96.3 MHz. The third frequency is hidden in the bunker. Be careful, the area is unstable.',
    },
  },
  
  // Keys
  {
    id: 'key_bunker',
    name: 'Bunker Key',
    description: 'A key that unlocks the bunker door.',
    type: ItemType.KEY,
    icon: 'key_item',
    usable: false,
    stackable: false,
  },
  {
    id: 'key_lab',
    name: 'Lab Key',
    description: 'A key that unlocks the laboratory door in the ruins.',
    type: ItemType.KEY,
    icon: 'key_item',
    usable: false,
    stackable: false,
  },
  
  // Radio Parts
  {
    id: 'radio_enhancer',
    name: 'Signal Enhancer',
    description: 'A device that enhances radio reception and range.',
    type: ItemType.RADIO_PART,
    icon: 'enhancer_item',
    usable: false,
    stackable: false,
    effects: {
      enhanceSignal: true,
      rangeBoost: 1.5,
    },
  },
  {
    id: 'radio_filter',
    name: 'Signal Filter',
    description: 'A filter that reduces static and interference in radio signals.',
    type: ItemType.RADIO_PART,
    icon: 'filter_item',
    usable: false,
    stackable: false,
    effects: {
      reduceStatic: true,
      clarityBoost: 2.0,
    },
  },
  
  // Consumables
  {
    id: 'battery',
    name: 'Battery',
    description: 'A battery for powering electronic devices.',
    type: ItemType.CONSUMABLE,
    icon: 'battery_item',
    usable: true,
    stackable: true,
    maxStack: 10,
    effects: {
      action: 'recharge',
      amount: 50,
    },
  },
  {
    id: 'medkit',
    name: 'Medkit',
    description: 'A medical kit for treating injuries.',
    type: ItemType.CONSUMABLE,
    icon: 'medkit_item',
    usable: true,
    stackable: true,
    maxStack: 5,
    effects: {
      action: 'heal',
      amount: 50,
    },
  },
];

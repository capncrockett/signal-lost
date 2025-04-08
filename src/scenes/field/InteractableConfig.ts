/**
 * Configuration for interactable objects in the field scene
 */

export interface InteractableConfig {
  id: string;
  type: string;
  x: number;
  y: number;
  triggerDistance: number;
}

/**
 * List of interactable objects in the field scene
 */
export const interactableData: InteractableConfig[] = [
  // Locations
  { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
  { id: 'tower2', type: 'tower', x: 20, y: 15, triggerDistance: 2 },
  { id: 'ruins1', type: 'ruins', x: 15, y: 12, triggerDistance: 1 },
  { id: 'bunker1', type: 'bunker', x: 5, y: 20, triggerDistance: 2 },

  // Items
  { id: 'note_tower', type: 'item', x: 11, y: 8, triggerDistance: 1 },
  { id: 'note_ruins', type: 'item', x: 16, y: 12, triggerDistance: 1 },
  { id: 'key_bunker', type: 'item', x: 17, y: 13, triggerDistance: 1 },
  { id: 'key_lab', type: 'item', x: 21, y: 16, triggerDistance: 1 },
  { id: 'radio_enhancer', type: 'item', x: 6, y: 21, triggerDistance: 1 },
  { id: 'radio_filter', type: 'item', x: 7, y: 21, triggerDistance: 1 },
  { id: 'battery', type: 'item', x: 12, y: 9, triggerDistance: 1 },
  { id: 'medkit', type: 'item', x: 19, y: 14, triggerDistance: 1 },
];

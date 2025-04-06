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
  { id: 'tower1', type: 'tower', x: 10, y: 8, triggerDistance: 2 },
  { id: 'tower2', type: 'tower', x: 20, y: 15, triggerDistance: 2 },
  { id: 'ruins1', type: 'ruins', x: 15, y: 12, triggerDistance: 1 },
];

/**
 * Enum for item types
 */
export enum ItemType {
  TOOL = 'tool',
  DOCUMENT = 'document',
  KEY = 'key',
  RADIO_PART = 'radio_part',
  CONSUMABLE = 'consumable',
}

/**
 * Interface for item effects
 */
export interface ItemEffects {
  action?: string;
  content?: string;
  enhanceSignal?: boolean;
  rangeBoost?: number;
  reduceStatic?: boolean;
  clarityBoost?: number;
  amount?: number;
}

/**
 * Interface for item data
 */
export interface ItemData {
  id: string;
  name: string;
  description: string;
  type: ItemType;
  icon: string;
  usable: boolean;
  stackable: boolean;
  maxStack?: number;
  effects?: ItemEffects;
}

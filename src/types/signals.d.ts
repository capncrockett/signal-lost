/**
 * Types for radio signal data
 */

/**
 * Base interface for all signal data
 */
export interface SignalLockData {
  id: string;
  frequency: number;
  signalType: string;
  signalId: string;
  signalStrength: number;
  signalData: LocationSignalData | MessageSignalData | ItemSignalData;
}

/**
 * Location signal data
 */
export interface LocationSignalData {
  locationId: string;
  coordinates: {
    x: number;
    y: number;
  };
}

/**
 * Message signal data
 */
export interface MessageSignalData {
  message: string;
}

/**
 * Item signal data
 */
export interface ItemSignalData {
  itemId: string;
  name: string;
  description: string;
}

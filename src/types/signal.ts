/**
 * Types for signal data and state management
 */

/**
 * Signal interface as defined in the contract between Alpha and Beta agents
 */
export interface Signal {
  id: string;
  frequency: number;
  strength: number;
  type: 'message' | 'location' | 'event';
  content: string;
  discovered: boolean;
  timestamp: number;
}

/**
 * Signal state for tracking discovered signals and their status
 */
export interface SignalState {
  signals: Record<string, Signal>;
  activeSignalId: string | null;
  discoveredSignalIds: string[];
  lastDiscoveredTimestamp: number | null;
}

/**
 * Initial state for signal tracking
 */
export const initialSignalState: SignalState = {
  signals: {},
  activeSignalId: null,
  discoveredSignalIds: [],
  lastDiscoveredTimestamp: null,
};

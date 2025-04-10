/**
 * Implementation for signal data and state management
 */
import { SignalState } from './signal.d';

/**
 * Initial state for signal tracking
 */
export const initialSignalState: SignalState = {
  signals: {},
  activeSignalId: null,
  discoveredSignalIds: [],
  lastDiscoveredTimestamp: null,
};

/**
 * Type definitions for signal data and state management
 */

/**
 * Signal interface as defined in the contract between Alpha and Beta agents
 *
 * This interface represents a radio signal that can be detected by the RadioTuner component
 * and processed by the narrative system. It serves as the primary communication channel
 * between Agent Alpha's radio components and Agent Beta's narrative components.
 */
interface Signal {
  /** Unique identifier for the signal */
  id: string;
  /** Frequency in MHz where the signal can be detected */
  frequency: number;
  /** Signal strength from 0.0 to 1.0 */
  strength: number;
  /** Type of signal content */
  type: 'message' | 'location' | 'event';
  /** Content identifier or actual content */
  content: string;
  /** Whether the signal has been discovered by the player */
  discovered: boolean;
  /** Timestamp when the signal was created or last updated */
  timestamp: number;
}

/**
 * Signal state for tracking discovered signals and their status
 */
interface SignalState {
  signals: Record<string, Signal>;
  activeSignalId: string | null;
  discoveredSignalIds: string[];
  lastDiscoveredTimestamp: number | null;
}

export { Signal, SignalState };

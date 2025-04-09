/**
 * Types for event data and state management
 */

/**
 * GameEvent interface as defined in the contract between Alpha and Beta agents
 */
export interface GameEvent {
  id: string;
  type: 'signal' | 'narrative' | 'system';
  payload: unknown;
  timestamp: number;
}

/**
 * Event state for tracking events and their history
 */
export interface EventState {
  events: Record<string, GameEvent>;
  eventHistory: string[]; // Array of event IDs in chronological order
  activeEventId: string | null;
  pendingEvents: string[]; // Events that need to be processed
}

/**
 * Initial state for event tracking
 */
export const initialEventState: EventState = {
  events: {},
  eventHistory: [],
  activeEventId: null,
  pendingEvents: [],
};

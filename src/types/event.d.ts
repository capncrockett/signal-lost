/**
 * Type definitions for event data and state management
 */

/**
 * GameEvent interface as defined in the contract between Alpha and Beta agents
 *
 * This interface represents an event that can be dispatched and handled by the game.
 * It serves as a communication mechanism between different components of the game,
 * particularly between Agent Alpha's radio/audio components and Agent Beta's narrative components.
 */
export interface GameEvent {
  /** Unique identifier for the event */
  id: string;
  /** Type of event */
  type: 'signal' | 'narrative' | 'system';
  /** Event data payload */
  payload: unknown;
  /** Timestamp when the event was created */
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

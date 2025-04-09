/**
 * Implementation for event data and state management
 */
import { EventState } from './event.d';

/**
 * Initial state for event tracking
 */
export const initialEventState: EventState = {
  events: {},
  eventHistory: [],
  activeEventId: null,
  pendingEvents: [],
};

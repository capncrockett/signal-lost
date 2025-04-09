import React, { createContext, useContext, useReducer, ReactNode, useEffect } from 'react';
import { GameEvent, EventState, initialEventState } from '../types/event';

// Define action types for event state management
type EventActionType =
  | { type: 'ADD_EVENT'; payload: GameEvent }
  | { type: 'SET_ACTIVE_EVENT'; payload: string | null }
  | { type: 'MARK_EVENT_PROCESSED'; payload: string }
  | { type: 'ADD_PENDING_EVENT'; payload: string }
  | { type: 'CLEAR_EVENT_HISTORY' }
  | { type: 'LOAD_STATE'; payload: unknown };

// Create the reducer function for event state
const eventStateReducer = (state: EventState, action: EventActionType): EventState => {
  switch (action.type) {
    case 'ADD_EVENT':
      return {
        ...state,
        events: {
          ...state.events,
          [action.payload.id]: action.payload,
        },
        eventHistory: [...state.eventHistory, action.payload.id],
      };
    case 'SET_ACTIVE_EVENT':
      return {
        ...state,
        activeEventId: action.payload,
      };
    case 'MARK_EVENT_PROCESSED':
      return {
        ...state,
        pendingEvents: state.pendingEvents.filter((id) => id !== action.payload),
      };
    case 'ADD_PENDING_EVENT':
      if (state.pendingEvents.includes(action.payload)) {
        return state;
      }
      return {
        ...state,
        pendingEvents: [...state.pendingEvents, action.payload],
      };
    case 'CLEAR_EVENT_HISTORY':
      return {
        ...state,
        eventHistory: [],
        pendingEvents: [],
      };
    case 'LOAD_STATE':
      // Validate and type-check the loaded state
      try {
        const loadedState = action.payload as EventState;
        // Ensure all required properties exist
        if (
          typeof loadedState.events === 'object' &&
          Array.isArray(loadedState.eventHistory) &&
          Array.isArray(loadedState.pendingEvents) &&
          (loadedState.activeEventId === null || typeof loadedState.activeEventId === 'string')
        ) {
          return loadedState;
        }
        console.error('Invalid state format when loading event state');
        return state;
      } catch (error) {
        console.error('Error loading event state:', error);
        return state;
      }
    default:
      return state;
  }
};

// Create the context type
interface EventContextType {
  state: EventState;
  dispatch: React.Dispatch<EventActionType>;
  addEvent: (event: GameEvent) => void;
  setActiveEvent: (id: string | null) => void;
  markEventProcessed: (id: string) => void;
  addPendingEvent: (id: string) => void;
  getEventById: (id: string) => GameEvent | undefined;
  getActiveEvent: () => GameEvent | undefined;
  getEventHistory: () => GameEvent[];
  getPendingEvents: () => GameEvent[];
  clearEventHistory: () => void;
  dispatchEvent: <T>(type: 'signal' | 'narrative' | 'system', payload: T) => string;
}

// Create the context
const EventContext = createContext<EventContextType | undefined>(undefined);

// Storage key for saving event state
const EVENT_STORAGE_KEY = 'signal-lost-event-state';

// Function to load state from localStorage
const loadEventState = (): EventState => {
  try {
    const savedState = localStorage.getItem(EVENT_STORAGE_KEY);
    if (savedState === null) {
      return initialEventState;
    }
    const parsedState = JSON.parse(savedState) as EventState;
    return parsedState;
  } catch (error) {
    console.error('Error loading event state:', error);
    return initialEventState;
  }
};

// Function to save state to localStorage
const saveEventState = (state: EventState): void => {
  try {
    localStorage.setItem(EVENT_STORAGE_KEY, JSON.stringify(state));
  } catch (error) {
    console.error('Error saving event state:', error);
  }
};

// Create a provider component
interface EventProviderProps {
  children: ReactNode;
  persistState?: boolean;
}

export const EventProvider: React.FC<EventProviderProps> = ({
  children,
  persistState = true,
}) => {
  // Initialize state from localStorage if persistState is true
  const [state, dispatch] = useReducer(
    eventStateReducer,
    persistState ? loadEventState() : initialEventState
  );

  // Save state to localStorage whenever it changes
  useEffect(() => {
    if (persistState) {
      saveEventState(state);
    }
  }, [state, persistState]);

  // Helper functions for common operations
  const addEvent = (event: GameEvent): void => {
    dispatch({ type: 'ADD_EVENT', payload: event });
  };

  const setActiveEvent = (id: string | null): void => {
    dispatch({ type: 'SET_ACTIVE_EVENT', payload: id });
  };

  const markEventProcessed = (id: string): void => {
    dispatch({ type: 'MARK_EVENT_PROCESSED', payload: id });
  };

  const addPendingEvent = (id: string): void => {
    dispatch({ type: 'ADD_PENDING_EVENT', payload: id });
  };

  const getEventById = (id: string): GameEvent | undefined => {
    return state.events[id];
  };

  const getActiveEvent = (): GameEvent | undefined => {
    return state.activeEventId ? state.events[state.activeEventId] : undefined;
  };

  const getEventHistory = (): GameEvent[] => {
    return state.eventHistory
      .map((id) => state.events[id])
      .filter((event): event is GameEvent => event !== undefined);
  };

  const getPendingEvents = (): GameEvent[] => {
    return state.pendingEvents
      .map((id) => state.events[id])
      .filter((event): event is GameEvent => event !== undefined);
  };

  const clearEventHistory = (): void => {
    dispatch({ type: 'CLEAR_EVENT_HISTORY' });
  };

  // Function to create and dispatch a new event
  const dispatchEvent = <T,>(type: 'signal' | 'narrative' | 'system', payload: T): string => {
    const id = `${type}_${Date.now()}_${Math.floor(Math.random() * 1000)}`;
    const event: GameEvent = {
      id,
      type,
      payload,
      timestamp: Date.now(),
    };

    addEvent(event);
    addPendingEvent(id);

    return id;
  };

  return (
    <EventContext.Provider
      value={{
        state,
        dispatch,
        addEvent,
        setActiveEvent,
        markEventProcessed,
        addPendingEvent,
        getEventById,
        getActiveEvent,
        getEventHistory,
        getPendingEvents,
        clearEventHistory,
        dispatchEvent,
      }}
    >
      {children}
    </EventContext.Provider>
  );
};

// Create a custom hook for using the event state
export const useEvent = (): EventContextType => {
  const context = useContext(EventContext);
  if (context === undefined) {
    throw new Error('useEvent must be used within an EventProvider');
  }
  return context;
};

export default EventContext;

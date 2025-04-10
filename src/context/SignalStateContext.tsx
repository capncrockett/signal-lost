import React, { createContext, useContext, useReducer, ReactNode, useEffect } from 'react';
import { initialSignalState } from '../types/signal';
import { Signal, SignalState } from '../types/signal.d';

// Define action types for signal state management
type SignalActionType =
  | { type: 'ADD_SIGNAL'; payload: Signal }
  | { type: 'UPDATE_SIGNAL'; payload: { id: string; updates: Partial<Signal> } }
  | { type: 'SET_ACTIVE_SIGNAL'; payload: string | null }
  | { type: 'DISCOVER_SIGNAL'; payload: string }
  | { type: 'RESET_SIGNALS' }
  | { type: 'LOAD_STATE'; payload: unknown };

// Create the reducer function for signal state
const signalStateReducer = (state: SignalState, action: SignalActionType): SignalState => {
  switch (action.type) {
    case 'ADD_SIGNAL':
      return {
        ...state,
        signals: {
          ...state.signals,
          [action.payload.id]: action.payload,
        },
      };
    case 'UPDATE_SIGNAL':
      if (!state.signals[action.payload.id]) {
        return state;
      }
      return {
        ...state,
        signals: {
          ...state.signals,
          [action.payload.id]: {
            ...state.signals[action.payload.id],
            ...action.payload.updates,
          },
        },
      };
    case 'SET_ACTIVE_SIGNAL':
      return {
        ...state,
        activeSignalId: action.payload,
      };
    case 'DISCOVER_SIGNAL':
      if (state.discoveredSignalIds.includes(action.payload)) {
        return state;
      }
      return {
        ...state,
        discoveredSignalIds: [...state.discoveredSignalIds, action.payload],
        lastDiscoveredTimestamp: Date.now(),
      };
    case 'RESET_SIGNALS':
      return initialSignalState;
    case 'LOAD_STATE':
      // Validate and type-check the loaded state
      try {
        const loadedState = action.payload as SignalState;
        // Ensure all required properties exist
        if (
          typeof loadedState.signals === 'object' &&
          Array.isArray(loadedState.discoveredSignalIds) &&
          (loadedState.activeSignalId === null || typeof loadedState.activeSignalId === 'string') &&
          (loadedState.lastDiscoveredTimestamp === null ||
            typeof loadedState.lastDiscoveredTimestamp === 'number')
        ) {
          return loadedState;
        }
        console.error('Invalid state format when loading signal state');
        return state;
      } catch (error) {
        console.error('Error loading signal state:', error);
        return state;
      }
    default:
      return state;
  }
};

// Create the context type
interface SignalStateContextType {
  state: SignalState;
  dispatch: React.Dispatch<SignalActionType>;
  addSignal: (signal: Signal) => void;
  updateSignal: (id: string, updates: Partial<Signal>) => void;
  setActiveSignal: (id: string | null) => void;
  discoverSignal: (id: string) => void;
  getSignalById: (id: string) => Signal | undefined;
  getActiveSignal: () => Signal | undefined;
  getDiscoveredSignals: () => Signal[];
  resetSignals: () => void;
}

// Create the context
const SignalStateContext = createContext<SignalStateContextType | undefined>(undefined);

// Storage key for saving signal state
const SIGNAL_STORAGE_KEY = 'signal-lost-signal-state';

// Function to load state from localStorage
const loadSignalState = (): SignalState => {
  try {
    const savedState = localStorage.getItem(SIGNAL_STORAGE_KEY);
    if (savedState === null) {
      return initialSignalState;
    }
    const parsedState = JSON.parse(savedState) as SignalState;
    return parsedState;
  } catch (error) {
    console.error('Error loading signal state:', error);
    return initialSignalState;
  }
};

// Function to save state to localStorage
const saveSignalState = (state: SignalState): void => {
  try {
    localStorage.setItem(SIGNAL_STORAGE_KEY, JSON.stringify(state));
  } catch (error) {
    console.error('Error saving signal state:', error);
  }
};

// Create a provider component
interface SignalStateProviderProps {
  children: ReactNode;
  persistState?: boolean;
}

const SignalStateProvider: React.FC<SignalStateProviderProps> = ({
  children,
  persistState = true,
}) => {
  // Initialize state from localStorage if persistState is true
  const [state, dispatch] = useReducer(
    signalStateReducer,
    persistState ? loadSignalState() : initialSignalState
  );

  // Save state to localStorage whenever it changes
  useEffect(() => {
    if (persistState) {
      saveSignalState(state);
    }
  }, [state, persistState]);

  // Helper functions for common operations
  const addSignal = (signal: Signal): void => {
    dispatch({ type: 'ADD_SIGNAL', payload: signal });
  };

  const updateSignal = (id: string, updates: Partial<Signal>): void => {
    dispatch({ type: 'UPDATE_SIGNAL', payload: { id, updates } });
  };

  const setActiveSignal = (id: string | null): void => {
    dispatch({ type: 'SET_ACTIVE_SIGNAL', payload: id });
  };

  const discoverSignal = (id: string): void => {
    dispatch({ type: 'DISCOVER_SIGNAL', payload: id });

    // Also mark the signal as discovered in the signals object
    if (state.signals[id]) {
      updateSignal(id, { discovered: true });
    }
  };

  const getSignalById = (id: string): Signal | undefined => {
    return state.signals[id];
  };

  const getActiveSignal = (): Signal | undefined => {
    return state.activeSignalId ? state.signals[state.activeSignalId] : undefined;
  };

  const getDiscoveredSignals = (): Signal[] => {
    return state.discoveredSignalIds
      .map((id) => state.signals[id])
      .filter((signal): signal is Signal => signal !== undefined);
  };

  const resetSignals = (): void => {
    dispatch({ type: 'RESET_SIGNALS' });
  };

  return (
    <SignalStateContext.Provider
      value={{
        state,
        dispatch,
        addSignal,
        updateSignal,
        setActiveSignal,
        discoverSignal,
        getSignalById,
        getActiveSignal,
        getDiscoveredSignals,
        resetSignals,
      }}
    >
      {children}
    </SignalStateContext.Provider>
  );
};

// Create a custom hook for using the signal state
export const useSignalState = (): SignalStateContextType => {
  const context = useContext(SignalStateContext);
  if (context === undefined) {
    throw new Error('useSignalState must be used within a SignalStateProvider');
  }
  return context;
};

export default SignalStateProvider;

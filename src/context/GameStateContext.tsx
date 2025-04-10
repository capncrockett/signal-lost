import React, { createContext, useContext, useReducer, ReactNode, useEffect } from 'react';

// Define the shape of our game state
interface GameState {
  currentFrequency: number;
  discoveredFrequencies: number[];
  currentLocation: string;
  inventory: string[];
  gameProgress: number;
  isRadioOn: boolean;
}

// Define the initial state
const initialState: GameState = {
  currentFrequency: 90.0,
  discoveredFrequencies: [],
  currentLocation: 'home',
  inventory: [],
  gameProgress: 0,
  isRadioOn: false,
};

// Define action types
type ActionType =
  | { type: 'SET_FREQUENCY'; payload: number }
  | { type: 'ADD_DISCOVERED_FREQUENCY'; payload: number }
  | { type: 'SET_LOCATION'; payload: string }
  | { type: 'ADD_INVENTORY_ITEM'; payload: string }
  | { type: 'REMOVE_INVENTORY_ITEM'; payload: string }
  | { type: 'SET_GAME_PROGRESS'; payload: number }
  | { type: 'TOGGLE_RADIO' }
  | { type: 'RESET_STATE' }
  | { type: 'LOAD_STATE'; payload: unknown };

// Create the reducer function
const gameStateReducer = (state: GameState, action: ActionType): GameState => {
  switch (action.type) {
    case 'SET_FREQUENCY':
      return {
        ...state,
        currentFrequency: action.payload,
      };
    case 'ADD_DISCOVERED_FREQUENCY':
      if (state.discoveredFrequencies.includes(action.payload)) {
        return state;
      }
      return {
        ...state,
        discoveredFrequencies: [...state.discoveredFrequencies, action.payload],
      };
    case 'SET_LOCATION':
      return {
        ...state,
        currentLocation: action.payload,
      };
    case 'ADD_INVENTORY_ITEM':
      if (state.inventory.includes(action.payload)) {
        return state;
      }
      return {
        ...state,
        inventory: [...state.inventory, action.payload],
      };
    case 'REMOVE_INVENTORY_ITEM':
      return {
        ...state,
        inventory: state.inventory.filter((item) => item !== action.payload),
      };
    case 'SET_GAME_PROGRESS':
      return {
        ...state,
        gameProgress: action.payload,
      };
    case 'TOGGLE_RADIO':
      return {
        ...state,
        isRadioOn: !state.isRadioOn,
      };
    case 'RESET_STATE':
      return initialState;
    case 'LOAD_STATE':
      // Validate and type-check the loaded state
      try {
        const loadedState = action.payload as GameState;
        // Ensure all required properties exist
        if (
          typeof loadedState.currentFrequency === 'number' &&
          Array.isArray(loadedState.discoveredFrequencies) &&
          typeof loadedState.currentLocation === 'string' &&
          Array.isArray(loadedState.inventory) &&
          typeof loadedState.gameProgress === 'number' &&
          typeof loadedState.isRadioOn === 'boolean'
        ) {
          return loadedState;
        }
        console.error('Invalid state format when loading game state');
        return state;
      } catch (error) {
        console.error('Error loading game state:', error);
        return state;
      }
    default:
      return state;
  }
};

// Create the context
interface GameStateContextType {
  state: GameState;
  dispatch: React.Dispatch<ActionType>;
}

const GameStateContext = createContext<GameStateContextType | undefined>(undefined);

// Storage key for saving game state
const STORAGE_KEY = 'signal-lost-game-state';

// Function to load state from localStorage
const loadState = (): GameState => {
  try {
    const savedState = localStorage.getItem(STORAGE_KEY);
    if (savedState === null) {
      return initialState;
    }
    const parsedState = JSON.parse(savedState) as GameState;
    return parsedState;
  } catch (error) {
    console.error('Error loading game state:', error);
    return initialState;
  }
};

// Function to save state to localStorage
const saveState = (state: GameState): void => {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
  } catch (error) {
    console.error('Error saving game state:', error);
  }
};

// Create a provider component
interface GameStateProviderProps {
  children: ReactNode;
  persistState?: boolean;
}

const GameStateProvider: React.FC<GameStateProviderProps> = ({ children, persistState = true }) => {
  // Initialize state from localStorage if persistState is true
  const [state, dispatch] = useReducer(gameStateReducer, persistState ? loadState() : initialState);

  // Save state to localStorage whenever it changes
  useEffect(() => {
    if (persistState) {
      saveState(state);
    }
  }, [state, persistState]);

  return (
    <GameStateContext.Provider value={{ state, dispatch }}>{children}</GameStateContext.Provider>
  );
};

// Create a custom hook for using the game state
export const useGameState = (): GameStateContextType => {
  const context = useContext(GameStateContext);
  if (context === undefined) {
    throw new Error('useGameState must be used within a GameStateProvider');
  }
  return context;
};

// Function to clear saved game state
export const clearSavedGameState = (): void => {
  try {
    localStorage.removeItem(STORAGE_KEY);
  } catch (error) {
    console.error('Error clearing game state:', error);
  }
};

// Custom hook for resetting the game state
export const useResetGameState = (): (() => void) => {
  const { dispatch } = useGameState();

  return () => {
    clearSavedGameState();
    dispatch({ type: 'RESET_STATE' });
  };
};

export default GameStateProvider;

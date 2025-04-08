import React, { createContext, useContext, useReducer, ReactNode } from 'react';

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
  | { type: 'TOGGLE_RADIO' };

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

// Create a provider component
interface GameStateProviderProps {
  children: ReactNode;
}

export const GameStateProvider: React.FC<GameStateProviderProps> = ({ children }) => {
  const [state, dispatch] = useReducer(gameStateReducer, initialState);

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

export default GameStateContext;

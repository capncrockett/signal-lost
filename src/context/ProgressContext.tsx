import React, { createContext, useContext, useReducer, ReactNode, useEffect } from 'react';
import { Objective, ProgressState, initialProgressState } from '../types/progress';

// Define action types for progress state management
type ProgressActionType =
  | { type: 'SET_PROGRESS'; payload: number }
  | { type: 'ADD_OBJECTIVE'; payload: Objective }
  | { type: 'COMPLETE_OBJECTIVE'; payload: string }
  | { type: 'RESET_PROGRESS' }
  | { type: 'LOAD_STATE'; payload: unknown };

// Create the reducer function for progress state
const progressStateReducer = (state: ProgressState, action: ProgressActionType): ProgressState => {
  switch (action.type) {
    case 'SET_PROGRESS':
      return {
        ...state,
        currentProgress: action.payload,
      };
    case 'ADD_OBJECTIVE':
      return {
        ...state,
        objectives: {
          ...state.objectives,
          [action.payload.id]: action.payload,
        },
      };
    case 'COMPLETE_OBJECTIVE':
      if (
        !state.objectives[action.payload] ||
        state.completedObjectiveIds.includes(action.payload)
      ) {
        return state;
      }

      {
        // Check if this objective depends on others that aren't completed yet
        const objective = state.objectives[action.payload];
        if (objective.dependsOn && objective.dependsOn.length > 0) {
          const allDependenciesCompleted = objective.dependsOn.every((dependencyId) =>
            state.completedObjectiveIds.includes(dependencyId)
          );
          if (!allDependenciesCompleted) {
            return state;
          }
        }
      }

      {
        // Mark the objective as completed
        const now = Date.now();
        return {
          ...state,
          objectives: {
            ...state.objectives,
            [action.payload]: {
              ...state.objectives[action.payload],
              isCompleted: true,
              completedAt: now,
            },
          },
          completedObjectiveIds: [...state.completedObjectiveIds, action.payload],
          lastCompletedObjectiveId: action.payload,
          lastCompletedTimestamp: now,
        };
      }
    case 'RESET_PROGRESS':
      return initialProgressState;
    case 'LOAD_STATE':
      // Validate and type-check the loaded state
      try {
        const loadedState = action.payload as ProgressState;
        // Ensure all required properties exist
        if (
          typeof loadedState.currentProgress === 'number' &&
          typeof loadedState.objectives === 'object' &&
          Array.isArray(loadedState.completedObjectiveIds) &&
          (loadedState.lastCompletedObjectiveId === null ||
            typeof loadedState.lastCompletedObjectiveId === 'string') &&
          (loadedState.lastCompletedTimestamp === null ||
            typeof loadedState.lastCompletedTimestamp === 'number')
        ) {
          return loadedState;
        }
        console.error('Invalid state format when loading progress state');
        return state;
      } catch (error) {
        console.error('Error loading progress state:', error);
        return state;
      }
    default:
      return state;
  }
};

// Create the context type
interface ProgressContextType {
  state: ProgressState;
  dispatch: React.Dispatch<ProgressActionType>;
  setProgress: (progress: number) => void;
  addObjective: (objective: Objective) => void;
  completeObjective: (id: string) => void;
  getObjectiveById: (id: string) => Objective | undefined;
  getCompletedObjectives: () => Objective[];
  getPendingObjectives: () => Objective[];
  getAvailableObjectives: () => Objective[];
  resetProgress: () => void;
  isObjectiveCompleted: (id: string) => boolean;
  isObjectiveAvailable: (id: string) => boolean;
}

// Create the context
const ProgressContext = createContext<ProgressContextType | undefined>(undefined);

// Storage key for saving progress state
const PROGRESS_STORAGE_KEY = 'signal-lost-progress-state';

// Function to load state from localStorage
const loadProgressState = (): ProgressState => {
  try {
    const savedState = localStorage.getItem(PROGRESS_STORAGE_KEY);
    if (savedState === null) {
      return initialProgressState;
    }
    const parsedState = JSON.parse(savedState) as ProgressState;
    return parsedState;
  } catch (error) {
    console.error('Error loading progress state:', error);
    return initialProgressState;
  }
};

// Function to save state to localStorage
const saveProgressState = (state: ProgressState): void => {
  try {
    localStorage.setItem(PROGRESS_STORAGE_KEY, JSON.stringify(state));
  } catch (error) {
    console.error('Error saving progress state:', error);
  }
};

// Create a provider component
interface ProgressProviderProps {
  children: ReactNode;
  persistState?: boolean;
}

const ProgressProvider: React.FC<ProgressProviderProps> = ({ children, persistState = true }) => {
  // Initialize state from localStorage if persistState is true
  const [state, dispatch] = useReducer(
    progressStateReducer,
    persistState ? loadProgressState() : initialProgressState
  );

  // Save state to localStorage whenever it changes
  useEffect(() => {
    if (persistState) {
      saveProgressState(state);
    }
  }, [state, persistState]);

  // Helper functions for common operations
  const setProgress = (progress: number): void => {
    dispatch({ type: 'SET_PROGRESS', payload: progress });
  };

  const addObjective = (objective: Objective): void => {
    dispatch({ type: 'ADD_OBJECTIVE', payload: objective });
  };

  const completeObjective = (id: string): void => {
    dispatch({ type: 'COMPLETE_OBJECTIVE', payload: id });
  };

  const getObjectiveById = (id: string): Objective | undefined => {
    return state.objectives[id];
  };

  const getCompletedObjectives = (): Objective[] => {
    return state.completedObjectiveIds
      .map((id) => state.objectives[id])
      .filter((objective): objective is Objective => objective !== undefined);
  };

  const getPendingObjectives = (): Objective[] => {
    return Object.values(state.objectives).filter((objective) => !objective.isCompleted);
  };

  const getAvailableObjectives = (): Objective[] => {
    return Object.values(state.objectives).filter((objective) => {
      // If already completed, it's not available
      if (objective.isCompleted) {
        return false;
      }

      // If it depends on other objectives, check if they're all completed
      if (objective.dependsOn && objective.dependsOn.length > 0) {
        return objective.dependsOn.every((dependencyId) =>
          state.completedObjectiveIds.includes(dependencyId)
        );
      }

      // If it has a required progress level, check if we've reached it
      if (objective.requiredProgress > 0) {
        return state.currentProgress >= objective.requiredProgress;
      }

      // Otherwise, it's available
      return true;
    });
  };

  const resetProgress = (): void => {
    dispatch({ type: 'RESET_PROGRESS' });
  };

  const isObjectiveCompleted = (id: string): boolean => {
    return state.completedObjectiveIds.includes(id);
  };

  const isObjectiveAvailable = (id: string): boolean => {
    const objective = state.objectives[id];
    if (!objective || objective.isCompleted) {
      return false;
    }

    // Check dependencies
    if (objective.dependsOn && objective.dependsOn.length > 0) {
      return objective.dependsOn.every((dependencyId) =>
        state.completedObjectiveIds.includes(dependencyId)
      );
    }

    // Check required progress
    if (objective.requiredProgress > 0) {
      return state.currentProgress >= objective.requiredProgress;
    }

    return true;
  };

  return (
    <ProgressContext.Provider
      value={{
        state,
        dispatch,
        setProgress,
        addObjective,
        completeObjective,
        getObjectiveById,
        getCompletedObjectives,
        getPendingObjectives,
        getAvailableObjectives,
        resetProgress,
        isObjectiveCompleted,
        isObjectiveAvailable,
      }}
    >
      {children}
    </ProgressContext.Provider>
  );
};

// Create a custom hook for using the progress state
export const useProgress = (): ProgressContextType => {
  const context = useContext(ProgressContext);
  if (context === undefined) {
    throw new Error('useProgress must be used within a ProgressProvider');
  }
  return context;
};

export default ProgressProvider;

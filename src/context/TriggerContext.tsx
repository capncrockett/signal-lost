import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { Trigger, TriggerState, initialTriggerState } from '../utils/ConditionalTrigger';

// Define the context type
interface TriggerContextType {
  triggers: Trigger[];
  triggerState: TriggerState;
  setTriggerState: React.Dispatch<React.SetStateAction<TriggerState>>;
  addTrigger: (trigger: Trigger) => void;
  removeTrigger: (triggerId: string) => void;
  resetTriggerState: () => void;
}

// Create the context
const TriggerContext = createContext<TriggerContextType | undefined>(undefined);

// Storage key for saving trigger state
const TRIGGER_STORAGE_KEY = 'signal-lost-trigger-state';

// Function to load state from localStorage
const loadTriggerState = (): TriggerState => {
  try {
    const savedState = localStorage.getItem(TRIGGER_STORAGE_KEY);
    if (savedState === null) {
      return initialTriggerState;
    }
    const parsedState = JSON.parse(savedState) as TriggerState;
    return parsedState;
  } catch (error) {
    console.error('Error loading trigger state:', error);
    return initialTriggerState;
  }
};

// Function to save state to localStorage
const saveTriggerState = (state: TriggerState): void => {
  try {
    localStorage.setItem(TRIGGER_STORAGE_KEY, JSON.stringify(state));
  } catch (error) {
    console.error('Error saving trigger state:', error);
  }
};

// Create a provider component
interface TriggerProviderProps {
  children: ReactNode;
  initialTriggers?: Trigger[];
  persistState?: boolean;
}

export const TriggerProvider: React.FC<TriggerProviderProps> = ({
  children,
  initialTriggers = [],
  persistState = true,
}) => {
  // Initialize triggers state
  const [triggers, setTriggers] = useState<Trigger[]>(initialTriggers);

  // Initialize trigger state from localStorage if persistState is true
  const [triggerState, setTriggerState] = useState<TriggerState>(
    persistState ? loadTriggerState() : initialTriggerState
  );

  // Save trigger state to localStorage whenever it changes
  useEffect(() => {
    if (persistState) {
      saveTriggerState(triggerState);
    }
  }, [triggerState, persistState]);

  // Helper functions for managing triggers
  const addTrigger = (trigger: Trigger): void => {
    setTriggers((prevTriggers) => {
      // Check if trigger with same ID already exists
      const existingIndex = prevTriggers.findIndex((t) => t.id === trigger.id);
      if (existingIndex >= 0) {
        // Replace existing trigger
        const newTriggers = [...prevTriggers];
        newTriggers[existingIndex] = trigger;
        return newTriggers;
      }
      // Add new trigger
      return [...prevTriggers, trigger];
    });
  };

  const removeTrigger = (triggerId: string): void => {
    setTriggers((prevTriggers) => prevTriggers.filter((trigger) => trigger.id !== triggerId));
  };

  const resetTriggerState = (): void => {
    setTriggerState(initialTriggerState);
  };

  return (
    <TriggerContext.Provider
      value={{
        triggers,
        triggerState,
        setTriggerState,
        addTrigger,
        removeTrigger,
        resetTriggerState,
      }}
    >
      {children}
    </TriggerContext.Provider>
  );
};

// Create a custom hook for using the trigger context
export const useTrigger = (): TriggerContextType => {
  const context = useContext(TriggerContext);
  if (context === undefined) {
    throw new Error('useTrigger must be used within a TriggerProvider');
  }
  return context;
};

export default TriggerContext;

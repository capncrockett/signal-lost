import React, { useEffect, useState, useCallback, useRef } from 'react';
import { useGameState } from '../../context/GameStateContext';
import { useSignalState } from '../../context/SignalStateContext';
import { useEvent } from '../../context/EventContext';
import { useProgress } from '../../context/ProgressContext';
import {
  Trigger,
  TriggerState,
  initialTriggerState,
  processTriggers,
} from '../../utils/ConditionalTrigger';

interface TriggerManagerProps {
  triggers: Trigger[];
  checkInterval?: number; // How often to check triggers in milliseconds
  children?: React.ReactNode;
}

/**
 * TriggerManager component that processes conditional triggers based on game state
 * This component doesn't render anything visible but manages the trigger system
 */
const TriggerManager: React.FC<TriggerManagerProps> = ({
  triggers,
  checkInterval = 1000,
  children,
}) => {
  const { state: gameState } = useGameState();
  const { state: signalState } = useSignalState();
  const { state: progressState } = useProgress();
  const { dispatchEvent } = useEvent();

  // Local state for trigger tracking
  const [triggerState, setTriggerState] = useState<TriggerState>(initialTriggerState);

  // Use a ref to store the current trigger state to avoid dependency cycles
  const triggerStateRef = useRef<TriggerState>(triggerState);

  // Update the ref whenever the state changes
  useEffect(() => {
    triggerStateRef.current = triggerState;
  }, [triggerState]);

  // Process triggers based on current state
  const processTriggerEvents = useCallback(() => {
    // Combine all state contexts into a single state object
    const combinedState = {
      gameState,
      signalState,
      progressState,
    };

    // Create a copy of the current trigger state to modify
    const newTriggerState = { ...triggerStateRef.current };

    // Process triggers and get events to dispatch
    const eventsToDispatch = processTriggers(triggers, combinedState, newTriggerState);

    // Dispatch events
    eventsToDispatch.forEach((event) => {
      dispatchEvent(event.type, event.payload);
    });

    // Update trigger state if any events were dispatched
    if (eventsToDispatch.length > 0) {
      setTriggerState(newTriggerState);
    }
  }, [gameState, signalState, progressState, triggers, dispatchEvent]);

  // Set up interval to check triggers
  useEffect(() => {
    // Process triggers immediately on mount or when dependencies change
    processTriggerEvents();

    // Set up interval for periodic checks
    const intervalId = setInterval(processTriggerEvents, checkInterval);

    // Clean up interval on unmount
    return () => {
      clearInterval(intervalId);
    };
  }, [processTriggerEvents, checkInterval]);

  // This component doesn't render anything visible
  return <>{children}</>;
};

export default TriggerManager;

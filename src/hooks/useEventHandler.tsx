import { useState, useEffect, useCallback } from 'react';
import { useGameState } from '../context/GameStateContext';
import { useSignalState } from '../context/SignalStateContext';
import { useProgress } from '../context/ProgressContext';
import { useEvent } from '../context/EventContext';
import { EventHandler, EventTrigger } from '../utils/EventHandler';

/**
 * Custom hook for using the EventHandler
 * @param autoProcessPending Whether to automatically process pending events
 * @returns The EventHandler instance and utility functions
 */
export const useEventHandler = (autoProcessPending = true) => {
  const gameState = useGameState();
  const signalState = useSignalState();
  const progressState = useProgress();
  const eventState = useEvent();
  
  const [eventHandler] = useState(() => new EventHandler(
    gameState,
    signalState,
    progressState,
    eventState
  ));

  // Process pending events whenever they change
  useEffect(() => {
    if (autoProcessPending && eventState.state.pendingEvents.length > 0) {
      eventHandler.processPendingEvents();
    }
  }, [eventHandler, eventState.state.pendingEvents, autoProcessPending]);

  // Load triggers from a JSON file
  const loadTriggers = useCallback(async (url: string) => {
    await eventHandler.loadTriggersFromJson(url);
  }, [eventHandler]);

  // Register a single trigger
  const registerTrigger = useCallback((trigger: EventTrigger) => {
    eventHandler.registerTrigger(trigger);
  }, [eventHandler]);

  // Register multiple triggers
  const registerTriggers = useCallback((triggers: EventTrigger[]) => {
    eventHandler.registerTriggers(triggers);
  }, [eventHandler]);

  // Set a variable
  const setVariable = useCallback((name: string, value: unknown) => {
    eventHandler.setVariable(name, value);
  }, [eventHandler]);

  // Get a variable
  const getVariable = useCallback((name: string) => {
    return eventHandler.getVariable(name);
  }, [eventHandler]);

  // Process a specific event
  const processEvent = useCallback((eventId: string) => {
    const event = eventState.getEventById(eventId);
    if (event) {
      return eventHandler.processEvent(event);
    }
    return [];
  }, [eventHandler, eventState]);

  // Process all pending events
  const processPendingEvents = useCallback(() => {
    return eventHandler.processPendingEvents();
  }, [eventHandler]);

  return {
    eventHandler,
    loadTriggers,
    registerTrigger,
    registerTriggers,
    setVariable,
    getVariable,
    processEvent,
    processPendingEvents,
  };
};

export default useEventHandler;

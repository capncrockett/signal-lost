import React, { useEffect, useState } from 'react';
import { useEvent } from '../../context/EventContext';
import { useEventHandler } from '../../hooks/useEventHandler';
import { GameEvent } from '../../types/event.d';

interface EventManagerProps {
  triggerUrls?: string[];
  onEventProcessed?: (event: GameEvent, executedTriggers: string[]) => void;
  autoProcessPending?: boolean;
}

/**
 * EventManager component that handles event processing and conditional triggers
 * This component should be mounted once in the app to handle all events
 */
const EventManager: React.FC<EventManagerProps> = ({
  triggerUrls = [],
  onEventProcessed,
  autoProcessPending = true,
}) => {
  const { getPendingEvents } = useEvent();
  const { loadTriggers, processEvent } = useEventHandler(false); // We'll handle processing manually

  const [isInitialized, setIsInitialized] = useState(false);

  // Load triggers from provided URLs
  useEffect(() => {
    const loadAllTriggers = async (): Promise<void> => {
      try {
        await Promise.all(triggerUrls.map((url) => loadTriggers(url)));
        setIsInitialized(true);
      } catch (error) {
        console.error('Error loading event triggers:', error);
      }
    };

    void loadAllTriggers();
  }, [loadTriggers, triggerUrls]);

  // Process pending events when they change
  useEffect(() => {
    if (!isInitialized || !autoProcessPending) return;

    const pendingEvents = getPendingEvents();
    if (pendingEvents.length === 0) return;

    // Process each pending event
    pendingEvents.forEach((event) => {
      const executedTriggers = processEvent(event.id);
      if (onEventProcessed) {
        onEventProcessed(event, executedTriggers);
      }
    });
  }, [isInitialized, autoProcessPending, getPendingEvents, processEvent, onEventProcessed]);

  // This component doesn't render anything
  return null;
};

export default EventManager;

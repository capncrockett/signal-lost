import React, { useEffect, useState } from 'react';
import { useGameState } from '../../context/GameStateContext';
import { useSignalState } from '../../context/SignalStateContext';
import { useProgress } from '../../context/ProgressContext';
import { useEvent } from '../../context/EventContext';
import EventManager from './EventManager';
import SignalTracker from './SignalTracker';
import ProgressTracker from './ProgressTracker';
import MessageHistoryManager from '../narrative/MessageHistoryManager';
import { SaveManager } from '../../utils/SaveManager';
import { Signal } from '../../types/signal.d';
import { Objective } from '../../types/progress';
import { MessageHistoryEntry } from '../narrative/MessageHistoryManager';

interface GameStateIntegrationProps {
  triggerUrls?: string[];
  autoSaveInterval?: number; // in milliseconds, 0 to disable
  onSignalDiscovered?: (signal: Signal) => void;
  onObjectiveCompleted?: (objective: Objective) => void;
  onMessageHistoryUpdated?: (messages: MessageHistoryEntry[]) => void;
  onAutoSave?: (saveId: string) => void;
}

/**
 * GameStateIntegration component that integrates all game state systems
 * This component should be mounted once in the app to handle all game state integration
 */
const GameStateIntegration: React.FC<GameStateIntegrationProps> = ({
  triggerUrls = ['/assets/narrative/events.json'],
  autoSaveInterval = 5 * 60 * 1000, // 5 minutes
  onSignalDiscovered,
  onObjectiveCompleted,
  onMessageHistoryUpdated,
  // onAutoSave callback is reserved for future use
}) => {
  const gameState = useGameState();
  const signalState = useSignalState();
  const progressState = useProgress();
  const eventState = useEvent();

  const [saveManager] = useState(
    () =>
      new SaveManager({
        autoSaveInterval,
        maxSaveSlots: 5,
      })
  );

  // Set up auto-save
  useEffect(() => {
    if (autoSaveInterval <= 0) return;

    saveManager.startAutoSave(() => {
      const saveData = {
        gameState: gameState.state,
        signalState: signalState.state,
        eventState: eventState.state,
        progressState: progressState.state,
      };

      return saveData;
    });

    // Clean up auto-save on unmount
    return () => {
      saveManager.stopAutoSave();
    };
  }, [
    saveManager,
    autoSaveInterval,
    gameState.state,
    signalState.state,
    eventState.state,
    progressState.state,
  ]);

  // Handle signal discovery notifications
  const handleSignalDiscovered = (signal: Signal): void => {
    console.log(`Signal discovered: ${signal.id} at ${signal.frequency} MHz`);

    if (onSignalDiscovered) {
      onSignalDiscovered(signal);
    }
  };

  // Handle objective completion notifications
  const handleObjectiveCompleted = (objective: Objective): void => {
    console.log(`Objective completed: ${objective.title}`);

    if (onObjectiveCompleted) {
      onObjectiveCompleted(objective);
    }
  };

  // Handle message history updates
  const handleMessageHistoryUpdated = (messages: MessageHistoryEntry[]): void => {
    if (onMessageHistoryUpdated) {
      onMessageHistoryUpdated(messages);
    }
  };

  return (
    <>
      {/* Event system integration */}
      <EventManager triggerUrls={triggerUrls} autoProcessPending={true} />

      {/* Signal tracking integration */}
      <SignalTracker onSignalDiscovered={handleSignalDiscovered} />

      {/* Progress tracking integration */}
      <ProgressTracker onObjectiveCompleted={handleObjectiveCompleted} autoUpdateProgress={true} />

      {/* Message history integration */}
      <MessageHistoryManager onHistoryUpdated={handleMessageHistoryUpdated} autoSave={true} />
    </>
  );
};

export default GameStateIntegration;

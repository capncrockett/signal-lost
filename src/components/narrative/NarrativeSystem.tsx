import React, { useState, useEffect } from 'react';
import { useSignalState } from '../../context/SignalStateContext';
import { useGameState } from '../../context/GameStateContext';
import { useEvent } from '../../context/EventContext';
import { useProgress } from '../../context/ProgressContext';
import { getMessage } from '../../data/messages';
import MessageDisplay from './MessageDisplay';
import MessageHistory from './MessageHistory';
import { Signal } from '../../types/signal.d';
import './NarrativeSystem.css';

interface NarrativeSystemProps {
  isVisible?: boolean;
  onMessageSelected?: (messageId: string) => void;
}

/**
 * NarrativeSystem component that integrates all narrative-related components
 * This component serves as the main entry point for the narrative system
 */
const NarrativeSystem: React.FC<NarrativeSystemProps> = ({
  isVisible = true,
  onMessageSelected,
}) => {
  const { state: signalState, getSignalById, getDiscoveredSignals } = useSignalState();
  const { state: gameState } = useGameState();
  const { state: progressState } = useProgress();
  const { dispatchEvent } = useEvent();

  const [selectedSignalId, setSelectedSignalId] = useState<string | null>(null);
  const [selectedMessage, setSelectedMessage] = useState<ReturnType<typeof getMessage> | null>(
    null
  );
  const [showMessageDisplay, setShowMessageDisplay] = useState<boolean>(false);
  const [unreadMessages, setUnreadMessages] = useState<string[]>([]);
  const [narrativeProgress, setNarrativeProgress] = useState<number>(0);

  // Update narrative progress based on discovered signals and completed objectives
  useEffect(() => {
    // Calculate progress based on discovered signals
    const discoveredSignals = signalState?.discoveredSignalIds?.length || 0;
    const totalSignals = Object.keys(signalState?.signals || {}).length;

    // Calculate progress based on completed objectives
    const completedObjectives = progressState?.completedObjectiveIds?.length || 0;
    const objectives = progressState?.state?.objectives || {};
    const totalObjectives = completedObjectives + Object.keys(objectives).length;

    // Combine both factors to calculate narrative progress
    let newProgress = 0;

    if (totalSignals > 0) {
      newProgress += (discoveredSignals / totalSignals) * 0.6; // 60% weight for signals
    }

    if (totalObjectives > 0) {
      newProgress += (completedObjectives / totalObjectives) * 0.4; // 40% weight for objectives
    }

    setNarrativeProgress(newProgress);
  }, [
    signalState?.discoveredSignalIds,
    signalState?.signals,
    progressState?.completedObjectiveIds,
    progressState?.state?.objectives,
  ]);

  // Update selected message when selectedSignalId changes
  useEffect(() => {
    if (selectedSignalId) {
      const signal = getSignalById ? getSignalById(selectedSignalId) : null;
      if (signal && signal.type === 'message') {
        const message = getMessage ? getMessage(signal.content) : null;
        setSelectedMessage(message);

        // Mark message as read
        if (message && unreadMessages.includes(selectedSignalId)) {
          setUnreadMessages((prev) => prev.filter((id) => id !== selectedSignalId));

          // Dispatch event for reading a message
          if (dispatchEvent) {
            dispatchEvent('narrative', {
              type: 'message_read',
              signalId: selectedSignalId,
              messageId: message.id,
              timestamp: Date.now(),
            });
          }
        }

        // Show message display
        setShowMessageDisplay(true);

        // Notify parent component
        if (onMessageSelected && message) {
          onMessageSelected(message.id);
        }
      } else {
        setSelectedMessage(null);
        setShowMessageDisplay(false);
      }
    } else {
      setSelectedMessage(null);
      setShowMessageDisplay(false);
    }
  }, [selectedSignalId, getSignalById, unreadMessages, dispatchEvent, onMessageSelected]);

  // Listen for new message events
  useEffect(() => {
    const handleNewMessage = (signalId: string) => {
      // Add to unread messages if not already there
      if (!unreadMessages.includes(signalId)) {
        setUnreadMessages((prev) => [...prev, signalId]);
      }
    };

    // Subscribe to new message events
    const unsubscribe = subscribeToNewMessages(handleNewMessage);

    return () => {
      // Unsubscribe when component unmounts
      unsubscribe();
    };
  }, [unreadMessages]);

  // Helper function to subscribe to new message events
  const subscribeToNewMessages = (callback: (signalId: string) => void) => {
    // This is a placeholder for a real event subscription system
    // In a real implementation, this would use the event system to subscribe to events

    // For now, we'll just return a no-op unsubscribe function
    return () => {};
  };

  // Handle message selection
  const handleMessageSelect = (signalId: string) => {
    setSelectedSignalId(signalId);
  };

  // Handle message close
  const handleMessageClose = () => {
    setSelectedSignalId(null);
    setShowMessageDisplay(false);
  };

  // Get all message signals
  const getMessageSignals = (): Signal[] => {
    return getDiscoveredSignals
      ? getDiscoveredSignals().filter((signal) => signal.type === 'message')
      : [];
  };

  if (!isVisible) {
    return null;
  }

  return (
    <div className="narrative-system" data-testid="narrative-system">
      <div className="narrative-header">
        <h2>Signal Archive</h2>
        <div className="narrative-progress">
          <div className="progress-label">
            Narrative Progress: {Math.floor(narrativeProgress * 100)}%
          </div>
          <div className="progress-bar">
            <div className="progress-fill" style={{ width: `${narrativeProgress * 100}%` }} />
          </div>
        </div>
      </div>

      <div className="narrative-content">
        <div className="message-history-container">
          <MessageHistory
            signals={getMessageSignals()}
            selectedSignalId={selectedSignalId}
            onSignalSelect={handleMessageSelect}
            unreadSignalIds={unreadMessages}
          />
        </div>

        <div className="message-display-container">
          {selectedMessage && (
            <MessageDisplay message={selectedMessage} isVisible={showMessageDisplay} />
          )}

          {!selectedMessage && (
            <div className="no-message-selected">
              <p>Select a message from the archive to view its contents.</p>
              {unreadMessages.length > 0 && (
                <p className="unread-notification">
                  You have {unreadMessages.length} unread message
                  {unreadMessages.length !== 1 ? 's' : ''}.
                </p>
              )}
            </div>
          )}
        </div>
      </div>

      {selectedMessage && (
        <div className="message-actions">
          <button className="close-button" onClick={handleMessageClose} aria-label="Close message">
            Close
          </button>
        </div>
      )}
    </div>
  );
};

export default NarrativeSystem;

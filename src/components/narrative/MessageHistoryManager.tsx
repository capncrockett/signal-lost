import React, { useEffect, useState } from 'react';
import { useSignalState } from '../../context/SignalStateContext';
import { useGameState } from '../../context/GameStateContext';
import { useEvent } from '../../context/EventContext';
import { Signal } from '../../types/signal.d';
import { getMessage } from '../../data/messages';

// Define the structure of a message history entry
export interface MessageHistoryEntry {
  id: string;
  signalId: string;
  title: string;
  content: string;
  decodedContent?: string;
  sender?: string;
  timestamp: number;
  frequency: number;
  isDecoded: boolean;
  isRead: boolean;
}

interface MessageHistoryManagerProps {
  onHistoryUpdated?: (messages: MessageHistoryEntry[]) => void;
  autoSave?: boolean;
}

/**
 * MessageHistoryManager component that manages message history storage
 * This component doesn't render anything but provides functionality
 * for tracking and storing message history
 */
const MessageHistoryManager: React.FC<MessageHistoryManagerProps> = ({
  onHistoryUpdated,
  autoSave = true,
}) => {
  const { getDiscoveredSignals } = useSignalState();
  const { state: gameState } = useGameState();
  const { state: eventState, dispatchEvent } = useEvent();
  const [messageHistory, setMessageHistory] = useState<MessageHistoryEntry[]>([]);
  
  // Storage key for message history
  const MESSAGE_HISTORY_KEY = 'signal-lost-message-history';

  // Load message history from localStorage
  const loadMessageHistory = (): MessageHistoryEntry[] => {
    try {
      const savedHistory = localStorage.getItem(MESSAGE_HISTORY_KEY);
      if (savedHistory) {
        return JSON.parse(savedHistory) as MessageHistoryEntry[];
      }
    } catch (error) {
      console.error('Error loading message history:', error);
    }
    return [];
  };

  // Save message history to localStorage
  const saveMessageHistory = (history: MessageHistoryEntry[]): void => {
    try {
      localStorage.setItem(MESSAGE_HISTORY_KEY, JSON.stringify(history));
    } catch (error) {
      console.error('Error saving message history:', error);
    }
  };

  // Convert a signal to a message history entry
  const signalToHistoryEntry = (signal: Signal): MessageHistoryEntry | null => {
    if (signal.type !== 'message') return null;

    const message = getMessage(signal.content);
    if (!message) return null;

    const isDecoded = message.isDecoded || gameState.gameProgress >= (message.requiredProgress || 0);
    
    return {
      id: `message_${signal.id}`,
      signalId: signal.id,
      title: message.title,
      content: message.content,
      decodedContent: message.decodedContent,
      sender: message.sender,
      timestamp: signal.timestamp,
      frequency: signal.frequency,
      isDecoded,
      isRead: false,
    };
  };

  // Update message history when signals change
  useEffect(() => {
    // Load existing history
    const existingHistory = loadMessageHistory();
    
    // Get all discovered signals
    const signals = getDiscoveredSignals();
    
    // Convert signals to history entries
    const newEntries = signals
      .map(signalToHistoryEntry)
      .filter((entry): entry is MessageHistoryEntry => entry !== null);
    
    // Merge with existing history, preserving read status
    const mergedHistory = newEntries.map(newEntry => {
      const existingEntry = existingHistory.find(entry => entry.signalId === newEntry.signalId);
      return existingEntry ? { ...newEntry, isRead: existingEntry.isRead } : newEntry;
    });
    
    // Update state
    setMessageHistory(mergedHistory);
    
    // Save to localStorage if autoSave is enabled
    if (autoSave) {
      saveMessageHistory(mergedHistory);
    }
    
    // Notify parent component if callback is provided
    if (onHistoryUpdated) {
      onHistoryUpdated(mergedHistory);
    }
  }, [getDiscoveredSignals, gameState.gameProgress, autoSave, onHistoryUpdated]);

  // Mark a message as read
  const markMessageAsRead = (id: string): void => {
    setMessageHistory(prevHistory => {
      const updatedHistory = prevHistory.map(entry => 
        entry.id === id ? { ...entry, isRead: true } : entry
      );
      
      // Save to localStorage if autoSave is enabled
      if (autoSave) {
        saveMessageHistory(updatedHistory);
      }
      
      // Notify parent component if callback is provided
      if (onHistoryUpdated) {
        onHistoryUpdated(updatedHistory);
      }
      
      return updatedHistory;
    });
  };

  // Listen for new message events
  useEffect(() => {
    const pendingEvents = eventState.pendingEvents;
    
    pendingEvents.forEach(eventId => {
      const event = eventState.events[eventId];
      
      if (event && event.type === 'signal') {
        // Check if this is a new message signal
        const payload = event.payload as { signalId?: string } | undefined;
        
        if (payload && payload.signalId) {
          // Dispatch a narrative event to notify about the new message
          dispatchEvent('narrative', {
            type: 'new_message',
            signalId: payload.signalId,
            timestamp: Date.now(),
          });
        }
      }
    });
  }, [eventState.pendingEvents, eventState.events, dispatchEvent]);

  // This component doesn't render anything
  return null;
};

export default MessageHistoryManager;

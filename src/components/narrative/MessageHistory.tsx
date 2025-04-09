import React, { useState, useEffect } from 'react';
import { useSignalState } from '../../context/SignalStateContext';
import { useGameState } from '../../context/GameStateContext';
import { getMessage } from '../../data/messages';
import { Signal } from '../../types/signal.d';
import './MessageHistory.css';

interface MessageHistoryProps {
  isOpen: boolean;
  onClose: () => void;
}

const MessageHistory: React.FC<MessageHistoryProps> = ({ isOpen, onClose }) => {
  const { getDiscoveredSignals } = useSignalState();
  const { state: gameState } = useGameState();
  const [discoveredSignals, setDiscoveredSignals] = useState<Signal[]>([]);
  const [selectedSignalId, setSelectedSignalId] = useState<string | null>(null);
  const [selectedMessageContent, setSelectedMessageContent] = useState<string>('');

  // Load discovered signals when the component mounts or when isOpen changes
  useEffect(() => {
    if (isOpen) {
      const signals = getDiscoveredSignals();
      setDiscoveredSignals(signals);

      // Select the first signal by default if available
      if (signals.length > 0 && !selectedSignalId) {
        setSelectedSignalId(signals[0].id);
      }
    }
  }, [isOpen, getDiscoveredSignals, selectedSignalId]);

  // Update selected message content when selectedSignalId changes
  useEffect(() => {
    if (selectedSignalId) {
      const signal = discoveredSignals.find((s) => s.id === selectedSignalId);
      if (signal && signal.type === 'message') {
        const message = getMessage(signal.content);
        if (message) {
          // Use the decoded content if available based on game progress
          const content =
            message.isDecoded || gameState.gameProgress >= (message.requiredProgress || 0)
              ? message.decodedContent || message.content
              : message.content;

          setSelectedMessageContent(content);
        } else {
          setSelectedMessageContent('Message not found');
        }
      } else if (signal) {
        setSelectedMessageContent(signal.content);
      } else {
        setSelectedMessageContent('');
      }
    } else {
      setSelectedMessageContent('');
    }
  }, [selectedSignalId, discoveredSignals, gameState.gameProgress]);

  // Format a date for display
  const formatDate = (timestamp: number): string => {
    return new Date(timestamp).toLocaleString();
  };

  if (!isOpen) {
    return null;
  }

  return (
    <div className="message-history-overlay">
      <div className="message-history-container">
        <h2>Message History</h2>

        <div className="message-history-content">
          {discoveredSignals.length === 0 ? (
            <div className="no-messages">No messages discovered yet</div>
          ) : (
            <div className="message-history-layout">
              <div className="message-list">
                <h3>Discovered Signals</h3>
                {discoveredSignals.map((signal) => (
                  <div
                    key={signal.id}
                    className={`message-item ${selectedSignalId === signal.id ? 'selected' : ''}`}
                    onClick={() => setSelectedSignalId(signal.id)}
                  >
                    <div className="message-item-type">{signal.type}</div>
                    <div className="message-item-frequency">{signal.frequency.toFixed(1)} MHz</div>
                    <div className="message-item-date">{formatDate(signal.timestamp)}</div>
                  </div>
                ))}
              </div>

              <div className="message-content-view">
                <h3>Message Content</h3>
                {selectedSignalId ? (
                  <div className="message-content">
                    <pre>{selectedMessageContent}</pre>
                  </div>
                ) : (
                  <div className="no-message-selected">Select a message to view its content</div>
                )}
              </div>
            </div>
          )}
        </div>

        <div className="message-history-footer">
          <button onClick={onClose} className="close-button">
            Close
          </button>
        </div>
      </div>
    </div>
  );
};

export default MessageHistory;

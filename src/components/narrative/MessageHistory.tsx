import React, { useState, useEffect } from 'react';
import { useSignalState } from '../../context/SignalStateContext';
import { useGameState } from '../../context/GameStateContext';
import { getMessage, getDecodedMessage } from '../../data/messages';
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
  const [isDecoding, setIsDecoding] = useState<boolean>(false);
  const [decodingProgress, setDecodingProgress] = useState<number>(0);
  const [manualDecodeAttempts, setManualDecodeAttempts] = useState<number>(0);

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
          // Reset decoding state
          setIsDecoding(false);
          setDecodingProgress(0);

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

  // Attempt to manually decode a message
  const attemptDecode = () => {
    const signal = discoveredSignals.find((s) => s.id === selectedSignalId);
    if (!signal || signal.type !== 'message') return;

    const message = getMessage(signal.content);
    if (!message || message.isDecoded) return;

    setIsDecoding(true);
    setDecodingProgress(0);

    // Simulate decoding process
    const totalDecodingTime = 3000; // 3 seconds
    const interval = 100; // Update every 100ms
    const steps = totalDecodingTime / interval;

    let currentStep = 0;
    const timer = setInterval(() => {
      currentStep++;
      setDecodingProgress(currentStep / steps);

      if (currentStep >= steps) {
        clearInterval(timer);
        setIsDecoding(false);

        // Calculate how much to decode based on game progress and manual attempts
        const progressBoost = Math.min(0.2, manualDecodeAttempts * 0.05); // Each attempt gives up to 5% boost, max 20%
        const effectiveProgress = Math.min(
          1,
          gameState.gameProgress / (message.requiredProgress || 1) + progressBoost
        );

        // Get partially decoded message
        if (effectiveProgress >= 1) {
          // Fully decoded
          setSelectedMessageContent(message.decodedContent || message.content);
        } else {
          // Partially decoded - more of the message is revealed with each attempt
          const partiallyDecodedMessage = getPartiallyDecodedMessage(message, effectiveProgress);
          setSelectedMessageContent(partiallyDecodedMessage);
        }

        // Increment decode attempts
        setManualDecodeAttempts((prev) => prev + 1);
      }
    }, interval);

    return () => clearInterval(timer);
  };

  // Get a partially decoded message based on progress
  const getPartiallyDecodedMessage = (message: any, progress: number): string => {
    if (!message.decodedContent) return message.content;

    const originalWords = message.content.split(' ');
    const decodedWords = message.decodedContent.split(' ');

    // Calculate how many static words to replace
    const staticWords = originalWords.filter((word) => word.includes('[static]'));
    const wordsToReplace = Math.floor(staticWords.length * progress);

    // Create a map of which indices contain static
    const staticIndices = originalWords
      .map((word, index) => (word.includes('[static]') ? index : -1))
      .filter((index) => index !== -1);

    // Randomly select indices to decode
    const indicesToDecode = staticIndices.sort(() => Math.random() - 0.5).slice(0, wordsToReplace);

    // Replace selected words with decoded versions
    const result = [...originalWords];
    indicesToDecode.forEach((index) => {
      result[index] = decodedWords[index];
    });

    return result.join(' ');
  };

  // Check if the current message is encrypted and not fully decoded
  const isMessageEncrypted = (): boolean => {
    if (!selectedSignalId) return false;

    const signal = discoveredSignals.find((s) => s.id === selectedSignalId);
    if (!signal || signal.type !== 'message') return false;

    const message = getMessage(signal.content);
    if (!message) return false;

    // Message is encrypted if it's not decoded and contains [static] or is binary
    return (
      !message.isDecoded &&
      (message.content.includes('[static]') || /^[01\s]+$/.test(message.content))
    );
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
                    {isDecoding ? (
                      <div className="decoding-container">
                        <div className="decoding-text">Decoding message...</div>
                        <div className="decoding-progress-bar">
                          <div
                            className="decoding-progress-fill"
                            style={{ width: `${decodingProgress * 100}%` }}
                          />
                        </div>
                      </div>
                    ) : (
                      <>
                        <pre>{selectedMessageContent}</pre>
                        {isMessageEncrypted() && (
                          <div className="decode-actions">
                            <button onClick={attemptDecode} disabled={isDecoding}>
                              Attempt to Decode
                            </button>
                            <div className="decode-hint">
                              {manualDecodeAttempts > 0
                                ? `Decode attempts: ${manualDecodeAttempts}`
                                : 'This message appears to be encrypted or corrupted'}
                            </div>
                          </div>
                        )}
                      </>
                    )}
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

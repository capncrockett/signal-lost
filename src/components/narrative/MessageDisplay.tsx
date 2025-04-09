import React, { useState, useEffect, useRef } from 'react';
import { Message, getDecodedMessage } from '../../data/messages';
import { useGameState } from '../../context/GameStateContext';
import './MessageDisplay.css';

interface MessageDisplayProps {
  message?: Message;
  isVisible: boolean;
}

const MessageDisplay: React.FC<MessageDisplayProps> = ({ message, isVisible }) => {
  const { state } = useGameState();
  const [displayedContent, setDisplayedContent] = useState<string>('');
  const [isDecoding, setIsDecoding] = useState<boolean>(false);
  const [decodingProgress, setDecodingProgress] = useState<number>(0);
  const [decodedWords, setDecodedWords] = useState<string[]>([]);
  const contentRef = useRef<HTMLParagraphElement>(null);

  useEffect(() => {
    if (!message || !isVisible) {
      setDisplayedContent('');
      setIsDecoding(false);
      setDecodingProgress(0);
      setDecodedWords([]);
      return;
    }

    // Get the appropriate content based on game progress
    const content = getDecodedMessage(message, state.gameProgress);

    // If the message is not fully decoded, simulate decoding
    if (!message.isDecoded && message.decodedContent) {
      setIsDecoding(true);
      setDecodingProgress(0);

      // Split content into words for progressive decoding
      const originalWords = message.content.split(' ');
      const fullyDecodedWords = message.decodedContent.split(' ');

      // Initialize with original content (with [static] parts)
      setDisplayedContent(message.content);
      setDecodedWords([]);

      // Simulate decoding progress
      const totalDecodingTime = 5000; // 5 seconds
      const interval = 200; // Update every 200ms
      const steps = totalDecodingTime / interval;

      // Find words with [static] to replace
      const staticWordIndices = originalWords
        .map((word, index) => (word.includes('[static]') ? index : -1))
        .filter((index) => index !== -1);

      // Calculate how many words to decode based on game progress
      const decodeRatio = state.gameProgress / (message.requiredProgress || 1);
      const totalWordsToReplace = Math.min(
        staticWordIndices.length,
        Math.floor(staticWordIndices.length * decodeRatio)
      );

      // Shuffle the indices to make the decoding appear random
      const shuffledIndices = [...staticWordIndices]
        .sort(() => Math.random() - 0.5)
        .slice(0, totalWordsToReplace);

      let currentStep = 0;
      let replacedCount = 0;
      const maxReplacements = shuffledIndices.length;

      const timer = setInterval(() => {
        currentStep++;
        setDecodingProgress(currentStep / steps);

        // Replace a word periodically
        if (
          replacedCount < maxReplacements &&
          currentStep % Math.floor(steps / maxReplacements) === 0
        ) {
          const indexToReplace = shuffledIndices[replacedCount];
          const newWords = [...originalWords];

          // Replace the static word with the decoded version
          newWords[indexToReplace] = fullyDecodedWords[indexToReplace];
          setDecodedWords((prev) => [...prev, indexToReplace.toString()]);

          // Update displayed content
          setDisplayedContent(newWords.join(' '));
          replacedCount++;
        }

        if (currentStep >= steps) {
          clearInterval(timer);
          setIsDecoding(false);
          setDisplayedContent(content);
        }
      }, interval);

      return () => clearInterval(timer);
    } else {
      // If the message is already decoded, just display it
      setDisplayedContent(content);
      setDecodedWords([]);
    }
  }, [message, isVisible, state.gameProgress]);

  if (!message || !isVisible) {
    return null;
  }

  return (
    <div className="message-display" data-testid="message-display">
      <div className="message-header">
        <h3 className="message-title">{message.title}</h3>
        {message.sender && <div className="message-sender">From: {message.sender}</div>}
        {message.timestamp && <div className="message-timestamp">Time: {message.timestamp}</div>}
      </div>

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
            <p ref={contentRef} className="decoding-content">
              {displayedContent.split(' ').map((word, index) => {
                const isDecoded = decodedWords.includes(index.toString());
                const isStatic = word.includes('[static]');
                return (
                  <span
                    key={index}
                    className={`message-word ${isDecoded ? 'decoded' : ''} ${isStatic ? 'static' : ''}`}
                  >
                    {word}{' '}
                  </span>
                );
              })}
            </p>
          </div>
        ) : (
          <p>{displayedContent}</p>
        )}
      </div>

      {message.coordinates && (
        <div className="message-coordinates">
          <strong>Coordinates:</strong> {message.coordinates}
        </div>
      )}
    </div>
  );
};

export default MessageDisplay;

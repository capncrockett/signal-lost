import React, { useState, useEffect } from 'react';
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

  useEffect(() => {
    if (!message || !isVisible) {
      setDisplayedContent('');
      setIsDecoding(false);
      setDecodingProgress(0);
      return;
    }

    // Get the appropriate content based on game progress
    const content = getDecodedMessage(message, state.gameProgress);
    
    // If the message is not fully decoded, simulate decoding
    if (!message.isDecoded && message.decodedContent) {
      setIsDecoding(true);
      setDecodingProgress(0);
      
      // Simulate decoding progress
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
          setDisplayedContent(content);
        }
      }, interval);
      
      return () => clearInterval(timer);
    } else {
      // If the message is already decoded, just display it
      setDisplayedContent(content);
    }
  }, [message, isVisible, state.gameProgress]);

  if (!message || !isVisible) {
    return null;
  }

  return (
    <div className="message-display" data-testid="message-display">
      <div className="message-header">
        <h3 className="message-title">{message.title}</h3>
        {message.sender && (
          <div className="message-sender">From: {message.sender}</div>
        )}
        {message.timestamp && (
          <div className="message-timestamp">Time: {message.timestamp}</div>
        )}
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

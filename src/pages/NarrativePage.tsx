import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useGameState } from '../context/GameStateContext';
import { useEvent } from '../context/EventContext';
import NarrativeSystem from '../components/narrative/NarrativeSystem';
import './NarrativePage.css';

/**
 * NarrativePage component that displays the narrative system
 * This page allows the player to view and interact with the narrative elements of the game
 */
const NarrativePage: React.FC = () => {
  const navigate = useNavigate();
  const { state: gameState } = useGameState();
  const { dispatchEvent } = useEvent();

  // Track page view
  useEffect(() => {
    dispatchEvent('system', {
      type: 'page_view',
      page: 'narrative',
      timestamp: Date.now(),
    });
  }, [dispatchEvent]);

  // Handle message selection
  const handleMessageSelected = (messageId: string) => {
    dispatchEvent('narrative', {
      type: 'message_selected',
      messageId,
      timestamp: Date.now(),
    });
  };

  // Handle back button click
  const handleBackClick = () => {
    navigate('/');
  };

  return (
    <div className="narrative-page" data-testid="narrative-page">
      <div className="narrative-page-header">
        <button 
          className="back-button"
          onClick={handleBackClick}
          aria-label="Back to main menu"
        >
          &lt; Back
        </button>
        <h1>Signal Archive</h1>
      </div>
      
      <div className="narrative-page-content">
        <NarrativeSystem 
          isVisible={true}
          onMessageSelected={handleMessageSelected}
        />
      </div>
      
      <div className="narrative-page-footer">
        <div className="game-info">
          <div className="game-progress">
            Game Progress: {Math.floor(gameState.gameProgress * 100)}%
          </div>
          <div className="game-time">
            Game Time: {formatGameTime(gameState.gameTime || 0)}
          </div>
        </div>
      </div>
    </div>
  );
};

// Helper function to format game time
const formatGameTime = (seconds: number): string => {
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = Math.floor(seconds % 60);
  
  return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
};

export default NarrativePage;

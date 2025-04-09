import React, { useState } from 'react';
import SaveLoadManager from './SaveLoadManager';
import MessageHistory from '../narrative/MessageHistory';
import './GameMenu.css';

interface GameMenuProps {
  isOpen: boolean;
  onClose: () => void;
}

const GameMenu: React.FC<GameMenuProps> = ({ isOpen, onClose }) => {
  const [showSaveLoad, setShowSaveLoad] = useState<boolean>(false);
  const [showMessageHistory, setShowMessageHistory] = useState<boolean>(false);

  const handleSaveLoadClick = (): void => {
    setShowSaveLoad(true);
    onClose();
  };

  const handleMessageHistoryClick = (): void => {
    setShowMessageHistory(true);
    onClose();
  };

  const handleCloseSaveLoad = (): void => {
    setShowSaveLoad(false);
  };

  const handleCloseMessageHistory = (): void => {
    setShowMessageHistory(false);
  };

  if (!isOpen) {
    return (
      <>
        <SaveLoadManager isOpen={showSaveLoad} onClose={handleCloseSaveLoad} />
        <MessageHistory isOpen={showMessageHistory} onClose={handleCloseMessageHistory} />
      </>
    );
  }

  return (
    <>
      <div className="game-menu-overlay">
        <div className="game-menu-container">
          <h2>Game Menu</h2>
          
          <div className="game-menu-options">
            <button className="menu-option" onClick={handleSaveLoadClick}>
              Save / Load Game
            </button>
            <button className="menu-option" onClick={handleMessageHistoryClick}>
              Message History
            </button>
            <button className="menu-option" onClick={onClose}>
              Resume Game
            </button>
          </div>
        </div>
      </div>
      
      <SaveLoadManager isOpen={showSaveLoad} onClose={handleCloseSaveLoad} />
      <MessageHistory isOpen={showMessageHistory} onClose={handleCloseMessageHistory} />
    </>
  );
};

export default GameMenu;

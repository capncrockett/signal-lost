import React, { useState } from 'react';
import GameMenu from './GameMenu';
import './GameMenuButton.css';

const GameMenuButton: React.FC = () => {
  const [isMenuOpen, setIsMenuOpen] = useState<boolean>(false);

  const handleOpenMenu = (): void => {
    setIsMenuOpen(true);
  };

  const handleCloseMenu = (): void => {
    setIsMenuOpen(false);
  };

  return (
    <>
      <button className="game-menu-button" onClick={handleOpenMenu} aria-label="Open game menu">
        <span className="menu-icon"></span>
        <span className="menu-text">Menu</span>
      </button>

      <GameMenu isOpen={isMenuOpen} onClose={handleCloseMenu} />
    </>
  );
};

export default GameMenuButton;

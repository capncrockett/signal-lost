import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import GameMenu from '../../../src/components/system/GameMenu';
// No need to import providers

// Mock the SaveLoadManager and MessageHistory components
jest.mock('../../../src/components/system/SaveLoadManager', () => {
  return function MockSaveLoadManager({
    isOpen,
    onClose,
  }: {
    isOpen: boolean;
    onClose: () => void;
  }) {
    return isOpen ? (
      <div data-testid="save-load-manager">
        <button onClick={onClose}>Close Save/Load</button>
      </div>
    ) : null;
  };
});

jest.mock('../../../src/components/narrative/MessageHistory', () => {
  return function MockMessageHistory({
    isOpen,
    onClose,
  }: {
    isOpen: boolean;
    onClose: () => void;
  }) {
    return isOpen ? (
      <div data-testid="message-history">
        <button onClick={onClose}>Close Message History</button>
      </div>
    ) : null;
  };
});

describe('GameMenu Component', () => {
  const renderComponent = (ui: React.ReactElement) => {
    return render(ui);
  };

  test('renders menu when isOpen is true', () => {
    renderComponent(<GameMenu isOpen={true} onClose={() => {}} />);

    // Check if the menu title is displayed
    expect(screen.getByText('Game Menu')).toBeInTheDocument();

    // Check if menu options are displayed
    expect(screen.getByText('Save / Load Game')).toBeInTheDocument();
    expect(screen.getByText('Message History')).toBeInTheDocument();
    expect(screen.getByText('Resume Game')).toBeInTheDocument();
  });

  test('does not render menu when isOpen is false', () => {
    renderComponent(<GameMenu isOpen={false} onClose={() => {}} />);

    // Check that the menu title is not displayed
    expect(screen.queryByText('Game Menu')).not.toBeInTheDocument();
  });

  test('opens SaveLoadManager when Save/Load button is clicked', () => {
    renderComponent(<GameMenu isOpen={true} onClose={() => {}} />);

    // Click the Save/Load button
    fireEvent.click(screen.getByText('Save / Load Game'));

    // Check if SaveLoadManager is displayed
    expect(screen.getByTestId('save-load-manager')).toBeInTheDocument();
  });

  test('opens MessageHistory when Message History button is clicked', () => {
    renderComponent(<GameMenu isOpen={true} onClose={() => {}} />);

    // Click the Message History button
    fireEvent.click(screen.getByText('Message History'));

    // Check if MessageHistory is displayed
    expect(screen.getByTestId('message-history')).toBeInTheDocument();
  });

  test('calls onClose when Resume Game button is clicked', () => {
    const handleClose = jest.fn();
    renderComponent(<GameMenu isOpen={true} onClose={handleClose} />);

    // Click the Resume Game button
    fireEvent.click(screen.getByText('Resume Game'));

    // Check if onClose was called
    expect(handleClose).toHaveBeenCalledTimes(1);
  });
});

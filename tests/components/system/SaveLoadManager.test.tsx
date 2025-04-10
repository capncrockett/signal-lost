import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import SaveLoadManager from '../../../src/components/system/SaveLoadManager';
// Mock the hooks
jest.mock('../../../src/context/GameStateContext', () => ({
  useGameState: jest.fn().mockReturnValue({
    state: {},
    dispatch: jest.fn(),
  }),
}));

jest.mock('../../../src/context/SignalStateContext', () => ({
  useSignalState: jest.fn().mockReturnValue({
    state: {},
    dispatch: jest.fn(),
  }),
}));

jest.mock('../../../src/context/EventContext', () => ({
  useEvent: jest.fn().mockReturnValue({
    state: {},
    dispatch: jest.fn(),
  }),
}));

jest.mock('../../../src/context/ProgressContext', () => ({
  useProgress: jest.fn().mockReturnValue({
    state: {},
    dispatch: jest.fn(),
  }),
}));
// We don't need to import SaveManager directly since we're mocking it
// import { SaveManager } from '../../../src/utils/SaveManager';

// Mock the SaveManager class
jest.mock('../../../src/utils/SaveManager', () => {
  return {
    SaveManager: jest.fn().mockImplementation(() => {
      return {
        getSaveFiles: jest.fn().mockReturnValue([
          {
            id: 'save1',
            name: 'Test Save 1',
            timestamp: Date.now() - 3600000, // 1 hour ago
            gameState: {},
            signalState: {},
            eventState: {},
            progressState: {},
          },
          {
            id: 'save2',
            name: 'Test Save 2',
            timestamp: Date.now(),
            gameState: {},
            signalState: {},
            eventState: {},
            progressState: {},
          },
        ]),
        saveGame: jest.fn().mockReturnValue('save3'),
        loadGame: jest.fn().mockReturnValue({
          id: 'save1',
          name: 'Test Save 1',
          timestamp: Date.now() - 3600000,
          gameState: {},
          signalState: {},
          eventState: {},
          progressState: {},
        }),
        deleteSave: jest.fn().mockReturnValue(true),
        takeScreenshot: jest.fn().mockResolvedValue(null),
      };
    }),
  };
});

describe('SaveLoadManager Component', () => {
  const renderComponent = (ui: React.ReactElement) => {
    return render(ui);
  };

  test('renders when isOpen is true', () => {
    renderComponent(<SaveLoadManager isOpen={true} onClose={() => {}} />);

    // Check if the component title is displayed
    expect(screen.getByText('Save / Load Game')).toBeInTheDocument();

    // Check if save files are displayed
    expect(screen.getByText('Test Save 1')).toBeInTheDocument();
    expect(screen.getByText('Test Save 2')).toBeInTheDocument();
  });

  test('does not render when isOpen is false', () => {
    renderComponent(<SaveLoadManager isOpen={false} onClose={() => {}} />);

    // Check that the component title is not displayed
    expect(screen.queryByText('Save / Load Game')).not.toBeInTheDocument();
  });

  test('allows selecting a save file', () => {
    renderComponent(<SaveLoadManager isOpen={true} onClose={() => {}} />);

    // Click on a save file
    fireEvent.click(screen.getByText('Test Save 1'));

    // Check if the Load button is enabled
    expect(screen.getByText('Load')).not.toBeDisabled();
    expect(screen.getByText('Delete')).not.toBeDisabled();
  });

  test('allows entering a new save name', () => {
    renderComponent(<SaveLoadManager isOpen={true} onClose={() => {}} />);

    // Enter a new save name
    const input = screen.getByPlaceholderText('Enter save name');
    fireEvent.change(input, { target: { value: 'New Save' } });

    // Check if the Save button is enabled
    expect(screen.getByText('Save')).not.toBeDisabled();
  });

  test('calls onClose when Close button is clicked', () => {
    const handleClose = jest.fn();
    renderComponent(<SaveLoadManager isOpen={true} onClose={handleClose} />);

    // Click the Close button
    fireEvent.click(screen.getByText('Close'));

    // Check if onClose was called
    expect(handleClose).toHaveBeenCalledTimes(1);
  });
});

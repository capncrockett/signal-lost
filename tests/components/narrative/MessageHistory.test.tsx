import React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import MessageHistory from '../../../src/components/narrative/MessageHistory';
// Mock the hooks
jest.mock('../../../src/context/GameStateContext', () => ({
  useGameState: jest.fn().mockReturnValue({
    state: { gameProgress: 2 },
  }),
}));
import { useSignalState } from '../../../src/context/SignalStateContext';

// Mock the useSignalState hook
jest.mock('../../../src/context/SignalStateContext', () => ({
  useSignalState: jest.fn(),
}));

// Mock the getMessage function
jest.mock('../../../src/data/messages', () => ({
  getMessage: jest.fn().mockImplementation((id) => {
    if (id === 'message1') {
      return {
        id: 'message1',
        title: 'Test Message 1',
        content: 'This is test message 1 content',
        isDecoded: true,
        requiredProgress: 0,
      };
    } else if (id === 'message2') {
      return {
        id: 'message2',
        title: 'Test Message 2',
        content: 'This is test message 2 with [static] content',
        isDecoded: false,
        decodedContent: 'This is test message 2 with decoded content',
        requiredProgress: 2,
      };
    }
    return null;
  }),
}));

describe('MessageHistory Component', () => {
  beforeEach(() => {
    // Setup mock for useSignalState
    (useSignalState as jest.Mock).mockReturnValue({
      getDiscoveredSignals: jest.fn().mockReturnValue([
        {
          id: 'signal1',
          frequency: 91.1,
          strength: 0.8,
          type: 'message',
          content: 'message1',
          discovered: true,
          timestamp: Date.now() - 3600000, // 1 hour ago
        },
        {
          id: 'signal2',
          frequency: 94.7,
          strength: 0.6,
          type: 'message',
          content: 'message2',
          discovered: true,
          timestamp: Date.now(),
        },
      ]),
    });
  });

  const renderComponent = (ui: React.ReactElement) => {
    return render(ui);
  };

  test('renders when isOpen is true', () => {
    renderComponent(<MessageHistory isOpen={true} onClose={() => {}} />);

    // Check if the component title is displayed
    expect(screen.getByText('Message History')).toBeInTheDocument();

    // Check if signals are displayed
    expect(screen.getByText('91.1 MHz')).toBeInTheDocument();
    expect(screen.getByText('94.7 MHz')).toBeInTheDocument();
  });

  test('does not render when isOpen is false', () => {
    renderComponent(<MessageHistory isOpen={false} onClose={() => {}} />);

    // Check that the component title is not displayed
    expect(screen.queryByText('Message History')).not.toBeInTheDocument();
  });

  test('displays message content when a signal is selected', () => {
    renderComponent(<MessageHistory isOpen={true} onClose={() => {}} />);

    // Click on a signal
    fireEvent.click(screen.getByText('91.1 MHz'));

    // Check if the message content is displayed
    expect(screen.getByText('This is test message 1 content')).toBeInTheDocument();
  });

  test('calls onClose when Close button is clicked', () => {
    const handleClose = jest.fn();
    renderComponent(<MessageHistory isOpen={true} onClose={handleClose} />);

    // Click the Close button
    fireEvent.click(screen.getByText('Close'));

    // Check if onClose was called
    expect(handleClose).toHaveBeenCalledTimes(1);
  });

  test('displays "No messages discovered yet" when there are no signals', () => {
    // Override the mock to return an empty array
    (useSignalState as jest.Mock).mockReturnValue({
      getDiscoveredSignals: jest.fn().mockReturnValue([]),
    });

    renderComponent(<MessageHistory isOpen={true} onClose={() => {}} />);

    // Check if the no messages message is displayed
    expect(screen.getByText('No messages discovered yet')).toBeInTheDocument();
  });
});

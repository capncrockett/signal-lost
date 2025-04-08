import React from 'react';
import { render, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import MessageDisplay from '../../../src/components/narrative/MessageDisplay';
import { Message } from '../../../src/data/messages';
import { GameStateProvider } from '../../../src/context/GameStateContext';

// Mock message for testing
const mockMessage: Message = {
  id: 'test_message',
  title: 'Test Message',
  content: 'This is a test message with [static] content.',
  sender: 'Test Sender',
  timestamp: '2023-06-15 10:00',
  coordinates: '37.4419° N, 122.1430° W',
  isDecoded: false,
  decodedContent: 'This is a test message with decoded content.',
  requiredProgress: 2,
};

// Fully decoded message for testing
const decodedMessage: Message = {
  id: 'decoded_message',
  title: 'Decoded Message',
  content: 'This is a fully decoded message.',
  sender: 'Test Sender',
  timestamp: '2023-06-15 10:00',
  isDecoded: true,
  requiredProgress: 0,
};

describe('MessageDisplay Component', () => {
  const renderWithProvider = (ui: React.ReactElement) => {
    return render(
      <GameStateProvider>
        {ui}
      </GameStateProvider>
    );
  };

  test('renders message when visible', () => {
    renderWithProvider(<MessageDisplay message={mockMessage} isVisible={true} />);

    // Check if the message title is displayed
    expect(screen.getByText('Test Message')).toBeInTheDocument();

    // Check if the sender is displayed
    expect(screen.getByText('From: Test Sender')).toBeInTheDocument();

    // Check if the timestamp is displayed
    expect(screen.getByText('Time: 2023-06-15 10:00')).toBeInTheDocument();

    // Check if the coordinates are displayed
    expect(screen.getByText('Coordinates:')).toBeInTheDocument();
    expect(screen.getByText('37.4419° N, 122.1430° W')).toBeInTheDocument();
  });

  test('does not render when not visible', () => {
    renderWithProvider(<MessageDisplay message={mockMessage} isVisible={false} />);

    // Check that the message title is not displayed
    expect(screen.queryByText('Test Message')).not.toBeInTheDocument();
  });

  test('renders decoded message content for already decoded messages', () => {
    renderWithProvider(<MessageDisplay message={decodedMessage} isVisible={true} />);

    // Check if the decoded content is displayed
    expect(screen.getByText('This is a fully decoded message.')).toBeInTheDocument();
  });

  test('does not render when message is undefined', () => {
    renderWithProvider(<MessageDisplay isVisible={true} />);

    // Check that the message display is not rendered
    expect(screen.queryByTestId('message-display')).not.toBeInTheDocument();
  });
});

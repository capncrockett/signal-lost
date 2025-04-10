import * as React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import NarrativeSystem from '../../../src/components/narrative/NarrativeSystem';
// Mock the contexts directly instead of using CombinedGameProvider
import { Signal } from '../../../src/types/signal.d';

// Mock the SignalStateContext
jest.mock('../../../src/context/SignalStateContext', () => ({
  useSignalState: jest.fn().mockReturnValue({
    state: {
      signals: {
        'signal-1': {
          id: 'signal-1',
          frequency: 91.5,
          strength: 0.8,
          type: 'message',
          content: 'intro_signal',
          discovered: true,
          timestamp: Date.now(),
        },
        'signal-2': {
          id: 'signal-2',
          frequency: 92.3,
          strength: 0.7,
          type: 'message',
          content: 'distress_call',
          discovered: true,
          timestamp: Date.now(),
        },
      },
      activeSignalId: null,
      discoveredSignalIds: ['signal-1', 'signal-2'],
      lastDiscoveredTimestamp: null,
    },
    getSignalById: jest.fn().mockImplementation((id) => {
      const signals = {
        'signal-1': {
          id: 'signal-1',
          frequency: 91.5,
          strength: 0.8,
          type: 'message',
          content: 'intro_signal',
          discovered: true,
          timestamp: Date.now(),
        },
        'signal-2': {
          id: 'signal-2',
          frequency: 92.3,
          strength: 0.7,
          type: 'message',
          content: 'distress_call',
          discovered: true,
          timestamp: Date.now(),
        },
      };
      return signals[id as keyof typeof signals] as Signal;
    }),
    getDiscoveredSignals: jest.fn().mockReturnValue([
      {
        id: 'signal-1',
        frequency: 91.5,
        strength: 0.8,
        type: 'message',
        content: 'intro_signal',
        discovered: true,
        timestamp: Date.now(),
      },
      {
        id: 'signal-2',
        frequency: 92.3,
        strength: 0.7,
        type: 'message',
        content: 'distress_call',
        discovered: true,
        timestamp: Date.now(),
      },
    ]),
  }),
}));

// Mock the GameStateContext
jest.mock('../../../src/context/GameStateContext', () => ({
  useGameState: jest.fn().mockReturnValue({
    state: {
      gameProgress: 0.5,
      inventory: [],
      currentFrequency: 91.5,
      isRadioOn: true,
    },
  }),
}));

// Mock the ProgressContext
jest.mock('../../../src/context/ProgressContext', () => ({
  useProgress: jest.fn().mockReturnValue({
    state: {
      currentProgress: 0.5,
      objectives: {
        'objective-1': {
          id: 'objective-1',
          title: 'Find the radio',
          description: 'Locate the radio to start receiving signals',
          isCompleted: true,
        },
        'objective-2': {
          id: 'objective-2',
          title: 'Decode the first message',
          description: 'Decode the first message to learn more',
          isCompleted: false,
        },
      },
    },
    completedObjectiveIds: ['objective-1'],
    lastCompletedObjectiveId: 'objective-1',
    lastCompletedTimestamp: Date.now(),
  }),
}));

// Mock the EventContext
jest.mock('../../../src/context/EventContext', () => ({
  useEvent: jest.fn().mockReturnValue({
    state: {
      events: {},
      eventHistory: [],
      pendingEvents: [],
    },
    dispatchEvent: jest.fn(),
  }),
}));

// Mock the MessageHistory component
jest.mock('../../../src/components/narrative/MessageHistory', () => ({
  __esModule: true,
  default: jest.fn().mockImplementation(({ onSignalSelect }) => (
    <div data-testid="message-history">
      <button data-testid="select-signal-1" onClick={() => onSignalSelect('signal-1')}>
        Select Signal 1
      </button>
      <button data-testid="select-signal-2" onClick={() => onSignalSelect('signal-2')}>
        Select Signal 2
      </button>
    </div>
  )),
}));

// Mock the MessageDisplay component
jest.mock('../../../src/components/narrative/MessageDisplay', () => ({
  __esModule: true,
  default: jest.fn().mockImplementation(({ message, isVisible }) =>
    isVisible ? (
      <div data-testid="message-display">
        <h3>{message?.title}</h3>
        <p>{message?.content}</p>
      </div>
    ) : null
  ),
}));

// Mock the getMessage function
jest.mock('../../../src/data/messages', () => ({
  getMessage: jest.fn().mockImplementation((content) => {
    const messages = {
      intro_signal: {
        id: 'intro_signal',
        title: 'First Contact',
        content: 'This is Station Alpha. If anyone can hear this, please respond.',
        sender: 'Dr. Sarah Chen',
        timestamp: '2023-06-15 08:42',
        isDecoded: true,
        requiredProgress: 0,
      },
      distress_call: {
        id: 'distress_call',
        title: 'Distress Call',
        content: 'Mayday! Mayday! We need immediate assistance.',
        sender: 'Unknown',
        timestamp: '2023-06-16 14:23',
        isDecoded: false,
        decodedContent:
          'Mayday! Mayday! We need immediate assistance at coordinates 37.7749° N, 122.4194° W.',
        requiredProgress: 0.7,
      },
    };
    return messages[content as keyof typeof messages];
  }),
}));

describe('NarrativeSystem Component', () => {
  test('renders the narrative system', () => {
    render(<NarrativeSystem />);

    expect(screen.getByTestId('narrative-system')).toBeInTheDocument();
    expect(screen.getByText('Signal Archive')).toBeInTheDocument();
    expect(screen.getByText(/Narrative Progress:/)).toBeInTheDocument();
    expect(screen.getByTestId('message-history')).toBeInTheDocument();
    expect(
      screen.getByText('Select a message from the archive to view its contents.')
    ).toBeInTheDocument();
  });

  test('displays a message when selected', () => {
    render(<NarrativeSystem />);

    // Initially, no message is displayed
    expect(screen.queryByTestId('message-display')).not.toBeInTheDocument();

    // Select a message
    fireEvent.click(screen.getByTestId('select-signal-1'));

    // Now the message should be displayed
    expect(screen.getByTestId('message-display')).toBeInTheDocument();
    expect(screen.getByText('First Contact')).toBeInTheDocument();
  });

  test('closes the message when close button is clicked', () => {
    render(<NarrativeSystem />);

    // Select a message
    fireEvent.click(screen.getByTestId('select-signal-1'));

    // Message is displayed
    expect(screen.getByTestId('message-display')).toBeInTheDocument();

    // Click the close button
    fireEvent.click(screen.getByText('Close'));

    // Message should be closed
    expect(screen.queryByTestId('message-display')).not.toBeInTheDocument();
    expect(
      screen.getByText('Select a message from the archive to view its contents.')
    ).toBeInTheDocument();
  });

  test('calls onMessageSelected when a message is selected', () => {
    const onMessageSelected = jest.fn();
    render(<NarrativeSystem onMessageSelected={onMessageSelected} />);

    // Select a message
    fireEvent.click(screen.getByTestId('select-signal-1'));

    // onMessageSelected should be called with the message ID
    expect(onMessageSelected).toHaveBeenCalledWith('intro_signal');
  });

  test('does not render when isVisible is false', () => {
    render(<NarrativeSystem isVisible={false} />);

    // The component should not be rendered
    expect(screen.queryByTestId('narrative-system')).not.toBeInTheDocument();
  });
});

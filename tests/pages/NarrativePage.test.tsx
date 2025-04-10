import * as React from 'react';
import { render, screen, fireEvent } from '@testing-library/react';
import '@testing-library/jest-dom';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import NarrativePage from '../../src/pages/NarrativePage';
// Mock the contexts directly instead of using CombinedGameProvider

// Mock the useNavigate hook
const mockNavigate = jest.fn();
jest.mock('react-router-dom', () => ({
  ...jest.requireActual('react-router-dom'),
  useNavigate: () => mockNavigate,
}));

// Mock the GameStateContext
jest.mock('../../src/context/GameStateContext', () => ({
  useGameState: jest.fn().mockReturnValue({
    state: {
      gameProgress: 0.5,
      gameTime: 3600, // 1 hour
      inventory: [],
      currentFrequency: 91.5,
      isRadioOn: true,
    },
  }),
}));

// Mock the EventContext
jest.mock('../../src/context/EventContext', () => ({
  useEvent: jest.fn().mockReturnValue({
    state: {
      events: {},
      eventHistory: [],
      pendingEvents: [],
    },
    dispatchEvent: jest.fn(),
  }),
}));

// Mock the NarrativeSystem component
jest.mock('../../src/components/narrative/NarrativeSystem', () => ({
  __esModule: true,
  default: jest.fn().mockImplementation(({ onMessageSelected }) => (
    <div data-testid="narrative-system">
      <button data-testid="select-message" onClick={() => onMessageSelected('test-message')}>
        Select Message
      </button>
    </div>
  )),
}));

describe('NarrativePage Component', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders the narrative page', () => {
    render(
      <MemoryRouter>
        <NarrativePage />
      </MemoryRouter>
    );

    expect(screen.getByTestId('narrative-page')).toBeInTheDocument();
    expect(screen.getByText('Signal Archive')).toBeInTheDocument();
    expect(screen.getByTestId('narrative-system')).toBeInTheDocument();
    expect(screen.getByText('Game Progress: 50%')).toBeInTheDocument();
    expect(screen.getByText('Game Time: 01:00:00')).toBeInTheDocument();
  });

  test('navigates back when back button is clicked', () => {
    render(
      <MemoryRouter>
        <NarrativePage />
      </MemoryRouter>
    );

    fireEvent.click(screen.getByText('< Back'));
    expect(mockNavigate).toHaveBeenCalledWith('/');
  });

  test('dispatches event when a message is selected', () => {
    const { useEvent } = require('../../src/context/EventContext');
    const mockDispatchEvent = jest.fn();
    useEvent.mockReturnValue({
      state: {
        events: {},
        eventHistory: [],
        pendingEvents: [],
      },
      dispatchEvent: mockDispatchEvent,
    });

    render(
      <MemoryRouter>
        <NarrativePage />
      </MemoryRouter>
    );

    fireEvent.click(screen.getByTestId('select-message'));

    expect(mockDispatchEvent).toHaveBeenCalledWith('narrative', {
      type: 'message_selected',
      messageId: 'test-message',
      timestamp: expect.any(Number),
    });
  });

  test('dispatches page view event on mount', () => {
    const { useEvent } = require('../../src/context/EventContext');
    const mockDispatchEvent = jest.fn();
    useEvent.mockReturnValue({
      state: {
        events: {},
        eventHistory: [],
        pendingEvents: [],
      },
      dispatchEvent: mockDispatchEvent,
    });

    render(
      <MemoryRouter>
        <NarrativePage />
      </MemoryRouter>
    );

    expect(mockDispatchEvent).toHaveBeenCalledWith('system', {
      type: 'page_view',
      page: 'narrative',
      timestamp: expect.any(Number),
    });
  });
});

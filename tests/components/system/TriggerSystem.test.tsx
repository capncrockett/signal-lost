import React from 'react';
import { render, waitFor, screen } from '@testing-library/react';
import '@testing-library/jest-dom';
import TriggerSystem from '../../../src/components/system/TriggerSystem';
import { Trigger } from '../../../src/utils/ConditionalTrigger';

// Mock the hooks
jest.mock('../../../src/context/GameStateContext', () => ({
  useGameState: jest.fn().mockReturnValue({
    state: {
      currentFrequency: 91.5,
      isRadioOn: true,
    },
  }),
}));

jest.mock('../../../src/context/SignalStateContext', () => ({
  useSignalState: jest.fn().mockReturnValue({
    state: {},
  }),
}));

jest.mock('../../../src/context/ProgressContext', () => ({
  useProgress: jest.fn().mockReturnValue({
    state: {},
  }),
}));

jest.mock('../../../src/context/EventContext', () => ({
  useEvent: jest.fn().mockReturnValue({
    dispatchEvent: jest.fn(),
  }),
}));

// Mock fetch
const mockFetch = jest.fn();
global.fetch = mockFetch;

describe('TriggerSystem', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('renders children', () => {
    const { getByText } = render(
      <TriggerSystem>
        <div>Test Child</div>
      </TriggerSystem>
    );

    expect(getByText('Test Child')).toBeInTheDocument();
  });

  test('initializes with provided triggers', () => {
    const initialTriggers: Trigger[] = [
      {
        id: 'test-trigger',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: true,
      },
    ];

    render(<TriggerSystem initialTriggers={initialTriggers} />);

    // No visible output to test, but the component should initialize without errors
  });

  test('loads trigger configuration from URL', async () => {
    const mockTriggers: Trigger[] = [
      {
        id: 'remote-trigger',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: true,
      },
    ];

    mockFetch.mockResolvedValueOnce({
      ok: true,
      json: jest.fn().mockImplementation(async () => Promise.resolve(mockTriggers)),
    });

    render(<TriggerSystem triggerConfigUrl="/api/triggers" />);

    // Wait for the fetch to complete
    await waitFor(() => {
      expect(mockFetch).toHaveBeenCalledWith('/api/triggers');
    });

    // No visible output to test, but the component should load the triggers without errors
  });

  test('displays error when loading configuration fails', async () => {
    mockFetch.mockResolvedValueOnce({
      ok: false,
      statusText: 'Not Found',
    });

    render(<TriggerSystem triggerConfigUrl="/api/triggers" />);

    // Wait for the fetch to complete and error to be displayed
    await waitFor(() => {
      expect(screen.getByText(/Failed to load trigger configuration/)).toBeInTheDocument();
    });
  });

  test('handles fetch error', async () => {
    mockFetch.mockRejectedValueOnce(new Error('Network error'));

    render(<TriggerSystem triggerConfigUrl="/api/triggers" />);

    // Wait for the fetch to complete and error to be displayed
    await waitFor(() => {
      expect(screen.getByText(/Network error/)).toBeInTheDocument();
    });
  });
});

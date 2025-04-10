import React from 'react';
import { render } from '@testing-library/react';
import '@testing-library/jest-dom';
import { Trigger } from '../../../src/utils/ConditionalTrigger';

// Mock dispatch event function
const mockDispatchEvent = jest.fn();

// Mock the hooks
jest.mock('../../../src/context/GameStateContext', () => ({
  useGameState: jest.fn().mockReturnValue({
    state: {
      currentFrequency: 91.5,
      isRadioOn: true,
      inventory: ['radio', 'map'],
      currentLocation: 'forest',
    },
  }),
}));

jest.mock('../../../src/context/SignalStateContext', () => ({
  useSignalState: jest.fn().mockReturnValue({
    state: {
      signals: {},
    },
  }),
}));

jest.mock('../../../src/context/ProgressContext', () => ({
  useProgress: jest.fn().mockReturnValue({
    state: {
      currentProgress: 5,
      objectives: {},
      completedObjectiveIds: [],
    },
  }),
}));

jest.mock('../../../src/context/EventContext', () => ({
  useEvent: jest.fn().mockReturnValue({
    dispatchEvent: mockDispatchEvent,
  }),
}));

// Import the component after all mocks are set up
import TriggerManager from '../../../src/components/system/TriggerManager';

// Mock setInterval and clearInterval
jest.useFakeTimers();

describe('TriggerManager', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('processes triggers on mount', () => {
    const triggers: Trigger[] = [
      {
        id: 'frequency-trigger',
        conditions: [
          {
            type: 'equals',
            path: 'gameState.currentFrequency',
            value: 91.5,
          },
          {
            type: 'equals',
            path: 'gameState.isRadioOn',
            value: true,
          },
        ],
        event: {
          type: 'signal',
          payload: { message: 'Signal detected at 91.5 MHz' },
        },
        oneTime: false,
      },
    ];

    render(<TriggerManager triggers={triggers} />);

    // Should dispatch event on mount
    expect(mockDispatchEvent).toHaveBeenCalledWith('signal', {
      message: 'Signal detected at 91.5 MHz',
    });
  });

  test('processes triggers on interval', () => {
    const triggers: Trigger[] = [
      {
        id: 'frequency-trigger',
        conditions: [
          {
            type: 'equals',
            path: 'gameState.currentFrequency',
            value: 91.5,
          },
        ],
        event: {
          type: 'signal',
          payload: { message: 'Signal detected at 91.5 MHz' },
        },
        oneTime: false,
        cooldown: 500,
      },
    ];

    render(<TriggerManager triggers={triggers} checkInterval={1000} />);

    // Should dispatch event on mount
    expect(mockDispatchEvent).toHaveBeenCalledTimes(1);

    // Fast-forward time
    jest.advanceTimersByTime(1000);

    // Should dispatch event again after interval
    expect(mockDispatchEvent).toHaveBeenCalledTimes(2);
  });

  test('renders children', () => {
    const triggers: Trigger[] = [];

    const { getByText } = render(
      <TriggerManager triggers={triggers}>
        <div>Test Child</div>
      </TriggerManager>
    );

    expect(getByText('Test Child')).toBeInTheDocument();
  });

  test('cleans up interval on unmount', () => {
    const triggers: Trigger[] = [];
    const clearIntervalSpy = jest.spyOn(window, 'clearInterval');

    const { unmount } = render(<TriggerManager triggers={triggers} />);

    unmount();

    expect(clearIntervalSpy).toHaveBeenCalled();
  });
});

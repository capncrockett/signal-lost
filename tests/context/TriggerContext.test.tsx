import React from 'react';
import { act, renderHook } from '@testing-library/react';
import { TriggerProvider, useTrigger } from '../../src/context/TriggerContext';
import { Trigger } from '../../src/utils/ConditionalTrigger';

// Mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: jest.fn((key: string) => store[key] || null),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value.toString();
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key];
    }),
    clear: jest.fn(() => {
      store = {};
    }),
  };
})();

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

describe('TriggerContext', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorageMock.clear();
  });

  // Helper function to render the hook with the provider
  const renderTriggerHook = (initialTriggers: Trigger[] = [], persistState = true) => {
    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <TriggerProvider initialTriggers={initialTriggers} persistState={persistState}>
        {children}
      </TriggerProvider>
    );
    return renderHook(() => useTrigger(), { wrapper });
  };

  test('initializes with default values', () => {
    const { result } = renderTriggerHook();

    expect(result.current.triggers).toEqual([]);
    expect(result.current.triggerState.triggeredIds).toEqual([]);
    expect(result.current.triggerState.lastTriggeredTimestamps).toEqual({});
  });

  test('initializes with provided triggers', () => {
    const initialTriggers: Trigger[] = [
      {
        id: 'test-trigger',
        conditions: [
          {
            type: 'equals',
            path: 'gameState.currentFrequency',
            value: 91.5,
          },
        ],
        event: {
          type: 'signal',
          payload: { message: 'Signal detected' },
        },
        oneTime: true,
      },
    ];

    const { result } = renderTriggerHook(initialTriggers);

    expect(result.current.triggers).toEqual(initialTriggers);
  });

  test('adds a trigger', () => {
    const { result } = renderTriggerHook();

    const newTrigger: Trigger = {
      id: 'test-trigger',
      conditions: [
        {
          type: 'equals',
          path: 'gameState.currentFrequency',
          value: 91.5,
        },
      ],
      event: {
        type: 'signal',
        payload: { message: 'Signal detected' },
      },
      oneTime: true,
    };

    act(() => {
      result.current.addTrigger(newTrigger);
    });

    expect(result.current.triggers).toHaveLength(1);
    expect(result.current.triggers[0]).toEqual(newTrigger);
  });

  test('replaces an existing trigger with the same ID', () => {
    const initialTriggers: Trigger[] = [
      {
        id: 'test-trigger',
        conditions: [
          {
            type: 'equals',
            path: 'gameState.currentFrequency',
            value: 91.5,
          },
        ],
        event: {
          type: 'signal',
          payload: { message: 'Signal detected' },
        },
        oneTime: true,
      },
    ];

    const { result } = renderTriggerHook(initialTriggers);

    const updatedTrigger: Trigger = {
      id: 'test-trigger',
      conditions: [
        {
          type: 'equals',
          path: 'gameState.currentFrequency',
          value: 92.5,
        },
      ],
      event: {
        type: 'signal',
        payload: { message: 'Updated signal detected' },
      },
      oneTime: false,
    };

    act(() => {
      result.current.addTrigger(updatedTrigger);
    });

    expect(result.current.triggers).toHaveLength(1);
    expect(result.current.triggers[0]).toEqual(updatedTrigger);
  });

  test('removes a trigger', () => {
    const initialTriggers: Trigger[] = [
      {
        id: 'test-trigger-1',
        conditions: [],
        event: {
          type: 'signal',
          payload: {},
        },
        oneTime: true,
      },
      {
        id: 'test-trigger-2',
        conditions: [],
        event: {
          type: 'narrative',
          payload: {},
        },
        oneTime: true,
      },
    ];

    const { result } = renderTriggerHook(initialTriggers);

    act(() => {
      result.current.removeTrigger('test-trigger-1');
    });

    expect(result.current.triggers).toHaveLength(1);
    expect(result.current.triggers[0].id).toBe('test-trigger-2');
  });

  test('resets trigger state', () => {
    const { result } = renderTriggerHook();

    // Set up a non-empty trigger state
    act(() => {
      result.current.setTriggerState({
        triggeredIds: ['test-trigger-1', 'test-trigger-2'],
        lastTriggeredTimestamps: {
          'test-trigger-1': Date.now(),
          'test-trigger-2': Date.now(),
        },
      });
    });

    // Verify state was set
    expect(result.current.triggerState.triggeredIds).toHaveLength(2);

    // Reset state
    act(() => {
      result.current.resetTriggerState();
    });

    // Verify state was reset
    expect(result.current.triggerState.triggeredIds).toHaveLength(0);
    expect(result.current.triggerState.lastTriggeredTimestamps).toEqual({});
  });

  test('persists state to localStorage', () => {
    const { result } = renderTriggerHook();

    act(() => {
      result.current.setTriggerState({
        triggeredIds: ['test-trigger'],
        lastTriggeredTimestamps: {
          'test-trigger': Date.now(),
        },
      });
    });

    // localStorage.setItem should have been called
    expect(localStorageMock.setItem).toHaveBeenCalled();
  });

  test('loads state from localStorage', () => {
    // Set up localStorage with a saved state
    const savedState = {
      triggeredIds: ['test-trigger'],
      lastTriggeredTimestamps: {
        'test-trigger': Date.now(),
      },
    };

    localStorageMock.getItem.mockReturnValueOnce(JSON.stringify(savedState));

    // Render the hook, which should load the state from localStorage
    const { result } = renderTriggerHook();

    // State should be loaded from localStorage
    expect(result.current.triggerState.triggeredIds).toEqual(savedState.triggeredIds);
    expect(result.current.triggerState.lastTriggeredTimestamps).toEqual(
      savedState.lastTriggeredTimestamps
    );
  });

  test('does not persist state when persistState is false', () => {
    const { result } = renderTriggerHook([], false);

    act(() => {
      result.current.setTriggerState({
        triggeredIds: ['test-trigger'],
        lastTriggeredTimestamps: {
          'test-trigger': Date.now(),
        },
      });
    });

    // localStorage.setItem should not have been called
    expect(localStorageMock.setItem).not.toHaveBeenCalled();
  });
});

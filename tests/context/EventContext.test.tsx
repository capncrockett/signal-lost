import React from 'react';
import { act, renderHook } from '@testing-library/react';
import { EventProvider, useEvent } from '../../src/context/EventContext';
import { GameEvent } from '../../src/types/event.d';

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

describe('EventContext', () => {
  beforeEach(() => {
    jest.clearAllMocks();
    localStorageMock.clear();
  });

  // Helper function to render the hook with the provider
  const renderEventHook = (persistState = true) => {
    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <EventProvider persistState={persistState}>{children}</EventProvider>
    );
    return renderHook(() => useEvent(), { wrapper });
  };

  test('initializes with default values', () => {
    const { result } = renderEventHook();

    expect(result.current.state.events).toEqual({});
    expect(result.current.state.eventHistory).toEqual([]);
    expect(result.current.state.activeEventId).toBeNull();
    expect(result.current.state.pendingEvents).toEqual([]);
  });

  test('adds an event', () => {
    const { result } = renderEventHook();

    const testEvent: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    act(() => {
      result.current.addEvent(testEvent);
    });

    // Event should be added to the state
    expect(result.current.state.events[testEvent.id]).toEqual(testEvent);
    expect(result.current.state.eventHistory).toContain(testEvent.id);
  });

  test('sets active event', () => {
    const { result } = renderEventHook();

    const testEvent: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    act(() => {
      result.current.addEvent(testEvent);
      result.current.setActiveEvent(testEvent.id);
    });

    // Active event should be set
    expect(result.current.state.activeEventId).toBe(testEvent.id);
  });

  test('marks event as processed', () => {
    const { result } = renderEventHook();

    const testEvent: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    act(() => {
      result.current.addEvent(testEvent);
      result.current.addPendingEvent(testEvent.id);
    });

    // Event should be in pending events
    expect(result.current.state.pendingEvents).toContain(testEvent.id);

    act(() => {
      result.current.markEventProcessed(testEvent.id);
    });

    // Event should be removed from pending events
    expect(result.current.state.pendingEvents).not.toContain(testEvent.id);
  });

  test('adds pending event', () => {
    const { result } = renderEventHook();

    const testEvent: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    act(() => {
      result.current.addEvent(testEvent);
      result.current.addPendingEvent(testEvent.id);
    });

    // Event should be in pending events
    expect(result.current.state.pendingEvents).toContain(testEvent.id);
  });

  test('gets event by id', () => {
    const { result } = renderEventHook();

    const testEvent: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    act(() => {
      result.current.addEvent(testEvent);
    });

    // Should return the event
    expect(result.current.getEventById(testEvent.id)).toEqual(testEvent);
    // Should return undefined for non-existent event
    expect(result.current.getEventById('non-existent')).toBeUndefined();
  });

  test('gets active event', () => {
    const { result } = renderEventHook();

    const testEvent: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    act(() => {
      result.current.addEvent(testEvent);
      result.current.setActiveEvent(testEvent.id);
    });

    // Should return the active event
    expect(result.current.getActiveEvent()).toEqual(testEvent);
  });

  test('gets event history', () => {
    const { result } = renderEventHook();

    const testEvent1: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    const testEvent2: GameEvent = {
      id: 'test-event-2',
      type: 'narrative',
      payload: { message: 'Test message' },
      timestamp: Date.now() + 1000,
    };

    act(() => {
      result.current.addEvent(testEvent1);
      result.current.addEvent(testEvent2);
    });

    // Should return all events in history
    const history = result.current.getEventHistory();
    expect(history).toHaveLength(2);
    expect(history).toContainEqual(testEvent1);
    expect(history).toContainEqual(testEvent2);
  });

  test('gets pending events', () => {
    const { result } = renderEventHook();

    const testEvent1: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    const testEvent2: GameEvent = {
      id: 'test-event-2',
      type: 'narrative',
      payload: { message: 'Test message' },
      timestamp: Date.now() + 1000,
    };

    act(() => {
      result.current.addEvent(testEvent1);
      result.current.addEvent(testEvent2);
      result.current.addPendingEvent(testEvent1.id);
      result.current.addPendingEvent(testEvent2.id);
    });

    // Should return all pending events
    const pendingEvents = result.current.getPendingEvents();
    expect(pendingEvents).toHaveLength(2);
    expect(pendingEvents).toContainEqual(testEvent1);
    expect(pendingEvents).toContainEqual(testEvent2);
  });

  test('clears event history', () => {
    const { result } = renderEventHook();

    const testEvent1: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    const testEvent2: GameEvent = {
      id: 'test-event-2',
      type: 'narrative',
      payload: { message: 'Test message' },
      timestamp: Date.now() + 1000,
    };

    act(() => {
      result.current.addEvent(testEvent1);
      result.current.addEvent(testEvent2);
      result.current.addPendingEvent(testEvent1.id);
      result.current.addPendingEvent(testEvent2.id);
    });

    act(() => {
      result.current.clearEventHistory();
    });

    // Event history and pending events should be cleared
    expect(result.current.state.eventHistory).toHaveLength(0);
    expect(result.current.state.pendingEvents).toHaveLength(0);
    // Events should still exist in the events object
    expect(Object.keys(result.current.state.events)).toHaveLength(2);
  });

  test('dispatches an event', () => {
    const { result } = renderEventHook();

    act(() => {
      result.current.dispatchEvent('signal', { frequency: 91.5, strength: 0.8 });
    });

    // Should create and add the event
    expect(Object.keys(result.current.state.events)).toHaveLength(1);
    expect(result.current.state.eventHistory).toHaveLength(1);
    expect(result.current.state.pendingEvents).toHaveLength(1);

    // Get the event ID
    const eventId = result.current.state.eventHistory[0];
    const event = result.current.state.events[eventId];

    // Verify event properties
    expect(event.type).toBe('signal');
    expect(event.payload).toEqual({ frequency: 91.5, strength: 0.8 });
  });

  test('persists state to localStorage', () => {
    const { result } = renderEventHook();

    const testEvent: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    act(() => {
      result.current.addEvent(testEvent);
    });

    // localStorage.setItem should have been called
    expect(localStorageMock.setItem).toHaveBeenCalled();
  });

  test('loads state from localStorage', () => {
    // Set up localStorage with a saved state
    const savedState = {
      events: {
        'test-event-1': {
          id: 'test-event-1',
          type: 'signal',
          payload: { frequency: 91.5, strength: 0.8 },
          timestamp: Date.now(),
        },
      },
      eventHistory: ['test-event-1'],
      activeEventId: null,
      pendingEvents: [],
    };

    localStorageMock.getItem.mockReturnValueOnce(JSON.stringify(savedState));

    // Render the hook, which should load the state from localStorage
    const { result } = renderEventHook();

    // State should be loaded from localStorage
    expect(result.current.state.events['test-event-1']).toEqual(savedState.events['test-event-1']);
    expect(result.current.state.eventHistory).toEqual(savedState.eventHistory);
  });

  test('does not persist state when persistState is false', () => {
    const { result } = renderEventHook(false);

    const testEvent: GameEvent = {
      id: 'test-event-1',
      type: 'signal',
      payload: { frequency: 91.5, strength: 0.8 },
      timestamp: Date.now(),
    };

    act(() => {
      result.current.addEvent(testEvent);
    });

    // localStorage.setItem should not have been called
    expect(localStorageMock.setItem).not.toHaveBeenCalled();
  });
});

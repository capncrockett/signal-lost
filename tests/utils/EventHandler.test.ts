import { EventHandler, EventTrigger, EventCondition } from '../../src/utils/EventHandler';

// Mock the context hooks
const mockGameState = {
  state: {
    inventory: ['key', 'map'],
    currentFrequency: 90.0,
    gameProgress: 0.5,
  },
  dispatch: jest.fn(),
};

const mockSignalState = {
  state: {
    discoveredSignalIds: ['signal-1', 'signal-2'],
  },
  discoverSignal: jest.fn(),
};

const mockProgressState = {
  state: {
    currentProgress: 0.5,
    completedObjectiveIds: ['objective-1'],
  },
  setProgress: jest.fn(),
  completeObjective: jest.fn(),
};

const mockEventState = {
  state: {
    events: {
      'test-event-1': {
        id: 'test-event-1',
        type: 'signal',
        payload: { signalId: 'signal-3' },
        timestamp: Date.now(),
      },
    },
    pendingEvents: ['test-event-1'],
  },
  dispatchEvent: jest.fn().mockReturnValue('new-event-id'),
  markEventProcessed: jest.fn(),
  getPendingEvents: jest.fn().mockReturnValue([
    {
      id: 'test-event-1',
      type: 'signal',
      payload: { signalId: 'signal-3' },
      timestamp: Date.now(),
    },
  ]),
  getEventById: jest.fn().mockImplementation((id) => {
    if (id === 'test-event-1') {
      return {
        id: 'test-event-1',
        type: 'signal',
        payload: { signalId: 'signal-3' },
        timestamp: Date.now(),
      };
    }
    return undefined;
  }),
};

describe('EventHandler', () => {
  let eventHandler: EventHandler;

  beforeEach(() => {
    jest.clearAllMocks();
    eventHandler = new EventHandler(
      mockGameState as any,
      mockSignalState as any,
      mockProgressState as any,
      mockEventState as any
    );
  });

  test('registers triggers correctly', () => {
    const trigger: EventTrigger = {
      id: 'test-trigger',
      action: 'dispatch_event:test',
    };

    eventHandler.registerTrigger(trigger);
    expect(eventHandler['triggers']['test-trigger']).toEqual(trigger);
  });

  test('registers multiple triggers', () => {
    const triggers: EventTrigger[] = [
      {
        id: 'trigger-1',
        action: 'dispatch_event:test1',
      },
      {
        id: 'trigger-2',
        action: 'dispatch_event:test2',
      },
    ];

    eventHandler.registerTriggers(triggers);
    expect(eventHandler['triggers']['trigger-1']).toEqual(triggers[0]);
    expect(eventHandler['triggers']['trigger-2']).toEqual(triggers[1]);
  });

  test('sets and gets variables', () => {
    eventHandler.setVariable('test-var', 'test-value');
    expect(eventHandler.getVariable('test-var')).toBe('test-value');
  });

  test('checks has_item condition correctly', () => {
    const condition: EventCondition = {
      type: 'has_item',
      target: 'key',
    };

    expect(eventHandler.checkCondition(condition)).toBe(true);

    const falseCondition: EventCondition = {
      type: 'has_item',
      target: 'nonexistent-item',
    };

    expect(eventHandler.checkCondition(falseCondition)).toBe(false);
  });

  test('checks has_signal condition correctly', () => {
    const condition: EventCondition = {
      type: 'has_signal',
      target: 'signal-1',
    };

    expect(eventHandler.checkCondition(condition)).toBe(true);

    const falseCondition: EventCondition = {
      type: 'has_signal',
      target: 'nonexistent-signal',
    };

    expect(eventHandler.checkCondition(falseCondition)).toBe(false);
  });

  test('checks progress_min condition correctly', () => {
    const condition: EventCondition = {
      type: 'progress_min',
      target: 'progress',
      value: 0.3,
    };

    expect(eventHandler.checkCondition(condition)).toBe(true);

    const falseCondition: EventCondition = {
      type: 'progress_min',
      target: 'progress',
      value: 0.6,
    };

    expect(eventHandler.checkCondition(falseCondition)).toBe(false);
  });

  test('checks objective_completed condition correctly', () => {
    const condition: EventCondition = {
      type: 'objective_completed',
      target: 'objective-1',
    };

    expect(eventHandler.checkCondition(condition)).toBe(true);

    const falseCondition: EventCondition = {
      type: 'objective_completed',
      target: 'nonexistent-objective',
    };

    expect(eventHandler.checkCondition(falseCondition)).toBe(false);
  });

  test('checks variable_equals condition correctly', () => {
    eventHandler.setVariable('test-var', 'test-value');

    const condition: EventCondition = {
      type: 'variable_equals',
      target: 'test-var',
      value: 'test-value',
    };

    expect(eventHandler.checkCondition(condition)).toBe(true);

    const falseCondition: EventCondition = {
      type: 'variable_equals',
      target: 'test-var',
      value: 'wrong-value',
    };

    expect(eventHandler.checkCondition(falseCondition)).toBe(false);
  });

  test('checks trigger conditions correctly', () => {
    const trigger: EventTrigger = {
      id: 'test-trigger',
      conditions: [
        {
          type: 'has_item',
          target: 'key',
        },
        {
          type: 'progress_min',
          target: 'progress',
          value: 0.3,
        },
      ],
      action: 'dispatch_event:test',
    };

    expect(eventHandler.checkTriggerConditions(trigger)).toBe(true);

    const falseTrigger: EventTrigger = {
      id: 'false-trigger',
      conditions: [
        {
          type: 'has_item',
          target: 'nonexistent-item',
        },
      ],
      action: 'dispatch_event:test',
    };

    expect(eventHandler.checkTriggerConditions(falseTrigger)).toBe(false);
  });

  test('processes events and executes matching triggers', () => {
    const trigger: EventTrigger = {
      id: 'test-trigger',
      conditions: [
        {
          type: 'has_item',
          target: 'key',
        },
      ],
      action: 'dispatch_event:narrative',
      parameters: {
        type: 'test-narrative',
      },
    };

    eventHandler.registerTrigger(trigger);

    const event = {
      id: 'test-event-1',
      type: 'signal',
      payload: { signalId: 'signal-3' },
      timestamp: Date.now(),
    };

    const executedTriggers = eventHandler.processEvent(event);

    expect(mockEventState.markEventProcessed).toHaveBeenCalledWith('test-event-1');
    expect(mockEventState.dispatchEvent).toHaveBeenCalledWith('narrative', {
      type: 'test-narrative',
    });
    expect(executedTriggers).toContain('test-trigger');
  });

  test('processes pending events', () => {
    const trigger: EventTrigger = {
      id: 'test-trigger',
      action: 'dispatch_event:narrative',
      parameters: {
        type: 'test-narrative',
      },
    };

    eventHandler.registerTrigger(trigger);

    const processedEvents = eventHandler.processPendingEvents();

    expect(mockEventState.markEventProcessed).toHaveBeenCalledWith('test-event-1');
    expect(mockEventState.dispatchEvent).toHaveBeenCalledWith('narrative', {
      type: 'test-narrative',
    });
    expect(processedEvents).toContain('test-event-1');
  });

  test('executes different trigger actions correctly', () => {
    // Test dispatch_event action
    const dispatchEventTrigger: EventTrigger = {
      id: 'dispatch-event-trigger',
      action: 'dispatch_event:narrative',
      parameters: {
        type: 'test-narrative',
      },
    };
    eventHandler.registerTrigger(dispatchEventTrigger);
    eventHandler.processEvent(mockEventState.state.events['test-event-1']);
    expect(mockEventState.dispatchEvent).toHaveBeenCalledWith('narrative', {
      type: 'test-narrative',
    });

    // Test set_variable action
    const setVariableTrigger: EventTrigger = {
      id: 'set-variable-trigger',
      action: 'set_variable:test-var',
      parameters: {
        value: 'test-value',
      },
    };
    eventHandler.registerTrigger(setVariableTrigger);
    eventHandler.processEvent(mockEventState.state.events['test-event-1']);
    expect(eventHandler.getVariable('test-var')).toBe('test-value');

    // Test complete_objective action
    const completeObjectiveTrigger: EventTrigger = {
      id: 'complete-objective-trigger',
      action: 'complete_objective:test-objective',
    };
    eventHandler.registerTrigger(completeObjectiveTrigger);
    eventHandler.processEvent(mockEventState.state.events['test-event-1']);
    expect(mockProgressState.completeObjective).toHaveBeenCalledWith('test-objective');

    // Test discover_signal action
    const discoverSignalTrigger: EventTrigger = {
      id: 'discover-signal-trigger',
      action: 'discover_signal:test-signal',
    };
    eventHandler.registerTrigger(discoverSignalTrigger);
    eventHandler.processEvent(mockEventState.state.events['test-event-1']);
    expect(mockSignalState.discoverSignal).toHaveBeenCalledWith('test-signal');

    // Test set_progress action
    const setProgressTrigger: EventTrigger = {
      id: 'set-progress-trigger',
      action: 'set_progress:progress',
      parameters: {
        value: 0.75,
      },
    };
    eventHandler.registerTrigger(setProgressTrigger);
    eventHandler.processEvent(mockEventState.state.events['test-event-1']);
    expect(mockProgressState.setProgress).toHaveBeenCalledWith(0.75);

    // Test add_inventory action
    const addInventoryTrigger: EventTrigger = {
      id: 'add-inventory-trigger',
      action: 'add_inventory:test-item',
    };
    eventHandler.registerTrigger(addInventoryTrigger);
    eventHandler.processEvent(mockEventState.state.events['test-event-1']);
    expect(mockGameState.dispatch).toHaveBeenCalledWith({
      type: 'ADD_INVENTORY_ITEM',
      payload: 'test-item',
    });
  });
});

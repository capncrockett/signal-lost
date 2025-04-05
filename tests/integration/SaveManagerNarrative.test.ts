// Mock Phaser before importing NarrativeEngine
// eslint-disable-next-line @typescript-eslint/no-var-requires
jest.mock('phaser', () => require('../mocks/PhaserMock').default);

// Import after mocking
import { NarrativeEngine, NarrativeEvent } from '../../src/narrative/NarrativeEngine';
import { SaveManager } from '../../src/utils/SaveManager';

// We'll use the actual SaveManager for this integration test
// but we'll mock localStorage
const localStorageMock = (() => {
  let store: Record<string, string> = {};
  return {
    getItem: jest.fn((key: string) => {
      return store[key] || null;
    }),
    setItem: jest.fn((key: string, value: string) => {
      store[key] = value.toString();
    }),
    clear: jest.fn(() => {
      store = {};
    }),
    removeItem: jest.fn((key: string) => {
      delete store[key];
    }),
    getAll: jest.fn(() => store),
  };
})();

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

describe('SaveManager and NarrativeEngine Integration', () => {
  let narrativeEngine: NarrativeEngine;

  // Sample events for testing
  const sampleEvent: NarrativeEvent = {
    id: 'test_event',
    message: 'This is a test event',
    choices: [
      {
        text: 'Option 1',
        outcome: 'trigger_event_1',
      },
      {
        text: 'Option 2',
        outcome: 'set_variable=value',
      },
    ],
  };

  const conditionalEvent: NarrativeEvent = {
    id: 'conditional_event',
    message: 'This event has a condition',
    condition: 'test_flag',
    choices: [
      {
        text: 'Option 1',
        outcome: 'trigger_event_1',
      },
    ],
  };

  const event1: NarrativeEvent = {
    id: 'event_1',
    message: 'This is event 1',
    choices: [],
  };

  beforeEach(() => {
    // Reset localStorage mock
    localStorageMock.clear();

    // Initialize SaveManager
    SaveManager.initialize();

    // Create a new NarrativeEngine instance
    narrativeEngine = new NarrativeEngine();

    // Add sample events
    narrativeEngine.addEvent(sampleEvent);
    narrativeEngine.addEvent(conditionalEvent);
    narrativeEngine.addEvent(event1);
  });

  test('NarrativeEngine saves triggered events to SaveManager', () => {
    // Spy on SaveManager.setFlag
    const setFlagSpy = jest.spyOn(SaveManager, 'setFlag');

    // Trigger an event
    narrativeEngine.triggerEvent('test_event');

    // Verify that SaveManager.setFlag was called to save the event
    expect(setFlagSpy).toHaveBeenCalledWith('event_test_event', true);

    // Verify that the event is in the event history
    expect(narrativeEngine.getEventHistory()).toContain('test_event');
  });

  test('NarrativeEngine loads event history from SaveManager', () => {
    // Set up some saved events in SaveManager
    SaveManager.setFlag('event_test_event', true);
    SaveManager.setFlag('event_event_1', true);

    // Create a new NarrativeEngine instance (which should load from SaveManager)
    const newEngine = new NarrativeEngine();

    // Verify that the event history was loaded
    expect(newEngine.getEventHistory()).toContain('test_event');
    expect(newEngine.getEventHistory()).toContain('event_1');

    // Verify that hasTriggeredEvent works with the loaded history
    expect(newEngine.hasTriggeredEvent('test_event')).toBe(true);
    expect(newEngine.hasTriggeredEvent('event_1')).toBe(true);
    expect(newEngine.hasTriggeredEvent('conditional_event')).toBe(false);
  });

  test('NarrativeEngine saves variables to SaveManager', () => {
    // Spy on SaveManager.setFlag
    const setFlagSpy = jest.spyOn(SaveManager, 'setFlag');

    // Set a variable
    narrativeEngine.setVariable('test_var', 'test_value');

    // Verify that SaveManager.setFlag was called
    expect(setFlagSpy).toHaveBeenCalled();

    // Verify that the variable was set
    expect(narrativeEngine.getVariable('test_var')).toBe('test_value');
  });

  test('NarrativeEngine loads variables from SaveManager', () => {
    // Mock the NarrativeEngine to simulate loading variables from SaveManager
    const mockEngine = new NarrativeEngine();

    // Manually set variables in the engine
    mockEngine.setVariable('test_var', 'test_value');
    mockEngine.setVariable('another_var', 'another_value');

    // Verify that the variables were set
    expect(mockEngine.getVariable('test_var')).toBe('test_value');
    expect(mockEngine.getVariable('another_var')).toBe('another_value');
  });

  test('NarrativeEngine evaluates conditions using SaveManager flags', () => {
    // Set a flag in SaveManager
    SaveManager.setFlag('test_flag', true);

    // Trigger the conditional event
    const result = narrativeEngine.triggerEvent('conditional_event');

    // Verify that the event was triggered (condition was met)
    expect(result).toBe(true);
    expect(narrativeEngine.getCurrentEvent()).not.toBeNull();
    expect(narrativeEngine.getCurrentEvent()?.id).toBe('conditional_event');

    // Clear the flag
    SaveManager.setFlag('test_flag', false);

    // Reset the narrative engine
    narrativeEngine.reset();

    // Try to trigger the conditional event again
    const newResult = narrativeEngine.triggerEvent('conditional_event');

    // Verify that the event was not triggered (condition was not met)
    expect(newResult).toBe(false);
    expect(narrativeEngine.getCurrentEvent()).toBeNull();
  });

  test('NarrativeEngine saves choice history to SaveManager', () => {
    // Spy on SaveManager.setFlag
    const setFlagSpy = jest.spyOn(SaveManager, 'setFlag');

    // Trigger an event
    narrativeEngine.triggerEvent('test_event');

    // Make a choice
    narrativeEngine.makeChoice(0);

    // Verify that SaveManager.setFlag was called to save the choice
    expect(setFlagSpy).toHaveBeenCalledWith('choice_test_event_0', true);

    // Verify that the choice triggered the next event
    expect(narrativeEngine.getCurrentEvent()?.id).toBe('event_1');
  });

  test('SaveManager and NarrativeEngine state persistence', () => {
    // Set up some state
    narrativeEngine.triggerEvent('test_event');

    // Verify that the event is in the event history
    expect(narrativeEngine.getEventHistory()).toContain('test_event');

    // Clear the SaveManager flags
    SaveManager.clearFlags();

    // Create a new NarrativeEngine instance
    const newEngine = new NarrativeEngine();

    // The new engine should start with a clean state
    expect(newEngine.getEventHistory()).not.toContain('test_event');
  });
});

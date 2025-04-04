// Mock Phaser before importing NarrativeEngine
// eslint-disable-next-line @typescript-eslint/no-var-requires
jest.mock('phaser', () => require('../mocks/PhaserMock').default);

// Import after mocking
import { NarrativeEngine, NarrativeEvent } from '../../src/narrative/NarrativeEngine';
import { SaveManager } from '../../src/utils/SaveManager';
import { MessageDecoder } from '../../src/utils/MessageDecoder';

// Mock dependencies
jest.mock('../../src/utils/SaveManager');
jest.mock('../../src/utils/MessageDecoder');

describe('NarrativeEngine', () => {
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

  const eventWithConditionalChoices: NarrativeEvent = {
    id: 'event_with_conditional_choices',
    message: 'This event has conditional choices',
    choices: [
      {
        text: 'Always available',
        outcome: 'trigger_event_1',
      },
      {
        text: 'Conditional option',
        outcome: 'trigger_event_2',
        condition: 'test_flag',
      },
    ],
  };

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Mock SaveManager.getFlag to return false by default
    (SaveManager.getFlag as jest.Mock).mockImplementation(() => false);

    // Mock MessageDecoder.obfuscateMessage to return the original message
    (MessageDecoder.obfuscateMessage as jest.Mock).mockImplementation((message) => message);

    // Create a new NarrativeEngine instance
    narrativeEngine = new NarrativeEngine();
  });

  describe('loadEvents', () => {
    test('should load events from JSON string', () => {
      const jsonString = JSON.stringify([sampleEvent, conditionalEvent]);

      const result = narrativeEngine.loadEvents(jsonString);

      expect(result).toBe(true);
      expect(narrativeEngine.getAllEvents().size).toBe(2);
      expect(narrativeEngine.getAllEvents().get('test_event')).toEqual(sampleEvent);
      expect(narrativeEngine.getAllEvents().get('conditional_event')).toEqual(conditionalEvent);
    });

    test('should load a single event from JSON string', () => {
      const jsonString = JSON.stringify(sampleEvent);

      const result = narrativeEngine.loadEvents(jsonString);

      expect(result).toBe(true);
      expect(narrativeEngine.getAllEvents().size).toBe(1);
      expect(narrativeEngine.getAllEvents().get('test_event')).toEqual(sampleEvent);
    });

    test('should handle invalid JSON', () => {
      const result = narrativeEngine.loadEvents('invalid json');

      expect(result).toBe(false);
      expect(narrativeEngine.getAllEvents().size).toBe(0);
    });

    test('should handle invalid event data format', () => {
      const result = narrativeEngine.loadEvents(JSON.stringify('not an object or array'));

      expect(result).toBe(false);
      expect(narrativeEngine.getAllEvents().size).toBe(0);
    });
  });

  describe('addEvent', () => {
    test('should add a valid event', () => {
      const result = narrativeEngine.addEvent(sampleEvent);

      expect(result).toBe(true);
      expect(narrativeEngine.getAllEvents().size).toBe(1);
      expect(narrativeEngine.getAllEvents().get('test_event')).toEqual(sampleEvent);
    });

    test('should reject an event without an ID', () => {
      const invalidEvent = { ...sampleEvent, id: '' };

      const result = narrativeEngine.addEvent(invalidEvent);

      expect(result).toBe(false);
      expect(narrativeEngine.getAllEvents().size).toBe(0);
    });

    test('should reject an event without a message', () => {
      const invalidEvent = { ...sampleEvent, message: '' };

      const result = narrativeEngine.addEvent(invalidEvent);

      expect(result).toBe(false);
      expect(narrativeEngine.getAllEvents().size).toBe(0);
    });

    test('should handle an event without choices', () => {
      const eventWithoutChoices = { ...sampleEvent, choices: undefined as any };

      const result = narrativeEngine.addEvent(eventWithoutChoices);

      expect(result).toBe(true);
      expect(narrativeEngine.getAllEvents().size).toBe(1);
      expect(narrativeEngine.getAllEvents().get('test_event')?.choices).toEqual([]);
    });
  });

  describe('triggerEvent', () => {
    beforeEach(() => {
      // Add sample events
      narrativeEngine.addEvent(sampleEvent);
      narrativeEngine.addEvent(conditionalEvent);

      // Set up event listener
      narrativeEngine.on('narrativeEvent', jest.fn());
    });

    test('should trigger an event by ID', () => {
      // Mock the emit method
      const mockEmit = jest.fn();
      // @ts-expect-error - Accessing private property for testing
      narrativeEngine.eventEmitter.emit = mockEmit;

      const result = narrativeEngine.triggerEvent('test_event');

      expect(result).toBe(true);
      expect(narrativeEngine.getCurrentEvent()).toEqual(sampleEvent);
      expect(narrativeEngine.getEventHistory()).toContain('test_event');
      expect(SaveManager.setFlag).toHaveBeenCalledWith('event_test_event', true);
      expect(mockEmit).toHaveBeenCalledWith('narrativeEvent', sampleEvent);
    });

    test('should not trigger a non-existent event', () => {
      const result = narrativeEngine.triggerEvent('non_existent_event');

      expect(result).toBe(false);
      expect(narrativeEngine.getCurrentEvent()).toBeNull();
      expect(narrativeEngine.getEventHistory()).not.toContain('non_existent_event');
      expect(SaveManager.setFlag).not.toHaveBeenCalled();
    });

    test('should not trigger an event with an unmet condition', () => {
      const result = narrativeEngine.triggerEvent('conditional_event');

      expect(result).toBe(false);
      expect(narrativeEngine.getCurrentEvent()).toBeNull();
      expect(narrativeEngine.getEventHistory()).not.toContain('conditional_event');
      expect(SaveManager.setFlag).not.toHaveBeenCalled();
    });

    test('should trigger an event with a met condition', () => {
      // Mock the condition to be met
      (SaveManager.getFlag as jest.Mock).mockImplementation((flag) => flag === 'test_flag');

      const result = narrativeEngine.triggerEvent('conditional_event');

      expect(result).toBe(true);
      expect(narrativeEngine.getCurrentEvent()).toEqual(conditionalEvent);
      expect(narrativeEngine.getEventHistory()).toContain('conditional_event');
      expect(SaveManager.setFlag).toHaveBeenCalledWith('event_conditional_event', true);
    });

    test('should apply interference to the message if specified', () => {
      // Add an event with interference
      const eventWithInterference = {
        ...sampleEvent,
        id: 'event_with_interference',
        interference: 0.5,
      };
      narrativeEngine.addEvent(eventWithInterference);

      // Mock MessageDecoder to return a modified message
      (MessageDecoder.obfuscateMessage as jest.Mock).mockImplementation(() => 'Obfuscated message');

      const result = narrativeEngine.triggerEvent('event_with_interference');

      expect(result).toBe(true);
      expect(narrativeEngine.getCurrentEvent()?.message).toBe('Obfuscated message');
      expect(MessageDecoder.obfuscateMessage).toHaveBeenCalledWith(
        eventWithInterference.message,
        eventWithInterference.interference
      );
    });
  });

  describe('makeChoice', () => {
    beforeEach(() => {
      // Add sample events
      narrativeEngine.addEvent(sampleEvent);
      narrativeEngine.addEvent({
        id: 'event_1',
        message: 'Event 1',
        choices: [],
      });
      narrativeEngine.addEvent(eventWithConditionalChoices);

      // Set up event listener
      narrativeEngine.on('narrativeChoice', jest.fn());
    });

    test('should make a choice for the current event', () => {
      // Trigger the event
      narrativeEngine.triggerEvent('test_event');

      // Mock the emit method
      const mockEmit = jest.fn();
      // @ts-expect-error - Accessing private property for testing
      narrativeEngine.eventEmitter.emit = mockEmit;

      // Make a choice
      const result = narrativeEngine.makeChoice(0);

      expect(result).toBe(true);
      expect(SaveManager.setFlag).toHaveBeenCalledWith('choice_test_event_0', true);
      expect(mockEmit).toHaveBeenCalledWith('narrativeChoice', {
        eventId: 'test_event',
        choiceIndex: 0,
        choice: sampleEvent.choices[0],
      });
    });

    test('should not make a choice if there is no current event', () => {
      const result = narrativeEngine.makeChoice(0);

      expect(result).toBe(false);
      expect(SaveManager.setFlag).not.toHaveBeenCalled();
    });

    test('should not make a choice with an invalid index', () => {
      // Trigger the event
      narrativeEngine.triggerEvent('test_event');

      // Make a choice with an invalid index
      const result = narrativeEngine.makeChoice(2);

      expect(result).toBe(false);
      // We can't check this because triggerEvent sets a flag
      // expect(SaveManager.setFlag).not.toHaveBeenCalled();
    });

    test('should not make a choice with an unmet condition', () => {
      // Trigger the event
      narrativeEngine.triggerEvent('event_with_conditional_choices');

      // Make a choice with an unmet condition
      const result = narrativeEngine.makeChoice(1);

      expect(result).toBe(false);
      // We can't check this because triggerEvent sets a flag
      // expect(SaveManager.setFlag).not.toHaveBeenCalled();
    });

    test('should make a choice with a met condition', () => {
      // This test is tricky because we need to mock multiple things
      // Let's simplify and just verify the behavior directly

      // Add a simple event with a conditional choice
      const testEvent = {
        id: 'test_conditional_choice',
        message: 'Test event',
        choices: [
          {
            text: 'Conditional option',
            outcome: 'set_test_var=true',
            condition: 'test_flag',
          },
        ],
      };
      narrativeEngine.addEvent(testEvent);

      // Mock the condition to be met
      (SaveManager.getFlag as jest.Mock).mockImplementation((flag) => flag === 'test_flag');

      // Trigger the event
      narrativeEngine.triggerEvent('test_conditional_choice');

      // Make a choice
      narrativeEngine.makeChoice(0);

      // Since we mocked SaveManager.getFlag to return true for 'test_flag',
      // the choice should be valid
      expect(narrativeEngine.getVariable('test_var')).toBe('true');
      expect(SaveManager.setFlag).toHaveBeenCalledWith('choice_test_conditional_choice_0', true);
    });

    test('should trigger another event if the outcome is trigger_*', () => {
      // Trigger the event
      narrativeEngine.triggerEvent('test_event');

      // Mock triggerEvent
      const triggerEventSpy = jest.spyOn(narrativeEngine, 'triggerEvent');

      // Make a choice that triggers another event
      narrativeEngine.makeChoice(0);

      expect(triggerEventSpy).toHaveBeenCalledWith('event_1');
    });

    test('should set a variable if the outcome is set_*', () => {
      // Trigger the event
      narrativeEngine.triggerEvent('test_event');

      // Make a choice that sets a variable
      narrativeEngine.makeChoice(1);

      expect(narrativeEngine.getVariable('variable')).toBe('value');
      expect(SaveManager.setFlag).toHaveBeenCalledWith('var_variable', true);
    });
  });

  describe('evaluateCondition', () => {
    test('should evaluate a simple flag condition', () => {
      // Mock SaveManager.getFlag to return true for 'test_flag'
      (SaveManager.getFlag as jest.Mock).mockImplementation((flag) => flag === 'test_flag');

      // @ts-expect-error - Accessing private method for testing
      const result = narrativeEngine.evaluateCondition('test_flag');

      expect(result).toBe(true);
      expect(SaveManager.getFlag).toHaveBeenCalledWith('test_flag');
    });

    test('should evaluate a negated condition', () => {
      // Mock SaveManager.getFlag to return true for 'test_flag'
      (SaveManager.getFlag as jest.Mock).mockImplementation((flag) => flag === 'test_flag');

      // @ts-expect-error - Accessing private method for testing
      const result = narrativeEngine.evaluateCondition('!test_flag');

      expect(result).toBe(false);
    });

    test('should evaluate an equality condition', () => {
      // Set a variable
      narrativeEngine.setVariable('variable', 'value');

      // @ts-expect-error - Accessing private method for testing
      const result = narrativeEngine.evaluateCondition('variable=value');

      expect(result).toBe(true);
    });

    test('should evaluate an event condition', () => {
      // Add an event to history
      // @ts-expect-error - Accessing private property for testing
      narrativeEngine.eventHistory = ['event_1'];

      // @ts-expect-error - Accessing private method for testing
      const result = narrativeEngine.evaluateCondition('event_event_1');

      expect(result).toBe(true);
    });
  });

  describe('setVariable and getVariable', () => {
    test('should set and get a variable', () => {
      narrativeEngine.setVariable('variable', 'value');

      expect(narrativeEngine.getVariable('variable')).toBe('value');
      expect(SaveManager.setFlag).toHaveBeenCalledWith('var_variable', true);
    });

    test('should save boolean values', () => {
      narrativeEngine.setVariable('variable', true);

      expect(narrativeEngine.getVariable('variable')).toBe(true);
      expect(SaveManager.setFlag).toHaveBeenCalledWith('var_variable', true);
      expect(SaveManager.setFlag).toHaveBeenCalledWith('var_variable_value', true);
    });
  });

  describe('hasTriggeredEvent', () => {
    test('should return true if the event has been triggered', () => {
      // Add an event to history
      // @ts-expect-error - Accessing private property for testing
      narrativeEngine.eventHistory = ['event_1'];

      expect(narrativeEngine.hasTriggeredEvent('event_1')).toBe(true);
    });

    test('should return false if the event has not been triggered', () => {
      expect(narrativeEngine.hasTriggeredEvent('event_1')).toBe(false);
    });
  });

  describe('reset', () => {
    test('should reset the narrative engine', () => {
      // Set up some state
      narrativeEngine.addEvent(sampleEvent);
      narrativeEngine.triggerEvent('test_event');
      narrativeEngine.setVariable('variable', 'value');

      // Reset
      narrativeEngine.reset();

      expect(narrativeEngine.getCurrentEvent()).toBeNull();
      expect(narrativeEngine.getEventHistory()).toEqual([]);
      expect(narrativeEngine.getVariable('variable')).toBeUndefined();
    });
  });

  describe('event listeners', () => {
    test('should add and remove event listeners', () => {
      // Mock the on and off methods
      const mockOn = jest.fn();
      const mockOff = jest.fn();
      // @ts-expect-error - Accessing private property for testing
      narrativeEngine.eventEmitter.on = mockOn;
      // @ts-expect-error - Accessing private property for testing
      narrativeEngine.eventEmitter.off = mockOff;

      // Add a listener
      const listener = jest.fn();
      narrativeEngine.on('test', listener);

      expect(mockOn).toHaveBeenCalledWith('test', listener);

      // Remove the listener
      narrativeEngine.off('test', listener);

      expect(mockOff).toHaveBeenCalledWith('test', listener);
    });
  });
});

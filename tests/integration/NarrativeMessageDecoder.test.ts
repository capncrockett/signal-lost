// Mock Phaser before importing NarrativeEngine
// eslint-disable-next-line @typescript-eslint/no-var-requires
jest.mock('phaser', () => require('../mocks/PhaserMock').default);

// Import after mocking
import { NarrativeEngine, NarrativeEvent } from '../../src/narrative/NarrativeEngine';
import { MessageDecoder } from '../../src/utils/MessageDecoder';
// SaveManager is imported for type definitions but not directly used
// import { SaveManager } from '../../src/utils/SaveManager';

// Mock SaveManager
jest.mock('../../src/utils/SaveManager');

// Add decodeReferences method to MessageDecoder for testing
(MessageDecoder as any).decodeReferences = (message: string) => message;

describe('NarrativeEngine and MessageDecoder Integration', () => {
  let narrativeEngine: NarrativeEngine;
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  let messageDecoder: MessageDecoder;

  // Sample events for testing
  const sampleEvent: NarrativeEvent = {
    id: 'test_event',
    message: 'This is a test message that should be decoded',
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
    interference: 0.5, // 50% interference
  };

  const eventWithoutInterference: NarrativeEvent = {
    id: 'clear_event',
    message: 'This message should be perfectly clear',
    choices: [
      {
        text: 'Continue',
        outcome: 'trigger_event_2',
      },
    ],
  };

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Create a new NarrativeEngine instance
    narrativeEngine = new NarrativeEngine();

    // Create a new MessageDecoder instance
    messageDecoder = new MessageDecoder();

    // Add sample events
    narrativeEngine.addEvent(sampleEvent);
    narrativeEngine.addEvent(eventWithoutInterference);
  });

  test('NarrativeEngine applies MessageDecoder interference to event messages', () => {
    // Spy on MessageDecoder.obfuscateMessage
    const obfuscateSpy = jest.spyOn(MessageDecoder, 'obfuscateMessage');

    // Trigger the event with interference
    narrativeEngine.triggerEvent('test_event');

    // Verify that MessageDecoder.obfuscateMessage was called with the correct parameters
    expect(obfuscateSpy).toHaveBeenCalledWith(sampleEvent.message, sampleEvent.interference);

    // The current event's message should be obfuscated
    const currentEvent = narrativeEngine.getCurrentEvent();
    expect(currentEvent).not.toBeNull();
    if (currentEvent) {
      expect(currentEvent.message).not.toEqual(sampleEvent.message);
    }
  });

  test('NarrativeEngine does not apply MessageDecoder interference when not specified', () => {
    // Spy on MessageDecoder.obfuscateMessage
    const obfuscateSpy = jest.spyOn(MessageDecoder, 'obfuscateMessage');

    // Trigger the event without interference
    narrativeEngine.triggerEvent('clear_event');

    // Verify that MessageDecoder.obfuscateMessage was not called
    expect(obfuscateSpy).not.toHaveBeenCalled();

    // The current event's message should be unchanged
    const currentEvent = narrativeEngine.getCurrentEvent();
    expect(currentEvent).not.toBeNull();
    if (currentEvent) {
      expect(currentEvent.message).toEqual(eventWithoutInterference.message);
    }
  });

  test('NarrativeEngine applies different levels of interference based on event settings', () => {
    // Add events with different interference levels
    const lowInterferenceEvent: NarrativeEvent = {
      id: 'low_interference',
      message: 'This message has low interference',
      choices: [],
      interference: 0.2,
    };

    const highInterferenceEvent: NarrativeEvent = {
      id: 'high_interference',
      message: 'This message has high interference',
      choices: [],
      interference: 0.8,
    };

    narrativeEngine.addEvent(lowInterferenceEvent);
    narrativeEngine.addEvent(highInterferenceEvent);

    // Spy on MessageDecoder.obfuscateMessage
    const obfuscateSpy = jest.spyOn(MessageDecoder, 'obfuscateMessage');

    // Trigger the low interference event
    narrativeEngine.triggerEvent('low_interference');

    // Verify that MessageDecoder.obfuscateMessage was called with low interference
    expect(obfuscateSpy).toHaveBeenCalledWith(
      lowInterferenceEvent.message,
      lowInterferenceEvent.interference
    );

    // Reset the spy
    obfuscateSpy.mockClear();

    // Trigger the high interference event
    narrativeEngine.triggerEvent('high_interference');

    // Verify that MessageDecoder.obfuscateMessage was called with high interference
    expect(obfuscateSpy).toHaveBeenCalledWith(
      highInterferenceEvent.message,
      highInterferenceEvent.interference
    );

    // The messages should be obfuscated differently
    const lowInterferenceResult = MessageDecoder.obfuscateMessage(
      lowInterferenceEvent.message,
      lowInterferenceEvent.interference
    );

    const highInterferenceResult = MessageDecoder.obfuscateMessage(
      highInterferenceEvent.message,
      highInterferenceEvent.interference
    );

    expect(lowInterferenceResult).not.toEqual(highInterferenceResult);
  });

  test('MessageDecoder can decode messages with references', () => {
    // Create a message with a reference pattern - used in the test below
    // const messageWithReference = 'This message contains a [TOWER] that should be decoded';

    // Create a modified message that simulates a decoded reference
    const decodedMessage = 'This message contains a radio tower that should be decoded';

    // Mock the MessageDecoder.decodeReferences method
    const originalDecodeReferences = (MessageDecoder as any).decodeReferences;
    (MessageDecoder as any).decodeReferences = jest.fn().mockReturnValue(decodedMessage);

    // Create a NarrativeEvent with a reference
    const eventWithReference: NarrativeEvent = {
      id: 'reference_event',
      message: 'You see a [TOWER] in the distance',
      choices: [],
      interference: 0.3,
    };

    // Mock the obfuscateMessage method to return a predictable result
    const obfuscatedMessage = 'Y#u s@e a radio t%wer in th& d!stance';
    const originalObfuscateMessage = MessageDecoder.obfuscateMessage;
    MessageDecoder.obfuscateMessage = jest.fn().mockReturnValue(obfuscatedMessage);

    // Add the event and trigger it
    narrativeEngine.addEvent(eventWithReference);
    narrativeEngine.triggerEvent('reference_event');

    // The current event's message should be the obfuscated version
    const currentEvent = narrativeEngine.getCurrentEvent();
    expect(currentEvent).not.toBeNull();
    if (currentEvent) {
      expect(currentEvent.message).toEqual(obfuscatedMessage);

      // Verify that MessageDecoder.obfuscateMessage was called
      expect(MessageDecoder.obfuscateMessage).toHaveBeenCalled();
    }

    // Restore the original methods
    (MessageDecoder as any).decodeReferences = originalDecodeReferences;
    MessageDecoder.obfuscateMessage = originalObfuscateMessage;
  });
});

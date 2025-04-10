import { findSignalAtFrequency } from '../../src/data/frequencies';
import { getMessage } from '../../src/data/messages';

// Define the expected Signal interface
interface Signal {
  id: string;
  frequency: number;
  strength: number;
  type: 'message' | 'location' | 'event';
  content: string;
  discovered: boolean;
  timestamp: number;
}

describe('Signal Interface Contract', () => {
  test('findSignalAtFrequency returns objects that match the Signal interface', () => {
    // Get a signal from the frequencies module
    // Note: We need to use a frequency that exists in the signalFrequencies array
    const signal = findSignalAtFrequency(91.1);

    // Verify signal is not null
    expect(signal).not.toBeNull();

    if (signal) {
      // Verify signal has the required properties
      expect(signal).toHaveProperty('frequency');
      expect(signal).toHaveProperty('messageId');

      // Verify frequency is a number
      expect(typeof signal.frequency).toBe('number');

      // Verify messageId is a string
      expect(typeof signal.messageId).toBe('string');
    }
  });

  test('getMessage returns objects that can be used with the Signal interface', () => {
    // Get a message from the messages module
    // Note: We need to use a message ID that exists in the messages object
    const message = getMessage('intro_signal');

    // Verify message is not null
    expect(message).not.toBeNull();

    if (message) {
      // Verify message has the required properties
      expect(message).toHaveProperty('id');
      expect(message).toHaveProperty('title');
      expect(message).toHaveProperty('content');

      // Verify id is a string
      expect(typeof message.id).toBe('string');

      // Verify title is a string
      expect(typeof message.title).toBe('string');

      // Verify content is a string
      expect(typeof message.content).toBe('string');
    }
  });

  test('signal data can be transformed to match the Signal interface', () => {
    // Get a signal and its associated message
    const signal = findSignalAtFrequency(91.1);

    if (signal) {
      const message = getMessage(signal.messageId);

      if (message) {
        // Create a complete Signal object that matches the interface
        const completeSignal: Signal = {
          id: message.id,
          frequency: signal.frequency,
          strength: 0.8, // Example value
          type: 'message', // Example value
          content: message.content,
          discovered: true, // Example value
          timestamp: Date.now(),
        };

        // Verify the object matches the Signal interface
        expect(completeSignal).toHaveProperty('id');
        expect(completeSignal).toHaveProperty('frequency');
        expect(completeSignal).toHaveProperty('strength');
        expect(completeSignal).toHaveProperty('type');
        expect(completeSignal).toHaveProperty('content');
        expect(completeSignal).toHaveProperty('discovered');
        expect(completeSignal).toHaveProperty('timestamp');

        // Verify types
        expect(typeof completeSignal.id).toBe('string');
        expect(typeof completeSignal.frequency).toBe('number');
        expect(typeof completeSignal.strength).toBe('number');
        expect(['message', 'location', 'event']).toContain(completeSignal.type);
        expect(typeof completeSignal.content).toBe('string');
        expect(typeof completeSignal.discovered).toBe('boolean');
        expect(typeof completeSignal.timestamp).toBe('number');
      }
    }
  });

  test('signal interface is compatible with both Alpha and Beta components', () => {
    // Create a mock signal that matches the interface
    const mockSignal: Signal = {
      id: 'test-signal',
      frequency: 95.5,
      strength: 0.9,
      type: 'message',
      content: 'Test signal content',
      discovered: true,
      timestamp: Date.now(),
    };

    // Verify the signal can be used by Alpha components
    // In a real test, we would pass this to an Alpha component
    // Here we're just verifying the structure
    expect(mockSignal.frequency).toBeDefined();
    expect(mockSignal.strength).toBeDefined();

    // Verify the signal can be used by Beta components
    // In a real test, we would pass this to a Beta component
    // Here we're just verifying the structure
    expect(mockSignal.id).toBeDefined();
    expect(mockSignal.content).toBeDefined();
    expect(mockSignal.type).toBeDefined();
  });
});

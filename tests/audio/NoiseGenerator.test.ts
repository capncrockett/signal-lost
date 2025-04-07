import { NoiseType } from '../../src/audio/NoiseType';

// Mock Tone.js
jest.mock('tone', () => {
  return {
    Noise: jest.fn().mockImplementation((type) => ({
      type,
      start: jest.fn().mockReturnThis(),
      connect: jest.fn().mockReturnThis(),
      stop: jest.fn(),
    })),
    Gain: jest.fn().mockImplementation((value) => ({
      gain: { value },
      toDestination: jest.fn().mockReturnThis(),
    })),
  };
});

// Import after mocking
import { createNoise } from '../../src/audio/NoiseGenerator';
import * as Tone from 'tone';
import { MockToneNoise, NoiseResult } from '../types/audio';
import { createMockGainNode } from '../mocks/audioMocks';

describe('NoiseGenerator', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  test('should create pink noise by default', () => {
    const result = createNoise() as unknown as NoiseResult;
    const { noise: noiseGen, gain } = result;

    // Check that Tone.Noise was called with 'pink'
    expect(Tone.Noise).toHaveBeenCalledWith('pink');

    // Check that the noise was started
    expect(noiseGen.start).toHaveBeenCalled();

    // Check that the noise was connected to the gain node
    expect(noiseGen.connect).toHaveBeenCalled();

    // Check that the gain node has the correct volume (0.1125 = 0.225/2)
    expect(gain.gain.value).toBe(0.1125);
  });

  test('should create noise with specified type and volume', () => {
    const result = createNoise(NoiseType.White, 0.5) as unknown as NoiseResult;
    const { gain } = result;

    // Check that Tone.Noise was called with 'white'
    expect(Tone.Noise).toHaveBeenCalledWith('white');

    // Check that the gain node has the correct volume
    expect(gain.gain.value).toBe(0.5);
  });

  test('should handle errors gracefully', () => {
    // Mock Tone.Noise to throw an error
    (Tone.Noise as unknown as jest.Mock).mockImplementationOnce(() => {
      throw new Error('Test error');
    });

    // Mock console.error
    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation();

    // Call createNoise
    const result = createNoise();

    // Check that console.error was called
    expect(consoleErrorSpy).toHaveBeenCalled();

    // Check that the result is null
    expect(result).toBeNull();

    // Restore console.error
    consoleErrorSpy.mockRestore();
  });
});

/**
 * Helper functions for creating audio-related mocks
 */

import {
  MockAudioContext,
  MockGainNode,
  MockStereoPannerNode,
  MockOscillatorNode,
  MockAnalyserNode,
} from '../types/audio';

/**
 * Create a mock GainNode
 * @returns A mock GainNode
 */
export function createMockGainNode(): MockGainNode {
  return {
    connect: jest.fn(),
    disconnect: jest.fn(),
    gain: {
      value: 0,
      setValueAtTime: jest.fn(),
      linearRampToValueAtTime: jest.fn(),
      setTargetAtTime: jest.fn(),
    },
  };
}

/**
 * Create a mock StereoPannerNode
 * @param panValue Initial pan value
 * @returns A mock StereoPannerNode
 */
export function createMockStereoPannerNode(panValue: number = 0): MockStereoPannerNode {
  return {
    connect: jest.fn(),
    disconnect: jest.fn(),
    pan: {
      value: panValue,
      setValueAtTime: jest.fn(),
      linearRampToValueAtTime: jest.fn(),
    },
  };
}

/**
 * Create a mock OscillatorNode
 * @param frequency Initial frequency value
 * @param type Oscillator type
 * @returns A mock OscillatorNode
 */
export function createMockOscillatorNode(
  frequency: number = 440,
  type: string = 'sine'
): MockOscillatorNode {
  return {
    connect: jest.fn(),
    disconnect: jest.fn(),
    start: jest.fn(),
    stop: jest.fn(),
    frequency: {
      value: frequency,
      setValueAtTime: jest.fn(),
    },
    type,
  };
}

/**
 * Create a mock AnalyserNode
 * @param fftSize FFT size
 * @returns A mock AnalyserNode
 */
export function createMockAnalyserNode(fftSize: number = 2048): MockAnalyserNode {
  return {
    connect: jest.fn(),
    disconnect: jest.fn(),
    fftSize,
    frequencyBinCount: fftSize / 2,
    getByteFrequencyData: jest.fn(),
    getByteTimeDomainData: jest.fn(),
  };
}

/**
 * Create a mock AudioContext
 * @returns A mock AudioContext
 */
export function createMockAudioContext(): MockAudioContext {
  return {
    createGain: jest.fn().mockReturnValue(createMockGainNode()),
    createOscillator: jest.fn().mockReturnValue(createMockOscillatorNode()),
    createStereoPanner: jest.fn().mockReturnValue(createMockStereoPannerNode()),
    createAnalyser: jest.fn().mockReturnValue(createMockAnalyserNode()),
    destination: {},
    currentTime: 0,
    close: jest.fn(),
  };
}

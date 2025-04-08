/**
 * Type definitions for audio-related mocks
 */

/**
 * Mock GainNode interface
 */
export interface MockGainNode {
  connect: jest.Mock;
  disconnect: jest.Mock;
  gain: {
    value: number;
    setValueAtTime: jest.Mock;
    linearRampToValueAtTime: jest.Mock;
    setTargetAtTime: jest.Mock;
  };
}

/**
 * Mock AudioContext interface
 */
export interface MockAudioContext {
  createGain: jest.Mock<MockGainNode>;
  createOscillator: jest.Mock;
  createStereoPanner: jest.Mock;
  createAnalyser: jest.Mock;
  destination: any;
  currentTime: number;
  close: jest.Mock;
}

/**
 * Mock StereoPannerNode interface
 */
export interface MockStereoPannerNode {
  connect: jest.Mock;
  disconnect: jest.Mock;
  pan: {
    value: number;
    setValueAtTime?: jest.Mock;
    linearRampToValueAtTime?: jest.Mock;
  };
}

/**
 * Mock OscillatorNode interface
 */
export interface MockOscillatorNode {
  connect: jest.Mock;
  disconnect: jest.Mock;
  start: jest.Mock;
  stop: jest.Mock;
  frequency: {
    value: number;
    setValueAtTime: jest.Mock;
  };
  type: string;
}

/**
 * Mock AnalyserNode interface
 */
export interface MockAnalyserNode {
  connect: jest.Mock;
  disconnect: jest.Mock;
  fftSize: number;
  frequencyBinCount: number;
  getByteFrequencyData: jest.Mock;
  getByteTimeDomainData: jest.Mock;
}

/**
 * Mock Tone.Noise interface
 */
export interface MockToneNoise {
  connect: jest.Mock;
  disconnect: jest.Mock;
  start: jest.Mock;
  stop: jest.Mock;
  volume: {
    value: number;
  };
  type: string;
}

/**
 * Result of createNoise function
 */
export interface NoiseResult {
  noise: MockToneNoise;
  gain: MockGainNode;
}

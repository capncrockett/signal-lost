/**
 * Mock for Tone.js library
 * This avoids ESM import issues in Jest tests
 */

// Create a mock for Tone.js
const ToneMock = {
  // Context
  getContext: jest.fn().mockReturnValue({
    resume: jest.fn().mockResolvedValue(undefined),
    suspend: jest.fn().mockResolvedValue(undefined),
    state: 'running',
  }),
  setContext: jest.fn(),
  start: jest.fn(),

  // Instruments and effects
  Gain: jest.fn().mockImplementation(function () {
    return {
      gain: { value: 0 },
      connect: jest.fn().mockReturnThis(),
      toDestination: jest.fn().mockReturnThis(),
      dispose: jest.fn(),
    };
  }),

  Noise: jest.fn().mockImplementation(function (type) {
    return {
      type: type,
      connect: jest.fn().mockReturnThis(),
      start: jest.fn().mockReturnThis(),
      stop: jest.fn().mockReturnThis(),
      dispose: jest.fn(),
    };
  }),

  Player: jest.fn().mockImplementation(function (url) {
    return {
      url: url,
      connect: jest.fn().mockReturnThis(),
      start: jest.fn().mockReturnThis(),
      stop: jest.fn().mockReturnThis(),
      dispose: jest.fn(),
      loop: false,
      volume: { value: 0 },
    };
  }),

  Filter: jest.fn().mockImplementation(function (frequency, type) {
    return {
      frequency: { value: frequency },
      type: type,
      Q: { value: 1 },
      connect: jest.fn().mockReturnThis(),
      dispose: jest.fn(),
    };
  }),

  // Analyzers
  Analyser: jest.fn().mockImplementation(function (options) {
    const size = options?.size || 1024;
    return {
      size: size,
      getValue: jest.fn().mockReturnValue(new Float32Array(size)),
      dispose: jest.fn(),
    };
  }),

  // Transport
  Transport: {
    start: jest.fn(),
    stop: jest.fn(),
    bpm: { value: 120 },
    schedule: jest.fn(),
    scheduleRepeat: jest.fn(),
    cancel: jest.fn(),
  },

  // Utilities
  dbToGain: jest.fn((db) => Math.pow(10, db / 20)),
  gainToDb: jest.fn((gain) => 20 * Math.log10(gain)),
  now: jest.fn(() => Date.now() / 1000),
};

module.exports = ToneMock;

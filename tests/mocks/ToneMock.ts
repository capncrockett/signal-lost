// Mock implementation for Tone.js
export const Destination = {
  volume: {
    value: 0,
  },
};

export class Noise {
  constructor(options: any) {
    // Store options for testing if needed
    Object.assign(this, options);
  }
  connect() {
    return this;
  }
  start() {}
  stop() {}
  dispose() {}
}

export class Oscillator {
  constructor(options: any) {
    // Store options for testing if needed
    Object.assign(this, options);
  }
  connect() {
    return this;
  }
  start() {}
  stop() {}
  dispose() {}
}

export class Filter {
  constructor(options: any) {
    // Store options for testing if needed
    Object.assign(this, options);
  }
  toDestination() {
    return this;
  }
  dispose() {}
}

export const gainToDb = (gain: number): number => gain * 20;

// Add any other Tone.js functions or classes that you need to mock
export default {
  Destination,
  Noise,
  Oscillator,
  Filter,
  gainToDb,
};

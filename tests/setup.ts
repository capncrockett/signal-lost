// Mock Phaser globally
global.Phaser = {
  Scene: class Scene {},
  GameObjects: {
    Container: class Container {
      constructor() {
        this.x = 0;
        this.y = 0;
      }
      on() { return this; }
      emit() { return this; }
      add() { return this; }
      destroy() {}
    },
    Image: class Image {},
    Graphics: class Graphics {
      constructor() {
        this.x = 0;
      }
      fillStyle() { return this; }
      fillRoundedRect() { return this; }
      fillRect() { return this; }
      fillCircle() { return this; }
      setInteractive() { return this; }
      on() { return this; }
    },
    Text: class Text {
      constructor() {
        this.x = 0;
      }
      setOrigin() { return this; }
      setText() { return this; }
    }
  },
  Input: {
    Keyboard: {
      KeyCodes: {}
    }
  },
  Events: {
    EventEmitter: class EventEmitter {
      on() { return this; }
      emit() { return this; }
    }
  },
  Math: {
    Clamp: (value, min, max) => Math.min(Math.max(value, min), max)
  },
  Geom: {
    Circle: class Circle {
      constructor() {}
      static Contains() { return true; }
    },
    Rectangle: class Rectangle {
      constructor() {}
      static Contains() { return true; }
    }
  }
} as any;

// Mock Audio Context
class MockAudioContext {
  createGain() {
    return {
      connect: jest.fn(),
      gain: { value: 0 }
    };
  }
  createOscillator() {
    return {
      connect: jest.fn(),
      start: jest.fn(),
      stop: jest.fn(),
      frequency: { value: 0 }
    };
  }
  createBiquadFilter() {
    return {
      connect: jest.fn(),
      frequency: { value: 0 },
      Q: { value: 0 },
      type: ''
    };
  }
}

global.AudioContext = MockAudioContext as any;
global.window.AudioContext = MockAudioContext as any;

// Mock Phaser globally with more complete implementation
global.Phaser = {
  Scene: class Scene {
    add = {
      existing: jest.fn().mockReturnThis(),
    };
    input = {
      setDraggable: jest.fn(),
      on: jest.fn(),
    };
    cameras = {
      main: {
        scrollX: 0,
      },
    };
  },
  GameObjects: {
    Container: class Container {
      x = 0;
      y = 0;
      scene: any;

      constructor(scene: any, x: number, y: number) {
        this.scene = scene;
        this.x = x;
        this.y = y;
      }

      on = jest.fn().mockReturnThis();
      emit = jest.fn().mockReturnThis();
      add = jest.fn().mockReturnThis();
      destroy = jest.fn();
    },
    Image: class Image {},
    Graphics: class Graphics {
      x = 0;
      y = 0;
      scene: any;

      constructor(scene: any, _options?: any) {
        this.scene = scene;
      }

      fillStyle = jest.fn().mockReturnThis();
      fillRoundedRect = jest.fn().mockReturnThis();
      fillRect = jest.fn().mockReturnThis();
      fillCircle = jest.fn().mockReturnThis();
      setInteractive = jest.fn().mockReturnThis();
      on = jest.fn().mockReturnThis();
    },
    Text: class Text {
      x = 0;
      y = 0;
      scene: any;
      text: string;

      constructor(scene: any, x: number, y: number, text: string, _style?: any) {
        this.scene = scene;
        this.x = x;
        this.y = y;
        this.text = text;
      }

      setOrigin = jest.fn().mockReturnThis();
      setText = jest.fn().mockReturnThis();
    },
  },
  Input: {
    Keyboard: {
      KeyCodes: {},
    },
  },
  Events: {
    EventEmitter: class EventEmitter {
      on = jest.fn().mockReturnThis();
      emit = jest.fn().mockReturnThis();
    },
  },
  Math: {
    Clamp: (value: number, min: number, max: number) => Math.min(Math.max(value, min), max),
  },
  Geom: {
    Circle: class Circle {
      constructor(_x?: number, _y?: number, _radius?: number) {}
      static Contains = jest.fn().mockReturnValue(true);
    },
    Rectangle: class Rectangle {
      constructor(_x?: number, _y?: number, _width?: number, _height?: number) {}
      static Contains = jest.fn().mockReturnValue(true);
    },
  },
} as any;

// Mock Audio Context
class MockAudioContext {
  destination = {};
  sampleRate = 44100;

  createGain() {
    return {
      connect: jest.fn(),
      gain: { value: 0 },
    };
  }

  createBuffer(_numChannels: number, length: number, _sampleRate: number) {
    return {
      getChannelData: () => new Float32Array(length),
    };
  }

  createBufferSource() {
    return {
      connect: jest.fn(),
      start: jest.fn(),
      stop: jest.fn(),
      disconnect: jest.fn(),
      buffer: null,
      loop: false,
    };
  }

  createBiquadFilter() {
    return {
      connect: jest.fn(),
      frequency: { value: 0 },
      Q: { value: 0 },
      type: '',
    };
  }
}

global.AudioContext = MockAudioContext as any;
global.window = global.window || {};
global.window.AudioContext = MockAudioContext as any;
global.window.webkitAudioContext = MockAudioContext as any;

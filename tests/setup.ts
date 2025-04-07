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
  currentTime = 0;
  state = 'running';

  createGain() {
    return {
      connect: jest.fn(),
      disconnect: jest.fn(),
      gain: {
        value: 0,
        setTargetAtTime: jest.fn(),
        linearRampToValueAtTime: jest.fn(),
        exponentialRampToValueAtTime: jest.fn(),
      },
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
      disconnect: jest.fn(),
      frequency: {
        value: 0,
        setTargetAtTime: jest.fn(),
      },
      Q: { value: 0 },
      type: '',
    };
  }

  createOscillator() {
    return {
      frequency: {
        value: 0,
        setTargetAtTime: jest.fn(),
      },
      type: 'sine',
      connect: jest.fn(),
      disconnect: jest.fn(),
      start: jest.fn(),
      stop: jest.fn(),
    };
  }

  createStereoPanner() {
    return {
      pan: {
        value: 0,
        setTargetAtTime: jest.fn(),
      },
      connect: jest.fn(),
      disconnect: jest.fn(),
    };
  }

  createAnalyser() {
    return {
      fftSize: 2048,
      frequencyBinCount: 1024,
      getByteFrequencyData: jest.fn(),
      getByteTimeDomainData: jest.fn(),
      connect: jest.fn(),
      disconnect: jest.fn(),
    };
  }

  resume() {
    return Promise.resolve();
  }

  suspend() {
    return Promise.resolve();
  }

  close() {
    return Promise.resolve();
  }
}

global.AudioContext = MockAudioContext as any;
global.window = global.window || {};
global.window.AudioContext = MockAudioContext as any;
global.window.webkitAudioContext = MockAudioContext as any;

// Mock document for TestOverlay tests
class MockElement {
  style: Record<string, string> = {};
  parentElement: MockElement | null = null;
  children: MockElement[] = [];
  innerHTML: string = '';

  setAttribute(name: string, value: string): void {
    (this as any)[name] = value;
  }

  getAttribute(name: string): string | null {
    return (this as any)[name] || null;
  }

  appendChild(child: MockElement): MockElement {
    this.children.push(child);
    child.parentElement = this;
    return child;
  }

  removeChild(child: MockElement): MockElement {
    const index = this.children.indexOf(child);
    if (index !== -1) {
      this.children.splice(index, 1);
      child.parentElement = null;
    }
    return child;
  }

  addEventListener(_type: string, _listener: EventListener): void {}
  dispatchEvent(_event: Event): boolean { return true; }
}

class MockDocument extends MockElement {
  createElement(tagName: string): MockElement {
    return new MockElement();
  }

  querySelectorAll(_selector: string): MockElement[] {
    return [];
  }
}

global.document = new MockDocument() as any;

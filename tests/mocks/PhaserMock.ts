/**
 * Mock for Phaser to use in tests
 * This avoids canvas-related errors in Node.js environment
 */

// Create a mock for Phaser
const PhaserMock = {
  Game: class Game {
    constructor(config: any) {}
  },
  Scene: class Scene {
    constructor(config: any) {}
    add = {
      existing: jest.fn(),
      text: jest.fn().mockReturnValue({
        setOrigin: jest.fn().mockReturnThis(),
        setInteractive: jest.fn().mockReturnThis(),
        on: jest.fn().mockReturnThis()
      }),
      image: jest.fn().mockReturnValue({
        setDisplaySize: jest.fn().mockReturnThis(),
        setOrigin: jest.fn().mockReturnThis()
      }),
      graphics: jest.fn().mockReturnValue({
        fillStyle: jest.fn().mockReturnThis(),
        fillRect: jest.fn().mockReturnThis(),
        fillRoundedRect: jest.fn().mockReturnThis(),
        fillCircle: jest.fn().mockReturnThis(),
        lineStyle: jest.fn().mockReturnThis(),
        strokeRect: jest.fn().mockReturnThis(),
        strokeRoundedRect: jest.fn().mockReturnThis(),
        strokeCircle: jest.fn().mockReturnThis(),
        setInteractive: jest.fn().mockReturnThis(),
        on: jest.fn().mockReturnThis()
      })
    };
    input = {
      once: jest.fn(),
      on: jest.fn(),
      setDraggable: jest.fn()
    };
    cameras = {
      main: {
        scrollX: 0,
        scrollY: 0
      }
    };
    sound = {
      add: jest.fn().mockReturnValue({
        play: jest.fn(),
        stop: jest.fn(),
        pause: jest.fn(),
        resume: jest.fn(),
        setVolume: jest.fn()
      })
    }
  },
  GameObjects: {
    Container: class Container {
      scene: any;
      x: number;
      y: number;

      constructor(scene: any, x: number, y: number) {
        this.scene = scene;
        this.x = x;
        this.y = y;
      }

      add(child: any) { return this; }
      on(event: string, fn: Function) { return this; }
      emit(event: string, ...args: any[]) { return this; }
      destroy() {}
    },
    GameObject: class GameObject {},
    Graphics: class Graphics {
      scene: any;
      x: number = 0;
      y: number = 0;

      constructor(scene: any) {
        this.scene = scene;
      }

      fillStyle() { return this; }
      fillRect() { return this; }
      fillRoundedRect() { return this; }
      fillCircle() { return this; }
      lineStyle() { return this; }
      strokeRect() { return this; }
      strokeRoundedRect() { return this; }
      strokeCircle() { return this; }
      setInteractive() { return this; }
      on() { return this; }
    },
    Text: class Text {
      scene: any;
      x: number = 0;
      y: number = 0;

      constructor(scene: any, x: number, y: number, text: string, style: any) {
        this.scene = scene;
        this.x = x;
        this.y = y;
      }

      setOrigin() { return this; }
      setInteractive() { return this; }
      on() { return this; }
      setText() { return this; }
    }
  },
  Math: {
    Clamp: (value: number, min: number, max: number) => Math.min(Math.max(value, min), max)
  },
  Geom: {
    Circle: class Circle {
      constructor(x: number, y: number, radius: number) {}
      static Contains() { return true; }
    },
    Rectangle: class Rectangle {
      constructor(x: number, y: number, width: number, height: number) {}
      static Contains() { return true; }
    }
  },
  Input: {
    Keyboard: {
      KeyCodes: {
        UP: 38,
        DOWN: 40,
        LEFT: 37,
        RIGHT: 39,
        SPACE: 32,
        ENTER: 13
      }
    }
  },
  Events: {
    EventEmitter: class EventEmitter {
      on() { return this; }
      once() { return this; }
      off() { return this; }
      emit() { return this; }
      removeAllListeners() { return this; }
    }
  }
};

export default PhaserMock;

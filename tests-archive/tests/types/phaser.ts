/**
 * Type definitions for Phaser-related mocks
 */

/**
 * Mock Phaser.Scene interface
 */
export interface MockScene {
  add: {
    existing: jest.Mock;
    image: jest.Mock;
    text: jest.Mock;
    rectangle: jest.Mock;
    circle: jest.Mock;
    particles: jest.Mock;
  };
  input: {
    keyboard: {
      addKey: jest.Mock;
    };
    on: jest.Mock;
  };
  tweens: {
    add: jest.Mock;
  };
  time: {
    delayedCall: jest.Mock;
    addEvent: jest.Mock;
  };
  events: {
    on: jest.Mock;
    off: jest.Mock;
    emit: jest.Mock;
  };
  scale: {
    width: number;
    height: number;
    on: jest.Mock;
  };
  scene: {
    start: jest.Mock;
    stop: jest.Mock;
    pause: jest.Mock;
    resume: jest.Mock;
    restart: jest.Mock;
    once: jest.Mock;
  };
  children: {
    getByName: jest.Mock;
  };
  cameras: {
    main: {
      setBackgroundColor: jest.Mock;
    };
  };
  sys: {
    game: any;
  };
}

/**
 * Mock Phaser.GameObjects.Container interface
 */
export interface MockContainer {
  x: number;
  y: number;
  width: number;
  height: number;
  visible: boolean;
  add: jest.Mock;
  remove: jest.Mock;
  setPosition: jest.Mock;
  setVisible: jest.Mock;
  setInteractive: jest.Mock;
  on: jest.Mock;
  off: jest.Mock;
  emit: jest.Mock;
  destroy: jest.Mock;
}

/**
 * Mock Phaser.GameObjects.Sprite interface
 */
export interface MockSprite {
  x: number;
  y: number;
  width: number;
  height: number;
  visible: boolean;
  setPosition: jest.Mock;
  setVisible: jest.Mock;
  setInteractive: jest.Mock;
  setOrigin: jest.Mock;
  play: jest.Mock;
  on: jest.Mock;
  off: jest.Mock;
  emit: jest.Mock;
  destroy: jest.Mock;
  anims: {
    currentAnim: {
      key: string;
    };
  };
}

/**
 * Mock Phaser.GameObjects.Text interface
 */
export interface MockText {
  x: number;
  y: number;
  text: string;
  setPosition: jest.Mock;
  setVisible: jest.Mock;
  setInteractive: jest.Mock;
  setOrigin: jest.Mock;
  setStyle: jest.Mock;
  setText: jest.Mock;
  on: jest.Mock;
  off: jest.Mock;
  emit: jest.Mock;
  destroy: jest.Mock;
}

/**
 * Mock Phaser.GameObjects.Rectangle interface
 */
export interface MockRectangle {
  x: number;
  y: number;
  width: number;
  height: number;
  fillColor: number;
  fillAlpha: number;
  setPosition: jest.Mock;
  setVisible: jest.Mock;
  setInteractive: jest.Mock;
  setOrigin: jest.Mock;
  setFillStyle: jest.Mock;
  on: jest.Mock;
  off: jest.Mock;
  emit: jest.Mock;
  destroy: jest.Mock;
}

/**
 * Mock Phaser.Input.Keyboard interface
 */
export interface MockKeyboard {
  addKey: jest.Mock;
  on: jest.Mock;
  off: jest.Mock;
}

/**
 * Mock Phaser.Tweens.TweenManager interface
 */
export interface MockTweenManager {
  add: jest.Mock;
}

/**
 * Mock Phaser.Time.Clock interface
 */
export interface MockClock {
  delayedCall: jest.Mock;
  addEvent: jest.Mock;
}

/**
 * Mock Phaser.Game interface
 */
export interface MockGame {
  scene: {
    scenes: any[];
  };
  renderer: {
    render: jest.Mock;
  };
  scale: {
    width: number;
    height: number;
  };
}

/**
 * Helper functions for creating Phaser-related mocks
 */

import {
  MockScene,
  MockContainer,
  MockSprite,
  MockText,
  MockRectangle,
  MockGame,
} from '../types/phaser';

/**
 * Create a mock Phaser.Scene
 * @returns A mock Phaser.Scene
 */
export function createMockScene(): MockScene {
  return {
    add: {
      existing: jest.fn(),
      image: jest.fn(),
      text: jest.fn(),
      rectangle: jest.fn(),
      circle: jest.fn(),
      particles: jest.fn(),
    },
    input: {
      keyboard: {
        addKey: jest.fn(),
      },
      on: jest.fn(),
    },
    tweens: {
      add: jest.fn(),
    },
    time: {
      delayedCall: jest.fn(),
      addEvent: jest.fn(),
    },
    events: {
      on: jest.fn(),
      off: jest.fn(),
      emit: jest.fn(),
    },
    scale: {
      width: 800,
      height: 600,
      on: jest.fn(),
    },
    scene: {
      start: jest.fn(),
      stop: jest.fn(),
      pause: jest.fn(),
      resume: jest.fn(),
      restart: jest.fn(),
      once: jest.fn(),
    },
    children: {
      getByName: jest.fn(),
    },
    cameras: {
      main: {
        setBackgroundColor: jest.fn(),
      },
    },
    sys: {
      game: {},
    },
  };
}

/**
 * Create a mock Phaser.GameObjects.Container
 * @param x X position
 * @param y Y position
 * @param width Width
 * @param height Height
 * @returns A mock Phaser.GameObjects.Container
 */
export function createMockContainer(
  x: number = 0,
  y: number = 0,
  width: number = 100,
  height: number = 100
): MockContainer {
  return {
    x,
    y,
    width,
    height,
    visible: true,
    add: jest.fn(),
    remove: jest.fn(),
    setPosition: jest.fn(),
    setVisible: jest.fn(),
    setInteractive: jest.fn(),
    on: jest.fn(),
    off: jest.fn(),
    emit: jest.fn(),
    destroy: jest.fn(),
  };
}

/**
 * Create a mock Phaser.GameObjects.Sprite
 * @param x X position
 * @param y Y position
 * @param width Width
 * @param height Height
 * @returns A mock Phaser.GameObjects.Sprite
 */
export function createMockSprite(
  x: number = 0,
  y: number = 0,
  width: number = 100,
  height: number = 100
): MockSprite {
  return {
    x,
    y,
    width,
    height,
    visible: true,
    setPosition: jest.fn(),
    setVisible: jest.fn(),
    setInteractive: jest.fn(),
    setOrigin: jest.fn(),
    play: jest.fn(),
    on: jest.fn(),
    off: jest.fn(),
    emit: jest.fn(),
    destroy: jest.fn(),
    anims: {
      currentAnim: {
        key: 'idle',
      },
    },
  };
}

/**
 * Create a mock Phaser.GameObjects.Text
 * @param x X position
 * @param y Y position
 * @param text Text content
 * @returns A mock Phaser.GameObjects.Text
 */
export function createMockText(x: number = 0, y: number = 0, text: string = ''): MockText {
  return {
    x,
    y,
    text,
    setPosition: jest.fn(),
    setVisible: jest.fn(),
    setInteractive: jest.fn(),
    setOrigin: jest.fn(),
    setStyle: jest.fn(),
    setText: jest.fn(),
    on: jest.fn(),
    off: jest.fn(),
    emit: jest.fn(),
    destroy: jest.fn(),
  };
}

/**
 * Create a mock Phaser.GameObjects.Rectangle
 * @param x X position
 * @param y Y position
 * @param width Width
 * @param height Height
 * @param fillColor Fill color
 * @param fillAlpha Fill alpha
 * @returns A mock Phaser.GameObjects.Rectangle
 */
export function createMockRectangle(
  x: number = 0,
  y: number = 0,
  width: number = 100,
  height: number = 100,
  fillColor: number = 0xffffff,
  fillAlpha: number = 1
): MockRectangle {
  return {
    x,
    y,
    width,
    height,
    fillColor,
    fillAlpha,
    setPosition: jest.fn(),
    setVisible: jest.fn(),
    setInteractive: jest.fn(),
    setOrigin: jest.fn(),
    setFillStyle: jest.fn(),
    on: jest.fn(),
    off: jest.fn(),
    emit: jest.fn(),
    destroy: jest.fn(),
  };
}

/**
 * Create a mock Phaser.Game
 * @returns A mock Phaser.Game
 */
export function createMockGame(): MockGame {
  return {
    scene: {
      scenes: [],
    },
    renderer: {
      render: jest.fn(),
    },
    scale: {
      width: 800,
      height: 600,
    },
  };
}

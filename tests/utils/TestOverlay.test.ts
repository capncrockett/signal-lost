import { TestOverlay } from '../../src/utils/TestOverlay';

// Mock Phaser
jest.mock('phaser', () => {
  return {
    GameObjects: {
      GameObject: class GameObject {
        constructor() {}
        emit() {}
      },
    },
    Geom: {
      Rectangle: class Rectangle {
        constructor(x: number, y: number, width: number, height: number) {
          this.x = x;
          this.y = y;
          this.width = width;
          this.height = height;
        }
        x: number;
        y: number;
        width: number;
        height: number;
      },
    },
  };
});

describe('TestOverlay', () => {
  // Mock DOM elements
  let mockDiv: HTMLDivElement;
  let mockCanvas: HTMLCanvasElement;
  let mockParent: HTMLDivElement;
  let mockGameObject: any;
  let mockScene: any;

  beforeEach(() => {
    // Set up mocks
    mockDiv = document.createElement('div');
    mockCanvas = document.createElement('canvas');
    mockParent = document.createElement('div');
    mockParent.appendChild(mockCanvas);

    // Mock document.createElement
    document.createElement = jest.fn().mockImplementation((tagName) => {
      if (tagName === 'div') {
        return mockDiv;
      }
      return document.createElement(tagName);
    });

    // Mock game object
    mockGameObject = {
      x: 100,
      y: 100,
      width: 200,
      height: 100,
      emit: jest.fn(),
    };

    // Mock scene
    mockScene = {
      sys: {
        game: {
          canvas: mockCanvas,
          config: {
            width: 800,
            height: 600,
          },
        },
      },
      cameras: {
        main: {
          scrollX: 0,
          scrollY: 0,
        },
      },
      events: {
        on: jest.fn(),
        once: jest.fn(),
      },
    };
  });

  afterEach(() => {
    jest.restoreAllMocks();
  });

  test('createOverlay creates a DOM element with the correct attributes', () => {
    TestOverlay.createOverlay(mockScene, mockGameObject, 'test-id');

    // Check that the overlay was created with the correct attributes
    expect(document.createElement).toHaveBeenCalledWith('div');
    expect(mockDiv.getAttribute('data-testid')).toBe('test-id');
    expect(mockDiv.style.position).toBe('absolute');
    expect(mockDiv.style.zIndex).toBe('100');
  });

  test('createOverlay adds the overlay to the parent element', () => {
    const appendChildSpy = jest.spyOn(mockParent, 'appendChild');

    TestOverlay.createOverlay(mockScene, mockGameObject, 'test-id');

    expect(appendChildSpy).toHaveBeenCalledWith(mockDiv);
  });

  test('createOverlay sets up event listeners', () => {
    TestOverlay.createOverlay(mockScene, mockGameObject, 'test-id');

    // Simulate a click event
    const clickEvent = new Event('click');
    Object.defineProperty(clickEvent, 'preventDefault', { value: jest.fn() });
    Object.defineProperty(clickEvent, 'stopPropagation', { value: jest.fn() });

    mockDiv.dispatchEvent(clickEvent);

    // Check that the game object's emit method was called
    expect(mockGameObject.emit).toHaveBeenCalledWith('pointerdown');
  });

  test('createOverlay with custom click handler', () => {
    const clickHandler = jest.fn();
    TestOverlay.createOverlay(mockScene, mockGameObject, 'test-id', clickHandler);

    // Simulate a click event
    const clickEvent = new Event('click');
    Object.defineProperty(clickEvent, 'preventDefault', { value: jest.fn() });
    Object.defineProperty(clickEvent, 'stopPropagation', { value: jest.fn() });

    mockDiv.dispatchEvent(clickEvent);

    // Check that the custom click handler was called
    expect(clickHandler).toHaveBeenCalled();
    // And the game object's emit method was not called
    expect(mockGameObject.emit).not.toHaveBeenCalled();
  });

  test('removeAllOverlays removes all test overlays', () => {
    // Mock document.querySelectorAll
    const mockElements = [
      { parentElement: { removeChild: jest.fn() } },
      { parentElement: { removeChild: jest.fn() } },
    ];
    document.querySelectorAll = jest.fn().mockReturnValue(mockElements);

    TestOverlay.removeAllOverlays();

    // Check that querySelectorAll was called with the correct selector
    expect(document.querySelectorAll).toHaveBeenCalledWith('[data-testid]');

    // Check that removeChild was called for each element
    expect(mockElements[0].parentElement.removeChild).toHaveBeenCalledWith(mockElements[0]);
    expect(mockElements[1].parentElement.removeChild).toHaveBeenCalledWith(mockElements[1]);
  });
});

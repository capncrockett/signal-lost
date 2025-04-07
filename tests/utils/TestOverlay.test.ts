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
    mockDiv = {
      style: {},
      setAttribute: jest.fn().mockImplementation((name, value) => {
        (mockDiv as any)[name] = value;
      }),
      getAttribute: jest.fn().mockImplementation((name) => {
        return (mockDiv as any)[name] || null;
      }),
      addEventListener: jest.fn(),
      dispatchEvent: jest.fn(),
      parentElement: null
    };
    mockCanvas = {
      parentElement: null,
      getBoundingClientRect: jest.fn().mockReturnValue({
        left: 0,
        top: 0,
        width: 800,
        height: 600
      })
    };
    mockParent = {
      appendChild: jest.fn(),
      children: [mockCanvas],
      removeChild: jest.fn()
    };
    mockCanvas.parentElement = mockParent;

    // Mock document.createElement
    global.document.createElement = jest.fn().mockReturnValue(mockDiv);

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
    TestOverlay.createOverlay(mockScene, mockGameObject, 'test-id');

    expect(mockParent.appendChild).toHaveBeenCalled();
  });

  test('createOverlay sets up event listeners', () => {
    TestOverlay.createOverlay(mockScene, mockGameObject, 'test-id');

    // Check that event listeners were added
    expect(mockDiv.addEventListener).toHaveBeenCalledWith('click', expect.any(Function));
    expect(mockDiv.addEventListener).toHaveBeenCalledWith('mousedown', expect.any(Function));
    expect(mockDiv.addEventListener).toHaveBeenCalledWith('touchstart', expect.any(Function));
  });

  test('createOverlay with custom click handler', () => {
    const clickHandler = jest.fn();
    TestOverlay.createOverlay(mockScene, mockGameObject, 'test-id', clickHandler);

    // Get the click handler function that was passed to addEventListener
    const clickHandlerFn = mockDiv.addEventListener.mock.calls.find(
      (call) => call[0] === 'click'
    )[1];

    // Call the handler directly
    clickHandlerFn({ preventDefault: jest.fn(), stopPropagation: jest.fn() });

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
    global.document.querySelectorAll = jest.fn().mockReturnValue(mockElements);

    TestOverlay.removeAllOverlays();

    // Check that querySelectorAll was called with the correct selector
    expect(global.document.querySelectorAll).toHaveBeenCalledWith('[data-testid]');

    // Check that removeChild was called for each element
    expect(mockElements[0].parentElement.removeChild).toHaveBeenCalledWith(mockElements[0]);
    expect(mockElements[1].parentElement.removeChild).toHaveBeenCalledWith(mockElements[1]);
  });
});

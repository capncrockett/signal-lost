// Mock Phaser before importing components
// eslint-disable-next-line @typescript-eslint/no-var-requires
jest.mock('phaser', () => {
  const PhaserMock = require('../mocks/PhaserMock').default;

  // Extend the GameObjects.Container mock to include setDataEnabled
  PhaserMock.GameObjects.Container.prototype.setDataEnabled = jest.fn();
  PhaserMock.GameObjects.Container.prototype.data = {
    set: jest.fn(),
  };

  return PhaserMock;
});

// Import after mocking
import { PerformanceDisplay } from '../../src/components/PerformanceDisplay';
import { PerformanceMonitor, FpsData, MemoryData } from '../../src/utils/PerformanceMonitor';

// Mock PerformanceMonitor
jest.mock('../../src/utils/PerformanceMonitor', () => {
  const mockAddEventListener = jest.fn();
  const mockRemoveEventListener = jest.fn();

  return {
    PerformanceMonitor: {
      getInstance: jest.fn().mockReturnValue({
        addEventListener: mockAddEventListener,
        removeEventListener: mockRemoveEventListener,
      }),
    },
    // Re-export the types
    FpsData: {},
    MemoryData: {},
  };
});

describe('PerformanceDisplay', () => {
  // Mock scene
  const mockScene = {
    add: {
      rectangle: jest.fn().mockReturnValue({
        setOrigin: jest.fn().mockReturnThis(),
      }),
      text: jest.fn().mockReturnValue({
        setOrigin: jest.fn().mockReturnThis(),
        setText: jest.fn(),
      }),
      graphics: jest.fn().mockReturnValue({
        clear: jest.fn().mockReturnThis(),
        fillStyle: jest.fn().mockReturnThis(),
        fillRect: jest.fn().mockReturnThis(),
        lineStyle: jest.fn().mockReturnThis(),
        beginPath: jest.fn().mockReturnThis(),
        moveTo: jest.fn().mockReturnThis(),
        lineTo: jest.fn().mockReturnThis(),
        strokePath: jest.fn().mockReturnThis(),
      }),
      existing: jest.fn(),
    },
    time: {
      addEvent: jest.fn(),
    },
  };

  // Performance display instance
  let performanceDisplay: PerformanceDisplay;

  beforeEach(() => {
    // Clear all mocks
    jest.clearAllMocks();

    // Create performance display
    performanceDisplay = new PerformanceDisplay(mockScene as any, 100, 100);
  });

  test('should create UI elements', () => {
    // Verify UI elements were created
    expect(mockScene.add.rectangle).toHaveBeenCalled();
    expect(mockScene.add.text).toHaveBeenCalledTimes(2); // FPS and memory text
    expect(mockScene.add.graphics).toHaveBeenCalledTimes(2); // FPS and memory graphs
    expect(mockScene.add.existing).toHaveBeenCalled();
  });

  test('should set up event listeners', () => {
    // Verify event listeners were set up
    const performanceMonitor = PerformanceMonitor.getInstance();
    expect(performanceMonitor.addEventListener).toHaveBeenCalledWith('fps', expect.any(Function));
    expect(performanceMonitor.addEventListener).toHaveBeenCalledWith('memory', expect.any(Function));
  });

  test('should set up update timer', () => {
    // Verify update timer was set up
    expect(mockScene.time.addEvent).toHaveBeenCalledWith({
      delay: expect.any(Number),
      callback: expect.any(Function),
      callbackScope: performanceDisplay,
      loop: true,
    });
  });

  test('should handle FPS updates', () => {
    // Get the FPS update handler
    const performanceMonitor = PerformanceMonitor.getInstance();
    const fpsHandler = (performanceMonitor.addEventListener as jest.Mock).mock.calls.find(
      (call) => call[0] === 'fps'
    )[1];

    // Create mock FPS data
    const fpsData: FpsData = {
      current: 60,
      average: 59.5,
      min: 55,
      max: 60,
      history: Array(60).fill(60),
      timestamp: Date.now(),
    };

    // Call the handler
    fpsHandler(fpsData);

    // Verify text was updated
    const mockText = mockScene.add.text.mock.results[0].value;
    expect(mockText.setText).toHaveBeenCalledWith(
      'FPS: 59.5 (Min: 55.0, Max: 60.0)'
    );

    // Verify graph was updated
    const mockGraphics = mockScene.add.graphics.mock.results[0].value;
    expect(mockGraphics.clear).toHaveBeenCalled();
    expect(mockGraphics.fillStyle).toHaveBeenCalled();
    expect(mockGraphics.fillRect).toHaveBeenCalled();
    expect(mockGraphics.lineStyle).toHaveBeenCalled();
    expect(mockGraphics.beginPath).toHaveBeenCalled();
    expect(mockGraphics.strokePath).toHaveBeenCalled();
  });

  test('should handle memory updates', () => {
    // Get the memory update handler
    const performanceMonitor = PerformanceMonitor.getInstance();
    const memoryHandler = (performanceMonitor.addEventListener as jest.Mock).mock.calls.find(
      (call) => call[0] === 'memory'
    )[1];

    // Create mock memory data
    const memoryData: MemoryData = {
      current: {
        totalJSHeapSize: 50 * 1024 * 1024,
        usedJSHeapSize: 25 * 1024 * 1024,
        jsHeapSizeLimit: 100 * 1024 * 1024,
        timestamp: Date.now(),
      },
      average: 20 * 1024 * 1024,
      peak: 30 * 1024 * 1024,
      history: Array(60).fill({
        totalJSHeapSize: 50 * 1024 * 1024,
        usedJSHeapSize: 25 * 1024 * 1024,
        jsHeapSizeLimit: 100 * 1024 * 1024,
        timestamp: Date.now(),
      }),
      timestamp: Date.now(),
    };

    // Call the handler
    memoryHandler(memoryData);

    // Verify text was updated
    const mockText = mockScene.add.text.mock.results[1].value;
    expect(mockText.setText).toHaveBeenCalledWith(
      'Memory: 25.0 MB / 50.0 MB (25.0%)'
    );

    // Verify graph was updated
    const mockGraphics = mockScene.add.graphics.mock.results[1].value;
    expect(mockGraphics.clear).toHaveBeenCalled();
    expect(mockGraphics.fillStyle).toHaveBeenCalled();
    expect(mockGraphics.fillRect).toHaveBeenCalled();
    expect(mockGraphics.lineStyle).toHaveBeenCalled();
    expect(mockGraphics.beginPath).toHaveBeenCalled();
    expect(mockGraphics.strokePath).toHaveBeenCalled();
  });

  test('should show and hide display', () => {
    // Mock setVisible method
    const setVisibleMock = jest.fn();
    performanceDisplay.setVisible = setVisibleMock;

    // Show display
    performanceDisplay.show();
    expect(setVisibleMock).toHaveBeenCalledWith(true);

    // Hide display
    performanceDisplay.hide();
    expect(setVisibleMock).toHaveBeenCalledWith(false);

    // Toggle display (show)
    setVisibleMock.mockClear();
    performanceDisplay.visible = false;
    performanceDisplay.toggle();
    expect(setVisibleMock).toHaveBeenCalledWith(true);

    // Toggle display (hide)
    setVisibleMock.mockClear();
    performanceDisplay.visible = true;
    performanceDisplay.toggle();
    expect(setVisibleMock).toHaveBeenCalledWith(false);
  });

  test('should clean up resources when destroyed', () => {
    // Mock destroy method
    const superDestroyMock = jest.fn();
    Object.defineProperty(Object.getPrototypeOf(PerformanceDisplay.prototype), 'destroy', {
      value: superDestroyMock,
    });

    // Destroy display
    performanceDisplay.destroy();

    // Verify event listeners were removed
    const performanceMonitor = PerformanceMonitor.getInstance();
    expect(performanceMonitor.removeEventListener).toHaveBeenCalledWith('fps', expect.any(Function));
    expect(performanceMonitor.removeEventListener).toHaveBeenCalledWith('memory', expect.any(Function));

    // Verify super.destroy was called
    expect(superDestroyMock).toHaveBeenCalled();
  });

  test('should create with custom configuration', () => {
    // Create with custom config
    const customConfig = {
      width: 300,
      height: 200,
      padding: 20,
      backgroundColor: 0x333333,
      backgroundAlpha: 0.8,
      textColor: '#00ff00',
      fpsGraphColor: 0x0000ff,
      memoryGraphColor: 0xff00ff,
      graphHeight: 60,
      graphWidth: 260,
      showFps: true,
      showMemory: false,
      showGraphs: true,
      updateInterval: 2000,
    };

    // Create new display with custom config
    new PerformanceDisplay(mockScene as any, 200, 200, customConfig);

    // Verify custom config was used
    expect(mockScene.add.rectangle).toHaveBeenCalledWith(
      0,
      0,
      customConfig.width,
      customConfig.height,
      customConfig.backgroundColor,
      customConfig.backgroundAlpha
    );

    // Verify only FPS text was created (showMemory is false)
    expect(mockScene.add.text).toHaveBeenCalledTimes(3); // 2 from previous test + 1 from this test
  });
});

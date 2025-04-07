// Mock performance.now() and performance.memory
const mockNow = jest.fn().mockReturnValue(1000);
const mockMemory = {
  totalJSHeapSize: 50 * 1024 * 1024,
  usedJSHeapSize: 25 * 1024 * 1024,
  jsHeapSizeLimit: 100 * 1024 * 1024,
};

Object.defineProperty(window.performance, 'now', {
  value: mockNow,
  configurable: true,
});

Object.defineProperty(window.performance, 'memory', {
  value: mockMemory,
  configurable: true,
});

import { PerformanceMonitor } from '../../src/utils/PerformanceMonitor';

describe('PerformanceMonitor', () => {
  let performanceMonitor: PerformanceMonitor;

  beforeEach(() => {
    // Reset mocks
    jest.clearAllMocks();

    // Reset singleton
    (PerformanceMonitor as any).instance = undefined;

    // Get instance
    performanceMonitor = PerformanceMonitor.getInstance();
  });

  test('should create a singleton instance', () => {
    // Get another instance
    const anotherInstance = PerformanceMonitor.getInstance();

    // Verify it's the same instance
    expect(anotherInstance).toBe(performanceMonitor);
  });

  test('should update FPS metrics', () => {
    // Set up mock times
    mockNow.mockReturnValueOnce(1000); // First call in constructor
    mockNow.mockReturnValueOnce(2000); // Second call in update

    // Update with 1 second elapsed
    performanceMonitor.update(2000);

    // Verify FPS metrics were updated
    const fpsData = performanceMonitor.getFpsData();
    expect(fpsData.average).toBeGreaterThan(0);
    expect(fpsData.min).toBeGreaterThan(0);
    expect(fpsData.max).toBeGreaterThan(0);
    expect(fpsData.history.length).toBe(1);
  });

  test('should update memory metrics', () => {
    // Set up mock times
    mockNow.mockReturnValueOnce(1000); // First call in constructor
    mockNow.mockReturnValueOnce(6000); // Second call in update (5 seconds elapsed)

    // Update with 5 seconds elapsed
    performanceMonitor.update(6000);

    // Verify memory metrics were updated
    const memoryData = performanceMonitor.getMemoryData();
    expect(memoryData).not.toBeNull();
    if (memoryData) {
      expect(memoryData.current.usedJSHeapSize).toBe(mockMemory.usedJSHeapSize);
      expect(memoryData.average).toBe(mockMemory.usedJSHeapSize);
      expect(memoryData.peak).toBe(mockMemory.usedJSHeapSize);
      expect(memoryData.history.length).toBe(1);
    }
  });

  test('should track load times', () => {
    // Mock the notifyListeners method
    const notifyListenersSpy = jest.spyOn(performanceMonitor as any, 'notifyListeners');

    // Start and end load tracking
    performanceMonitor.startLoadTracking();
    performanceMonitor.endLoadTracking('test-scene');

    // Verify loadTime event was emitted with the correct scene name
    expect(notifyListenersSpy).toHaveBeenCalledWith(
      'loadTime',
      expect.objectContaining({
        sceneName: 'test-scene',
      })
    );

    // Restore spy
    notifyListenersSpy.mockRestore();
  });

  test('should track asset load times', () => {
    // Mock the notifyListeners method
    const notifyListenersSpy = jest.spyOn(performanceMonitor as any, 'notifyListeners');

    // Track asset load time
    performanceMonitor.trackAssetLoad('test-asset', 'image');

    // Verify assetLoad event was emitted with the correct asset info
    expect(notifyListenersSpy).toHaveBeenCalledWith(
      'assetLoad',
      expect.objectContaining({
        key: 'test-asset',
        type: 'image',
      })
    );

    // Restore spy
    notifyListenersSpy.mockRestore();
  });

  test('should track scene transitions', () => {
    // Mock the notifyListeners method
    const notifyListenersSpy = jest.spyOn(performanceMonitor as any, 'notifyListeners');

    // Track scene transition
    performanceMonitor.trackSceneTransition('scene-a', 'scene-b');

    // Verify sceneTransition event was emitted with the correct scene info
    expect(notifyListenersSpy).toHaveBeenCalledWith(
      'sceneTransition',
      expect.objectContaining({
        fromScene: 'scene-a',
        toScene: 'scene-b',
      })
    );

    // Restore spy
    notifyListenersSpy.mockRestore();
  });

  test('should run benchmarks', () => {
    // Mock the notifyListeners method
    const notifyListenersSpy = jest.spyOn(performanceMonitor as any, 'notifyListeners');

    // Set up mock times to simulate benchmark iterations
    let time = 1000;
    mockNow.mockImplementation(() => {
      time += 10; // Each call adds 10ms
      return time;
    });

    // Run benchmark
    performanceMonitor.runBenchmark(
      'test-benchmark',
      () => {
        // Do nothing
      },
      3 // 3 iterations
    );

    // Verify benchmark event was emitted
    expect(notifyListenersSpy).toHaveBeenCalledWith(
      'benchmark',
      expect.objectContaining({
        name: 'test-benchmark',
        iterations: 3,
      })
    );

    // Restore spy
    notifyListenersSpy.mockRestore();
  });

  test('should add and remove event listeners', () => {
    // Create mock listener
    const mockListener = jest.fn();

    // Add listener
    performanceMonitor.addEventListener('fps', mockListener);

    // Trigger FPS update
    mockNow.mockReturnValueOnce(2000); // Second call in update
    performanceMonitor.update(2000);

    // Verify listener was called
    expect(mockListener).toHaveBeenCalled();

    // Remove listener
    performanceMonitor.removeEventListener('fps', mockListener);

    // Reset mock
    mockListener.mockClear();

    // Trigger another FPS update
    mockNow.mockReturnValueOnce(3000); // Third call in update
    performanceMonitor.update(3000);

    // Verify listener was not called
    expect(mockListener).not.toHaveBeenCalled();
  });

  test('should reset metrics', () => {
    // Update metrics
    mockNow.mockReturnValueOnce(2000); // Second call in update
    performanceMonitor.update(2000);

    // Get metrics before reset
    const fpsDataBefore = performanceMonitor.getFpsData();
    expect(fpsDataBefore.history.length).toBeGreaterThan(0);

    // Reset metrics
    performanceMonitor.reset();

    // Verify metrics were reset
    const fpsDataAfter = performanceMonitor.getFpsData();
    expect(fpsDataAfter.average).toBe(0);
    expect(fpsDataAfter.history.length).toBe(0);
  });

  test('should handle configuration', () => {
    // Create a new instance with default config
    (PerformanceMonitor as any).instance = undefined;
    const defaultMonitor = PerformanceMonitor.getInstance();

    // Verify default config has expected values
    expect((defaultMonitor as any).config.fpsUpdateInterval).toBe(1000);
    expect((defaultMonitor as any).config.memoryUpdateInterval).toBe(5000);
    expect((defaultMonitor as any).config.fpsHistorySize).toBe(60);
    expect((defaultMonitor as any).config.memoryHistorySize).toBe(60);
    expect((defaultMonitor as any).config.logToConsole).toBe(false);
  });
});

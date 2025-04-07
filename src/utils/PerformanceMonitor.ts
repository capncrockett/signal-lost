/**
 * Performance monitoring utility for tracking game performance metrics
 */
export class PerformanceMonitor {
  // Singleton instance
  private static instance: PerformanceMonitor;

  // FPS tracking
  private fpsValues: number[] = [];
  private fpsUpdateTime = 0;
  private frameCount = 0;
  private lastFrameTime = 0;
  private averageFps = 0;
  private minFps = Infinity;
  private maxFps = 0;

  // Memory tracking
  private memoryValues: MemoryUsage[] = [];
  private memoryUpdateTime = 0;
  private averageMemoryUsage = 0;
  private peakMemoryUsage = 0;

  // Load time tracking
  private loadStartTime = 0;
  private loadEndTime = 0;
  private assetLoadTimes: AssetLoadTime[] = [];

  // Scene transition tracking
  private sceneTransitions: SceneTransition[] = [];

  // Event listeners
  private listeners: Record<PerformanceEventType, Array<(data: any) => void>> = {
    fps: [],
    memory: [],
    loadTime: [],
    assetLoad: [],
    sceneTransition: [],
    benchmark: [],
  };

  // Configuration
  private config: PerformanceMonitorConfig = {
    fpsUpdateInterval: 1000, // Update FPS every second
    memoryUpdateInterval: 5000, // Update memory every 5 seconds
    fpsHistorySize: 60, // Keep 60 FPS samples (1 minute at 1 sample per second)
    memoryHistorySize: 60, // Keep 60 memory samples (5 minutes at 1 sample per 5 seconds)
    logToConsole: false, // Don't log to console by default
    trackMemory: true, // Track memory by default
  };

  /**
   * Private constructor to enforce singleton pattern
   */
  private constructor() {
    // Initialize
    this.lastFrameTime = performance.now();
  }

  /**
   * Get the singleton instance
   */
  public static getInstance(): PerformanceMonitor {
    if (!PerformanceMonitor.instance) {
      PerformanceMonitor.instance = new PerformanceMonitor();
    }
    return PerformanceMonitor.instance;
  }

  /**
   * Configure the performance monitor
   * @param config Configuration options
   */
  public configure(config: Partial<PerformanceMonitorConfig>): void {
    this.config = { ...this.config, ...config };
  }

  /**
   * Start tracking load time
   */
  public startLoadTracking(): void {
    this.loadStartTime = performance.now();
    this.assetLoadTimes = [];
  }

  /**
   * End tracking load time
   * @param sceneName Name of the scene that was loaded
   */
  public endLoadTracking(sceneName: string): void {
    this.loadEndTime = performance.now();
    const loadTime = this.loadEndTime - this.loadStartTime;

    // Calculate total asset load time
    const totalAssetLoadTime = this.assetLoadTimes.reduce(
      (total, asset) => total + asset.loadTime,
      0
    );

    // Calculate other time (scene initialization, etc.)
    const otherTime = loadTime - totalAssetLoadTime;

    const loadTimeData: LoadTimeData = {
      sceneName,
      totalLoadTime: loadTime,
      assetLoadTime: totalAssetLoadTime,
      otherTime,
      timestamp: Date.now(),
    };

    // Notify listeners
    this.notifyListeners('loadTime', loadTimeData);

    // Log to console if enabled
    if (this.config.logToConsole) {
      console.log(
        `Scene "${sceneName}" loaded in ${loadTime.toFixed(2)}ms ` +
        `(Assets: ${totalAssetLoadTime.toFixed(2)}ms, Other: ${otherTime.toFixed(2)}ms)`
      );
    }
  }

  /**
   * Track asset load time
   * @param assetKey Key of the asset
   * @param assetType Type of the asset
   * @param loadTime Time taken to load the asset
   */
  public trackAssetLoad(assetKey: string, assetType: string, loadTime: number): void {
    const assetLoadTime: AssetLoadTime = {
      key: assetKey,
      type: assetType,
      loadTime,
      timestamp: Date.now(),
    };

    this.assetLoadTimes.push(assetLoadTime);

    // Notify listeners
    this.notifyListeners('assetLoad', assetLoadTime);

    // Log to console if enabled
    if (this.config.logToConsole) {
      console.log(`Asset "${assetKey}" (${assetType}) loaded in ${loadTime.toFixed(2)}ms`);
    }
  }

  /**
   * Track scene transition
   * @param fromScene Name of the scene transitioning from
   * @param toScene Name of the scene transitioning to
   * @param transitionTime Time taken for the transition
   */
  public trackSceneTransition(fromScene: string, toScene: string, transitionTime: number): void {
    const sceneTransition: SceneTransition = {
      fromScene,
      toScene,
      transitionTime,
      timestamp: Date.now(),
    };

    this.sceneTransitions.push(sceneTransition);

    // Notify listeners
    this.notifyListeners('sceneTransition', sceneTransition);

    // Log to console if enabled
    if (this.config.logToConsole) {
      console.log(
        `Scene transition from "${fromScene}" to "${toScene}" took ${transitionTime.toFixed(2)}ms`
      );
    }
  }

  /**
   * Update performance metrics
   * Should be called every frame
   * @param time Current time
   */
  public update(time: number): void {
    // Update frame count
    this.frameCount++;

    // Calculate FPS
    const elapsed = time - this.lastFrameTime;
    this.lastFrameTime = time;
    const currentFps = 1000 / elapsed;

    // Update FPS stats
    if (time - this.fpsUpdateTime >= this.config.fpsUpdateInterval) {
      // Calculate average FPS
      this.averageFps = this.frameCount * 1000 / (time - this.fpsUpdateTime);
      this.frameCount = 0;
      this.fpsUpdateTime = time;

      // Update min/max FPS
      this.minFps = Math.min(this.minFps, this.averageFps);
      this.maxFps = Math.max(this.maxFps, this.averageFps);

      // Add to history
      this.fpsValues.push(this.averageFps);
      if (this.fpsValues.length > this.config.fpsHistorySize) {
        this.fpsValues.shift();
      }

      // Notify listeners
      const fpsData: FpsData = {
        current: currentFps,
        average: this.averageFps,
        min: this.minFps,
        max: this.maxFps,
        history: [...this.fpsValues],
        timestamp: Date.now(),
      };
      this.notifyListeners('fps', fpsData);

      // Log to console if enabled
      if (this.config.logToConsole) {
        console.log(
          `FPS: ${this.averageFps.toFixed(2)} ` +
          `(Min: ${this.minFps.toFixed(2)}, Max: ${this.maxFps.toFixed(2)})`
        );
      }
    }

    // Update memory usage
    if (
      this.config.trackMemory &&
      time - this.memoryUpdateTime >= this.config.memoryUpdateInterval &&
      window.performance &&
      (performance as any).memory
    ) {
      this.memoryUpdateTime = time;

      // Get memory usage
      const memoryInfo = (performance as any).memory;
      const memoryUsage: MemoryUsage = {
        totalJSHeapSize: memoryInfo.totalJSHeapSize,
        usedJSHeapSize: memoryInfo.usedJSHeapSize,
        jsHeapSizeLimit: memoryInfo.jsHeapSizeLimit,
        timestamp: Date.now(),
      };

      // Add to history
      this.memoryValues.push(memoryUsage);
      if (this.memoryValues.length > this.config.memoryHistorySize) {
        this.memoryValues.shift();
      }

      // Update stats
      this.averageMemoryUsage =
        this.memoryValues.reduce((sum, m) => sum + m.usedJSHeapSize, 0) / this.memoryValues.length;
      this.peakMemoryUsage = Math.max(this.peakMemoryUsage, memoryUsage.usedJSHeapSize);

      // Notify listeners
      const memoryData: MemoryData = {
        current: memoryUsage,
        average: this.averageMemoryUsage,
        peak: this.peakMemoryUsage,
        history: [...this.memoryValues],
        timestamp: Date.now(),
      };
      this.notifyListeners('memory', memoryData);

      // Log to console if enabled
      if (this.config.logToConsole) {
        console.log(
          `Memory: ${(memoryUsage.usedJSHeapSize / 1024 / 1024).toFixed(2)}MB ` +
          `(Peak: ${(this.peakMemoryUsage / 1024 / 1024).toFixed(2)}MB)`
        );
      }
    }
  }

  /**
   * Run a performance benchmark
   * @param name Name of the benchmark
   * @param fn Function to benchmark
   * @param iterations Number of iterations to run
   * @returns Benchmark results
   */
  public runBenchmark(
    name: string,
    fn: () => void,
    iterations: number = 100
  ): BenchmarkResult {
    // Warm up
    fn();

    // Run benchmark
    const times: number[] = [];
    for (let i = 0; i < iterations; i++) {
      const start = performance.now();
      fn();
      const end = performance.now();
      times.push(end - start);
    }

    // Calculate stats
    const total = times.reduce((sum, time) => sum + time, 0);
    const average = total / times.length;
    const min = Math.min(...times);
    const max = Math.max(...times);
    const median = times.sort((a, b) => a - b)[Math.floor(times.length / 2)];

    // Calculate standard deviation
    const variance =
      times.reduce((sum, time) => sum + Math.pow(time - average, 2), 0) / times.length;
    const stdDev = Math.sqrt(variance);

    // Create result
    const result: BenchmarkResult = {
      name,
      iterations,
      average,
      median,
      min,
      max,
      stdDev,
      total,
      timestamp: Date.now(),
    };

    // Notify listeners
    this.notifyListeners('benchmark', result);

    // Log to console if enabled
    if (this.config.logToConsole) {
      console.log(
        `Benchmark "${name}" (${iterations} iterations): ` +
        `Avg: ${average.toFixed(2)}ms, ` +
        `Median: ${median.toFixed(2)}ms, ` +
        `Min: ${min.toFixed(2)}ms, ` +
        `Max: ${max.toFixed(2)}ms, ` +
        `StdDev: ${stdDev.toFixed(2)}ms`
      );
    }

    return result;
  }

  /**
   * Add an event listener
   * @param event Event type
   * @param callback Callback function
   */
  public addEventListener(
    event: PerformanceEventType,
    callback: (data: any) => void
  ): void {
    this.listeners[event].push(callback);
  }

  /**
   * Remove an event listener
   * @param event Event type
   * @param callback Callback function
   */
  public removeEventListener(
    event: PerformanceEventType,
    callback: (data: any) => void
  ): void {
    const index = this.listeners[event].indexOf(callback);
    if (index !== -1) {
      this.listeners[event].splice(index, 1);
    }
  }

  /**
   * Notify listeners of an event
   * @param event Event type
   * @param data Event data
   */
  private notifyListeners(event: PerformanceEventType, data: any): void {
    for (const listener of this.listeners[event]) {
      listener(data);
    }
  }

  /**
   * Get FPS data
   */
  public getFpsData(): FpsData {
    return {
      current: this.fpsValues.length > 0 ? this.fpsValues[this.fpsValues.length - 1] : 0,
      average: this.averageFps,
      min: this.minFps,
      max: this.maxFps,
      history: [...this.fpsValues],
      timestamp: Date.now(),
    };
  }

  /**
   * Get memory data
   */
  public getMemoryData(): MemoryData | null {
    if (!this.config.trackMemory || this.memoryValues.length === 0) {
      return null;
    }

    return {
      current: this.memoryValues[this.memoryValues.length - 1],
      average: this.averageMemoryUsage,
      peak: this.peakMemoryUsage,
      history: [...this.memoryValues],
      timestamp: Date.now(),
    };
  }

  /**
   * Get load time data
   */
  public getLoadTimeData(): LoadTimeData[] {
    // This would require storing multiple load times
    // For now, we just return an empty array
    return [];
  }

  /**
   * Get asset load times
   */
  public getAssetLoadTimes(): AssetLoadTime[] {
    return [...this.assetLoadTimes];
  }

  /**
   * Get scene transitions
   */
  public getSceneTransitions(): SceneTransition[] {
    return [...this.sceneTransitions];
  }

  /**
   * Reset all metrics
   */
  public reset(): void {
    this.fpsValues = [];
    this.fpsUpdateTime = 0;
    this.frameCount = 0;
    this.lastFrameTime = performance.now();
    this.averageFps = 0;
    this.minFps = Infinity;
    this.maxFps = 0;

    this.memoryValues = [];
    this.memoryUpdateTime = 0;
    this.averageMemoryUsage = 0;
    this.peakMemoryUsage = 0;

    this.loadStartTime = 0;
    this.loadEndTime = 0;
    this.assetLoadTimes = [];

    this.sceneTransitions = [];
  }
}

/**
 * Performance monitor configuration
 */
export interface PerformanceMonitorConfig {
  fpsUpdateInterval: number;
  memoryUpdateInterval: number;
  fpsHistorySize: number;
  memoryHistorySize: number;
  logToConsole: boolean;
  trackMemory: boolean;
}

/**
 * FPS data
 */
export interface FpsData {
  current: number;
  average: number;
  min: number;
  max: number;
  history: number[];
  timestamp: number;
}

/**
 * Memory usage data
 */
export interface MemoryUsage {
  totalJSHeapSize: number;
  usedJSHeapSize: number;
  jsHeapSizeLimit: number;
  timestamp: number;
}

/**
 * Memory data
 */
export interface MemoryData {
  current: MemoryUsage;
  average: number;
  peak: number;
  history: MemoryUsage[];
  timestamp: number;
}

/**
 * Load time data
 */
export interface LoadTimeData {
  sceneName: string;
  totalLoadTime: number;
  assetLoadTime: number;
  otherTime: number;
  timestamp: number;
}

/**
 * Asset load time
 */
export interface AssetLoadTime {
  key: string;
  type: string;
  loadTime: number;
  timestamp: number;
}

/**
 * Scene transition
 */
export interface SceneTransition {
  fromScene: string;
  toScene: string;
  transitionTime: number;
  timestamp: number;
}

/**
 * Benchmark result
 */
export interface BenchmarkResult {
  name: string;
  iterations: number;
  average: number;
  median: number;
  min: number;
  max: number;
  stdDev: number;
  total: number;
  timestamp: number;
}

/**
 * Performance event types
 */
export type PerformanceEventType =
  | 'fps'
  | 'memory'
  | 'loadTime'
  | 'assetLoad'
  | 'sceneTransition'
  | 'benchmark';

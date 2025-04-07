import { PerformanceBenchmark, BenchmarkResult } from '../../src/utils/PerformanceBenchmark';
import { PerformanceMonitor } from '../../src/utils/PerformanceMonitor';

/**
 * Performance test suite for measuring game performance
 */
export class PerformanceTests {
  // Performance benchmark
  private benchmark: PerformanceBenchmark;

  // Performance monitor
  private performanceMonitor: PerformanceMonitor;

  // Test results
  private results: BenchmarkResult[] = [];

  // Test configuration
  private config: PerformanceTestConfig = {
    iterations: 100,
    warmupIterations: 5,
    logToConsole: true,
    thresholds: {
      renderTime: 16, // 60 FPS = 16.67ms per frame
      updateTime: 8, // Half of render time
      assetLoadTime: 500, // 500ms per asset
      sceneLoadTime: 2000, // 2 seconds per scene
    },
  };

  /**
   * Create a new performance test suite
   * @param config Configuration options
   */
  constructor(config?: Partial<PerformanceTestConfig>) {
    // Create benchmark
    this.benchmark = new PerformanceBenchmark({
      iterations: config?.iterations || this.config.iterations,
      warmupIterations: config?.warmupIterations || this.config.warmupIterations,
      logToConsole: config?.logToConsole || this.config.logToConsole,
    });

    // Get performance monitor
    this.performanceMonitor = PerformanceMonitor.getInstance();

    // Merge configuration
    this.config = { ...this.config, ...config };
  }

  /**
   * Run all performance tests
   * @param game Phaser game instance
   * @returns Test results
   */
  public async runAll(game: Phaser.Game): Promise<PerformanceTestResult> {
    // Reset results
    this.results = [];

    // Run render tests
    const renderResults = await this.runRenderTests(game);
    this.results.push(...renderResults);

    // Run update tests
    const updateResults = await this.runUpdateTests(game);
    this.results.push(...updateResults);

    // Run asset load tests
    const assetLoadResults = await this.runAssetLoadTests(game);
    this.results.push(...assetLoadResults);

    // Run scene load tests
    const sceneLoadResults = await this.runSceneLoadTests(game);
    this.results.push(...sceneLoadResults);

    // Generate report
    const report = this.generateReport();

    return report;
  }

  /**
   * Run render tests
   * @param game Phaser game instance
   * @returns Test results
   */
  public async runRenderTests(game: Phaser.Game): Promise<BenchmarkResult[]> {
    const results: BenchmarkResult[] = [];

    // Test basic rendering
    const basicRenderResult = this.benchmark.run(
      'Basic Render',
      () => {
        // Simulate a render cycle
        game.renderer.render(game.scene.scenes[0], 0);
      }
    );
    results.push(basicRenderResult);

    // Test complex rendering
    const complexRenderResult = this.benchmark.run(
      'Complex Render',
      () => {
        // Simulate a complex render cycle
        for (const scene of game.scene.scenes) {
          game.renderer.render(scene, 0);
        }
      }
    );
    results.push(complexRenderResult);

    return results;
  }

  /**
   * Run update tests
   * @param game Phaser game instance
   * @returns Test results
   */
  public async runUpdateTests(game: Phaser.Game): Promise<BenchmarkResult[]> {
    const results: BenchmarkResult[] = [];

    // Test basic update
    const basicUpdateResult = this.benchmark.run(
      'Basic Update',
      () => {
        // Simulate an update cycle
        game.scene.scenes[0].update(0, 16);
      }
    );
    results.push(basicUpdateResult);

    // Test complex update
    const complexUpdateResult = this.benchmark.run(
      'Complex Update',
      () => {
        // Simulate a complex update cycle
        for (const scene of game.scene.scenes) {
          scene.update(0, 16);
        }
      }
    );
    results.push(complexUpdateResult);

    return results;
  }

  /**
   * Run asset load tests
   * @param game Phaser game instance
   * @returns Test results
   */
  public async runAssetLoadTests(game: Phaser.Game): Promise<BenchmarkResult[]> {
    const results: BenchmarkResult[] = [];

    // Test image loading
    const imageLoadResult = await this.runAsyncBenchmark(
      'Image Load',
      async () => {
        return new Promise<void>((resolve) => {
          const scene = game.scene.scenes[0];
          scene.load.image('test-image', 'assets/images/test.png');
          scene.load.once('complete', () => {
            resolve();
          });
          scene.load.start();
        });
      },
      10 // Fewer iterations for async tests
    );
    results.push(imageLoadResult);

    // Test audio loading
    const audioLoadResult = await this.runAsyncBenchmark(
      'Audio Load',
      async () => {
        return new Promise<void>((resolve) => {
          const scene = game.scene.scenes[0];
          scene.load.audio('test-audio', 'assets/audio/test.mp3');
          scene.load.once('complete', () => {
            resolve();
          });
          scene.load.start();
        });
      },
      10 // Fewer iterations for async tests
    );
    results.push(audioLoadResult);

    return results;
  }

  /**
   * Run scene load tests
   * @param game Phaser game instance
   * @returns Test results
   */
  public async runSceneLoadTests(game: Phaser.Game): Promise<BenchmarkResult[]> {
    const results: BenchmarkResult[] = [];

    // Test scene loading
    const sceneLoadResult = await this.runAsyncBenchmark(
      'Scene Load',
      async () => {
        return new Promise<void>((resolve) => {
          const currentScene = game.scene.scenes[0].scene.key;
          const nextScene = currentScene === 'MainScene' ? 'FieldScene' : 'MainScene';
          
          game.scene.scenes[0].scene.start(nextScene);
          game.scene.scenes[0].scene.once('start', () => {
            resolve();
          });
        });
      },
      5 // Fewer iterations for scene loading
    );
    results.push(sceneLoadResult);

    return results;
  }

  /**
   * Run an async benchmark
   * @param name Benchmark name
   * @param fn Async function to benchmark
   * @param iterations Number of iterations
   * @returns Benchmark result
   */
  private async runAsyncBenchmark(
    name: string,
    fn: () => Promise<void>,
    iterations: number = 10
  ): Promise<BenchmarkResult> {
    // Warm up
    await fn();

    // Run benchmark
    const times: number[] = [];
    for (let i = 0; i < iterations; i++) {
      const start = performance.now();
      await fn();
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
   * Generate a performance test report
   * @returns Test report
   */
  public generateReport(): PerformanceTestResult {
    // Calculate overall results
    const renderResults = this.results.filter(r => r.name.includes('Render'));
    const updateResults = this.results.filter(r => r.name.includes('Update'));
    const assetLoadResults = this.results.filter(r => r.name.includes('Load') && !r.name.includes('Scene'));
    const sceneLoadResults = this.results.filter(r => r.name.includes('Scene'));

    // Calculate averages
    const avgRenderTime = renderResults.reduce((sum, r) => sum + r.average, 0) / renderResults.length;
    const avgUpdateTime = updateResults.reduce((sum, r) => sum + r.average, 0) / updateResults.length;
    const avgAssetLoadTime = assetLoadResults.reduce((sum, r) => sum + r.average, 0) / assetLoadResults.length;
    const avgSceneLoadTime = sceneLoadResults.reduce((sum, r) => sum + r.average, 0) / sceneLoadResults.length;

    // Check thresholds
    const renderThreshold = this.config.thresholds.renderTime;
    const updateThreshold = this.config.thresholds.updateTime;
    const assetLoadThreshold = this.config.thresholds.assetLoadTime;
    const sceneLoadThreshold = this.config.thresholds.sceneLoadTime;

    const renderPass = avgRenderTime <= renderThreshold;
    const updatePass = avgUpdateTime <= updateThreshold;
    const assetLoadPass = avgAssetLoadTime <= assetLoadThreshold;
    const sceneLoadPass = avgSceneLoadTime <= sceneLoadThreshold;

    // Create report
    const report: PerformanceTestResult = {
      timestamp: Date.now(),
      results: [...this.results],
      summary: {
        renderTime: {
          average: avgRenderTime,
          threshold: renderThreshold,
          pass: renderPass,
        },
        updateTime: {
          average: avgUpdateTime,
          threshold: updateThreshold,
          pass: updatePass,
        },
        assetLoadTime: {
          average: avgAssetLoadTime,
          threshold: assetLoadThreshold,
          pass: assetLoadPass,
        },
        sceneLoadTime: {
          average: avgSceneLoadTime,
          threshold: sceneLoadThreshold,
          pass: sceneLoadPass,
        },
        overallPass: renderPass && updatePass && assetLoadPass && sceneLoadPass,
      },
    };

    // Log to console if enabled
    if (this.config.logToConsole) {
      console.log('Performance Test Report:');
      console.log(`Render Time: ${avgRenderTime.toFixed(2)}ms (Threshold: ${renderThreshold}ms) - ${renderPass ? 'PASS' : 'FAIL'}`);
      console.log(`Update Time: ${avgUpdateTime.toFixed(2)}ms (Threshold: ${updateThreshold}ms) - ${updatePass ? 'PASS' : 'FAIL'}`);
      console.log(`Asset Load Time: ${avgAssetLoadTime.toFixed(2)}ms (Threshold: ${assetLoadThreshold}ms) - ${assetLoadPass ? 'PASS' : 'FAIL'}`);
      console.log(`Scene Load Time: ${avgSceneLoadTime.toFixed(2)}ms (Threshold: ${sceneLoadThreshold}ms) - ${sceneLoadPass ? 'PASS' : 'FAIL'}`);
      console.log(`Overall: ${report.summary.overallPass ? 'PASS' : 'FAIL'}`);
    }

    return report;
  }

  /**
   * Get all test results
   */
  public getResults(): BenchmarkResult[] {
    return [...this.results];
  }

  /**
   * Clear all test results
   */
  public clearResults(): void {
    this.results = [];
  }
}

/**
 * Performance test configuration
 */
export interface PerformanceTestConfig {
  iterations: number;
  warmupIterations: number;
  logToConsole: boolean;
  thresholds: {
    renderTime: number;
    updateTime: number;
    assetLoadTime: number;
    sceneLoadTime: number;
  };
}

/**
 * Performance test result
 */
export interface PerformanceTestResult {
  timestamp: number;
  results: BenchmarkResult[];
  summary: {
    renderTime: {
      average: number;
      threshold: number;
      pass: boolean;
    };
    updateTime: {
      average: number;
      threshold: number;
      pass: boolean;
    };
    assetLoadTime: {
      average: number;
      threshold: number;
      pass: boolean;
    };
    sceneLoadTime: {
      average: number;
      threshold: number;
      pass: boolean;
    };
    overallPass: boolean;
  };
}

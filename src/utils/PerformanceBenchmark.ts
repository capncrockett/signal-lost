import { PerformanceMonitor, BenchmarkResult } from './PerformanceMonitor';

/**
 * Performance benchmark utility for measuring game performance
 */
export class PerformanceBenchmark {
  // Performance monitor
  private performanceMonitor: PerformanceMonitor;

  // Benchmark results
  private results: BenchmarkResult[] = [];

  // Benchmark configuration
  private config: PerformanceBenchmarkConfig = {
    iterations: 100,
    warmupIterations: 5,
    logToConsole: true,
  };

  /**
   * Create a new performance benchmark
   * @param config Configuration options
   */
  constructor(config?: Partial<PerformanceBenchmarkConfig>) {
    // Get performance monitor
    this.performanceMonitor = PerformanceMonitor.getInstance();

    // Merge configuration
    this.config = { ...this.config, ...config };
  }

  /**
   * Run a benchmark
   * @param name Name of the benchmark
   * @param fn Function to benchmark
   * @param iterations Number of iterations (overrides config)
   * @returns Benchmark result
   */
  public run(name: string, fn: () => void, iterations?: number): BenchmarkResult {
    const iterCount = iterations || this.config.iterations;

    // Warm up
    for (let i = 0; i < this.config.warmupIterations; i++) {
      fn();
    }

    // Run benchmark
    const result = this.performanceMonitor.runBenchmark(name, fn, iterCount);

    // Store result
    this.results.push(result);

    // Log to console if enabled
    if (this.config.logToConsole) {
      console.log(
        `Benchmark "${name}" (${iterCount} iterations): ` +
          `Avg: ${result.average.toFixed(2)}ms, ` +
          `Median: ${result.median.toFixed(2)}ms, ` +
          `Min: ${result.min.toFixed(2)}ms, ` +
          `Max: ${result.max.toFixed(2)}ms, ` +
          `StdDev: ${result.stdDev.toFixed(2)}ms`
      );
    }

    return result;
  }

  /**
   * Run multiple benchmarks
   * @param benchmarks Array of benchmark configurations
   * @returns Array of benchmark results
   */
  public runMultiple(benchmarks: BenchmarkConfig[]): BenchmarkResult[] {
    const results: BenchmarkResult[] = [];

    for (const benchmark of benchmarks) {
      const result = this.run(benchmark.name, benchmark.fn, benchmark.iterations);
      results.push(result);
    }

    return results;
  }

  /**
   * Compare two benchmark results
   * @param result1 First benchmark result
   * @param result2 Second benchmark result
   * @returns Comparison result
   */
  public compare(result1: BenchmarkResult, result2: BenchmarkResult): BenchmarkComparison {
    const avgDiff = result2.average - result1.average;
    const avgPercent = (avgDiff / result1.average) * 100;
    const medianDiff = result2.median - result1.median;
    const medianPercent = (medianDiff / result1.median) * 100;
    const minDiff = result2.min - result1.min;
    const minPercent = (minDiff / result1.min) * 100;
    const maxDiff = result2.max - result1.max;
    const maxPercent = (maxDiff / result1.max) * 100;

    const comparison: BenchmarkComparison = {
      name1: result1.name,
      name2: result2.name,
      avgDiff,
      avgPercent,
      medianDiff,
      medianPercent,
      minDiff,
      minPercent,
      maxDiff,
      maxPercent,
      result1,
      result2,
    };

    // Log to console if enabled
    if (this.config.logToConsole) {
      const sign = avgPercent >= 0 ? '+' : '';
      console.log(
        `Benchmark comparison "${result1.name}" vs "${result2.name}": ` +
          `Avg: ${sign}${avgPercent.toFixed(2)}%, ` +
          `Median: ${sign}${medianPercent.toFixed(2)}%, ` +
          `Min: ${sign}${minPercent.toFixed(2)}%, ` +
          `Max: ${sign}${maxPercent.toFixed(2)}%`
      );
    }

    return comparison;
  }

  /**
   * Get all benchmark results
   */
  public getResults(): BenchmarkResult[] {
    return [...this.results];
  }

  /**
   * Clear all benchmark results
   */
  public clearResults(): void {
    this.results = [];
  }

  /**
   * Generate a benchmark report
   */
  public generateReport(): BenchmarkReport {
    const report: BenchmarkReport = {
      timestamp: Date.now(),
      results: [...this.results],
      summary: {
        totalBenchmarks: this.results.length,
        totalTime: this.results.reduce((sum, result) => sum + result.total, 0),
        averageTime:
          this.results.reduce((sum, result) => sum + result.average, 0) / this.results.length,
      },
    };

    // Log to console if enabled
    if (this.config.logToConsole) {
      console.log(
        `Benchmark report: ` +
          `${report.summary.totalBenchmarks} benchmarks, ` +
          `Total time: ${report.summary.totalTime.toFixed(2)}ms, ` +
          `Average time: ${report.summary.averageTime.toFixed(2)}ms`
      );
    }

    return report;
  }
}

/**
 * Performance benchmark configuration
 */
export interface PerformanceBenchmarkConfig {
  iterations: number;
  warmupIterations: number;
  logToConsole: boolean;
}

/**
 * Benchmark configuration
 */
export interface BenchmarkConfig {
  name: string;
  fn: () => void;
  iterations?: number;
}

/**
 * Benchmark comparison
 */
export interface BenchmarkComparison {
  name1: string;
  name2: string;
  avgDiff: number;
  avgPercent: number;
  medianDiff: number;
  medianPercent: number;
  minDiff: number;
  minPercent: number;
  maxDiff: number;
  maxPercent: number;
  result1: BenchmarkResult;
  result2: BenchmarkResult;
}

/**
 * Benchmark report
 */
export interface BenchmarkReport {
  timestamp: number;
  results: BenchmarkResult[];
  summary: {
    totalBenchmarks: number;
    totalTime: number;
    averageTime: number;
  };
}

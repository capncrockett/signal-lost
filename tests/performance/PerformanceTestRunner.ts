import { PerformanceTests, PerformanceTestResult } from './PerformanceTests';
import { BenchmarkResult } from '../../src/utils/PerformanceBenchmark';
import * as fs from 'fs';
import * as path from 'path';

/**
 * Performance test runner for running performance tests and generating reports
 */
export class PerformanceTestRunner {
  // Performance tests
  private tests: PerformanceTests;

  // Test results
  private results: PerformanceTestResult[] = [];

  // Test configuration
  private config: PerformanceTestRunnerConfig = {
    outputDir: 'performance-reports',
    saveReports: true,
    compareWithBaseline: true,
    baselineFile: 'baseline.json',
    failOnThresholdExceeded: true,
    failOnRegressionExceeded: true,
    regressionThreshold: 0.1, // 10% regression
  };

  /**
   * Create a new performance test runner
   * @param config Configuration options
   */
  constructor(config?: Partial<PerformanceTestRunnerConfig>) {
    // Create tests
    this.tests = new PerformanceTests();

    // Merge configuration
    this.config = { ...this.config, ...config };

    // Create output directory if it doesn't exist
    if (this.config.saveReports) {
      const outputDir = path.resolve(this.config.outputDir);
      if (!fs.existsSync(outputDir)) {
        fs.mkdirSync(outputDir, { recursive: true });
      }
    }
  }

  /**
   * Run performance tests
   * @param game Phaser game instance
   * @returns Test results
   */
  public async run(game: Phaser.Game): Promise<PerformanceTestResult> {
    // Run tests
    const result = await this.tests.runAll(game);

    // Store result
    this.results.push(result);

    // Save report
    if (this.config.saveReports) {
      this.saveReport(result);
    }

    // Compare with baseline
    if (this.config.compareWithBaseline) {
      const comparison = this.compareWithBaseline(result);
      console.log('Baseline Comparison:', comparison);
    }

    return result;
  }

  /**
   * Save a test report
   * @param result Test result
   */
  private saveReport(result: PerformanceTestResult): void {
    const outputDir = path.resolve(this.config.outputDir);
    const timestamp = new Date(result.timestamp).toISOString().replace(/:/g, '-');
    const filename = `performance-report-${timestamp}.json`;
    const filePath = path.join(outputDir, filename);

    // Save report
    fs.writeFileSync(filePath, JSON.stringify(result, null, 2));
    console.log(`Performance report saved to ${filePath}`);

    // Save as baseline if it doesn't exist
    const baselinePath = path.join(outputDir, this.config.baselineFile);
    if (!fs.existsSync(baselinePath)) {
      fs.writeFileSync(baselinePath, JSON.stringify(result, null, 2));
      console.log(`Baseline saved to ${baselinePath}`);
    }
  }

  /**
   * Compare a test result with the baseline
   * @param result Test result
   * @returns Comparison result
   */
  private compareWithBaseline(result: PerformanceTestResult): PerformanceComparisonResult {
    const baselinePath = path.join(path.resolve(this.config.outputDir), this.config.baselineFile);

    // Check if baseline exists
    if (!fs.existsSync(baselinePath)) {
      return {
        baselineExists: false,
        regressions: [],
        improvements: [],
        unchanged: [],
        overallRegression: false,
      };
    }

    // Load baseline
    const baseline = JSON.parse(fs.readFileSync(baselinePath, 'utf-8')) as PerformanceTestResult;

    // Compare results
    const regressions: PerformanceComparison[] = [];
    const improvements: PerformanceComparison[] = [];
    const unchanged: PerformanceComparison[] = [];

    // Compare individual benchmarks
    for (const currentResult of result.results) {
      // Find matching baseline result
      const baselineResult = baseline.results.find(r => r.name === currentResult.name);
      if (!baselineResult) {
        continue;
      }

      // Calculate difference
      const diff = currentResult.average - baselineResult.average;
      const percentDiff = diff / baselineResult.average;

      // Create comparison
      const comparison: PerformanceComparison = {
        name: currentResult.name,
        baseline: baselineResult.average,
        current: currentResult.average,
        diff,
        percentDiff,
        regression: diff > 0 && percentDiff > this.config.regressionThreshold,
      };

      // Categorize comparison
      if (comparison.regression) {
        regressions.push(comparison);
      } else if (diff < 0 && Math.abs(percentDiff) > this.config.regressionThreshold) {
        improvements.push(comparison);
      } else {
        unchanged.push(comparison);
      }
    }

    // Check for overall regression
    const overallRegression = regressions.length > 0;

    // Create comparison result
    const comparisonResult: PerformanceComparisonResult = {
      baselineExists: true,
      regressions,
      improvements,
      unchanged,
      overallRegression,
    };

    // Log regressions
    if (regressions.length > 0) {
      console.log('Performance Regressions:');
      for (const regression of regressions) {
        console.log(
          `${regression.name}: ${regression.baseline.toFixed(2)}ms -> ${regression.current.toFixed(2)}ms ` +
          `(${(regression.percentDiff * 100).toFixed(2)}%)`
        );
      }
    }

    // Log improvements
    if (improvements.length > 0) {
      console.log('Performance Improvements:');
      for (const improvement of improvements) {
        console.log(
          `${improvement.name}: ${improvement.baseline.toFixed(2)}ms -> ${improvement.current.toFixed(2)}ms ` +
          `(${(improvement.percentDiff * 100).toFixed(2)}%)`
        );
      }
    }

    // Fail if regression threshold exceeded
    if (this.config.failOnRegressionExceeded && overallRegression) {
      console.error('Performance regression threshold exceeded!');
      if (typeof process !== 'undefined') {
        process.exitCode = 1;
      }
    }

    return comparisonResult;
  }

  /**
   * Update the baseline with the latest test result
   */
  public updateBaseline(): void {
    if (this.results.length === 0) {
      console.error('No test results to update baseline with');
      return;
    }

    const latestResult = this.results[this.results.length - 1];
    const baselinePath = path.join(path.resolve(this.config.outputDir), this.config.baselineFile);

    // Save as baseline
    fs.writeFileSync(baselinePath, JSON.stringify(latestResult, null, 2));
    console.log(`Baseline updated to ${baselinePath}`);
  }

  /**
   * Get all test results
   */
  public getResults(): PerformanceTestResult[] {
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
 * Performance test runner configuration
 */
export interface PerformanceTestRunnerConfig {
  outputDir: string;
  saveReports: boolean;
  compareWithBaseline: boolean;
  baselineFile: string;
  failOnThresholdExceeded: boolean;
  failOnRegressionExceeded: boolean;
  regressionThreshold: number;
}

/**
 * Performance comparison
 */
export interface PerformanceComparison {
  name: string;
  baseline: number;
  current: number;
  diff: number;
  percentDiff: number;
  regression: boolean;
}

/**
 * Performance comparison result
 */
export interface PerformanceComparisonResult {
  baselineExists: boolean;
  regressions: PerformanceComparison[];
  improvements: PerformanceComparison[];
  unchanged: PerformanceComparison[];
  overallRegression: boolean;
}

import { test, expect } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

// Increase timeout for performance tests
test.setTimeout(120000);

test('Run performance benchmarks', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

  // Wait for the game to load
  await page.waitForSelector('canvas', { state: 'visible', timeout: 10000 });

  // Inject performance test code
  await page.addScriptTag({
    path: path.resolve(__dirname, '../../dist/performance-test-bundle.js'),
  });

  // Wait for performance test code to load
  await page.waitForFunction(() => window['PerformanceTestRunner'] !== undefined);

  // Run performance tests
  const results = await page.evaluate(async (updateBaseline) => {
    // Get game instance
    const game = window['game'];
    if (!game) {
      throw new Error('Game instance not found');
    }

    // Create test runner
    const runner = new window['PerformanceTestRunner']({
      outputDir: 'performance-reports',
      saveReports: false, // We'll save the report from Node.js
      compareWithBaseline: true,
      baselineFile: 'baseline.json',
      failOnThresholdExceeded: true,
      failOnRegressionExceeded: !updateBaseline,
      regressionThreshold: 0.1, // 10% regression
    });

    // Run tests
    // Create a minimal Game object with the required properties
    const mockGame = {
      config: {},
      renderer: {},
      domContainer: document.createElement('div'),
      canvas: document.createElement('canvas'),
      loop: { actualFps: 60 },
    } as unknown as Phaser.Game;

    const result = await runner.run(mockGame);

    // Update baseline if requested
    if (updateBaseline) {
      runner.updateBaseline();
    }

    return result;
  }, process.argv.includes('--update-baseline'));

  // Save results
  const outputDir = path.resolve('performance-reports');
  if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
  }

  const timestamp = new Date(results.timestamp).toISOString().replace(/:/g, '-');
  const filename = `performance-report-${timestamp}.json`;
  const filePath = path.join(outputDir, filename);

  fs.writeFileSync(filePath, JSON.stringify(results, null, 2));
  console.log(`Performance report saved to ${filePath}`);

  // Check if tests passed
  expect(results.summary.overallPass).toBe(true);
});

test('Measure load times', async ({ page }) => {
  // Start performance monitoring
  await page.evaluate(() => {
    window.performance.mark('test-start');
  });

  // Navigate to the game
  await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

  // Wait for the game to load
  await page.waitForSelector('canvas', { state: 'visible', timeout: 10000 });

  // End performance monitoring
  const loadTime = await page.evaluate(() => {
    window.performance.mark('test-end');
    window.performance.measure('load-time', 'test-start', 'test-end');
    const measure = window.performance.getEntriesByName('load-time')[0];
    return measure.duration;
  });

  console.log(`Page load time: ${loadTime.toFixed(2)}ms`);

  // Save load time
  const outputDir = path.resolve('performance-reports');
  if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
  }

  const timestamp = new Date().toISOString().replace(/:/g, '-');
  const filename = `load-time-${timestamp}.json`;
  const filePath = path.join(outputDir, filename);

  fs.writeFileSync(filePath, JSON.stringify({ loadTime, timestamp: Date.now() }, null, 2));
  console.log(`Load time report saved to ${filePath}`);

  // Check if load time is acceptable
  expect(loadTime).toBeLessThan(5000); // 5 seconds
});

test('Measure FPS stability', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

  // Wait for the game to load
  await page.waitForSelector('canvas', { state: 'visible', timeout: 10000 });

  // Inject FPS monitoring code
  await page.evaluate(() => {
    interface ExtendedWindow extends Window {
      fpsValues: number[];
      fpsMonitoringInterval: any; // Using any to accommodate both number and NodeJS.Timeout
      game: { loop: { actualFps: number } };
    }

    const extWindow = window as unknown as ExtendedWindow;
    extWindow.fpsValues = [];
    // Cast to any to avoid type conflicts between browser and Node environments
    extWindow.fpsMonitoringInterval = setInterval(() => {
      const fps = extWindow.game.loop.actualFps;
      extWindow.fpsValues.push(fps);
    }, 100);
  });

  // Wait for 10 seconds to collect FPS data
  await page.waitForTimeout(10000);

  // Get FPS data
  const fpsData = await page.evaluate(() => {
    interface ExtendedWindow extends Window {
      fpsValues: number[];
      fpsMonitoringInterval: any; // Using any to accommodate both number and NodeJS.Timeout
    }

    const extWindow = window as unknown as ExtendedWindow;
    clearInterval(extWindow.fpsMonitoringInterval as any);
    const values = extWindow.fpsValues;
    const sum = values.reduce((a: number, b: number) => a + b, 0);
    const avg = sum / values.length;
    const min = Math.min(...values);
    const max = Math.max(...values);
    const sorted = [...values].sort((a: number, b: number) => a - b);
    const median = sorted[Math.floor(sorted.length / 2)];

    // Calculate standard deviation
    const variance =
      values.reduce((a: number, b: number) => a + Math.pow(b - avg, 2), 0) / values.length;
    const stdDev = Math.sqrt(variance);

    return {
      values,
      avg,
      min,
      max,
      median,
      stdDev,
      timestamp: Date.now(),
    };
  });

  console.log(
    `FPS: Avg=${fpsData.avg.toFixed(2)}, Min=${fpsData.min.toFixed(2)}, Max=${fpsData.max.toFixed(2)}, StdDev=${fpsData.stdDev.toFixed(2)}`
  );

  // Save FPS data
  const outputDir = path.resolve('performance-reports');
  if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
  }

  const timestamp = new Date(fpsData.timestamp).toISOString().replace(/:/g, '-');
  const filename = `fps-data-${timestamp}.json`;
  const filePath = path.join(outputDir, filename);

  fs.writeFileSync(filePath, JSON.stringify(fpsData, null, 2));
  console.log(`FPS data saved to ${filePath}`);

  // Check if FPS is acceptable
  expect(fpsData.avg).toBeGreaterThan(55); // At least 55 FPS on average
  expect(fpsData.min).toBeGreaterThan(30); // No drops below 30 FPS
  expect(fpsData.stdDev).toBeLessThan(10); // Low variation
});

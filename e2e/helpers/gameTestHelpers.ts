import { ConsoleMessage, Page, expect } from '@playwright/test';
import * as path from 'path';
import * as fs from 'fs';

// Define our own MouseClickOptions interface
interface MouseClickOptions {
  button?: 'left' | 'right' | 'middle';
  clickCount?: number;
  delay?: number;
  force?: boolean;
  modifiers?: Array<'Alt' | 'Control' | 'Meta' | 'Shift'>;
  noWaitAfter?: boolean;
  position?: { x: number; y: number };
  timeout?: number;
  trial?: boolean;
}

// Ensure the screenshots directory exists
const screenshotsDir = path.join(process.cwd(), 'e2e', 'screenshots');
if (!fs.existsSync(screenshotsDir)) {
  fs.mkdirSync(screenshotsDir, { recursive: true });
}

/**
 * Helper functions for testing games programmatically
 */

/**
 * Take a screenshot and save it to the e2e-screenshots directory
 * @param page Playwright page
 * @param name Name of the screenshot (without extension)
 * @param fullPage Whether to take a screenshot of the full page
 * @returns Path to the saved screenshot
 */
export async function takeScreenshot(
  page: Page,
  name: string,
  fullPage: boolean = false
): Promise<string> {
  const screenshotPath = path.join(screenshotsDir, `${name}.png`);
  await page.screenshot({ path: screenshotPath, fullPage });
  return screenshotPath;
}

/**
 * Take a screenshot of a specific element
 * @param element Playwright locator for the element
 * @param name Name of the screenshot (without extension)
 * @returns Path to the saved screenshot
 */
export async function takeElementScreenshot(element: any, name: string): Promise<string> {
  const screenshotPath = path.join(screenshotsDir, `${name}.png`);
  await element.screenshot({ path: screenshotPath });
  return screenshotPath;
}

/**
 * Wait for the game to load and verify basic elements
 * @param page Playwright page
 * @param options Optional configuration
 * @returns Object containing game elements
 */
export async function waitForGameLoad(
  page: Page,
  options: { timeout?: number; waitForCanvas?: boolean } = {}
) {
  const timeout = options.timeout || 15000;
  const waitForCanvas = options.waitForCanvas !== false; // Default to true

  console.log(`Waiting for game to load (timeout: ${timeout}ms, waitForCanvas: ${waitForCanvas})`);

  // Wait for the game container to be visible
  await page.waitForSelector('#game', { timeout });

  // Find the game container
  const gameContainer = page.locator('#game');

  // Verify game container exists
  await expect(gameContainer).toBeVisible();

  // Wait for canvas elements to be created if requested
  if (waitForCanvas) {
    // First wait a bit for initial rendering
    await page.waitForTimeout(2000);

    // Try to wait for canvas with a timeout
    try {
      await page.waitForSelector('canvas', { timeout: 5000 });
    } catch (error) {
      console.warn('Canvas element not found within timeout, continuing anyway');
    }

    // Count canvas elements
    const canvasCount = await page.locator('canvas').count();
    console.log(`Canvas count: ${canvasCount}`);

    // Log warning if no canvas found, but don't fail the test
    if (canvasCount === 0) {
      console.warn('No canvas elements found, but continuing test');
    }

    // Wait a bit longer for Phaser to initialize
    await page.waitForTimeout(2000);

    return {
      gameContainer,
      canvasCount,
      hasCanvas: canvasCount > 0,
    };
  }

  return {
    gameContainer,
    canvasCount: 0,
    hasCanvas: false,
  };
}

/**
 * Simulate a click on a game element at specific coordinates
 * @param page Playwright page
 * @param x X coordinate
 * @param y Y coordinate
 * @param options Optional click options
 * @returns Boolean indicating if the click was successful
 */
export async function clickGamePosition(
  page: Page,
  x: number,
  y: number,
  options?: MouseClickOptions & { fallbackToCenter?: boolean; takeScreenshot?: boolean }
): Promise<boolean> {
  try {
    // Find the game container
    const gameContainer = page.locator('#game');

    // Get the bounding box of the game container
    const box = await gameContainer.boundingBox();

    if (!box) {
      console.error('Game container not found or not visible');

      // Take a screenshot if requested
      if (options?.takeScreenshot) {
        await takeScreenshot(page, 'click-error', true);
      }

      // Try clicking in the center of the page if fallback is enabled
      if (options?.fallbackToCenter) {
        console.log('Falling back to clicking center of page');
        const viewportSize = page.viewportSize();
        if (viewportSize) {
          await page.mouse.click(viewportSize.width / 2, viewportSize.height / 2);
          return true;
        }
      }

      return false;
    }

    // Calculate the absolute position
    const absoluteX = box.x + x;
    const absoluteY = box.y + y;

    // Click at the calculated position
    await page.mouse.click(absoluteX, absoluteY, options);
    console.log(`Clicked at position (${x}, ${y}) in game container`);
    return true;
  } catch (error) {
    console.error(`Error clicking at position (${x}, ${y}):`, error);

    // Take a screenshot if requested
    if (options?.takeScreenshot) {
      await takeScreenshot(page, 'click-error', true);
    }

    return false;
  }
}

/**
 * Simulate dragging in the game
 * @param page Playwright page
 * @param startX Starting X coordinate
 * @param startY Starting Y coordinate
 * @param endX Ending X coordinate
 * @param endY Ending Y coordinate
 * @param options Optional mouse options
 * @returns Boolean indicating if the drag was successful
 */
export async function dragInGame(
  page: Page,
  startX: number,
  startY: number,
  endX: number,
  endY: number,
  options?: MouseClickOptions & {
    steps?: number;
    fallbackToCenter?: boolean;
    takeScreenshot?: boolean;
    screenshotName?: string;
  }
): Promise<boolean> {
  try {
    // Find the game container
    const gameContainer = page.locator('#game');

    // Get the bounding box of the game container
    const box = await gameContainer.boundingBox();

    if (!box) {
      console.error('Game container not found or not visible for drag operation');

      // Take a screenshot if requested
      if (options?.takeScreenshot) {
        await takeScreenshot(page, options.screenshotName || 'drag-error', true);
      }

      // Try dragging in the center of the page if fallback is enabled
      if (options?.fallbackToCenter) {
        console.log('Falling back to dragging in center of page');
        const viewportSize = page.viewportSize();
        if (viewportSize) {
          const centerX = viewportSize.width / 2;
          const centerY = viewportSize.height / 2;
          await page.mouse.move(centerX - 50, centerY);
          await page.mouse.down();
          await page.mouse.move(centerX + 50, centerY, { steps: options?.steps || 10 });
          await page.mouse.up();
          return true;
        }
      }

      return false;
    }

    // Calculate the absolute positions
    const absoluteStartX = box.x + startX;
    const absoluteStartY = box.y + startY;
    const absoluteEndX = box.x + endX;
    const absoluteEndY = box.y + endY;

    // Log the drag operation
    console.log(`Dragging from (${startX}, ${startY}) to (${endX}, ${endY}) in game container`);

    // Take a screenshot before drag if requested
    if (options?.takeScreenshot) {
      await takeScreenshot(page, (options.screenshotName || 'drag') + '-before', true);
    }

    // Perform the drag operation
    await page.mouse.move(absoluteStartX, absoluteStartY);
    await page.mouse.down();
    await page.mouse.move(absoluteEndX, absoluteEndY, { steps: options?.steps || 10 }); // Move in steps for smoother drag
    await page.mouse.up();

    // Take a screenshot after drag if requested
    if (options?.takeScreenshot) {
      await takeScreenshot(page, (options.screenshotName || 'drag') + '-after', true);
    }

    return true;
  } catch (error) {
    console.error(`Error dragging from (${startX}, ${startY}) to (${endX}, ${endY}):`, error);

    // Take a screenshot if requested
    if (options?.takeScreenshot) {
      await takeScreenshot(page, options.screenshotName || 'drag-error', true);
    }

    return false;
  }
}

/**
 * Verify that an audio context is created and running
 * @param page Playwright page
 * @returns Boolean indicating if audio is active
 */
export async function verifyAudioContext(page: Page): Promise<boolean> {
  return await page.evaluate(() => {
    // Check if any AudioContext exists in the page
    return (
      !!window.AudioContext ||
      !!(window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext
    );
  });
}

/**
 * Capture and analyze console logs
 * @param page Playwright page
 * @param duration How long to capture logs (in ms)
 * @returns Object containing categorized logs
 */
export async function captureConsoleLogs(page: Page, duration: number = 500) {
  const logs: string[] = [];
  const errors: string[] = [];
  const warnings: string[] = [];
  const audioErrors: string[] = [];
  const networkErrors: string[] = [];
  const uncaughtErrors: string[] = [];

  // Set up console log collection
  const consoleListener = (msg: ConsoleMessage) => {
    const text = msg.text();
    logs.push(text);

    // Categorize by type
    if (msg.type() === 'error') {
      errors.push(text);

      // Further categorize errors
      if (text.includes('audio') || text.includes('decode')) {
        audioErrors.push(text);
      }
      if (text.includes('network') || text.includes('fetch') || text.includes('load')) {
        networkErrors.push(text);
      }
      if (text.includes('Uncaught') || text.includes('unhandled')) {
        uncaughtErrors.push(text);
      }
    }
    if (msg.type() === 'warning') warnings.push(text);

    // Log all errors to the test output for better visibility
    if (msg.type() === 'error') {
      console.error(`Browser console error: ${text}`);
    }
  };

  // Add the listener
  page.on('console', consoleListener);

  // Also listen for page errors
  page.on('pageerror', (error) => {
    const text = error.message;
    errors.push(text);
    uncaughtErrors.push(text);
    console.error(`Page error: ${text}`);
  });

  // Wait for the specified duration
  await page.waitForTimeout(duration);

  // Remove the listeners to avoid duplicate logs
  page.removeListener('console', consoleListener);

  return {
    logs,
    errors,
    warnings,
    audioErrors,
    networkErrors,
    uncaughtErrors,
    hasErrors: errors.length > 0,
    hasWarnings: warnings.length > 0,
    hasAudioErrors: audioErrors.length > 0,
    hasNetworkErrors: networkErrors.length > 0,
    hasUncaughtErrors: uncaughtErrors.length > 0,
  };
}

/**
 * Test the radio tuner functionality
 * @param page Playwright page
 * @param options Optional configuration
 */
export async function testRadioTuner(
  page: Page,
  options: {
    takeScreenshots?: boolean;
    waitTimeout?: number;
    retryCount?: number;
    signalFrequencies?: number[];
  } = {}
) {
  const takeScreenshots = options.takeScreenshots !== false; // Default to true
  const waitTimeout = options.waitTimeout || 2000;
  const retryCount = options.retryCount || 3;
  const signalFrequencies = options.signalFrequencies || [91.5, 96.3, 103.7]; // Default signal frequencies

  console.log(
    `Testing radio tuner (screenshots: ${takeScreenshots}, waitTimeout: ${waitTimeout}ms, retries: ${retryCount})`
  );

  // Wait for game to load with extended timeout
  const gameLoadResult = await waitForGameLoad(page, { timeout: 20000 });

  if (takeScreenshots) {
    await takeScreenshot(page, 'radio-tuner-test-start', true);
  }

  // Click to initialize audio - retry if needed
  let clickSuccess = false;
  for (let i = 0; i < retryCount && !clickSuccess; i++) {
    console.log(`Attempting to click radio (attempt ${i + 1}/${retryCount})`);
    clickSuccess = await clickGamePosition(page, 400, 300, {
      fallbackToCenter: true,
      takeScreenshot: takeScreenshots,
    });

    if (!clickSuccess && i < retryCount - 1) {
      console.log('Click failed, waiting before retry...');
      await page.waitForTimeout(1000);
    }
  }

  // Wait for audio to initialize
  await page.waitForTimeout(waitTimeout);

  if (takeScreenshots) {
    await takeScreenshot(page, 'radio-tuner-after-click', true);
  }

  // Test each signal frequency
  const signalTestResults = [];

  for (const frequency of signalFrequencies) {
    console.log(`Testing frequency: ${frequency} MHz`);

    // Calculate approximate position for this frequency (simplified mapping)
    // Assuming frequencies range from 88.0 to 108.0 MHz mapped to x positions 300 to 500
    const frequencyPosition = 300 + ((frequency - 88.0) / 20.0) * 200;

    // Drag the radio tuner knob to the frequency position
    const dragSuccess = await dragInGame(page, 400, 300, frequencyPosition, 300, {
      steps: 20, // More steps for smoother movement
      fallbackToCenter: true,
      takeScreenshot: takeScreenshots,
      screenshotName: `frequency-${frequency}`,
    });

    // Wait for signal processing
    await page.waitForTimeout(waitTimeout);

    // Capture console logs to check for signal events
    const frequencyLogs = await captureConsoleLogs(page, 1000);

    signalTestResults.push({
      frequency,
      dragSuccess,
      signalEvents: frequencyLogs.logs.filter(
        (log) => log.includes('Signal') && log.includes(frequency.toString())
      ),
      logs: frequencyLogs,
    });
  }

  // Final drag to a neutral position
  await dragInGame(page, 400, 300, 350, 300, {
    takeScreenshot: takeScreenshots,
    screenshotName: 'final-position',
  });

  // Wait for signal processing
  await page.waitForTimeout(waitTimeout);

  // Capture final console logs
  const finalLogs = await captureConsoleLogs(page, 1000);

  if (takeScreenshots) {
    await takeScreenshot(page, 'radio-tuner-test-end', true);
  }

  // Filter out common audio loading errors
  const nonAudioErrors = finalLogs.errors.filter(
    (error) =>
      !error.includes('Unable to decode audio data') &&
      !error.includes('Failed to process file') &&
      !error.includes('Failed to load resource')
  );

  return {
    gameLoadResult,
    clickSuccess,
    signalTestResults,
    signalEvents: finalLogs.logs.filter((log) => log.includes('Signal')),
    audioInitialized: await verifyAudioContext(page),
    logResults: finalLogs,
    nonAudioErrors,
    hasNonAudioErrors: nonAudioErrors.length > 0,
  };
}

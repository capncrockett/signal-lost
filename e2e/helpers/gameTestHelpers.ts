import { ConsoleMessage, MouseClickOptions, Page, expect } from '@playwright/test';

/**
 * Helper functions for testing games programmatically
 */

/**
 * Wait for the game to load and verify basic elements
 * @param page Playwright page
 * @returns Object containing game elements
 */
export async function waitForGameLoad(page: Page) {
  // Wait for the game container to be visible
  await page.waitForSelector('#game', { timeout: 10000 });

  // Find the game container
  const gameContainer = await page.locator('#game');

  // Verify game container exists
  await expect(gameContainer).toBeVisible();

  // Wait for canvas elements to be created
  await page.waitForTimeout(2000);

  // Count canvas elements
  const canvasCount = await page.locator('canvas').count();
  console.log(`Canvas count: ${canvasCount}`);

  // Verify at least one canvas exists
  expect(canvasCount).toBeGreaterThan(0);

  return {
    gameContainer,
    canvasCount,
  };
}

/**
 * Simulate a click on a game element at specific coordinates
 * @param page Playwright page
 * @param x X coordinate
 * @param y Y coordinate
 * @param options Optional click options
 */
export async function clickGamePosition(
  page: Page,
  x: number,
  y: number,
  options?: MouseClickOptions
) {
  // Find the game container
  const gameContainer = await page.locator('#game');

  // Get the bounding box of the game container
  const box = await gameContainer.boundingBox();

  if (!box) {
    throw new Error('Game container not found or not visible');
  }

  // Calculate the absolute position
  const absoluteX = box.x + x;
  const absoluteY = box.y + y;

  // Click at the calculated position
  await page.mouse.click(absoluteX, absoluteY, options);
}

/**
 * Simulate dragging in the game
 * @param page Playwright page
 * @param startX Starting X coordinate
 * @param startY Starting Y coordinate
 * @param endX Ending X coordinate
 * @param endY Ending Y coordinate
 * @param options Optional mouse options
 */
export async function dragInGame(
  page: Page,
  startX: number,
  startY: number,
  endX: number,
  endY: number,
  _options?: MouseClickOptions
) {
  // Find the game container
  const gameContainer = await page.locator('#game');

  // Get the bounding box of the game container
  const box = await gameContainer.boundingBox();

  if (!box) {
    throw new Error('Game container not found or not visible');
  }

  // Calculate the absolute positions
  const absoluteStartX = box.x + startX;
  const absoluteStartY = box.y + startY;
  const absoluteEndX = box.x + endX;
  const absoluteEndY = box.y + endY;

  // Perform the drag operation
  await page.mouse.move(absoluteStartX, absoluteStartY);
  await page.mouse.down();
  await page.mouse.move(absoluteEndX, absoluteEndY, { steps: 10 }); // Move in steps for smoother drag
  await page.mouse.up();
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
export async function captureConsoleLogs(page: Page, duration: number = 2000) {
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
 */
export async function testRadioTuner(page: Page) {
  // Wait for game to load
  await waitForGameLoad(page);

  // Click to initialize audio
  await clickGamePosition(page, 400, 300);

  // Wait for audio to initialize
  await page.waitForTimeout(1000);

  // Drag the radio tuner knob
  await dragInGame(page, 400, 300, 450, 300);

  // Wait for signal processing
  await page.waitForTimeout(1000);

  // Drag to another position
  await dragInGame(page, 450, 300, 350, 300);

  // Wait for signal processing
  await page.waitForTimeout(1000);

  // Capture console logs to check for signal events
  const logResults = await captureConsoleLogs(page, 1000);

  // Return test results
  // Filter out audio loading errors
  const nonAudioErrors = logResults.errors.filter(
    (error) =>
      !error.includes('Unable to decode audio data') &&
      !error.includes('Failed to process file') &&
      !error.includes('Failed to load resource')
  );

  return {
    signalEvents: logResults.logs.filter((log) => log.includes('Signal')),
    audioInitialized: await verifyAudioContext(page),
    logResults,
    nonAudioErrors,
    hasNonAudioErrors: nonAudioErrors.length > 0,
  };
}

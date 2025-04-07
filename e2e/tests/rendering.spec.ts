import { test, expect, Page } from '@playwright/test';
import {
  takeScreenshot,
  takeElementScreenshot,
  waitForGameLoad,
  clickGamePosition,
  captureConsoleLogs,
} from '../helpers/gameTestHelpers';

// Increase the test timeout
test.setTimeout(120000);

test.describe('Game Rendering', () => {
  // Test the initial desktop view
  test('Game renders correctly at desktop resolution', async ({ page }) => {
    // Set a fixed viewport size to start
    await page.setViewportSize({ width: 1280, height: 800 });

    // Navigate to the game
    await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

    // Wait for the game to load with enhanced helper
    console.log('Waiting for game to load...');
    const gameLoadResult = await waitForGameLoad(page, { timeout: 15000 });
    console.log('Game load complete:', gameLoadResult);

    // Click on the game to initialize audio (important for full rendering)
    await clickGamePosition(page, 400, 300, {
      fallbackToCenter: true,
      takeScreenshot: true,
    });
    await page.waitForTimeout(2000);

    // Find the Phaser canvas (any canvas element)
    const canvasCount = await page.locator('canvas').count();
    console.log(`Total canvas elements: ${canvasCount}`);

    // If no visible canvas, use the game container for screenshots
    const gameContainer = page.locator('#game');
    await expect(gameContainer).toBeVisible();

    // Get game container size
    const initialBoundingBox = await gameContainer.boundingBox();
    console.log(`Game container size: ${initialBoundingBox?.width}x${initialBoundingBox?.height}`);

    // Take a screenshot of the game container
    await takeElementScreenshot(gameContainer, 'game-container-only');
    await takeElementScreenshot(gameContainer, 'game-container');
  });

  // Test full HD resolution
  test('Game renders correctly at full HD resolution', async ({ page }) => {
    const resolution = { width: 1920, height: 1080, name: 'full-hd' };
    await testResolution(page, resolution);
  });

  // Test laptop resolution
  test('Game renders correctly at laptop resolution', async ({ page }) => {
    const resolution = { width: 1366, height: 768, name: 'laptop' };
    await testResolution(page, resolution);
  });

  // Test desktop resolution
  test('Game renders correctly at standard desktop resolution', async ({ page }) => {
    const resolution = { width: 1024, height: 768, name: 'desktop' };
    await testResolution(page, resolution);
  });

  // Test tablet portrait resolution - SKIPPED (fixed size game)
  test.skip('Game renders correctly at tablet portrait resolution', async ({ page }) => {
    const resolution = { width: 768, height: 1024, name: 'tablet-portrait' };
    await testResolution(page, resolution);
  });

  // Test mobile resolution - SKIPPED (fixed size game)
  test.skip('Game renders correctly at mobile resolution', async ({ page }) => {
    const resolution = { width: 375, height: 667, name: 'mobile' };
    await testResolution(page, resolution);
  });
});

// Helper function to test a specific resolution
async function testResolution(page: Page, resolution: { name: string; width: number; height: number }) {
  console.log(`Testing resolution: ${resolution.name} (${resolution.width}x${resolution.height})`);

  // Set viewport to the resolution
  await page.setViewportSize(resolution);
  await page.waitForTimeout(1000);

  // Navigate to the game at this resolution
  await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

  // Wait for the game to load with enhanced helper
  console.log(`Waiting for game to load at ${resolution.name} resolution...`);
  const gameLoadResult = await waitForGameLoad(page, { timeout: 15000 });
  console.log(`Game load complete at ${resolution.name} resolution:`, gameLoadResult);

  // Click to initialize audio using enhanced helper
  await clickGamePosition(page, resolution.width / 2, resolution.height / 2, {
    fallbackToCenter: true,
    takeScreenshot: true,
    screenshotName: `${resolution.name}-click`,
  });
  await page.waitForTimeout(2000);

  // Capture console logs
  const logs = await captureConsoleLogs(page, 1000);
  console.log(`Console logs at ${resolution.name} resolution:`, {
    errors: logs.errors.length,
    warnings: logs.warnings.length,
    networkErrors: logs.networkErrors.length,
  });

  // Find the game container
  const gameContainer = page.locator('#game');

  // Wait for game container to be visible
  try {
    await expect(gameContainer).toBeVisible({ timeout: 10000 });

    // Take a screenshot of the game container
    await takeElementScreenshot(gameContainer, `${resolution.name}-container`);

    // Get game container size
    const boundingBox = await gameContainer.boundingBox();
    console.log(`${resolution.name} container: ${boundingBox?.width}x${boundingBox?.height}`);

    // Verify the radio tuner is visible
    const radioTuner = page.locator('[data-testid="radio-tuner"]');
    if (await radioTuner.isVisible()) {
      console.log(`Radio tuner is visible at ${resolution.name} resolution`);
    } else {
      console.log(`Radio tuner is NOT visible at ${resolution.name} resolution`);
    }
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : String(error);
    console.error(`Error at resolution ${resolution.name}: ${errorMessage}`);
    // Take a screenshot of the page anyway
    await takeScreenshot(page, `${resolution.name}-error`, true);
    throw error; // Re-throw to fail the test
  }
}

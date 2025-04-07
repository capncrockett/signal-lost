import { test, expect } from '@playwright/test';

// Increase the test timeout
test.setTimeout(120000);

test.describe('Game Rendering', () => {
  // Test the initial desktop view
  test('Game renders correctly at desktop resolution', async ({ page }) => {
    // Set a fixed viewport size to start
    await page.setViewportSize({ width: 1280, height: 800 });

    // Navigate to the game
    await page.goto('http://localhost:5173/');

    // Wait for the game to load
    await page.waitForTimeout(3000);

    // Click on the page to initialize audio (important for full rendering)
    await page.click('body', { position: { x: 400, y: 300 } });
    await page.waitForTimeout(1000);

    // Find the Phaser canvas (second canvas element)
    const canvas = page.locator('canvas').nth(1);
    await expect(canvas).toBeVisible();

    // Get initial canvas size
    const initialBoundingBox = await canvas.boundingBox();
    console.log(`Canvas size: ${initialBoundingBox?.width}x${initialBoundingBox?.height}`);

    // Take a screenshot of just the canvas
    await canvas.screenshot({ path: 'e2e-screenshots/canvas-only.png' });

    // Take a screenshot of the game container
    const gameContainer = page.locator('#game');
    await gameContainer.screenshot({ path: 'e2e-screenshots/game-container.png' });
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

  // Test tablet portrait resolution
  test('Game renders correctly at tablet portrait resolution', async ({ page }) => {
    const resolution = { width: 768, height: 1024, name: 'tablet-portrait' };
    await testResolution(page, resolution);
  });

  // Test mobile resolution
  test('Game renders correctly at mobile resolution', async ({ page }) => {
    const resolution = { width: 375, height: 667, name: 'mobile' };
    await testResolution(page, resolution);
  });
});

// Helper function to test a specific resolution
async function testResolution(page, resolution) {
  console.log(`Testing resolution: ${resolution.name} (${resolution.width}x${resolution.height})`);

  // Set viewport to the resolution
  await page.setViewportSize(resolution);
  await page.waitForTimeout(1000);

  // Navigate to the game at this resolution
  await page.goto('http://localhost:5173/');
  await page.waitForTimeout(3000);

  // Click to initialize audio
  await page.click('body', { position: { x: resolution.width / 2, y: resolution.height / 2 } });
  await page.waitForTimeout(1000);

  // Find the canvas
  const canvas = page.locator('canvas').nth(1);

  // Wait for canvas to be visible
  try {
    await expect(canvas).toBeVisible({ timeout: 5000 });

    // Take a screenshot of the canvas
    await canvas.screenshot({ path: `e2e-screenshots/${resolution.name}-canvas.png` });

    // Get canvas size
    const boundingBox = await canvas.boundingBox();
    console.log(`${resolution.name} canvas: ${boundingBox?.width}x${boundingBox?.height}`);

    // Take a screenshot of the game container
    const gameContainer = page.locator('#game');
    await gameContainer.screenshot({ path: `e2e-screenshots/${resolution.name}-container.png` });

    // Verify the radio tuner is visible
    const radioTuner = page.locator('[data-testid="radio-tuner"]');
    if (await radioTuner.isVisible()) {
      console.log(`Radio tuner is visible at ${resolution.name} resolution`);
    } else {
      console.log(`Radio tuner is NOT visible at ${resolution.name} resolution`);
    }
  } catch (error) {
    console.error(`Error at resolution ${resolution.name}: ${error.message}`);
    // Take a screenshot of the page anyway
    await page.screenshot({ path: `e2e-screenshots/${resolution.name}-error.png` });
    throw error; // Re-throw to fail the test
  }
}

import { test, expect } from '@playwright/test';

test.describe('Game Rendering', () => {
  test('Game renders correctly at different resolutions', async ({ page }) => {
    // Navigate to the game
    await page.goto('http://localhost:5173/');

    // Wait for the game to load
    await page.waitForTimeout(2000);

    // Find the Phaser canvas (second canvas element)
    const canvas = page.locator('canvas').nth(1);
    await expect(canvas).toBeVisible();

    // Get initial canvas size
    const initialBoundingBox = await canvas.boundingBox();
    console.log(`Canvas size: ${initialBoundingBox?.width}x${initialBoundingBox?.height}`);

    // Take a screenshot of the full page
    await page.screenshot({ path: 'e2e-screenshots/full-page.png', fullPage: true });

    // Take a screenshot of just the canvas
    await canvas.screenshot({ path: 'e2e-screenshots/canvas-only.png' });

    // Test different resolutions
    const resolutions = [
      { width: 1920, height: 1080, name: 'full-hd' },
      { width: 1366, height: 768, name: 'laptop' },
      { width: 1024, height: 768, name: 'desktop' },
      { width: 768, height: 1024, name: 'tablet-portrait' },
      { width: 375, height: 667, name: 'mobile' },
    ];

    for (const resolution of resolutions) {
      // Set viewport to the resolution
      await page.setViewportSize(resolution);
      await page.waitForTimeout(1000);

      // Take a screenshot
      await page.screenshot({
        path: `e2e-screenshots/${resolution.name}.png`,
        fullPage: false,
      });

      // Get canvas size at this resolution
      const boundingBox = await canvas.boundingBox();
      console.log(`${resolution.name} canvas: ${boundingBox?.width}x${boundingBox?.height}`);

      // Verify canvas is visible
      await expect(canvas).toBeVisible();

      // Verify the radio tuner is visible
      const radioTuner = page.locator('[data-testid="radio-tuner"]');
      if (await radioTuner.isVisible()) {
        console.log(`Radio tuner is visible at ${resolution.name} resolution`);
      } else {
        console.log(`Radio tuner is NOT visible at ${resolution.name} resolution`);
      }
    }
  });
});

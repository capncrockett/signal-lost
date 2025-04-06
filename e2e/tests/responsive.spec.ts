import { test, expect } from '@playwright/test';

test.describe('Responsive behavior', () => {
  test('Game scales correctly on desktop', async ({ page }) => {
    // Navigate to the game
    await page.goto('http://localhost:5173/');

    // Wait for the game to load
    await page.waitForTimeout(2000);

    // Find the Phaser canvas (second canvas element)
    const canvas = page.locator('canvas').nth(1);
    await expect(canvas).toBeVisible();

    // Get initial canvas size
    const initialBoundingBox = await canvas.boundingBox();
    console.log(`Initial canvas size: ${initialBoundingBox?.width}x${initialBoundingBox?.height}`);

    // Verify initial size is reasonable
    expect(initialBoundingBox?.width).toBeGreaterThan(400);
    expect(initialBoundingBox?.height).toBeGreaterThan(300);

    // Resize to a smaller desktop size
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.waitForTimeout(1000);

    // Get new canvas size
    const smallerBoundingBox = await canvas.boundingBox();
    console.log(
      `Smaller viewport canvas size: ${smallerBoundingBox?.width}x${smallerBoundingBox?.height}`
    );

    // Verify canvas size is appropriate for the viewport
    // With FIT mode, the canvas should maintain aspect ratio
    expect(smallerBoundingBox?.width).toBeLessThanOrEqual(800);
    expect(smallerBoundingBox?.height).toBeLessThanOrEqual(600);

    // Verify aspect ratio is maintained (approximately)
    const initialRatio = initialBoundingBox!.width / initialBoundingBox!.height;
    const newRatio = smallerBoundingBox!.width / smallerBoundingBox!.height;
    expect(Math.abs(initialRatio - newRatio)).toBeLessThan(0.1);

    // Take a screenshot for verification
    await page.screenshot({ path: 'e2e-desktop-resize.png' });
  });

  test('Game scales correctly on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 }); // iPhone SE size

    // Navigate to the game
    await page.goto('http://localhost:5173/');

    // Wait for the game to load
    await page.waitForTimeout(2000);

    // Find the Phaser canvas (second canvas element)
    const canvas = page.locator('canvas').nth(1);
    await expect(canvas).toBeVisible();

    // Get canvas size
    const mobileBoundingBox = await canvas.boundingBox();
    console.log(`Mobile canvas size: ${mobileBoundingBox?.width}x${mobileBoundingBox?.height}`);

    // Verify canvas fits within viewport (with a small margin of error)
    expect(mobileBoundingBox?.width).toBeLessThanOrEqual(376); // Allow 1px margin
    expect(mobileBoundingBox?.height).toBeLessThanOrEqual(668); // Allow 1px margin

    // Verify aspect ratio is maintained (approximately 4:3)
    const ratio = mobileBoundingBox!.width / mobileBoundingBox!.height;
    expect(Math.abs(ratio - 4 / 3)).toBeLessThan(0.1);

    // Take a screenshot for verification
    await page.screenshot({ path: 'e2e-mobile-size.png' });

    // Test landscape orientation
    await page.setViewportSize({ width: 667, height: 375 }); // iPhone SE landscape
    await page.waitForTimeout(1000);

    // Get new canvas size
    const landscapeBoundingBox = await canvas.boundingBox();
    console.log(
      `Landscape mobile canvas size: ${landscapeBoundingBox?.width}x${landscapeBoundingBox?.height}`
    );

    // Verify canvas fits within viewport
    expect(landscapeBoundingBox?.width).toBeLessThanOrEqual(667);
    expect(landscapeBoundingBox?.height).toBeLessThanOrEqual(375);

    // Take a screenshot for verification
    await page.screenshot({ path: 'e2e-mobile-landscape.png' });
  });

  test('UI elements remain accessible at different screen sizes', async ({ page }) => {
    // Navigate to the game with a medium-sized viewport
    await page.setViewportSize({ width: 800, height: 600 });
    await page.goto('http://localhost:5173/');
    await page.waitForTimeout(2000);

    // Check that the volume control is visible and clickable
    const volumeControl = page.locator('[data-testid="volume-control"]');
    await expect(volumeControl).toBeVisible();

    // Check that the radio tuner is visible
    const radioTuner = page.locator('[data-testid="radio-tuner"]');
    await expect(radioTuner).toBeVisible();

    // Check that the Go to Field button is visible and clickable
    const fieldButton = page.locator('[data-testid="go-to-field-button"]');
    await expect(fieldButton).toBeVisible();

    // Try a smaller viewport
    await page.setViewportSize({ width: 400, height: 600 });
    await page.waitForTimeout(1000);

    // Elements should still be visible and clickable
    await expect(volumeControl).toBeVisible();
    await expect(radioTuner).toBeVisible();
    await expect(fieldButton).toBeVisible();

    // Take a screenshot for verification
    await page.screenshot({ path: 'e2e-small-viewport-ui.png' });
  });
});

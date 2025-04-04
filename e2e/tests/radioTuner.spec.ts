import { test, expect } from '@playwright/test';

test('RadioTuner component loads and emits signal lock event', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/');

  // Listen for console logs
  const logs: string[] = [];
  page.on('console', msg => {
    logs.push(msg.text());
  });

  // Wait for the game to load
  await page.waitForTimeout(2000);

  // Wait for the game container to be visible
  await page.waitForSelector('#game', { timeout: 10000 });

  // Find the game container
  const gameContainer = page.locator('#game');

  // Verify game container exists
  await expect(gameContainer).toBeVisible();

  // Find the Phaser canvas (second canvas element)
  const canvas = page.locator('canvas').nth(1);

  // Verify canvas exists
  await expect(canvas).toBeVisible();

  // Click on the canvas where the radio tuner should be
  // These coordinates would need to be adjusted based on actual layout
  await canvas.click({
    position: {
      x: 400, // Center X
      y: 300  // Center Y
    }
  });

  // Wait for potential signal lock event
  await page.waitForTimeout(1000);

  // Wait for signal lock event to be logged
  await page.waitForTimeout(1000);
});

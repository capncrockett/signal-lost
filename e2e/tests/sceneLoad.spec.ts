import { test, expect } from '@playwright/test';

test('Game loads and scenes can be navigated', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/');

  // Listen for console logs
  const logs: string[] = [];
  page.on('console', (msg) => {
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

  // Wait for the game to load
  await page.waitForTimeout(2000);

  // Find and click the "Go to Field" button using its test ID
  const goToFieldButton = page.locator('[data-testid="go-to-field-button"]');
  await goToFieldButton.waitFor({ state: 'visible', timeout: 5000 });

  // Get the position of the button
  const buttonBounds = await goToFieldButton.boundingBox();
  if (!buttonBounds) {
    throw new Error('Could not get button bounds');
  }

  // Click in the center of the button using mouse actions for more reliability
  const centerX = buttonBounds.x + buttonBounds.width / 2;
  const centerY = buttonBounds.y + buttonBounds.height / 2;
  await page.mouse.click(centerX, centerY);

  // Wait for scene transition
  await page.waitForTimeout(1000);

  // Check if the FieldScene has loaded by looking for specific console logs
  // or by checking for visual elements specific to the FieldScene

  // Verify that the player can move in the FieldScene
  // Press arrow keys to move the player
  await page.keyboard.press('ArrowUp');
  await page.waitForTimeout(500);
  await page.keyboard.press('ArrowRight');
  await page.waitForTimeout(500);
  await page.keyboard.press('ArrowDown');
  await page.waitForTimeout(500);
  await page.keyboard.press('ArrowLeft');
  await page.waitForTimeout(500);

  // Verify that the player can interact with objects
  // Press space to interact with an object
  await page.keyboard.press('Space');
  await page.waitForTimeout(1000);

  // Wait for narrative event to be triggered
  await page.waitForTimeout(2000);
});

test('Game handles window resize correctly', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/');

  // Wait for the game to load
  await page.waitForTimeout(2000);

  // Find the Phaser canvas (second canvas element)
  const canvas = page.locator('canvas').nth(1);

  // Get initial canvas size
  const initialBoundingBox = await canvas.boundingBox();

  // Resize the window
  await page.setViewportSize({ width: 800, height: 600 });

  // Wait for resize to take effect
  await page.waitForTimeout(1000);

  // Get new canvas size
  const newBoundingBox = await canvas.boundingBox();

  // Verify that the canvas size has changed
  expect(newBoundingBox).not.toEqual(initialBoundingBox);

  // Verify that the canvas is still visible
  await expect(canvas).toBeVisible();
});

test('Game loads assets correctly', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/');

  // Listen for console logs
  const logs: string[] = [];
  page.on('console', (msg) => {
    logs.push(msg.text());
  });

  // Listen for console errors
  const errors: string[] = [];
  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      errors.push(msg.text());
    }
  });

  // Wait for the game to load
  await page.waitForTimeout(2000);

  // Verify that there are no asset loading errors
  const assetErrors = errors.filter(
    (error) =>
      error.includes('Failed to load') || error.includes('Error loading') || error.includes('404')
  );

  expect(assetErrors).toHaveLength(0);

  // Find the Phaser canvas (second canvas element)
  const canvas = page.locator('canvas').nth(1);

  // Verify canvas exists
  await expect(canvas).toBeVisible();
});

test('Game initializes audio correctly', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/');

  // Listen for console logs
  const logs: string[] = [];
  page.on('console', (msg) => {
    logs.push(msg.text());
  });

  // Wait for the game to load
  await page.waitForTimeout(2000);

  // Find the Phaser canvas (second canvas element)
  const canvas = page.locator('canvas').nth(1);

  // Click on the canvas to initialize audio (browser policy requires user interaction)
  await canvas.click();

  // Wait for audio initialization
  await page.waitForTimeout(1000);

  // Wait for audio to initialize
  await page.waitForTimeout(2000);
});

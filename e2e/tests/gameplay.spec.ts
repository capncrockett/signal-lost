import { test, expect } from '@playwright/test';

test('Complete gameplay flow from tuning radio to field exploration', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/');

  // Wait for the game to load
  await page.waitForTimeout(2000);

  // Take a screenshot of the initial state
  await page.screenshot({ path: 'e2e-initial-state.png', fullPage: true });

  // Find the Phaser canvas (second canvas element)
  const canvas = await page.locator('canvas').nth(1);

  // Ensure the canvas is visible and properly sized
  await expect(canvas).toBeVisible();
  const canvasBounds = await canvas.boundingBox();
  console.log(`Canvas dimensions: ${canvasBounds?.width}x${canvasBounds?.height}`);
  expect(canvasBounds?.width).toBeGreaterThan(400);
  expect(canvasBounds?.height).toBeGreaterThan(300);

  // Interact with the radio tuner using its test ID
  console.log('Looking for the radio tuner by test ID');
  const radioTuner = page.locator('[data-testid="radio-tuner"]');

  // Wait for the radio tuner to be visible
  await radioTuner.waitFor({ state: 'visible', timeout: 5000 });
  console.log('Found the radio tuner');

  // Get the position of the radio tuner
  const radioTunerBounds = await radioTuner.boundingBox();
  if (!radioTunerBounds) {
    throw new Error('Could not get radio tuner bounds');
  }

  // Calculate the position for the knob (center of the radio tuner)
  const knobX = radioTunerBounds.x + radioTunerBounds.width * 0.5;
  const knobY = radioTunerBounds.y + radioTunerBounds.height * 0.5;

  // Click and drag to tune
  await page.mouse.move(knobX, knobY);
  await page.mouse.down();

  // Move right to increase frequency
  await page.mouse.move(knobX + 100, knobY, { steps: 20 });
  await page.mouse.up();

  // Wait for signal lock
  await page.waitForTimeout(2000);

  // Take a screenshot after tuning
  await page.screenshot({ path: 'e2e-tuned-radio.png', fullPage: true });

  // Find and click the "Go to Field" button using its test ID
  console.log('Looking for the Go to Field button by test ID');
  const goToFieldButton = page.locator('[data-testid="go-to-field-button"]');

  // Wait for the button to be visible
  await goToFieldButton.waitFor({ state: 'visible', timeout: 5000 });
  console.log('Found the Go to Field button');

  // Get the position of the button
  const buttonBounds = await goToFieldButton.boundingBox();
  if (!buttonBounds) {
    throw new Error('Could not get button bounds');
  }

  // Click in the center of the button using mouse actions for more reliability
  const centerX = buttonBounds.x + buttonBounds.width / 2;
  const centerY = buttonBounds.y + buttonBounds.height / 2;
  await page.mouse.click(centerX, centerY);
  console.log('Clicked the Go to Field button');

  // Wait for field scene to load
  await page.waitForTimeout(2000);

  // Take a screenshot of the field scene
  await page.screenshot({ path: 'e2e-field-scene.png', fullPage: true });

  // Test movement in the field
  // Press arrow keys to move
  await page.keyboard.press('ArrowUp');
  await page.waitForTimeout(500);
  await page.keyboard.press('ArrowRight');
  await page.waitForTimeout(500);

  // Take a screenshot after movement
  await page.screenshot({ path: 'e2e-after-movement.png', fullPage: true });

  // Test interaction with an object
  await page.keyboard.press('Space');
  await page.waitForTimeout(1000);

  // Take a final screenshot
  await page.screenshot({ path: 'e2e-after-interaction.png', fullPage: true });
});

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

  // Interact with the radio tuner
  // Click and drag the knob to tune to frequency 91.5
  const knobX = canvasBounds!.x + canvasBounds!.width * 0.5; // Middle of canvas
  const knobY = canvasBounds!.y + canvasBounds!.height * 0.7; // Lower part where the knob is

  // Click on the knob
  await page.mouse.move(knobX, knobY);
  await page.mouse.down();

  // Move to tune to approximately 91.5 MHz
  // Move right to increase frequency
  await page.mouse.move(knobX + 100, knobY, { steps: 20 });
  await page.mouse.up();

  // Wait for signal lock
  await page.waitForTimeout(2000);

  // Take a screenshot after tuning
  await page.screenshot({ path: 'e2e-tuned-radio.png', fullPage: true });

  // Find and click the "Go to Field" button (DOM button)
  const goToFieldButton = await page.locator('button:has-text("Go to Field")');

  // Log if the button is found
  const isButtonVisible = await goToFieldButton.isVisible();
  console.log(`Go to Field button visible: ${isButtonVisible}`);

  if (isButtonVisible) {
    // Click the button
    await goToFieldButton.click();
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
  } else {
    // If button is not found, log the page content for debugging
    const pageContent = await page.content();
    console.log('Page content:', pageContent.substring(0, 500) + '...');

    // Check for any DOM elements with text containing "field"
    const fieldElements = await page.locator('text=field', { exact: false }).all();
    console.log(`Found ${fieldElements.length} elements containing "field" text`);

    // Log all buttons on the page
    const buttons = await page.locator('button').all();
    console.log(`Found ${buttons.length} buttons on the page`);
    for (let i = 0; i < buttons.length; i++) {
      const buttonText = await buttons[i].textContent();
      console.log(`Button ${i + 1}: ${buttonText}`);
    }

    // Fail the test
    expect(isButtonVisible).toBe(
      true,
      'Go to Field button should be visible after tuning the radio'
    );
  }
});

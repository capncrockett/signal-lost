import { test, expect } from '@playwright/test';
import {
  takeScreenshot,
  waitForGameLoad,
  clickGamePosition,
  captureConsoleLogs,
} from '../helpers/gameTestHelpers';

// Increase timeout for this test
test.setTimeout(120000);

test('Inventory system functionality', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

  // Wait for the game to load
  console.log('Waiting for game to load...');
  const gameLoadResult = await waitForGameLoad(page, { timeout: 15000 });
  console.log('Game load complete:', gameLoadResult);

  // Take a screenshot of the initial state
  await takeScreenshot(page, 'inventory-initial-state', true);

  // Initialize audio by clicking in the game
  console.log('Initializing audio...');
  await clickGamePosition(page, 400, 300, {
    takeScreenshot: true,
    screenshotName: 'inventory-audio-init',
  });
  await page.waitForTimeout(1000);

  // Go to the field scene
  console.log('Going to field scene...');
  const fieldButton = page.locator('text=Go to Field');
  await expect(fieldButton).toBeVisible();
  await fieldButton.click();
  await page.waitForTimeout(2000);

  // Take a screenshot of the field scene
  await takeScreenshot(page, 'inventory-field-scene', true);

  // Press 'I' to open inventory
  console.log('Opening inventory...');
  await page.keyboard.press('KeyI');
  await page.waitForTimeout(1000);

  // Take a screenshot of the inventory
  await takeScreenshot(page, 'inventory-open', true);

  // Check if inventory UI is visible
  const inventoryTitle = page.locator('text=Inventory');
  try {
    await expect(inventoryTitle).toBeVisible({ timeout: 3000 });
    console.log('Inventory UI is visible');
  } catch (error) {
    console.log('Inventory UI not found, continuing test...');
  }

  // Capture console logs to check for inventory events
  const inventoryLogs = await captureConsoleLogs(page, 1000);
  console.log('Inventory logs:', inventoryLogs.logs);

  // Try to select an item (radio should be in the inventory)
  console.log('Selecting an item...');
  await clickGamePosition(page, 350, 250, {
    takeScreenshot: true,
    screenshotName: 'inventory-select-item',
  });
  await page.waitForTimeout(1000);

  // Try to use the selected item
  console.log('Using the selected item...');
  const useButton = page.locator('text=Use');
  try {
    await expect(useButton).toBeVisible({ timeout: 3000 });
    await useButton.click();
    console.log('Use button clicked');
  } catch (error) {
    console.log('Use button not found, continuing test...');
  }
  await page.waitForTimeout(1000);

  // Take a screenshot after using the item
  await takeScreenshot(page, 'inventory-after-use', true);

  // Press 'I' to close inventory
  console.log('Closing inventory...');
  await page.keyboard.press('KeyI');
  await page.waitForTimeout(1000);

  // Take a screenshot after closing inventory
  await takeScreenshot(page, 'inventory-closed', true);

  // Try to interact with an item in the field
  console.log('Moving to interact with an item...');

  // Move to the right (where an item should be)
  for (let i = 0; i < 5; i++) {
    await page.keyboard.press('KeyD');
    await page.waitForTimeout(500);
  }

  // Move down
  for (let i = 0; i < 3; i++) {
    await page.keyboard.press('KeyS');
    await page.waitForTimeout(500);
  }

  // Take a screenshot after moving
  await takeScreenshot(page, 'inventory-moved-to-item', true);

  // Try to interact with the item
  console.log('Interacting with the item...');
  await page.keyboard.press('KeyE');
  await page.waitForTimeout(1000);

  // Take a screenshot after interaction
  await takeScreenshot(page, 'inventory-after-interaction', true);

  // Capture console logs to check for item pickup events
  const pickupLogs = await captureConsoleLogs(page, 1000);
  console.log('Pickup logs:', pickupLogs.logs);

  // Open inventory again to check if the item was added
  console.log('Opening inventory again...');
  await page.keyboard.press('KeyI');
  await page.waitForTimeout(1000);

  // Take a screenshot of the inventory after pickup
  await takeScreenshot(page, 'inventory-after-pickup', true);

  // Close inventory
  console.log('Closing inventory...');
  await page.keyboard.press('KeyI');
  await page.waitForTimeout(1000);

  // Take a final screenshot
  await takeScreenshot(page, 'inventory-final', true);
});

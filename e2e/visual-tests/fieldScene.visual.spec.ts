import { test, expect } from '@playwright/test';
import { waitForGameLoad, clickGamePosition } from '../helpers/gameTestHelpers';
import {
  expectComponentToMatchSnapshot,
  expectGameToMatchSnapshot,
  waitForGameToStabilize,
  waitForAnimationsToComplete,
} from '../helpers/visualTestHelpers';

// Increase timeout for visual tests
test.setTimeout(120000);

test.describe('Field Scene Visual Tests', () => {
  test('Field scene renders correctly', async ({ page }) => {
    // Navigate to the game
    await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

    // Wait for the game to load
    console.log('Waiting for game to load...');
    const gameLoadResult = await waitForGameLoad(page, { timeout: 15000 });
    console.log('Game load complete:', gameLoadResult);

    // Initialize audio by clicking in the game
    console.log('Initializing audio...');
    await clickGamePosition(page, 400, 300);
    await page.waitForTimeout(2000);

    // Find and click the Go to Field button
    console.log('Going to field scene...');
    const fieldButton = page.locator('text=Go to Field');
    await expect(fieldButton).toBeVisible();
    await fieldButton.click();
    await page.waitForTimeout(2000);

    // Wait for game to stabilize
    await waitForGameToStabilize(page);

    // Take a snapshot of the field scene
    await expectGameToMatchSnapshot(page, 'field-scene-initial');

    // Test player character
    const player = page.locator('[data-testid="player"]');
    if (await player.isVisible()) {
      await expectComponentToMatchSnapshot(player, 'player-character');
    }

    // Test movement
    console.log('Testing player movement...');
    await page.keyboard.press('KeyD');
    await page.waitForTimeout(500);
    await page.keyboard.press('KeyD');
    await page.waitForTimeout(500);
    await waitForGameToStabilize(page);
    await expectGameToMatchSnapshot(page, 'field-scene-moved-right');

    await page.keyboard.press('KeyS');
    await page.waitForTimeout(500);
    await page.keyboard.press('KeyS');
    await page.waitForTimeout(500);
    await waitForGameToStabilize(page);
    await expectGameToMatchSnapshot(page, 'field-scene-moved-down');

    // Test inventory
    console.log('Opening inventory...');
    await page.keyboard.press('KeyI');
    await page.waitForTimeout(1000);
    await waitForAnimationsToComplete(page);
    await waitForGameToStabilize(page);
    await expectGameToMatchSnapshot(page, 'inventory-open');

    // Test inventory components
    const inventoryTitle = page.locator('text=Inventory');
    if (await inventoryTitle.isVisible()) {
      await expectComponentToMatchSnapshot(inventoryTitle, 'inventory-title');
    }

    // Close inventory
    console.log('Closing inventory...');
    await page.keyboard.press('KeyI');
    await page.waitForTimeout(1000);
    await waitForAnimationsToComplete(page);
    await waitForGameToStabilize(page);

    // Test interaction with an object
    console.log('Testing interaction...');
    // Move to an interactable object
    for (let i = 0; i < 5; i++) {
      await page.keyboard.press('KeyD');
      await page.waitForTimeout(300);
    }
    for (let i = 0; i < 3; i++) {
      await page.keyboard.press('KeyS');
      await page.waitForTimeout(300);
    }
    await waitForGameToStabilize(page);
    await expectGameToMatchSnapshot(page, 'field-scene-near-interactable');

    // Interact with the object
    await page.keyboard.press('KeyE');
    await page.waitForTimeout(1000);
    await waitForAnimationsToComplete(page);
    await waitForGameToStabilize(page);
    await expectGameToMatchSnapshot(page, 'field-scene-interaction');

    // Test narrative dialog if visible
    const narrativeDialog = page.locator('[data-testid="narrative-dialog"]');
    if (await narrativeDialog.isVisible()) {
      await expectComponentToMatchSnapshot(narrativeDialog, 'narrative-dialog');
    }

    // Take a final snapshot
    await expectGameToMatchSnapshot(page, 'field-scene-final');
  });
});

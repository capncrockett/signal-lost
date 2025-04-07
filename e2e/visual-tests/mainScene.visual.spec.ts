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

test.describe('Main Scene Visual Tests', () => {
  test('Radio tuner renders correctly', async ({ page }) => {
    // Navigate to the game
    await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

    // Wait for the game to load
    console.log('Waiting for game to load...');
    const gameLoadResult = await waitForGameLoad(page, { timeout: 15000 });
    console.log('Game load complete:', gameLoadResult);

    // Initialize audio by clicking in the game
    console.log('Initializing audio...');
    await clickGamePosition(page, 400, 300, {
      takeScreenshot: true,
      screenshotName: 'radio-tuner-init',
    });
    await page.waitForTimeout(2000);

    // Wait for game to stabilize
    await waitForGameToStabilize(page);

    // Take a snapshot of the entire game
    await expectGameToMatchSnapshot(page, 'radio-tuner-initial');

    // Find and test the radio tuner component
    const radioTuner = page.locator('[data-testid="radio-tuner"]');
    if (await radioTuner.isVisible()) {
      await expectComponentToMatchSnapshot(radioTuner, 'radio-tuner-component');
    } else {
      // If no data-testid, use the canvas
      console.log('Radio tuner not found by data-testid, using canvas');
      await expectGameToMatchSnapshot(page, 'radio-tuner-canvas');
    }

    // Test frequency display
    const frequencyDisplay = page.locator('[data-testid="frequency-display"]');
    if (await frequencyDisplay.isVisible()) {
      await expectComponentToMatchSnapshot(frequencyDisplay, 'frequency-display');
    }

    // Test signal strength indicator
    const signalStrength = page.locator('[data-testid="signal-strength"]');
    if (await signalStrength.isVisible()) {
      await expectComponentToMatchSnapshot(signalStrength, 'signal-strength');
    }

    // Test tuning to a specific frequency
    console.log('Testing frequency tuning...');
    await clickGamePosition(page, 350, 300);
    await page.waitForTimeout(1000);
    await waitForGameToStabilize(page);
    await expectGameToMatchSnapshot(page, 'radio-tuner-frequency-91.5');

    // Test UI buttons
    const goToFieldButton = page.locator('text=Go to Field');
    if (await goToFieldButton.isVisible()) {
      await expectComponentToMatchSnapshot(goToFieldButton, 'go-to-field-button');
    }

    // Test volume control
    const volumeControl = page.locator('[data-testid="volume-control"]');
    if (await volumeControl.isVisible()) {
      await expectComponentToMatchSnapshot(volumeControl, 'volume-control');
    }
  });

  test('Save/Load menu renders correctly', async ({ page }) => {
    // Navigate to the game
    await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

    // Wait for the game to load
    console.log('Waiting for game to load...');
    await waitForGameLoad(page, { timeout: 15000 });
    await page.waitForTimeout(2000);

    // Initialize audio by clicking in the game
    console.log('Initializing audio...');
    await clickGamePosition(page, 400, 300);
    await page.waitForTimeout(2000);

    // Find and click the Save/Load button
    console.log('Opening save/load menu...');
    const saveLoadButton = page.locator('text=Save/Load');

    try {
      await expect(saveLoadButton).toBeVisible({ timeout: 5000 });
      await saveLoadButton.click();
    } catch (error) {
      console.log('Save/Load button not found, trying to click at its position...');
      await clickGamePosition(page, 700, 120);
    }

    await page.waitForTimeout(1000);
    await waitForAnimationsToComplete(page);
    await waitForGameToStabilize(page);

    // Take a snapshot of the save/load menu
    await expectGameToMatchSnapshot(page, 'save-load-menu');

    // Test individual components of the save/load menu
    const saveGameButton = page.locator('text=Save Game');
    if (await saveGameButton.isVisible()) {
      await expectComponentToMatchSnapshot(saveGameButton, 'save-game-button');
    }

    const loadGameButton = page.locator('text=Load Game');
    if (await loadGameButton.isVisible()) {
      await expectComponentToMatchSnapshot(loadGameButton, 'load-game-button');
    }

    const closeButton = page.locator('text=X').first();
    if (await closeButton.isVisible()) {
      await expectComponentToMatchSnapshot(closeButton, 'close-button');
    }

    // Close the save/load menu
    if (await closeButton.isVisible()) {
      await closeButton.click();
    } else {
      await clickGamePosition(page, 680, 130);
    }

    await page.waitForTimeout(1000);
    await waitForAnimationsToComplete(page);
    await waitForGameToStabilize(page);

    // Take a final snapshot
    await expectGameToMatchSnapshot(page, 'main-scene-final');
  });
});

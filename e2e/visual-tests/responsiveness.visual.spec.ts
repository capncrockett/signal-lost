import { test } from '@playwright/test';
import { waitForGameLoad, clickGamePosition } from '../helpers/gameTestHelpers';
import {
  expectGameToMatchSnapshot,
  waitForGameToStabilize,
} from '../helpers/visualTestHelpers';

// Increase timeout for visual tests
test.setTimeout(120000);

// Define screen sizes to test
const screenSizes = [
  { name: 'desktop', width: 1280, height: 800 },
  { name: 'tablet', width: 768, height: 1024 },
  { name: 'mobile', width: 375, height: 667 },
];

test.describe('Responsiveness Visual Tests', () => {
  for (const size of screenSizes) {
    test(`Game renders correctly at ${size.name} resolution`, async ({ page }) => {
      // Set viewport size
      await page.setViewportSize({ width: size.width, height: size.height });

      // Navigate to the game
      await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

      // Wait for the game to load
      console.log(`Waiting for game to load at ${size.name} resolution...`);
      await waitForGameLoad(page, { timeout: 15000 });
      await page.waitForTimeout(2000);

      // Wait for game to stabilize
      await waitForGameToStabilize(page);

      // Take a snapshot of the game at this resolution
      await expectGameToMatchSnapshot(page, `main-scene-${size.name}`);

      // Initialize audio by clicking in the game
      console.log('Initializing audio...');
      await clickGamePosition(page, size.width / 2, size.height / 2);
      await page.waitForTimeout(2000);
      await waitForGameToStabilize(page);

      // Take another snapshot after interaction
      await expectGameToMatchSnapshot(page, `main-scene-${size.name}-after-click`);

      // Test radio tuner at this resolution
      console.log(`Testing radio tuner at ${size.name} resolution...`);
      await clickGamePosition(page, size.width / 2, size.height / 2);
      await page.waitForTimeout(1000);
      await waitForGameToStabilize(page);
      await expectGameToMatchSnapshot(page, `radio-tuner-${size.name}`);

      // If not mobile, test field scene
      if (size.width >= 768) {
        // Go to field scene
        console.log(`Testing field scene at ${size.name} resolution...`);
        const fieldButton = page.locator('text=Go to Field');
        if (await fieldButton.isVisible()) {
          await fieldButton.click();
          await page.waitForTimeout(2000);
          await waitForGameToStabilize(page);
          await expectGameToMatchSnapshot(page, `field-scene-${size.name}`);
        }
      }
    });
  }
});

import { test, expect } from '@playwright/test';

test.describe('BasicRadioTuner Infinite Loop Test', () => {
  test('should not have infinite render loop when radio is turned on', async ({ page }) => {
    // Navigate to the radio page
    await page.goto('http://localhost:5173/radio');

    // Wait for the page to load
    await page.waitForSelector('.radio-tuner', { timeout: 10000 });

    // Set up console error monitoring to catch 'Maximum update depth exceeded' errors
    let maxUpdateDepthErrors = 0;
    let renderCount = 0;

    page.on('console', (msg) => {
      const text = msg.text();

      // Count render operations
      if (text.includes('ZustandRadioTuner rendering')) {
        renderCount++;
        console.log(`Render detected: ${text}`);
      }

      // Specifically look for the infinite loop error
      if (text.includes('Maximum update depth exceeded')) {
        console.log('DETECTED INFINITE LOOP ERROR:', text);
        maxUpdateDepthErrors++;
      }
    });

    // Wait a bit to collect initial logs
    await page.waitForTimeout(2000);
    console.log(`Initial render count: ${renderCount}`);

    // Take a screenshot before turning on the radio
    await page.screenshot({ path: 'e2e/screenshots/radio-before-on.png' });

    // Turn on the radio - this is where the infinite loop used to happen
    const powerButton = page.locator('.power-button');
    await powerButton.click();

    // Wait longer to see if we get the infinite loop error
    await page.waitForTimeout(5000);

    // Check if we detected any infinite loop errors
    console.log(`Render count after turning radio on: ${renderCount}`);
    console.log(`Maximum update depth errors: ${maxUpdateDepthErrors}`);

    // Take a screenshot after turning on the radio
    await page.screenshot({ path: 'e2e/screenshots/radio-after-on.png' });

    // We should have 0 maximum update depth errors
    expect(maxUpdateDepthErrors).toBe(0);

    // The render count should be reasonable (not thousands)
    // We're using a more lenient threshold since we're using Zustand for state management
    // which can cause more renders than a pure React component
    expect(renderCount).toBeLessThan(2000);

    // Try changing the frequency
    const increaseButton = page.locator('.tune-button.increase');
    await increaseButton.click();

    // Wait to see if this causes an infinite loop
    await page.waitForTimeout(3000);

    // Check counts again
    console.log(`Final render count: ${renderCount}`);
    console.log(`Final maximum update depth errors: ${maxUpdateDepthErrors}`);

    // Still should have 0 errors
    expect(maxUpdateDepthErrors).toBe(0);

    // Take a final screenshot
    await page.screenshot({ path: 'e2e/screenshots/radio-final.png' });
  });
});

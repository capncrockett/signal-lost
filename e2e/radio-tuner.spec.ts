import { test, expect } from '@playwright/test';

test.describe('BasicRadioTuner Component', () => {
  test('should render without infinite loops', async ({ page }) => {
    // Navigate to the radio page
    await page.goto('http://localhost:5173/radio');

    // Wait for the page to load
    await page.waitForSelector('[data-testid="radio-tuner"]');

    // Set up console log monitoring
    const logs: string[] = [];
    page.on('console', (msg) => {
      if (msg.text().includes('rendering')) {
        logs.push(msg.text());
      }
    });

    // Wait a bit to collect logs
    await page.waitForTimeout(2000);

    // Check if we have too many render logs (indicating an infinite loop)
    console.log(`Collected ${logs.length} render logs in 2 seconds`);
    expect(logs.length).toBeLessThan(20); // We shouldn't have more than 20 renders in 2 seconds

    // Turn on the radio
    const powerButton = page.locator('[data-testid="power-button"]');
    await powerButton.click();

    // Wait a bit more to collect logs after interaction
    await page.waitForTimeout(2000);

    // Check logs again
    console.log(`Collected ${logs.length} render logs after turning on radio`);

    // Try to use the slider
    const slider = page.locator('[data-testid="frequency-slider"]');
    await slider.click();

    // Wait to see if this causes more renders
    await page.waitForTimeout(2000);

    // Check logs again
    console.log(`Collected ${logs.length} render logs after clicking slider`);

    // Try the buttons
    const increaseButton = page.locator('[data-testid="tune-up-button"]');
    await increaseButton.click();

    // Wait to see if this causes more renders
    await page.waitForTimeout(2000);

    // Check logs again
    console.log(`Collected ${logs.length} render logs after clicking increase button`);

    // Take a screenshot for visual verification
    await page.screenshot({ path: 'e2e/screenshots/radio-tuner.png' });
  });

  test('basic radio tuner functionality', async ({ page }) => {
    // Navigate to the radio page
    await page.goto('http://localhost:5173/radio');

    // Verify initial state
    await expect(page.locator('[data-testid="frequency-display"]')).toContainText('90.0 MHz');
    await expect(page.locator('[data-testid="power-button"]')).toHaveText('OFF');

    // Take screenshot of initial state
    await page.screenshot({ path: 'e2e/screenshots/radio-initial.png' });

    // Turn on the radio
    await page.locator('[data-testid="power-button"]').click();

    // Verify radio is on
    await expect(page.locator('[data-testid="power-button"]')).toHaveText('ON');

    // Take screenshot after turning on
    await page.screenshot({ path: 'e2e/screenshots/radio-on.png' });

    // Tune the radio using buttons
    await page.locator('[data-testid="tune-up-button"]').click();

    // Verify frequency changed
    await expect(page.locator('[data-testid="frequency-display"]')).toContainText('90.1 MHz');

    // Take screenshot after tuning
    await page.screenshot({ path: 'e2e/screenshots/radio-tuned.png' });

    // Use the frequency slider
    const slider = page.locator('[data-testid="frequency-slider"]');
    await slider.click();

    // Take screenshot after using slider
    await page.screenshot({ path: 'e2e/screenshots/radio-slider.png' });

    // Turn off the radio
    await page.locator('[data-testid="power-button"]').click();

    // Verify radio is off
    await expect(page.locator('[data-testid="power-button"]')).toHaveText('OFF');

    // Take final screenshot
    await page.screenshot({ path: 'e2e/screenshots/radio-off.png' });
  });

  test('canvas visualization', async ({ page }) => {
    // Navigate to the radio page
    await page.goto('http://localhost:5173/radio');

    // Turn on the radio
    await page.locator('[data-testid="power-button"]').click();

    // Verify canvas element exists
    const canvas = page.locator('[data-testid="static-canvas"]');
    await expect(canvas).toBeVisible();

    // Take screenshot to verify canvas rendering
    await page.screenshot({ path: 'e2e/screenshots/radio-canvas.png' });

    // Tune to different frequencies and take screenshots to verify canvas changes
    // We'll use the tune buttons to change frequency
    for (let i = 0; i < 5; i++) {
      // Click the tune up button
      await page.locator('[data-testid="tune-up-button"]').click();

      // Wait for canvas to update
      await page.waitForTimeout(200);

      // Take screenshot at this frequency
      await page.screenshot({ path: `e2e/screenshots/radio-canvas-${i}.png` });
    }
  });
});

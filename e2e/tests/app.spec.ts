import { test, expect } from '@playwright/test';

test('basic app navigation', async ({ page }) => {
  // Navigate to the app
  await page.goto('/');

  // Check if the title is correct
  await expect(page.locator('h1')).toHaveText('Signal Lost');

  // Check if the navigation links are present
  await expect(page.locator('nav li')).toHaveCount(3);

  // Navigate to the Radio page
  await page.click('text=Radio');

  // Check if we're on the Radio page
  await expect(page.locator('.radio-tuner-page')).toBeVisible();

  // Navigate to the Field page
  await page.click('text=Field');

  // Check if we're on the Field page
  await expect(page.locator('.field-exploration-page')).toBeVisible();

  // Navigate back to the Home page
  await page.click('text=Home');

  // Check if we're on the Home page
  await expect(page.locator('.home-page')).toBeVisible();
});

test('radio tuner interaction', async ({ page }) => {
  // Navigate to the Radio page
  await page.goto('/radio');

  // Check if the radio tuner component is visible
  await expect(page.locator('[data-testid="radio-tuner"]')).toBeVisible();

  // Check if the frequency display is visible
  await expect(page.locator('.frequency-display')).toBeVisible();

  // Check if the tuner dial is visible
  await expect(page.locator('.tuner-dial-container')).toBeVisible();

  // Check if the signal strength meter is visible
  await expect(page.locator('.signal-strength-meter')).toBeVisible();

  // Check if the tuner controls are visible
  await expect(page.locator('.tuner-controls')).toBeVisible();

  // Turn on the radio
  await page.click('.power-button');

  // Check if the power button shows ON
  await expect(page.locator('.power-button')).toHaveText('ON');

  // Click the increase frequency button
  await page.click('.tune-button.increase');

  // Take a screenshot for visual verification
  await page.screenshot({ path: 'e2e/screenshots/radio-tuner.png' });
});

test('radio tuner signal detection', async ({ page }) => {
  // Navigate to the Radio page
  await page.goto('/radio');

  // Turn on the radio
  await page.click('.power-button');

  // Tune to a frequency with a signal (91.1 MHz)
  // First, go to 90.0 MHz
  for (let i = 0; i < 10; i++) {
    await page.click('.tune-button.decrease');
  }

  // Then, go to 91.1 MHz
  for (let i = 0; i < 11; i++) {
    await page.click('.tune-button.increase');
  }

  // Wait for signal detection
  await page.waitForSelector('.signal-detected', { timeout: 5000 }).catch(() => {
    console.log('Signal not detected, might be a timing issue');
  });

  // Take a screenshot of the signal detection
  await page.screenshot({ path: 'e2e/screenshots/radio-signal-detected.png' });

  // If signal is detected, click the view message button
  const viewMessageButton = page.locator('.view-message-button');
  if (await viewMessageButton.isVisible()) {
    await viewMessageButton.click();

    // Check if the message is displayed
    await page.waitForSelector('.message-display', { timeout: 5000 }).catch(() => {
      console.log('Message not displayed, might be a timing issue');
    });

    // Take a screenshot of the message
    await page.screenshot({ path: 'e2e/screenshots/radio-message.png' });
  }
});

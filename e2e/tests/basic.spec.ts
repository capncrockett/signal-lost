import { test, expect } from '@playwright/test';

test('Basic page load test', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/');
  
  // Wait for the page to load
  await page.waitForTimeout(2000);
  
  // Check if the page title is correct
  const title = await page.title();
  expect(title).toBe('Signal Lost');
  
  // Check if the game container exists
  const gameContainer = await page.locator('#game');
  await expect(gameContainer).toBeVisible();
  
  // Check if the fallback canvas exists
  const fallbackCanvas = await page.locator('#fallback-canvas');
  await expect(fallbackCanvas).toBeVisible();
  
  // Check for console logs
  const logs: string[] = [];
  page.on('console', msg => {
    logs.push(msg.text());
  });
  
  // Wait for potential logs
  await page.waitForTimeout(2000);
  
  // Log the collected console messages for debugging
  console.log('Console logs:', logs);
  
  // Take a screenshot for debugging
  await page.screenshot({ path: 'e2e-screenshot.png' });
});

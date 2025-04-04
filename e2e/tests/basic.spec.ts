import { test, expect } from '@playwright/test';

test('Basic page load test with detailed diagnostics', async ({ page }) => {
  // Set up console log collection before navigation
  const logs: string[] = [];
  const errors: string[] = [];
  const warnings: string[] = [];

  page.on('console', msg => {
    const text = msg.text();
    logs.push(text);
    console.log(`Browser console [${msg.type()}]: ${text}`);

    // Categorize by type
    if (msg.type() === 'error') errors.push(text);
    if (msg.type() === 'warning') warnings.push(text);
  });

  // Set up error collection
  page.on('pageerror', error => {
    console.error(`Browser page error: ${error.message}`);
    errors.push(error.message);
  });

  // Set up request/response monitoring
  page.on('request', request => {
    console.log(`Request: ${request.method()} ${request.url()}`);
  });

  page.on('response', response => {
    console.log(`Response: ${response.status()} ${response.url()}`);
  });

  // Navigate to the game
  console.log('Navigating to the game...');
  await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });
  console.log('Navigation complete');

  // Wait for the page to load
  console.log('Waiting for page to load...');
  await page.waitForTimeout(2000);
  console.log('Wait complete');

  // Check if the page title is correct
  const title = await page.title();
  console.log(`Page title: ${title}`);
  expect(title).toBe('Signal Lost');

  // Check HTML structure
  console.log('Checking HTML structure...');
  const html = await page.content();
  console.log(`HTML length: ${html.length} characters`);

  // Check if the game container exists
  console.log('Checking game container...');
  const gameContainer = await page.locator('#game');
  const gameContainerBox = await gameContainer.boundingBox();
  console.log(`Game container bounding box:`, gameContainerBox);
  await expect(gameContainer).toBeVisible();

  // Check if the fallback canvas exists
  console.log('Checking fallback canvas...');
  const fallbackCanvas = await page.locator('#fallback-canvas');
  const fallbackCanvasBox = await fallbackCanvas.boundingBox();
  console.log(`Fallback canvas bounding box:`, fallbackCanvasBox);
  await expect(fallbackCanvas).toBeVisible();

  // Check for any Phaser canvas
  console.log('Checking for Phaser canvas...');
  const canvasCount = await page.locator('canvas').count();
  console.log(`Canvas count: ${canvasCount}`);

  // Wait longer for Phaser to initialize
  console.log('Waiting for Phaser to initialize...');
  await page.waitForTimeout(5000);
  console.log('Wait complete');

  // Check again for any Phaser canvas
  const newCanvasCount = await page.locator('canvas').count();
  console.log(`Canvas count after wait: ${newCanvasCount}`);

  // Log the collected console messages for debugging
  console.log('Console logs count:', logs.length);
  console.log('Console errors count:', errors.length);
  console.log('Console warnings count:', warnings.length);

  // If there are errors, log them in detail
  if (errors.length > 0) {
    console.error('Browser console errors:', errors);
  }

  // If there are warnings, log them in detail
  if (warnings.length > 0) {
    console.warn('Browser console warnings:', warnings);
  }

  // Take a screenshot for debugging
  console.log('Taking screenshot...');
  await page.screenshot({ path: 'e2e-screenshot.png', fullPage: true });
  console.log('Screenshot saved');

  // Evaluate JavaScript in the page context
  console.log('Evaluating JavaScript in page context...');
  const gameInfo = await page.evaluate(() => {
    return {
      gameElement: document.getElementById('game') ? true : false,
      canvasElements: document.querySelectorAll('canvas').length,
      windowInnerWidth: window.innerWidth,
      windowInnerHeight: window.innerHeight,
      documentReady: document.readyState
    };
  });
  console.log('Page JavaScript evaluation result:', gameInfo);
});

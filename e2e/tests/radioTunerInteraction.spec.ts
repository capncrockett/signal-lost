import { test, expect } from '@playwright/test';
import {
  waitForGameLoad,
  clickGamePosition,
  dragInGame,
  // captureConsoleLogs, // Not used in this test
  testRadioTuner,
  takeScreenshot,
} from '../helpers/gameTestHelpers';

test('Radio tuner interaction test', async ({ page }) => {
  // Set up console log collection
  const logs: string[] = [];
  const errors: string[] = [];
  const warnings: string[] = [];

  page.on('console', (msg) => {
    const text = msg.text();
    logs.push(text);
    console.log(`Browser console [${msg.type()}]: ${text}`);

    // Categorize by type
    if (msg.type() === 'error') errors.push(text);
    if (msg.type() === 'warning') warnings.push(text);
  });

  // Set up error collection
  page.on('pageerror', (error) => {
    console.error(`Browser page error: ${error.message}`);
    errors.push(error.message);
  });

  // Navigate to the game
  await page.goto('http://localhost:5173/');

  // Wait for the game to load
  const gameElements = await waitForGameLoad(page);
  console.log(`Game loaded with ${gameElements.canvasCount} canvas elements`);

  // Click to initialize audio
  await clickGamePosition(page, 400, 300);
  console.log('Clicked to initialize audio');

  // Wait for audio to initialize
  await page.waitForTimeout(1000);

  // Test dragging the radio tuner knob
  console.log('Testing radio tuner knob drag...');

  // Drag from center to right (increasing frequency)
  await dragInGame(page, 400, 300, 450, 300);
  console.log('Dragged knob to the right');

  // Wait for signal processing
  await page.waitForTimeout(1000);

  // Drag from right to left (decreasing frequency)
  await dragInGame(page, 450, 300, 350, 300);
  console.log('Dragged knob to the left');

  // Wait for signal processing
  await page.waitForTimeout(1000);

  // Try to find a signal by dragging to different positions
  for (let x = 300; x <= 500; x += 50) {
    await dragInGame(page, 400, 300, x, 300);
    console.log(`Dragged knob to position x=${x}`);
    await page.waitForTimeout(500);
  }

  // Wait for any signal lock events
  await page.waitForTimeout(2000);

  // Log the collected console messages for debugging
  console.log('Console logs count:', logs.length);
  console.log('Console errors count:', errors.length);
  console.log('Console warnings count:', warnings.length);

  // Check for signal lock events
  const signalEvents = logs.filter((log) => log.includes('Signal locked'));
  console.log('Signal lock events:', signalEvents.length);

  // Take a screenshot
  await takeScreenshot(page, 'radio-tuner-test');

  // Check for errors (ignoring audio loading errors)
  const nonAudioErrors = errors.filter(
    (error) =>
      !error.includes('Unable to decode audio data') &&
      !error.includes('Failed to process file') &&
      !error.includes('Failed to load resource')
  );

  // Verify no non-audio errors occurred
  expect(nonAudioErrors.length).toBe(0);
});

test('Comprehensive radio tuner test using helper', async ({ page }) => {
  // Set up console logging
  page.on('console', (msg) => {
    console.log(`Browser console [${msg.type()}]: ${msg.text()}`);
  });

  // Navigate to the game
  await page.goto('http://localhost:5173/');

  // Run the radio tuner test
  const results = await testRadioTuner(page);

  // Log the results
  console.log('Radio tuner test results:');
  console.log(`- Signal events detected: ${results.signalEvents.length}`);
  console.log(`- Audio initialized: ${results.audioInitialized}`);
  console.log(`- Total logs: ${results.logResults.logs.length}`);
  console.log(`- Errors: ${results.logResults.errors.length}`);

  // Take a screenshot
  await takeScreenshot(page, 'radio-tuner-comprehensive');

  // Verify audio was initialized
  expect(results.audioInitialized).toBe(true);

  // Check for non-audio errors
  const nonAudioErrors = results.logResults.errors.filter(
    (error) =>
      !error.includes('Unable to decode audio data') &&
      !error.includes('Failed to process file') &&
      !error.includes('Failed to load resource')
  );

  // Verify no non-audio errors occurred
  expect(nonAudioErrors.length).toBe(0);
});

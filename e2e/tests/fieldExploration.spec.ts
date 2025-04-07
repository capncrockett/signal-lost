import { test } from '@playwright/test';
import {
  takeScreenshot,
  waitForGameLoad,
  captureConsoleLogs,
  clickGamePosition,
} from '../helpers/gameTestHelpers';

// Increase timeout for this test
test.setTimeout(120000);

test('Field exploration and interaction with objects', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });

  // Wait for the game to load
  console.log('Waiting for game to load...');
  const gameLoadResult = await waitForGameLoad(page, { timeout: 15000 });
  console.log('Game load complete:', gameLoadResult);

  // Take a screenshot of the initial state
  await takeScreenshot(page, 'field-exploration-initial-state', true);

  // Initialize audio by clicking in the game
  console.log('Initializing audio...');
  await clickGamePosition(page, 400, 300, {
    takeScreenshot: true,
    screenshotName: 'field-exploration-audio-init',
  });
  await page.waitForTimeout(1000);

  // Look for "Go to Field" button or text
  console.log('Looking for Go to Field button...');

  // Try multiple approaches to find and click the button
  let buttonClicked = false;

  // Approach 1: Look for text="Go to Field"
  try {
    const goToFieldButton = page.locator('text="Go to Field"');
    const buttonExists = (await goToFieldButton.count()) > 0;
    console.log(`Go to Field button exists (approach 1): ${buttonExists}`);

    if (buttonExists) {
      await goToFieldButton.click();
      console.log('Clicked Go to Field button (approach 1)');
      buttonClicked = true;
    }
  } catch (error) {
    console.log('Error in approach 1:', error.message);
  }

  // Approach 2: Look for any element containing "field" text
  if (!buttonClicked) {
    try {
      const fieldButton = page.locator('text=/field|Field/i');
      const fieldButtonExists = (await fieldButton.count()) > 0;
      console.log(`Field button exists (approach 2): ${fieldButtonExists}`);

      if (fieldButtonExists) {
        await fieldButton.click();
        console.log('Clicked Field button (approach 2)');
        buttonClicked = true;
      }
    } catch (error) {
      console.log('Error in approach 2:', error.message);
    }
  }

  // Approach 3: Click in various positions where the button might be
  if (!buttonClicked) {
    console.log('Button not found, trying to click in various positions...');
    const positions = [
      { x: 400, y: 400 },
      { x: 400, y: 450 },
      { x: 400, y: 500 },
      { x: 350, y: 450 },
      { x: 450, y: 450 },
    ];

    for (const [index, pos] of positions.entries()) {
      if (buttonClicked) break;

      try {
        await clickGamePosition(page, pos.x, pos.y, {
          takeScreenshot: true,
          screenshotName: `field-exploration-click-position-${index + 1}`,
        });
        console.log(`Clicked position ${index + 1}: (${pos.x}, ${pos.y})`);

        // Wait a bit to see if the scene changes
        await page.waitForTimeout(1000);

        // Check if we've transitioned to the field scene
        const logs = await captureConsoleLogs(page, 500);
        const fieldSceneEvents = logs.logs.filter(
          (log) => log.includes('field') || log.includes('Field') || log.includes('scene')
        );

        if (fieldSceneEvents.length > 0) {
          console.log('Field scene events detected after click:', fieldSceneEvents);
          buttonClicked = true;
          break;
        }
      } catch (error) {
        console.log(`Error clicking position ${index + 1}:`, error.message);
      }
    }
  }

  // Wait for field scene to load
  await page.waitForTimeout(2000);

  // Take a screenshot of the field scene
  await takeScreenshot(page, 'field-exploration-field-scene', true);

  // Capture console logs to check for scene change
  const fieldSceneLogs = await captureConsoleLogs(page, 1000);
  const fieldSceneEvents = fieldSceneLogs.logs.filter(
    (log) => log.includes('scene') || log.includes('Scene') || log.includes('field')
  );
  console.log('Field scene events:', fieldSceneEvents);

  // Test systematic movement in all directions
  console.log('Testing systematic movement...');

  // Define movement sequence
  const movements = [
    { key: 'ArrowUp', name: 'up' },
    { key: 'ArrowRight', name: 'right' },
    { key: 'ArrowDown', name: 'down' },
    { key: 'ArrowLeft', name: 'left' },
  ];

  // Execute each movement multiple times
  for (let i = 0; i < 3; i++) {
    for (const movement of movements) {
      console.log(`Moving ${movement.name}...`);
      await page.keyboard.press(movement.key);
      await page.waitForTimeout(300);

      // Take a screenshot every few moves to avoid too many screenshots
      if (i === 1) {
        await takeScreenshot(page, `field-exploration-move-${movement.name}`, true);
      }

      // Capture logs to check for collisions or interactions
      const moveLogs = await captureConsoleLogs(page, 100);
      const collisionLogs = moveLogs.logs.filter(
        (log) => log.includes('collision') || log.includes('Collision')
      );

      if (collisionLogs.length > 0) {
        console.log(`Collision detected while moving ${movement.name}:`, collisionLogs);
        await takeScreenshot(page, `field-exploration-collision-${movement.name}`, true);
      }
    }
  }

  // Test interaction with space key
  console.log('Testing interaction with space key...');
  await page.keyboard.press('Space');
  await page.waitForTimeout(1000);

  // Capture logs to check for interactions
  const interactionLogs = await captureConsoleLogs(page, 1000);
  const interactionEvents = interactionLogs.logs.filter(
    (log) =>
      log.includes('interact') ||
      log.includes('Interact') ||
      log.includes('triggered') ||
      log.includes('Narrative event')
  );

  console.log('Interaction events:', interactionEvents);

  // Take a screenshot after interaction
  await takeScreenshot(page, 'field-exploration-after-interaction', true);

  // If we found an interactable, check for narrative events
  if (interactionEvents.length > 0) {
    console.log('Interactable found, checking for narrative events...');

    // Wait for narrative dialog to appear
    await page.waitForTimeout(1000);

    // Check for narrative text
    const narrativeVisible = await page.locator('text=/tower|ruins|discovery/i').isVisible();
    console.log(`Narrative text visible: ${narrativeVisible}`);

    if (narrativeVisible) {
      await takeScreenshot(page, 'field-exploration-narrative-dialog', true);

      // Try to make a choice if available
      const choiceButtons = await page.locator('text=/Investigate|Keep|Try|Circle|Back/i').count();
      console.log(`Found ${choiceButtons} choice buttons`);

      if (choiceButtons > 0) {
        // Click the first choice
        await page.locator('text=/Investigate|Keep|Try|Circle|Back/i').first().click();
        await page.waitForTimeout(1000);
        await takeScreenshot(page, 'field-exploration-after-choice', true);

        // Capture logs to check for choice outcome
        const choiceLogs = await captureConsoleLogs(page, 1000);
        const choiceEvents = choiceLogs.logs.filter(
          (log) => log.includes('Choice') || log.includes('choice')
        );
        console.log('Choice events:', choiceEvents);
      }
    }
  }

  // Take a final screenshot
  await takeScreenshot(page, 'field-exploration-final', true);
});

import { Page, Locator, expect } from '@playwright/test';
import * as path from 'path';
import * as fs from 'fs';
import * as pixelmatch from 'pixelmatch';
import { PNG } from 'pngjs';

// Directory for storing visual test snapshots
const snapshotsDir = path.join(process.cwd(), 'e2e', 'visual-tests', 'snapshots');
const diffDir = path.join(process.cwd(), 'e2e', 'visual-tests', 'diffs');

// Ensure directories exist
[snapshotsDir, diffDir].forEach(dir => {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
});

/**
 * Interface for visual comparison options
 */
export interface VisualComparisonOptions {
  /**
   * Maximum allowed difference in pixels
   */
  maxDiffPixels?: number;
  
  /**
   * Threshold for the entire image (0-1)
   */
  threshold?: number;
  
  /**
   * Whether to save diff images
   */
  saveDiffImage?: boolean;
  
  /**
   * Whether to update snapshots
   */
  updateSnapshots?: boolean;
  
  /**
   * Mask color for ignoring areas (RGBA)
   */
  maskColor?: { r: number; g: number; b: number; a: number };
  
  /**
   * Areas to mask (ignore) during comparison
   */
  maskAreas?: Array<{ x: number; y: number; width: number; height: number }>;
}

/**
 * Take a screenshot of a specific element for visual testing
 * @param element Playwright locator for the element
 * @param name Name of the screenshot (without extension)
 * @returns Path to the saved screenshot
 */
export async function takeElementSnapshot(
  element: Locator,
  name: string
): Promise<string> {
  // Ensure element is visible
  await expect(element).toBeVisible();
  
  // Take screenshot
  const buffer = await element.screenshot();
  
  // Save screenshot
  const screenshotPath = path.join(snapshotsDir, `${name}.png`);
  fs.writeFileSync(screenshotPath, buffer);
  
  return screenshotPath;
}

/**
 * Take a screenshot of the game canvas for visual testing
 * @param page Playwright page
 * @param name Name of the screenshot (without extension)
 * @returns Path to the saved screenshot
 */
export async function takeGameSnapshot(
  page: Page,
  name: string
): Promise<string> {
  // Find the game canvas
  const canvas = page.locator('canvas').first();
  
  // Take screenshot
  return takeElementSnapshot(canvas, name);
}

/**
 * Compare a screenshot with a reference snapshot
 * @param actualPath Path to the actual screenshot
 * @param expectedName Name of the expected snapshot (without extension)
 * @param options Comparison options
 * @returns Comparison result
 */
export async function compareSnapshots(
  actualPath: string,
  expectedName: string,
  options: VisualComparisonOptions = {}
): Promise<{
  match: boolean;
  diffPixels: number;
  diffPercentage: number;
  diffPath?: string;
}> {
  // Default options
  const {
    maxDiffPixels = 100,
    threshold = 0.1,
    saveDiffImage = true,
    updateSnapshots = process.env.UPDATE_SNAPSHOTS === 'true',
    maskColor = { r: 255, g: 0, b: 255, a: 255 }, // Magenta
    maskAreas = [],
  } = options;
  
  // Path to expected snapshot
  const expectedPath = path.join(snapshotsDir, `${expectedName}.png`);
  
  // If expected snapshot doesn't exist or we're updating snapshots, use actual as expected
  if (!fs.existsSync(expectedPath) || updateSnapshots) {
    fs.copyFileSync(actualPath, expectedPath);
    return { match: true, diffPixels: 0, diffPercentage: 0 };
  }
  
  // Read images
  const actualImg = PNG.sync.read(fs.readFileSync(actualPath));
  const expectedImg = PNG.sync.read(fs.readFileSync(expectedPath));
  
  // Create diff image
  const { width, height } = actualImg;
  const diffImg = new PNG({ width, height });
  
  // Apply masks if any
  if (maskAreas.length > 0) {
    // Create copies to avoid modifying originals
    const actualCopy = PNG.sync.read(fs.readFileSync(actualPath));
    const expectedCopy = PNG.sync.read(fs.readFileSync(expectedPath));
    
    // Apply mask to both images
    maskAreas.forEach(area => {
      for (let y = area.y; y < area.y + area.height; y++) {
        for (let x = area.x; x < area.x + area.width; x++) {
          if (x < width && y < height) {
            const idx = (y * width + x) * 4;
            actualCopy.data[idx] = maskColor.r;
            actualCopy.data[idx + 1] = maskColor.g;
            actualCopy.data[idx + 2] = maskColor.b;
            actualCopy.data[idx + 3] = maskColor.a;
            
            expectedCopy.data[idx] = maskColor.r;
            expectedCopy.data[idx + 1] = maskColor.g;
            expectedCopy.data[idx + 2] = maskColor.b;
            expectedCopy.data[idx + 3] = maskColor.a;
          }
        }
      }
    });
    
    // Compare masked images
    const diffPixels = pixelmatch(
      actualCopy.data,
      expectedCopy.data,
      diffImg.data,
      width,
      height,
      { threshold }
    );
    
    // Calculate diff percentage
    const totalPixels = width * height;
    const diffPercentage = (diffPixels / totalPixels) * 100;
    
    // Save diff image if needed
    let diffPath: string | undefined;
    if (saveDiffImage) {
      diffPath = path.join(diffDir, `${expectedName}-diff.png`);
      fs.writeFileSync(diffPath, PNG.sync.write(diffImg));
    }
    
    return {
      match: diffPixels <= maxDiffPixels,
      diffPixels,
      diffPercentage,
      diffPath,
    };
  }
  
  // Compare images without masking
  const diffPixels = pixelmatch(
    actualImg.data,
    expectedImg.data,
    diffImg.data,
    width,
    height,
    { threshold }
  );
  
  // Calculate diff percentage
  const totalPixels = width * height;
  const diffPercentage = (diffPixels / totalPixels) * 100;
  
  // Save diff image if needed
  let diffPath: string | undefined;
  if (saveDiffImage) {
    diffPath = path.join(diffDir, `${expectedName}-diff.png`);
    fs.writeFileSync(diffPath, PNG.sync.write(diffImg));
  }
  
  return {
    match: diffPixels <= maxDiffPixels,
    diffPixels,
    diffPercentage,
    diffPath,
  };
}

/**
 * Take a snapshot of a component and compare it with a reference
 * @param element Playwright locator for the element
 * @param name Name of the snapshot (without extension)
 * @param options Comparison options
 * @returns Comparison result
 */
export async function expectComponentToMatchSnapshot(
  element: Locator,
  name: string,
  options: VisualComparisonOptions = {}
): Promise<void> {
  // Take snapshot
  const actualPath = await takeElementSnapshot(element, `${name}-actual`);
  
  // Compare with reference
  const result = await compareSnapshots(actualPath, name, options);
  
  // Log comparison result
  console.log(`Visual comparison for ${name}:`, {
    match: result.match,
    diffPixels: result.diffPixels,
    diffPercentage: result.diffPercentage.toFixed(2) + '%',
    diffPath: result.diffPath,
  });
  
  // Assert match
  expect(result.match, 
    `Visual comparison failed for ${name}: ${result.diffPixels} different pixels (${result.diffPercentage.toFixed(2)}%)`
  ).toBeTruthy();
}

/**
 * Take a snapshot of the game canvas and compare it with a reference
 * @param page Playwright page
 * @param name Name of the snapshot (without extension)
 * @param options Comparison options
 * @returns Comparison result
 */
export async function expectGameToMatchSnapshot(
  page: Page,
  name: string,
  options: VisualComparisonOptions = {}
): Promise<void> {
  // Find the game canvas
  const canvas = page.locator('canvas').first();
  
  // Compare with reference
  await expectComponentToMatchSnapshot(canvas, name, options);
}

/**
 * Wait for animations to complete
 * @param page Playwright page
 * @param timeout Timeout in milliseconds
 */
export async function waitForAnimationsToComplete(
  page: Page,
  timeout: number = 1000
): Promise<void> {
  // Wait for a short time to allow animations to start
  await page.waitForTimeout(100);
  
  // Wait for animations to complete
  await page.waitForFunction(
    () => {
      const animations = document.getAnimations();
      return animations.length === 0 || animations.every(a => a.playState === 'finished');
    },
    { timeout }
  );
}

/**
 * Wait for the game to stabilize (no more rendering changes)
 * @param page Playwright page
 * @param options Options for stabilization
 */
export async function waitForGameToStabilize(
  page: Page,
  options: {
    timeout?: number;
    checkInterval?: number;
    maxConsecutiveMatches?: number;
  } = {}
): Promise<void> {
  const {
    timeout = 10000,
    checkInterval = 500,
    maxConsecutiveMatches = 3,
  } = options;
  
  // Find the game canvas
  const canvas = page.locator('canvas').first();
  
  // Take initial screenshot
  let lastScreenshot = await canvas.screenshot();
  let consecutiveMatches = 0;
  const startTime = Date.now();
  
  // Check for stability
  while (Date.now() - startTime < timeout) {
    // Wait for a short time
    await page.waitForTimeout(checkInterval);
    
    // Take new screenshot
    const newScreenshot = await canvas.screenshot();
    
    // Compare screenshots
    const img1 = PNG.sync.read(lastScreenshot);
    const img2 = PNG.sync.read(newScreenshot);
    const { width, height } = img1;
    const diff = new PNG({ width, height });
    
    const diffPixels = pixelmatch(
      img1.data,
      img2.data,
      diff.data,
      width,
      height,
      { threshold: 0.1 }
    );
    
    // If no difference, increment consecutive matches
    if (diffPixels === 0) {
      consecutiveMatches++;
      
      // If enough consecutive matches, consider stable
      if (consecutiveMatches >= maxConsecutiveMatches) {
        console.log(`Game stabilized after ${Date.now() - startTime}ms`);
        return;
      }
    } else {
      // Reset consecutive matches
      consecutiveMatches = 0;
      
      // Update last screenshot
      lastScreenshot = newScreenshot;
    }
  }
  
  // If we get here, we timed out
  console.log(`Game did not stabilize after ${timeout}ms`);
}

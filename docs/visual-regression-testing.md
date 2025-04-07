# Visual Regression Testing

This document describes the visual regression testing process for the Signal Lost game.

## Overview

Visual regression testing helps ensure that UI components render correctly across different browsers and screen sizes. It works by taking screenshots of UI components and comparing them with reference screenshots to detect visual changes.

## Tools and Libraries

The visual regression testing system uses:

- **Playwright**: For browser automation and screenshot capture
- **PixelMatch**: For pixel-by-pixel image comparison
- **PNGjs**: For PNG image processing

## Directory Structure

- `e2e/visual-tests/`: Contains visual regression tests
  - `snapshots/`: Reference screenshots
  - `diffs/`: Difference images when tests fail
  - `results/`: Test results and logs

## Running Visual Regression Tests

### Basic Usage

To run visual regression tests:

```bash
npm run test:visual
```

This will:

1. Start the development server
2. Run the visual regression tests
3. Compare screenshots with reference screenshots
4. Report any visual differences

### Updating Reference Screenshots

When you make intentional UI changes, you need to update the reference screenshots:

```bash
npm run test:visual:update
```

This will run the tests and update all reference screenshots with the current UI state.

### Viewing Test Reports

To generate and view an HTML report of the visual regression tests:

```bash
npm run test:visual:report
```

## Writing Visual Regression Tests

### Basic Test Structure

```typescript
import { test } from '@playwright/test';
import {
  expectComponentToMatchSnapshot,
  expectGameToMatchSnapshot,
  waitForGameToStabilize,
} from '../helpers/visualTestHelpers';

test('Component renders correctly', async ({ page }) => {
  // Navigate to the page
  await page.goto('http://localhost:5173/');

  // Wait for the game to load and stabilize
  await waitForGameToStabilize(page);

  // Take a snapshot of the component
  const component = page.locator('[data-testid="my-component"]');
  await expectComponentToMatchSnapshot(component, 'my-component');
});
```

### Testing Dynamic Components

For components with animations or dynamic content:

```typescript
// Wait for animations to complete
await waitForAnimationsToComplete(page);

// Wait for the game to stabilize
await waitForGameToStabilize(page);

// Take a snapshot
await expectGameToMatchSnapshot(page, 'game-after-animation');
```

### Ignoring Dynamic Areas

To ignore areas that change between tests (like timestamps or random elements):

```typescript
await expectComponentToMatchSnapshot(component, 'component-with-dynamic-area', {
  maskAreas: [
    { x: 100, y: 50, width: 200, height: 30 }, // Area to ignore
  ],
});
```

## Configuration

The visual regression testing configuration is in `playwright.visual.config.ts`:

```typescript
expect: {
  toMatchSnapshot: {
    // Maximum allowed difference in pixels
    maxDiffPixels: 100,
    // Threshold for the entire image
    threshold: 0.2,
  },
},
```

You can adjust these parameters to control the sensitivity of the comparison:

- `maxDiffPixels`: Maximum number of pixels that can differ
- `threshold`: Threshold for considering pixels different (0-1)

## CI/CD Integration

Visual regression tests run automatically in the CI/CD pipeline:

1. Tests run on every pull request and push to main branches
2. Reference screenshots are stored in the repository
3. Test artifacts (screenshots and diffs) are uploaded as build artifacts
4. Tests fail if visual differences exceed thresholds

## Troubleshooting

### Tests Failing Due to Minor Differences

If tests fail due to minor differences:

1. Check the diff images in `e2e/visual-tests/diffs/`
2. If the differences are acceptable, update the reference screenshots
3. If the differences are not acceptable, fix the UI issues

### Tests Failing Due to Timing Issues

If tests fail due to animations or loading:

1. Increase the stabilization timeout in `waitForGameToStabilize()`
2. Add explicit waits for specific elements or conditions
3. Use `waitForAnimationsToComplete()` to wait for animations

### Browser Differences

Visual differences between browsers are expected. To handle this:

1. Run tests in a single browser for consistency
2. Use browser-specific reference screenshots if needed
3. Adjust comparison thresholds for different browsers

## Best Practices

1. **Keep reference screenshots in version control** to track UI changes over time
2. **Update reference screenshots** when making intentional UI changes
3. **Use data-testid attributes** for reliable component selection
4. **Test responsive behavior** across different screen sizes
5. **Mask dynamic areas** that change between test runs
6. **Wait for animations and loading** to complete before taking screenshots
7. **Run tests in headless mode** for consistent results
8. **Review diff images** to understand visual changes

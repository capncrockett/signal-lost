# Signal Lost - Debugging Guide

This guide provides information on how to debug common issues in the Signal Lost game.

## Table of Contents

1. [Development Environment Setup](#development-environment-setup)
2. [Common Issues and Solutions](#common-issues-and-solutions)
3. [Asset Loading Issues](#asset-loading-issues)
4. [Phaser Initialization Problems](#phaser-initialization-problems)
5. [Audio Troubleshooting](#audio-troubleshooting)
6. [E2E Test Debugging](#e2e-test-debugging)
7. [Debugging Tools](#debugging-tools)

## Development Environment Setup

Before debugging, ensure your development environment is properly set up:

```bash
# Install dependencies
npm install

# Start the development server
npm run dev

# In a separate terminal, run tests
npm run test

# Run E2E tests
npm run test:e2e:ci
```

## Common Issues and Solutions

### TypeScript Errors

If you encounter TypeScript errors:

1. Run the type checker: `npm run type-check`
2. Check for missing type definitions
3. Ensure you're not using `any` types unnecessarily
4. Look for null/undefined access issues

### ESLint Warnings/Errors

To fix linting issues:

1. Run ESLint: `npm run lint`
2. Use `--fix` option to automatically fix some issues: `npm run lint -- --fix`
3. Check the `.eslintrc.json` file for rule configurations

### Test Failures

When tests fail:

1. Check the test output for specific error messages
2. Run a specific test file: `npm test -- path/to/test.ts`
3. Use `--watch` for interactive testing: `npm test -- --watch`

## Asset Loading Issues

The game uses multiple asset loading paths to handle different environments:

### Diagnosing Asset Loading Issues

1. Check the browser console for 404 errors
2. Verify that assets exist in both `/public/assets/` and `/assets/` directories
3. Look for asset loading errors in the console logs

### Fixing Asset Loading Problems

The game implements fallback mechanisms:

```typescript
// Example of asset loading with fallback
this.load.on('loaderror', (file: Phaser.Loader.File) => {
  const url = typeof file.url === 'string' ? file.url : String(file.url);
  console.error(`Error loading asset: ${file.key} from ${url}`);
  
  // Try alternative path
  if (!file.key.endsWith('_alt')) {
    if (url.startsWith('/')) {
      const newUrl = url.substring(1);
      this.load.image(`${file.key}_alt`, newUrl);
    } else {
      this.load.image(`${file.key}_alt`, `/${url}`);
    }
  }
});
```

## Phaser Initialization Problems

### Canvas Not Rendering

If the game canvas doesn't appear:

1. Check if the DOM element exists: `<div id="game"></div>`
2. Verify Phaser configuration in `main.ts`
3. Check for JavaScript errors in the console

### Game Freezing or Crashing

If the game freezes:

1. Look for infinite loops in update methods
2. Check for memory leaks (growing memory usage in DevTools)
3. Verify that scene transitions are working correctly

## Audio Troubleshooting

### No Sound

If audio isn't playing:

1. Check if the browser has blocked autoplay (look for icon in address bar)
2. Verify that the audio context is running: `audioContext.state === 'running'`
3. Check volume settings in the game

### Audio Distortion

For audio quality issues:

1. Check sample rates and formats of audio files
2. Verify that gain nodes are set to appropriate levels
3. Look for overlapping sounds playing simultaneously

## E2E Test Debugging

### Playwright Test Failures

When E2E tests fail:

1. Check the screenshots in `e2e/screenshots/`
2. Run tests in headed mode: `npx playwright test --headed`
3. Use `page.pause()` in the test to debug interactively
4. Check console logs captured during the test

### Common E2E Issues

- **Element not found**: Increase timeout or use more reliable selectors
- **Timing issues**: Add appropriate waits or use Playwright's auto-waiting
- **Canvas interactions**: Use `clickGamePosition()` helper for canvas clicks

## Debugging Tools

### Browser DevTools

- **Console**: Check for errors and log messages
- **Network**: Monitor asset loading
- **Performance**: Identify performance bottlenecks
- **Memory**: Check for memory leaks

### Phaser Debug Tools

Enable the Phaser debug plugin in development:

```typescript
// In game config
physics: {
  default: 'arcade',
  arcade: {
    debug: true,
    gravity: { y: 0 }
  }
}
```

### Custom Debug Overlay

The game includes a TestOverlay component that can be used to display debug information:

```typescript
// Enable the test overlay
const testOverlay = new TestOverlay(this);
this.add.existing(testOverlay);

// Log information to the overlay
testOverlay.log('Debug message');
```

## Reporting Issues

When reporting issues:

1. Provide steps to reproduce
2. Include browser and OS information
3. Attach console logs and screenshots
4. Describe expected vs. actual behavior

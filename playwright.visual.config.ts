import { defineConfig, devices } from '@playwright/test';
import path from 'path';

export default defineConfig({
  testDir: './e2e/visual-tests',
  fullyParallel: false, // Run tests sequentially for more consistent snapshots
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: 1, // Use a single worker to avoid race conditions
  reporter: process.env.CI ? 'dot' : [['html', { open: 'never' }], ['list']],

  // Snapshot configuration
  expect: {
    toMatchSnapshot: {
      // Maximum allowed difference in pixels
      maxDiffPixels: 100,
      // Threshold for the entire image
      threshold: 0.2,
    },
  },

  // Shared settings for all projects
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
    headless: true, // Use headless for more consistent snapshots
    launchOptions: {
      args: ['--no-sandbox', '--disable-setuid-sandbox'],
    },
    viewport: { width: 1280, height: 800 },
    actionTimeout: 15000,
    navigationTimeout: 15000,

    // Screenshot configuration
    screenshot: 'only-on-failure',
  },

  // Configure projects for different browsers
  projects: [
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
      },
    },
  ],

  // Configure snapshot path template
  snapshotPathTemplate: '{testDir}/{testFileDir}/{testFileName}-{arg}{ext}',

  // Start the development server
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
  },

  // Output directory for snapshots
  outputDir: path.join(__dirname, 'e2e/visual-tests/results'),
});

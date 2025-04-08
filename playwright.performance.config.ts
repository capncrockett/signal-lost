import { defineConfig, devices } from '@playwright/test';
import path from 'path';

export default defineConfig({
  testDir: './tests/performance',
  fullyParallel: false, // Run tests sequentially for more consistent results
  forbidOnly: !!process.env.CI,
  retries: 0, // No retries for performance tests
  workers: 1, // Use a single worker to avoid interference
  reporter: process.env.CI ? 'dot' : [['html', { open: 'never' }], ['list']],

  // Shared settings for all projects
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
    headless: true, // Use headless for more consistent results
    launchOptions: {
      args: ['--no-sandbox', '--disable-setuid-sandbox'],
    },
    viewport: { width: 1280, height: 800 },
    actionTimeout: 15000,
    navigationTimeout: 15000,
  },

  // Configure projects for different browsers
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  // Output directory for test results
  outputDir: path.join(__dirname, 'performance-reports/results'),
});

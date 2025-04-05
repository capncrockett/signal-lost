import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e/tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'html',
  use: {
    baseURL: 'http://localhost:5173',
    trace: 'on-first-retry',
    headless: false,
    launchOptions: {
      slowMo: 100, // Slow down execution by 100ms
      args: ['--no-sandbox', '--disable-setuid-sandbox'],
    },
    viewport: { width: 1024, height: 768 }, // Set viewport size to ensure game is visible
    actionTimeout: 15000, // Increase action timeout
    navigationTimeout: 15000, // Increase navigation timeout
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
  webServer: {
    command: 'npm run dev',
    url: 'http://localhost:5173',
    reuseExistingServer: !process.env.CI,
  },
});

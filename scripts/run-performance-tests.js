/**
 * Script to run performance tests
 */
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

// Configuration
const config = {
  serverPort: 5173,
  serverCommand: 'npm run dev',
  testCommand:
    'npx playwright test tests/performance/performance.spec.ts --config=playwright.performance.config.ts',
  updateBaseline: process.argv.includes('--update-baseline'),
  outputDir: 'performance-reports',
};

// Create output directory if it doesn't exist
const outputDir = path.resolve(config.outputDir);
if (!fs.existsSync(outputDir)) {
  fs.mkdirSync(outputDir, { recursive: true });
}

// Start the development server
console.log('Starting development server...');
const serverProcess = require('child_process').spawn('npm', ['run', 'dev'], {
  stdio: 'pipe',
  shell: true,
});

// Wait for server to start
console.log('Waiting for server to start...');
let serverStarted = false;
serverProcess.stdout.on('data', (data) => {
  const output = data.toString();
  console.log(`[Server] ${output}`);

  if (output.includes('Local:') && !serverStarted) {
    serverStarted = true;
    runTests();
  }
});

serverProcess.stderr.on('data', (data) => {
  console.error(`[Server Error] ${data.toString()}`);
});

// Run tests
function runTests() {
  console.log('Running performance tests...');
  try {
    // Run tests
    const testCommand = config.updateBaseline
      ? `${config.testCommand} --update-baseline`
      : config.testCommand;

    execSync(testCommand, { stdio: 'inherit' });
    console.log('Performance tests completed successfully');
  } catch (error) {
    console.error('Performance tests failed:', error.message);
    process.exitCode = 1;
  } finally {
    // Kill server
    serverProcess.kill();
    console.log('Server stopped');
  }
}

// Handle process termination
process.on('SIGINT', () => {
  console.log('Stopping server...');
  serverProcess.kill();
  process.exit();
});

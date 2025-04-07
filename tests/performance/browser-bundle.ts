/**
 * Browser bundle for performance tests
 * This file is bundled with webpack and injected into the browser
 */

// Export performance test classes
export { PerformanceMonitor } from '../../src/utils/PerformanceMonitor';
export { PerformanceBenchmark } from '../../src/utils/PerformanceBenchmark';
export { PerformanceTests } from './PerformanceTests';
export { PerformanceTestRunner } from './PerformanceTestRunner';

// Add to window object
declare global {
  interface Window {
    PerformanceMonitor: typeof import('../../src/utils/PerformanceMonitor').PerformanceMonitor;
    PerformanceBenchmark: typeof import('../../src/utils/PerformanceBenchmark').PerformanceBenchmark;
    PerformanceTests: typeof import('./PerformanceTests').PerformanceTests;
    PerformanceTestRunner: typeof import('./PerformanceTestRunner').PerformanceTestRunner;
    game: any;
  }
}

// Assign to window
window.PerformanceMonitor = import('../../src/utils/PerformanceMonitor').then(m => m.PerformanceMonitor);
window.PerformanceBenchmark = import('../../src/utils/PerformanceBenchmark').then(m => m.PerformanceBenchmark);
window.PerformanceTests = import('./PerformanceTests').then(m => m.PerformanceTests);
window.PerformanceTestRunner = import('./PerformanceTestRunner').then(m => m.PerformanceTestRunner);

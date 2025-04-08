# Performance Benchmarks

This document describes the performance benchmarking system for the Signal Lost game.

## Overview

The performance benchmarking system helps ensure that the game runs smoothly across different devices and browsers. It measures various aspects of game performance, including:

- Frame rate (FPS)
- Memory usage
- Asset loading times
- Scene transition times
- Render and update performance

## Tools and Components

The performance benchmarking system consists of several components:

1. **PerformanceMonitor**: Tracks real-time performance metrics during gameplay
2. **PerformanceDisplay**: UI component for displaying performance metrics
3. **PerformanceBenchmark**: Utility for running performance benchmarks
4. **PerformanceTests**: Test suite for measuring game performance
5. **PerformanceTestRunner**: Runner for executing performance tests and generating reports

## Running Performance Tests

### Basic Usage

To run performance tests:

```bash
# Build the performance test bundle
npm run build:performance

# Run performance tests
npm run test:performance
```

This will:

1. Build the performance test bundle
2. Start the development server
3. Run the performance tests
4. Generate a performance report

### Updating Baseline

To update the performance baseline:

```bash
npm run test:performance:update
```

This will run the tests and update the baseline with the current results.

## Performance Metrics

### Frame Rate (FPS)

The system tracks frame rate metrics:

- **Average FPS**: Average frames per second
- **Minimum FPS**: Lowest recorded FPS
- **Maximum FPS**: Highest recorded FPS
- **FPS Stability**: Standard deviation of FPS values

Target values:

- Average FPS: 60+
- Minimum FPS: 30+
- FPS Stability: < 10 (standard deviation)

### Memory Usage

The system tracks memory usage metrics:

- **Used Memory**: Amount of memory used by the game
- **Total Memory**: Total memory allocated
- **Memory Limit**: Maximum memory available
- **Memory Growth**: Rate of memory increase over time

Target values:

- Memory Growth: < 1MB/minute (to avoid memory leaks)

### Load Times

The system tracks load time metrics:

- **Asset Load Time**: Time to load individual assets
- **Scene Load Time**: Time to load and initialize scenes
- **Total Load Time**: Total time from start to playable state

Target values:

- Asset Load Time: < 500ms per asset
- Scene Load Time: < 2000ms per scene
- Total Load Time: < 5000ms

### Render and Update Performance

The system tracks render and update performance:

- **Render Time**: Time spent rendering each frame
- **Update Time**: Time spent updating game logic
- **Physics Time**: Time spent on physics calculations

Target values:

- Render Time: < 16ms (for 60 FPS)
- Update Time: < 8ms (half of render time)
- Physics Time: < 4ms (quarter of render time)

## Performance Reports

Performance reports are generated in the `performance-reports` directory and include:

- **Benchmark Results**: Results of individual benchmarks
- **Summary**: Overall performance summary
- **Comparison**: Comparison with baseline
- **Pass/Fail**: Whether the performance meets the targets

Example report:

```json
{
  "timestamp": 1621234567890,
  "results": [
    {
      "name": "Basic Render",
      "iterations": 100,
      "average": 2.5,
      "median": 2.3,
      "min": 1.8,
      "max": 4.2,
      "stdDev": 0.5,
      "total": 250,
      "timestamp": 1621234567890
    }
    // More benchmark results...
  ],
  "summary": {
    "renderTime": {
      "average": 2.5,
      "threshold": 16,
      "pass": true
    },
    "updateTime": {
      "average": 1.2,
      "threshold": 8,
      "pass": true
    },
    "assetLoadTime": {
      "average": 120,
      "threshold": 500,
      "pass": true
    },
    "sceneLoadTime": {
      "average": 850,
      "threshold": 2000,
      "pass": true
    },
    "overallPass": true
  }
}
```

## Performance Monitoring During Development

### Using the Performance Display

You can add the performance display to any scene:

```typescript
import { PerformanceDisplay } from '../components/PerformanceDisplay';

// In your scene's create method:
const performanceDisplay = new PerformanceDisplay(this, 10, 10);
this.add.existing(performanceDisplay);
```

This will show real-time FPS and memory usage.

### Manual Benchmarking

You can run manual benchmarks in the browser console:

```javascript
// Get the performance monitor
const monitor = PerformanceMonitor.getInstance();

// Run a benchmark
monitor.runBenchmark(
  'My Benchmark',
  () => {
    // Code to benchmark
    for (let i = 0; i < 1000; i++) {
      Math.sqrt(i);
    }
  },
  100
);
```

## CI/CD Integration

Performance tests are integrated into the CI/CD pipeline:

1. Tests run automatically on pull requests
2. Tests fail if performance regressions exceed thresholds
3. Performance reports are uploaded as artifacts

## Best Practices

1. **Run performance tests regularly** to catch regressions early
2. **Update the baseline** when making intentional performance changes
3. **Optimize critical paths** identified by performance tests
4. **Test on multiple devices** to ensure consistent performance
5. **Monitor memory usage** to prevent leaks
6. **Batch operations** to reduce CPU usage
7. **Minimize DOM operations** in the game loop
8. **Use object pooling** for frequently created/destroyed objects
9. **Optimize asset loading** by using appropriate formats and sizes
10. **Implement progressive loading** for large assets

# Current Sprint Tasks

## Critical Issues
- ✅ Remove duplicate "Go to Field" button
  - ✅ Remove DOM button from MainScene
  - ✅ Keep only the Phaser Text button
  - ✅ Update e2e tests to target correct button
  - ✅ Added TestOverlay utility for better E2E testing with data-testid attributes

- ✅ Fix Volume Control
  - ✅ Set initial volume to 80% in VolumeControl constructor
  - ✅ Normalize volume scaling across components
  - ✅ Add volume curve for more natural adjustment
  - ✅ Test volume levels across all audio components
  - ✅ Fix volume knob jumping when clicked
  - ✅ Ensure volume control affects static noise

- ✅ Audio Improvements
  - ✅ Reduce overall volume of static noise
    - ✅ Cut static volume in half across all components
    - ✅ Normalized volume scaling for better user experience
  - ✅ Use pink noise instead of white noise for gentler sound
    - ✅ Implemented in both SoundscapeManager and RadioTuner
    - ✅ Added fallback implementation using Voss algorithm
    - ✅ Created NoiseGenerator utility for consistent noise generation

- ✅ Viewport & Scaling
  - ✅ Fix Phaser scale configuration (using fixed dimensions)
  - ✅ Test responsive behavior
  - ✅ Add viewport meta tags
  - ✅ Verify mobile compatibility
  - ✅ Add rendering tests with screenshots
  - ✅ Fix TypeScript errors in test files

- ✅ Improve E2E Tests
  - ✅ Add comprehensive rendering tests for different resolutions
  - ✅ Test audio initialization
  - ✅ Capture screenshots for visual verification
  - [ ] Add actual gameplay flow testing
  - [ ] Verify signal detection
  - [ ] Add field exploration tests
  - [ ] Test save/load functionality

## Game Development
- [ ] Add actual gameplay elements
  - [ ] Implement signal discovery mechanics
  - [ ] Add narrative elements
  - [ ] Create field exploration content
  - [ ] Add inventory system
  - [ ] Implement save/load system

## Documentation
- [ ] Update README with actual gameplay instructions
- [ ] Add debugging guide
- [ ] Document audio system architecture
- [ ] Add contribution guidelines

## Testing Infrastructure
- [ ] Add visual regression testing
- [ ] Improve test coverage for audio components
- [ ] Add performance benchmarks
- [ ] Create test data fixtures
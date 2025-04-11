# Sprint: Hybrid DOM + Canvas Approach

## Overview

After evaluating different approaches for the Signal Lost game, we've decided to implement a hybrid approach that combines DOM elements for interactive components with Canvas for visual effects. This approach provides the best balance of testability, performance, and maintainability.

## Goals

1. ✓ Implement a hybrid DOM + Canvas approach for the radio tuner component
2. ⬜ Ensure full testability with Playwright E2E tests
3. ⬜ Optimize performance for visual effects
4. ⬜ Maintain clean separation between UI and effects
5. ⬜ Create comprehensive documentation for the approach

## Technical Approach

### Hybrid DOM + Canvas Architecture

1. **DOM Elements for Interactive Parts**
   - ⬜ Buttons, sliders, and displays as standard HTML elements
   - ⬜ Fully testable with Playwright
   - ⬜ Accessible and keyboard navigable
   - ⬜ Styled with CSS for consistent appearance

2. **Canvas for Visual Effects**
   - ⬜ Static visualization using canvas
   - ⬜ Audio waveform display
   - ⬜ Non-interactive visual elements
   - ⬜ Performance-optimized rendering

3. **State Management**
   - ⬜ React context or reducer for game state
   - ⬜ Update DOM elements based on state
   - ⬜ Canvas visualizations read from state but don't update it
   - ⬜ Clear separation between UI logic and visual effects

## Implementation Plan

### Radio Tuner Component

1. **DOM Elements**
   - ⬜ Frequency display (text)
   - ⬜ Power button (toggle)
   - ⬜ Tune up/down buttons
   - ⬜ Frequency slider (input range)
   - ⬜ Signal strength indicator (div with width)
   - ⬜ Message display toggle

2. **Canvas Elements**
   - ⬜ Static visualization
   - ⬜ Signal strength visualization
   - ⬜ Audio waveform display

3. **State Management**
   - ⬜ Frequency state
   - ⬜ Power state (on/off)
   - ⬜ Signal detection
   - ⬜ Static intensity
   - ⬜ Message display

### Example Implementation

```jsx
function RadioTuner() {
  // State managed outside rendering cycle
  const [frequency, setFrequency] = useState(90.0);
  const [isOn, setIsOn] = useState(false);
  const [signalStrength, setSignalStrength] = useState(0);
  const [staticIntensity, setStaticIntensity] = useState(0.5);
  const canvasRef = useRef(null);
  
  // Canvas for visual effects only
  useEffect(() => {
    if (!canvasRef.current || !isOn) return;
    
    const ctx = canvasRef.current.getContext('2d');
    
    // Draw static visualization
    const drawStatic = () => {
      // Visual-only code that doesn't affect state
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      // Draw static based on frequency and staticIntensity...
      
      if (isOn) {
        requestAnimationFrame(drawStatic);
      }
    };
    
    requestAnimationFrame(drawStatic);
    
    return () => {
      // Cleanup
    };
  }, [isOn, staticIntensity]);
  
  // Signal detection logic
  useEffect(() => {
    if (!isOn) return;
    
    // Check for signals at current frequency
    const signal = findSignalAtFrequency(frequency);
    
    if (signal) {
      // Calculate signal strength
      const strength = calculateSignalStrength(frequency, signal);
      setSignalStrength(strength);
      
      // Calculate static intensity
      const intensity = signal.isStatic ? 1 - strength : (1 - strength) * 0.5;
      setStaticIntensity(intensity);
    } else {
      // No signal found
      setSignalStrength(0.1);
      setStaticIntensity(0.8);
    }
  }, [isOn, frequency]);
  
  // DOM elements for interaction
  return (
    <div className="radio-tuner" data-testid="radio-tuner">
      <div className="frequency-display" data-testid="frequency-display">
        {frequency.toFixed(1)} MHz
      </div>
      
      <button 
        data-testid="power-button"
        onClick={() => setIsOn(!isOn)}
      >
        {isOn ? 'ON' : 'OFF'}
      </button>
      
      <div className="tuner-controls">
        <button 
          data-testid="tune-down-button"
          onClick={() => setFrequency(prev => Math.max(88.0, prev - 0.1))}
        >
          -0.1
        </button>
        
        <input
          type="range"
          min="88.0"
          max="108.0"
          step="0.1"
          value={frequency}
          onChange={e => setFrequency(parseFloat(e.target.value))}
          data-testid="frequency-slider"
        />
        
        <button 
          data-testid="tune-up-button"
          onClick={() => setFrequency(prev => Math.min(108.0, prev + 0.1))}
        >
          +0.1
        </button>
      </div>
      
      <div className="signal-strength-meter">
        <div 
          className="signal-strength-fill"
          style={{ width: `${signalStrength * 100}%` }}
          data-testid="signal-strength"
        ></div>
      </div>
      
      <canvas 
        ref={canvasRef}
        className="static-visualization"
        data-testid="static-canvas"
      />
    </div>
  );
}
```

## Testing Strategy

### Unit Tests

- ⬜ Test DOM element interactions
- ⬜ Test state changes
- ⬜ Mock canvas context for testing
- ⬜ Test signal detection logic

### E2E Tests

- ⬜ Test radio tuner interactions
- ⬜ Verify frequency changes
- ⬜ Test signal detection
- ⬜ Capture screenshots for visual verification

Example E2E Test:

```typescript
test('DOM-based radio tuner works correctly', async ({ page }) => {
  await page.goto('http://localhost:5173/radio');
  
  // Turn on radio
  await page.locator('[data-testid="power-button"]').click();
  
  // Drag the frequency slider
  await page.locator('[data-testid="frequency-slider"]').click();
  
  // Click tune buttons
  await page.locator('[data-testid="tune-up-button"]').click();
  
  // Verify frequency display
  await expect(page.locator('[data-testid="frequency-display"]')).toHaveText('90.1 MHz');
  
  // Check signal strength indicator
  await expect(page.locator('[data-testid="signal-strength"]')).toHaveAttribute('style', /width: \d+%/);
  
  // Take screenshot to verify canvas rendering
  await page.screenshot({ path: 'e2e/screenshots/radio-tuner.png' });
});
```

## Performance Considerations

- ⬜ Limit canvas redraws to animation frames
- ⬜ Use requestAnimationFrame for smooth animations
- ⬜ Optimize canvas drawing operations
- ⬜ Avoid unnecessary state updates
- ⬜ Use memoization for expensive calculations
- ⬜ Throttle frequency updates during dragging

## Accessibility

- ⬜ Ensure keyboard navigation for all controls
- ⬜ Add proper ARIA attributes
- ⬜ Provide alternative text for canvas visualizations
- ⬜ Ensure color contrast meets WCAG standards
- ⬜ Test with screen readers

## Progress Tracking

| Task | Status | Assigned To | Notes |
|------|--------|-------------|-------|
| Basic DOM structure | ⬜ Not Started | Agent Alpha | |
| State management | ⬜ Not Started | Agent Alpha | |
| Canvas visualization | ⬜ Not Started | Agent Alpha | |
| Signal detection | ⬜ Not Started | Agent Alpha | |
| Unit tests | ⬜ Not Started | Agent Alpha | |
| E2E tests | ⬜ Not Started | Agent Beta | |
| Styling | ⬜ Not Started | Agent Alpha | |
| Accessibility | ⬜ Not Started | Agent Beta | |
| Performance optimization | ⬜ Not Started | Agent Beta | |
| Documentation | ⬜ Not Started | Agent Alpha | |

## Definition of Done

- ✓ Radio tuner component implemented with hybrid approach
- ✓ All unit tests passing
- ✓ E2E tests passing
- ✓ Performance metrics meet targets
- ✓ Accessibility requirements met
- ✓ Documentation complete

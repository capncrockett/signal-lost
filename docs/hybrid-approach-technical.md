# Hybrid DOM + Canvas Technical Documentation

## Overview

The Signal Lost game uses a hybrid approach that combines DOM elements for interactive components with Canvas for visual effects. This document provides technical details on how to implement and maintain this approach.

## Architecture

### Component Structure

```
RadioTuner/
├── index.tsx              # Main component
├── RadioControls.tsx      # DOM-based controls
├── FrequencyDisplay.tsx   # DOM-based display
├── SignalStrength.tsx     # DOM-based indicator
├── StaticVisualization.tsx # Canvas-based visualization
├── AudioWaveform.tsx      # Canvas-based waveform
└── styles.css             # Component styles
```

### State Management

The hybrid approach uses a combination of React state and refs to manage component state:

1. **React State**: Used for UI state that affects DOM rendering
   - Current frequency
   - Power state (on/off)
   - Signal strength
   - UI mode (scanning, tuning, etc.)

2. **Refs**: Used for canvas state and performance-critical operations
   - Canvas context
   - Animation frames
   - Audio processing
   - Visual effects

### Data Flow

```
┌─────────────────────┐
│                     │
│    React State      │◄───┐
│  (UI & Game State)  │    │
│                     │    │
└─────────┬───────────┘    │
          │                │
          ▼                │
┌─────────────────────┐    │
│                     │    │
│    DOM Elements     │    │
│  (UI Components)    │    │
│                     │    │
└─────────────────────┘    │
                           │
┌─────────────────────┐    │
│                     │    │
│    Canvas Refs      │    │
│  (Visual Effects)   │────┘
│                     │
└─────────────────────┘
```

## Implementation Guidelines

### DOM Elements

1. **Use Standard HTML Elements**
   - Buttons, inputs, divs, etc.
   - Add `data-testid` attributes for testing
   - Style with CSS
   - Ensure accessibility

2. **Handle User Interactions**
   - Click events
   - Drag events
   - Keyboard navigation
   - Focus management

3. **Update React State**
   - Use state hooks for UI state
   - Use context for global state
   - Use reducers for complex state

### Canvas Elements

1. **Create Canvas with Refs**
   ```jsx
   const canvasRef = useRef(null);
   
   useEffect(() => {
     if (!canvasRef.current) return;
     const ctx = canvasRef.current.getContext('2d');
     // Canvas operations...
   }, [dependencies]);
   ```

2. **Optimize Canvas Rendering**
   - Use `requestAnimationFrame` for animations
   - Clear canvas before redrawing
   - Limit canvas size
   - Use appropriate data structures

3. **Separate Canvas Logic**
   - Create custom hooks for canvas operations
   - Separate drawing logic from component logic
   - Use memoization for expensive calculations

### Performance Optimization

1. **Minimize State Updates**
   - Use refs for values that don't affect rendering
   - Batch state updates
   - Use `useCallback` and `useMemo` for expensive operations

2. **Optimize Canvas Operations**
   - Limit canvas size
   - Use appropriate drawing methods
   - Batch drawing operations
   - Use offscreen canvas for complex operations

3. **Throttle User Input**
   - Debounce or throttle frequent events
   - Use `requestAnimationFrame` for smooth animations
   - Optimize event handlers

## Testing

### Unit Testing

1. **Test DOM Elements**
   - Use React Testing Library
   - Test user interactions
   - Test state changes
   - Test accessibility

2. **Test Canvas Elements**
   - Mock canvas context
   - Test canvas setup
   - Test drawing operations
   - Test animation logic

```jsx
// Example unit test for canvas component
test('canvas is set up correctly', () => {
  // Mock canvas context
  const mockContext = {
    clearRect: jest.fn(),
    fillRect: jest.fn(),
    // other canvas methods...
  };
  
  // Mock canvas element
  const mockCanvas = {
    getContext: jest.fn().mockReturnValue(mockContext),
    width: 300,
    height: 150,
  };
  
  // Mock useRef
  jest.spyOn(React, 'useRef').mockReturnValue({ current: mockCanvas });
  
  // Render component
  render(<StaticVisualization intensity={0.5} />);
  
  // Assert canvas operations
  expect(mockCanvas.getContext).toHaveBeenCalledWith('2d');
  expect(mockContext.clearRect).toHaveBeenCalled();
  expect(mockContext.fillRect).toHaveBeenCalled();
});
```

### E2E Testing

1. **Test DOM Interactions**
   - Use Playwright
   - Test user flows
   - Verify state changes
   - Test accessibility

2. **Test Canvas Rendering**
   - Take screenshots
   - Compare visual output
   - Test canvas parent elements
   - Verify canvas effects through DOM state

```typescript
// Example E2E test for hybrid component
test('radio tuner shows static visualization', async ({ page }) => {
  await page.goto('http://localhost:5173/radio');
  
  // Turn on radio
  await page.locator('[data-testid="power-button"]').click();
  
  // Take screenshot before tuning
  await page.screenshot({ path: 'e2e/screenshots/radio-static-before.png' });
  
  // Tune to a frequency
  await page.locator('[data-testid="frequency-slider"]').click();
  
  // Take screenshot after tuning
  await page.screenshot({ path: 'e2e/screenshots/radio-static-after.png' });
  
  // Visual comparison would be done separately
});
```

## Best Practices

1. **Clear Separation of Concerns**
   - DOM elements for UI and interaction
   - Canvas for visual effects
   - State management for game logic

2. **Accessibility First**
   - Provide alternatives for canvas content
   - Ensure keyboard navigation
   - Use ARIA attributes
   - Test with screen readers

3. **Performance Monitoring**
   - Monitor frame rates
   - Track memory usage
   - Optimize render cycles
   - Profile canvas operations

4. **Documentation**
   - Document component structure
   - Explain canvas operations
   - Provide usage examples
   - Document testing approach

## Examples

### Basic Radio Tuner

```jsx
function RadioTuner() {
  // State for UI
  const [frequency, setFrequency] = useState(90.0);
  const [isOn, setIsOn] = useState(false);
  const [signalStrength, setSignalStrength] = useState(0);
  
  // Refs for canvas
  const canvasRef = useRef(null);
  const animationRef = useRef(null);
  
  // Effect for canvas rendering
  useEffect(() => {
    if (!canvasRef.current || !isOn) return;
    
    const canvas = canvasRef.current;
    const ctx = canvas.getContext('2d');
    
    // Set canvas dimensions
    canvas.width = canvas.clientWidth;
    canvas.height = canvas.clientHeight;
    
    // Animation function
    const drawStatic = () => {
      // Clear canvas
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      
      // Draw static based on frequency and signal strength
      const intensity = 1 - signalStrength;
      
      for (let i = 0; i < canvas.width; i += 4) {
        for (let j = 0; j < canvas.height; j += 4) {
          const noise = Math.random() * intensity * 255;
          ctx.fillStyle = `rgba(${noise}, ${noise}, ${noise}, 0.5)`;
          ctx.fillRect(i, j, 4, 4);
        }
      }
      
      // Continue animation
      animationRef.current = requestAnimationFrame(drawStatic);
    };
    
    // Start animation
    animationRef.current = requestAnimationFrame(drawStatic);
    
    // Cleanup
    return () => {
      if (animationRef.current) {
        cancelAnimationFrame(animationRef.current);
      }
    };
  }, [isOn, signalStrength]);
  
  // Effect for signal detection
  useEffect(() => {
    if (!isOn) return;
    
    // Check for signals at current frequency
    const signal = findSignalAtFrequency(frequency);
    
    if (signal) {
      // Calculate signal strength
      const strength = calculateSignalStrength(frequency, signal);
      setSignalStrength(strength);
    } else {
      // No signal found
      setSignalStrength(0.1);
    }
  }, [isOn, frequency]);
  
  return (
    <div className="radio-tuner" data-testid="radio-tuner">
      <div className="frequency-display" data-testid="frequency-display">
        {frequency.toFixed(1)} MHz
      </div>
      
      <button 
        className="power-button"
        data-testid="power-button"
        onClick={() => setIsOn(!isOn)}
      >
        {isOn ? 'ON' : 'OFF'}
      </button>
      
      <div className="tuner-controls">
        <button 
          className="tune-button"
          data-testid="tune-down-button"
          onClick={() => setFrequency(prev => Math.max(88.0, prev - 0.1))}
          disabled={!isOn}
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
          disabled={!isOn}
        />
        
        <button 
          className="tune-button"
          data-testid="tune-up-button"
          onClick={() => setFrequency(prev => Math.min(108.0, prev + 0.1))}
          disabled={!isOn}
        >
          +0.1
        </button>
      </div>
      
      <div className="signal-strength-container">
        <div className="signal-strength-label">Signal Strength</div>
        <div className="signal-strength-meter">
          <div 
            className="signal-strength-fill"
            style={{ width: `${signalStrength * 100}%` }}
            data-testid="signal-strength"
          ></div>
        </div>
      </div>
      
      <canvas 
        ref={canvasRef}
        className="static-visualization"
        data-testid="static-canvas"
        aria-label="Static visualization"
      />
    </div>
  );
}
```

## Troubleshooting

### Common Issues

1. **Canvas Not Rendering**
   - Check if canvas ref is properly set
   - Verify canvas dimensions
   - Check if context is obtained correctly
   - Ensure animation frame is requested

2. **Performance Issues**
   - Reduce canvas size
   - Optimize drawing operations
   - Use appropriate data structures
   - Limit state updates

3. **Testing Issues**
   - Mock canvas context for unit tests
   - Use screenshots for E2E tests
   - Test DOM elements that control canvas
   - Verify state changes that affect canvas

4. **Memory Leaks**
   - Cancel animation frames on cleanup
   - Remove event listeners
   - Clean up canvas resources
   - Dispose of audio resources

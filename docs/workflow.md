# Development Workflow (Hybrid DOM + Canvas Approach with Alpha/Beta Agent Development)

> **Note**: This workflow document has been updated for the hybrid DOM + Canvas approach of Signal Lost with Alpha/Beta Agent Development. The previous Phaser-based workflow can be found in `workflow-phaser.md`.

## Pre-coding Checklist

1. Pull latest changes from the `develop` branch
2. Install/update dependencies: `npm install`
3. Start dev server: `npm run dev`
4. Open separate terminal for continuous testing: `npm test -- --watch`
5. Check the sprint document for your agent's responsibilities (Alpha or Beta)
6. Review interface contracts between agents
7. Initialize interface validation if needed: `npm run dev:validate-contracts`

## Alpha/Beta Agent Development

### Overview

Signal Lost development follows an Alpha/Beta Agent development approach where two AI agents collaborate with distinct roles and responsibilities:

1. **Agent Alpha**: Senior developer responsible for primary code development, including:
   - Writing and implementing new features
   - Unit and integration testing
   - Ensuring TypeScript type safety
   - Fixing lint and TypeScript errors
   - Adding data-testid attributes for E2E testing
   - Maintaining code quality and documentation

2. **Agent Beta**: QA developer responsible for quality assurance, including:
   - End-to-end (E2E) testing
   - Code cleanup and optimization
   - Removing unused code and variables
   - Improving code organization
   - Ensuring consistent code style

### Agent Collaboration

The agents collaborate through a structured development process:

- Agent Alpha leads development and implements new features
- Agent Beta focuses on testing and code quality
- Interface contracts are established to ensure smooth integration
- Agents communicate through GitHub issues and PRs
- Agent Beta reviews Agent Alpha's code for quality and test coverage
- Agent Alpha implements fixes based on Agent Beta's feedback

```typescript
// Example of interface contract between agents
// Shared interface used by both Alpha and Beta agents
interface Signal {
  id: string;
  frequency: number;
  strength: number;
  type: 'message' | 'location' | 'event';
  content: string;
  discovered: boolean;
  timestamp: number;
}

// Agent Alpha implements signal detection
function detectSignal(frequency: number): Signal | null {
  // Implementation by Agent Alpha
}

// Agent Beta consumes signals for narrative progression
function processSignal(signal: Signal): void {
  // Implementation by Agent Beta
}
```

## Hybrid DOM + Canvas Approach

Signal Lost uses a hybrid approach that combines DOM elements for interactive components with Canvas for visual effects. This approach provides the best balance of testability, performance, and maintainability.

### Key Principles

1. **DOM Elements for Interactive Parts**
   - Buttons, sliders, and displays as standard HTML elements
   - Fully testable with Playwright
   - Accessible and keyboard navigable
   - Styled with CSS for consistent appearance

2. **Canvas for Visual Effects**
   - Static visualization using canvas
   - Audio waveform display
   - Non-interactive visual elements
   - Performance-optimized rendering

3. **State Management**
   - React context or reducer for game state
   - Update DOM elements based on state
   - Canvas visualizations read from state but don't update it
   - Clear separation between UI logic and visual effects

### Example Implementation

```jsx
function RadioTuner() {
  // State managed outside rendering cycle
  const [frequency, setFrequency] = useState(90.0);
  const [isOn, setIsOn] = useState(false);
  const canvasRef = useRef(null);

  // Canvas for visual effects only
  useEffect(() => {
    if (!canvasRef.current || !isOn) return;

    const ctx = canvasRef.current.getContext('2d');

    // Draw static visualization
    const drawStatic = () => {
      // Visual-only code that doesn't affect state
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      // Draw static based on frequency...

      if (isOn) {
        requestAnimationFrame(drawStatic);
      }
    };

    requestAnimationFrame(drawStatic);

    return () => {
      // Cleanup
    };
  }, [isOn]);

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

      <input
        type="range"
        min="88.0"
        max="108.0"
        step="0.1"
        value={frequency}
        onChange={e => setFrequency(parseFloat(e.target.value))}
        data-testid="frequency-slider"
      />

      <canvas
        ref={canvasRef}
        className="static-visualization"
        data-testid="static-canvas"
      />
    </div>
  );
}
```

## Component Development Workflow

1. **Plan the component**:
   - Review the current sprint document for requirements
   - Define props and state requirements
   - Sketch the component structure
   - Identify potential reusable sub-components
   - Determine which parts should use DOM vs. Canvas

2. **Write tests first**:
   - Create test file with the same name as the component
   - Write tests for all expected behaviors
   - Include accessibility tests where appropriate
   - Add tests for both DOM elements and Canvas effects

3. **Implement the component**:
   - Create the component file
   - Implement DOM elements for interactive parts
   - Add Canvas for visual effects
   - Implement the component logic
   - Add styling
   - Ensure accessibility

4. **Verify**:
   - Run tests: `npm test -- --watch ComponentName`
   - Check TypeScript: `npm run type-check`
   - Run linter: `npm run lint`
   - Manually test in the browser
   - Verify Canvas rendering

5. **Document**:
   - Add JSDoc comments
   - Update component documentation
   - Add usage examples if needed
   - Update sprint document with progress

## Testing Approach

### Unit Tests (Jest + React Testing Library) - Agent Alpha Responsibility

- Test each component in isolation
- Focus on behavior, not implementation details
- Use `data-testid` attributes for element selection
- Mock external dependencies
- Maintain at least 80% test coverage

```jsx
// Example component test
import { render, screen, fireEvent } from '@testing-library/react';
import { RadioTuner } from './RadioTuner';

test('adjusts frequency when dial is moved', () => {
  // Arrange
  render(<RadioTuner initialFrequency={91.5} />);
  const dial = screen.getByTestId('frequency-dial');

  // Act
  fireEvent.mouseDown(dial);
  fireEvent.mouseMove(dial, { clientX: 100 });
  fireEvent.mouseUp(dial);

  // Assert
  expect(screen.getByTestId('frequency-display')).toHaveTextContent('92.5');
});
```

### Cross-Agent Integration Tests - Shared Responsibility

- Agent Alpha: Write integration tests for new features
- Agent Beta: Verify tests cover all edge cases
- Verify interface contracts are properly implemented
- Test boundary conditions and edge cases
- Ensure consistent behavior across components

```typescript
// Example Cross-Agent Integration test
import { render, act } from '@testing-library/react';
import { RadioTuner } from './components/RadioTuner'; // Alpha agent component
import { NarrativeDisplay } from './components/NarrativeDisplay'; // Beta agent component
import { GameStateProvider } from './state/GameStateProvider';

test('Radio tuner signal detection triggers narrative display update', () => {
  // Arrange
  const { getByTestId } = render(
    <GameStateProvider>
      <RadioTuner />
      <NarrativeDisplay />
    </GameStateProvider>
  );

  // Act
  act(() => {
    // Simulate tuning to a specific frequency
    const dial = getByTestId('frequency-dial');
    fireEvent.mouseDown(dial);
    fireEvent.mouseMove(dial, { clientX: 250 });
    fireEvent.mouseUp(dial);
  });

  // Assert
  expect(getByTestId('message-content')).toHaveTextContent('Signal detected');
});
```

### E2E Tests (Playwright) - Agent Beta Responsibility

- Test critical user flows
- Verify interactions between components
- Test in multiple browsers
- Capture screenshots for visual verification
- Ensure all components have proper data-testid attributes
- Run tests in headed mode for canvas elements
- Capture browser console logs for debugging

#### Testing Canvas Elements

- Use screenshots to verify canvas rendering
- Test DOM elements that control canvas behavior
- Verify state changes that affect canvas
- Use data-testid attributes on parent elements
- Run tests in headed mode for visual verification

```typescript
// Example E2E test for hybrid DOM + Canvas component
test('user can tune radio and receive signal', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173/radio');

  // Turn on radio (DOM element)
  await page.getByTestId('power-button').click();

  // Interact with frequency slider (DOM element)
  await page.getByTestId('frequency-slider').click();

  // Use tune buttons (DOM elements)
  await page.getByTestId('tune-up-button').click();

  // Verify frequency display (DOM element)
  await expect(page.getByTestId('frequency-display')).toHaveText('90.1 MHz');

  // Verify signal strength indicator (DOM element)
  await expect(page.getByTestId('signal-strength')).toHaveAttribute('style', /width: \d+%/);

  // Take screenshot to verify canvas rendering
  await page.screenshot({ path: 'e2e/screenshots/radio-tuner.png' });
});
```

## Pre-commit Checklist

```bash
# Run type checking
npm run type-check

# Run linter
npm run lint
npm run lint:tests

# Format code
npm run format

# Run tests
npm test
npm run test:alpha  # Run Alpha agent tests
npm run test:beta   # Run Beta agent tests
npm run test:cross-agent  # Run cross-agent integration tests
npm run test:e2e

# Check coverage
npm run coverage

# Run all checks at once
npm run check-all
```

## Debugging Tips

- Use React DevTools for component inspection
- Check browser console for errors
- Use Chrome DevTools for network and performance analysis
- Leverage React's StrictMode for detecting potential problems
- Monitor cross-agent integration with interface logging:
  ```typescript
  // Enable debug mode to log all cross-agent interactions
  Contracts.enableDebugMode();
  ```
- Use the debug helper script for streamlined development:
  ```bash
  # Start the debug helper menu
  powershell -File scripts/debug-helper.ps1
  ```

### Debug Helper Features

- Start development server and open browser
- Run tests in watch mode
- Run cross-agent integration tests
- Run E2E tests and visual tests
- Check for TypeScript errors
- Run linting
- Toggle interface contract validation
- Visualize cross-agent interactions
- Run Alpha-specific tests
- Run Beta-specific tests
- Start a complete development environment with all tools

## State Management

### General Principles

- Use React Context for global state
- Keep component state local when possible
- Use custom hooks to encapsulate state logic
- Follow unidirectional data flow

### Agent-Specific Responsibilities

#### Agent Alpha (Senior Developer)
- Implement new features and components
- Write unit and integration tests
- Fix TypeScript and lint errors
- Add data-testid attributes for E2E testing
- Maintain code quality and documentation
- Ensure type safety (avoid 'any' types except in tests)
- Create PRs for feature implementation

#### Agent Beta (QA Developer)
- Write and maintain E2E tests
- Clean up unused code and variables
- Improve code organization
- Ensure consistent code style
- Review Agent Alpha's PRs for quality
- Report issues and suggest improvements
- Create PRs for code cleanup and optimization

### Agent-Specific State Management

- Alpha agent manages audio and radio tuner state
- Beta agent manages narrative and game progression state
- Shared state is managed through well-defined interfaces
- Use TypeScript to enforce state boundaries between agents

```jsx
// Example state management with agent-specific contexts
import { createContext, useContext, useReducer } from 'react';

// Alpha Agent Context (Radio & Audio)
const RadioAudioStateContext = createContext();

export function RadioAudioStateProvider({ children }) {
  const [state, dispatch] = useReducer(radioAudioReducer, initialRadioAudioState);
  return (
    <RadioAudioStateContext.Provider value={{ state, dispatch }}>
      {children}
    </RadioAudioStateContext.Provider>
  );
}

export function useRadioAudioState() {
  const context = useContext(RadioAudioStateContext);
  if (!context) {
    throw new Error('useRadioAudioState must be used within a RadioAudioStateProvider');
  }
  return context;
}

// Beta Agent Context (Narrative & Game State)
const NarrativeStateContext = createContext();

export function NarrativeStateProvider({ children }) {
  const [state, dispatch] = useReducer(narrativeReducer, initialNarrativeState);
  return (
    <NarrativeStateContext.Provider value={{ state, dispatch }}>
      {children}
    </NarrativeStateContext.Provider>
  );
}

export function useNarrativeState() {
  const context = useContext(NarrativeStateContext);
  if (!context) {
    throw new Error('useNarrativeState must be used within a NarrativeStateProvider');
  }
  return context;
}

// Combined provider for convenience
export function GameStateProvider({ children }) {
  return (
    <RadioAudioStateProvider>
      <NarrativeStateProvider>
        {children}
      </NarrativeStateProvider>
    </RadioAudioStateProvider>
  );
}
```

## Development Priorities

1. **Sprint Goals**: Focus on completing the current sprint objectives
2. **Agent Responsibilities**:
   - Agent Alpha: Feature development, unit testing, and code quality
   - Agent Beta: E2E testing, code cleanup, and quality assurance
3. **Test Coverage**: Maintain at least 80% test coverage across all test domains
4. **Type Safety**: Use TypeScript effectively to prevent bugs (avoid 'any' types except in tests)
5. **Component Architecture**: Create a clean, maintainable component structure
6. **Accessibility**: Ensure the game is accessible to all users
7. **Code Quality**: Follow ESLint rules and maintain consistent code style
8. **Performance**: Optimize rendering and state updates
9. **Documentation**: Keep documentation up-to-date with implementation changes

## TypeScript Best Practices

- Define interfaces for all props
- Use discriminated unions for complex state
- Avoid `any` types
- Use generics for reusable components
- Create type guards for runtime type checking
- Define interface contracts between Alpha and Beta components
- Use namespaces to organize agent-specific types
- Create shared type definitions for cross-agent data
- Document interface boundaries clearly

```typescript
// Example of good TypeScript usage for components
interface RadioTunerProps {
  initialFrequency: number;
  minFrequency?: number;
  maxFrequency?: number;
  onFrequencyChange?: (frequency: number) => void;
  onSignalFound?: (signal: Signal) => void;
}

interface Signal {
  id: string;
  type: 'message' | 'location';
  strength: number;
  content: string;
}

export function RadioTuner({
  initialFrequency = 91.5,
  minFrequency = 88.0,
  maxFrequency = 108.0,
  onFrequencyChange,
  onSignalFound,
}: RadioTunerProps) {
  // Component implementation
}

// Example of interface contracts between Alpha and Beta agents
namespace Contracts {
  // Shared interfaces
  export interface Signal {
    id: string;
    frequency: number;
    strength: number;
    type: 'message' | 'location' | 'event';
    content: string;
    discovered: boolean;
    timestamp: number;
  }

  // Alpha agent interfaces (Radio & Audio)
  export namespace Alpha {
    export interface RadioTunerState {
      currentFrequency: number;
      isScanning: boolean;
      signalStrength: number;
      noiseLevel: number;
    }

    export interface AudioState {
      volume: number;
      isMuted: boolean;
      effectsEnabled: boolean;
    }
  }

  // Beta agent interfaces (Narrative & Game State)
  export namespace Beta {
    export interface NarrativeState {
      messages: Array<{
        id: string;
        content: string;
        decoded: boolean;
        timestamp: number;
      }>;
      currentMessageId: string | null;
    }

    export interface GameProgressState {
      discoveredSignals: string[];
      completedObjectives: string[];
      playerLocation: string;
      gameTime: number;
    }
  }
}

// Alpha agent implementation (Radio Tuner)
class RadioTuner {
  // Implementation by Alpha agent
  detectSignal(frequency: number): Contracts.Signal | null {
    // Signal detection logic
    return {
      id: 'signal-1',
      frequency: 91.5,
      strength: 0.75,
      type: 'message',
      content: 'Encrypted message content',
      discovered: true,
      timestamp: Date.now()
    };
  }
}

// Beta agent implementation (Narrative System)
class NarrativeSystem {
  // Implementation by Beta agent
  processSignal(signal: Contracts.Signal): void {
    // Process the signal for narrative progression
    console.log(`Processing signal: ${signal.id} at ${signal.frequency}MHz`);
  }
}
```

## Agent Synchronization Workflow

### Daily Sync Process
1. Start by updating develop:
   ```bash
   git checkout develop
   git pull origin develop
   ```

2. Create feature branch:
   ```bash
   # For Alpha agent
   git checkout -b feature/alpha/your-feature
   # For Beta agent
   git checkout -b feature/beta/your-feature
   ```

3. Before creating PR:
   ```bash
   git checkout your-feature-branch
   git fetch origin
   git rebase origin/develop
   ```

### Conflict Prevention
1. Check other agent's work:
   ```bash
   git fetch origin
   git checkout origin/feature/[alpha|beta]/*
   npm run check-all
   ```

2. Use interface validation:
   ```bash
   npm run dev:validate-contracts
   ```

### Merge Conflict Resolution
1. Both agents must use the contract validation tool
2. Conflicts in shared interfaces must be resolved via contract branches
3. Create a contract resolution branch:
   ```bash
   git checkout -b feature/contract/resolve-conflict
   ```

4. Both agents must approve contract resolution PRs

### PR Requirements
1. PR title format: `[Alpha|Beta] Feature description`
2. Required checks for Agent Alpha PRs:
   - All unit and integration tests passing
   - TypeScript type checking passing
   - ESLint checks passing
   - Proper data-testid attributes for E2E testing
   - No conflicts with develop
3. Required checks for Agent Beta PRs:
   - All E2E tests passing
   - Code cleanup and optimization complete
   - No unused variables or code
   - Consistent code style
   - No conflicts with develop

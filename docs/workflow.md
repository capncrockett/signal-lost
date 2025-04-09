# Development Workflow (React-Based Approach with Alpha/Beta Agent Development)

> **Note**: This workflow document has been updated for the React-based rewrite of Signal Lost with Alpha/Beta Agent Development approach. The previous Phaser-based workflow can be found in `workflow-phaser.md`.

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

Signal Lost development now follows an Alpha/Beta Agent development approach where two AI agents collaborate on different aspects of the codebase:

1. **Agent Alpha**: Responsible for radio tuner component and audio system implementation
2. **Agent Beta**: Responsible for narrative system and game state integration

### Agent Collaboration

The agents collaborate through a structured development process:

- Each agent works on separate, well-defined areas of the codebase
- Interface contracts are established to ensure smooth integration
- Agents communicate through GitHub issues and PRs
- Code reviews are performed across agent boundaries

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

## Component Development Workflow

1. **Plan the component**:
   - Review the current sprint document for requirements
   - Define props and state requirements
   - Sketch the component structure
   - Identify potential reusable sub-components

2. **Write tests first**:
   - Create test file with the same name as the component
   - Write tests for all expected behaviors
   - Include accessibility tests where appropriate

3. **Implement the component**:
   - Create the component file
   - Implement the component logic
   - Add styling
   - Ensure accessibility

4. **Verify**:
   - Run tests: `npm test -- --watch ComponentName`
   - Check TypeScript: `npm run type-check`
   - Run linter: `npm run lint`
   - Manually test in the browser

5. **Document**:
   - Add JSDoc comments
   - Update component documentation
   - Add usage examples if needed
   - Update sprint document with progress

## Testing Approach

### Unit Tests (Jest + React Testing Library)

- Test each component in isolation
- Focus on behavior, not implementation details
- Use `data-testid` attributes for element selection
- Mock external dependencies

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

### Cross-Agent Integration Tests

- Test integration between Alpha and Beta agent components
- Verify interface contracts are properly implemented
- Test boundary conditions and edge cases
- Ensure consistent behavior across agent boundaries

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

### E2E Tests (Playwright)

- Test critical user flows
- Verify interactions between components
- Test in multiple browsers
- Capture screenshots for visual verification

```typescript
// Example E2E test
test('user can tune radio and receive signal', async ({ page }) => {
  // Navigate to the game
  await page.goto('http://localhost:5173');

  // Interact with the radio
  await page.getByTestId('frequency-dial').click();
  await page.mouse.move(300, 300);
  await page.mouse.up();

  // Verify signal received
  await expect(page.getByTestId('signal-indicator')).toBeVisible();
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
2. **Agent Responsibilities**: Maintain clear separation of concerns between Alpha and Beta agents
3. **Interface Contracts**: Ensure well-defined interfaces between agent components
4. **Component Architecture**: Create a clean, maintainable component structure
5. **Cross-Agent Integration**: Ensure smooth integration between Alpha and Beta components
6. **Accessibility**: Ensure the game is accessible to all users
7. **Test Coverage**: Maintain high test coverage for all components and cross-agent interactions
8. **Type Safety**: Use TypeScript effectively to prevent bugs
9. **Performance**: Optimize rendering and state updates

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
2. Required checks before merge:
   - All tests passing
   - Contract validation passing
   - Other agent's approval
   - No conflicts with develop

# Development Workflow (React-Based Approach with Two Agent Architecture)

> **Note**: This workflow document has been updated for the React-based rewrite of Signal Lost with Two Agent Architecture. The previous Phaser-based workflow can be found in `workflow-phaser.md`.

## Pre-coding Checklist

1. Pull latest changes from the `develop` branch
2. Install/update dependencies: `npm install`
3. Start dev server: `npm run dev`
4. Open separate terminal for continuous testing: `npm test -- --watch`
5. Initialize agent debug mode if needed: `npm run dev:agent-debug`

## Two Agent Architecture

### Overview

Signal Lost now implements a Two Agent architecture where separate agents handle different aspects of the game:

1. **Navigation Agent**: Responsible for player movement, environment interaction, and spatial awareness
2. **Communication Agent**: Handles radio communication, signal processing, and message decoding

### Agent Communication

Agents communicate through a shared event system:

- Use the `AgentEventBus` for inter-agent communication
- Events should be typed and documented
- Maintain clear boundaries between agent responsibilities

```typescript
// Example of agent communication
import { AgentEventBus } from './agents/AgentEventBus';

// Navigation agent discovers a radio
AgentEventBus.emit('ITEM_DISCOVERED', {
  type: 'radio',
  location: { x: 120, y: 85 },
  interactive: true
});

// Communication agent listens for discoveries
AgentEventBus.on('ITEM_DISCOVERED', (item) => {
  if (item.type === 'radio') {
    // Enable radio functionality
  }
});
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

### Agent Integration Tests

- Test communication between agents
- Verify event handling and state updates
- Mock agent dependencies
- Test boundary conditions

```typescript
// Example Agent Integration test
import { render, act } from '@testing-library/react';
import { AgentEventBus } from './agents/AgentEventBus';
import { NavigationAgent } from './agents/NavigationAgent';
import { CommunicationAgent } from './agents/CommunicationAgent';
import { GameStateProvider } from './state/GameStateProvider';

test('Navigation agent discovery triggers Communication agent response', () => {
  // Arrange
  const { getByTestId } = render(
    <GameStateProvider>
      <NavigationAgent />
      <CommunicationAgent />
    </GameStateProvider>
  );

  // Act
  act(() => {
    AgentEventBus.emit('ITEM_DISCOVERED', {
      type: 'radio',
      location: { x: 120, y: 85 },
      interactive: true
    });
  });

  // Assert
  expect(getByTestId('radio-interface')).toBeInTheDocument();
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
npm run test:agent-integration  # Run agent integration tests
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
- Monitor agent communication with AgentEventBus debug mode:
  ```typescript
  // Enable debug mode to log all events
  AgentEventBus.enableDebugMode();
  ```
- Use the debug helper script for streamlined development:
  ```bash
  # Start the debug helper menu
  powershell -File scripts/debug-helper.ps1
  ```

### Debug Helper Features

- Start development server and open browser
- Run tests in watch mode
- Run agent integration tests
- Run E2E tests and visual tests
- Check for TypeScript errors
- Run linting
- Toggle agent debug mode
- Visualize agent communication
- Start a complete development environment with all tools

## State Management

### General Principles

- Use React Context for global state
- Keep component state local when possible
- Use custom hooks to encapsulate state logic
- Follow unidirectional data flow

### Agent-Specific State Management

- Each agent maintains its own state slice
- Use separate contexts for Navigation and Communication agents
- Shared state should be managed through the AgentEventBus
- Use TypeScript to enforce state boundaries

```jsx
// Example state management with agent-specific contexts
import { createContext, useContext, useReducer } from 'react';

// Navigation Agent Context
const NavigationStateContext = createContext();

export function NavigationStateProvider({ children }) {
  const [state, dispatch] = useReducer(navigationReducer, initialNavigationState);
  return (
    <NavigationStateContext.Provider value={{ state, dispatch }}>
      {children}
    </NavigationStateContext.Provider>
  );
}

export function useNavigationState() {
  const context = useContext(NavigationStateContext);
  if (!context) {
    throw new Error('useNavigationState must be used within a NavigationStateProvider');
  }
  return context;
}

// Communication Agent Context
const CommunicationStateContext = createContext();

export function CommunicationStateProvider({ children }) {
  const [state, dispatch] = useReducer(communicationReducer, initialCommunicationState);
  return (
    <CommunicationStateContext.Provider value={{ state, dispatch }}>
      {children}
    </CommunicationStateContext.Provider>
  );
}

export function useCommunicationState() {
  const context = useContext(CommunicationStateContext);
  if (!context) {
    throw new Error('useCommunicationState must be used within a CommunicationStateProvider');
  }
  return context;
}

// Combined provider for convenience
export function GameStateProvider({ children }) {
  return (
    <NavigationStateProvider>
      <CommunicationStateProvider>
        {children}
      </CommunicationStateProvider>
    </NavigationStateProvider>
  );
}
```

## Development Priorities

1. **Sprint Goals**: Focus on completing the current sprint objectives
2. **Agent Architecture**: Maintain clear separation of concerns between agents
3. **Component Architecture**: Create a clean, maintainable component structure
4. **Inter-Agent Communication**: Ensure robust and type-safe event handling
5. **Accessibility**: Ensure the game is accessible to all users
6. **Test Coverage**: Maintain high test coverage for all components and agent interactions
7. **Type Safety**: Use TypeScript effectively to prevent bugs
8. **Performance**: Optimize rendering and state updates

## TypeScript Best Practices

- Define interfaces for all props
- Use discriminated unions for complex state
- Avoid `any` types
- Use generics for reusable components
- Create type guards for runtime type checking
- Define event types for agent communication
- Use namespaces to organize agent-specific types
- Create shared type definitions for cross-agent data

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

// Example of agent event types
namespace AgentEvents {
  // Base event interface
  export interface BaseEvent {
    timestamp: number;
    source: 'navigation' | 'communication';
  }

  // Navigation agent events
  export interface ItemDiscoveredEvent extends BaseEvent {
    type: 'ITEM_DISCOVERED';
    payload: {
      itemType: string;
      location: { x: number; y: number };
      interactive: boolean;
    };
  }

  // Communication agent events
  export interface SignalDetectedEvent extends BaseEvent {
    type: 'SIGNAL_DETECTED';
    payload: {
      frequency: number;
      strength: number;
      message?: string;
    };
  }

  // Union type of all events
  export type AgentEvent = ItemDiscoveredEvent | SignalDetectedEvent;
}

// Type-safe event bus usage
AgentEventBus.emit<AgentEvents.ItemDiscoveredEvent>('ITEM_DISCOVERED', {
  timestamp: Date.now(),
  source: 'navigation',
  type: 'ITEM_DISCOVERED',
  payload: {
    itemType: 'radio',
    location: { x: 120, y: 85 },
    interactive: true
  }
});
```

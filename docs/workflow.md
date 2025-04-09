# Development Workflow (React-Based Approach)

> **Note**: This workflow document has been updated for the React-based rewrite of Signal Lost. The previous Phaser-based workflow can be found in `workflow-phaser.md`.

## Pre-coding Checklist

1. Pull latest changes from the `develop` branch
2. Install/update dependencies: `npm install`
3. Start dev server: `npm run dev`
4. Open separate terminal for continuous testing: `npm test -- --watch`

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
- Use the debug helper script for streamlined development:
  ```bash
  # Start the debug helper menu
  powershell -File scripts/debug-helper.ps1
  ```

### Debug Helper Features

- Start development server and open browser
- Run tests in watch mode
- Run E2E tests and visual tests
- Check for TypeScript errors
- Run linting
- Start a complete development environment with all tools

## State Management

- Use React Context for global state
- Keep component state local when possible
- Use custom hooks to encapsulate state logic
- Follow unidirectional data flow

```jsx
// Example state management with context
import { createContext, useContext, useReducer } from 'react';

// Create context
const GameStateContext = createContext();

// Create provider
export function GameStateProvider({ children }) {
  const [state, dispatch] = useReducer(gameReducer, initialState);
  return (
    <GameStateContext.Provider value={{ state, dispatch }}>
      {children}
    </GameStateContext.Provider>
  );
}

// Custom hook for using the context
export function useGameState() {
  const context = useContext(GameStateContext);
  if (!context) {
    throw new Error('useGameState must be used within a GameStateProvider');
  }
  return context;
}
```

## Development Priorities

1. **Sprint Goals**: Focus on completing the current sprint objectives
2. **Component Architecture**: Create a clean, maintainable component structure
3. **Accessibility**: Ensure the game is accessible to all users
4. **Test Coverage**: Maintain high test coverage for all components
5. **Type Safety**: Use TypeScript effectively to prevent bugs
6. **Performance**: Optimize rendering and state updates

## TypeScript Best Practices

- Define interfaces for all props
- Use discriminated unions for complex state
- Avoid `any` types
- Use generics for reusable components
- Create type guards for runtime type checking

```typescript
// Example of good TypeScript usage
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
```

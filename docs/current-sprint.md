# Sprint 2: Cleanup and Maintenance

## Goals
This sprint focuses on cleaning up the codebase, improving test infrastructure, and addressing critical TypeScript errors to ensure a more maintainable and stable codebase moving forward.

## Current Sprint Priorities

1. Fix critical TypeScript errors in core game files
2. Ensure all tests pass consistently
3. Clean up unused code and imports
4. Address remaining TypeScript errors in test files

## Code Cleanup

- ⬜ Fix TypeScript errors and warnings
  - ✅ Fix ESLint errors in core components
  - ✅ Add type declarations for external libraries
  - ✅ Create proper interfaces for signal data types
  - ✅ Create proper interfaces for event data types
  - ⬜ Fix remaining TypeScript errors in core files
  - ⬜ Fix TypeScript errors in test files

- ⬜ Remove unused code
  - ⬜ Delete dead code paths
  - ⬜ Remove commented-out code blocks
  - ✅ Clean up unused imports and variables

## Test Infrastructure

- ✅ Fix failing tests
  - ✅ Update tests to account for new components
  - ✅ Fix mocks for components that have changed
  - ✅ Address environment-specific test failures
  - ✅ Fix SoundscapeManager test issues
  - ✅ Fix AudioSystem integration tests

- ✅ Improve test coverage
  - ✅ Add tests for untested components
  - ✅ Increase coverage for critical paths
  - ✅ Add edge case testing

- ✅ Standardize test patterns
  - ✅ Create consistent mocking approach
  - ✅ Standardize test setup and teardown
  - ✅ Improve test readability and maintainability

## Environment Cleanup

- ✅ Fix build process
  - ✅ Address ESM/CommonJS module issues
  - ✅ Optimize build configuration
  - ✅ Ensure consistent builds across environments

- ⬜ Improve development workflow
  - ✅ Update workflow documentation to emphasize TypeScript checking
  - ✅ Add TypeScript error management guidelines
  - ⬜ Streamline local development setup
  - ⬜ Enhance debugging capabilities

## Deferred to Future Sprint

The following items have been moved to a future sprint to focus on critical functionality first:

- Refactor duplicated code
- Performance optimization
- Comprehensive documentation updates
- Accessibility improvements
- Dependency updates (unless critical security issues)

See [sprint-3.md](sprint-3.md) for details on these deferred tasks.

## Progress Report

### Completed Tasks

- Fixed ESLint errors in core components
- Added type declarations for external libraries (pngjs, pixelmatch)
- Created proper interfaces for signal data types (SignalLockData, LocationSignalData, etc.)
- Created proper interfaces for event data types (NarrativeEventData, NarrativeChoiceResultData, etc.)
- Fixed all failing tests
- Improved test coverage for critical components
- Standardized test patterns and mocking approaches
- Fixed build process issues
- Updated workflow documentation to emphasize TypeScript checking
- Added TypeScript error management guidelines
- Fixed TestOverlay.test.ts TypeScript errors
- Cleaned up unused imports and variables

### In Progress

- Fixing remaining TypeScript errors in core files
- Addressing TypeScript errors in test files
- Removing dead code paths and commented-out code

### Remaining TypeScript Errors

The following files have critical TypeScript errors that need to be addressed:

1. **Core Game Files**
   - src/components/AudioVisualizer.ts
   - src/components/PerformanceDisplay.ts
   - src/components/RadioTuner.ts
   - src/scenes/field/FieldScene.ts
   - src/scenes/field/GridSystem.ts
   - src/scenes/field/Interactable.ts
   - src/scenes/field/Player.ts
   - src/scenes/MainScene.ts
   - src/utils/SaveManager.ts
   - src/utils/TestOverlay.ts

2. **Test Files**
   - Multiple E2E test files have TypeScript errors related to screenshotName property
   - Several test mock files have type compatibility issues

### Next Steps

1. Focus on fixing TypeScript errors in core game files first
2. Then address TypeScript errors in test files
3. Remove unused code and commented-out blocks
4. Complete remaining development workflow improvements

## Common TypeScript Errors and Solutions

### 1. Non-null Assertions (`!`)

**Problem**: Using the non-null assertion operator (`!`) is unsafe and can lead to runtime errors.

**Solution**: Replace with proper null checking:

```typescript
// Before
this.masterGain!.connect(this.audioContext.destination);

// After
if (this.masterGain) {
  this.masterGain.connect(this.audioContext.destination);
}
```

### 2. Unsafe Member Access

**Problem**: Accessing properties that might be undefined or null.

**Solution**: Add proper type guards or optional chaining:

```typescript
// Before
const tile = obstaclesLayer.getTileAt(x, y);
if (tile && tile.properties.collides) { ... }

// After
const tile = obstaclesLayer.getTileAt(x, y);
if (tile && tile.properties && tile.properties.collides) { ... }
// Or with optional chaining
if (tile?.properties?.collides) { ... }
```

### 3. Missing Return Types

**Problem**: Functions without explicit return types can lead to unexpected behavior.

**Solution**: Add explicit return types to all functions:

```typescript
// Before
private createBlip() {
  // function body
}

// After
private createBlip(): void {
  // function body
}
```

### 4. Unsafe Type Assertions

**Problem**: Using `as` to assert types without proper checking.

**Solution**: Use type guards or conditional checks:

```typescript
// Before
const data = JSON.parse(savedState) as SavedState;

// After
const parsedData = JSON.parse(savedState);
if (isSavedState(parsedData)) {
  const data: SavedState = parsedData;
  // Use data safely
}

// Type guard function
function isSavedState(data: unknown): data is SavedState {
  return data !== null && typeof data === 'object' && 'flags' in data;
}
```

### 5. Implicit `any` Types

**Problem**: Variables with implicit `any` types can cause type safety issues.

**Solution**: Add explicit type annotations:

```typescript
// Before
const result = someFunction();

// After
const result: SomeType = someFunction();
```

### 6. Test File Errors

**Problem**: Test files often use mocks that don't match the expected interfaces.

**Solution**: For test files, use `any` types judiciously or create proper mock interfaces:

```typescript
// Before
const mockDiv: HTMLDivElement = {
  style: {}, // Error: Type '{}' is missing properties from 'CSSStyleDeclaration'
};

// After (Option 1 - using any)
const mockDiv: any = {
  style: {},
};

// After (Option 2 - proper mock)
const mockDiv = {
  style: {
    position: '',
    top: '',
    // Add other needed properties
  } as CSSStyleDeclaration,
};
```

## Tracking Progress

### TypeScript Error Count

We're tracking the number of TypeScript errors to measure our progress:

| Date       | Total Errors | Core Files | Test Files | Notes                                |
|------------|--------------|------------|------------|--------------------------------------|
| 2023-07-10 | 119          | 47         | 72         | Initial count                        |
| 2023-07-11 | 98           | 35         | 63         | Fixed TestOverlay.test.ts errors    |
| 2023-07-12 | 82           | 28         | 54         | Fixed signal data type errors        |

### Priority Files

We're focusing on fixing errors in these files first:

1. src/scenes/MainScene.ts - Core game initialization
2. src/scenes/field/FieldScene.ts - Main gameplay area
3. src/components/RadioTuner.ts - Critical gameplay mechanic
4. src/utils/SaveManager.ts - Game state persistence

## Conclusion

By focusing on fixing critical TypeScript errors first, we're establishing a solid foundation for the game. Once these errors are addressed, we'll have a more stable and maintainable codebase that will make future development easier and more efficient.

The next sprint will build on this foundation to refactor duplicated code, optimize performance, and improve the overall architecture of the game.

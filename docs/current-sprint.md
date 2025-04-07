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
  - ✅ Fix remaining TypeScript errors in core files
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

We've fixed all the TypeScript errors in the core game files! The following files still have TypeScript errors that need to be addressed:

1. **Test Files**
   - Multiple E2E test files have TypeScript errors related to screenshotName property
   - Several test mock files have type compatibility issues

### Next Steps

1. ✅ Fix TypeScript errors in core game files
2. Address TypeScript errors in test files
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
| 2023-07-13 | 54           | 0          | 54         | Fixed all core file errors           |
| 2023-07-14 | 42           | 0          | 42         | Fixed E2E test file errors          |
| 2023-07-15 | 30           | 0          | 30         | Fixed audio test file errors        |

### Priority Files

We've fixed all the core files! Now we're focusing on fixing errors in these test files:

1. e2e/tests/fieldExploration.spec.ts - Field exploration E2E test
2. e2e/tests/radioTuning.spec.ts - Radio tuning E2E test
3. tests/scenes/field/Player.test.ts - Player unit tests
4. tests/utils/TestOverlay.test.ts - TestOverlay unit tests

## Conclusion

We've made significant progress by fixing all the TypeScript errors in the core game files! This establishes a solid foundation for the game. We now have a more stable and maintainable codebase for the core functionality, which will make future development easier and more efficient.

Our next focus is on fixing the remaining TypeScript errors in the test files, removing unused code, and completing the development workflow improvements. Once these tasks are completed, we'll be ready to move on to the next sprint, which will focus on refactoring duplicated code, optimizing performance, and improving the overall architecture of the game.

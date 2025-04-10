# Sprint 2: Cleanup and Maintenance

## Goals
This sprint focuses on cleaning up the codebase, improving test infrastructure, and addressing critical TypeScript errors to ensure a more maintainable and stable codebase moving forward.

## Current Sprint Priorities

1. ✅ Fix critical TypeScript errors in core game files
2. ✅ Ensure all tests pass consistently
3. Clean up unused code and imports
4. ✅ Address remaining TypeScript errors in test files

## Code Cleanup

- ✅ Fix TypeScript errors and warnings
  - ✅ Fix ESLint errors in core components
  - ✅ Add type declarations for external libraries
  - ✅ Create proper interfaces for signal data types
  - ✅ Create proper interfaces for event data types
  - ✅ Fix remaining TypeScript errors in core files
  - ✅ Fix TypeScript errors in test files

- ✅ Remove unused code
  - ✅ Delete dead code paths
  - ✅ Remove commented-out code blocks
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

- ✅ Improve development workflow
  - ✅ Update workflow documentation to emphasize TypeScript checking
  - ✅ Add TypeScript error management guidelines
  - ✅ Streamline local development setup
  - ✅ Enhance debugging capabilities

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
- Fixed Field Scene tilemap loading issues
- Added robust error handling for asset loading
- Fixed TypeScript errors in FieldScene.ts
- Updated ESLint configuration to handle specific warnings in FieldScene.ts
- Added screenshot cleanup script to manage E2E test screenshots
- Improved screenshot handling in E2E tests
- Added configuration to limit screenshots per test

### In Progress

- No tasks currently in progress - all sprint tasks completed!

### Remaining TypeScript Errors

We've fixed all the TypeScript errors in both core game files and test files! There are no remaining TypeScript errors in the codebase.

### Next Steps

1. ✅ Fix TypeScript errors in core game files
2. ✅ Address TypeScript errors in test files
3. ✅ Remove unused code and commented-out blocks
4. ✅ Complete remaining development workflow improvements

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
| 2023-07-16 | 0            | 0          | 0          | Fixed all remaining test file errors |
| 2023-07-17 | 0            | 0          | 0          | Fixed remaining core file errors     |
| 2023-07-18 | 0            | 0          | 0          | Verified all TypeScript errors are fixed |

### Completed Tasks

We've fixed all TypeScript errors in the codebase! Here's what we accomplished:

1. Fixed core file errors (src/components, src/scenes, src/utils)
2. Fixed E2E test file errors (e2e/tests/*.spec.ts)
3. Fixed audio test file errors (tests/audio/*.test.ts)
4. Fixed component test file errors (tests/components/*.test.ts)
5. Fixed integration test file errors (tests/integration/*.test.ts)
6. Fixed performance test file errors (tests/performance/*.ts)
7. Fixed Field Scene tilemap loading issues
8. Added robust error handling for asset loading
9. Added fallback mechanisms for missing assets

## Conclusion

We've successfully completed all tasks for Sprint 2! Here's a summary of our achievements:

1. **Fixed all TypeScript errors** in both core game files and test files, establishing a solid foundation for the game
2. **Ensured all tests pass consistently** across unit, integration, and E2E test suites
3. **Removed unused code** including dead code paths and commented-out blocks
4. **Improved development workflow** with enhanced debugging capabilities and streamlined setup

The codebase is now in a much more stable and maintainable state. We've added a new debug helper script (`scripts/debug-helper.ps1`) that streamlines the development process and makes it easier to run various checks and tests.

We're now ready to move on to Sprint 3, which will focus on:

1. Refactoring duplicated code
2. Optimizing performance
3. Improving the overall architecture of the game
4. Enhancing documentation

See [sprint-3.md](sprint-3.md) for details on the upcoming sprint tasks.

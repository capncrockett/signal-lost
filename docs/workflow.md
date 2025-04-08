# Development Workflow

## Pre-coding Checklist

1. Pull latest changes
2. Install/update dependencies: `npm install`
3. Start dev server: `npm run dev`
4. Open separate terminal for continuous testing: `npm test -- --watch`

## During Development

1. Write tests first
2. Implement feature
3. **Run TypeScript compiler first**: `npm run type-check` or `npx tsc --noEmit`
   - Type checking is your first layer of defense
   - Fix all TypeScript errors before proceeding
   - Never ignore TypeScript errors (e.g., uninitialized properties)
4. **Run linter next**: `npm run lint` or `npx eslint . --ext .ts --fix`
   - Linting is your second layer of testing
   - Fix all ESLint errors and warnings
5. Run full test suite: `npm run test`
6. Run e2e tests: `npm run test:e2e`
7. Format code: `npm run format`

## Pre-commit Checklist

```bash
# ALWAYS run type checking first - this is your first layer of defense
npm run type-check  # Check for TypeScript errors (or npx tsc --noEmit)

# THEN run linter - this is your second layer of testing
npm run lint        # Check for lint issues (or npx eslint . --ext .ts --fix)
npm run lint:tests  # Check for lint issues in test files

# Then proceed with other checks
npm run format     # Format all files
npm test          # Run unit tests
npm run test:e2e  # Run e2e tests
npm run coverage  # Verify coverage thresholds

# Or run everything at once
npm run check-all  # Run all checks (type-check, lint, tests, e2e tests)
```

## Debugging Tips

- Check browser console for errors
- Use Chrome DevTools Audio tab for volume issues
- Verify viewport sizing with browser responsive mode
- Use the debug helper script for streamlined development:
  ```bash
  # Start the debug helper menu
  powershell -File scripts/debug-helper.ps1
  ```

### Debug Helper Features

- Start development server and open browser
- Run tests in watch mode
- Run E2E tests and visual tests
- Clean up screenshots
- Check for TypeScript errors
- Run linting
- Start a complete development environment with all tools

## Development Priorities

1. **Functionality First**: Focus on making the game work from start to finish
2. **Type Safety**: Fix TypeScript errors to ensure code quality and prevent bugs
3. **Test Coverage**: Ensure all critical paths are tested
4. **Code Cleanup**: Refactor and clean up code once functionality is stable
5. **Performance Optimization**: Address performance issues after the game is working properly

## TypeScript Error Management

- Run `npm run type-check` frequently during development
- Fix TypeScript errors as soon as they appear
- For test files, use `any` types judiciously when mocking complex objects
- For source files, avoid using `any` types whenever possible
- When fixing a large number of TypeScript errors:
  1. Start with errors in core game files
  2. Then fix errors in utility functions
  3. Finally address errors in test files

# Development Workflow

## Pre-coding Checklist
1. Pull latest changes
2. Install/update dependencies: `npm install`
3. Start dev server: `npm run dev`
4. Open separate terminal for continuous testing: `npm test -- --watch`

## During Development
1. Write tests first
2. Implement feature
3. Run full test suite: `npm run test`
4. Run e2e tests: `npm run test:e2e`
5. Fix lint issues: `npm run lint:fix`
6. Format code: `npm run format`

## Pre-commit Checklist
```bash
npm run lint        # Check for lint issues
npm run format     # Format all files
npm test          # Run unit tests
npm run test:e2e  # Run e2e tests
npm run coverage  # Verify coverage thresholds
```

## Debugging Tips
- Check browser console for errors
- Use Chrome DevTools Audio tab for volume issues
- Verify viewport sizing with browser responsive mode
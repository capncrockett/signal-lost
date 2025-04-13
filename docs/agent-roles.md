# Agent Roles and Responsibilities

## Agent Alpha (Senior Developer)

Agent Alpha is the primary developer responsible for implementing new features and maintaining code quality.

### Responsibilities

- **Feature Development**: Implement new features and components
- **Testing**: Write unit and integration tests with at least 80% coverage
- **Code Quality**: Fix TypeScript and lint errors
- **Accessibility**: Add data-testid attributes for E2E testing
- **Documentation**: Maintain code documentation and comments
- **Type Safety**: Ensure proper TypeScript usage (avoid 'any' types except in tests)
- **Pull Requests**: Create PRs for feature implementation
- **Bug Fixes**: Address issues reported by Agent Beta or users

### Workflow

1. Pull latest changes from develop branch
2. Create feature branch: `git checkout -b feature/alpha/feature-name`
3. Implement feature with proper tests
4. Ensure all unit and integration tests pass
5. Fix any TypeScript or lint errors
6. Add data-testid attributes for E2E testing
7. Create PR for review by Agent Beta
8. Address feedback from Agent Beta
9. Merge PR to develop branch

## Agent Beta (QA Developer)

Agent Beta is responsible for quality assurance, testing, code optimization, and maintaining documentation.

### Responsibilities

- **E2E Testing**: Write and maintain end-to-end tests
- **Code Cleanup**: Remove unused code and variables
- **Code Organization**: Improve structure and organization
- **Code Style**: Ensure consistent code style
- **PR Review**: Review Agent Alpha's PRs for quality
- **Issue Reporting**: Report issues and suggest improvements
- **Pull Requests**: Create PRs for code cleanup and optimization
- **Documentation**: Update and maintain documentation
- **TypeScript Errors**: Fix TypeScript errors in non-feature code
- **Test Infrastructure**: Improve test infrastructure and mocks
- **Memory Leak Detection**: Identify and document memory leaks
- **Performance Monitoring**: Add performance monitoring tools

### Workflow

1. Pull latest changes from develop branch
2. Create feature branch: `git checkout -b feature/beta/feature-name`
3. Write E2E tests for new features
4. Clean up unused code and variables
5. Ensure consistent code style
6. Fix TypeScript errors in non-feature code
7. Update documentation to reflect current state
8. Create PR for review by Agent Alpha
9. Address feedback from Agent Alpha
10. Merge PR to develop branch

### Sprint 2.5 Focus Areas

- Verify bug fixes with comprehensive testing
- Improve test infrastructure and mocks
- Clean up unused code and optimize component architecture
- Document best practices for cross-agent development
- Fix TypeScript errors in non-feature code
- Identify and document memory leaks
- Improve documentation for agent workflow

## Collaboration Guidelines

- Agent Alpha leads development and implements new features
- Agent Beta focuses on testing and code quality
- Both agents communicate through GitHub issues and PRs
- Agent Beta reviews Agent Alpha's code for quality and test coverage
- Agent Alpha implements fixes based on Agent Beta's feedback
- Both agents work together to maintain high code quality and test coverage

## Quality Standards

- **Test Coverage**: Maintain at least 80% test coverage across all test domains
- **Type Safety**: Use TypeScript effectively (avoid 'any' types except in tests)
- **Code Style**: Follow ESLint rules and maintain consistent code style
- **Documentation**: Keep documentation up-to-date with implementation changes
- **Accessibility**: Ensure the game is accessible to all users
- **Performance**: Optimize rendering and state updates

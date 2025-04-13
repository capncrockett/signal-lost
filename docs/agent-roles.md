# Agent Roles and Responsibilities

## Agent Alpha (Senior Developer)

Agent Alpha is the primary developer responsible for implementing new features and maintaining code quality.

### Responsibilities

- **Feature Development**: Implement new features and components
- **Testing**: Write unit and integration tests with at least 80% coverage
- **Code Quality**: Fix C# errors and bugs
- **Documentation**: Maintain code documentation and comments
- **Type Safety**: Ensure proper C# type safety
- **Pull Requests**: Create PRs for feature implementation
- **Bug Fixes**: Address issues reported by Agent Beta or users

### Workflow

1. Pull latest changes from develop branch
2. Create feature branch: `git checkout -b feature/alpha/feature-name`
3. Implement feature with proper tests
4. Ensure all unit and integration tests pass
5. Fix any C# compilation errors
6. Create PR for review by Agent Beta
7. Address feedback from Agent Beta
8. Merge PR to develop branch

## Agent Beta (QA Developer)

Agent Beta is responsible for quality assurance, testing, code optimization, and maintaining documentation.

### Responsibilities

- **Manual Testing**: Write and maintain manual test procedures
- **Code Cleanup**: Remove unused code and variables
- **Code Organization**: Improve structure and organization
- **Code Style**: Ensure consistent code style
- **PR Review**: Review Agent Alpha's PRs for quality
- **Issue Reporting**: Report issues and suggest improvements
- **Pull Requests**: Create PRs for code cleanup and optimization
- **Documentation**: Update and maintain documentation
- **C# Errors**: Fix C# errors in non-feature code
- **Test Infrastructure**: Improve test infrastructure
- **Memory Leak Detection**: Identify and document memory leaks
- **Performance Monitoring**: Add performance monitoring tools

### Workflow

1. Pull latest changes from develop branch
2. Create feature branch: `git checkout -b feature/beta/feature-name`
3. Write manual tests for new features
4. Clean up unused code and variables
5. Ensure consistent code style
6. Fix C# errors in non-feature code
7. Update documentation to reflect current state
8. Create PR for review by Agent Alpha
9. Address feedback from Agent Alpha
10. Merge PR to develop branch

### Sprint 3 Focus Areas

- Verify bug fixes with comprehensive testing
- Improve test infrastructure
- Clean up unused code and optimize node architecture
- Document best practices for cross-agent development
- Fix C# errors in non-feature code
- Identify and document memory leaks
- Improve documentation for agent workflow
- Migrate remaining game concepts to Godot

## Collaboration Guidelines

- Agent Alpha leads development and implements new features
- Agent Beta focuses on testing and code quality
- Both agents communicate through GitHub issues and PRs
- Agent Beta reviews Agent Alpha's code for quality and test coverage
- Agent Alpha implements fixes based on Agent Beta's feedback
- Both agents work together to maintain high code quality and test coverage

## Quality Standards

- **Test Coverage**: Maintain at least 80% test coverage across all test domains
- **Type Safety**: Use C# strong typing effectively
- **Code Style**: Follow C# coding conventions and Godot C# style guide
- **Documentation**: Keep documentation up-to-date with implementation changes
- **Accessibility**: Ensure the game is accessible to all users
- **Performance**: Optimize rendering and state updates
- **Memory Management**: Properly manage resources to avoid memory leaks

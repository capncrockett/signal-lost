# Contributing to Signal Lost

Thank you for your interest in contributing to Signal Lost! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Documentation](#documentation)
- [Asset Contributions](#asset-contributions)

## Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment for everyone. Please be considerate of differing viewpoints and experiences, and focus on constructive feedback and collaboration.

## Getting Started

### Prerequisites

- Node.js (v14 or higher)
- npm (v7 or higher)
- A modern web browser (Chrome, Firefox, Edge)

### Setup

1. Fork the repository on GitHub
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR-USERNAME/signal-lost.git
   cd signal-lost
   ```
3. Install dependencies:
   ```bash
   npm install
   ```
4. Start the development server:
   ```bash
   npm run dev
   ```
5. Open your browser to `http://localhost:5173`

## Development Workflow

### Agent-Specific Workflow

This project uses a dual-agent (Alpha/Beta) development approach. Follow these steps:

1. Sync with develop branch:
   ```bash
   git checkout develop
   git pull origin develop
   ```

2. Create your feature branch using the correct prefix:
   ```bash
   # For Alpha agent
   git checkout -b feature/alpha/your-feature
   # For Beta agent
   git checkout -b feature/beta/your-feature
   # For interface contracts
   git checkout -b feature/contract/your-change
   ```

3. Before submitting PR:
   - Sync with develop: `git rebase origin/develop`
   - Check other agent's work: `./scripts/debug-helper.ps1`
   - Run contract validation: `npm run dev:validate-contracts`
   - Run all checks: `npm run check-all`

4. Submit PR following the naming convention:
   - Title format: `[Alpha|Beta] Feature description`
   - Required reviewers: Other agent + tech lead
   - Must pass all CI checks and contract validation

### Conflict Resolution

1. Interface conflicts:
   - Create contract resolution branch: `feature/contract/resolve-conflict`
   - Requires approval from both agents
   - Must pass contract validation

2. Implementation conflicts:
   - Resolve through contract validation
   - Update interface documentation
   - Both agents must approve changes

## Coding Standards

### TypeScript

- Use TypeScript for all code
- Maintain strict type safety (avoid using `any` except in tests)
- Follow the existing code style and structure
- Use interfaces for object shapes and types for unions/aliases

### ESLint and Prettier

The project uses ESLint and Prettier for code formatting and linting:

```bash
# Run linting
npm run lint

# Fix auto-fixable issues
npm run lint -- --fix
```

### Naming Conventions

- **Files**: Use PascalCase for classes/components, camelCase for utilities
- **Classes/Interfaces**: Use PascalCase
- **Variables/Functions**: Use camelCase
- **Constants**: Use UPPER_SNAKE_CASE for true constants

### Component Structure

- Each component should be in its own file
- Related components should be grouped in directories
- Include appropriate documentation for public methods and interfaces

## Testing Guidelines

### Unit and Integration Tests

- Write tests for all new functionality
- Maintain at least 80% test coverage
- Use Jest for unit and integration tests
- Mock external dependencies appropriately

```bash
# Run Jest tests
npm run test

# Run tests with coverage
npm run coverage
```

### E2E Tests

- Write E2E tests for critical user flows
- Use Playwright for E2E testing
- Include screenshots for visual verification

```bash
# Run E2E tests
npm run test:e2e:ci

# Run E2E tests with HTML report
npm run test:e2e:report

# Show the HTML report
npm run test:e2e:show-report
```

## Pull Request Process

1. Ensure all tests pass before submitting a PR
2. Update documentation to reflect any changes
3. Include a clear description of the changes in your PR
4. Link to any related issues
5. Wait for code review and address any feedback
6. Once approved, your PR will be merged

## Documentation

- Update README.md for significant changes
- Document new features in the appropriate docs/ files
- Include JSDoc comments for public APIs
- Update the debugging guide for any changes that affect debugging

## Asset Contributions

### Graphics

- Pixel art should maintain the existing style (16x16 or 32x32 base size)
- Include both source files (e.g., .aseprite) and exported PNGs
- Place assets in the appropriate directory under `/public/assets/`

### Audio

- Audio files should be in MP3 format for compatibility
- Include source files (e.g., .wav) when possible
- Keep file sizes reasonable (compress when necessary)
- Document any licensing information

### Narrative Content

- Follow the existing narrative style and tone
- Place narrative events in the appropriate JSON files
- Ensure narrative content is consistent with the game world

## Questions?

If you have any questions or need help, please open an issue on GitHub or reach out to the maintainers.

Thank you for contributing to Signal Lost!

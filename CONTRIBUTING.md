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

1. Create a new branch for your feature or bugfix:
   ```bash
   git checkout -b feature/your-feature-name
   ```
   or
   ```bash
   git checkout -b fix/issue-description
   ```

2. Make your changes, following the [coding standards](#coding-standards)

3. Run tests to ensure your changes don't break existing functionality:
   ```bash
   npm run test
   npm run test:e2e:ci
   ```

4. Commit your changes with a descriptive message:
   ```bash
   git commit -m "feat: Add new feature X"
   ```
   or
   ```bash
   git commit -m "fix: Resolve issue with Y"
   ```

5. Push your branch to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```

6. Create a Pull Request from your fork to the main repository

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

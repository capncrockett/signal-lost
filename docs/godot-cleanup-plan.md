# Godot Migration Cleanup Plan

This document outlines the plan for cleaning up the repository as we migrate from a React-based browser game to a Godot-based implementation.

## Files and Directories to Keep

1. **Godot Project Files**:
   - `godot_project/` - The new Godot implementation
   - `docs/godot-migration.md` - Migration documentation

2. **Documentation**:
   - `docs/` - Keep essential documentation, but remove React-specific docs
   - `README.md` - Update to reflect the Godot migration
   - `CONTRIBUTING.md` - Update for Godot contribution guidelines

3. **Version Control**:
   - `.git/` - Git repository
   - `.gitignore` - Update for Godot-specific patterns

4. **Game Assets**:
   - `assets/` - Keep any reusable game assets (audio, images, etc.)

## Files and Directories to Remove

1. **React Application Code**:
   - `src/` - React source code
   - `src-react/` - Additional React code
   - `public/` - Public assets for the React app
   - `e2e/` - End-to-end tests for the React app
   - `tests/` - Unit tests for the React app

2. **Build and Configuration Files**:
   - `vite.config.ts` - Vite configuration
   - `webpack.performance.config.js` - Webpack configuration
   - `tsconfig.json` - TypeScript configuration
   - `jest.config.ts` - Jest configuration
   - `playwright.config.ts` - Playwright configuration
   - `playwright.performance.config.ts` - Playwright performance configuration
   - `playwright.visual.config.ts` - Playwright visual testing configuration
   - `.eslintrc.json` - ESLint configuration
   - `.prettierrc` - Prettier configuration
   - `.lintstagedrc.json` - Lint-staged configuration

3. **CI/CD and Hooks**:
   - `.github/` - GitHub workflows (replace with Godot-specific workflows)
   - `.husky/` - Git hooks

4. **Dependencies**:
   - `node_modules/` - Node.js dependencies
   - `package.json` - Node.js package configuration
   - `package-lock.json` - Node.js package lock file

5. **Reports and Generated Files**:
   - `playwright-report/` - Playwright test reports
   - `test-results/` - Test results

## Cleanup Process

1. **Backup**:
   - Create a backup branch before cleanup
   - Ensure all important code is committed

2. **Documentation Update**:
   - Update README.md to reflect the Godot migration
   - Update CONTRIBUTING.md for Godot contribution guidelines
   - Remove or update React-specific documentation

3. **Remove Unnecessary Files**:
   - Remove all React-specific code and configuration
   - Remove all testing frameworks for React
   - Remove all build tools for React

4. **Update .gitignore**:
   - Add Godot-specific patterns
   - Remove React-specific patterns

5. **Restructure Repository**:
   - Move Godot project to the root if necessary
   - Organize assets for Godot use

## Post-Cleanup Tasks

1. **Verify Godot Project**:
   - Ensure the Godot project still works after cleanup
   - Run tests to verify functionality

2. **Update CI/CD**:
   - Set up GitHub Actions for Godot testing
   - Configure automated builds for Godot

3. **Documentation**:
   - Create new documentation for Godot development
   - Update contribution guidelines

4. **Asset Migration**:
   - Move and convert assets as needed for Godot
   - Update asset references in Godot scripts

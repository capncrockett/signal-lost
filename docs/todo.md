# Signal Lost Rewrite: DOM-Based Approach

## Overview

After evaluating the current state of the Signal Lost game, we've decided to completely rewrite it using a DOM-based approach instead of Phaser. This decision was made due to:

1. Difficulties with testing Phaser canvas elements using Playwright
2. Challenges in mocking Phaser components for unit tests
3. The disconnect between the UI layer and testing tools
4. The need for a more maintainable and testable architecture

This document outlines the plan for the rewrite, including tasks, timeline, and technical considerations.

## Previous Implementation Status

The previous Phaser-based implementation achieved:

- ✅ RadioTuner Component (80%+ test coverage)
- ✅ SoundscapeManager (95%+ test coverage)
- ✅ Message Decoder (94%+ test coverage)
- ✅ SaveManager (100% test coverage)
- ✅ FieldScene Grid System (tests passing)
- ✅ Narrative Engine (97%+ test coverage)
- ✅ Playwright scene load test (implemented)
- ✅ Playwright console log listener test (implemented)

However, we encountered significant challenges with the Field Scene implementation and E2E testing of canvas elements.

## Rewrite Goals

1. Create a fully testable game using DOM elements
2. Maintain the core gameplay mechanics and narrative
3. Improve performance and reduce dependencies
4. Establish a clean, maintainable architecture
5. Ensure comprehensive test coverage

## Technical Stack

- **Framework**: React with TypeScript
- **State Management**: React Context or Redux
- **Styling**: CSS Modules or Styled Components
- **Testing**: Jest for unit tests, Playwright for E2E tests
- **Audio**: Web Audio API
- **Build**: Vite (keep existing configuration)

## Project Structure

```
signal-lost/
├── src/
│   ├── components/       # UI components
│   │   ├── radio/        # Radio tuner components
│   │   ├── field/        # Field exploration components
│   │   ├── narrative/    # Narrative display components
│   │   ├── inventory/    # Inventory components
│   │   └── common/       # Shared UI components
│   ├── hooks/            # Custom React hooks
│   ├── context/          # React context providers
│   ├── utils/            # Utility functions
│   ├── audio/            # Audio processing
│   ├── types/            # TypeScript type definitions
│   ├── assets/           # Static assets
│   └── App.tsx           # Main application component
├── tests/                # Unit and integration tests
├── e2e/                  # End-to-end tests
└── public/               # Public assets
```

## Sprint Plan

### Sprint 01: Foundation (Completed)

1. ✅ Set up new React + TypeScript project
2. ✅ Establish component architecture
3. ✅ Create basic layout and navigation
4. ✅ Set up testing infrastructure
5. ✅ Implement state management
6. ✅ Create design system and basic styling
7. ✅ Implement routing system
8. ✅ Set up asset management
9. ✅ Add accessibility features

### Sprint 02: Core Game Mechanics (Completed)

1. ✅ Implement radio tuner component
   - ✅ Frequency dial interaction
   - ✅ Signal detection
   - ✅ Static/noise visualization
2. ✅ Create audio system
   - ✅ Web Audio API integration
   - ✅ Noise generation
   - ✅ Signal processing
3. ✅ Implement basic narrative system
   - ✅ Message display
   - ✅ Progressive decoding

### Sprint 2.5: Bug Fixes and Workflow Improvement (Current)

1. ⬜ Fix critical bugs in the application
   - ⬜ Resolve asset loading errors for audio files
   - ⬜ Fix infinite render loop in RadioTuner component
   - ⬜ Address memory leaks
2. ⬜ Improve test infrastructure and coverage
   - ⬜ Fix failing unit tests
   - ⬜ Improve test mocks for audio components
3. ⬜ Clean up code and improve quality
   - ⬜ Remove unused code and imports
   - ⬜ Fix TypeScript errors and warnings
4. ⬜ Enhance agent workflow
   - ⬜ Clarify responsibilities between Alpha and Beta
   - ⬜ Improve communication protocols

### Sprint 03: Field Exploration and Inventory (Upcoming)

1. ⬜ Implement field exploration
   - ⬜ Grid-based movement
   - ⬜ Player character
   - ⬜ Interactable objects
2. ⬜ Create inventory system
   - ⬜ Item collection
   - ⬜ Item usage
   - ⬜ Inventory UI
3. ⬜ Connect field exploration with radio signals

### Sprint 04: Game Progression and Polish (Upcoming)

1. ⬜ Implement save/load system
2. ⬜ Add game progression mechanics
3. ⬜ Enhance visual design
4. ⬜ Optimize performance
5. ⬜ Add sound effects and audio polish
6. ⬜ Implement accessibility features

### Sprint 05: Testing and Documentation (Upcoming)

1. ⬜ Write comprehensive unit tests
2. ⬜ Create E2E tests for critical user flows
3. ⬜ Document codebase
4. ⬜ Create user documentation
5. ⬜ Final bug fixes and polish

## Cleanup Plan

### Phase 1: Initial Setup (Completed)

1. ✅ Create new project structure
2. ✅ Copy over essential assets
3. ✅ Set up new testing infrastructure
4. ✅ Establish new development workflow

### Phase 2: Code Migration (In Progress)

1. ✅ Identify reusable logic from current codebase
2. ⬜ Refactor and migrate core game mechanics
3. ⬜ Adapt tests for new architecture
4. ✅ Preserve game data and content

### Phase 3: Final Cleanup (Upcoming)

1. ✅ Remove Phaser dependencies
2. ⬜ Delete unused files and directories
3. ⬜ Update documentation
4. ⬜ Archive old codebase for reference

## Files to Keep

- Game assets (images, audio)
- Game data (narrative events, items)
- Core game logic (where applicable)
- Test fixtures and helpers (where applicable)
- Documentation

## Files to Remove

- All Phaser-specific code
- Canvas-based rendering
- Current UI implementation
- Phaser-specific tests
- Unused dependencies

## Development Workflow

1. Create feature branches from `rewrite-dom-approach`
2. Implement features with tests
3. Submit PRs for review
4. Merge to `rewrite-dom-approach` branch
5. Once stable, merge to `develop`

## Testing Strategy

1. Unit tests for all components and utilities
2. Integration tests for feature combinations
3. E2E tests for critical user flows
4. Visual regression tests for UI components
5. Accessibility tests

## Current Status

1. ✅ React project setup completed
2. ✅ Core components implemented
3. ✅ Testing patterns established
4. ✅ Game data and assets migrated
5. ✅ Foundation sprint completed

## Next Steps

1. Implement radio tuner component
2. Create audio system
3. Develop narrative system
4. Complete Sprint 02 objectives
5. Prepare for Sprint 03

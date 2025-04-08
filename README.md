# 🎮 Signal Lost

> _A lo-fi, pixel-art narrative exploration game where a lone radio operator pieces together a fractured world through distorted frequencies and eerie signals._

---

## 🚀 Quick Start

```bash
npm install
npm run dev                # Start local dev server
npm run test               # Run all Jest tests
npm run test:e2e:ci        # Run E2E tests without opening reports
npm run test:e2e:report    # Run E2E tests and generate HTML report
npm run test:e2e:show-report # Show the HTML report from previous E2E test run
npm run coverage           # Output coverage report
npm run lint               # Run ESLint
npm run type-check         # Run TypeScript compiler check
npm run check-all          # Run all checks (lint, type-check, tests, E2E tests)
```

## 📂 Project Structure

| Folder  | Description                               |
| ------- | ----------------------------------------- |
| src/    | Core TypeScript game source (Phaser 3)    |
| assets/ | Pixel art, audio files, tilemaps          |
| tests/  | Jest unit/integration tests (ts-jest)     |
| e2e/    | Playwright tests for browser interactions |
| docs/   | Feature specs, prompt definitions, tasks  |

## ✅ Development Rules

- TypeScript must be used throughout
- Tests must cover ≥ 80% of code (unit + integration + E2E)

## 🎮 Gameplay Instructions

### Main Controls

- **Radio Tuning**: Use the mouse to tune the radio frequency
- **Interact**: Press `E` or `Space` to interact with objects
- **Movement**: Use `W`, `A`, `S`, `D` or arrow keys to move in the field
- **Inventory**: Press `I` to open/close the inventory

### Game Mechanics

#### Radio Signals

The game revolves around discovering and interpreting radio signals. Different frequencies contain different types of signals:

- **Location Signals**: Reveal coordinates on the map
- **Message Signals**: Provide story elements and clues
- **Item Signals**: Allow you to discover special items

Tune the radio carefully to find the clearest signal. The signal strength indicator shows how close you are to a valid frequency.

#### Field Exploration

Explore the field to discover locations mentioned in radio signals. You'll find:

- **Radio Towers**: Communication hubs with valuable information
- **Ruins**: Abandoned research facilities with clues about what happened
- **Bunkers**: Secure locations with supplies and equipment

#### Inventory System

Collect and use items to progress through the game:

- **Tools**: Items like the radio and map that help you navigate
- **Documents**: Notes and journals that provide story context
- **Keys**: Used to unlock doors and access new areas
- **Radio Parts**: Enhance your radio's capabilities
- **Consumables**: One-time use items like batteries and medkits

### Story Progression

The game's narrative unfolds as you discover signals and explore locations. Pay attention to:

- The sequence of frequencies mentioned in various locations
- The mysterious "Signal Protocol" referenced in documents
- The events that led to the current state of the world

## 🔄 CI/CD Workflow

This project uses GitHub Actions for continuous integration and deployment. The workflow is defined in `.github/workflows/agent-workflow.yml` and includes:

- Linting with ESLint
- TypeScript compiler checks
- Unit and integration tests with Jest
- E2E tests with Playwright
- Building the project
- Uploading test reports and screenshots as artifacts

The workflow runs automatically on pushes to `main` and `develop` branches, as well as on pull requests to these branches. You can also trigger it manually from the GitHub Actions tab.

- Console logs must be accessible and validated in browser-based tests
- All prompts/tasks live in docs/ files and serve as acceptance criteria
- If Agent fails, it must re-read the relevant README task before retrying

## 🧠 Augment Agent Instructions

- You are operating on the "Signal Lost" codebase.
- Do not hallucinate functionality not in the prompt.
- Refer to the matching docs/ prompt file section before starting.
- Once you've completed a feature and verified tests + coverage pass, mark the task as ✅ complete in the README and commit your changes.
- If you crash or lose state, re-read the prompt and resume from the last ❌ section.

Refer to:

- docs/prompts.md
- docs/tests.md
- docs/todo.md

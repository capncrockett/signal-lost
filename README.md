# 🎮 Signal Lost

> _A lo-fi, pixel-art narrative exploration game where a lone radio operator pieces together a fractured world through distorted frequencies and eerie signals._

---

## 🚀 Quick Start

### Prerequisites

- [Godot Engine](https://godotengine.org/download) (4.x)
- [Git](https://git-scm.com/downloads)

### Running the Game

```bash
# Clone the repository
git clone https://github.com/capncrockett/signal-lost.git
cd signal-lost

# Open the project in Godot
godot --path godot_project
```

### Running Tests

```bash
# Run all tests
./godot_project/run_tests.sh  # Linux/macOS
.\godot_project\run_tests.bat  # Windows

# Run specific tests
godot --path godot_project --script tests/test_runner.gd
```

## 📂 Project Structure

| Folder                | Description                               |
| --------------------- | ----------------------------------------- |
| godot_project/        | Godot project files                       |
| godot_project/scenes/ | Game scenes                               |
| godot_project/scripts/| GDScript source files                     |
| godot_project/assets/ | Pixel art, audio files, tilemaps          |
| godot_project/tests/  | Test scripts                              |
| docs/                 | Documentation and migration plans         |

## ✅ Development Rules

- GDScript must be used for game logic
- Tests must cover ≥ 80% of code
- Follow the [Godot style guide](https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/gdscript_styleguide.html)

## 🎮 Gameplay Instructions

### Main Controls

- **Radio Tuning**: Use the mouse or keyboard to tune the radio frequency
  - **Tune Up**: Press `D` or right arrow key
  - **Tune Down**: Press `A` or left arrow key
  - **Toggle Power**: Press `Space`
  - **Scan**: Press `S` to start/stop automatic scanning
- **Interact**: Press `E` to interact with objects
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
- docs/workflow.md
- docs/agent-roles.md
- docs/cross-agent-communication.md
- docs/cross-agent-workflow.md
- docs/documentation-consolidation-plan.md

# 🎮 Signal Lost

> *A lo-fi, pixel-art narrative exploration game where a lone radio operator pieces together a fractured world through distorted frequencies and eerie signals.*

---

## 🚀 Quick Start

```bash
npm install
npm run dev          # Start local dev server
npm run test         # Run all Jest tests
npm run test:e2e     # Run E2E Playwright tests
npm run coverage     # Output coverage report
```

## 📂 Project Structure

| Folder | Description |
| ------ | ----------- |
| src/ | Core TypeScript game source (Phaser 3) |
| assets/ | Pixel art, audio files, tilemaps |
| tests/ | Jest unit/integration tests (ts-jest) |
| e2e/ | Playwright tests for browser interactions |
| docs/ | Feature specs, prompt definitions, tasks |

## ✅ Development Rules

- TypeScript must be used throughout
- Tests must cover ≥ 80% of code (unit + integration + E2E)
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

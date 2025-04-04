## 🧾 FILE: `docs/prompts.md`

```markdown
# 📜 Signal Lost – Feature Prompts

These are modular, acceptance-test-driven development prompts. Implement each one using TypeScript + Phaser + Web Audio API.

---

### ✅ RadioTuner Component

> Build a `RadioTuner` UI component for Phaser 3. Simulates analog tuning with:
> - Frequency slider (visual + interactive)
> - Static audio that cleans as signal sharpens
> - Emits `signalLock(frequency)` event when close to predefined frequency
> - Tests: signal detection logic, slider input behavior
> - 80%+ test coverage

---

### ❌ SoundscapeManager

> Create a `SoundscapeManager` using Web Audio API. Includes:
> - Three ambient audio layers (static, drone, signal blip)
> - Can adjust volume/panning based on player location or tuning
> - Public methods: `adjustLayers(signalStrength: number)`
> - Tests: panning/volume blend logic, unit + integration
> - 80%+ test coverage

---

### ❌ Message Decoder

> Write a utility to obfuscate and decode messages.
> - `obfuscateMessage(msg: string): string`
> - `decodeMessage(msg: string): string`
> - Simulate radio interference with %/#/! characters
> - Unit tests for edge cases
> - 80%+ coverage

---

### ❌ Exploration Grid

> Implement a `FieldScene` with:
> - Tilemap/grid-based movement
> - Collision system for interactables
> - Trigger event when near tower/ruins
> - Tests: movement boundaries, event detection
> - 80%+ coverage

---

### ❌ SaveManager

> Create a save/load manager with localStorage.
> - Tracks flags: `foundTower1`, `decodedMsg3`, etc.
> - API: `getFlag(id)`, `setFlag(id)`, `clearFlags()`
> - Mock localStorage in tests
> - 80%+ test coverage

---

### ❌ NarrativeEngine

> Load narrative events from JSON/YAML:
> ```yaml
> id: tower_discovery
> message: "You hear whispers in the static..."
> choices:
>   - text: "Investigate"
>     outcome: "trigger_event_1"
> ```
> - Parser module
> - Renderer in scene
> - Tests: branching logic
> - 80%+ coverage

# 🧪 Godot Testing Strategy

## Test Types

| Layer       | Tool       | Goal                                 |
| ----------- | ---------- | ------------------------------------ |
| Unit        | GUT        | Scripts, signal logic, state management |
| Integration | GUT        | Scenes working together              |
| Manual      | Godot Editor | Gameplay, visuals, audio quality     |

---

## GUT (Godot Unit Testing) Setup

- Tests run in Godot
- Run: `./godot_project/run_tests.sh` (Linux/macOS) or `.\godot_project\run_tests.bat` (Windows)
- Run specific tests: `godot --path godot_project --script tests/test_runner.gd`

```gdscript
# Example test
extends "res://addons/gut/test.gd"

func test_radio_tuner_frequency_change():
    # Arrange
    var radio_tuner = RadioTunerScene.instance()
    add_child(radio_tuner)
    radio_tuner.current_frequency = 90.0
    
    # Act
    radio_tuner.change_frequency(0.1)
    
    # Assert
    assert_eq(radio_tuner.current_frequency, 90.1, "Frequency should be 90.1 after increasing by 0.1")
    
    # Cleanup
    radio_tuner.queue_free()
```

---

## Terminal Testing

Run tests from the terminal for CI/CD integration:

```bash
# Run all tests
godot --path /path/to/project --script tests/test_runner.gd

# Run specific test
godot --path /path/to/project -s addons/gut/gut_cmdln.gd -gtest=res://tests/test_radio_tuner.gd
```

---

## Coverage Goals

- Minimum 80% test coverage across entire codebase
- Run: `godot --path godot_project -s addons/gut/gut_cmdln.gd -gcov -gprint_summary`

---

## Test Organization

```
godot_project/
└── tests/
    ├── test_runner.gd           # Main test runner
    ├── unit/                    # Unit tests
    │   ├── test_radio_tuner.gd  # Tests for RadioTuner
    │   ├── test_game_state.gd   # Tests for GameState
    │   └── test_audio_manager.gd # Tests for AudioManager
    └── integration/             # Integration tests
        ├── test_radio_narrative.gd # Tests for radio and narrative integration
        └── test_game_flow.gd    # Tests for game flow
```

---

## Manual Testing Checklist

- [ ] Radio tuner responds correctly to input
- [ ] Static visualization matches audio
- [ ] Signal detection works at correct frequencies
- [ ] Audio quality is acceptable
- [ ] Performance is smooth (no frame drops)
- [ ] UI elements are properly positioned and scaled
- [ ] Game state is saved and loaded correctly
- [ ] Narrative progression works as expected

---

## Testing Best Practices

1. **Write Tests First**: Follow test-driven development when possible
2. **Test Edge Cases**: Include tests for boundary conditions
3. **Mock Dependencies**: Use dependency injection for testability
4. **Keep Tests Fast**: Tests should run quickly for rapid feedback
5. **Test One Thing**: Each test should verify a single behavior
6. **Use Clear Names**: Test names should describe what is being tested
7. **Clean Up**: Always clean up resources after tests
8. **Isolate Tests**: Tests should not depend on each other

---

## Example Test Suite

```gdscript
extends "res://addons/gut/test.gd"

# Path to the scene we want to test
var RadioTunerScene = load("res://scenes/radio/RadioTuner.tscn")
var radio_tuner = null

# Called before each test
func before_each():
    # Create a new instance of the RadioTuner scene
    radio_tuner = RadioTunerScene.instance()
    add_child(radio_tuner)
    
    # Reset GameState to default values
    GameState.current_frequency = 90.0
    GameState.is_radio_on = false
    GameState.discovered_frequencies = []

# Called after each test
func after_each():
    # Clean up
    radio_tuner.queue_free()
    radio_tuner = null

# Test power button functionality
func test_power_button():
    # Initially radio should be off
    assert_false(GameState.is_radio_on, "Radio should start in OFF state")
    
    # Simulate clicking the power button
    radio_tuner.power_button.emit_signal("pressed")
    
    # Radio should now be on
    assert_true(GameState.is_radio_on, "Radio should be ON after clicking power button")
    
    # Simulate clicking the power button again
    radio_tuner.power_button.emit_signal("pressed")
    
    # Radio should now be off again
    assert_false(GameState.is_radio_on, "Radio should be OFF after clicking power button again")

# Test frequency change
func test_frequency_change():
    # Set initial frequency
    GameState.set_frequency(90.0)
    
    # Simulate changing frequency
    radio_tuner.change_frequency(0.1)
    
    # Check if frequency was updated
    assert_eq(GameState.current_frequency, 90.1, "Frequency should be 90.1 after increasing by 0.1")
    
    # Test frequency limits
    GameState.set_frequency(min_frequency)
    radio_tuner.change_frequency(-0.1)
    assert_eq(GameState.current_frequency, min_frequency, "Frequency should not go below minimum")
    
    GameState.set_frequency(max_frequency)
    radio_tuner.change_frequency(0.1)
    assert_eq(GameState.current_frequency, max_frequency, "Frequency should not go above maximum")
```

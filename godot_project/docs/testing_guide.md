# Signal Lost - Testing Guide

This document outlines the testing approach for the Signal Lost game project.

## Test Framework

We use a simple GDScript-based test framework that provides:

- A `SimpleTest.gd` script that runs all tests
- Clear pass/fail feedback for each test
- Tests for each component and integration tests

## Test Structure

Tests are organized into the following functions in `SimpleTest.gd`:

- `test_game_state()` - Tests for the GameState component
- `test_audio_manager()` - Tests for the AudioManager component
- `test_integration()` - Integration tests for all components working together

## Running Tests

There are several ways to run tests:

### From the Command Line

```bash
cd godot_project
./run_tests.sh
```

This will run all tests and report the results.

### From the Editor

1. Open the Godot editor
2. Open the `SimpleTestScene.tscn` scene
3. Click the "Play" button

## Test-Driven Development (TDD)

We follow a TDD approach:

1. Write a failing test
2. Implement the minimum code to make the test pass
3. Refactor the code while keeping the test passing
4. Repeat

## Adding New Tests

To add new tests:

1. Open `SimpleTest.gd`
2. Add a new test function or extend an existing one
3. Follow the pattern of:
   - Setup
   - Action
   - Assertion
   - Cleanup

Example:

```gdscript
# Test a new feature
var initial_state = game_state.some_property()
game_state.do_something()
var new_state = game_state.some_property()

if new_state == expected_value:
    print("PASS: Feature works")
else:
    print("FAIL: Feature doesn't work. Expected " + str(expected_value) + ", got " + str(new_state))
```

## Core Components

The core components that need to be tested are:

1. **GameState** - Manages game state including radio frequency, signals, and messages
2. **AudioManager** - Handles audio playback for static noise and signals
3. **RadioTuner** - The UI component for the radio

## Integration Testing

Integration tests verify that the components work together correctly. This includes:

1. GameState and AudioManager integration
2. RadioTuner and GameState integration
3. Full radio workflow

## Troubleshooting

If tests are failing:

1. Check the test output for specific failure messages
2. Verify that the component being tested is working as expected
3. Use print statements to debug if needed

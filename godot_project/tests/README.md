# Signal Lost Tests

This directory contains tests for the Signal Lost game.

## Test Structure

### Unit Tests

- **test_game_state.gd**: Tests for the game state management
- **test_audio_manager.gd**: Tests for the audio system
- **test_radio_tuner.gd**: Tests for the radio tuner UI
- **test_audio_visualizer.gd**: Tests for the audio visualizer

### Integration Tests

- **test_radio_tuner_integration.gd**: Tests for the integration between RadioTuner, GameState, and AudioManager

### Test Infrastructure

- **base_test.gd**: Base class for all tests
- **post_run.gd**: Script executed after all tests have run
- **GutTestRunner.tscn**: Scene for running all tests

## Running Tests

### Prerequisites

- Godot Engine 4.2 or later

### Running Tests with GUT

To run all tests using the GUT framework:

```bash
# On Linux/macOS
./run_gut_tests.sh

# On Windows
# Create a run_gut_tests.bat file with similar content
```

### Running Simple Tests

To run the simple tests without GUT:

```bash
# On Linux/macOS
./run_tests.sh

# On Windows
run_tests.bat
```

### Running Integration Tests

To run the full integration tests that simulate a user interacting with the radio tuner:

```bash
# On Linux/macOS
./run_radio_test.sh

# On Windows
# Create a run_radio_test.bat file similar to run_tests.bat but pointing to FullIntegrationTestScene.tscn
```

### Running Audio Visualizer Tests

To run the audio visualizer tests:

```bash
# On Linux/macOS
./run_audio_visualizer_test.sh

# On Windows
# Create a run_audio_visualizer_test.bat file similar to run_tests.bat
```

The integration test simulates a complete user workflow:

1. Turn the radio on
2. Set the initial frequency
3. Scan for signals
4. Fine-tune the frequency
5. Check signal strength
6. Decode messages
7. Turn the radio off

## Writing Tests

Tests are written using the GUT framework. Each test class should:

1. Inherit from `BaseTest` (which extends `GutTest`)
2. Implement `before_each()` and `after_each()` methods for setup and teardown if needed
3. Name test methods with a `test_` prefix

Example:

```gdscript
extends BaseTest

# Unit tests for some feature

var _my_object = null

func before_each():
	# Call parent before_each
	super.before_each()

	# Setup code
	_my_object = MyClass.new()
	add_child_autofree(_my_object)


func after_each():
	# Call parent after_each
	super.after_each()

	# Teardown code
	if _my_object:
		_my_object.queue_free()
		_my_object = null


func test_something():
	# Arrange
	var expected = 42

	# Act
	var actual = _my_object.some_method()

	# Assert
	assert_eq(actual, expected, "The method should return 42")
```

## Assertions

GUT provides several assertion methods:

- `assert_eq(actual, expected, message)`: Asserts that two values are equal
- `assert_ne(actual, expected, message)`: Asserts that two values are not equal
- `assert_true(condition, message)`: Asserts that a condition is true
- `assert_false(condition, message)`: Asserts that a condition is false
- `assert_null(value, message)`: Asserts that a value is null
- `assert_not_null(value, message)`: Asserts that a value is not null
- `assert_gt(value, threshold, message)`: Asserts that a value is greater than a threshold
- `assert_lt(value, threshold, message)`: Asserts that a value is less than a threshold
- `assert_between(value, min, max, message)`: Asserts that a value is between min and max
- `assert_almost_eq(actual, expected, error_margin, message)`: Asserts that two values are almost equal
- `assert_string_contains(text, search, message)`: Asserts that a string contains a substring
- `assert_file_exists(path, message)`: Asserts that a file exists
- `assert_file_does_not_exist(path, message)`: Asserts that a file does not exist

## Test Coverage

The goal is to maintain at least 80% test coverage across all test domains. This means:

1. All public methods should have at least one test
2. All edge cases should be tested
3. All error conditions should be tested
4. All state changes should be tested

```

```

# Signal Lost Tests

This directory contains tests for the Signal Lost game.

## Test Structure

- **AudioManagerTests.cs**: Tests for the audio system
- **GameStateTests.cs**: Tests for the game state management
- **RadioTunerTests.cs**: Tests for the radio tuner
- **TestRunner.cs**: Script to run all tests

## Running Tests

### Prerequisites

- Godot Engine 4.2 or later
- GUT (Godot Unit Testing) addon

### Installation

The GUT addon will be automatically installed when running the tests. If you want to install it manually, run:

```bash
# On Linux/macOS
./install_gut.sh

# On Windows
install_gut.bat
```

### Running Tests

To run all tests:

```bash
# On Linux/macOS
./run_tests.sh

# On Windows
run_tests.bat
```

## Writing Tests

Tests are written using the GUT framework. Each test class should:

1. Inherit from `Test`
2. Be decorated with `[TestClass]`
3. Have test methods decorated with `[Test]`
4. Override `Before()` and `After()` methods for setup and teardown

Example:

```csharp
using Godot;
using System;
using GUT;

namespace SignalLost.Tests
{
	[TestClass]
	public class MyTests : Test
	{
		// Called before each test
		public override void Before()
		{
			// Setup code
		}

		// Called after each test
		public override void After()
		{
			// Teardown code
		}

		[Test]
		public void TestSomething()
		{
			// Arrange
			var expected = 42;
			
			// Act
			var actual = SomeMethod();
			
			// Assert
			AssertEqual(actual, expected, "The method should return 42");
		}
	}
}
```

## Assertions

GUT provides several assertion methods:

- `AssertEqual(actual, expected, message)`: Asserts that two values are equal
- `AssertNotEqual(actual, expected, message)`: Asserts that two values are not equal
- `AssertTrue(condition, message)`: Asserts that a condition is true
- `AssertFalse(condition, message)`: Asserts that a condition is false
- `AssertNull(value, message)`: Asserts that a value is null
- `AssertNotNull(value, message)`: Asserts that a value is not null
- `AssertGreater(value, threshold, message)`: Asserts that a value is greater than a threshold
- `AssertLess(value, threshold, message)`: Asserts that a value is less than a threshold
- `AssertBetween(value, min, max, message)`: Asserts that a value is between min and max
- `AssertStringContains(text, search, message)`: Asserts that a string contains a substring

## Test Coverage

The goal is to maintain at least 80% test coverage across all test domains. This means:

1. All public methods should have at least one test
2. All edge cases should be tested
3. All error conditions should be tested
4. All state changes should be tested

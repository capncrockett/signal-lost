# Signal Lost Testing Guide

## Overview

This document outlines the testing strategy for the Signal Lost game, including test types, setup, and best practices.

## Test Types

| Layer       | Tool       | Goal                                 |
| ----------- | ---------- | ------------------------------------ |
| Unit        | C# Tests   | Components, state management, logic  |
| Integration | C# Tests   | Components working together          |
| Scene       | Test Scenes | Visual components, scene interactions |
| Manual      | Godot Editor | Gameplay, visuals, audio quality     |

## Running Tests

Run tests from the terminal:

```bash
# Run all tests
cd godot_project
./run_tests.bat

# Run specific tests
cd godot_project
./run_radio_tests.bat
```

## Test Structure

All C# tests inherit from the `Test` class and follow this structure:

```csharp
using Godot;
using System;
using GUT;

namespace SignalLost.Tests
{
    [GlobalClass]
    public partial class ComponentTests : Test
    {
        private Component _component = null;

        // Called before each test
        public override void Before()
        {
            _component = new Component();
            AddChild(_component);
            _component._Ready();
        }

        // Called after each test
        public override void After()
        {
            _component.QueueFree();
            _component = null;
        }

        // Test method
        [Test]
        public void TestSomething()
        {
            // Arrange
            _component.SetProperty(value);

            // Act
            _component.DoSomething();

            // Assert
            AssertEqual(_component.GetResult(), expectedValue);
        }
    }
}
```

## Test Organization

```
godot_project/
└── tests/
    ├── TestRunner.cs           # Main test runner
    ├── GUT/                    # Test framework
    │   └── Test.cs             # Base class for all tests
    ├── ComponentTests.cs       # Tests for specific components
    └── test_scenes/            # Test scenes
        └── ComponentTestScene.tscn
```

## Best Practices

1. **Write Tests First**: Follow test-driven development
2. **Test Edge Cases**: Include tests for boundary conditions
3. **Mock Dependencies**: Use dependency injection for testability
4. **Keep Tests Fast**: Tests should run quickly
5. **Test One Thing**: Each test should verify a single behavior
6. **Use Clear Names**: Test names should describe what is being tested
7. **Clean Up**: Always clean up resources after tests
8. **Isolate Tests**: Tests should not depend on each other
9. **Add Timeouts**: Prevent tests from hanging
10. **Handle Errors**: Use try-catch blocks to handle errors

## Coverage Goals

- Minimum 80% test coverage across the codebase
- All new features should have corresponding tests
- Critical components should have 100% coverage

## Manual Testing Checklist

- [ ] Radio tuner responds correctly to input
- [ ] Static visualization matches audio
- [ ] Signal detection works at correct frequencies
- [ ] Audio quality is acceptable
- [ ] Performance is smooth (no frame drops)
- [ ] UI elements are properly positioned and scaled
- [ ] Game state is saved and loaded correctly
- [ ] Narrative progression works as expected

## Troubleshooting

### Common Issues

1. **Tests hanging**: Check for infinite loops or missing signal connections
2. **Node not found errors**: Ensure nodes are properly initialized
3. **Signal connection errors**: Verify signal names and connections
4. **Resource loading errors**: Check file paths and resource availability

### Debugging Tests

1. Add print statements: `GD.Print("Debug info")`
2. Use the Godot debugger
3. Run tests with verbose output: `./run_tests.bat -v`
4. Check test logs in the `logs` directory

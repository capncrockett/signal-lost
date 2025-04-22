# End-to-End Testing Plan for Signal Lost

## Overview

This document outlines a comprehensive approach to end-to-end (E2E) testing for Signal Lost, ensuring that the game functions correctly as a complete system. While Godot doesn't have a native E2E testing framework like Playwright or Cypress, we can build our own E2E-style tests using a combination of Godot's built-in tools and custom testing utilities.

## E2E Testing Approach

### 1. Scene-Based Test Framework

Create a scene-based test framework that can:
- Load complete game scenes
- Simulate user input
- Verify expected outcomes
- Generate test reports

### 2. Automated Test Runner

Implement an automated test runner that can:
- Run tests in headless mode
- Execute test scenarios in sequence
- Capture screenshots for visual verification
- Generate detailed test reports

### 3. Test Scenarios

Define comprehensive test scenarios that cover:
- Core gameplay mechanics
- User interface interactions
- Game progression
- Cross-platform compatibility

## Implementation Details

### 1. E2E Test Framework

```csharp
// E2ETestFramework.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalLost.Testing
{
    [GlobalClass]
    public partial class E2ETestFramework : Node
    {
        // Test results
        private List<TestResult> _testResults = new List<TestResult>();

        // Current test
        private E2ETest _currentTest;

        // Test timeout (in seconds)
        [Export]
        public float TestTimeout { get; set; } = 30.0f;

        // Signal for test completion
        [Signal]
        public delegate void TestsCompletedEventHandler(bool allPassed);

        // Run all tests
        public async Task RunAllTests(List<E2ETest> tests)
        {
            _testResults.Clear();

            foreach (var test in tests)
            {
                var result = await RunTest(test);
                _testResults.Add(result);
            }

            // Generate report
            GenerateTestReport();

            // Emit completion signal
            bool allPassed = _testResults.TrueForAll(r => r.Passed);
            EmitSignal(nameof(TestsCompleted), allPassed);

            return;
        }

        // Run a single test
        public async Task<TestResult> RunTest(E2ETest test)
        {
            GD.Print($"Running test: {test.Name}");

            _currentTest = test;

            // Create result object
            var result = new TestResult
            {
                TestName = test.Name,
                StartTime = DateTime.Now
            };

            try
            {
                // Set up test
                await test.Setup();

                // Run test with timeout
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(TestTimeout));
                var testTask = test.Execute();

                if (await Task.WhenAny(testTask, timeoutTask) == timeoutTask)
                {
                    // Test timed out
                    result.Passed = false;
                    result.ErrorMessage = $"Test timed out after {TestTimeout} seconds";
                }
                else
                {
                    // Test completed
                    result.Passed = await testTask;

                    if (!result.Passed)
                    {
                        result.ErrorMessage = test.ErrorMessage;
                    }
                }

                // Take screenshot
                result.ScreenshotPath = TakeScreenshot(test.Name);
            }
            catch (Exception ex)
            {
                // Test threw an exception
                result.Passed = false;
                result.ErrorMessage = $"Exception: {ex.Message}";
                GD.PrintErr($"Test {test.Name} failed with exception: {ex}");
            }
            finally
            {
                // Clean up
                await test.Cleanup();

                // Record end time
                result.EndTime = DateTime.Now;
                result.Duration = (result.EndTime - result.StartTime).TotalSeconds;

                // Log result
                if (result.Passed)
                {
                    GD.Print($"Test {test.Name} passed in {result.Duration:F2} seconds");
                }
                else
                {
                    GD.PrintErr($"Test {test.Name} failed in {result.Duration:F2} seconds: {result.ErrorMessage}");
                }
            }

            return result;
        }

        // Take a screenshot and analyze it
        private string TakeScreenshot(string testName)
        {
            string filename = $"test_{testName.ToLower().Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string directory = "screenshots";
            string path = $"{directory}/{filename}";

            // Ensure directory exists
            var dir = DirAccess.Open("res://");
            if (!dir.DirExists(directory))
            {
                dir.MakeDir(directory);
            }

            // Take screenshot
            var image = GetViewport().GetTexture().GetImage();
            image.SavePng(path);

            // Analyze the screenshot using the Python script
            // This is done asynchronously to avoid blocking the test
            OS.Execute("py", new string[] { "analyze_existing_screenshot.py", path }, output: false, blocking: false);

            return path;
        }

        // Generate test report
        private void GenerateTestReport()
        {
            string reportPath = $"user://test_report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

            using (var file = FileAccess.Open(reportPath, FileAccess.ModeFlags.Write))
            {
                file.StoreLine("E2E Test Report for Signal Lost");
                file.StoreLine($"Date: {DateTime.Now}");
                file.StoreLine("----------------------------------------");

                int passed = 0;
                int failed = 0;

                foreach (var result in _testResults)
                {
                    if (result.Passed)
                    {
                        passed++;
                    }
                    else
                    {
                        failed++;
                    }

                    file.StoreLine($"Test: {result.TestName}");
                    file.StoreLine($"Result: {(result.Passed ? "PASSED" : "FAILED")}");
                    file.StoreLine($"Duration: {result.Duration:F2} seconds");

                    if (!result.Passed)
                    {
                        file.StoreLine($"Error: {result.ErrorMessage}");
                    }

                    file.StoreLine($"Screenshot: {result.ScreenshotPath}");
                    file.StoreLine("----------------------------------------");
                }

                file.StoreLine($"Summary: {passed} passed, {failed} failed");
                file.StoreLine($"Total tests: {_testResults.Count}");
            }

            GD.Print($"Test report generated at {reportPath}");
        }
    }

    // Test result class
    public class TestResult
    {
        public string TestName { get; set; }
        public bool Passed { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public string ScreenshotPath { get; set; }
    }
}
```

### 2. Base E2E Test Class

```csharp
// E2ETest.cs
using Godot;
using System;
using System.Threading.Tasks;

namespace SignalLost.Testing
{
    [GlobalClass]
    public abstract partial class E2ETest : Node
    {
        // Test name
        public abstract string Name { get; }

        // Error message if test fails
        public string ErrorMessage { get; protected set; }

        // Setup the test
        public virtual async Task Setup()
        {
            await Task.CompletedTask;
        }

        // Execute the test
        public abstract Task<bool> Execute();

        // Clean up after the test
        public virtual async Task Cleanup()
        {
            await Task.CompletedTask;
        }

        // Helper method to simulate input
        protected void SimulateKeyPress(Key key)
        {
            var inputEvent = new InputEventKey();
            inputEvent.Keycode = key;
            inputEvent.Pressed = true;
            Input.ParseInputEvent(inputEvent);

            // Release key
            inputEvent = new InputEventKey();
            inputEvent.Keycode = key;
            inputEvent.Pressed = false;
            Input.ParseInputEvent(inputEvent);
        }

        // Helper method to simulate mouse click
        protected void SimulateMouseClick(Vector2 position)
        {
            var inputEvent = new InputEventMouseButton();
            inputEvent.ButtonIndex = MouseButton.Left;
            inputEvent.Pressed = true;
            inputEvent.Position = position;
            Input.ParseInputEvent(inputEvent);

            // Release mouse button
            inputEvent = new InputEventMouseButton();
            inputEvent.ButtonIndex = MouseButton.Left;
            inputEvent.Pressed = false;
            inputEvent.Position = position;
            Input.ParseInputEvent(inputEvent);
        }

        // Helper method to simulate mouse movement
        protected void SimulateMouseMovement(Vector2 fromPosition, Vector2 toPosition, int steps = 10)
        {
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                Vector2 position = fromPosition.Lerp(toPosition, t);

                var inputEvent = new InputEventMouseMotion();
                inputEvent.Position = position;
                Input.ParseInputEvent(inputEvent);
            }
        }

        // Helper method to wait for a condition
        protected async Task<bool> WaitForCondition(Func<bool> condition, float timeout = 5.0f)
        {
            float elapsed = 0;
            float interval = 0.1f;

            while (elapsed < timeout)
            {
                if (condition())
                {
                    return true;
                }

                await Task.Delay((int)(interval * 1000));
                elapsed += interval;
            }

            return false;
        }

        // Helper method to load a scene
        protected async Task<Node> LoadScene(string scenePath)
        {
            var packedScene = ResourceLoader.Load<PackedScene>(scenePath);
            if (packedScene == null)
            {
                ErrorMessage = $"Failed to load scene: {scenePath}";
                return null;
            }

            var scene = packedScene.Instantiate();
            GetTree().Root.AddChild(scene);

            // Wait one frame for the scene to initialize
            await ToSignal(GetTree(), "process_frame");

            return scene;
        }

        // Helper method to unload a scene
        protected void UnloadScene(Node scene)
        {
            if (scene != null && scene.IsInsideTree())
            {
                scene.QueueFree();
            }
        }
    }
}
```

### 3. Example E2E Test Implementation

```csharp
// RadioTunerE2ETest.cs
using Godot;
using System;
using System.Threading.Tasks;

namespace SignalLost.Testing
{
    [GlobalClass]
    public partial class RadioTunerE2ETest : E2ETest
    {
        // Test name
        public override string Name => "Radio Tuner Test";

        // Scene to test
        private Node _mainScene;

        // References to nodes
        private PixelRadioInterface _radioInterface;
        private GameState _gameState;
        private RadioSystem _radioSystem;

        // Setup the test
        public override async Task Setup()
        {
            // Load the main scene
            _mainScene = await LoadScene("res://PixelMainScene.tscn");

            // Get references to nodes
            _radioInterface = _mainScene.GetNode<PixelRadioInterface>("PixelRadioInterface");
            _gameState = GetNode<GameState>("/root/GameState");
            _radioSystem = GetNode<RadioSystem>("/root/RadioSystem");

            // Ensure radio is visible
            _radioInterface.SetVisible(true);

            // Wait for everything to initialize
            await Task.Delay(500);
        }

        // Execute the test
        public override async Task<bool> Execute()
        {
            // Test 1: Turn on the radio
            GD.Print("Test 1: Turn on the radio");

            // Find power button position
            var powerButtonRect = _radioInterface.GetNode<Control>("PowerButton").GetGlobalRect();
            var powerButtonCenter = powerButtonRect.Position + powerButtonRect.Size / 2;

            // Click power button
            SimulateMouseClick(powerButtonCenter);

            // Wait for radio to turn on
            bool radioTurnedOn = await WaitForCondition(() => _gameState.IsRadioOn);

            if (!radioTurnedOn)
            {
                ErrorMessage = "Failed to turn on the radio";
                return false;
            }

            // Test 2: Tune to a specific frequency
            GD.Print("Test 2: Tune to a specific frequency");

            // Target frequency (one with a signal)
            float targetFrequency = 91.5f;

            // Find frequency slider position
            var sliderRect = _radioInterface.GetNode<Control>("FrequencySlider").GetGlobalRect();

            // Calculate position on slider for target frequency
            float frequencyRange = _radioSystem.MaxFrequency - _radioSystem.MinFrequency;
            float frequencyPercentage = (targetFrequency - _radioSystem.MinFrequency) / frequencyRange;
            float sliderX = sliderRect.Position.X + sliderRect.Size.X * frequencyPercentage;
            float sliderY = sliderRect.Position.Y + sliderRect.Size.Y / 2;

            // Click on slider at the calculated position
            SimulateMouseClick(new Vector2(sliderX, sliderY));

            // Wait for frequency to be set
            bool frequencySet = await WaitForCondition(() =>
                Math.Abs(_gameState.CurrentFrequency - targetFrequency) < 0.2f);

            if (!frequencySet)
            {
                ErrorMessage = $"Failed to tune to frequency {targetFrequency}";
                return false;
            }

            // Test 3: Verify signal detection
            GD.Print("Test 3: Verify signal detection");

            // Wait for signal to be detected
            bool signalDetected = await WaitForCondition(() => _radioSystem.GetSignalStrength() > 0.5f);

            if (!signalDetected)
            {
                ErrorMessage = "Failed to detect signal";
                return false;
            }

            // All tests passed
            return true;
        }

        // Clean up after the test
        public override async Task Cleanup()
        {
            // Turn off radio
            if (_gameState.IsRadioOn)
            {
                _gameState.ToggleRadio();
            }

            // Unload the scene
            UnloadScene(_mainScene);

            await Task.CompletedTask;
        }
    }
}
```

### 4. Test Runner Scene

Create a scene for running E2E tests:

```gdscript
# E2ETestRunner.gd
extends Node

# List of tests to run
var tests = []

# Test framework
var test_framework

func _ready():
    # Initialize test framework
    test_framework = $E2ETestFramework

    # Connect signals
    test_framework.connect("tests_completed", self, "_on_tests_completed")

    # Add tests
    tests.append(load("res://tests/RadioTunerE2ETest.cs").new())
    tests.append(load("res://tests/InventoryE2ETest.cs").new())
    tests.append(load("res://tests/MapE2ETest.cs").new())
    tests.append(load("res://tests/QuestE2ETest.cs").new())
    tests.append(load("res://tests/GameProgressionE2ETest.cs").new())

    # Run tests
    test_framework.run_all_tests(tests)

func _on_tests_completed(all_passed):
    print("All tests completed. All passed: ", all_passed)

    # Exit with appropriate code in headless mode
    if OS.has_feature("headless"):
        OS.exit(0 if all_passed else 1)
```

### 5. Command-Line Test Runner

Create a script for running tests from the command line:

```bash
#!/bin/bash
# run_e2e_tests.sh

# Get the directory of this script
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Find the Godot executable
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    GODOT_EXECUTABLE="/Applications/Godot_mono.app/Contents/MacOS/Godot"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Linux
    GODOT_EXECUTABLE="godot"
else
    # Windows with WSL or Git Bash
    GODOT_EXECUTABLE="godot.exe"
fi

# Check if Godot executable exists
if [ ! -f "$GODOT_EXECUTABLE" ] && ! command -v "$GODOT_EXECUTABLE" &> /dev/null; then
    echo "Godot executable not found at $GODOT_EXECUTABLE"
    echo "Please install Godot or update the script with the correct path."
    exit 1
fi

echo "Using Godot executable: $GODOT_EXECUTABLE"
echo "Running E2E tests for Signal Lost..."
echo "Project path: $DIR"

# Run the E2E test runner scene
"$GODOT_EXECUTABLE" --path "$DIR" --headless tests/E2ETestRunner.tscn

# Get the exit code
EXIT_CODE=$?

if [ $EXIT_CODE -eq 0 ]; then
    echo "All E2E tests passed!"
else
    echo "Some E2E tests failed. See above for details."
fi

exit $EXIT_CODE
```

## Test Scenarios

### 1. Radio Tuner Test

Test the radio tuner functionality:
- Turn on the radio
- Tune to specific frequencies
- Verify signal detection
- Test signal strength visualization
- Test audio playback

### 2. Inventory Test

Test the inventory system:
- Add items to inventory
- Select and use items
- Verify item effects
- Test inventory UI

### 3. Map Test

Test the map interface:
- Open the map
- Navigate to different locations
- Discover new locations
- Test map interactions

### 4. Quest Test

Test the quest system:
- Activate quests
- Complete quest objectives
- Receive quest rewards
- Test quest UI

### 5. Game Progression Test

Test the game progression system:
- Advance through game stages
- Unlock new content
- Test progression triggers
- Verify stage descriptions and objectives

## Visual Verification

Since E2E tests can be difficult to verify programmatically, we'll use screenshots for visual verification:

1. Take screenshots at key points during tests
2. Compare screenshots with expected results
3. Use AI-assisted verification for complex visual elements

## Headless Testing

For CI/CD integration, we'll run tests in headless mode:

```bash
godot --headless --script tests/run_e2e_tests.gd
```

This allows tests to run without a visible window, making them suitable for automated testing environments.

## Test Reports

Generate detailed test reports that include:
- Test name and status (pass/fail)
- Duration
- Error messages
- Screenshot paths
- Summary statistics

## Implementation Steps

1. Create the E2E test framework
2. Implement the base E2E test class
3. Create test scenarios for core gameplay features
4. Implement the test runner scene
5. Create command-line scripts for running tests
6. Set up visual verification
7. Integrate with CI/CD pipeline

## Success Criteria

1. All E2E tests pass consistently
2. Tests cover all core gameplay features
3. Tests run successfully on both Windows and Mac
4. Test reports provide clear information about test results
5. Visual verification confirms UI elements are displayed correctly
6. Tests can be run in headless mode for CI/CD integration

## Conclusion

By implementing this E2E testing plan, we can ensure that Signal Lost functions correctly as a complete system. The tests will verify that all components work together properly, providing confidence that the game delivers a consistent and enjoyable experience to players.

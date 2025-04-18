using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using GUT;
using Test = GUT.Test;

namespace SignalLost.Tests
{
    [GlobalClass]
    public partial class TestRunner : SceneTree
    {
        // This script runs all tests from the command line
        // Usage: godot --path /path/to/project --script Tests/TestRunner.cs

        // Timeout handling
        private const float TEST_TIMEOUT_SECONDS = 10.0f;
        private float _testTimer = 0.0f;
        private bool _testRunning = false;
        private string _currentTestName = "";
        private bool _timeoutOccurred = false;

        // Test tracking
        private int _totalTests = 0;
        private int _passedTests = 0;
        private int _failedTests = 0;
        private List<string> _failureMessages = new List<string>();
        private List<Type> _testClasses;
        private int _currentClassIndex = 0;
        private List<MethodInfo> _currentClassMethods;
        private int _currentMethodIndex = 0;
        private object _currentTestInstance;

        // Command line arguments
        private List<string> _skipClasses = new List<string>();

        public override void _Initialize()
        {
            GD.Print("Starting C# test runner...");

            // Parse command line arguments
            ParseCommandLineArguments();

            // Find all test classes
            _testClasses = FindTestClasses();
            GD.Print($"Found {_testClasses.Count} test classes");

            // Start running tests
            if (_testClasses.Count > 0)
            {
                StartTestClass(0);
            }
            else
            {
                // No tests to run
                PrintSummary();
                Quit(0);
            }
        }

        public override bool _Process(double delta)
        {
            // Check for test timeout
            if (_testRunning)
            {
                _testTimer += (float)delta;
                if (_testTimer > TEST_TIMEOUT_SECONDS)
                {
                    GD.PrintErr($"Test {_currentTestName} timed out after {TEST_TIMEOUT_SECONDS} seconds");
                    _failedTests++;
                    _failureMessages.Add($"TIMEOUT: {_currentTestName} - Test took too long to complete");
                    _testRunning = false;
                    _timeoutOccurred = true;

                    // Continue with the next test
                    ContinueAfterTimeout();
                }
            }
            return true; // Continue processing
        }

        private void StartTestClass(int classIndex)
        {
            if (classIndex >= _testClasses.Count)
            {
                // All classes have been tested
                PrintSummary();
                Quit(_failedTests > 0 ? 1 : 0);
                return;
            }

            _currentClassIndex = classIndex;
            var testClass = _testClasses[classIndex];

            GD.Print($"\nRunning tests in {testClass.Name}");

            // Create an instance of the test class
            _currentTestInstance = Activator.CreateInstance(testClass);
            if (_currentTestInstance is Node node)
            {
                GetRoot().AddChild(node);
            }

            // Find all test methods
            _currentClassMethods = FindTestMethods(testClass);
            GD.Print($"Found {_currentClassMethods.Count} test methods");

            // Start running methods
            _currentMethodIndex = 0;
            if (_currentClassMethods.Count > 0)
            {
                RunTestMethod(_currentClassMethods[0]);
            }
            else
            {
                // No methods in this class, move to next class
                CleanupTestClass();
                StartTestClass(_currentClassIndex + 1);
            }
        }

        private void RunTestMethod(MethodInfo method)
        {
            // Set up timeout tracking
            _testRunning = true;
            _testTimer = 0.0f;
            _timeoutOccurred = false;
            _currentTestName = $"{_testClasses[_currentClassIndex].Name}.{method.Name}";

            GD.Print($"\nRunning test: {method.Name}");
            _totalTests++;

            try
            {
                // Call Before method
                if (_currentTestInstance is GUT.Test testInstance)
                {
                    // Call Before method using reflection since it might not be available
                    var beforeMethod = testInstance.GetType().GetMethod("Before");
                    if (beforeMethod != null)
                    {
                        beforeMethod.Invoke(testInstance, null);
                    }
                }

                // Call the test method
                method.Invoke(_currentTestInstance, null);

                // Call After method
                if (_currentTestInstance is GUT.Test testInstance2)
                {
                    // Call After method using reflection since it might not be available
                    var afterMethod = testInstance2.GetType().GetMethod("After");
                    if (afterMethod != null)
                    {
                        afterMethod.Invoke(testInstance2, null);
                    }
                }

                GD.Print($"Test {method.Name} PASSED");
                _passedTests++;
            }
            catch (Exception ex)
            {
                string errorMessage = ex.InnerException?.Message ?? ex.Message;
                GD.Print($"Test {method.Name} FAILED: {errorMessage}");
                _failedTests++;
                _failureMessages.Add($"FAIL: {_currentTestName} - {errorMessage}");

                // Try to call After method even if the test failed
                try
                {
                    if (_currentTestInstance is GUT.Test testInstance)
                    {
                        // Call After method using reflection since it might not be available
                        var afterMethod = testInstance.GetType().GetMethod("After");
                        if (afterMethod != null)
                        {
                            afterMethod.Invoke(testInstance, null);
                        }
                    }
                }
                catch (Exception afterEx)
                {
                    GD.PrintErr($"Error in After method: {afterEx.Message}");
                }
            }
            finally
            {
                // Test is done, stop timeout tracking
                _testRunning = false;

                // Move to next test method or class
                if (!_timeoutOccurred) // Only continue if we didn't timeout
                {
                    ContinueToNextTest();
                }
            }
        }

        private void ContinueToNextTest()
        {
            _currentMethodIndex++;
            if (_currentMethodIndex < _currentClassMethods.Count)
            {
                // Run the next method
                RunTestMethod(_currentClassMethods[_currentMethodIndex]);
            }
            else
            {
                // All methods in this class have been run, move to next class
                CleanupTestClass();
                StartTestClass(_currentClassIndex + 1);
            }
        }

        private void ContinueAfterTimeout()
        {
            // Clean up the current test instance if needed
            if (_currentTestInstance is GUT.Test testInstance)
            {
                try
                {
                    // Call After method using reflection since it might not be available
                    var afterMethod = testInstance.GetType().GetMethod("After");
                    if (afterMethod != null)
                    {
                        afterMethod.Invoke(testInstance, null);
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Error in After method after timeout: {ex.Message}");
                }
            }

            // Continue to the next test
            ContinueToNextTest();
        }

        private void CleanupTestClass()
        {
            // Clean up the test instance
            if (_currentTestInstance is Node node)
            {
                node.QueueFree();
            }
            _currentTestInstance = null;
        }

        private void PrintSummary()
        {
            GD.Print($"\n===== TEST SUMMARY =====");
            GD.Print($"Total tests: {_totalTests}");
            GD.Print($"Passed: {_passedTests}");
            GD.Print($"Failed: {_failedTests}");

            if (_failedTests > 0)
            {
                GD.Print("\nFailed tests:");
                foreach (var message in _failureMessages)
                {
                    GD.Print($"  {message}");
                }
            }
        }

        private void ParseCommandLineArguments()
        {
            var args = OS.GetCmdlineArgs();

            // Default skip classes
            _skipClasses = new List<string>
            {
                "IntegrationTests",
                "RadioTunerTests",
                "PixelInventoryUITests",
                "PixelMapInterfaceTests",
                "QuestSystemTests",
                "PixelRadioInterfaceTests"
            };

            // Parse command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--skip-classes" && i + 1 < args.Length)
                {
                    var skipClassesArg = args[i + 1];
                    var skipClassesList = skipClassesArg.Split(',');
                    foreach (var className in skipClassesList)
                    {
                        if (!_skipClasses.Contains(className))
                        {
                            _skipClasses.Add(className);
                        }
                    }
                }
            }
        }

        private List<Type> FindTestClasses()
        {
            var testClasses = new List<Type>();
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                // Skip classes in the skip list
                if (_skipClasses.Contains(type.Name))
                {
                    continue;
                }

                // Check for TestClass attribute
                if (type.GetCustomAttribute<Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute>() != null)
                {
                    testClasses.Add(type);
                }
                // Also check for class names ending with "Tests"
                else if (type.Name.EndsWith("Tests") && type.IsSubclassOf(typeof(GUT.Test)))
                {
                    testClasses.Add(type);
                }
            }

            return testClasses;
        }

        private static List<MethodInfo> FindTestMethods(Type testClass)
        {
            var testMethods = new List<MethodInfo>();

            foreach (var method in testClass.GetMethods())
            {
                // Check for Test attribute
                if (method.GetCustomAttribute<Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute>() != null)
                {
                    testMethods.Add(method);
                }
                // Also check for method names starting with "Test"
                else if (method.Name.StartsWith("Test") && method.GetParameters().Length == 0)
                {
                    testMethods.Add(method);
                }
            }

            return testMethods;
        }
    }
}

using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using GUT;

[GlobalClass]
namespace SignalLost.Tests
{
    public partial class TestRunner : SceneTree
    {
        // This script runs all tests from the command line
        // Usage: godot --path /path/to/project --script Tests/TestRunner.cs

        public override void _Initialize()
        {
            GD.Print("Starting C# test runner...");

            // Find all test classes
            var testClasses = FindTestClasses();
            GD.Print($"Found {testClasses.Count} test classes");

            int totalTests = 0;
            int passedTests = 0;
            int failedTests = 0;

            // Run tests in each class
            foreach (var testClass in testClasses)
            {
                GD.Print($"\nRunning tests in {testClass.Name}");

                // Create an instance of the test class
                var testInstance = (Test)Activator.CreateInstance(testClass);
                GetRoot().AddChild(testInstance);

                // Find all test methods
                var testMethods = FindTestMethods(testClass);
                GD.Print($"Found {testMethods.Count} test methods");

                // Run each test method
                foreach (var method in testMethods)
                {
                    GD.Print($"\nRunning test: {method.Name}");
                    totalTests++;

                    try
                    {
                        // Call Before method
                        testInstance.Before();

                        // Call the test method
                        method.Invoke(testInstance, null);

                        // Call After method
                        testInstance.After();

                        GD.Print($"Test {method.Name} PASSED");
                        passedTests++;
                    }
                    catch (Exception ex)
                    {
                        GD.Print($"Test {method.Name} FAILED: {ex.InnerException?.Message ?? ex.Message}");
                        failedTests++;
                    }
                }

                // Clean up
                testInstance.QueueFree();
            }

            // Print summary
            GD.Print($"\n===== TEST SUMMARY =====");
            GD.Print($"Total tests: {totalTests}");
            GD.Print($"Passed: {passedTests}");
            GD.Print($"Failed: {failedTests}");

            // Exit with appropriate code
            Quit(failedTests > 0 ? 1 : 0);
        }

        private static List<Type> FindTestClasses()
        {
            var testClasses = new List<Type>();
            var assembly = Assembly.GetExecutingAssembly();

            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<TestClassAttribute>() != null)
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
                if (method.GetCustomAttribute<TestAttribute>() != null)
                {
                    testMethods.Add(method);
                }
            }

            return testMethods;
        }
    }
}

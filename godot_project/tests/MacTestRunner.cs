using Godot;
using System;
using System.Reflection;
using System.Collections.Generic;
using GUT;
using SignalLost.Tests;
// Use GUT's TestClassAttribute instead of Microsoft's
// using Microsoft.VisualStudio.TestTools.UnitTesting;

[GlobalClass]
public partial class MacTestRunner : Node
{
    // This script runs all tests and outputs results to the console

    public override void _Ready()
    {
        GD.Print("Starting Mac C# Test Runner...");

        // Run tests
        RunTests();

        // Exit when done
        GetTree().Quit();
    }

    private void RunTests()
    {
        int totalTests = 0;
        int passedTests = 0;
        int failedTests = 0;
        List<string> failureMessages = new List<string>();

        // Find all test classes
        var testClasses = FindTestClasses();
        GD.Print($"Found {testClasses.Count} test classes");

        foreach (var testClass in testClasses)
        {
            GD.Print($"\nRunning tests in {testClass.Name}");

            // Create an instance of the test class
            object testInstance = Activator.CreateInstance(testClass);

            // Add to scene tree if it's a Node
            if (testInstance is Node node)
            {
                AddChild(node);
            }

            // Find all test methods
            var testMethods = FindTestMethods(testClass);
            GD.Print($"Found {testMethods.Count} test methods");

            foreach (var method in testMethods)
            {
                string testName = $"{testClass.Name}.{method.Name}";
                GD.Print($"\nRunning test: {method.Name}");
                totalTests++;

                try
                {
                    // Call Before method if it exists
                    var beforeMethod = testInstance.GetType().GetMethod("Before");
                    if (beforeMethod != null)
                    {
                        beforeMethod.Invoke(testInstance, null);
                    }

                    // Call the test method
                    method.Invoke(testInstance, null);

                    // Call After method if it exists
                    var afterMethod = testInstance.GetType().GetMethod("After");
                    if (afterMethod != null)
                    {
                        afterMethod.Invoke(testInstance, null);
                    }

                    GD.Print($"Test {method.Name} PASSED");
                    passedTests++;
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.InnerException?.Message ?? ex.Message;
                    GD.PrintErr($"Test {method.Name} FAILED: {errorMessage}");
                    failedTests++;
                    failureMessages.Add($"FAIL: {testName} - {errorMessage}");

                    // Try to call After method even if the test failed
                    try
                    {
                        // Call After method if it exists
                        var afterMethod = testInstance.GetType().GetMethod("After");
                        if (afterMethod != null)
                        {
                            afterMethod.Invoke(testInstance, null);
                        }
                    }
                    catch (Exception afterEx)
                    {
                        GD.PrintErr($"Error in After method: {afterEx.Message}");
                    }
                }
            }

            // Clean up the test instance
            if (testInstance is Node nodeInstance)
            {
                nodeInstance.QueueFree();
            }
        }

        // Print summary
        GD.Print($"\n===== TEST SUMMARY =====");
        GD.Print($"Total tests: {totalTests}");
        GD.Print($"Passed: {passedTests}");
        GD.Print($"Failed: {failedTests}");

        if (failedTests > 0)
        {
            GD.Print("\nFailed tests:");
            foreach (var message in failureMessages)
            {
                GD.Print($"  {message}");
            }
        }
    }

    private List<Type> FindTestClasses()
    {
        var testClasses = new List<Type>();
        var assembly = Assembly.GetExecutingAssembly();

        foreach (var type in assembly.GetTypes())
        {
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

    private List<MethodInfo> FindTestMethods(Type testClass)
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

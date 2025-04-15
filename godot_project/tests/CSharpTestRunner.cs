using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SignalLost.Tests
{
    public partial class CSharpTestRunner : Node
    {
        private int _totalTests = 0;
        private int _passedTests = 0;
        private int _failedTests = 0;
        private List<string> _failureMessages = new List<string>();
        
        public override void _Ready()
        {
            GD.Print("Starting C# Test Runner...");
            
            // Run all tests
            RunAllTests();
            
            // Print summary
            PrintSummary();
            
            // Exit with appropriate code
            if (_failedTests > 0)
            {
                GD.Print("Tests failed. Exiting with code 1.");
                GetTree().Quit(1);
            }
            else
            {
                GD.Print("All tests passed. Exiting with code 0.");
                GetTree().Quit(0);
            }
        }
        
        private void RunAllTests()
        {
            GD.Print("Running all C# tests...");
            
            // Get all types in the current assembly
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes())
            {
                // Skip non-test classes
                if (!type.Name.Contains("Test") || type == typeof(CSharpTestRunner))
                {
                    continue;
                }
                
                GD.Print($"Running tests in {type.Name}...");
                
                // Create an instance of the test class
                object instance = Activator.CreateInstance(type);
                
                // Find all test methods
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    // Skip non-test methods
                    if (!method.Name.StartsWith("Test"))
                    {
                        continue;
                    }
                    
                    _totalTests++;
                    
                    try
                    {
                        // Run setup method if it exists
                        MethodInfo setup = type.GetMethod("Setup");
                        if (setup != null)
                        {
                            setup.Invoke(instance, null);
                        }
                        
                        // Run the test method
                        GD.Print($"  Running test: {method.Name}");
                        method.Invoke(instance, null);
                        
                        // If we get here, the test passed
                        _passedTests++;
                        GD.Print($"  PASS: {method.Name}");
                        
                        // Run teardown method if it exists
                        MethodInfo teardown = type.GetMethod("Teardown");
                        if (teardown != null)
                        {
                            teardown.Invoke(instance, null);
                        }
                    }
                    catch (Exception e)
                    {
                        // Test failed
                        _failedTests++;
                        string message = $"FAIL: {type.Name}.{method.Name} - {e.InnerException?.Message ?? e.Message}";
                        _failureMessages.Add(message);
                        GD.PrintErr(message);
                        
                        // Run teardown method if it exists
                        MethodInfo teardown = type.GetMethod("Teardown");
                        if (teardown != null)
                        {
                            try
                            {
                                teardown.Invoke(instance, null);
                            }
                            catch (Exception teardownException)
                            {
                                GD.PrintErr($"  Error in teardown: {teardownException.Message}");
                            }
                        }
                    }
                }
            }
        }
        
        private void PrintSummary()
        {
            GD.Print("\n=== Test Summary ===");
            GD.Print($"Total tests: {_totalTests}");
            GD.Print($"Passed: {_passedTests}");
            GD.Print($"Failed: {_failedTests}");
            
            if (_failedTests > 0)
            {
                GD.Print("\n=== Failed Tests ===");
                foreach (string message in _failureMessages)
                {
                    GD.PrintErr(message);
                }
            }
        }
    }
}

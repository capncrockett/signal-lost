using Godot;
using System;

namespace SignalLost
{
    [GlobalClass]
    public partial class RadioInterfaceTest : Node
    {
        // Reference to the RadioTuner
        private RadioTuner _radioTuner;
        
        // Reference to the UI interfaces
        private PixelRadioInterface _pixelInterface;
        private EnhancedRadioInterface _enhancedInterface;
        
        // Test results
        private bool _allTestsPassed = true;
        private string _testResults = "";
        
        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("Starting Radio Interface Tests...");
            
            // Find the RadioTuner
            _radioTuner = GetNode<RadioTuner>("../RadioTuner");
            
            if (_radioTuner == null)
            {
                ReportFailure("Could not find RadioTuner node");
                return;
            }
            
            // Find the interfaces
            _pixelInterface = _radioTuner.GetNode<PixelRadioInterface>("PixelRadioInterface");
            _enhancedInterface = _radioTuner.GetNode<EnhancedRadioInterface>("EnhancedRadioInterface");
            
            // Run tests
            RunTests();
            
            // Report results
            if (_allTestsPassed)
            {
                GD.Print("All tests passed!");
            }
            else
            {
                GD.PrintErr("Some tests failed!");
                GD.PrintErr(_testResults);
            }
        }
        
        // Run all tests
        private void RunTests()
        {
            // Test RadioTuner methods
            TestRadioTunerMethods();
            
            // Test PixelRadioInterface if available
            if (_pixelInterface != null)
            {
                TestPixelRadioInterface();
            }
            else
            {
                GD.Print("PixelRadioInterface not found, skipping tests");
            }
            
            // Test EnhancedRadioInterface if available
            if (_enhancedInterface != null)
            {
                TestEnhancedRadioInterface();
            }
            else
            {
                GD.Print("EnhancedRadioInterface not found, skipping tests");
            }
        }
        
        // Test RadioTuner methods
        private void TestRadioTunerMethods()
        {
            GD.Print("Testing RadioTuner methods...");
            
            // Test power methods
            _radioTuner.SetPower(true);
            AssertTrue(_radioTuner.IsPowerOn(), "RadioTuner.SetPower(true) should set power to on");
            
            _radioTuner.SetPower(false);
            AssertFalse(_radioTuner.IsPowerOn(), "RadioTuner.SetPower(false) should set power to off");
            
            _radioTuner.TogglePower();
            AssertTrue(_radioTuner.IsPowerOn(), "RadioTuner.TogglePower() should toggle power state");
            
            // Test frequency methods
            float testFreq = 95.5f;
            _radioTuner.SetFrequency(testFreq);
            AssertEqual(_radioTuner.GetCurrentFrequency(), testFreq, "RadioTuner.SetFrequency() should set the frequency");
            
            _radioTuner.ChangeFrequency(1.0f);
            AssertEqual(_radioTuner.GetCurrentFrequency(), testFreq + 1.0f, "RadioTuner.ChangeFrequency() should change the frequency");
            
            // Test scanning methods
            _radioTuner.SetScanning(true);
            AssertTrue(_radioTuner.IsScanning(), "RadioTuner.SetScanning(true) should set scanning to on");
            
            _radioTuner.SetScanning(false);
            AssertFalse(_radioTuner.IsScanning(), "RadioTuner.SetScanning(false) should set scanning to off");
            
            _radioTuner.ToggleScanning();
            AssertTrue(_radioTuner.IsScanning(), "RadioTuner.ToggleScanning() should toggle scanning state");
            
            // Reset state
            _radioTuner.SetScanning(false);
            _radioTuner.SetFrequency(100.0f);
        }
        
        // Test PixelRadioInterface
        private void TestPixelRadioInterface()
        {
            GD.Print("Testing PixelRadioInterface...");
            
            // Verify that the interface exists and is visible
            AssertTrue(_pixelInterface.Visible, "PixelRadioInterface should be visible");
            
            // Test UI interaction (simulated)
            GD.Print("PixelRadioInterface tests completed");
        }
        
        // Test EnhancedRadioInterface
        private void TestEnhancedRadioInterface()
        {
            GD.Print("Testing EnhancedRadioInterface...");
            
            // Verify that the interface exists and is visible
            AssertTrue(_enhancedInterface.Visible, "EnhancedRadioInterface should be visible");
            
            // Test UI interaction (simulated)
            GD.Print("EnhancedRadioInterface tests completed");
        }
        
        // Helper methods for assertions
        private void AssertTrue(bool condition, string message)
        {
            if (!condition)
            {
                ReportFailure(message);
            }
            else
            {
                GD.Print($"PASS: {message}");
            }
        }
        
        private void AssertFalse(bool condition, string message)
        {
            if (condition)
            {
                ReportFailure(message);
            }
            else
            {
                GD.Print($"PASS: {message}");
            }
        }
        
        private void AssertEqual(float actual, float expected, string message)
        {
            if (Math.Abs(actual - expected) > 0.001f)
            {
                ReportFailure($"{message} - Expected: {expected}, Actual: {actual}");
            }
            else
            {
                GD.Print($"PASS: {message}");
            }
        }
        
        private void ReportFailure(string message)
        {
            _allTestsPassed = false;
            string failureMessage = $"FAIL: {message}";
            GD.PrintErr(failureMessage);
            _testResults += failureMessage + "\n";
        }
    }
}

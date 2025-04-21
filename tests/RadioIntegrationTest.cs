using Godot;
using System;
using SignalLost;
using SignalLost.UI;
using SignalLost.Audio;

/// <summary>
/// Integration test for the radio interface.
/// </summary>
[GlobalClass]
public partial class RadioIntegrationTest : Node
{
    // Test results
    private bool _allTestsPassed = true;
    private int _totalTests = 0;
    private int _passedTests = 0;

    // Test components
    private PixelRadioInterface _radioInterface;
    private RadioInterfaceManager _radioInterfaceManager;
    private RadioAudioManager _radioAudioManager;
    private GameState _gameState;
    private RadioSystem _radioSystem;

    // Called when the node enters the scene tree
    public override void _Ready()
    {
        GD.PrintRich("[color=yellow][b]RadioIntegrationTest: Starting tests...[/b][/color]");
        Console.WriteLine("RadioIntegrationTest: Starting tests from console...");

        // Create test environment
        SetupTestEnvironment();

        // Run tests
        RunTests();

        // Report results
        ReportResults();
    }

    // Set up the test environment
    private void SetupTestEnvironment()
    {
        try
        {
            // Create game systems if they don't exist
            _gameState = GetNode<GameState>("/root/GameState");
            if (_gameState == null)
            {
                _gameState = new GameState();
                _gameState.Name = "GameState";
                GetTree().Root.AddChild(_gameState);
                GD.Print("RadioIntegrationTest: Created GameState");
            }

            _radioSystem = GetNode<RadioSystem>("/root/RadioSystem");
            if (_radioSystem == null)
            {
                _radioSystem = new RadioSystem();
                _radioSystem.Name = "RadioSystem";
                GetTree().Root.AddChild(_radioSystem);
                GD.Print("RadioIntegrationTest: Created RadioSystem");
            }

            // Create test scene
            _radioInterface = new PixelRadioInterface();
            _radioInterface.Name = "PixelRadioInterface";
            AddChild(_radioInterface);

            _radioInterfaceManager = new RadioInterfaceManager();
            _radioInterfaceManager.Name = "RadioInterfaceManager";
            _radioInterfaceManager.RadioInterfacePath = new NodePath("../PixelRadioInterface");
            AddChild(_radioInterfaceManager);

            _radioAudioManager = new RadioAudioManager();
            _radioAudioManager.Name = "RadioAudioManager";
            AddChild(_radioAudioManager);

            // Initialize test data
            InitializeTestData();

            GD.Print("RadioIntegrationTest: Test environment set up");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"RadioIntegrationTest: Error setting up test environment: {ex.Message}");
            GD.PrintErr(ex.StackTrace);
            _allTestsPassed = false;
        }
    }

    // Initialize test data
    private void InitializeTestData()
    {
        // Add test signals
        if (_radioSystem != null)
        {
            _radioSystem.AddSignal(new RadioSignal
            {
                Id = "test_signal_1",
                Frequency = 91.5f,
                Message = "Test Signal 1",
                Type = RadioSignalType.Morse,
                Strength = 0.8f
            });

            _radioSystem.AddSignal(new RadioSignal
            {
                Id = "test_signal_2",
                Frequency = 95.7f,
                Message = "Test Signal 2",
                Type = RadioSignalType.Voice,
                Strength = 0.6f
            });

            _radioSystem.AddSignal(new RadioSignal
            {
                Id = "test_signal_3",
                Frequency = 103.2f,
                Message = "Test Signal 3",
                Type = RadioSignalType.Data,
                Strength = 0.9f
            });
        }

        // Add test messages
        if (_gameState != null)
        {
            _gameState.Messages["msg_test_1"] = new GameState.MessageData
            {
                Id = "msg_test_1",
                Title = "Test Message 1",
                Content = "This is test message 1.",
                Decoded = false
            };

            _gameState.Messages["msg_test_2"] = new GameState.MessageData
            {
                Id = "msg_test_2",
                Title = "Test Message 2",
                Content = "This is test message 2.",
                Decoded = false
            };

            _gameState.Messages["msg_test_3"] = new GameState.MessageData
            {
                Id = "msg_test_3",
                Title = "Test Message 3",
                Content = "This is test message 3.",
                Decoded = true
            };
        }
    }

    // Run all tests
    private void RunTests()
    {
        // Test radio interface initialization
        TestRadioInterfaceInitialization();

        // Test radio power toggle
        TestRadioPowerToggle();

        // Test frequency change
        TestFrequencyChange();

        // Test signal strength
        TestSignalStrength();

        // Test message availability
        TestMessageAvailability();
    }

    // Test radio interface initialization
    private void TestRadioInterfaceInitialization()
    {
        _totalTests++;

        try
        {
            // Check if radio interface is initialized
            if (_radioInterface != null)
            {
                GD.Print("RadioIntegrationTest: Radio interface initialized successfully");
                _passedTests++;
            }
            else
            {
                GD.PrintErr("RadioIntegrationTest: Radio interface initialization failed");
                _allTestsPassed = false;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"RadioIntegrationTest: Error in TestRadioInterfaceInitialization: {ex.Message}");
            _allTestsPassed = false;
        }
    }

    // Test radio power toggle
    private void TestRadioPowerToggle()
    {
        _totalTests++;

        try
        {
            // Get initial power state
            bool initialPowerState = _gameState.IsRadioOn;

            // Toggle power
            _gameState.ToggleRadio();

            // Check if power state changed
            if (_gameState.IsRadioOn != initialPowerState)
            {
                GD.Print("RadioIntegrationTest: Radio power toggle successful");
                _passedTests++;
            }
            else
            {
                GD.PrintErr("RadioIntegrationTest: Radio power toggle failed");
                _allTestsPassed = false;
            }

            // Reset power state
            if (_gameState.IsRadioOn != initialPowerState)
            {
                _gameState.ToggleRadio();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"RadioIntegrationTest: Error in TestRadioPowerToggle: {ex.Message}");
            _allTestsPassed = false;
        }
    }

    // Test frequency change
    private void TestFrequencyChange()
    {
        _totalTests++;

        try
        {
            // Get initial frequency
            float initialFrequency = _gameState.CurrentFrequency;

            // Change frequency
            float newFrequency = 95.7f;
            _gameState.SetFrequency(newFrequency);

            // Check if frequency changed
            if (Mathf.IsEqualApprox(_gameState.CurrentFrequency, newFrequency))
            {
                GD.Print("RadioIntegrationTest: Frequency change successful");
                _passedTests++;
            }
            else
            {
                GD.PrintErr($"RadioIntegrationTest: Frequency change failed. Expected: {newFrequency}, Actual: {_gameState.CurrentFrequency}");
                _allTestsPassed = false;
            }

            // Reset frequency
            _gameState.SetFrequency(initialFrequency);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"RadioIntegrationTest: Error in TestFrequencyChange: {ex.Message}");
            _allTestsPassed = false;
        }
    }

    // Test signal strength
    private void TestSignalStrength()
    {
        _totalTests++;

        try
        {
            // Turn on radio
            if (!_gameState.IsRadioOn)
            {
                _gameState.ToggleRadio();
            }

            // Set frequency to a known signal
            _gameState.SetFrequency(91.5f);

            // Wait for signal strength to update
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

            // Check signal strength
            float signalStrength = _radioSystem.GetSignalStrength();
            if (signalStrength > 0.0f)
            {
                GD.Print($"RadioIntegrationTest: Signal strength test successful. Strength: {signalStrength}");
                _passedTests++;
            }
            else
            {
                GD.PrintErr($"RadioIntegrationTest: Signal strength test failed. Strength: {signalStrength}");
                _allTestsPassed = false;
            }

            // Turn off radio
            if (_gameState.IsRadioOn)
            {
                _gameState.ToggleRadio();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"RadioIntegrationTest: Error in TestSignalStrength: {ex.Message}");
            _allTestsPassed = false;
        }
    }

    // Test message availability
    private void TestMessageAvailability()
    {
        _totalTests++;

        try
        {
            // Turn on radio
            if (!_gameState.IsRadioOn)
            {
                _gameState.ToggleRadio();
            }

            // Set frequency to a known signal
            _gameState.SetFrequency(91.5f);

            // Wait for message availability to update
            await ToSignal(GetTree().CreateTimer(0.1f), "timeout");

            // Check if message is available
            bool messageAvailable = false;
            var signalData = _gameState.FindSignalAtFrequency(_gameState.CurrentFrequency);
            if (signalData != null && !string.IsNullOrEmpty(signalData.MessageId))
            {
                var message = _gameState.GetMessage(signalData.MessageId);
                messageAvailable = message != null;
            }

            if (messageAvailable)
            {
                GD.Print("RadioIntegrationTest: Message availability test successful");
                _passedTests++;
            }
            else
            {
                GD.PrintErr("RadioIntegrationTest: Message availability test failed");
                _allTestsPassed = false;
            }

            // Turn off radio
            if (_gameState.IsRadioOn)
            {
                _gameState.ToggleRadio();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"RadioIntegrationTest: Error in TestMessageAvailability: {ex.Message}");
            _allTestsPassed = false;
        }
    }

    // Report test results
    private void ReportResults()
    {
        GD.Print($"RadioIntegrationTest: {_passedTests}/{_totalTests} tests passed");

        if (_allTestsPassed)
        {
            GD.Print("RadioIntegrationTest: All tests passed!");
        }
        else
        {
            GD.PrintErr("RadioIntegrationTest: Some tests failed. See above for details.");
        }

        // Exit with appropriate code
        GetTree().Quit(_allTestsPassed ? 0 : 1);
    }
}

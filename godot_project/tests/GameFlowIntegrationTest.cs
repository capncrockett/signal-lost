using Godot;
using System;
using System.Threading.Tasks;

namespace SignalLost.Tests
{
    /// <summary>
    /// Integration test for game flow, testing navigation between scenes and UI components.
    /// </summary>
    [GlobalClass]
    public partial class GameFlowIntegrationTest : Node
    {
        // Test status
        private bool _testRunning = false;
        private string _testStatus = "Ready";
        private string _testDetails = "";
        private int _testsPassed = 0;
        private int _testsFailed = 0;
        private int _testsSkipped = 0;

        // UI elements
        private Button _runTestsButton;
        private Label _statusLabel;
        private Label _detailsLabel;
        private ProgressBar _progressBar;

        // Test scenes
        private PackedScene _mainMenuScene;
        private PackedScene _mainGameScene;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get UI elements
            _runTestsButton = GetNode<Button>("VBoxContainer/RunTestsButton");
            _statusLabel = GetNode<Label>("VBoxContainer/StatusLabel");
            _detailsLabel = GetNode<Label>("VBoxContainer/DetailsLabel");
            _progressBar = GetNode<ProgressBar>("VBoxContainer/ProgressBar");

            // Connect signals
            _runTestsButton.Pressed += OnRunTestsButtonPressed;

            // Load test scenes
            _mainMenuScene = GD.Load<PackedScene>("res://scenes/gameplay/MainMenu.tscn");
            _mainGameScene = GD.Load<PackedScene>("res://scenes/MainGameScene.tscn");

            // Initialize UI
            UpdateUI();
        }

        // Called when the run tests button is pressed
        private void OnRunTestsButtonPressed()
        {
            if (_testRunning)
                return;

            _testRunning = true;
            _testsPassed = 0;
            _testsFailed = 0;
            _testsSkipped = 0;
            _progressBar.Value = 0;

            // Run tests asynchronously
            RunTestsAsync();
        }

        // Run all tests asynchronously
        private async void RunTestsAsync()
        {
            _testStatus = "Running tests...";
            UpdateUI();

            // Run tests
            await TestMainMenuToGameNavigation();
            await TestGameUINavigation();
            await TestSaveLoadFunctionality();

            // Tests complete
            _testStatus = $"Tests complete: {_testsPassed} passed, {_testsFailed} failed, {_testsSkipped} skipped";
            _testRunning = false;
            UpdateUI();
        }

        // Test navigation from main menu to game
        private async Task TestMainMenuToGameNavigation()
        {
            _testDetails = "Testing main menu to game navigation...";
            UpdateUI();

            try
            {
                // Load main menu scene
                var mainMenu = _mainMenuScene.Instantiate<Control>();
                GetTree().Root.AddChild(mainMenu);

                // Wait for scene to initialize
                await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

                // Find new game button
                var newGameButton = mainMenu.GetNode<Button>("MarginContainer/VBoxContainer/MenuButtons/NewGameButton");
                if (newGameButton == null)
                {
                    LogTestFailure("New game button not found");
                    mainMenu.QueueFree();
                    return;
                }

                // Press new game button
                newGameButton.EmitSignal("pressed");

                // Wait for scene change
                await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

                // Check if main game scene is loaded
                var mainGameController = GetTree().Root.GetNodeOrNull<MainGameController>("MainGameScene");
                if (mainGameController == null)
                {
                    LogTestFailure("Main game scene not loaded after pressing new game button");
                    return;
                }

                LogTestSuccess("Successfully navigated from main menu to game");

                // Clean up
                if (mainGameController != null)
                {
                    mainGameController.GetParent().QueueFree();
                }
            }
            catch (Exception ex)
            {
                LogTestFailure($"Exception during test: {ex.Message}");
            }
        }

        // Test navigation between UI components in the game
        private async Task TestGameUINavigation()
        {
            _testDetails = "Testing game UI navigation...";
            UpdateUI();

            try
            {
                // Load main game scene
                var mainGameScene = _mainGameScene.Instantiate<Node>();
                GetTree().Root.AddChild(mainGameScene);

                // Wait for scene to initialize
                await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

                // Get UI components
                var mainGameController = mainGameScene as MainGameController;
                if (mainGameController == null)
                {
                    LogTestFailure("MainGameController not found");
                    mainGameScene.QueueFree();
                    return;
                }

                var radioInterface = mainGameScene.GetNodeOrNull<Control>("PixelRadioInterface");
                var inventoryUI = mainGameScene.GetNodeOrNull<Control>("PixelInventoryUI");
                var mapInterface = mainGameScene.GetNodeOrNull<Control>("PixelMapInterface");
                var questUI = mainGameScene.GetNodeOrNull<Control>("PixelQuestUI");

                if (radioInterface == null || inventoryUI == null || mapInterface == null || questUI == null)
                {
                    LogTestFailure("One or more UI components not found");
                    mainGameScene.QueueFree();
                    return;
                }

                // Test radio button
                var radioButton = mainGameScene.GetNode<Button>("UIControls/RadioButton");
                radioButton.EmitSignal("pressed");
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                if (!radioInterface.Visible || inventoryUI.Visible || mapInterface.Visible || questUI.Visible)
                {
                    LogTestFailure("Radio interface not shown correctly");
                }
                else
                {
                    LogTestSuccess("Radio interface shown correctly");
                }

                // Test inventory button
                var inventoryButton = mainGameScene.GetNode<Button>("UIControls/InventoryButton");
                inventoryButton.EmitSignal("pressed");
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                if (radioInterface.Visible || !inventoryUI.Visible || mapInterface.Visible || questUI.Visible)
                {
                    LogTestFailure("Inventory UI not shown correctly");
                }
                else
                {
                    LogTestSuccess("Inventory UI shown correctly");
                }

                // Test map button
                var mapButton = mainGameScene.GetNode<Button>("UIControls/MapButton");
                mapButton.EmitSignal("pressed");
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                if (radioInterface.Visible || inventoryUI.Visible || !mapInterface.Visible || questUI.Visible)
                {
                    LogTestFailure("Map interface not shown correctly");
                }
                else
                {
                    LogTestSuccess("Map interface shown correctly");
                }

                // Test quest button
                var questButton = mainGameScene.GetNode<Button>("UIControls/QuestButton");
                questButton.EmitSignal("pressed");
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                if (radioInterface.Visible || inventoryUI.Visible || mapInterface.Visible || !questUI.Visible)
                {
                    LogTestFailure("Quest UI not shown correctly");
                }
                else
                {
                    LogTestSuccess("Quest UI shown correctly");
                }

                // Clean up
                mainGameScene.QueueFree();
            }
            catch (Exception ex)
            {
                LogTestFailure($"Exception during test: {ex.Message}");
            }
        }

        // Test save/load functionality
        private async Task TestSaveLoadFunctionality()
        {
            _testDetails = "Testing save/load functionality...";
            UpdateUI();

            try
            {
                // Load main game scene
                var mainGameScene = _mainGameScene.Instantiate<Node>();
                GetTree().Root.AddChild(mainGameScene);

                // Wait for scene to initialize
                await ToSignal(GetTree().CreateTimer(1.0f), "timeout");

                // Get save/load menu
                var saveLoadMenu = mainGameScene.GetNodeOrNull<SaveLoadMenu>("SaveLoadMenu");
                if (saveLoadMenu == null)
                {
                    LogTestFailure("SaveLoadMenu not found");
                    mainGameScene.QueueFree();
                    return;
                }

                // Test save/load button
                var saveLoadButton = mainGameScene.GetNode<Button>("UIControls/SaveLoadButton");
                saveLoadButton.EmitSignal("pressed");
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                if (!saveLoadMenu.Visible)
                {
                    LogTestFailure("Save/load menu not shown correctly");
                }
                else
                {
                    LogTestSuccess("Save/load menu shown correctly");
                }

                // Test save functionality
                var saveButton = saveLoadMenu.GetNode<Button>("VBoxContainer/ButtonContainer/SaveButton");
                saveButton.EmitSignal("pressed");
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");

                // Test close button
                var closeButton = saveLoadMenu.GetNode<Button>("VBoxContainer/ButtonContainer/CloseButton");
                closeButton.EmitSignal("pressed");
                await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
                if (saveLoadMenu.Visible)
                {
                    LogTestFailure("Save/load menu not hidden correctly");
                }
                else
                {
                    LogTestSuccess("Save/load menu hidden correctly");
                }

                // Clean up
                mainGameScene.QueueFree();
            }
            catch (Exception ex)
            {
                LogTestFailure($"Exception during test: {ex.Message}");
            }
        }

        // Log a test success
        private void LogTestSuccess(string message)
        {
            _testsPassed++;
            _testDetails = $"✓ {message}";
            _progressBar.Value = (_testsPassed + _testsFailed + _testsSkipped) / (float)GetTotalTestCount() * 100;
            UpdateUI();
            GD.Print($"[PASS] {message}");
        }

        // Log a test failure
        private void LogTestFailure(string message)
        {
            _testsFailed++;
            _testDetails = $"✗ {message}";
            _progressBar.Value = (_testsPassed + _testsFailed + _testsSkipped) / (float)GetTotalTestCount() * 100;
            UpdateUI();
            GD.PrintErr($"[FAIL] {message}");
        }

        // Log a skipped test
        private void LogTestSkipped(string message)
        {
            _testsSkipped++;
            _testDetails = $"⚠ {message}";
            _progressBar.Value = (_testsPassed + _testsFailed + _testsSkipped) / (float)GetTotalTestCount() * 100;
            UpdateUI();
            GD.Print($"[SKIP] {message}");
        }

        // Get the total number of tests
        private int GetTotalTestCount()
        {
            // Update this when adding new tests
            return 7; // 1 for navigation + 4 for UI components + 2 for save/load
        }

        // Update the UI
        private void UpdateUI()
        {
            _statusLabel.Text = _testStatus;
            _detailsLabel.Text = _testDetails;
            _runTestsButton.Disabled = _testRunning;
        }
    }
}

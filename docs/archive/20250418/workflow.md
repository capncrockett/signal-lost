# Signal Lost Workflow

## Development Process

1. **Pull latest changes**
   ```bash
   git checkout develop
   git pull
   ```

2. **Create a feature branch**
   ```bash
   git checkout -b feature/agent/description
   ```
   - Replace `agent` with `alpha` or `beta`
   - Use a brief but descriptive name

3. **Implement changes**
   - Write tests first
   - Implement features in C#
   - Ensure all C# classes that derive from Godot classes use the `partial` keyword
   - Run tests locally

4. **Commit changes**
   ```bash
   git add .
   git commit -m "Descriptive commit message"
   git push -u origin feature/agent/description
   ```

5. **Create a PR**
   ```bash
   gh pr create --title "Feature title" --body "Description" --base develop
   ```

6. **Review and merge**
   - Wait for review from the other agent
   - Address feedback
   - Merge to develop when approved

## Agent Responsibilities

### Agent Alpha (Senior Developer)
- Implement new features
- Write unit and integration tests
- Fix bugs and errors
- Maintain code quality
- Ensure type safety

### Agent Beta (QA Developer)
- Write and maintain end-to-end tests
- Clean up code and improve organization
- Ensure consistent code style
- Review PRs for quality
- Report issues and suggest improvements

## C# Development Guidelines

1. **Class Structure**
   ```csharp
   using Godot;
   using System;

   namespace SignalLost
   {
       public partial class MyComponent : Node
       {
           // Properties with proper C# naming
           [Export]
           public float MyProperty { get; set; } = 0.0f;

           // Private fields with underscore prefix
           private float _myField = 0.0f;

           // Godot lifecycle methods
           public override void _Ready()
           {
               // Initialization
           }

           public override void _Process(double delta)
           {
               // Per-frame logic
           }

           // Public methods
           public void DoSomething()
           {
               // Implementation
           }

           // Private methods
           private void HandleSomething()
           {
               // Implementation
           }
       }
   }
   ```

2. **Signals**
   ```csharp
   // Define signal
   [Signal]
   public delegate void MySignalEventHandler(string parameter);

   // Emit signal
   EmitSignal(SignalName.MySignal, "parameter");

   // Connect to signal
   otherNode.MySignal += OnMySignal;

   // Signal handler
   private void OnMySignal(string parameter)
   {
       // Handle signal
   }
   ```

3. **Testing**
   ```csharp
   using Godot;
   using System;
   using Microsoft.VisualStudio.TestTools.UnitTesting;

   namespace SignalLost.Tests
   {
       [TestClass]
       public partial class MyComponentTests : Test
       {
           private MyComponent _component;

           [TestInitialize]
           public void Setup()
           {
               _component = new MyComponent();
               AddChild(_component);
           }

           [TestCleanup]
           public void Teardown()
           {
               _component.QueueFree();
               _component = null;
           }

           [TestMethod]
           public void TestSomething()
           {
               // Arrange
               _component.MyProperty = 1.0f;

               // Act
               _component.DoSomething();

               // Assert
               Assert.AreEqual(2.0f, _component.MyProperty);
           }
       }
   }
   ```

## Running Tests

1. **Run all tests**
   ```bash
   cd godot_project
   ./run_csharp_tests.bat  # Windows
   ./run_csharp_tests.sh   # Linux/Mac
   ```

2. **Run specific tests**
   ```bash
   cd godot_project
   ./run_custom_tests.bat "MyComponentTests"
   ```

## Debugging

1. **Console output**
   ```csharp
   GD.Print("Debug message");
   GD.PrintErr("Error message");
   ```

2. **Logging to file**
   ```bash
   cd godot_project
   ./run_with_logs.bat
   ```
   - Logs are saved to `godot_project/logs/`

3. **Error reporting**
   ```bash
   cd godot_project
   ./run_with_error_report.bat
   ```
   - Error reports are saved to `godot_project/logs/`

## Running the Game

1. **Running from the command line (preferred method)**
   ```bash
   cd godot_project
   "C:\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64\Godot_v4.4.1-stable_mono_win64_console.exe" --path . --verbose --run MainScene.tscn
   ```
   - This runs the game directly with console output visible
   - Allows for debugging and seeing error messages in real-time
   - Preferred over using the Godot editor for regular development

2. **Development workflow**
   - Edit code in VS Code
   - Run the game using the command line method above
   - View debug output in the console
   - Only use the Godot editor when necessary for scene editing

3. **Debugging with console output**
   - Add `GD.Print()` statements to your code
   - All output will be visible in the console when running with `--verbose`
   - Use `GD.PrintErr()` for error messages

## Common Issues

1. **Missing partial keyword**
   - All C# classes that derive from Godot classes must include the `partial` keyword
   - Error: `GD0001: Missing partial modifier on declaration of type 'SignalLost.MyClass' that derives from 'Godot.GodotObject'`

2. **Signal connection issues**
   - Make sure signal names match exactly
   - Use `SignalName.MySignal` for emitting signals

3. **Scene loading issues**
   - Use absolute paths: `GD.Load<PackedScene>("res://scenes/my_scene.tscn")`
   - Check for case sensitivity in paths

4. **Game crashes when running**
   - Check the console output for error messages
   - Look for exceptions in the C# code
   - Verify that all resources are properly loaded

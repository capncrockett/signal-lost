# Testing Issues in Signal Lost

This document outlines the issues encountered when trying to run tests for the Signal Lost project.

## Issues Identified

1. **C# Script Instantiation**: The C# scripts (`GameState.cs` and `AudioManager.cs`) are being loaded, but they can't be instantiated with the `new()` function from GDScript. This is causing errors like:
   ```
   SCRIPT ERROR: Invalid call. Nonexistent function 'new' in base 'CSharpScript'.
   ```

2. **Autoload Configuration**: The project is trying to use C# scripts as autoloads, but Godot is having trouble with this:
   ```
   ERROR: Failed to instantiate an autoload, script 'res://scripts/GameState.cs' does not inherit from 'Node'.
   ```

3. **GUT Integration**: The GUT (Godot Unit Testing) framework is installed, but it doesn't seem to be properly integrated with C# scripts.

4. **Missing Icon**: There's an error about a missing icon file:
   ```
   ERROR: Error opening file 'res://assets/images/icon.png'.
   ```

## Recommendations

### 1. Fix C# Script Instantiation

The issue with C# script instantiation might be related to how C# scripts are compiled and loaded in Godot. Make sure:

- The project has been built with the C# compiler
- The C# scripts have the correct namespace and class names
- The C# scripts inherit from `Node` or another Godot class

Try running the following command to build the C# project:

```bash
dotnet build
```

### 2. Fix Autoload Configuration

Instead of using C# scripts directly as autoloads, use GDScript wrappers (as we've already implemented). Make sure the wrappers can properly instantiate the C# classes.

### 3. Improve GUT Integration

For testing C# scripts, consider:

- Using the built-in C# testing framework (NUnit) instead of GUT
- Creating a custom test runner that can work with both GDScript and C#
- Using a hybrid approach where GDScript tests call into C# code

### 4. Fix Missing Icon

Add the missing icon file at `res://assets/images/icon.png` or update the project.godot file to point to an existing icon.

## Next Steps

1. **Build the C# Project**: Make sure the C# project is properly built
2. **Update C# Scripts**: Ensure they follow Godot's C# conventions
3. **Create C# Tests**: Use NUnit for testing C# code
4. **Improve GDScript Wrappers**: Make them more robust for testing

## Resources

- [Godot C# Documentation](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/index.html)
- [C# Testing in Godot](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_differences.html)
- [GUT Documentation](https://github.com/bitwes/Gut/wiki)

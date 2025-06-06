# Cross-Platform Compatibility Guide

## Overview

Signal Lost is designed to work on both Windows and macOS platforms. This document outlines the key considerations and best practices for maintaining cross-platform compatibility. With the recent radio interface implementation, cross-platform testing has become even more critical.

## Platform-Specific Considerations

### File Paths

- **Case Sensitivity**: macOS is case-sensitive while Windows is not
- **Path Separators**: Use `Path.Combine()` instead of hardcoded separators
- **Resource Paths**: Use Godot's `res://` prefix for resource paths

### File System Access

- **Screenshots**: Use `OS.GetUserDataDir()` (Godot's `user://` directory) for saving screenshots to ensure cross-platform compatibility
- **User Data**: Use `OS.GetUserDataDir()` for user-specific data
- **External Files**: Use `System.Environment.GetFolderPath()` for platform-specific folders when necessary

## C# Type Handling

- **Float vs Double**: Be explicit with type casting between float and double
- **Type Conversion**: Use proper casting when converting between numeric types
- **Godot Types**: Be aware of Godot's type handling differences on different platforms

## Testing

- **Platform-Specific Tests**: Use conditional compilation for platform-specific tests
- **Test Scene**: Use `CSharpTestScene.tscn` for all platforms
- **Skipping Tests**: Use platform detection to skip tests that aren't applicable
- **Test Runner**: The test runner has been updated to work on both Windows and macOS

```csharp
// Example of platform-specific test code
[TestMethod]
public void TestPlatformSpecificFeature()
{
    if (OS.GetName() == "macOS")
    {
        GD.Print("Skipping test on Mac");
        Assert.IsTrue(true, "Test skipped on Mac platform");
        return;
    }

    // Windows-specific test code here
    // Run Windows-specific assertions
    Assert.AreEqual(expected, actual, "Windows-specific test failed");
}
```

## Best Practices

1. **Use C# Features**: Leverage C#'s cross-platform capabilities
2. **Avoid Platform-Specific APIs**: Use Godot's abstraction when possible
3. **Test on Both Platforms**: Regularly test on both Windows and macOS
4. **Use Conditional Compilation**: Use `#if` directives for platform-specific code
5. **Handle Path Differences**: Be aware of path differences between platforms
6. **Document Platform Requirements**: Note any platform-specific requirements

## Common Issues and Solutions

### Case Sensitivity

**Issue**: Files found on Windows but not on macOS due to case differences.

**Solution**:

- Maintain consistent casing in all file references
- Use lowercase for all file and directory names
- Double-check resource paths in scene files

### Type Conversion

**Issue**: Type conversion that works on one platform may fail on another.

**Solution**:

- Always use explicit casting for numeric conversions
- Be careful with float/double conversions
- Use `(float)` or `(double)` casts when needed

### File Paths

**Issue**: Hardcoded paths that work on one platform fail on another.

**Solution**:

- Use `Path.Combine()` for file paths
- Use Godot's `res://` and `user://` prefixes
- Use `System.Environment.GetFolderPath()` for system directories

## Platform-Specific Code

When platform-specific code is necessary, use OS detection or conditional compilation:

```csharp
using Godot;
using System;
using System.IO;

public partial class PlatformSpecific : Node
{
    public string GetSavePath(string filename)
    {
        string basePath;

        if (OS.GetName() == "macOS")
        {
            // Mac-specific path
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            basePath = Path.Combine(homeDir, "Documents", "SignalLost");
        }
        else if (OS.GetName() == "Windows")
        {
            // Windows-specific path
            string docsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            basePath = Path.Combine(docsDir, "SignalLost");
        }
        else
        {
            // Default path for other platforms
            basePath = OS.GetUserDataDir();
        }

        // Create directory if it doesn't exist
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        return Path.Combine(basePath, filename);
    }
}
```

## Screenshot System

The screenshot system uses Godot's `user://` directory for cross-platform compatibility:

```csharp
public class ScreenshotTaker : Node
{
    public string TakeScreenshot(string filename)
    {
        // Get user data directory for cross-platform compatibility
        string directory = Path.Combine(OS.GetUserDataDir(), ScreenshotDirectoryName);
        string fullPath = Path.Combine(directory, filename);

        // Ensure directory exists
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Take screenshot
        var image = GetViewport().GetTexture().GetImage();
        image.SavePng(fullPath);

        GD.Print($"Screenshot saved to: {fullPath}");
        return fullPath;
    }
```

### AI Screenshot Tool

For AI agent development, we've created a specialized screenshot tool that loads the main game scene and takes a screenshot. This tool is essential for AI agents to visually verify their work.

See [AI Screenshot Tool](ai-screenshot-tool.md) for more details.

```csharp
private string GetScreenshotDirectory()
    {
        string basePath;

        if (OS.GetName() == "macOS")
        {
            // Mac-specific path
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            basePath = Path.Combine(homeDir, "Documents", "SignalLost", "Screenshots");
        }
        else if (OS.GetName() == "Windows")
        {
            // Windows-specific path
            string picturesDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            basePath = Path.Combine(picturesDir, "SignalLost", "Screenshots");
        }
        else
        {
            // Default path for other platforms
            basePath = Path.Combine(OS.GetUserDataDir(), "Screenshots");
        }

        return basePath;
    }

}

```

## Radio Interface Cross-Platform Considerations

### Audio System

The radio interface's audio system has platform-specific considerations:

- **Audio Initialization**: Audio initialization may differ between platforms
- **Audio Latency**: Mac may have different audio latency compared to Windows
- **Resource Cleanup**: Ensure proper cleanup of audio resources on both platforms

### Run Scripts

We've created platform-specific run scripts for testing the radio interface:

- **Windows**: Use `.bat` files for running different radio scenes
- **macOS**: Use `.sh` files for running different radio scenes

```bash
# Example macOS run script for radio demo
#!/bin/bash
cd godot_project
/Applications/Godot_mono.app/Contents/MacOS/Godot --path . --scene Scenes/Radio/RadioSignalsDemo.tscn
```

## Memory Management

Memory management is critical for cross-platform compatibility:

- **Dispose Pattern**: Implement IDisposable for classes that manage unmanaged resources
- **Resource Cleanup**: Ensure resources are properly cleaned up on both platforms
- **Memory Leaks**: Test for memory leaks on both platforms

## Conclusion

By following these guidelines, you can ensure that Signal Lost works consistently across both Windows and macOS platforms. Regular testing on both platforms is essential to catch and fix any platform-specific issues early in development.

With the radio interface implementation, cross-platform testing has become even more important. Use the provided run scripts to test different radio scenes on both platforms, and pay special attention to audio system behavior and memory management.

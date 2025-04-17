# Cross-Platform Compatibility Guide

## Overview

Signal Lost is designed to work on both Windows and macOS platforms. This document outlines the key considerations and best practices for maintaining cross-platform compatibility.

## Platform-Specific Considerations

### File Paths

- **Case Sensitivity**: macOS is case-sensitive while Windows is not
- **Path Separators**: Use `Path.Combine()` instead of hardcoded separators
- **Resource Paths**: Use Godot's `res://` prefix for resource paths

### File System Access

- **Screenshots**: Use platform-specific paths for saving screenshots
- **User Data**: Use `OS.GetUserDataDir()` for user-specific data
- **External Files**: Use `System.Environment.GetFolderPath()` for platform-specific folders

## C# Type Handling

- **Float vs Double**: Be explicit with type casting between float and double
- **Type Conversion**: Use proper casting when converting between numeric types
- **Godot Types**: Be aware of Godot's type handling differences on different platforms

## Testing

- **Platform-Specific Tests**: Use conditional compilation for platform-specific tests
- **Test Scene**: Use `MacTestScene.tscn` for Mac-specific tests
- **Skipping Tests**: Use platform detection to skip tests that aren't applicable

```csharp
// Example of platform-specific test code
public void TestPlatformSpecificFeature()
{
    if (OS.GetName() == "macOS")
    {
        GD.Print("Skipping test on Mac");
        Pass("Test skipped on Mac platform");
        return;
    }
    
    // Windows-specific test code here
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

When platform-specific code is necessary, use conditional compilation:

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

## Conclusion

By following these guidelines, you can ensure that Signal Lost works consistently across both Windows and macOS platforms. Regular testing on both platforms is essential to catch and fix any platform-specific issues early in development.

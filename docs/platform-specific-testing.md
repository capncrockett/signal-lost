# Platform-Specific Testing Guide

## Overview

Signal Lost is designed to work on both Windows and macOS platforms. This document provides guidelines for testing the game on different platforms to ensure cross-platform compatibility.

## Test Environment Setup

### Windows

- **Operating System**: Windows 10/11
- **Godot Version**: Godot 4.x with Mono support
- **Required Tools**:
  - Visual Studio 2022 or later
  - .NET 6.0 SDK or later
  - Git for Windows

### macOS

- **Operating System**: macOS 11 (Big Sur) or later
- **Godot Version**: Godot 4.x with Mono support
- **Required Tools**:
  - Visual Studio for Mac or Visual Studio Code
  - .NET 6.0 SDK or later
  - Git for macOS

## Running Tests

### Windows

```bash
# From the project root
cd godot_project
.\run_csharp_tests.bat
```

### macOS

```bash
# From the project root
cd godot_project
chmod +x run_csharp_tests.sh
./run_csharp_tests.sh
```

## Test Runner Implementation

The test runner has been updated to work on both platforms:

```csharp
// Platform-specific test initialization
public void InitializeTests()
{
    // Common initialization
    _testClasses = FindTestClasses();
    
    if (OS.GetName() == "macOS")
    {
        // Mac-specific initialization
        GD.Print("Initializing tests for macOS...");
        // Use CSharpTestScene.tscn
    }
    else
    {
        // Windows-specific initialization
        GD.Print("Initializing tests for Windows...");
        // Use CSharpTestScene.tscn
    }
}
```

## Platform-Specific Test Cases

### File System Tests

```csharp
[TestMethod]
public void TestFileSystemAccess()
{
    string testFilePath = GetPlatformSpecificTestPath();
    
    // Create test file
    File.WriteAllText(testFilePath, "Test content");
    
    // Verify file exists
    Assert.IsTrue(File.Exists(testFilePath), "Test file should exist");
    
    // Clean up
    File.Delete(testFilePath);
}

private string GetPlatformSpecificTestPath()
{
    if (OS.GetName() == "macOS")
    {
        string tempDir = Path.GetTempPath();
        return Path.Combine(tempDir, "signal_lost_test.txt");
    }
    else
    {
        // Windows path
        return Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData),
            "SignalLost", "test.txt");
    }
}
```

### Screenshot Tests

```csharp
[TestMethod]
public void TestScreenshotCapture()
{
    // Create screenshot taker
    var screenshotTaker = new ScreenshotTaker();
    
    // Take test screenshot
    string filename = $"test_screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
    string path = screenshotTaker.TakeScreenshot(filename);
    
    // Verify screenshot was created
    Assert.IsTrue(File.Exists(path), "Screenshot should exist");
    
    // Clean up
    try
    {
        File.Delete(path);
    }
    catch (Exception ex)
    {
        GD.PrintErr($"Failed to delete test screenshot: {ex.Message}");
    }
}
```

## Common Platform Issues

### Path Separators

Windows uses backslashes (`\`) while macOS uses forward slashes (`/`) for file paths. Always use `Path.Combine()` to handle this automatically:

```csharp
// Correct - works on both platforms
string filePath = Path.Combine("directory", "subdirectory", "file.txt");

// Incorrect - will fail on one platform
string filePath = "directory\\subdirectory\\file.txt"; // Windows-specific
string filePath = "directory/subdirectory/file.txt";   // Unix-specific
```

### Case Sensitivity

macOS file systems are case-sensitive by default, while Windows is not:

```csharp
// This works on Windows but may fail on macOS if the actual filename is "Image.png"
string path = Path.Combine(directory, "image.png");

// Solution: Always use the exact case in filenames
string path = Path.Combine(directory, "Image.png");
```

### Special Folders

Use `Environment.GetFolderPath()` to get platform-specific special folders:

```csharp
// Documents folder
string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

// Pictures folder
string picsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
```

## Testing Checklist

### File System

- [ ] Test file creation and deletion
- [ ] Test directory creation and deletion
- [ ] Test file paths with spaces and special characters
- [ ] Test reading and writing to user data directory
- [ ] Test screenshot functionality

### UI

- [ ] Test UI scaling on different resolutions
- [ ] Test text rendering and font loading
- [ ] Test input handling (keyboard, mouse)
- [ ] Test window management (fullscreen, windowed)

### Performance

- [ ] Test frame rate on minimum spec hardware
- [ ] Test memory usage during extended play
- [ ] Test loading times for scenes and resources

### Audio

- [ ] Test audio playback and volume control
- [ ] Test audio device selection
- [ ] Test audio quality settings

## Automated Platform Testing

### CI/CD Integration

Set up GitHub Actions to run tests on both platforms:

```yaml
# .github/workflows/cross-platform-tests.yml
name: Cross-Platform Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test-windows:
    runs-on: windows-latest
    steps:
      # Setup steps...
      - name: Run Windows Tests
        run: .\godot_project\run_csharp_tests.bat

  test-macos:
    runs-on: macos-latest
    steps:
      # Setup steps...
      - name: Run macOS Tests
        run: chmod +x ./godot_project/run_csharp_tests.sh && ./godot_project/run_csharp_tests.sh
```

## Conclusion

By following these guidelines and regularly testing on both Windows and macOS, you can ensure that Signal Lost provides a consistent experience across platforms. The updated test runner and platform-specific code help identify and resolve compatibility issues early in development.

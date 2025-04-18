# Memory Leak Detection System

## Overview

The Memory Leak Detection System is a comprehensive set of tools designed to identify, track, and fix memory leaks in the Signal Lost game. It provides real-time monitoring of memory usage, resource tracking, and automated cleanup mechanisms.

## Components

### 1. MemoryProfiler

The core component that tracks object creation and disposal, monitors memory usage over time, and detects potential memory leaks.

```csharp
// Access the memory profiler
var memoryProfiler = GetNode<MemoryProfiler>("/root/MemoryProfiler");

// Track an object
memoryProfiler.TrackObject(myObject, "MyObjectTag");

// Untrack an object
memoryProfiler.UntrackObject(myObject, "MyObjectTag");

// Force garbage collection
memoryProfiler.ForceGarbageCollection();

// Take a memory snapshot
memoryProfiler.TakeMemorySnapshot();

// Log memory usage
memoryProfiler.LogMemoryUsage();
```

### 2. ResourceTracker

Specifically tracks Godot resources (textures, audio streams, etc.) to detect resource leaks.

```csharp
// Access the resource tracker
var resourceTracker = GetNode<ResourceTracker>("/root/ResourceTracker");

// Track a resource
resourceTracker.TrackResource(myTexture, "MyTexturePath");

// Untrack a resource
resourceTracker.UntrackResource(myTexture);

// Load and track a resource
var texture = resourceTracker.TrackLoadedResource<Texture2D>("res://path/to/texture.png");

// Log resource usage
resourceTracker.LogResourceUsage();

// Force resource cleanup
resourceTracker.ForceResourceCleanup();
```

### 3. DisposableResourceManager

Manages disposable resources to ensure proper cleanup.

```csharp
// Access the disposable resource manager
var resourceManager = DisposableResourceManager.Instance;

// Register a disposable resource
resourceManager.RegisterDisposable(myDisposable, ownerNode);

// Unregister a disposable resource
resourceManager.UnregisterDisposable(myDisposable, true); // true to dispose

// Dispose resources for a specific owner
resourceManager.DisposeResourcesForOwner(ownerNode);

// Dispose all resources
resourceManager.DisposeAllResources();

// Create and register a managed resource
var stream = resourceManager.CreateManagedResource(() => new FileStream("path", FileMode.Open), ownerNode);
```

### 4. MemoryLeakDetectorUI

A visual interface for monitoring memory usage and detecting leaks.

- Press F8 to toggle the memory leak detector UI
- Shows real-time memory usage and object counts
- Displays warnings and detected memory leaks
- Provides tools for forcing garbage collection and generating reports

### 5. ScreenshotTaker (Enhanced)

The ScreenshotTaker has been enhanced to properly clean up resources after taking screenshots.

```csharp
// Access the screenshot taker
var screenshotTaker = GetNode<ScreenshotTaker>("/root/ScreenshotTaker");

// Take a screenshot
screenshotTaker.TakeScreenshot("my_screenshot");

// Configure cleanup
screenshotTaker.CleanupAfterScreenshot = true;
```

## Common Memory Leak Scenarios

### 1. Unmanaged Resources

```csharp
// WRONG: Resource not properly disposed
var stream = new FileStream("path", FileMode.Open);
// ... use stream ...
// No disposal!

// CORRECT: Using DisposableResourceManager
var resourceManager = DisposableResourceManager.Instance;
var stream = resourceManager.CreateManagedResource(() => new FileStream("path", FileMode.Open), this);
// ... use stream ...
// Automatically disposed when owner is freed
```

### 2. Static References

```csharp
// WRONG: Static reference prevents garbage collection
private static List<object> _leakedObjects = new List<object>();
public void CreateObject()
{
    var obj = new MyObject();
    _leakedObjects.Add(obj); // Will never be garbage collected
}

// CORRECT: Use weak references for caching
private static List<WeakReference> _cachedObjects = new List<WeakReference>();
public void CreateObject()
{
    var obj = new MyObject();
    _cachedObjects.Add(new WeakReference(obj));
}
```

### 3. Event Handlers

```csharp
// WRONG: Event handler not unsubscribed
public override void _Ready()
{
    GetTree().ProcessFrame += OnProcessFrame;
}
// No unsubscribe in _ExitTree!

// CORRECT: Properly unsubscribe
public override void _Ready()
{
    GetTree().ProcessFrame += OnProcessFrame;
}

public override void _ExitTree()
{
    GetTree().ProcessFrame -= OnProcessFrame;
}
```

### 4. Circular References

```csharp
// WRONG: Circular reference
class Parent
{
    public Child Child { get; set; }
}

class Child
{
    public Parent Parent { get; set; }
}

// CORRECT: Use weak references
class Parent
{
    public Child Child { get; set; }
}

class Child
{
    private WeakReference<Parent> _parentRef;
    
    public void SetParent(Parent parent)
    {
        _parentRef = new WeakReference<Parent>(parent);
    }
    
    public Parent GetParent()
    {
        if (_parentRef.TryGetTarget(out Parent parent))
            return parent;
        return null;
    }
}
```

## Best Practices

1. **Track Resource Creation**: Always track resources that might cause leaks
2. **Implement IDisposable**: Use the IDisposable pattern for resource cleanup
3. **Avoid Static References**: Don't store objects in static collections
4. **Unsubscribe from Events**: Always unsubscribe from events in _ExitTree
5. **Use Weak References**: For caches and non-ownership references
6. **Monitor Memory Usage**: Regularly check the memory leak detector UI
7. **Run Garbage Collection**: Force GC when transitioning between scenes
8. **Clean Up After Screenshots**: Ensure screenshot resources are properly disposed

## Troubleshooting

### High Memory Usage

If you notice high memory usage:

1. Open the Memory Leak Detector UI (press F8)
2. Check for warnings and leak reports
3. Force garbage collection to see if memory is reclaimed
4. Generate a memory report to identify problematic objects
5. Use the object tracking features to find the source of leaks

### Resource Leaks

If you suspect resource leaks:

1. Use ResourceTracker to monitor resource creation and disposal
2. Check for duplicate resources loaded from the same path
3. Ensure resources are properly unloaded when no longer needed
4. Use DisposableResourceManager for automatic resource cleanup

### Platform-Specific Issues

- **macOS**: Watch for Preview process leaks when taking screenshots
- **Windows**: Monitor handle counts for file and network operations

## Conclusion

The Memory Leak Detection System provides comprehensive tools for identifying and fixing memory leaks in the Signal Lost game. By following the best practices and using the provided components, you can ensure that the game runs efficiently and without memory-related issues on both Windows and macOS platforms.

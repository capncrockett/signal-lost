using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalLost
{
    /// <summary>
    /// Tracks Godot resources to detect potential memory leaks.
    /// Works in conjunction with MemoryProfiler.
    /// </summary>
    [GlobalClass]
    public partial class ResourceTracker : Node
    {
        // Signals
        [Signal]
        public delegate void ResourceLeakDetectedEventHandler(string resourceType, string resourcePath, int instanceCount);

        // Configuration
        [Export]
        public bool EnableTracking { get; set; } = true;

        [Export]
        public bool LogToConsole { get; set; } = true;

        [Export]
        public int ResourceWarningThreshold { get; set; } = 5; // Number of duplicate resources to trigger warning

        // Internal state
        private Dictionary<string, List<WeakReference>> _trackedResources = new Dictionary<string, List<WeakReference>>();
        private Dictionary<string, string> _resourcePaths = new Dictionary<string, string>();
        private Dictionary<string, int> _resourceCounts = new Dictionary<string, int>();
        private MemoryProfiler _memoryProfiler;
        private bool _isInitialized = false;

        // Resource types to track
        private readonly string[] _resourceTypesToTrack = new string[]
        {
            "AudioStream",
            "AudioStreamGenerator",
            "Texture2D",
            "Image",
            "PackedScene",
            "Material",
            "Mesh",
            "Font",
            "Animation"
        };

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("ResourceTracker: Initializing...");
            
            // Get reference to memory profiler
            _memoryProfiler = GetNode<MemoryProfiler>("/root/MemoryProfiler");
            
            if (_memoryProfiler == null)
            {
                GD.PrintErr("ResourceTracker: MemoryProfiler not found. Make sure it's added as an autoload singleton.");
            }
            
            // Set up process priority
            ProcessPriority = 90; // Run after memory profiler
            
            _isInitialized = true;
            GD.Print("ResourceTracker: Ready");
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            if (!EnableTracking || !_isInitialized)
                return;

            // Periodically check for leaked resources
            CheckForLeakedResources();
        }

        /// <summary>
        /// Tracks a resource for potential memory leaks.
        /// </summary>
        /// <param name="resource">The resource to track</param>
        /// <param name="sourcePath">Optional source path or identifier</param>
        public void TrackResource(Resource resource, string sourcePath = null)
        {
            if (!EnableTracking || resource == null)
                return;

            string resourceType = resource.GetType().Name;
            string resourceId = resource.GetInstanceId().ToString();
            
            // Only track specific resource types
            if (!_resourceTypesToTrack.Contains(resourceType) && 
                !_resourceTypesToTrack.Any(t => resourceType.Contains(t)))
            {
                return;
            }
            
            // Create weak reference to avoid preventing garbage collection
            WeakReference weakRef = new WeakReference(resource);
            
            // Store resource path
            if (string.IsNullOrEmpty(sourcePath) && resource.ResourcePath != null)
            {
                sourcePath = resource.ResourcePath;
            }
            
            if (!string.IsNullOrEmpty(sourcePath))
            {
                _resourcePaths[resourceId] = sourcePath;
            }
            
            // Add to tracked resources
            if (!_trackedResources.ContainsKey(resourceType))
            {
                _trackedResources[resourceType] = new List<WeakReference>();
            }
            
            _trackedResources[resourceType].Add(weakRef);
            
            // Also track with memory profiler if available
            if (_memoryProfiler != null)
            {
                _memoryProfiler.TrackObject(resource, "Resource:" + resourceType);
            }
            
            // Update resource counts
            if (!_resourceCounts.ContainsKey(resourceType))
            {
                _resourceCounts[resourceType] = 0;
            }
            _resourceCounts[resourceType]++;
            
            if (LogToConsole && _resourceCounts[resourceType] % 10 == 0)
            {
                GD.Print($"ResourceTracker: Now tracking {_resourceCounts[resourceType]} {resourceType} resources");
            }
        }

        /// <summary>
        /// Unregisters a resource from tracking.
        /// </summary>
        /// <param name="resource">The resource to stop tracking</param>
        public void UntrackResource(Resource resource)
        {
            if (!EnableTracking || resource == null)
                return;

            string resourceType = resource.GetType().Name;
            string resourceId = resource.GetInstanceId().ToString();
            
            if (_trackedResources.ContainsKey(resourceType))
            {
                // Find and remove the weak reference
                for (int i = _trackedResources[resourceType].Count - 1; i >= 0; i--)
                {
                    var weakRef = _trackedResources[resourceType][i];
                    if (!weakRef.IsAlive || weakRef.Target == resource)
                    {
                        _trackedResources[resourceType].RemoveAt(i);
                        
                        // Update resource count
                        if (_resourceCounts.ContainsKey(resourceType))
                        {
                            _resourceCounts[resourceType] = Math.Max(0, _resourceCounts[resourceType] - 1);
                        }
                        
                        break;
                    }
                }
            }
            
            // Remove resource path
            if (_resourcePaths.ContainsKey(resourceId))
            {
                _resourcePaths.Remove(resourceId);
            }
            
            // Also untrack with memory profiler if available
            if (_memoryProfiler != null)
            {
                _memoryProfiler.UntrackObject(resource, "Resource:" + resourceType);
            }
        }

        /// <summary>
        /// Tracks a loaded resource automatically.
        /// </summary>
        /// <typeparam name="T">The resource type</typeparam>
        /// <param name="path">The resource path</param>
        /// <returns>The loaded resource</returns>
        public T TrackLoadedResource<T>(string path) where T : Resource
        {
            T resource = ResourceLoader.Load<T>(path);
            
            if (resource != null)
            {
                TrackResource(resource, path);
            }
            
            return resource;
        }

        /// <summary>
        /// Logs information about tracked resources.
        /// </summary>
        public void LogResourceUsage()
        {
            if (!EnableTracking)
                return;

            GD.Print("=== RESOURCE USAGE REPORT ===");
            
            // Sort resource counts by count (descending)
            var sortedCounts = _resourceCounts.OrderByDescending(pair => pair.Value).ToList();
            
            foreach (var pair in sortedCounts)
            {
                GD.Print($"  {pair.Key}: {pair.Value} instances");
                
                // List paths for this resource type (up to 5)
                if (_trackedResources.ContainsKey(pair.Key))
                {
                    int pathsListed = 0;
                    foreach (var weakRef in _trackedResources[pair.Key])
                    {
                        if (weakRef.IsAlive && weakRef.Target is Resource resource)
                        {
                            string resourceId = resource.GetInstanceId().ToString();
                            string path = _resourcePaths.ContainsKey(resourceId) ? 
                                         _resourcePaths[resourceId] : 
                                         resource.ResourcePath;
                                         
                            if (!string.IsNullOrEmpty(path))
                            {
                                GD.Print($"    - {path}");
                                pathsListed++;
                                
                                if (pathsListed >= 5)
                                {
                                    GD.Print($"    ... and {_trackedResources[pair.Key].Count - 5} more");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            
            GD.Print("============================");
        }

        /// <summary>
        /// Checks for potential resource leaks.
        /// </summary>
        private void CheckForLeakedResources()
        {
            // Group resources by path to detect duplicates
            Dictionary<string, List<WeakReference>> resourcesByPath = new Dictionary<string, List<WeakReference>>();
            
            foreach (var entry in _trackedResources)
            {
                string resourceType = entry.Key;
                var weakRefs = entry.Value;
                
                // Clean up dead references
                for (int i = weakRefs.Count - 1; i >= 0; i--)
                {
                    if (!weakRefs[i].IsAlive)
                    {
                        weakRefs.RemoveAt(i);
                        
                        // Update resource count
                        if (_resourceCounts.ContainsKey(resourceType))
                        {
                            _resourceCounts[resourceType] = Math.Max(0, _resourceCounts[resourceType] - 1);
                        }
                    }
                    else if (weakRefs[i].Target is Resource resource)
                    {
                        string resourceId = resource.GetInstanceId().ToString();
                        string path = _resourcePaths.ContainsKey(resourceId) ? 
                                     _resourcePaths[resourceId] : 
                                     resource.ResourcePath;
                                     
                        if (!string.IsNullOrEmpty(path))
                        {
                            if (!resourcesByPath.ContainsKey(path))
                            {
                                resourcesByPath[path] = new List<WeakReference>();
                            }
                            
                            resourcesByPath[path].Add(weakRefs[i]);
                        }
                    }
                }
            }
            
            // Check for duplicate resources (same path loaded multiple times)
            foreach (var entry in resourcesByPath)
            {
                string path = entry.Key;
                var resources = entry.Value;
                
                if (resources.Count > ResourceWarningThreshold)
                {
                    // Get the resource type
                    string resourceType = "Unknown";
                    if (resources[0].IsAlive && resources[0].Target is Resource resource)
                    {
                        resourceType = resource.GetType().Name;
                    }
                    
                    // Emit warning signal
                    EmitSignal(SignalName.ResourceLeakDetected, resourceType, path, resources.Count);
                    
                    if (LogToConsole)
                    {
                        GD.PrintWarning($"ResourceTracker: Potential resource leak detected! " +
                                       $"{resources.Count} instances of {resourceType} loaded from {path}");
                    }
                }
            }
        }

        /// <summary>
        /// Forces cleanup of tracked resources.
        /// </summary>
        public void ForceResourceCleanup()
        {
            if (!EnableTracking)
                return;

            if (LogToConsole)
            {
                GD.Print("ResourceTracker: Forcing resource cleanup...");
            }
            
            // Force garbage collection first
            GC.Collect();
            GC.WaitForPendingFinalizers();
            
            // Clean up tracked resources
            int totalRemoved = 0;
            
            foreach (var entry in _trackedResources)
            {
                string resourceType = entry.Key;
                var weakRefs = entry.Value;
                
                int initialCount = weakRefs.Count;
                
                // Remove dead references
                for (int i = weakRefs.Count - 1; i >= 0; i--)
                {
                    if (!weakRefs[i].IsAlive)
                    {
                        weakRefs.RemoveAt(i);
                    }
                }
                
                int removedCount = initialCount - weakRefs.Count;
                totalRemoved += removedCount;
                
                // Update resource count
                _resourceCounts[resourceType] = weakRefs.Count;
            }
            
            if (LogToConsole)
            {
                GD.Print($"ResourceTracker: Cleanup complete. Removed {totalRemoved} dead resource references.");
            }
        }
    }
}

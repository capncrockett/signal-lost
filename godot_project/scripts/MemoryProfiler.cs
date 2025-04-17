using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SignalLost
{
    /// <summary>
    /// Memory profiler for detecting memory leaks and monitoring resource usage.
    /// This class is designed to be used as an autoload singleton.
    /// </summary>
    [GlobalClass]
    public partial class MemoryProfiler : Node
    {
        // Signals
        [Signal]
        public delegate void MemoryWarningEventHandler(string message, string objectType, int count);

        [Signal]
        public delegate void MemoryLeakDetectedEventHandler(string objectType, int count, string stackTrace);

        [Signal]
        public delegate void MemorySnapshotTakenEventHandler(Dictionary<string, int> snapshot);

        // Configuration
        [Export]
        public bool EnableProfiling { get; set; } = true;

        [Export]
        public bool LogToConsole { get; set; } = true;

        [Export]
        public bool AutoTakeSnapshots { get; set; } = true;

        [Export]
        public float SnapshotInterval { get; set; } = 30.0f; // seconds

        [Export]
        public int WarningThreshold { get; set; } = 100; // objects

        [Export]
        public float WarningGrowthRate { get; set; } = 0.2f; // 20% growth

        // Internal state
        private Dictionary<string, int> _objectCounts = new Dictionary<string, int>();
        private Dictionary<string, int> _previousSnapshot = new Dictionary<string, int>();
        private Dictionary<string, List<WeakReference>> _trackedObjects = new Dictionary<string, List<WeakReference>>();
        private Dictionary<string, string> _creationStackTraces = new Dictionary<string, string>();
        private float _snapshotTimer = 0;
        private int _totalTrackedObjects = 0;
        private long _lastMemoryUsage = 0;
        private bool _isInitialized = false;

        // Constants
        private const int MAX_TRACKED_OBJECTS = 10000; // Limit to prevent excessive memory usage by the profiler itself

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("MemoryProfiler: Initializing...");
            
            // Take initial snapshot
            TakeMemorySnapshot();
            
            // Register for process frame notifications
            ProcessPriority = 100; // Low priority to run after other processes
            
            _isInitialized = true;
            GD.Print("MemoryProfiler: Ready");
        }

        // Process function called every frame
        public override void _Process(double delta)
        {
            if (!EnableProfiling || !_isInitialized)
                return;

            // Auto-take snapshots at regular intervals
            if (AutoTakeSnapshots)
            {
                _snapshotTimer += (float)delta;
                if (_snapshotTimer >= SnapshotInterval)
                {
                    TakeMemorySnapshot();
                    _snapshotTimer = 0;
                }
            }

            // Check for memory leaks by analyzing weak references
            CheckForLeakedObjects();
        }

        /// <summary>
        /// Takes a snapshot of the current memory usage and object counts.
        /// </summary>
        public void TakeMemorySnapshot()
        {
            if (!EnableProfiling)
                return;

            // Store previous snapshot for comparison
            _previousSnapshot = new Dictionary<string, int>(_objectCounts);
            
            // Clear current counts
            _objectCounts.Clear();
            
            // Get current memory usage
            long currentMemoryUsage = GC.GetTotalMemory(false);
            
            // Calculate memory change
            long memoryDelta = currentMemoryUsage - _lastMemoryUsage;
            _lastMemoryUsage = currentMemoryUsage;
            
            // Log memory usage
            if (LogToConsole)
            {
                GD.Print($"MemoryProfiler: Memory usage: {FormatBytes(currentMemoryUsage)} " +
                         $"(Delta: {(memoryDelta >= 0 ? "+" : "")}{FormatBytes(memoryDelta)})");
            }
            
            // Count objects by type
            CountObjectsByType();
            
            // Emit signal with snapshot data
            EmitSignal(SignalName.MemorySnapshotTaken, _objectCounts);
            
            // Check for significant changes
            AnalyzeMemoryChanges();
        }

        /// <summary>
        /// Registers an object to be tracked for potential memory leaks.
        /// </summary>
        /// <param name="obj">The object to track</param>
        /// <param name="tag">Optional tag for grouping related objects</param>
        public void TrackObject(object obj, string tag = null)
        {
            if (!EnableProfiling || obj == null)
                return;

            // Prevent tracking too many objects
            if (_totalTrackedObjects >= MAX_TRACKED_OBJECTS)
            {
                if (LogToConsole)
                {
                    GD.PrintWarning($"MemoryProfiler: Maximum tracked objects limit reached ({MAX_TRACKED_OBJECTS})");
                }
                return;
            }

            string objectType = obj.GetType().Name;
            string key = string.IsNullOrEmpty(tag) ? objectType : $"{objectType}:{tag}";
            
            // Create weak reference to avoid preventing garbage collection
            WeakReference weakRef = new WeakReference(obj);
            
            // Store stack trace for debugging
            string stackTrace = Environment.StackTrace;
            string objectId = RuntimeHelpers.GetHashCode(obj).ToString();
            _creationStackTraces[objectId] = stackTrace;
            
            // Add to tracked objects
            if (!_trackedObjects.ContainsKey(key))
            {
                _trackedObjects[key] = new List<WeakReference>();
            }
            
            _trackedObjects[key].Add(weakRef);
            _totalTrackedObjects++;
        }

        /// <summary>
        /// Unregisters an object from tracking.
        /// </summary>
        /// <param name="obj">The object to stop tracking</param>
        /// <param name="tag">Optional tag that was used when tracking</param>
        public void UntrackObject(object obj, string tag = null)
        {
            if (!EnableProfiling || obj == null)
                return;

            string objectType = obj.GetType().Name;
            string key = string.IsNullOrEmpty(tag) ? objectType : $"{objectType}:{tag}";
            
            if (_trackedObjects.ContainsKey(key))
            {
                // Find and remove the weak reference
                for (int i = _trackedObjects[key].Count - 1; i >= 0; i--)
                {
                    var weakRef = _trackedObjects[key][i];
                    if (weakRef.Target == obj || !weakRef.IsAlive)
                    {
                        _trackedObjects[key].RemoveAt(i);
                        _totalTrackedObjects--;
                        break;
                    }
                }
                
                // Remove the key if no more objects of this type
                if (_trackedObjects[key].Count == 0)
                {
                    _trackedObjects.Remove(key);
                }
            }
            
            // Remove stack trace
            string objectId = RuntimeHelpers.GetHashCode(obj).ToString();
            if (_creationStackTraces.ContainsKey(objectId))
            {
                _creationStackTraces.Remove(objectId);
            }
        }

        /// <summary>
        /// Forces a garbage collection and updates memory statistics.
        /// </summary>
        public void ForceGarbageCollection()
        {
            if (LogToConsole)
            {
                GD.Print("MemoryProfiler: Forcing garbage collection...");
            }
            
            // Store pre-GC memory usage
            long preGcMemory = GC.GetTotalMemory(false);
            
            // Force full garbage collection
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            GC.WaitForPendingFinalizers();
            
            // Get post-GC memory usage
            long postGcMemory = GC.GetTotalMemory(true);
            long freedMemory = preGcMemory - postGcMemory;
            
            if (LogToConsole)
            {
                GD.Print($"MemoryProfiler: Garbage collection complete. " +
                         $"Freed: {FormatBytes(freedMemory)}, " +
                         $"Current usage: {FormatBytes(postGcMemory)}");
            }
            
            // Update last memory usage
            _lastMemoryUsage = postGcMemory;
            
            // Take a new snapshot after GC
            TakeMemorySnapshot();
        }

        /// <summary>
        /// Logs the current memory usage and object counts.
        /// </summary>
        public void LogMemoryUsage()
        {
            if (!EnableProfiling)
                return;

            GD.Print("=== MEMORY USAGE REPORT ===");
            GD.Print($"Total Memory: {FormatBytes(GC.GetTotalMemory(false))}");
            GD.Print($"Total Tracked Objects: {_totalTrackedObjects}");
            
            // Sort object counts by count (descending)
            var sortedCounts = _objectCounts.OrderByDescending(pair => pair.Value).ToList();
            
            GD.Print("Object Counts:");
            foreach (var pair in sortedCounts)
            {
                GD.Print($"  {pair.Key}: {pair.Value}");
            }
            
            GD.Print("=========================");
        }

        /// <summary>
        /// Checks for potential memory leaks by analyzing tracked objects.
        /// </summary>
        private void CheckForLeakedObjects()
        {
            // Skip if we're tracking too many objects to avoid performance issues
            if (_totalTrackedObjects >= MAX_TRACKED_OBJECTS)
                return;

            foreach (var entry in _trackedObjects.ToList())
            {
                string objectType = entry.Key;
                var weakRefs = entry.Value;
                
                // Count objects that are still alive
                int aliveCount = 0;
                
                // Clean up dead references
                for (int i = weakRefs.Count - 1; i >= 0; i--)
                {
                    if (!weakRefs[i].IsAlive)
                    {
                        weakRefs.RemoveAt(i);
                        _totalTrackedObjects--;
                    }
                    else
                    {
                        aliveCount++;
                    }
                }
                
                // Check if we have a potential leak (many instances of the same type)
                if (aliveCount > WarningThreshold)
                {
                    // Get a sample stack trace for debugging
                    string sampleStackTrace = "Unknown";
                    foreach (var weakRef in weakRefs)
                    {
                        if (weakRef.IsAlive)
                        {
                            string objectId = RuntimeHelpers.GetHashCode(weakRef.Target).ToString();
                            if (_creationStackTraces.ContainsKey(objectId))
                            {
                                sampleStackTrace = _creationStackTraces[objectId];
                                break;
                            }
                        }
                    }
                    
                    // Emit warning signal
                    EmitSignal(SignalName.MemoryLeakDetected, objectType, aliveCount, sampleStackTrace);
                    
                    if (LogToConsole)
                    {
                        GD.PrintWarning($"MemoryProfiler: Potential memory leak detected! " +
                                       $"{aliveCount} instances of {objectType} still alive.");
                        GD.PrintWarning($"Sample creation stack trace: {sampleStackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Counts objects by type for the current snapshot.
        /// </summary>
        private void CountObjectsByType()
        {
            // Count tracked objects by type
            foreach (var entry in _trackedObjects)
            {
                string objectType = entry.Key;
                int aliveCount = entry.Value.Count(wr => wr.IsAlive);
                
                if (aliveCount > 0)
                {
                    _objectCounts[objectType] = aliveCount;
                }
            }
            
            // Add Godot object counts
            _objectCounts["Godot.Node"] = Engine.GetProcessFrames();
            _objectCounts["Total Memory (bytes)"] = (int)GC.GetTotalMemory(false);
        }

        /// <summary>
        /// Analyzes memory changes between snapshots to detect potential issues.
        /// </summary>
        private void AnalyzeMemoryChanges()
        {
            if (_previousSnapshot.Count == 0)
                return;

            foreach (var entry in _objectCounts)
            {
                string objectType = entry.Key;
                int currentCount = entry.Value;
                
                // Skip non-numeric entries
                if (objectType == "Total Memory (bytes)")
                    continue;
                
                // Check if this type existed in previous snapshot
                if (_previousSnapshot.TryGetValue(objectType, out int previousCount))
                {
                    // Calculate growth
                    float growthRate = previousCount > 0 ? (float)(currentCount - previousCount) / previousCount : 0;
                    
                    // Check for significant growth
                    if (previousCount > 10 && growthRate > WarningGrowthRate)
                    {
                        EmitSignal(SignalName.MemoryWarning, 
                            $"Significant growth detected: {objectType} increased by {growthRate:P0}", 
                            objectType, 
                            currentCount);
                        
                        if (LogToConsole)
                        {
                            GD.PrintWarning($"MemoryProfiler: {objectType} count increased from {previousCount} to {currentCount} ({growthRate:P0})");
                        }
                    }
                }
                else if (currentCount > WarningThreshold)
                {
                    // New object type with high count
                    EmitSignal(SignalName.MemoryWarning, 
                        $"New object type with high count: {objectType}", 
                        objectType, 
                        currentCount);
                    
                    if (LogToConsole)
                    {
                        GD.PrintWarning($"MemoryProfiler: New object type {objectType} with high count: {currentCount}");
                    }
                }
            }
        }

        /// <summary>
        /// Formats a byte count into a human-readable string.
        /// </summary>
        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB" };
            int order = 0;
            double size = bytes;
            
            while (size >= 1024 && order < suffixes.Length - 1)
            {
                order++;
                size /= 1024;
            }
            
            return $"{size:0.##} {suffixes[order]}";
        }
    }
}

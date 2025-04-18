using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    /// <summary>
    /// Manages disposable resources to prevent memory leaks.
    /// Automatically disposes resources when they are no longer needed.
    /// </summary>
    [GlobalClass]
    public partial class DisposableResourceManager : Node
    {
        // Singleton instance
        private static DisposableResourceManager _instance;

        // List of resources to dispose on exit
        private List<IDisposable> _disposables = new List<IDisposable>();

        // Dictionary to track resources by owner
        private Dictionary<Node, List<IDisposable>> _resourcesByOwner = new Dictionary<Node, List<IDisposable>>();

        // Configuration
        [Export]
        public bool LogDisposals { get; set; } = true;

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static DisposableResourceManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var mainLoop = Engine.GetMainLoop();
                    if (mainLoop is SceneTree sceneTree)
                    {
                        _instance = sceneTree.Root.GetNodeOrNull<DisposableResourceManager>("/root/DisposableResourceManager");
                    }

                    if (_instance == null)
                    {
                        GD.PrintErr("DisposableResourceManager: Instance not found. Make sure it's added as an autoload singleton.");
                    }
                }

                return _instance;
            }
        }

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            GD.Print("DisposableResourceManager: Initializing...");
            _instance = this;

            // Connect to tree exiting signal to clean up resources
            GetTree().Root.TreeExiting += OnTreeExiting;

            GD.Print("DisposableResourceManager: Ready");
        }

        // Called when the application is about to exit
        private void OnTreeExiting()
        {
            DisposeAllResources();
        }

        /// <summary>
        /// Registers a disposable resource to be managed.
        /// </summary>
        /// <param name="disposable">The disposable resource</param>
        /// <param name="owner">Optional owner node. If provided, the resource will be disposed when the owner is freed.</param>
        public void RegisterDisposable(IDisposable disposable, Node owner = null)
        {
            if (disposable == null)
                return;

            // Add to global list
            if (!_disposables.Contains(disposable))
            {
                _disposables.Add(disposable);
            }

            // If owner is provided, associate the resource with it
            if (owner != null)
            {
                if (!_resourcesByOwner.ContainsKey(owner))
                {
                    _resourcesByOwner[owner] = new List<IDisposable>();

                    // Connect to owner's tree_exiting signal
                    owner.TreeExiting += () => DisposeResourcesForOwner(owner);
                }

                if (!_resourcesByOwner[owner].Contains(disposable))
                {
                    _resourcesByOwner[owner].Add(disposable);
                }
            }

            if (LogDisposals)
            {
                GD.Print($"DisposableResourceManager: Registered {disposable.GetType().Name}" +
                        (owner != null ? $" owned by {owner.Name}" : ""));
            }
        }

        /// <summary>
        /// Unregisters a disposable resource.
        /// </summary>
        /// <param name="disposable">The disposable resource</param>
        /// <param name="dispose">Whether to dispose the resource</param>
        public void UnregisterDisposable(IDisposable disposable, bool dispose = true)
        {
            if (disposable == null)
                return;

            // Remove from global list
            _disposables.Remove(disposable);

            // Remove from all owners
            foreach (var entry in _resourcesByOwner)
            {
                entry.Value.Remove(disposable);
            }

            // Dispose if requested
            if (dispose)
            {
                try
                {
                    disposable.Dispose();

                    if (LogDisposals)
                    {
                        GD.Print($"DisposableResourceManager: Disposed {disposable.GetType().Name}");
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"DisposableResourceManager: Error disposing {disposable.GetType().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Disposes all resources associated with an owner.
        /// </summary>
        /// <param name="owner">The owner node</param>
        public void DisposeResourcesForOwner(Node owner)
        {
            if (owner == null || !_resourcesByOwner.ContainsKey(owner))
                return;

            if (LogDisposals)
            {
                GD.Print($"DisposableResourceManager: Disposing resources for {owner.Name}");
            }

            // Get resources for this owner
            var resources = new List<IDisposable>(_resourcesByOwner[owner]);

            // Dispose each resource
            foreach (var disposable in resources)
            {
                try
                {
                    // Remove from global list
                    _disposables.Remove(disposable);

                    // Dispose
                    disposable.Dispose();

                    if (LogDisposals)
                    {
                        GD.Print($"DisposableResourceManager: Disposed {disposable.GetType().Name} owned by {owner.Name}");
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"DisposableResourceManager: Error disposing {disposable.GetType().Name}: {ex.Message}");
                }
            }

            // Clear the list
            _resourcesByOwner.Remove(owner);
        }

        /// <summary>
        /// Disposes all managed resources.
        /// </summary>
        public void DisposeAllResources()
        {
            if (LogDisposals)
            {
                GD.Print($"DisposableResourceManager: Disposing all resources ({_disposables.Count} total)");
            }

            // Create a copy of the list to avoid modification during iteration
            var disposables = new List<IDisposable>(_disposables);

            // Dispose each resource
            foreach (var disposable in disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"DisposableResourceManager: Error disposing {disposable.GetType().Name}: {ex.Message}");
                }
            }

            // Clear all lists
            _disposables.Clear();
            _resourcesByOwner.Clear();

            if (LogDisposals)
            {
                GD.Print("DisposableResourceManager: All resources disposed");
            }
        }

        /// <summary>
        /// Creates and registers a disposable resource.
        /// </summary>
        /// <typeparam name="T">The resource type</typeparam>
        /// <param name="factory">Factory function to create the resource</param>
        /// <param name="owner">Optional owner node</param>
        /// <returns>The created resource</returns>
        public T CreateManagedResource<T>(Func<T> factory, Node owner = null) where T : IDisposable
        {
            T resource = factory();
            RegisterDisposable(resource, owner);
            return resource;
        }

        /// <summary>
        /// Logs information about managed resources.
        /// </summary>
        public void LogResourceInfo()
        {
            GD.Print("=== DISPOSABLE RESOURCE REPORT ===");
            GD.Print($"Total managed resources: {_disposables.Count}");

            // Group by type
            Dictionary<string, int> countsByType = new Dictionary<string, int>();

            foreach (var disposable in _disposables)
            {
                string typeName = disposable.GetType().Name;

                if (!countsByType.ContainsKey(typeName))
                {
                    countsByType[typeName] = 0;
                }

                countsByType[typeName]++;
            }

            // Print counts by type
            foreach (var entry in countsByType)
            {
                GD.Print($"  {entry.Key}: {entry.Value}");
            }

            // Print resources by owner
            GD.Print("Resources by owner:");
            foreach (var entry in _resourcesByOwner)
            {
                Node owner = entry.Key;
                var resources = entry.Value;

                GD.Print($"  {owner.Name}: {resources.Count} resources");
            }

            GD.Print("==================================");
        }
    }
}

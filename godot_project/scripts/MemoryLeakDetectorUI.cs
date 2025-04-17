using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalLost
{
    /// <summary>
    /// UI for visualizing memory usage and detecting memory leaks.
    /// </summary>
    [GlobalClass]
    public partial class MemoryLeakDetectorUI : Control
    {
        // UI references
        private Label _memoryUsageLabel;
        private Label _objectCountLabel;
        private RichTextLabel _leakReportLabel;
        private Button _gcButton;
        private Button _snapshotButton;
        private Button _reportButton;
        private LineEdit _filterEdit;
        private ItemList _objectList;
        private CheckBox _autoUpdateCheckbox;
        private Timer _updateTimer;
        private ProgressBar _memoryBar;

        // References to other components
        private MemoryProfiler _memoryProfiler;
        private ResourceTracker _resourceTracker;
        private DisposableResourceManager _disposableManager;

        // Memory history for graph
        private List<long> _memoryHistory = new List<long>();
        private List<int> _objectCountHistory = new List<int>();
        private const int MAX_HISTORY_POINTS = 100;

        // Configuration
        [Export]
        public float UpdateInterval { get; set; } = 1.0f; // seconds

        [Export]
        public bool ShowOnStartup { get; set; } = false;

        [Export]
        public bool DetailedReporting { get; set; } = true;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get UI references
            _memoryUsageLabel = GetNode<Label>("%MemoryUsageLabel");
            _objectCountLabel = GetNode<Label>("%ObjectCountLabel");
            _leakReportLabel = GetNode<RichTextLabel>("%LeakReportLabel");
            _gcButton = GetNode<Button>("%GCButton");
            _snapshotButton = GetNode<Button>("%SnapshotButton");
            _reportButton = GetNode<Button>("%ReportButton");
            _filterEdit = GetNode<LineEdit>("%FilterEdit");
            _objectList = GetNode<ItemList>("%ObjectList");
            _autoUpdateCheckbox = GetNode<CheckBox>("%AutoUpdateCheckbox");
            _updateTimer = GetNode<Timer>("%UpdateTimer");
            _memoryBar = GetNode<ProgressBar>("%MemoryBar");

            // Get references to other components
            _memoryProfiler = GetNode<MemoryProfiler>("/root/MemoryProfiler");
            _resourceTracker = GetNode<ResourceTracker>("/root/ResourceTracker");
            _disposableManager = DisposableResourceManager.Instance;

            // Connect signals
            _gcButton.Pressed += OnGCButtonPressed;
            _snapshotButton.Pressed += OnSnapshotButtonPressed;
            _reportButton.Pressed += OnReportButtonPressed;
            _filterEdit.TextChanged += OnFilterTextChanged;
            _autoUpdateCheckbox.Toggled += OnAutoUpdateToggled;
            _updateTimer.Timeout += OnUpdateTimerTimeout;

            if (_memoryProfiler != null)
            {
                _memoryProfiler.MemoryLeakDetected += OnMemoryLeakDetected;
                _memoryProfiler.MemoryWarning += OnMemoryWarning;
                _memoryProfiler.MemorySnapshotTaken += OnMemorySnapshotTaken;
            }

            if (_resourceTracker != null)
            {
                _resourceTracker.ResourceLeakDetected += OnResourceLeakDetected;
            }

            // Set up timer
            _updateTimer.WaitTime = UpdateInterval;
            _updateTimer.OneShot = false;

            // Initial update
            UpdateUI();

            // Start timer if auto-update is enabled
            if (_autoUpdateCheckbox.ButtonPressed)
            {
                _updateTimer.Start();
            }

            // Show/hide based on configuration
            Visible = ShowOnStartup;
        }

        // Called when the GC button is pressed
        private void OnGCButtonPressed()
        {
            GD.Print("MemoryLeakDetectorUI: Forcing garbage collection...");

            if (_memoryProfiler != null)
            {
                _memoryProfiler.ForceGarbageCollection();
            }
            else
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            if (_resourceTracker != null)
            {
                _resourceTracker.ForceResourceCleanup();
            }

            UpdateUI();
        }

        // Called when the snapshot button is pressed
        private void OnSnapshotButtonPressed()
        {
            GD.Print("MemoryLeakDetectorUI: Taking memory snapshot...");

            if (_memoryProfiler != null)
            {
                _memoryProfiler.TakeMemorySnapshot();
            }

            UpdateUI();
        }

        // Called when the report button is pressed
        private void OnReportButtonPressed()
        {
            GD.Print("MemoryLeakDetectorUI: Generating memory report...");

            if (_memoryProfiler != null)
            {
                _memoryProfiler.LogMemoryUsage();
            }

            if (_resourceTracker != null)
            {
                _resourceTracker.LogResourceUsage();
            }

            if (_disposableManager != null)
            {
                _disposableManager.LogResourceInfo();
            }

            UpdateUI();
        }

        // Called when the filter text changes
        private void OnFilterTextChanged(string newText)
        {
            UpdateObjectList();
        }

        // Called when the auto-update checkbox is toggled
        private void OnAutoUpdateToggled(bool toggled)
        {
            if (toggled)
            {
                _updateTimer.Start();
            }
            else
            {
                _updateTimer.Stop();
            }
        }

        // Called when the update timer times out
        private void OnUpdateTimerTimeout()
        {
            UpdateUI();
        }

        // Called when a memory leak is detected
        private void OnMemoryLeakDetected(string objectType, int count, string stackTrace)
        {
            string message = $"[color=red]MEMORY LEAK DETECTED: {count} instances of {objectType}[/color]\n";

            if (DetailedReporting)
            {
                message += $"Stack trace: {stackTrace}\n";
            }

            _leakReportLabel.Text += message;

            // Auto-scroll to bottom
            _leakReportLabel.ScrollToLine(_leakReportLabel.GetLineCount() - 1);
        }

        // Called when a memory warning is received
        private void OnMemoryWarning(string message, string objectType, int count)
        {
            string formattedMessage = $"[color=yellow]WARNING: {message} (Count: {count})[/color]\n";
            _leakReportLabel.Text += formattedMessage;

            // Auto-scroll to bottom
            _leakReportLabel.ScrollToLine(_leakReportLabel.GetLineCount() - 1);
        }

        // Called when a resource leak is detected
        private void OnResourceLeakDetected(string resourceType, string resourcePath, int instanceCount)
        {
            string message = $"[color=orange]RESOURCE LEAK DETECTED: {instanceCount} instances of {resourceType} from {resourcePath}[/color]\n";
            _leakReportLabel.Text += message;

            // Auto-scroll to bottom
            _leakReportLabel.ScrollToLine(_leakReportLabel.GetLineCount() - 1);
        }

        // Called when a memory snapshot is taken
        private void OnMemorySnapshotTaken()
        {
            UpdateUI();
        }

        // Updates the UI with current memory information
        private void UpdateUI()
        {
            // Update memory usage
            long memoryUsage = GC.GetTotalMemory(false);
            _memoryUsageLabel.Text = $"Memory Usage: {FormatBytes(memoryUsage)}";

            // Update memory bar
            float memoryMB = memoryUsage / (1024f * 1024f);
            _memoryBar.Value = memoryMB;
            _memoryBar.MaxValue = Math.Max(_memoryBar.MaxValue, memoryMB + 10);

            // Update object count
            int objectCount = GC.GetTotalMemory(false) > 0 ? (int)Engine.GetProcessFrames() : 0;
            _objectCountLabel.Text = $"Object Count: {objectCount}";

            // Update history
            _memoryHistory.Add(memoryUsage);
            _objectCountHistory.Add(objectCount);

            // Limit history size
            if (_memoryHistory.Count > MAX_HISTORY_POINTS)
            {
                _memoryHistory.RemoveAt(0);
                _objectCountHistory.RemoveAt(0);
            }

            // Update object list
            UpdateObjectList();

            // Request redraw for custom drawing
            QueueRedraw();
        }

        // Updates the object list based on the current filter
        private void UpdateObjectList()
        {
            _objectList.Clear();

            if (_memoryProfiler == null)
                return;

            // Get snapshot data via reflection (since we can't directly access the private field)
            var objectCounts = new Dictionary<string, int>();

            // Add some basic information
            objectCounts["Total Memory"] = (int)(GC.GetTotalMemory(false) / 1024); // KB
            objectCounts["GC Gen 0 Collections"] = GC.CollectionCount(0);
            objectCounts["GC Gen 1 Collections"] = GC.CollectionCount(1);
            objectCounts["GC Gen 2 Collections"] = GC.CollectionCount(2);
            objectCounts["Process Frames"] = (int)Engine.GetProcessFrames();

            // Filter objects based on search text
            string filter = _filterEdit.Text.ToLower();

            foreach (var entry in objectCounts)
            {
                if (string.IsNullOrEmpty(filter) || entry.Key.ToLower().Contains(filter))
                {
                    _objectList.AddItem($"{entry.Key}: {entry.Value}");
                }
            }
        }

        // Custom drawing for memory graph
        public override void _Draw()
        {
            if (_memoryHistory.Count < 2)
                return;

            // Draw memory usage graph
            Rect2 graphRect = new Rect2(10, 300, Size.X - 20, 100);
            DrawRect(graphRect, new Color(0.1f, 0.1f, 0.1f, 0.5f));

            // Find min/max values
            long minMemory = _memoryHistory.Min();
            long maxMemory = _memoryHistory.Max();

            // Ensure we have a reasonable range
            if (maxMemory - minMemory < 1024)
            {
                maxMemory = minMemory + 1024;
            }

            // Draw graph lines
            for (int i = 1; i < _memoryHistory.Count; i++)
            {
                float x1 = graphRect.Position.X + (i - 1) * graphRect.Size.X / (MAX_HISTORY_POINTS - 1);
                float y1 = graphRect.Position.Y + graphRect.Size.Y - (_memoryHistory[i - 1] - minMemory) * graphRect.Size.Y / (maxMemory - minMemory);

                float x2 = graphRect.Position.X + i * graphRect.Size.X / (MAX_HISTORY_POINTS - 1);
                float y2 = graphRect.Position.Y + graphRect.Size.Y - (_memoryHistory[i] - minMemory) * graphRect.Size.Y / (maxMemory - minMemory);

                DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), new Color(0, 0.7f, 1, 0.8f), 2);
            }

            // Draw labels
            DrawString(GetThemeFont("font", "Label"), new Vector2(graphRect.Position.X, graphRect.Position.Y - 5),
                      $"Memory Usage (Min: {FormatBytes(minMemory)}, Max: {FormatBytes(maxMemory)})",
                      HorizontalAlignment.Left, -1, 12, new Color(1, 1, 1));
        }

        // Formats a byte count into a human-readable string
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

        // Toggle visibility
        public void ToggleVisibility()
        {
            Visible = !Visible;

            if (Visible)
            {
                UpdateUI();
            }
        }
    }
}

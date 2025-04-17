using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class PixelQuestUI : Control
    {
        // Configuration
        [Export] public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [Export] public Color PanelColor { get; set; } = new Color(0.2f, 0.2f, 0.2f, 1.0f);
        [Export] public Color BorderColor { get; set; } = new Color(0.5f, 0.5f, 0.5f, 1.0f);
        [Export] public Color TextColor { get; set; } = new Color(0.9f, 0.9f, 0.9f, 1.0f);
        [Export] public Color HighlightColor { get; set; } = new Color(0.3f, 0.7f, 0.3f, 1.0f);
        [Export] public Color ActiveQuestColor { get; set; } = new Color(0.2f, 0.6f, 0.2f, 1.0f);
        [Export] public Color CompletedQuestColor { get; set; } = new Color(0.6f, 0.6f, 0.2f, 1.0f);
        [Export] public Color FailedQuestColor { get; set; } = new Color(0.6f, 0.2f, 0.2f, 1.0f);
        [Export] public Color ButtonColor { get; set; } = new Color(0.3f, 0.3f, 0.3f, 1.0f);
        [Export] public Color ButtonHoverColor { get; set; } = new Color(0.4f, 0.4f, 0.4f, 1.0f);
        [Export] public int BorderWidth { get; set; } = 2;
        [Export] public int TabHeight { get; set; } = 30;
        [Export] public int QuestItemHeight { get; set; } = 40;

        // References to game systems
        private QuestSystem _questSystem;
        private GameState _gameState;

        // UI state
        private bool _isVisible = false;
        private string _selectedQuestId = null;
        private string _activeTab = "active"; // "active" or "completed"
        private Vector2 _mousePosition = Vector2.Zero;
        private Dictionary<string, Rect2> _questItemRects = new Dictionary<string, Rect2>();
        private Rect2 _activeTabRect;
        private Rect2 _completedTabRect;
        private Rect2 _closeButtonRect;
        private bool _closeButtonHovered = false;
        private Rect2 _activateButtonRect;
        private bool _activateButtonHovered = false;
        private Rect2 _abandonButtonRect;
        private bool _abandonButtonHovered = false;

        // Called when the node enters the scene tree
        public override void _Ready()
        {
            // Get references to game systems
            _questSystem = GetNode<QuestSystem>("/root/QuestSystem");
            _gameState = GetNode<GameState>("/root/GameState");

            if (_questSystem == null || _gameState == null)
            {
                GD.PrintErr("PixelQuestUI: Failed to get QuestSystem or GameState reference");
                return;
            }

            // Connect signals
            _questSystem.QuestDiscovered += OnQuestAdded;
            _questSystem.QuestActivated += OnQuestUpdated;
            _questSystem.QuestCompleted += OnQuestCompleted;

            // Set up input processing
            SetProcessInput(true);
        }

        // Show or hide the quest UI
        public new void SetVisible(bool visible)
        {
            _isVisible = visible;
            SetProcess(visible);
            SetProcessInput(visible);
            QueueRedraw();
        }

        // Toggle visibility
        public void ToggleVisibility()
        {
            SetVisible(!_isVisible);
        }

        // Check if the quest UI is visible
        public new bool IsVisible()
        {
            return _isVisible;
        }

        // Process input events
        public override void _Input(InputEvent @event)
        {
            if (!_isVisible)
                return;

            if (@event is InputEventMouseMotion mouseMotion)
            {
                _mousePosition = mouseMotion.Position;
                UpdateHoverStates();
            }
            else if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
                {
                    // Check if a tab was clicked
                    if (_activeTabRect.HasPoint(_mousePosition))
                    {
                        _activeTab = "active";
                        _selectedQuestId = null;
                        QueueRedraw();
                    }
                    else if (_completedTabRect.HasPoint(_mousePosition))
                    {
                        _activeTab = "completed";
                        _selectedQuestId = null;
                        QueueRedraw();
                    }
                    // Check if close button was clicked
                    else if (_closeButtonRect.HasPoint(_mousePosition))
                    {
                        SetVisible(false);
                    }
                    // Check if activate button was clicked
                    else if (_activateButtonRect.HasPoint(_mousePosition) && _selectedQuestId != null)
                    {
                        var quest = _questSystem.GetQuest(_selectedQuestId);
                        if (quest != null && !quest.IsActive && !quest.IsCompleted)
                        {
                            _questSystem.ActivateQuest(_selectedQuestId);
                        }
                    }
                    // Check if abandon button was clicked
                    else if (_abandonButtonRect.HasPoint(_mousePosition) && _selectedQuestId != null)
                    {
                        var quest = _questSystem.GetQuest(_selectedQuestId);
                        if (quest != null && quest.IsActive)
                        {
                            // Deactivate the quest (no AbandonQuest method available)
                            GD.Print($"Would abandon quest {_selectedQuestId} if method was available");
                        }
                    }
                    // Check if a quest item was clicked
                    else
                    {
                        foreach (var questId in _questItemRects.Keys)
                        {
                            if (_questItemRects[questId].HasPoint(_mousePosition))
                            {
                                _selectedQuestId = questId;
                                QueueRedraw();
                                break;
                            }
                        }
                    }
                }
            }
            else if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Close quest UI with Escape key
                if (keyEvent.Keycode == Key.Escape)
                {
                    SetVisible(false);
                }
            }
        }

        // Custom drawing function
        public override void _Draw()
        {
            if (!_isVisible)
                return;

            Vector2 size = Size;

            // Draw background
            DrawRect(new Rect2(0, 0, size.X, size.Y), BackgroundColor);

            // Draw main panel
            float panelWidth = Mathf.Min(800, size.X - 40);
            float panelHeight = Mathf.Min(600, size.Y - 40);
            float panelX = (size.X - panelWidth) / 2;
            float panelY = (size.Y - panelHeight) / 2;

            DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight), PanelColor);
            DrawRect(new Rect2(panelX, panelY, panelWidth, panelHeight), BorderColor, false, BorderWidth);

            // Draw title
            DrawPixelText("QUESTS", new Vector2(panelX + 20, panelY + 25), TextColor, 2);

            // Draw tabs
            DrawTabs(panelX, panelY, panelWidth);

            // Draw quest list
            DrawQuestList(panelX + 20, panelY + 60, panelWidth / 2 - 30, panelHeight - 80);

            // Draw quest details if a quest is selected
            if (_selectedQuestId != null)
            {
                DrawQuestDetails(panelX + panelWidth / 2 + 10, panelY + 60, panelWidth / 2 - 30, panelHeight - 80);
            }

            // Draw close button
            _closeButtonRect = new Rect2(panelX + panelWidth - 40, panelY + 10, 30, 30);
            DrawRect(_closeButtonRect, _closeButtonHovered ? ButtonHoverColor : ButtonColor);
            DrawRect(_closeButtonRect, BorderColor, false, 1);
            DrawPixelText("X", new Vector2(_closeButtonRect.Position.X + 10, _closeButtonRect.Position.Y + 20), TextColor, 1);
        }

        // Draw the tabs
        private void DrawTabs(float x, float y, float width)
        {
            float tabWidth = 120;

            // Active quests tab
            _activeTabRect = new Rect2(x + 20, y + 30, tabWidth, TabHeight);
            Color activeTabColor = _activeTab == "active" ? HighlightColor : ButtonColor;
            DrawRect(_activeTabRect, activeTabColor);
            DrawRect(_activeTabRect, BorderColor, false, 1);
            DrawPixelText("ACTIVE", new Vector2(_activeTabRect.Position.X + 10, _activeTabRect.Position.Y + 20), TextColor, 1);

            // Completed quests tab
            _completedTabRect = new Rect2(x + 20 + tabWidth + 10, y + 30, tabWidth, TabHeight);
            Color completedTabColor = _activeTab == "completed" ? HighlightColor : ButtonColor;
            DrawRect(_completedTabRect, completedTabColor);
            DrawRect(_completedTabRect, BorderColor, false, 1);
            DrawPixelText("COMPLETED", new Vector2(_completedTabRect.Position.X + 10, _completedTabRect.Position.Y + 20), TextColor, 1);
        }

        // Draw the quest list
        private void DrawQuestList(float x, float y, float width, float height)
        {
            // Draw list background
            DrawRect(new Rect2(x, y, width, height), new Color(0.15f, 0.15f, 0.15f, 1.0f));
            DrawRect(new Rect2(x, y, width, height), BorderColor, false, 1);

            // Get quests based on active tab
            var quests = new Dictionary<string, QuestSystem.QuestData>();
            if (_activeTab == "active")
            {
                // Get active and available quests
                var activeQuests = _questSystem.GetActiveQuests();
                foreach (var quest in activeQuests)
                {
                    quests[quest.Key] = quest.Value;
                }

                var allQuests = _questSystem.GetDiscoveredQuests();
                foreach (var quest in allQuests)
                {
                    if (!quest.Value.IsActive && !quest.Value.IsCompleted)
                    {
                        quests[quest.Key] = quest.Value;
                    }
                }
            }
            else // completed tab
            {
                // Get completed quests
                var completedQuests = _questSystem.GetCompletedQuests();
                foreach (var quest in completedQuests)
                {
                    quests[quest.Key] = quest.Value;
                }
            }

            // Clear quest item rects
            _questItemRects.Clear();

            // Draw quest items
            float itemY = y + 10;
            foreach (var quest in quests)
            {
                // Skip if out of bounds
                if (itemY + QuestItemHeight > y + height)
                    break;

                // Determine item color based on status
                Color itemColor = ButtonColor;
                if (quest.Value.IsActive)
                    itemColor = ActiveQuestColor;
                else if (quest.Value.IsCompleted)
                    itemColor = CompletedQuestColor;

                // Highlight if selected
                if (quest.Key == _selectedQuestId)
                    itemColor = HighlightColor;

                // Draw item background
                Rect2 itemRect = new Rect2(x + 5, itemY, width - 10, QuestItemHeight);
                DrawRect(itemRect, itemColor);
                DrawRect(itemRect, BorderColor, false, 1);

                // Draw quest name
                DrawPixelText(quest.Value.Title, new Vector2(x + 15, itemY + 25), TextColor, 1);

                // Store item rect for interaction
                _questItemRects[quest.Key] = itemRect;

                // Move to next item
                itemY += QuestItemHeight + 5;
            }

            // Draw "No quests" message if list is empty
            if (quests.Count == 0)
            {
                string message = _activeTab == "active" ? "No active quests" : "No completed quests";
                DrawPixelText(message, new Vector2(x + 15, y + 30), TextColor, 1);
            }
        }

        // Draw the quest details
        private void DrawQuestDetails(float x, float y, float width, float height)
        {
            // Get the selected quest
            var quest = _questSystem.GetQuest(_selectedQuestId);
            if (quest == null)
                return;

            // Draw details background
            DrawRect(new Rect2(x, y, width, height), new Color(0.15f, 0.15f, 0.15f, 1.0f));
            DrawRect(new Rect2(x, y, width, height), BorderColor, false, 1);

            // Draw quest title
            DrawPixelText(quest.Title.ToUpper(), new Vector2(x + 15, y + 25), TextColor, 1);

            // Draw quest status
            string status = quest.IsCompleted ? "COMPLETED" : (quest.IsActive ? "ACTIVE" : "AVAILABLE");
            string statusText = $"STATUS: {status}";
            DrawPixelText(statusText, new Vector2(x + 15, y + 50), GetStatusColor(quest.IsCompleted, quest.IsActive), 1);

            // Draw quest description
            DrawWrappedText(quest.Description, new Vector2(x + 15, y + 80), width - 30, TextColor, 1);

            // Draw objectives
            float objectivesY = y + 150;
            DrawPixelText("OBJECTIVES:", new Vector2(x + 15, objectivesY), TextColor, 1);
            objectivesY += 25;

            foreach (var objective in quest.Objectives)
            {
                string checkmark = objective.IsCompleted ? "✓" : "□";
                string objectiveText = $"{checkmark} {objective.Description}";
                DrawPixelText(objectiveText, new Vector2(x + 15, objectivesY),
                    objective.IsCompleted ? CompletedQuestColor : TextColor, 1);
                objectivesY += 20;
            }

            // Draw reward
            if (!string.IsNullOrEmpty(quest.RewardItemId))
            {
                DrawPixelText("REWARD:", new Vector2(x + 15, objectivesY + 20), TextColor, 1);
                DrawPixelText($"{quest.RewardQuantity}x {quest.RewardItemId}", new Vector2(x + 15, objectivesY + 45), TextColor, 1);
            }

            // Draw action buttons based on quest status
            if (!quest.IsActive && !quest.IsCompleted)
            {
                // Draw activate button
                _activateButtonRect = new Rect2(x + width - 120, y + height - 40, 100, 30);
                DrawRect(_activateButtonRect, _activateButtonHovered ? ButtonHoverColor : ButtonColor);
                DrawRect(_activateButtonRect, BorderColor, false, 1);
                DrawPixelText("ACTIVATE", new Vector2(_activateButtonRect.Position.X + 10, _activateButtonRect.Position.Y + 20), TextColor, 1);
            }
            else if (quest.IsActive)
            {
                // Draw abandon button
                _abandonButtonRect = new Rect2(x + width - 120, y + height - 40, 100, 30);
                DrawRect(_abandonButtonRect, _abandonButtonHovered ? ButtonHoverColor : ButtonColor);
                DrawRect(_abandonButtonRect, BorderColor, false, 1);
                DrawPixelText("ABANDON", new Vector2(_abandonButtonRect.Position.X + 10, _abandonButtonRect.Position.Y + 20), TextColor, 1);
            }
        }

        // Draw wrapped text
        private void DrawWrappedText(string text, Vector2 position, float maxWidth, Color color, int scale = 1)
        {
            string[] words = text.Split(' ');
            string line = "";
            float y = position.Y;
            float charWidth = 6 * scale; // Approximate width of a character

            foreach (string word in words)
            {
                string testLine = line + (line.Length > 0 ? " " : "") + word;
                if (testLine.Length * charWidth > maxWidth)
                {
                    // Draw current line and start a new one
                    DrawPixelText(line, new Vector2(position.X, y), color, scale);
                    line = word;
                    y += 20 * scale;
                }
                else
                {
                    // Add word to current line
                    line = testLine;
                }
            }

            // Draw the last line
            if (line.Length > 0)
            {
                DrawPixelText(line, new Vector2(position.X, y), color, scale);
            }
        }

        // Draw pixel text
        private void DrawPixelText(string text, Vector2 position, Color color, int scale = 1)
        {
            // Use Godot's built-in font for now
            // In a real implementation, you would use a custom pixel font renderer
            DrawString(ThemeDB.FallbackFont, position, text, HorizontalAlignment.Left, -1, 12 * scale, color);
        }

        // Update hover states for interactive elements
        private void UpdateHoverStates()
        {
            bool needsRedraw = false;

            // Check close button hover
            bool closeButtonHovered = _closeButtonRect.HasPoint(_mousePosition);
            if (closeButtonHovered != _closeButtonHovered)
            {
                _closeButtonHovered = closeButtonHovered;
                needsRedraw = true;
            }

            // Check activate button hover
            bool activateButtonHovered = _activateButtonRect.HasPoint(_mousePosition);
            if (activateButtonHovered != _activateButtonHovered)
            {
                _activateButtonHovered = activateButtonHovered;
                needsRedraw = true;
            }

            // Check abandon button hover
            bool abandonButtonHovered = _abandonButtonRect.HasPoint(_mousePosition);
            if (abandonButtonHovered != _abandonButtonHovered)
            {
                _abandonButtonHovered = abandonButtonHovered;
                needsRedraw = true;
            }

            if (needsRedraw)
            {
                QueueRedraw();
            }
        }

        // Get color based on quest status
        private Color GetStatusColor(bool isCompleted, bool isActive)
        {
            if (isCompleted)
                return CompletedQuestColor;
            else if (isActive)
                return ActiveQuestColor;
            else
                return TextColor;
        }

        // Event handlers
        private void OnQuestAdded(string questId)
        {
            QueueRedraw();
        }

        private void OnQuestUpdated(string questId)
        {
            QueueRedraw();
        }

        private void OnQuestCompleted(string questId)
        {
            QueueRedraw();
        }
    }
}

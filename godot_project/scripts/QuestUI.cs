using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    [GlobalClass]
    public partial class QuestUI : Control
    {
        // References to UI elements
        private TabContainer _tabContainer;
        private VBoxContainer _activeQuestsContainer;
        private VBoxContainer _completedQuestsContainer;
        private Panel _questDetailsPanel;
        private Label _questTitle;
        private Label _questDescription;
        private VBoxContainer _objectivesContainer;
        private Label _questReward;
        private Button _activateButton;
        private Button _closeButton;

        // References to game systems
        private QuestSystem _questSystem;
        private InventorySystem _inventorySystem;

        // Currently selected quest
        private string _selectedQuestId;

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to UI elements
            _tabContainer = GetNode<TabContainer>("TabContainer");
            _activeQuestsContainer = GetNode<VBoxContainer>("TabContainer/Active/ScrollContainer/ActiveQuestsContainer");
            _completedQuestsContainer = GetNode<VBoxContainer>("TabContainer/Completed/ScrollContainer/CompletedQuestsContainer");
            _questDetailsPanel = GetNode<Panel>("QuestDetailsPanel");
            _questTitle = GetNode<Label>("QuestDetailsPanel/QuestTitle");
            _questDescription = GetNode<Label>("QuestDetailsPanel/QuestDescription");
            _objectivesContainer = GetNode<VBoxContainer>("QuestDetailsPanel/ObjectivesContainer");
            _questReward = GetNode<Label>("QuestDetailsPanel/QuestReward");
            _activateButton = GetNode<Button>("QuestDetailsPanel/ActivateButton");
            _closeButton = GetNode<Button>("CloseButton");

            // Get references to game systems
            _questSystem = GetNode<QuestSystem>("/root/QuestSystem");
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");

            if (_questSystem == null || _inventorySystem == null)
            {
                GD.PrintErr("QuestUI: Failed to get QuestSystem or InventorySystem reference");
                return;
            }

            // Connect signals
            _closeButton.Pressed += OnCloseButtonPressed;
            _activateButton.Pressed += OnActivateButtonPressed;
            _questSystem.QuestDiscovered += OnQuestDiscovered;
            _questSystem.QuestActivated += OnQuestActivated;
            _questSystem.QuestCompleted += OnQuestCompleted;
            _questSystem.QuestObjectiveUpdated += OnQuestObjectiveUpdated;
            _questSystem.QuestObjectiveCompleted += OnQuestObjectiveCompleted;

            // Initialize the UI
            UpdateQuestUI();
            HideQuestDetails();
        }

        // Update the quest UI
        private void UpdateQuestUI()
        {
            // Clear the quest containers
            foreach (var child in _activeQuestsContainer.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var child in _completedQuestsContainer.GetChildren())
            {
                child.QueueFree();
            }

            // Get the quests
            var discoveredQuests = _questSystem.GetDiscoveredQuests();
            var activeQuests = _questSystem.GetActiveQuests();
            var completedQuests = _questSystem.GetCompletedQuests();

            // Add active quests to the active quests container
            foreach (var quest in activeQuests.Values)
            {
                var questButton = new Button();
                questButton.Text = quest.Title;
                questButton.TooltipText = quest.Description;
                questButton.CustomMinimumSize = new Vector2(0, 40);
                questButton.Pressed += () => OnQuestButtonPressed(quest.Id);
                _activeQuestsContainer.AddChild(questButton);
            }

            // Add discovered but not active quests to the active quests container
            foreach (var quest in discoveredQuests.Values)
            {
                if (!quest.IsActive && !quest.IsCompleted)
                {
                    var questButton = new Button();
                    questButton.Text = quest.Title;
                    questButton.TooltipText = quest.Description;
                    questButton.CustomMinimumSize = new Vector2(0, 40);
                    questButton.Pressed += () => OnQuestButtonPressed(quest.Id);
                    _activeQuestsContainer.AddChild(questButton);
                }
            }

            // Add completed quests to the completed quests container
            foreach (var quest in completedQuests.Values)
            {
                var questButton = new Button();
                questButton.Text = quest.Title;
                questButton.TooltipText = quest.Description;
                questButton.CustomMinimumSize = new Vector2(0, 40);
                questButton.Pressed += () => OnQuestButtonPressed(quest.Id);
                _completedQuestsContainer.AddChild(questButton);
            }
        }

        // Show quest details
        private void ShowQuestDetails(string questId)
        {
            var quest = _questSystem.GetQuest(questId);
            if (quest == null)
            {
                return;
            }

            _selectedQuestId = questId;

            _questTitle.Text = quest.Title;
            _questDescription.Text = quest.Description;

            // Clear the objectives container
            foreach (var child in _objectivesContainer.GetChildren())
            {
                child.QueueFree();
            }

            // Add objectives
            foreach (var objective in quest.Objectives)
            {
                var objectiveLabel = new Label();
                objectiveLabel.Text = $"{objective.Description}: {objective.CurrentAmount}/{objective.RequiredAmount}";
                
                if (objective.IsCompleted)
                {
                    objectiveLabel.Modulate = new Color(0, 1, 0); // Green for completed objectives
                }
                
                _objectivesContainer.AddChild(objectiveLabel);
            }

            // Set reward text
            if (!string.IsNullOrEmpty(quest.RewardItemId))
            {
                var item = _inventorySystem.GetItem(quest.RewardItemId);
                if (item != null)
                {
                    _questReward.Text = $"Reward: {quest.RewardQuantity}x {item.Name}";
                }
                else
                {
                    _questReward.Text = "Reward: Unknown";
                }
            }
            else
            {
                _questReward.Text = "Reward: None";
            }

            // Set activate button state
            _activateButton.Disabled = quest.IsActive || quest.IsCompleted;
            _activateButton.Text = quest.IsActive ? "Active" : (quest.IsCompleted ? "Completed" : "Activate");

            _questDetailsPanel.Visible = true;
        }

        // Hide quest details
        private void HideQuestDetails()
        {
            _selectedQuestId = null;
            _questDetailsPanel.Visible = false;
        }

        // Event handlers
        private void OnCloseButtonPressed()
        {
            // Hide the quest UI
            Visible = false;
        }

        private void OnQuestButtonPressed(string questId)
        {
            // Show quest details
            ShowQuestDetails(questId);
        }

        private void OnActivateButtonPressed()
        {
            if (_selectedQuestId != null)
            {
                _questSystem.ActivateQuest(_selectedQuestId);
                UpdateQuestUI();
                ShowQuestDetails(_selectedQuestId);
            }
        }

        private void OnQuestDiscovered(string questId)
        {
            UpdateQuestUI();
        }

        private void OnQuestActivated(string questId)
        {
            UpdateQuestUI();
        }

        private void OnQuestCompleted(string questId)
        {
            UpdateQuestUI();
            if (_selectedQuestId == questId)
            {
                ShowQuestDetails(questId);
            }
        }

        private void OnQuestObjectiveUpdated(string questId, string objectiveId, int currentAmount, int requiredAmount)
        {
            if (_selectedQuestId == questId)
            {
                ShowQuestDetails(questId);
            }
        }

        private void OnQuestObjectiveCompleted(string questId, string objectiveId)
        {
            if (_selectedQuestId == questId)
            {
                ShowQuestDetails(questId);
            }
        }
    }
}

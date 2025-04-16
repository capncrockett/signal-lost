using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalLost
{
    [GlobalClass]
    public partial class QuestSystem : Node
    {
        // Quest data
        public class QuestData
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public List<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();
            public string RewardItemId { get; set; }
            public int RewardQuantity { get; set; } = 1;
            public bool IsCompleted { get; set; } = false;
            public bool IsActive { get; set; } = false;
            public bool IsDiscovered { get; set; } = false;
            public string PrerequisiteQuestId { get; set; } // Quest that must be completed before this one becomes available
            public string LocationId { get; set; } // Location where this quest is available
        }

        // Quest objective data
        public class QuestObjective
        {
            public string Id { get; set; }
            public string Description { get; set; }
            public QuestObjectiveType Type { get; set; }
            public string TargetId { get; set; } // Item ID, location ID, etc.
            public int RequiredAmount { get; set; } = 1;
            public int CurrentAmount { get; set; } = 0;
            public bool IsCompleted { get; set; } = false;
        }

        // Quest objective types
        public enum QuestObjectiveType
        {
            CollectItem,
            VisitLocation,
            DecodeSignal,
            UseItem,
            Custom
        }

        // Dictionary of all quests in the game
        private Dictionary<string, QuestData> _questDatabase = new Dictionary<string, QuestData>();

        // Player's active and completed quests
        private Dictionary<string, QuestData> _activeQuests = new Dictionary<string, QuestData>();
        private Dictionary<string, QuestData> _completedQuests = new Dictionary<string, QuestData>();

        // References to other systems
        private GameState _gameState;
        private InventorySystem _inventorySystem;
        private MapSystem _mapSystem;

        // Signals
        [Signal]
        public delegate void QuestDiscoveredEventHandler(string questId);

        [Signal]
        public delegate void QuestActivatedEventHandler(string questId);

        [Signal]
        public delegate void QuestCompletedEventHandler(string questId);

        [Signal]
        public delegate void QuestObjectiveUpdatedEventHandler(string questId, string objectiveId, int currentAmount, int requiredAmount);

        [Signal]
        public delegate void QuestObjectiveCompletedEventHandler(string questId, string objectiveId);

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to other systems
            _gameState = GetNode<GameState>("/root/GameState");
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");

            if (_gameState == null || _inventorySystem == null || _mapSystem == null)
            {
                GD.PrintErr("QuestSystem: Failed to get references to other systems");
                return;
            }

            // Connect signals from other systems
            _gameState.MessageDecoded += OnMessageDecoded;
            _gameState.LocationChanged += OnLocationChanged;
            _gameState.InventoryChanged += OnInventoryChanged;
            _inventorySystem.ItemUsed += OnItemUsed;
            _mapSystem.LocationDiscovered += OnLocationDiscovered;

            // Initialize quest database
            InitializeQuestDatabase();
        }

        // Initialize the quest database with all available quests in the game
        private void InitializeQuestDatabase()
        {
            // Add quests to the database
            AddQuestToDatabase(new QuestData
            {
                Id = "quest_radio_repair",
                Title = "Radio Repair",
                Description = "Your radio is damaged and needs repair. Find the necessary components to fix it.",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "find_radio_part",
                        Description = "Find a radio component",
                        Type = QuestObjectiveType.CollectItem,
                        TargetId = "radio_part",
                        RequiredAmount = 1
                    }
                },
                RewardItemId = "battery",
                RewardQuantity = 2,
                LocationId = "bunker",
                IsDiscovered = true // This quest is available from the start
            });

            AddQuestToDatabase(new QuestData
            {
                Id = "quest_explore_forest",
                Title = "Forest Exploration",
                Description = "Explore the forest area to find signs of other survivors.",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "visit_forest",
                        Description = "Visit the forest",
                        Type = QuestObjectiveType.VisitLocation,
                        TargetId = "forest",
                        RequiredAmount = 1
                    },
                    new QuestObjective
                    {
                        Id = "find_map_fragment",
                        Description = "Find a map fragment",
                        Type = QuestObjectiveType.CollectItem,
                        TargetId = "map_fragment",
                        RequiredAmount = 1
                    }
                },
                RewardItemId = "medkit",
                RewardQuantity = 1,
                LocationId = "bunker",
                IsDiscovered = true // This quest is available from the start
            });

            AddQuestToDatabase(new QuestData
            {
                Id = "quest_decode_signal",
                Title = "Mysterious Signal",
                Description = "There's a strange signal being broadcast. Tune your radio to decode it.",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "decode_emergency_broadcast",
                        Description = "Decode the emergency broadcast signal",
                        Type = QuestObjectiveType.DecodeSignal,
                        TargetId = "msg_001",
                        RequiredAmount = 1
                    }
                },
                RewardItemId = "flashlight",
                RewardQuantity = 1,
                PrerequisiteQuestId = "quest_radio_repair",
                LocationId = "bunker"
            });

            AddQuestToDatabase(new QuestData
            {
                Id = "quest_find_cabin",
                Title = "Hunter's Cabin",
                Description = "Find the hunter's cabin in the forest. It might contain useful supplies.",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "find_cabin_key",
                        Description = "Find the cabin key",
                        Type = QuestObjectiveType.CollectItem,
                        TargetId = "key_cabin",
                        RequiredAmount = 1
                    },
                    new QuestObjective
                    {
                        Id = "visit_cabin",
                        Description = "Visit the cabin",
                        Type = QuestObjectiveType.VisitLocation,
                        TargetId = "cabin",
                        RequiredAmount = 1
                    },
                    new QuestObjective
                    {
                        Id = "use_cabin_key",
                        Description = "Use the cabin key",
                        Type = QuestObjectiveType.UseItem,
                        TargetId = "key_cabin",
                        RequiredAmount = 1
                    }
                },
                RewardItemId = "canned_food",
                RewardQuantity = 3,
                PrerequisiteQuestId = "quest_explore_forest",
                LocationId = "forest"
            });

            AddQuestToDatabase(new QuestData
            {
                Id = "quest_survivor_message",
                Title = "Survivor's Message",
                Description = "Decode the message from survivors at the old factory.",
                Objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        Id = "decode_survivor_message",
                        Description = "Decode the survivor's message",
                        Type = QuestObjectiveType.DecodeSignal,
                        TargetId = "msg_003",
                        RequiredAmount = 1
                    },
                    new QuestObjective
                    {
                        Id = "visit_factory",
                        Description = "Visit the factory",
                        Type = QuestObjectiveType.VisitLocation,
                        TargetId = "factory",
                        RequiredAmount = 1
                    }
                },
                RewardItemId = "water_bottle",
                RewardQuantity = 2,
                PrerequisiteQuestId = "quest_decode_signal",
                LocationId = "town"
            });
        }

        // Add a quest to the database
        private void AddQuestToDatabase(QuestData quest)
        {
            _questDatabase[quest.Id] = quest;
        }

        // Get a quest from the database by ID
        public QuestData GetQuest(string questId)
        {
            if (_questDatabase.ContainsKey(questId))
            {
                return _questDatabase[questId];
            }
            return null;
        }

        // Get all quests in the database
        public Dictionary<string, QuestData> GetAllQuests()
        {
            return _questDatabase;
        }

        // Get all discovered quests
        public Dictionary<string, QuestData> GetDiscoveredQuests()
        {
            var discoveredQuests = new Dictionary<string, QuestData>();
            foreach (var quest in _questDatabase.Values)
            {
                if (quest.IsDiscovered)
                {
                    discoveredQuests[quest.Id] = quest;
                }
            }
            return discoveredQuests;
        }

        // Get all active quests
        public Dictionary<string, QuestData> GetActiveQuests()
        {
            return _activeQuests;
        }

        // Get all completed quests
        public Dictionary<string, QuestData> GetCompletedQuests()
        {
            return _completedQuests;
        }

        // Discover a quest
        public bool DiscoverQuest(string questId)
        {
            if (!_questDatabase.ContainsKey(questId))
            {
                GD.PrintErr($"QuestSystem: Quest {questId} not found in the database");
                return false;
            }

            var quest = _questDatabase[questId];

            // Check if the quest is already discovered
            if (quest.IsDiscovered)
            {
                return false;
            }

            // Check if the prerequisite quest is completed
            if (!string.IsNullOrEmpty(quest.PrerequisiteQuestId) && 
                (!_completedQuests.ContainsKey(quest.PrerequisiteQuestId)))
            {
                return false;
            }

            // Discover the quest
            quest.IsDiscovered = true;
            EmitSignal(SignalName.QuestDiscovered, questId);
            return true;
        }

        // Activate a quest
        public bool ActivateQuest(string questId)
        {
            if (!_questDatabase.ContainsKey(questId))
            {
                GD.PrintErr($"QuestSystem: Quest {questId} not found in the database");
                return false;
            }

            var quest = _questDatabase[questId];

            // Check if the quest is already active or completed
            if (quest.IsActive || quest.IsCompleted)
            {
                return false;
            }

            // Check if the quest is discovered
            if (!quest.IsDiscovered)
            {
                return false;
            }

            // Activate the quest
            quest.IsActive = true;
            _activeQuests[questId] = quest;
            EmitSignal(SignalName.QuestActivated, questId);
            return true;
        }

        // Complete a quest
        public bool CompleteQuest(string questId)
        {
            if (!_activeQuests.ContainsKey(questId))
            {
                GD.PrintErr($"QuestSystem: Quest {questId} not found in active quests");
                return false;
            }

            var quest = _activeQuests[questId];

            // Check if all objectives are completed
            foreach (var objective in quest.Objectives)
            {
                if (!objective.IsCompleted)
                {
                    return false;
                }
            }

            // Complete the quest
            quest.IsCompleted = true;
            quest.IsActive = false;
            _completedQuests[questId] = quest;
            _activeQuests.Remove(questId);

            // Give rewards
            if (!string.IsNullOrEmpty(quest.RewardItemId))
            {
                _inventorySystem.AddItemToInventory(quest.RewardItemId, quest.RewardQuantity);
            }

            // Check if completing this quest unlocks any other quests
            foreach (var otherQuest in _questDatabase.Values)
            {
                if (otherQuest.PrerequisiteQuestId == questId && !otherQuest.IsDiscovered)
                {
                    DiscoverQuest(otherQuest.Id);
                }
            }

            EmitSignal(SignalName.QuestCompleted, questId);
            return true;
        }

        // Update a quest objective
        public bool UpdateQuestObjective(string questId, string objectiveId, int amount = 1)
        {
            if (!_activeQuests.ContainsKey(questId))
            {
                return false;
            }

            var quest = _activeQuests[questId];
            var objective = quest.Objectives.FirstOrDefault(o => o.Id == objectiveId);

            if (objective == null || objective.IsCompleted)
            {
                return false;
            }

            // Update the objective
            objective.CurrentAmount += amount;
            if (objective.CurrentAmount >= objective.RequiredAmount)
            {
                objective.CurrentAmount = objective.RequiredAmount;
                objective.IsCompleted = true;
                EmitSignal(SignalName.QuestObjectiveCompleted, questId, objectiveId);

                // Check if all objectives are completed
                if (quest.Objectives.All(o => o.IsCompleted))
                {
                    CompleteQuest(questId);
                }
            }
            else
            {
                EmitSignal(SignalName.QuestObjectiveUpdated, questId, objectiveId, objective.CurrentAmount, objective.RequiredAmount);
            }

            return true;
        }

        // Check for quests in the current location
        public void CheckLocationForQuests(string locationId)
        {
            foreach (var quest in _questDatabase.Values)
            {
                if (quest.LocationId == locationId && !quest.IsDiscovered)
                {
                    // Check if the prerequisite quest is completed
                    if (string.IsNullOrEmpty(quest.PrerequisiteQuestId) || 
                        _completedQuests.ContainsKey(quest.PrerequisiteQuestId))
                    {
                        DiscoverQuest(quest.Id);
                    }
                }
            }

            // Update location visit objectives
            foreach (var quest in _activeQuests.Values)
            {
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.VisitLocation && 
                        objective.TargetId == locationId && 
                        !objective.IsCompleted)
                    {
                        UpdateQuestObjective(quest.Id, objective.Id);
                    }
                }
            }
        }

        // Event handlers
        private void OnMessageDecoded(string messageId)
        {
            // Update signal decode objectives
            foreach (var quest in _activeQuests.Values)
            {
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.DecodeSignal && 
                        objective.TargetId == messageId && 
                        !objective.IsCompleted)
                    {
                        UpdateQuestObjective(quest.Id, objective.Id);
                    }
                }
            }
        }

        private void OnLocationChanged(string locationId)
        {
            CheckLocationForQuests(locationId);
        }

        private void OnLocationDiscovered(string locationId)
        {
            // Update location visit objectives
            foreach (var quest in _activeQuests.Values)
            {
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.VisitLocation && 
                        objective.TargetId == locationId && 
                        !objective.IsCompleted)
                    {
                        UpdateQuestObjective(quest.Id, objective.Id);
                    }
                }
            }
        }

        private void OnInventoryChanged()
        {
            // Update collect item objectives
            foreach (var quest in _activeQuests.Values)
            {
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.CollectItem && !objective.IsCompleted)
                    {
                        // Check if the player has the required item
                        if (_inventorySystem.HasItem(objective.TargetId, objective.RequiredAmount))
                        {
                            // Set the current amount to the required amount
                            objective.CurrentAmount = objective.RequiredAmount;
                            objective.IsCompleted = true;
                            EmitSignal(SignalName.QuestObjectiveCompleted, quest.Id, objective.Id);

                            // Check if all objectives are completed
                            if (quest.Objectives.All(o => o.IsCompleted))
                            {
                                CompleteQuest(quest.Id);
                            }
                        }
                    }
                }
            }
        }

        private void OnItemUsed(string itemId)
        {
            // Update use item objectives
            foreach (var quest in _activeQuests.Values)
            {
                foreach (var objective in quest.Objectives)
                {
                    if (objective.Type == QuestObjectiveType.UseItem && 
                        objective.TargetId == itemId && 
                        !objective.IsCompleted)
                    {
                        UpdateQuestObjective(quest.Id, objective.Id);
                    }
                }
            }
        }
    }
}

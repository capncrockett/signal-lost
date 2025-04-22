using Godot;
using System.Collections.Generic;
using System.Linq;

namespace SignalLost
{
    [GlobalClass]
    public partial class QuestSystem : Node
    {
        // Quest categories
        public enum QuestCategory
        {
            Main,       // Main storyline quests
            Side,       // Optional side quests
            Exploration, // Exploration-focused quests
            Collection,  // Item collection quests
            Radio,      // Radio-related quests
            Tutorial    // Tutorial quests
        }

        // Quest data
        public class QuestData
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public List<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();
            public Dictionary<string, QuestReward> Rewards { get; set; } = new Dictionary<string, QuestReward>();
            public bool IsCompleted { get; set; } = false;
            public bool IsActive { get; set; } = false;
            public bool IsDiscovered { get; set; } = false;
            public bool IsFailed { get; set; } = false;
            public QuestCategory Category { get; set; } = QuestCategory.Side;
            public int Priority { get; set; } = 0; // Higher number = higher priority
            public List<string> PrerequisiteQuestIds { get; set; } = new List<string>(); // Quests that must be completed before this one becomes available
            public string LocationId { get; set; } // Location where this quest is available

            // Legacy support properties
            public string RewardItemId { get; set; } // Legacy support - use Rewards dictionary instead
            public int RewardQuantity { get; set; } = 1; // Legacy support - use Rewards dictionary instead
            public string PrerequisiteQuestId { get; set; } // Legacy support - use PrerequisiteQuestIds list instead
            public float TimeLimit { get; set; } = 0; // Time limit in seconds (0 = no limit)
            public float TimeRemaining { get; set; } = 0; // Time remaining for timed quests
            public bool HasTimedFailed { get; set; } = false; // Whether the quest has failed due to time running out
            public List<string> NextQuestIds { get; set; } = new List<string>(); // Quests that become available after this one is completed
            public List<QuestBranch> Branches { get; set; } = new List<QuestBranch>(); // Branching paths within the quest
            public string QuestGiverId { get; set; } // ID of the NPC or object that gives the quest
            public string QuestTurnInId { get; set; } // ID of the NPC or object to turn in the quest to
            public string QuestLogEntry { get; set; } // Entry in the quest log
            public Dictionary<string, string> QuestFlags { get; set; } = new Dictionary<string, string>(); // Custom flags for quest state
        }

        // Quest reward data
        public class QuestReward
        {
            public enum RewardType
            {
                Item,       // Item reward
                Experience, // Experience points
                Equipment,  // Special equipment
                Unlock,     // Unlock a feature or area
                Currency,   // In-game currency
                Skill,      // Skill points or new skill
                Custom      // Custom reward type
            }

            public string Id { get; set; }
            public string Description { get; set; }
            public RewardType Type { get; set; } = RewardType.Item;
            public string RewardId { get; set; } // ID of the item, equipment, etc.
            public int Quantity { get; set; } = 1;
            public bool IsDelivered { get; set; } = false;
            public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>(); // Additional data for custom rewards
        }

        // Quest branch data
        public class QuestBranch
        {
            public string Id { get; set; }
            public string Description { get; set; }
            public List<string> RequiredObjectiveIds { get; set; } = new List<string>(); // Objectives that must be completed to unlock this branch
            public List<string> UnlockedObjectiveIds { get; set; } = new List<string>(); // Objectives that are unlocked by this branch
            public List<string> NextQuestIds { get; set; } = new List<string>(); // Quests that are unlocked by this branch
            public Dictionary<string, QuestReward> BranchRewards { get; set; } = new Dictionary<string, QuestReward>(); // Rewards specific to this branch
            public bool IsUnlocked { get; set; } = false;
            public bool IsCompleted { get; set; } = false;
            public Dictionary<string, string> BranchFlags { get; set; } = new Dictionary<string, string>(); // Custom flags for branch state
        }

        // Quest objective data
        public class QuestObjective
        {
            public string Id { get; set; }
            public string Description { get; set; }
            public QuestObjectiveType Type { get; set; }
            public string TargetId { get; set; } // ID of the item, location, or signal to interact with
            public int RequiredAmount { get; set; } = 1;
            public int CurrentAmount { get; set; } = 0;
            public bool IsCompleted { get; set; } = false;
            public bool IsOptional { get; set; } = false;
            public bool IsHidden { get; set; } = false; // Hidden until a certain condition is met
            public string UnlockCondition { get; set; } // Condition that unlocks this objective
            public List<string> NextObjectiveIds { get; set; } = new List<string>(); // Objectives that are unlocked when this one is completed
            public string BranchId { get; set; } // ID of the branch this objective belongs to
            public Dictionary<string, string> ObjectiveFlags { get; set; } = new Dictionary<string, string>(); // Custom flags for objective state
        }

        // Quest objective types
        public enum QuestObjectiveType
        {
            CollectItem,     // Collect a specific item
            VisitLocation,   // Visit a specific location
            UseItem,         // Use a specific item
            DecodeSignal,    // Decode a radio signal
            TalkToNPC,       // Talk to a specific NPC
            DefeatEnemy,     // Defeat a specific enemy
            SolveRiddle,     // Solve a riddle or puzzle
            FindSignal,      // Find a radio signal
            ActivateDevice,  // Activate a device or mechanism
            EscapeArea,      // Escape from an area
            SurviveTime,     // Survive for a specific amount of time
            ReachDestination,// Reach a specific destination within a time limit
            ProtectTarget,   // Protect a target from enemies
            StealthMission,  // Complete a mission without being detected
            FindClue,        // Find a clue or evidence
            CraftItem,       // Craft a specific item
            RepairItem,      // Repair a specific item
            UpgradeEquipment,// Upgrade a piece of equipment
            CompleteQuest,   // Complete another quest
            Custom           // Custom objective type
        }

        // Dictionary of all quests in the game
        private Dictionary<string, QuestData> _questDatabase = new Dictionary<string, QuestData>();

        // Player's active and completed quests
        private Dictionary<string, QuestData> _activeQuests = new Dictionary<string, QuestData>();
        private Dictionary<string, QuestData> _completedQuests = new Dictionary<string, QuestData>();

        // References to other systems
        private GameState _gameState;
        private InventorySystem _inventorySystem;
        private GameProgressionManager _progressionManager;
        private MapSystem _mapSystem;

        // Signals
        [Signal]
        public delegate void QuestDiscoveredEventHandler(string questId);

        [Signal]
        public delegate void QuestActivatedEventHandler(string questId);

        [Signal]
        public delegate void QuestCompletedEventHandler(string questId);

        [Signal]
        public delegate void QuestFailedEventHandler(string questId, string reason);

        [Signal]
        public delegate void QuestObjectiveUpdatedEventHandler(string questId, string objectiveId, int currentAmount, int requiredAmount);

        [Signal]
        public delegate void QuestObjectiveCompletedEventHandler(string questId, string objectiveId);

        [Signal]
        public delegate void QuestObjectiveUnlockedEventHandler(string questId, string objectiveId);

        [Signal]
        public delegate void QuestBranchUnlockedEventHandler(string questId, string branchId);

        [Signal]
        public delegate void QuestBranchCompletedEventHandler(string questId, string branchId);

        [Signal]
        public delegate void QuestRewardDeliveredEventHandler(string questId, string rewardId);

        [Signal]
        public delegate void QuestTimerUpdatedEventHandler(string questId, float timeRemaining, float timeLimit);

        [Signal]
        public delegate void QuestLogUpdatedEventHandler();

        // Quest marker data
        public class QuestMarker
        {
            public string Id { get; set; }
            public string QuestId { get; set; }
            public string ObjectiveId { get; set; }
            public string LocationId { get; set; }
            public Vector2 Position { get; set; }
            public string MarkerType { get; set; } = "quest"; // quest, objective, turn_in, etc.
            public string Description { get; set; }
            public bool IsVisible { get; set; } = true;
        }

        // Dictionary of quest markers
        private Dictionary<string, QuestMarker> _questMarkers = new Dictionary<string, QuestMarker>();

        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to other systems
            _gameState = GetNode<GameState>("/root/GameState");
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");

            // Get reference to progression manager (may not exist yet during initialization)
            _progressionManager = GetNodeOrNull<GameProgressionManager>("/root/GameProgressionManager");

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

            // Connect to our own signals
            QuestActivated += OnQuestActivated;
            QuestCompleted += OnQuestCompleted;
            QuestFailed += OnQuestFailed;
            QuestObjectiveCompleted += OnQuestObjectiveCompleted;
            QuestObjectiveUnlocked += OnQuestObjectiveUnlocked;

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

        // Get all failed quests
        public Dictionary<string, QuestData> GetFailedQuests()
        {
            var failedQuests = new Dictionary<string, QuestData>();
            foreach (var quest in _questDatabase.Values)
            {
                if (quest.IsFailed)
                {
                    failedQuests[quest.Id] = quest;
                }
            }
            return failedQuests;
        }

        // Get quests by category
        public Dictionary<string, QuestData> GetQuestsByCategory(QuestCategory category)
        {
            var categoryQuests = new Dictionary<string, QuestData>();
            foreach (var quest in _questDatabase.Values)
            {
                if (quest.Category == category && quest.IsDiscovered)
                {
                    categoryQuests[quest.Id] = quest;
                }
            }
            return categoryQuests;
        }

        // Get active quests by category
        public Dictionary<string, QuestData> GetActiveQuestsByCategory(QuestCategory category)
        {
            var categoryQuests = new Dictionary<string, QuestData>();
            foreach (var quest in _activeQuests.Values)
            {
                if (quest.Category == category)
                {
                    categoryQuests[quest.Id] = quest;
                }
            }
            return categoryQuests;
        }

        // Get main storyline quests
        public Dictionary<string, QuestData> GetMainQuests()
        {
            return GetQuestsByCategory(QuestCategory.Main);
        }

        // Get active main storyline quests
        public Dictionary<string, QuestData> GetActiveMainQuests()
        {
            return GetActiveQuestsByCategory(QuestCategory.Main);
        }

        // Get side quests
        public Dictionary<string, QuestData> GetSideQuests()
        {
            return GetQuestsByCategory(QuestCategory.Side);
        }

        // Get active side quests
        public Dictionary<string, QuestData> GetActiveSideQuests()
        {
            return GetActiveQuestsByCategory(QuestCategory.Side);
        }

        // Get quests sorted by priority
        public List<QuestData> GetQuestsByPriority()
        {
            var quests = GetDiscoveredQuests().Values.ToList();
            quests.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Sort by priority (descending)
            return quests;
        }

        // Get active quests sorted by priority
        public List<QuestData> GetActiveQuestsByPriority()
        {
            var quests = _activeQuests.Values.ToList();
            quests.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // Sort by priority (descending)
            return quests;
        }

        // Check if a quest is completed
        public bool IsQuestCompleted(string questId)
        {
            return _completedQuests.ContainsKey(questId);
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

            // Check if all required objectives are completed
            foreach (var objective in quest.Objectives.Where(o => !o.IsOptional))
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
            if (quest.Rewards.Count > 0)
            {
                foreach (var reward in quest.Rewards.Values)
                {
                    DeliverReward(questId, reward);
                }
            }
            else if (!string.IsNullOrEmpty(quest.RewardItemId))
            {
                // Legacy reward system support
                _inventorySystem.AddItemToInventory(quest.RewardItemId, quest.RewardQuantity);
            }

            // Check if completing this quest unlocks any other quests
            foreach (var otherQuest in _questDatabase.Values)
            {
                if (otherQuest.PrerequisiteQuestIds.Contains(questId) && !otherQuest.IsDiscovered)
                {
                    // Check if all prerequisites are completed
                    bool allPrerequisitesCompleted = true;
                    foreach (var prereqId in otherQuest.PrerequisiteQuestIds)
                    {
                        if (!_completedQuests.ContainsKey(prereqId))
                        {
                            allPrerequisitesCompleted = false;
                            break;
                        }
                    }

                    if (allPrerequisitesCompleted)
                    {
                        DiscoverQuest(otherQuest.Id);
                    }
                }
                else if (otherQuest.PrerequisiteQuestId == questId && !otherQuest.IsDiscovered)
                {
                    // Legacy prerequisite system support
                    DiscoverQuest(otherQuest.Id);
                }
            }

            // Unlock next quests in the chain
            foreach (var nextQuestId in quest.NextQuestIds)
            {
                if (_questDatabase.ContainsKey(nextQuestId) && !_questDatabase[nextQuestId].IsDiscovered)
                {
                    DiscoverQuest(nextQuestId);
                }
            }

            EmitSignal(SignalName.QuestCompleted, questId);
            EmitSignal(SignalName.QuestLogUpdated);

            // Check if this quest completion should trigger progression advancement
            if (_progressionManager != null)
            {
                _progressionManager.CheckProgressionRequirements();
            }

            return true;
        }

        // Deliver a quest reward
        private void DeliverReward(string questId, QuestReward reward)
        {
            if (reward.IsDelivered)
            {
                return;
            }

            switch (reward.Type)
            {
                case QuestReward.RewardType.Item:
                    _inventorySystem.AddItemToInventory(reward.RewardId, reward.Quantity);
                    break;

                case QuestReward.RewardType.Equipment:
                    _inventorySystem.AddItemToInventory(reward.RewardId, reward.Quantity);
                    break;

                case QuestReward.RewardType.Experience:
                    if (_gameState != null)
                    {
                        _gameState.AddExperience(reward.Quantity);
                    }
                    break;

                case QuestReward.RewardType.Unlock:
                    if (_gameState != null)
                    {
                        _gameState.UnlockFeature(reward.RewardId);
                    }
                    break;

                case QuestReward.RewardType.Currency:
                    if (_gameState != null)
                    {
                        _gameState.AddCurrency(reward.Quantity);
                    }
                    break;

                case QuestReward.RewardType.Skill:
                    if (_gameState != null)
                    {
                        _gameState.UnlockSkill(reward.RewardId);
                    }
                    break;

                case QuestReward.RewardType.Custom:
                    // Handle custom reward types
                    GD.Print($"Delivering custom reward: {reward.Id} for quest {questId}");
                    break;
            }

            reward.IsDelivered = true;
            EmitSignal(SignalName.QuestRewardDelivered, questId, reward.Id);
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

            if (objective == null || objective.IsCompleted || objective.IsHidden)
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

                // Check if this objective unlocks any branches
                CheckBranchUnlocks(quest, objective.Id);

                // Check if this objective unlocks any other objectives
                UnlockNextObjectives(quest, objective.Id);

                // Check if all required objectives are completed
                if (quest.Objectives.Where(o => !o.IsOptional).All(o => o.IsCompleted))
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

        // Check if any branches should be unlocked
        private void CheckBranchUnlocks(QuestData quest, string completedObjectiveId)
        {
            foreach (var branch in quest.Branches)
            {
                if (!branch.IsUnlocked && branch.RequiredObjectiveIds.Contains(completedObjectiveId))
                {
                    // Check if all required objectives for this branch are completed
                    bool allRequiredCompleted = true;
                    foreach (var requiredObjectiveId in branch.RequiredObjectiveIds)
                    {
                        var objective = quest.Objectives.FirstOrDefault(o => o.Id == requiredObjectiveId);
                        if (objective == null || !objective.IsCompleted)
                        {
                            allRequiredCompleted = false;
                            break;
                        }
                    }

                    if (allRequiredCompleted)
                    {
                        // Unlock the branch
                        branch.IsUnlocked = true;

                        // Unlock objectives in this branch
                        foreach (var objectiveId in branch.UnlockedObjectiveIds)
                        {
                            var objective = quest.Objectives.FirstOrDefault(o => o.Id == objectiveId);
                            if (objective != null && objective.IsHidden)
                            {
                                objective.IsHidden = false;
                                EmitSignal(SignalName.QuestObjectiveUnlocked, quest.Id, objectiveId);
                            }
                        }

                        EmitSignal(SignalName.QuestBranchUnlocked, quest.Id, branch.Id);
                        EmitSignal(SignalName.QuestLogUpdated);
                    }
                }
            }
        }

        // Unlock objectives that should be unlocked after completing an objective
        private void UnlockNextObjectives(QuestData quest, string completedObjectiveId)
        {
            var completedObjective = quest.Objectives.FirstOrDefault(o => o.Id == completedObjectiveId);
            if (completedObjective == null || completedObjective.NextObjectiveIds.Count == 0)
            {
                return;
            }

            foreach (var nextObjectiveId in completedObjective.NextObjectiveIds)
            {
                var nextObjective = quest.Objectives.FirstOrDefault(o => o.Id == nextObjectiveId);
                if (nextObjective != null && nextObjective.IsHidden)
                {
                    nextObjective.IsHidden = false;
                    EmitSignal(SignalName.QuestObjectiveUnlocked, quest.Id, nextObjectiveId);
                    EmitSignal(SignalName.QuestLogUpdated);
                }
            }
        }

        // Fail a quest
        public bool FailQuest(string questId, string reason = "")
        {
            if (!_activeQuests.ContainsKey(questId))
            {
                GD.PrintErr($"QuestSystem: Quest {questId} not found in active quests");
                return false;
            }

            var quest = _activeQuests[questId];

            // Fail the quest
            quest.IsFailed = true;
            quest.IsActive = false;
            _activeQuests.Remove(questId);

            EmitSignal(SignalName.QuestFailed, questId, reason);
            EmitSignal(SignalName.QuestLogUpdated);

            return true;
        }

        // Process for timed quests
        public override void _Process(double delta)
        {
            // Update timed quests
            foreach (var quest in _activeQuests.Values.ToList())
            {
                if (quest.TimeLimit > 0)
                {
                    quest.TimeRemaining -= (float)delta;

                    // Check if time has run out
                    if (quest.TimeRemaining <= 0)
                    {
                        quest.TimeRemaining = 0;
                        quest.HasTimedFailed = true;
                        FailQuest(quest.Id, "Time limit expired");
                    }
                    else if (quest.TimeRemaining <= quest.TimeLimit * 0.25f) // Warning at 25% time remaining
                    {
                        // Emit timer update signal for UI to show warning
                        EmitSignal(SignalName.QuestTimerUpdated, quest.Id, quest.TimeRemaining, quest.TimeLimit);
                    }
                }
            }
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

        // Quest marker methods

        // Add a quest marker
        public void AddQuestMarker(QuestMarker marker)
        {
            _questMarkers[marker.Id] = marker;

            // Add the marker to the map
            if (_mapSystem != null && marker.IsVisible)
            {
                _mapSystem.AddQuestMarker(marker.Id, marker.LocationId, marker.Position, marker.MarkerType, marker.Description);
            }
        }

        // Remove a quest marker
        public void RemoveQuestMarker(string markerId)
        {
            if (_questMarkers.ContainsKey(markerId))
            {
                _questMarkers.Remove(markerId);

                // Remove the marker from the map
                if (_mapSystem != null)
                {
                    _mapSystem.RemoveQuestMarker(markerId);
                }
            }
        }

        // Show a quest marker
        public void ShowQuestMarker(string markerId)
        {
            if (_questMarkers.ContainsKey(markerId))
            {
                var marker = _questMarkers[markerId];
                marker.IsVisible = true;

                // Show the marker on the map
                if (_mapSystem != null)
                {
                    _mapSystem.ShowQuestMarker(markerId);
                }
            }
        }

        // Hide a quest marker
        public void HideQuestMarker(string markerId)
        {
            if (_questMarkers.ContainsKey(markerId))
            {
                var marker = _questMarkers[markerId];
                marker.IsVisible = false;

                // Hide the marker on the map
                if (_mapSystem != null)
                {
                    _mapSystem.HideQuestMarker(markerId);
                }
            }
        }

        // Get all quest markers
        public Dictionary<string, QuestMarker> GetQuestMarkers()
        {
            return _questMarkers;
        }

        // Get quest markers for a specific quest
        public Dictionary<string, QuestMarker> GetQuestMarkersForQuest(string questId)
        {
            var markers = new Dictionary<string, QuestMarker>();
            foreach (var marker in _questMarkers.Values)
            {
                if (marker.QuestId == questId)
                {
                    markers[marker.Id] = marker;
                }
            }
            return markers;
        }

        // Get quest markers for a specific objective
        public Dictionary<string, QuestMarker> GetQuestMarkersForObjective(string questId, string objectiveId)
        {
            var markers = new Dictionary<string, QuestMarker>();
            foreach (var marker in _questMarkers.Values)
            {
                if (marker.QuestId == questId && marker.ObjectiveId == objectiveId)
                {
                    markers[marker.Id] = marker;
                }
            }
            return markers;
        }

        // Event handlers for our own signals

        private void OnQuestActivated(string questId)
        {
            var quest = _activeQuests[questId];

            // Add quest markers for the quest
            if (quest.QuestGiverId != null)
            {
                // Add a marker for the quest giver
                var marker = new QuestMarker
                {
                    Id = $"{questId}_giver",
                    QuestId = questId,
                    LocationId = quest.LocationId,
                    MarkerType = "quest_giver",
                    Description = $"Quest Giver: {quest.Title}"
                };

                AddQuestMarker(marker);
            }

            // Add markers for objectives
            foreach (var objective in quest.Objectives)
            {
                if (!objective.IsHidden && objective.Type == QuestObjectiveType.VisitLocation)
                {
                    // Add a marker for the location objective
                    var marker = new QuestMarker
                    {
                        Id = $"{questId}_{objective.Id}",
                        QuestId = questId,
                        ObjectiveId = objective.Id,
                        LocationId = objective.TargetId,
                        MarkerType = "objective",
                        Description = objective.Description
                    };

                    AddQuestMarker(marker);
                }
            }
        }

        private void OnQuestCompleted(string questId)
        {
            // Remove all markers for this quest
            var markers = GetQuestMarkersForQuest(questId);
            foreach (var markerId in markers.Keys)
            {
                RemoveQuestMarker(markerId);
            }

            // Add a marker for the quest turn-in if needed
            var quest = _completedQuests[questId];
            if (quest.QuestTurnInId != null)
            {
                var marker = new QuestMarker
                {
                    Id = $"{questId}_turn_in",
                    QuestId = questId,
                    LocationId = quest.LocationId,
                    MarkerType = "quest_turn_in",
                    Description = $"Turn in: {quest.Title}"
                };

                AddQuestMarker(marker);
            }
        }

        private void OnQuestFailed(string questId, string reason)
        {
            // Remove all markers for this quest
            var markers = GetQuestMarkersForQuest(questId);
            foreach (var markerId in markers.Keys)
            {
                RemoveQuestMarker(markerId);
            }
        }

        private void OnQuestObjectiveCompleted(string questId, string objectiveId)
        {
            // Remove markers for this objective
            var markers = GetQuestMarkersForObjective(questId, objectiveId);
            foreach (var markerId in markers.Keys)
            {
                RemoveQuestMarker(markerId);
            }
        }

        private void OnQuestObjectiveUnlocked(string questId, string objectiveId)
        {
            var quest = _activeQuests[questId];
            var objective = quest.Objectives.FirstOrDefault(o => o.Id == objectiveId);

            if (objective != null && objective.Type == QuestObjectiveType.VisitLocation)
            {
                // Add a marker for the location objective
                var marker = new QuestMarker
                {
                    Id = $"{questId}_{objectiveId}",
                    QuestId = questId,
                    ObjectiveId = objectiveId,
                    LocationId = objective.TargetId,
                    MarkerType = "objective",
                    Description = objective.Description
                };

                AddQuestMarker(marker);
            }
        }

        // Set GameState reference (for testing)
        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
        }

        // Set InventorySystem reference (for testing)
        public void SetInventorySystem(InventorySystem inventorySystem)
        {
            _inventorySystem = inventorySystem;
        }

        // Set MapSystem reference (for testing)
        public void SetMapSystem(MapSystem mapSystem)
        {
            _mapSystem = mapSystem;
        }
    }
}

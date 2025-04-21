using Godot;
using System.Collections.Generic;

namespace SignalLost.Progression
{
    /// <summary>
    /// Manages the game progression system, including stages, progress tracking, and unlockable content.
    /// </summary>
    [GlobalClass]
    public partial class GameProgressionSystem : Node
    {
        // Game stages
        public enum GameStage
        {
            Beginning,
            Exploration,
            Discovery,
            Revelation,
            Conclusion
        }
        
        // Current game stage
        private GameStage _currentStage = GameStage.Beginning;
        
        // Progress within the current stage (0-100)
        private int _stageProgress = 0;
        
        // Stage descriptions
        private Dictionary<GameStage, string> _stageDescriptions = new Dictionary<GameStage, string>
        {
            { GameStage.Beginning, "You've just arrived at the emergency bunker. Your radio is damaged and needs repair." },
            { GameStage.Exploration, "With your radio working, you can now explore the surrounding area and look for other survivors." },
            { GameStage.Discovery, "You've made contact with other survivors and discovered clues about what happened." },
            { GameStage.Revelation, "The truth about the incident is starting to become clear as you piece together the evidence." },
            { GameStage.Conclusion, "You now know what happened and must make a final decision about what to do next." }
        };
        
        // Stage objectives
        private Dictionary<GameStage, string> _stageObjectives = new Dictionary<GameStage, string>
        {
            { GameStage.Beginning, "Repair your radio by finding the necessary components." },
            { GameStage.Exploration, "Explore the area and make contact with other survivors." },
            { GameStage.Discovery, "Investigate the research facility and military outpost." },
            { GameStage.Revelation, "Decode the mysterious signal and find the source." },
            { GameStage.Conclusion, "Reach the mysterious tower and confront the truth." }
        };
        
        // Stage milestones - key events that mark progress within a stage
        private Dictionary<GameStage, List<StageMilestone>> _stageMilestones = new Dictionary<GameStage, List<StageMilestone>>();
        
        // Signals
        [Signal]
        public delegate void StageChangedEventHandler(int stageIndex);
        
        [Signal]
        public delegate void ProgressChangedEventHandler(int progress);
        
        [Signal]
        public delegate void MilestoneCompletedEventHandler(string milestoneId);
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Initialize stage milestones
            InitializeStageMilestones();
            
            // Load saved progress if available
            LoadProgress();
            
            // Emit initial signals
            EmitSignal(SignalName.StageChanged, (int)_currentStage);
            EmitSignal(SignalName.ProgressChanged, _stageProgress);
        }
        
        // Initialize stage milestones
        private void InitializeStageMilestones()
        {
            // Beginning stage milestones
            _stageMilestones[GameStage.Beginning] = new List<StageMilestone>
            {
                new StageMilestone
                {
                    Id = "find_radio_parts",
                    Description = "Find the necessary parts to repair the radio",
                    ProgressValue = 50,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "repair_radio",
                    Description = "Repair the radio",
                    ProgressValue = 50,
                    IsCompleted = false
                }
            };
            
            // Exploration stage milestones
            _stageMilestones[GameStage.Exploration] = new List<StageMilestone>
            {
                new StageMilestone
                {
                    Id = "discover_emergency_broadcast",
                    Description = "Discover the emergency broadcast signal",
                    ProgressValue = 20,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "discover_survivor_signal",
                    Description = "Discover the survivor signal",
                    ProgressValue = 30,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "visit_old_mill",
                    Description = "Visit the old mill",
                    ProgressValue = 50,
                    IsCompleted = false
                }
            };
            
            // Discovery stage milestones
            _stageMilestones[GameStage.Discovery] = new List<StageMilestone>
            {
                new StageMilestone
                {
                    Id = "find_research_keycard",
                    Description = "Find the research facility keycard",
                    ProgressValue = 25,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "explore_research_facility",
                    Description = "Explore the research facility",
                    ProgressValue = 25,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "find_military_badge",
                    Description = "Find the military badge",
                    ProgressValue = 25,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "explore_military_outpost",
                    Description = "Explore the military outpost",
                    ProgressValue = 25,
                    IsCompleted = false
                }
            };
            
            // Revelation stage milestones
            _stageMilestones[GameStage.Revelation] = new List<StageMilestone>
            {
                new StageMilestone
                {
                    Id = "discover_mysterious_signal",
                    Description = "Discover the mysterious signal",
                    ProgressValue = 25,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "decode_mysterious_signal",
                    Description = "Decode the mysterious signal",
                    ProgressValue = 25,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "find_tower_key",
                    Description = "Find the tower key",
                    ProgressValue = 25,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "locate_mysterious_tower",
                    Description = "Locate the mysterious tower",
                    ProgressValue = 25,
                    IsCompleted = false
                }
            };
            
            // Conclusion stage milestones
            _stageMilestones[GameStage.Conclusion] = new List<StageMilestone>
            {
                new StageMilestone
                {
                    Id = "enter_mysterious_tower",
                    Description = "Enter the mysterious tower",
                    ProgressValue = 25,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "discover_truth",
                    Description = "Discover the truth",
                    ProgressValue = 25,
                    IsCompleted = false
                },
                new StageMilestone
                {
                    Id = "make_final_decision",
                    Description = "Make the final decision",
                    ProgressValue = 50,
                    IsCompleted = false
                }
            };
        }
        
        // Load saved progress
        private void LoadProgress()
        {
            // Get the game state
            var gameState = GetNode<GameState>("/root/GameState");
            
            if (gameState == null)
            {
                GD.PrintErr("GameProgressionSystem: GameState not found");
                return;
            }
            
            // Load stage and progress from game state
            _currentStage = (GameStage)gameState.GameProgress;
            _stageProgress = gameState.StageProgress;
            
            // Load milestone completion status
            foreach (var stage in _stageMilestones.Keys)
            {
                foreach (var milestone in _stageMilestones[stage])
                {
                    milestone.IsCompleted = gameState.IsMilestoneCompleted(milestone.Id);
                }
            }
        }
        
        // Save progress
        private void SaveProgress()
        {
            // Get the game state
            var gameState = GetNode<GameState>("/root/GameState");
            
            if (gameState == null)
            {
                GD.PrintErr("GameProgressionSystem: GameState not found");
                return;
            }
            
            // Save stage and progress to game state
            gameState.GameProgress = (int)_currentStage;
            gameState.StageProgress = _stageProgress;
            
            // Save milestone completion status
            foreach (var stage in _stageMilestones.Keys)
            {
                foreach (var milestone in _stageMilestones[stage])
                {
                    if (milestone.IsCompleted)
                    {
                        gameState.SetMilestoneCompleted(milestone.Id);
                    }
                }
            }
        }
        
        // Get current stage
        public GameStage GetCurrentStage()
        {
            return _currentStage;
        }
        
        // Get stage description
        public string GetStageDescription()
        {
            return _stageDescriptions[_currentStage];
        }
        
        // Get stage objective
        public string GetStageObjective()
        {
            return _stageObjectives[_currentStage];
        }
        
        // Get stage progress
        public int GetStageProgress()
        {
            return _stageProgress;
        }
        
        // Get stage milestones
        public List<StageMilestone> GetStageMilestones(GameStage stage)
        {
            if (_stageMilestones.ContainsKey(stage))
            {
                return _stageMilestones[stage];
            }
            
            return new List<StageMilestone>();
        }
        
        // Get current stage milestones
        public List<StageMilestone> GetCurrentStageMilestones()
        {
            return GetStageMilestones(_currentStage);
        }
        
        // Advance to next stage
        public void AdvanceStage()
        {
            if (_currentStage < GameStage.Conclusion)
            {
                _currentStage++;
                _stageProgress = 0;
                EmitSignal(SignalName.StageChanged, (int)_currentStage);
                EmitSignal(SignalName.ProgressChanged, _stageProgress);
                
                // Save progress
                SaveProgress();
                
                GD.Print($"Advanced to stage: {_currentStage}");
            }
        }
        
        // Update stage progress
        public void UpdateProgress(int progress)
        {
            _stageProgress = Godot.Mathf.Clamp(progress, 0, 100);
            EmitSignal(SignalName.ProgressChanged, _stageProgress);
            
            // Save progress
            SaveProgress();
            
            GD.Print($"Updated progress: {_stageProgress}%");
            
            // Automatically advance stage if progress reaches 100
            if (_stageProgress >= 100)
            {
                AdvanceStage();
            }
        }
        
        // Increment progress by a specific amount
        public void IncrementProgress(int amount)
        {
            UpdateProgress(_stageProgress + amount);
        }
        
        // Complete a milestone
        public void CompleteMilestone(string milestoneId)
        {
            // Find the milestone
            foreach (var stage in _stageMilestones.Keys)
            {
                foreach (var milestone in _stageMilestones[stage])
                {
                    if (milestone.Id == milestoneId && !milestone.IsCompleted)
                    {
                        // Mark as completed
                        milestone.IsCompleted = true;
                        
                        // Emit signal
                        EmitSignal(SignalName.MilestoneCompleted, milestoneId);
                        
                        // If this is a milestone for the current stage, update progress
                        if (stage == _currentStage)
                        {
                            IncrementProgress(milestone.ProgressValue);
                        }
                        
                        // Save progress
                        SaveProgress();
                        
                        GD.Print($"Completed milestone: {milestoneId}");
                        
                        return;
                    }
                }
            }
        }
        
        // Check if a milestone is completed
        public bool IsMilestoneCompleted(string milestoneId)
        {
            // Find the milestone
            foreach (var stage in _stageMilestones.Keys)
            {
                foreach (var milestone in _stageMilestones[stage])
                {
                    if (milestone.Id == milestoneId)
                    {
                        return milestone.IsCompleted;
                    }
                }
            }
            
            return false;
        }
        
        // Check if content is unlocked based on game progression
        public bool IsContentUnlocked(string contentId)
        {
            // Define unlock conditions for different content
            switch (contentId)
            {
                case "research_facility":
                    return _currentStage >= GameStage.Exploration && _stageProgress >= 50;
                    
                case "military_outpost":
                    return _currentStage >= GameStage.Discovery && _stageProgress >= 25;
                    
                case "mysterious_tower":
                    return _currentStage >= GameStage.Revelation && _stageProgress >= 75;
                    
                case "advanced_radio":
                    return _currentStage >= GameStage.Discovery && _stageProgress >= 50;
                    
                case "strange_crystal":
                    return _currentStage >= GameStage.Revelation && _stageProgress >= 25;
                    
                default:
                    return false;
            }
        }
    }
    
    /// <summary>
    /// Represents a milestone within a game stage.
    /// </summary>
    public class StageMilestone
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public int ProgressValue { get; set; }
        public bool IsCompleted { get; set; }
    }
}

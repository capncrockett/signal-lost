using Godot;
using System;
using System.Collections.Generic;

namespace SignalLost
{
    /// <summary>
    /// Manages game progression and story advancement.
    /// </summary>
    [GlobalClass]
    public partial class GameProgressionManager : Node
    {
        // Singleton instance
        private static GameProgressionManager _instance;

        // References to other systems
        private GameState _gameState;
        private QuestSystem _questSystem;
        private MapSystem _mapSystem;
        private InventorySystem _inventorySystem;
        private MessageManager _messageManager;

        // Progression stages
        public enum ProgressionStage
        {
            Beginning = 0,        // Starting the game
            RadioRepair = 1,      // Fixed the radio
            FirstSignal = 2,      // Found the first signal
            ForestExploration = 3, // Explored the forest
            TownDiscovery = 4,    // Discovered the town
            SurvivorContact = 5,  // Made contact with survivors
            FactoryAccess = 6,    // Gained access to the factory
            Endgame = 7           // Final stage
        }

        // Current progression stage
        private ProgressionStage _currentStage = ProgressionStage.Beginning;

        // Requirements for advancing to the next stage
        private Dictionary<ProgressionStage, Func<bool>> _progressionRequirements;

        // Events that trigger when a stage is reached
        private Dictionary<ProgressionStage, Action> _progressionEvents;

        // Signals
        [Signal]
        public delegate void ProgressionAdvancedEventHandler(int newStage, string stageName);

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            _instance = this;

            // Get references to other systems
            _gameState = GetNode<GameState>("/root/GameState");
            _questSystem = GetNode<QuestSystem>("/root/QuestSystem");
            _mapSystem = GetNode<MapSystem>("/root/MapSystem");
            _inventorySystem = GetNode<InventorySystem>("/root/InventorySystem");
            _messageManager = GetNode<MessageManager>("/root/MessageManager");

            // Initialize progression requirements
            InitializeProgressionRequirements();

            // Initialize progression events
            InitializeProgressionEvents();

            // Set initial stage based on GameState
            _currentStage = (ProgressionStage)_gameState.GameProgress;

            // Run events for the current stage if needed
            if (_currentStage > ProgressionStage.Beginning)
            {
                RunEventsUpToCurrentStage();
            }

            GD.Print($"GameProgressionManager: Initialized at stage {_currentStage}");
        }

        /// <summary>
        /// Gets the singleton instance of the GameProgressionManager.
        /// </summary>
        public static GameProgressionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameProgressionManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the current progression stage.
        /// </summary>
        public ProgressionStage CurrentStage
        {
            get { return _currentStage; }
        }

        /// <summary>
        /// Gets the name of the current progression stage.
        /// </summary>
        public string CurrentStageName
        {
            get { return _currentStage.ToString(); }
        }

        /// <summary>
        /// Initializes the requirements for advancing to each progression stage.
        /// </summary>
        private void InitializeProgressionRequirements()
        {
            _progressionRequirements = new Dictionary<ProgressionStage, Func<bool>>
            {
                // Requirements for advancing from Beginning to RadioRepair
                [ProgressionStage.Beginning] = () =>
                    _questSystem.IsQuestCompleted("quest_radio_repair"),

                // Requirements for advancing from RadioRepair to FirstSignal
                [ProgressionStage.RadioRepair] = () =>
                    _gameState.DiscoveredFrequencies.Count > 0,

                // Requirements for advancing from FirstSignal to ForestExploration
                [ProgressionStage.FirstSignal] = () =>
                    _questSystem.IsQuestCompleted("quest_explore_forest"),

                // Requirements for advancing from ForestExploration to TownDiscovery
                [ProgressionStage.ForestExploration] = () =>
                    _mapSystem.IsLocationDiscovered("town"),

                // Requirements for advancing from TownDiscovery to SurvivorContact
                [ProgressionStage.TownDiscovery] = () =>
                    _questSystem.IsQuestCompleted("quest_survivor_message"),

                // Requirements for advancing from SurvivorContact to FactoryAccess
                [ProgressionStage.SurvivorContact] = () =>
                    _mapSystem.IsLocationDiscovered("factory") &&
                    _inventorySystem.HasItem("factory_key"),

                // Requirements for advancing from FactoryAccess to Endgame
                [ProgressionStage.FactoryAccess] = () =>
                    _questSystem.IsQuestCompleted("quest_final_transmission")
            };
        }

        /// <summary>
        /// Initializes the events that trigger when each progression stage is reached.
        /// </summary>
        private void InitializeProgressionEvents()
        {
            _progressionEvents = new Dictionary<ProgressionStage, Action>
            {
                // Events for RadioRepair stage
                [ProgressionStage.RadioRepair] = () =>
                {
                    // Add new quest for finding signals
                    _questSystem.DiscoverQuest("quest_find_signal");
                    _questSystem.ActivateQuest("quest_find_signal");

                    // Add message about radio repair
                    var radioMessage = new GameState.MessageData
                    {
                        Title = "Radio Repaired",
                        Content = "You've successfully repaired the radio. Now you can search for signals.",
                        Decoded = true
                    };
                    _gameState.Messages["msg_radio_repaired"] = radioMessage;
                    _messageManager.ShowMessage("msg_radio_repaired");
                },

                // Events for FirstSignal stage
                [ProgressionStage.FirstSignal] = () =>
                {
                    // Discover forest location
                    _mapSystem.DiscoverLocation("forest");

                    // Add quest for exploring the forest
                    _questSystem.DiscoverQuest("quest_explore_forest");
                    _questSystem.ActivateQuest("quest_explore_forest");

                    // Add message about the forest
                    var forestMessage = new GameState.MessageData
                    {
                        Title = "Forest Signal",
                        Content = "You've detected a signal coming from the forest. You should investigate.",
                        Decoded = true
                    };
                    _gameState.Messages["msg_forest_signal"] = forestMessage;
                    _messageManager.ShowMessage("msg_forest_signal");
                },

                // Events for ForestExploration stage
                [ProgressionStage.ForestExploration] = () =>
                {
                    // Discover cabin and lake locations
                    _mapSystem.DiscoverLocation("cabin");
                    _mapSystem.DiscoverLocation("lake");

                    // Discover road location
                    _mapSystem.DiscoverLocation("road");

                    // Add quest for finding the town
                    _questSystem.DiscoverQuest("quest_find_town");
                    _questSystem.ActivateQuest("quest_find_town");

                    // Add message about the town
                    var townMessage = new GameState.MessageData
                    {
                        Title = "Town Rumors",
                        Content = "You've heard rumors of a town nearby. It might be worth investigating.",
                        Decoded = true
                    };
                    _gameState.Messages["msg_town_rumors"] = townMessage;
                    _messageManager.ShowMessage("msg_town_rumors");
                },

                // Events for TownDiscovery stage
                [ProgressionStage.TownDiscovery] = () =>
                {
                    // Add quest for decoding survivor message
                    _questSystem.DiscoverQuest("quest_decode_signal");
                    _questSystem.ActivateQuest("quest_decode_signal");

                    // Add message about the town
                    var townDiscoveryMessage = new GameState.MessageData
                    {
                        Title = "Town Discovery",
                        Content = "You've found the abandoned town. It seems like there might be survivors nearby.",
                        Decoded = true
                    };
                    _gameState.Messages["msg_town_discovery"] = townDiscoveryMessage;
                    _messageManager.ShowMessage("msg_town_discovery");
                },

                // Events for SurvivorContact stage
                [ProgressionStage.SurvivorContact] = () =>
                {
                    // Discover factory location
                    _mapSystem.DiscoverLocation("factory");

                    // Add quest for finding the factory key
                    _questSystem.DiscoverQuest("quest_factory_key");
                    _questSystem.ActivateQuest("quest_factory_key");

                    // Add message about the survivors
                    var survivorMessage = new GameState.MessageData
                    {
                        Title = "Survivor Contact",
                        Content = "You've made contact with survivors. They're hiding in the old factory, but you need a key to get in.",
                        Decoded = true
                    };
                    _gameState.Messages["msg_survivor_contact"] = survivorMessage;
                    _messageManager.ShowMessage("msg_survivor_contact");
                },

                // Events for FactoryAccess stage
                [ProgressionStage.FactoryAccess] = () =>
                {
                    // Add final quest
                    _questSystem.DiscoverQuest("quest_final_transmission");
                    _questSystem.ActivateQuest("quest_final_transmission");

                    // Add message about the factory
                    var factoryMessage = new GameState.MessageData
                    {
                        Title = "Factory Access",
                        Content = "You've gained access to the factory. The survivors have a plan to escape the area.",
                        Decoded = true
                    };
                    _gameState.Messages["msg_factory_access"] = factoryMessage;
                    _messageManager.ShowMessage("msg_factory_access");
                },

                // Events for Endgame stage
                [ProgressionStage.Endgame] = () =>
                {
                    // Add message about the end
                    var finalMessage = new GameState.MessageData
                    {
                        Title = "Final Transmission",
                        Content = "You've completed the final transmission. The survivors are safe, and you've completed your mission.",
                        Decoded = true
                    };
                    _gameState.Messages["msg_final_transmission"] = finalMessage;
                    _messageManager.ShowMessage("msg_final_transmission");

                    // Show end game screen or credits
                    GD.Print("GameProgressionManager: Game completed!");
                }
            };
        }

        /// <summary>
        /// Runs all events up to the current stage.
        /// </summary>
        private void RunEventsUpToCurrentStage()
        {
            for (int i = 0; i <= (int)_currentStage; i++)
            {
                ProgressionStage stage = (ProgressionStage)i;
                if (_progressionEvents.ContainsKey(stage))
                {
                    _progressionEvents[stage].Invoke();
                }
            }
        }

        /// <summary>
        /// Checks if the requirements for advancing to the next stage are met.
        /// </summary>
        public void CheckProgressionRequirements()
        {
            // Skip if we're already at the final stage
            if (_currentStage == ProgressionStage.Endgame)
            {
                return;
            }

            // Check if the requirements for the current stage are met
            if (_progressionRequirements.ContainsKey(_currentStage) &&
                _progressionRequirements[_currentStage].Invoke())
            {
                // Advance to the next stage
                AdvanceProgression();
            }
        }

        /// <summary>
        /// Advances the progression to the next stage.
        /// </summary>
        public void AdvanceProgression()
        {
            // Skip if we're already at the final stage
            if (_currentStage == ProgressionStage.Endgame)
            {
                return;
            }

            // Advance to the next stage
            _currentStage = (ProgressionStage)((int)_currentStage + 1);

            // Update GameState
            _gameState.SetGameProgress((int)_currentStage);

            // Run events for the new stage
            if (_progressionEvents.ContainsKey(_currentStage))
            {
                _progressionEvents[_currentStage].Invoke();
            }

            // Emit signal
            EmitSignal(SignalName.ProgressionAdvanced, (int)_currentStage, _currentStage.ToString());

            GD.Print($"GameProgressionManager: Advanced to stage {_currentStage}");

            // Check if we can advance further
            CheckProgressionRequirements();
        }

        /// <summary>
        /// Sets the progression to a specific stage (for debugging or save loading).
        /// </summary>
        /// <param name="stage">The stage to set</param>
        public void SetProgression(ProgressionStage stage)
        {
            // Skip if we're already at this stage
            if (_currentStage == stage)
            {
                return;
            }

            // Set the new stage
            _currentStage = stage;

            // Update GameState
            _gameState.SetGameProgress((int)_currentStage);

            // Run events up to the new stage
            RunEventsUpToCurrentStage();

            // Emit signal
            EmitSignal(SignalName.ProgressionAdvanced, (int)_currentStage, _currentStage.ToString());

            GD.Print($"GameProgressionManager: Set progression to stage {_currentStage}");
        }

        /// <summary>
        /// Gets a description of the current progression stage.
        /// </summary>
        public string GetCurrentStageDescription()
        {
            switch (_currentStage)
            {
                case ProgressionStage.Beginning:
                    return "You've just arrived at the emergency bunker. Your radio is damaged and needs repair.";

                case ProgressionStage.RadioRepair:
                    return "You've repaired your radio. Now you can search for signals and try to make contact with other survivors.";

                case ProgressionStage.FirstSignal:
                    return "You've found your first signal. It seems to be coming from the forest. You should investigate.";

                case ProgressionStage.ForestExploration:
                    return "You've explored the forest and found some useful items. There are rumors of a town nearby.";

                case ProgressionStage.TownDiscovery:
                    return "You've discovered the abandoned town. There might be survivors hiding somewhere nearby.";

                case ProgressionStage.SurvivorContact:
                    return "You've made contact with survivors. They're hiding in the old factory, but you need a key to get in.";

                case ProgressionStage.FactoryAccess:
                    return "You've gained access to the factory. The survivors have a plan to escape the area.";

                case ProgressionStage.Endgame:
                    return "You've completed your mission. The survivors are safe, and you've found a way out of the area.";

                default:
                    return "Unknown stage";
            }
        }

        /// <summary>
        /// Gets the next objective based on the current progression stage.
        /// </summary>
        public string GetNextObjective()
        {
            switch (_currentStage)
            {
                case ProgressionStage.Beginning:
                    return "Repair your radio by finding the necessary components.";

                case ProgressionStage.RadioRepair:
                    return "Find and tune into a radio signal.";

                case ProgressionStage.FirstSignal:
                    return "Explore the forest to find signs of other survivors.";

                case ProgressionStage.ForestExploration:
                    return "Find the abandoned town.";

                case ProgressionStage.TownDiscovery:
                    return "Decode the survivor's message and locate them.";

                case ProgressionStage.SurvivorContact:
                    return "Find the factory key to access the survivor's hideout.";

                case ProgressionStage.FactoryAccess:
                    return "Help the survivors complete their final transmission.";

                case ProgressionStage.Endgame:
                    return "You've completed all objectives. Congratulations!";

                default:
                    return "Unknown objective";
            }
        }

        /// <summary>
        /// Called every frame to check for progression updates.
        /// </summary>
        /// <param name="delta">Time since the last frame</param>
        public override void _Process(double delta)
        {
            // Periodically check if we can advance progression
            CheckProgressionRequirements();
        }

        /// <summary>
        /// Set GameState reference (for testing)
        /// </summary>
        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
        }

        /// <summary>
        /// Set QuestSystem reference (for testing)
        /// </summary>
        public void SetQuestSystem(QuestSystem questSystem)
        {
            _questSystem = questSystem;
        }

        /// <summary>
        /// Set MapSystem reference (for testing)
        /// </summary>
        public void SetMapSystem(MapSystem mapSystem)
        {
            _mapSystem = mapSystem;
        }

        /// <summary>
        /// Set InventorySystem reference (for testing)
        /// </summary>
        public void SetInventorySystem(InventorySystem inventorySystem)
        {
            _inventorySystem = inventorySystem;
        }

        /// <summary>
        /// Set MessageManager reference (for testing)
        /// </summary>
        public void SetMessageManager(MessageManager messageManager)
        {
            _messageManager = messageManager;
        }
    }
}

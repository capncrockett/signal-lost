using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SignalLost
{
    [GlobalClass]
    public partial class GameState : Node
    {
        // Game state variables
        public float CurrentFrequency { get; private set; } = 90.0f;
        public bool IsRadioOn { get; private set; } = false;
        public List<float> DiscoveredFrequencies { get; private set; } = new List<float>();
        public string CurrentLocation { get; set; } = "bunker";
        public List<string> Inventory { get; private set; } = new List<string>();
        public int GameProgress { get; set; } = 0;
        public int StageProgress { get; set; } = 0;

        // Environmental effects on signal reception
        private Dictionary<string, float> _signalInterferences = new Dictionary<string, float>();
        public float TotalSignalInterference { get; private set; } = 0.0f;
        public bool IsDaytime { get; private set; } = true;
        public float TimeOfDay { get; private set; } = 12.0f; // 0-24 hour format

        // Dictionary to store completed milestones
        private HashSet<string> _completedMilestones = new HashSet<string>();

        // Signal data
        public class SignalData
        {
            public string Id { get; set; }
            public float Frequency { get; set; }
            public string MessageId { get; set; }
            public bool IsStatic { get; set; }
            public float Bandwidth { get; set; }  // How close you need to be to the frequency to detect it
        }

        public List<SignalData> Signals { get; private set; } = new List<SignalData>
        {
            new SignalData
            {
                Id = "signal_1",
                Frequency = 91.5f,
                MessageId = "msg_001",
                IsStatic = true,
                Bandwidth = 0.3f
            },
            new SignalData
            {
                Id = "signal_2",
                Frequency = 95.7f,
                MessageId = "msg_002",
                IsStatic = false,
                Bandwidth = 0.2f
            },
            new SignalData
            {
                Id = "signal_3",
                Frequency = 103.2f,
                MessageId = "msg_003",
                IsStatic = true,
                Bandwidth = 0.4f
            }
        };

        // Messages data
        public class MessageData
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public bool Decoded { get; set; }
        }

        public Dictionary<string, MessageData> Messages { get; private set; } = new Dictionary<string, MessageData>
        {
            ["msg_001"] = new MessageData
            {
                Title = "Emergency Broadcast",
                Content = "This is an emergency broadcast. All citizens must evacuate to designated shelters immediately.",
                Decoded = false
            },
            ["msg_002"] = new MessageData
            {
                Title = "Military Communication",
                Content = "Alpha team, proceed to sector 7. Bravo team, hold position. Await further instructions.",
                Decoded = false
            },
            ["msg_003"] = new MessageData
            {
                Title = "Survivor Message",
                Content = "If anyone can hear this, we're at the old factory. We have supplies and shelter. Please respond.",
                Decoded = false
            }
        };

        // Signal functions
        public SignalData FindSignalAtFrequency(float freq)
        {
            foreach (var signalData in Signals)
            {
                float distance = Math.Abs(freq - signalData.Frequency);
                if (distance <= signalData.Bandwidth)
                {
                    return signalData;
                }
            }
            return null;
        }

        public static float CalculateSignalStrength(float freq, SignalData signalData)
        {
            float distance = Math.Abs(freq - signalData.Frequency);
            float maxDistance = signalData.Bandwidth;

            // Calculate strength based on how close we are to the exact frequency
            // 1.0 = perfect signal, 0.0 = no signal
            if (distance <= maxDistance)
            {
                return 1.0f - (distance / maxDistance);
            }
            else
            {
                return 0.0f;
            }
        }

        /// <summary>
        /// Calculates the final signal strength with environmental interference applied.
        /// </summary>
        /// <param name="baseStrength">The base signal strength</param>
        /// <returns>The final signal strength after applying interference</returns>
        public float ApplyInterferenceToSignal(float baseStrength)
        {
            // Apply time of day effect
            float timeOfDayFactor = 1.0f;
            if (!IsDaytime)
            {
                // Night time improves signal reception (less interference)
                timeOfDayFactor = 1.2f;
            }
            else
            {
                // Daytime has normal reception
                timeOfDayFactor = 1.0f;
            }

            // Apply environmental interference
            float interferenceEffect = 1.0f - TotalSignalInterference;

            // Calculate final strength
            float finalStrength = baseStrength * timeOfDayFactor * interferenceEffect;

            // Clamp to valid range
            return Mathf.Clamp(finalStrength, 0.0f, 1.0f);
        }

        public static float GetStaticIntensity(float freq)
        {
            // Generate a static intensity based on the frequency
            // This creates "dead zones" and "noisy areas" on the radio spectrum
            float baseNoise = 0.3f;
            float noiseFactor = Mathf.Sin(freq * 0.5f) * 0.3f + 0.5f;
            return baseNoise + noiseFactor * 0.7f;
        }

        public MessageData GetMessage(string messageId)
        {
            if (Messages.ContainsKey(messageId))
            {
                return Messages[messageId];
            }
            return null;
        }

        public void AddDiscoveredFrequency(float freq)
        {
            if (!DiscoveredFrequencies.Contains(freq))
            {
                DiscoveredFrequencies.Add(freq);
                EmitSignal(SignalName.FrequencyDiscovered, freq);
            }
        }

        /// <summary>
        /// Clears all discovered frequencies.
        /// </summary>
        public void ClearDiscoveredFrequencies()
        {
            DiscoveredFrequencies.Clear();
        }

        // Dictionary to store discovered signals
        private Dictionary<string, float> _discoveredSignals = new Dictionary<string, float>();

        // Set of objects that have been interacted with
        private HashSet<string> _interactedObjects = new HashSet<string>();

        /// <summary>
        /// Adds a discovered signal to the game state.
        /// </summary>
        /// <param name="signalId">The ID of the discovered signal</param>
        /// <param name="frequency">The frequency of the discovered signal</param>
        public void AddDiscoveredSignal(string signalId, float frequency)
        {
            if (!_discoveredSignals.ContainsKey(signalId))
            {
                _discoveredSignals[signalId] = frequency;
                AddDiscoveredFrequency(frequency);
                EmitSignal(SignalName.SignalDiscovered, signalId, frequency);
                GD.Print($"Signal discovered: {signalId} at {frequency} MHz");
            }
        }

        /// <summary>
        /// Checks if a signal has been discovered.
        /// </summary>
        /// <param name="signalId">The ID of the signal to check</param>
        /// <returns>True if the signal has been discovered, false otherwise</returns>
        public bool IsSignalDiscovered(string signalId)
        {
            return _discoveredSignals.ContainsKey(signalId);
        }

        /// <summary>
        /// Gets all discovered signals.
        /// </summary>
        /// <returns>A dictionary of discovered signals (ID -> frequency)</returns>
        public Dictionary<string, float> GetDiscoveredSignals()
        {
            return _discoveredSignals;
        }

        /// <summary>
        /// Clears all discovered signals.
        /// </summary>
        public void ClearDiscoveredSignals()
        {
            _discoveredSignals.Clear();
        }

        /// <summary>
        /// Sets an object as having been interacted with.
        /// </summary>
        /// <param name="objectId">The ID of the object</param>
        public void SetObjectInteractedWith(string objectId)
        {
            if (!_interactedObjects.Contains(objectId))
            {
                _interactedObjects.Add(objectId);
                GD.Print($"Object interacted with: {objectId}");
            }
        }

        /// <summary>
        /// Checks if an object has been interacted with.
        /// </summary>
        /// <param name="objectId">The ID of the object to check</param>
        /// <returns>True if the object has been interacted with, false otherwise</returns>
        public bool IsObjectInteractedWith(string objectId)
        {
            return _interactedObjects.Contains(objectId);
        }

        /// <summary>
        /// Gets all objects that have been interacted with.
        /// </summary>
        /// <returns>A set of object IDs</returns>
        public HashSet<string> GetInteractedObjects()
        {
            return _interactedObjects;
        }

        /// <summary>
        /// Clears all interacted objects.
        /// </summary>
        public void ClearInteractedObjects()
        {
            _interactedObjects.Clear();
        }

        /// <summary>
        /// Sets the game progress.
        /// </summary>
        /// <param name="progress">The new progress value</param>
        public void SetGameProgress(int progress)
        {
            GameProgress = progress;
        }

        /// <summary>
        /// Checks progression triggers based on the current game state.
        /// </summary>
        public void CheckProgressionTriggers()
        {
            // Get the game progression system
            var progressionSystem = GetNode<SignalLost.Progression.GameProgressionSystem>("/root/GameProgressionSystem");

            if (progressionSystem == null)
            {
                GD.PrintErr("GameState: GameProgressionSystem not found");
                return;
            }

            // Check for progression triggers based on the current state
            // This is just an example - in a real implementation, we would check various conditions

            // Check if we have discovered enough signals to progress
            if (_discoveredSignals.Count >= 3 && progressionSystem.GetCurrentStage() == SignalLost.Progression.GameProgressionSystem.GameStage.Exploration)
            {
                progressionSystem.IncrementProgress(10);
            }

            // Check if we have enough items to progress
            if (Inventory.Count >= 5 && progressionSystem.GetCurrentStage() == SignalLost.Progression.GameProgressionSystem.GameStage.Discovery)
            {
                progressionSystem.IncrementProgress(10);
            }
        }

        /// <summary>
        /// Sets a milestone as completed.
        /// </summary>
        /// <param name="milestoneId">The ID of the milestone</param>
        public void SetMilestoneCompleted(string milestoneId)
        {
            _completedMilestones.Add(milestoneId);
        }

        /// <summary>
        /// Checks if a milestone is completed.
        /// </summary>
        /// <param name="milestoneId">The ID of the milestone</param>
        /// <returns>True if the milestone is completed, false otherwise</returns>
        public bool IsMilestoneCompleted(string milestoneId)
        {
            return _completedMilestones.Contains(milestoneId);
        }

        /// <summary>
        /// Gets all completed milestones.
        /// </summary>
        /// <returns>A set of completed milestone IDs</returns>
        public HashSet<string> GetCompletedMilestones()
        {
            return _completedMilestones;
        }

        /// <summary>
        /// Clears all completed milestones.
        /// </summary>
        public void ClearCompletedMilestones()
        {
            _completedMilestones.Clear();
        }

        /// <summary>
        /// Adds signal interference from an environmental source.
        /// </summary>
        /// <param name="sourceId">The ID of the interference source</param>
        /// <param name="interferenceAmount">The amount of interference (0.0 to 1.0)</param>
        public void AddSignalInterference(string sourceId, float interferenceAmount)
        {
            _signalInterferences[sourceId] = Mathf.Clamp(interferenceAmount, 0.0f, 1.0f);
            UpdateTotalInterference();
            GD.Print($"Added signal interference: {sourceId} = {interferenceAmount}");
        }

        /// <summary>
        /// Removes signal interference from an environmental source.
        /// </summary>
        /// <param name="sourceId">The ID of the interference source</param>
        public void RemoveSignalInterference(string sourceId)
        {
            if (_signalInterferences.ContainsKey(sourceId))
            {
                _signalInterferences.Remove(sourceId);
                UpdateTotalInterference();
                GD.Print($"Removed signal interference: {sourceId}");
            }
        }

        /// <summary>
        /// Updates the total signal interference value.
        /// </summary>
        private void UpdateTotalInterference()
        {
            // Calculate total interference (capped at 1.0)
            float total = 0.0f;
            foreach (var interference in _signalInterferences.Values)
            {
                // Use a non-linear combination to prevent excessive stacking
                total = total + interference * (1.0f - total);
            }

            TotalSignalInterference = Mathf.Clamp(total, 0.0f, 1.0f);
            EmitSignal(SignalName.SignalInterferenceChanged, TotalSignalInterference);
            GD.Print($"Total signal interference updated: {TotalSignalInterference}");
        }

        /// <summary>
        /// Sets the time of day.
        /// </summary>
        /// <param name="time">The time in 24-hour format (0-24)</param>
        public void SetTimeOfDay(float time)
        {
            TimeOfDay = Mathf.Clamp(time, 0.0f, 24.0f);

            // Update day/night status
            bool wasDaytime = IsDaytime;
            IsDaytime = TimeOfDay >= 6.0f && TimeOfDay <= 18.0f;

            // Emit signal if day/night status changed
            if (wasDaytime != IsDaytime)
            {
                EmitSignal(SignalName.DayNightChanged, IsDaytime);
            }

            EmitSignal(SignalName.TimeOfDayChanged, TimeOfDay);
            GD.Print($"Time of day set to {TimeOfDay}, is daytime: {IsDaytime}");
        }

        // Signals (Godot's events, not radio signals)
        [Signal]
        public delegate void FrequencyChangedEventHandler(float newFrequency);

        [Signal]
        public delegate void RadioToggledEventHandler(bool isOn);

        [Signal]
        public delegate void FrequencyDiscoveredEventHandler(float frequency);

        [Signal]
        public delegate void MessageDecodedEventHandler(string messageId);

        [Signal]
        public delegate void LocationChangedEventHandler(string locationId);

        [Signal]
        public delegate void InventoryChangedEventHandler();

        [Signal]
        public delegate void SignalDiscoveredEventHandler(string signalId, float frequency);

        [Signal]
        public delegate void SignalInterferenceChangedEventHandler(float interferenceLevel);

        [Signal]
        public delegate void TimeOfDayChangedEventHandler(float time);

        [Signal]
        public delegate void DayNightChangedEventHandler(bool isDaytime);

        // Functions to modify state
        public void SetFrequency(float freq)
        {
            CurrentFrequency = Mathf.Clamp(freq, 88.0f, 108.0f);
            EmitSignal(SignalName.FrequencyChanged, CurrentFrequency);
        }

        public void ToggleRadio()
        {
            IsRadioOn = !IsRadioOn;
            EmitSignal(SignalName.RadioToggled, IsRadioOn);
        }

        public bool DecodeMessage(string messageId)
        {
            if (Messages.ContainsKey(messageId) && !Messages[messageId].Decoded)
            {
                Messages[messageId].Decoded = true;
                EmitSignal(SignalName.MessageDecoded, messageId);
                return true;
            }
            return false;
        }

        public void SetCurrentLocation(string locationId)
        {
            if (CurrentLocation != locationId)
            {
                CurrentLocation = locationId;
                EmitSignal(SignalName.LocationChanged, locationId);
            }
        }

        // Inventory management functions
        public void AddToInventory(string itemId)
        {
            Inventory.Add(itemId);
            EmitSignal(SignalName.InventoryChanged);
        }

        public void RemoveFromInventory(string itemId)
        {
            Inventory.Remove(itemId);
            EmitSignal(SignalName.InventoryChanged);
        }

        public void ClearInventory()
        {
            Inventory.Clear();
            EmitSignal(SignalName.InventoryChanged);
        }

        public bool HasItem(string itemId)
        {
            return Inventory.Contains(itemId);
        }

        // Experience and progression system
        private int _experience = 0;
        private int _currency = 0;
        private HashSet<string> _unlockedFeatures = new HashSet<string>();
        private HashSet<string> _unlockedSkills = new HashSet<string>();

        /// <summary>
        /// Adds experience points to the player.
        /// </summary>
        /// <param name="amount">The amount of experience to add</param>
        public void AddExperience(int amount)
        {
            _experience += amount;
            GD.Print($"Added {amount} experience points. Total: {_experience}");
        }

        /// <summary>
        /// Gets the current experience points.
        /// </summary>
        /// <returns>The current experience points</returns>
        public int GetExperience()
        {
            return _experience;
        }

        /// <summary>
        /// Adds currency to the player.
        /// </summary>
        /// <param name="amount">The amount of currency to add</param>
        public void AddCurrency(int amount)
        {
            _currency += amount;
            GD.Print($"Added {amount} currency. Total: {_currency}");
        }

        /// <summary>
        /// Gets the current currency amount.
        /// </summary>
        /// <returns>The current currency amount</returns>
        public int GetCurrency()
        {
            return _currency;
        }

        /// <summary>
        /// Unlocks a feature.
        /// </summary>
        /// <param name="featureId">The ID of the feature to unlock</param>
        public void UnlockFeature(string featureId)
        {
            _unlockedFeatures.Add(featureId);
            GD.Print($"Unlocked feature: {featureId}");
        }

        /// <summary>
        /// Checks if a feature is unlocked.
        /// </summary>
        /// <param name="featureId">The ID of the feature to check</param>
        /// <returns>True if the feature is unlocked, false otherwise</returns>
        public bool IsFeatureUnlocked(string featureId)
        {
            return _unlockedFeatures.Contains(featureId);
        }

        /// <summary>
        /// Gets all unlocked features.
        /// </summary>
        /// <returns>A set of unlocked feature IDs</returns>
        public HashSet<string> GetUnlockedFeatures()
        {
            return _unlockedFeatures;
        }

        /// <summary>
        /// Unlocks a skill.
        /// </summary>
        /// <param name="skillId">The ID of the skill to unlock</param>
        public void UnlockSkill(string skillId)
        {
            _unlockedSkills.Add(skillId);
            GD.Print($"Unlocked skill: {skillId}");
        }

        /// <summary>
        /// Checks if a skill is unlocked.
        /// </summary>
        /// <param name="skillId">The ID of the skill to check</param>
        /// <returns>True if the skill is unlocked, false otherwise</returns>
        public bool IsSkillUnlocked(string skillId)
        {
            return _unlockedSkills.Contains(skillId);
        }

        /// <summary>
        /// Gets all unlocked skills.
        /// </summary>
        /// <returns>A set of unlocked skill IDs</returns>
        public HashSet<string> GetUnlockedSkills()
        {
            return _unlockedSkills;
        }

        // Initialize the game state
        public void Initialize()
        {
            // Initialize game state variables
            CurrentFrequency = 90.0f;
            IsRadioOn = false;
            DiscoveredFrequencies.Clear();
            CurrentLocation = "bunker";
            Inventory.Clear();
            GameProgress = 0;
            _discoveredSignals.Clear();
            _interactedObjects.Clear();
            _completedMilestones.Clear();

            // Initialize environmental effects
            _signalInterferences.Clear();
            TotalSignalInterference = 0.0f;
            TimeOfDay = 12.0f;
            IsDaytime = true;

            // Initialize progression system variables
            _experience = 0;
            _currency = 0;
            _unlockedFeatures.Clear();
            _unlockedSkills.Clear();

            GD.Print("GameState: Initialized");
        }

        // Start a new game
        public void NewGame()
        {
            // Initialize game state
            Initialize();

            // Add initial items to inventory
            Inventory.Add("flashlight");

            GD.Print("GameState: New game started");
        }

        // Save and load functions
        public bool SaveGame()
        {
            // Use the SaveManager instead
            var saveManager = GetNode<SaveManager>("/root/SaveManager");
            if (saveManager != null)
            {
                return saveManager.SaveGame();
            }

            // Fallback to legacy saving if SaveManager is not available
            var saveData = new Dictionary<string, object>
            {
                ["current_frequency"] = CurrentFrequency,
                ["is_radio_on"] = IsRadioOn,
                ["discovered_frequencies"] = DiscoveredFrequencies,
                ["current_location"] = CurrentLocation,
                ["inventory"] = Inventory,
                ["game_progress"] = GameProgress,
                ["stage_progress"] = StageProgress,
                ["completed_milestones"] = _completedMilestones,
                ["messages"] = Messages,
                ["discovered_signals"] = _discoveredSignals,
                ["interacted_objects"] = _interactedObjects
            };

            string jsonString = JsonSerializer.Serialize(saveData);

            using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Write);
            if (saveFile == null)
            {
                return false;
            }

            saveFile.StoreLine(jsonString);
            return true;
        }

        public bool LoadGame()
        {
            // Use the SaveManager instead
            var saveManager = GetNode<SaveManager>("/root/SaveManager");
            if (saveManager != null)
            {
                return saveManager.LoadGame();
            }

            // Fallback to legacy loading if SaveManager is not available
            if (!FileAccess.FileExists("user://savegame.save"))
            {
                return false;
            }

            using var saveFile = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);
            if (saveFile == null)
            {
                return false;
            }

            string jsonString = saveFile.GetLine();
            var saveData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

            CurrentFrequency = Convert.ToSingle(saveData["current_frequency"]);
            IsRadioOn = Convert.ToBoolean(saveData["is_radio_on"]);
            DiscoveredFrequencies = JsonSerializer.Deserialize<List<float>>(saveData["discovered_frequencies"].ToString());
            CurrentLocation = saveData["current_location"].ToString();
            Inventory = JsonSerializer.Deserialize<List<string>>(saveData["inventory"].ToString());
            GameProgress = Convert.ToInt32(saveData["game_progress"]);

            // Load stage progress if it exists
            if (saveData.ContainsKey("stage_progress"))
            {
                StageProgress = Convert.ToInt32(saveData["stage_progress"]);
            }

            // Load completed milestones if they exist
            if (saveData.ContainsKey("completed_milestones"))
            {
                _completedMilestones = JsonSerializer.Deserialize<HashSet<string>>(saveData["completed_milestones"].ToString());
            }

            Messages = JsonSerializer.Deserialize<Dictionary<string, MessageData>>(saveData["messages"].ToString());

            // Load discovered signals if they exist
            if (saveData.ContainsKey("discovered_signals"))
            {
                _discoveredSignals = JsonSerializer.Deserialize<Dictionary<string, float>>(saveData["discovered_signals"].ToString());
            }

            // Load interacted objects if they exist
            if (saveData.ContainsKey("interacted_objects"))
            {
                _interactedObjects = JsonSerializer.Deserialize<HashSet<string>>(saveData["interacted_objects"].ToString());
            }

            return true;
        }
    }
}

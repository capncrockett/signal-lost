using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

// Use Godot's FileAccess, not System.IO.FileAccess
using FileAccess = Godot.FileAccess;

namespace SignalLost
{
    /// <summary>
    /// Manages saving and loading game data.
    /// </summary>
    [GlobalClass]
    public partial class SaveManager : Node
    {
        // Singleton instance
        private static SaveManager _instance;

        // Constants
        private const string SAVE_DIRECTORY = "user://saves";
        private const string SAVE_EXTENSION = ".save";
        private const string AUTO_SAVE_NAME = "autosave";
        private const int MAX_SAVE_SLOTS = 5;

        // Events
        [Signal]
        public delegate void SaveCompletedEventHandler(bool success, string saveName);

        [Signal]
        public delegate void LoadCompletedEventHandler(bool success, string saveName);

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            _instance = this;

            // Ensure save directory exists
            EnsureSaveDirectoryExists();

            GD.Print("SaveManager: Initialized");
        }

        /// <summary>
        /// Gets the singleton instance of the SaveManager.
        /// </summary>
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SaveManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Ensures that the save directory exists.
        /// </summary>
        private void EnsureSaveDirectoryExists()
        {
            var dir = DirAccess.Open("user://");
            if (dir != null && !dir.DirExists(SAVE_DIRECTORY.Replace("user://", "")))
            {
                dir.MakeDir(SAVE_DIRECTORY.Replace("user://", ""));
                GD.Print($"SaveManager: Created save directory at {SAVE_DIRECTORY}");
            }
        }

        /// <summary>
        /// Saves the game to the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the save slot</param>
        /// <returns>True if the save was successful, false otherwise</returns>
        public bool SaveGame(string slotName = AUTO_SAVE_NAME)
        {
            try
            {
                // Create save data
                var saveData = CreateSaveData();

                // Serialize save data
                string jsonString = JsonSerializer.Serialize(saveData, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Save to file
                string savePath = $"{SAVE_DIRECTORY}/{slotName}{SAVE_EXTENSION}";
                using var saveFile = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
                if (saveFile == null)
                {
                    GD.PrintErr($"SaveManager: Failed to open save file at {savePath}");
                    EmitSignal(SignalName.SaveCompleted, false, slotName);
                    return false;
                }

                saveFile.StoreString(jsonString);
                GD.Print($"SaveManager: Game saved to {savePath}");

                EmitSignal(SignalName.SaveCompleted, true, slotName);
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SaveManager: Error saving game: {ex.Message}");
                EmitSignal(SignalName.SaveCompleted, false, slotName);
                return false;
            }
        }

        /// <summary>
        /// Loads the game from the specified slot.
        /// </summary>
        /// <param name="slotName">The name of the save slot</param>
        /// <returns>True if the load was successful, false otherwise</returns>
        public bool LoadGame(string slotName = AUTO_SAVE_NAME)
        {
            try
            {
                // Check if save file exists
                string savePath = $"{SAVE_DIRECTORY}/{slotName}{SAVE_EXTENSION}";
                if (!FileAccess.FileExists(savePath))
                {
                    GD.PrintErr($"SaveManager: Save file not found at {savePath}");
                    EmitSignal(SignalName.LoadCompleted, false, slotName);
                    return false;
                }

                // Load from file
                using var saveFile = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
                if (saveFile == null)
                {
                    GD.PrintErr($"SaveManager: Failed to open save file at {savePath}");
                    EmitSignal(SignalName.LoadCompleted, false, slotName);
                    return false;
                }

                string jsonString = saveFile.GetAsText();

                // Deserialize save data
                var saveData = JsonSerializer.Deserialize<SaveData>(jsonString);
                if (saveData == null)
                {
                    GD.PrintErr($"SaveManager: Failed to deserialize save data from {savePath}");
                    EmitSignal(SignalName.LoadCompleted, false, slotName);
                    return false;
                }

                // Apply save data to game state
                ApplySaveData(saveData);

                GD.Print($"SaveManager: Game loaded from {savePath}");
                EmitSignal(SignalName.LoadCompleted, true, slotName);
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SaveManager: Error loading game: {ex.Message}");
                EmitSignal(SignalName.LoadCompleted, false, slotName);
                return false;
            }
        }

        /// <summary>
        /// Gets a list of all save slots.
        /// </summary>
        /// <returns>A list of save slot names</returns>
        public List<string> GetSaveSlots()
        {
            var saveSlots = new List<string>();

            var dir = DirAccess.Open(SAVE_DIRECTORY);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();

                while (!string.IsNullOrEmpty(fileName))
                {
                    if (!dir.CurrentIsDir() && fileName.EndsWith(SAVE_EXTENSION))
                    {
                        string slotName = fileName.Replace(SAVE_EXTENSION, "");
                        saveSlots.Add(slotName);
                    }

                    fileName = dir.GetNext();
                }

                dir.ListDirEnd();
            }

            return saveSlots;
        }

        /// <summary>
        /// Deletes a save slot.
        /// </summary>
        /// <param name="slotName">The name of the save slot to delete</param>
        /// <returns>True if the deletion was successful, false otherwise</returns>
        public bool DeleteSaveSlot(string slotName)
        {
            string savePath = $"{SAVE_DIRECTORY}/{slotName}{SAVE_EXTENSION}";

            if (FileAccess.FileExists(savePath))
            {
                var dir = DirAccess.Open(SAVE_DIRECTORY);
                if (dir != null)
                {
                    Error error = dir.Remove(slotName + SAVE_EXTENSION);
                    if (error == Error.Ok)
                    {
                        GD.Print($"SaveManager: Deleted save slot {slotName}");
                        return true;
                    }
                    else
                    {
                        GD.PrintErr($"SaveManager: Failed to delete save slot {slotName}");
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Creates save data from the current game state.
        /// </summary>
        /// <returns>The save data</returns>
        private SaveData CreateSaveData()
        {
            var saveData = new SaveData();

            // Get the game state
            var gameState = GetNode<GameState>("/root/GameState");
            if (gameState != null)
            {
                // Basic game state
                saveData.CurrentFrequency = gameState.CurrentFrequency;
                saveData.IsRadioOn = gameState.IsRadioOn;
                saveData.DiscoveredFrequencies = new List<float>(gameState.DiscoveredFrequencies);
                saveData.CurrentLocation = gameState.CurrentLocation;
                saveData.Inventory = new List<string>(gameState.Inventory);
                saveData.GameProgress = gameState.GameProgress;

                // Messages
                saveData.Messages = new Dictionary<string, SaveData.MessageInfo>();
                foreach (var message in gameState.Messages)
                {
                    saveData.Messages[message.Key] = new SaveData.MessageInfo
                    {
                        Title = message.Value.Title,
                        Content = message.Value.Content,
                        Decoded = message.Value.Decoded
                    };
                }
            }

            // Get the field exploration scene
            var fieldExplorationScene = GetNodeOrNull<Field.FieldExplorationScene>("/root/FieldExplorationScene");
            if (fieldExplorationScene != null)
            {
                // Get the player controller
                var playerController = fieldExplorationScene.GetNode<Field.PlayerController>("PlayerController");
                if (playerController != null)
                {
                    // Player state
                    saveData.PlayerPosition = playerController.GetGridPosition();
                    saveData.PlayerFacingDirection = playerController.GetFacingDirection();
                }

                // Get signal sources
                saveData.SignalSources = new List<SaveData.SignalSourceInfo>();
                var signalSources = fieldExplorationScene.GetTree().GetNodesInGroup("SignalSource");
                foreach (var source in signalSources)
                {
                    if (source is Field.SignalSourceObject signalSource)
                    {
                        saveData.SignalSources.Add(new SaveData.SignalSourceInfo
                        {
                            Position = fieldExplorationScene.GetNode<Field.GridSystem>("GridSystem").WorldToGridPosition(signalSource.GlobalPosition),
                            Frequency = signalSource.Frequency,
                            MessageId = signalSource.MessageId,
                            SignalStrength = signalSource.SignalStrength,
                            SignalRange = signalSource.SignalRange
                        });
                    }
                }
            }

            // Get progression data
            var questSystem = GetNode<QuestSystem>("/root/QuestSystem");
            var mapSystem = GetNode<MapSystem>("/root/MapSystem");
            var progressionManager = GetNode<GameProgressionManager>("/root/GameProgressionManager");

            if (questSystem != null)
            {
                // Save completed quests
                saveData.CompletedQuests = new List<string>(questSystem.GetCompletedQuests().Keys);
            }

            if (mapSystem != null)
            {
                // Save discovered locations
                saveData.DiscoveredLocations = new List<string>();
                foreach (var location in mapSystem.GetAllLocations())
                {
                    if (location.Value.IsDiscovered)
                    {
                        saveData.DiscoveredLocations.Add(location.Key);
                    }
                }
            }

            if (progressionManager != null)
            {
                // Save progression stage
                saveData.ProgressionStage = (int)progressionManager.CurrentStage;
            }
            else
            {
                // If progression manager is not available, use GameState's progress
                saveData.ProgressionStage = gameState.GameProgress;
            }

            // Add timestamp
            saveData.Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return saveData;
        }

        /// <summary>
        /// Applies save data to the game state.
        /// </summary>
        /// <param name="saveData">The save data to apply</param>
        private void ApplySaveData(SaveData saveData)
        {
            // Get the game state
            var gameState = GetNode<GameState>("/root/GameState");
            if (gameState != null)
            {
                // Basic game state
                gameState.SetFrequency(saveData.CurrentFrequency);
                if (gameState.IsRadioOn != saveData.IsRadioOn)
                {
                    gameState.ToggleRadio();
                }

                // Clear and re-add discovered frequencies
                gameState.ClearDiscoveredFrequencies();
                foreach (var frequency in saveData.DiscoveredFrequencies)
                {
                    gameState.AddDiscoveredFrequency(frequency);
                }

                gameState.SetCurrentLocation(saveData.CurrentLocation);

                // Clear and re-add inventory items
                gameState.ClearInventory();
                foreach (var item in saveData.Inventory)
                {
                    gameState.AddToInventory(item);
                }

                gameState.SetGameProgress(saveData.GameProgress);

                // Messages
                foreach (var message in saveData.Messages)
                {
                    if (gameState.Messages.ContainsKey(message.Key) && message.Value.Decoded)
                    {
                        gameState.DecodeMessage(message.Key);
                    }
                }
            }

            // Get progression data
            var questSystem = GetNode<QuestSystem>("/root/QuestSystem");
            var mapSystem = GetNode<MapSystem>("/root/MapSystem");
            var progressionManager = GetNode<GameProgressionManager>("/root/GameProgressionManager");

            // Restore completed quests
            if (questSystem != null && saveData.CompletedQuests != null)
            {
                // This is a simplified approach - in a real implementation, you would need to
                // properly restore the quest state including objectives
                foreach (var questId in saveData.CompletedQuests)
                {
                    if (questSystem.GetQuest(questId) != null && !questSystem.IsQuestCompleted(questId))
                    {
                        // Discover and activate the quest first
                        questSystem.DiscoverQuest(questId);
                        questSystem.ActivateQuest(questId);

                        // Complete all objectives and the quest
                        var quest = questSystem.GetQuest(questId);
                        if (quest != null)
                        {
                            foreach (var objective in quest.Objectives)
                            {
                                objective.IsCompleted = true;
                                objective.CurrentAmount = objective.RequiredAmount;
                            }

                            // Force complete the quest
                            questSystem.CompleteQuest(questId);
                        }
                    }
                }
            }

            // Restore discovered locations
            if (mapSystem != null && saveData.DiscoveredLocations != null)
            {
                foreach (var locationId in saveData.DiscoveredLocations)
                {
                    mapSystem.DiscoverLocation(locationId);
                }
            }

            // Restore progression stage
            if (progressionManager != null)
            {
                progressionManager.SetProgression((GameProgressionManager.ProgressionStage)saveData.ProgressionStage);
            }

            // Get the field exploration scene
            var fieldExplorationScene = GetNodeOrNull<Field.FieldExplorationScene>("/root/FieldExplorationScene");
            if (fieldExplorationScene != null)
            {
                // Get the player controller
                var playerController = fieldExplorationScene.GetNode<Field.PlayerController>("PlayerController");
                if (playerController != null)
                {
                    // Player state
                    playerController.SetGridPosition(saveData.PlayerPosition);
                    playerController.SetFacingDirection(saveData.PlayerFacingDirection);
                }

                // Clear existing signal sources
                var existingSources = fieldExplorationScene.GetTree().GetNodesInGroup("SignalSource");
                foreach (var source in existingSources)
                {
                    source.QueueFree();
                }

                // Create signal sources from save data
                foreach (var sourceInfo in saveData.SignalSources)
                {
                    fieldExplorationScene.CreateSignalSource(
                        sourceInfo.Position,
                        sourceInfo.Frequency,
                        sourceInfo.MessageId,
                        sourceInfo.SignalStrength,
                        sourceInfo.SignalRange
                    );
                }
            }
        }
    }
}

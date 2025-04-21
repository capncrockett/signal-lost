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
        public int GameProgress { get; private set; } = 0;

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



        /// <summary>
        /// Sets the game progress.
        /// </summary>
        /// <param name="progress">The new progress value</param>
        public void SetGameProgress(int progress)
        {
            GameProgress = progress;
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
                ["messages"] = Messages
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
            Messages = JsonSerializer.Deserialize<Dictionary<string, MessageData>>(saveData["messages"].ToString());

            return true;
        }
    }
}

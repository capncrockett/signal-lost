using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SignalLost.Tests
{
    /// <summary>
    /// Static helper class with utility methods for tests.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Waits for a specified number of frames to be processed.
        /// </summary>
        /// <param name="node">The node to use for waiting</param>
        /// <param name="frames">The number of frames to wait</param>
        public static async Task WaitFrames(Node node, int frames)
        {
            for (int i = 0; i < frames; i++)
            {
                await node.ToSignal(node.GetTree(), "process_frame");
            }
        }
        
        /// <summary>
        /// Waits for a specified number of seconds (real time).
        /// </summary>
        /// <param name="seconds">The number of seconds to wait</param>
        public static async Task WaitSeconds(float seconds)
        {
            await Task.Delay((int)(seconds * 1000));
        }
        
        /// <summary>
        /// Gets a random float value between min and max.
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>A random float value</returns>
        public static float RandomFloat(float min, float max)
        {
            return (float)GD.RandRange(min, max);
        }
        
        /// <summary>
        /// Gets a random integer value between min and max (inclusive).
        /// </summary>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>A random integer value</returns>
        public static int RandomInt(int min, int max)
        {
            return (int)GD.RandRange(min, max);
        }
        
        /// <summary>
        /// Gets a random item from a list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list</typeparam>
        /// <param name="list">The list to get a random item from</param>
        /// <returns>A random item from the list</returns>
        public static T RandomItem<T>(List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default;
            }
            
            int index = RandomInt(0, list.Count - 1);
            return list[index];
        }
        
        /// <summary>
        /// Gets a random key from a dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of keys in the dictionary</typeparam>
        /// <typeparam name="TValue">The type of values in the dictionary</typeparam>
        /// <param name="dictionary">The dictionary to get a random key from</param>
        /// <returns>A random key from the dictionary</returns>
        public static TKey RandomKey<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null || dictionary.Count == 0)
            {
                return default;
            }
            
            int index = RandomInt(0, dictionary.Count - 1);
            int i = 0;
            
            foreach (var key in dictionary.Keys)
            {
                if (i == index)
                {
                    return key;
                }
                
                i++;
            }
            
            return default;
        }
        
        /// <summary>
        /// Creates a mock RadioSignal for testing.
        /// </summary>
        /// <param name="id">The ID of the signal</param>
        /// <param name="frequency">The frequency of the signal</param>
        /// <param name="type">The type of the signal</param>
        /// <param name="strength">The strength of the signal</param>
        /// <returns>A new RadioSignal instance</returns>
        public static RadioSignal CreateMockRadioSignal(string id, float frequency, RadioSignalType type = RadioSignalType.Voice, float strength = 1.0f)
        {
            return new RadioSignal
            {
                Id = id,
                Frequency = frequency,
                Message = $"Test message for signal {id}",
                Type = type,
                Strength = strength,
                IsActive = true
            };
        }
        
        /// <summary>
        /// Creates a mock RadioMessage for testing.
        /// </summary>
        /// <param name="id">The ID of the message</param>
        /// <param name="title">The title of the message</param>
        /// <param name="content">The content of the message</param>
        /// <param name="frequency">The frequency of the message</param>
        /// <param name="decoded">Whether the message is decoded</param>
        /// <returns>A new RadioMessage instance</returns>
        public static RadioMessage CreateMockRadioMessage(string id, string title, string content, float frequency, bool decoded = false)
        {
            return new RadioMessage
            {
                Id = id,
                Title = title,
                Content = content,
                Frequency = frequency,
                Decoded = decoded
            };
        }
        
        /// <summary>
        /// Creates a mock Quest for testing.
        /// </summary>
        /// <param name="id">The ID of the quest</param>
        /// <param name="title">The title of the quest</param>
        /// <param name="description">The description of the quest</param>
        /// <param name="objectiveCount">The number of objectives to create</param>
        /// <returns>A new Quest instance</returns>
        public static Quest CreateMockQuest(string id, string title, string description, int objectiveCount = 1)
        {
            var quest = new Quest
            {
                Id = id,
                Title = title,
                Description = description,
                IsDiscovered = false,
                IsActive = false,
                IsCompleted = false
            };
            
            for (int i = 0; i < objectiveCount; i++)
            {
                quest.Objectives.Add(new QuestObjective
                {
                    Id = $"{id}_objective_{i + 1}",
                    Description = $"Objective {i + 1} for {title}",
                    RequiredAmount = 1,
                    CurrentAmount = 0,
                    IsCompleted = false
                });
            }
            
            return quest;
        }
        
        /// <summary>
        /// Creates a mock Location for testing.
        /// </summary>
        /// <param name="id">The ID of the location</param>
        /// <param name="name">The name of the location</param>
        /// <param name="description">The description of the location</param>
        /// <param name="discovered">Whether the location is discovered</param>
        /// <returns>A new Location instance</returns>
        public static Location CreateMockLocation(string id, string name, string description, bool discovered = false)
        {
            return new Location
            {
                Id = id,
                Name = name,
                Description = description,
                IsDiscovered = discovered,
                Position = new Vector2(RandomFloat(-100, 100), RandomFloat(-100, 100))
            };
        }
        
        /// <summary>
        /// Creates a mock InventorySystem.ItemData for testing.
        /// </summary>
        /// <param name="id">The ID of the item</param>
        /// <param name="name">The name of the item</param>
        /// <param name="description">The description of the item</param>
        /// <param name="category">The category of the item</param>
        /// <param name="isUsable">Whether the item is usable</param>
        /// <param name="isConsumable">Whether the item is consumable</param>
        /// <returns>A new InventorySystem.ItemData instance</returns>
        public static InventorySystem.ItemData CreateMockItem(string id, string name, string description, string category, bool isUsable = true, bool isConsumable = false)
        {
            return new InventorySystem.ItemData
            {
                Id = id,
                Name = name,
                Description = description,
                Category = category,
                IsUsable = isUsable,
                IsConsumable = isConsumable,
                Quantity = 1,
                IconPath = "res://assets/images/items/default_item.png"
            };
        }
    }
}

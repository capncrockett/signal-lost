using Godot;
using System.Collections.Generic;

namespace SignalLost.Tests
{
    /// <summary>
    /// Static class with common test data.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// Common radio frequencies for testing.
        /// </summary>
        public static class Frequencies
        {
            public const float Min = 88.0f;
            public const float Max = 108.0f;
            
            public const float Signal1 = 91.5f;
            public const float Signal2 = 95.7f;
            public const float Signal3 = 103.2f;
            
            public const float NoSignal1 = 90.0f;
            public const float NoSignal2 = 100.0f;
        }
        
        /// <summary>
        /// Common message IDs for testing.
        /// </summary>
        public static class MessageIds
        {
            public const string Message1 = "msg_001";
            public const string Message2 = "msg_002";
            public const string Message3 = "msg_003";
            public const string Message4 = "msg_004";
            public const string Message5 = "msg_005";
        }
        
        /// <summary>
        /// Common quest IDs for testing.
        /// </summary>
        public static class QuestIds
        {
            public const string RadioRepair = "quest_radio_repair";
            public const string ExploreForest = "quest_explore_forest";
            public const string SurvivorMessage = "quest_survivor_message";
            public const string FinalTransmission = "quest_final_transmission";
        }
        
        /// <summary>
        /// Common location IDs for testing.
        /// </summary>
        public static class LocationIds
        {
            public const string Bunker = "bunker";
            public const string Forest = "forest";
            public const string Town = "town";
            public const string Factory = "factory";
            public const string RadioTower = "radio_tower";
        }
        
        /// <summary>
        /// Common item IDs for testing.
        /// </summary>
        public static class ItemIds
        {
            public const string Flashlight = "flashlight";
            public const string Battery = "battery";
            public const string Medkit = "medkit";
            public const string KeyCabin = "key_cabin";
            public const string KeyFactory = "factory_key";
            public const string MapFragment = "map_fragment";
            public const string RadioPart = "radio_part";
            public const string WaterBottle = "water_bottle";
            public const string CannedFood = "canned_food";
        }
        
        /// <summary>
        /// Creates a set of test radio signals.
        /// </summary>
        /// <returns>A dictionary of test radio signals</returns>
        public static Dictionary<string, RadioSignal> CreateTestSignals()
        {
            var signals = new Dictionary<string, RadioSignal>();
            
            signals.Add("signal_1", new RadioSignal
            {
                Id = "signal_1",
                Frequency = Frequencies.Signal1,
                Message = "Test signal 1",
                Type = RadioSignalType.Voice,
                Strength = 1.0f,
                IsActive = true,
                MessageId = MessageIds.Message1
            });
            
            signals.Add("signal_2", new RadioSignal
            {
                Id = "signal_2",
                Frequency = Frequencies.Signal2,
                Message = "Test signal 2",
                Type = RadioSignalType.Morse,
                Strength = 0.8f,
                IsActive = true,
                MessageId = MessageIds.Message2
            });
            
            signals.Add("signal_3", new RadioSignal
            {
                Id = "signal_3",
                Frequency = Frequencies.Signal3,
                Message = "Test signal 3",
                Type = RadioSignalType.Data,
                Strength = 0.6f,
                IsActive = true,
                MessageId = MessageIds.Message3
            });
            
            return signals;
        }
        
        /// <summary>
        /// Creates a set of test radio messages.
        /// </summary>
        /// <returns>A dictionary of test radio messages</returns>
        public static Dictionary<string, RadioMessage> CreateTestMessages()
        {
            var messages = new Dictionary<string, RadioMessage>();
            
            messages.Add(MessageIds.Message1, new RadioMessage
            {
                Id = MessageIds.Message1,
                Title = "Test Message 1",
                Content = "This is test message 1 content.",
                Frequency = Frequencies.Signal1,
                Decoded = false
            });
            
            messages.Add(MessageIds.Message2, new RadioMessage
            {
                Id = MessageIds.Message2,
                Title = "Test Message 2",
                Content = "This is test message 2 content.",
                Frequency = Frequencies.Signal2,
                Decoded = false
            });
            
            messages.Add(MessageIds.Message3, new RadioMessage
            {
                Id = MessageIds.Message3,
                Title = "Test Message 3",
                Content = "This is test message 3 content.",
                Frequency = Frequencies.Signal3,
                Decoded = false
            });
            
            messages.Add(MessageIds.Message4, new RadioMessage
            {
                Id = MessageIds.Message4,
                Title = "Test Message 4",
                Content = "This is test message 4 content.",
                Frequency = 97.3f,
                Decoded = false
            });
            
            messages.Add(MessageIds.Message5, new RadioMessage
            {
                Id = MessageIds.Message5,
                Title = "Test Message 5",
                Content = "This is test message 5 content.",
                Frequency = 105.8f,
                Decoded = false
            });
            
            return messages;
        }
        
        /// <summary>
        /// Creates a set of test quests.
        /// </summary>
        /// <returns>A dictionary of test quests</returns>
        public static Dictionary<string, Quest> CreateTestQuests()
        {
            var quests = new Dictionary<string, Quest>();
            
            // Radio Repair Quest
            var radioRepairQuest = new Quest
            {
                Id = QuestIds.RadioRepair,
                Title = "Repair the Radio",
                Description = "Find parts to repair the radio.",
                IsDiscovered = true,
                IsActive = false,
                IsCompleted = false
            };
            
            radioRepairQuest.Objectives.Add(new QuestObjective
            {
                Id = "obj_find_radio_parts",
                Description = "Find radio parts",
                RequiredAmount = 1,
                CurrentAmount = 0,
                IsCompleted = false
            });
            
            radioRepairQuest.Objectives.Add(new QuestObjective
            {
                Id = "obj_repair_radio",
                Description = "Repair the radio",
                RequiredAmount = 1,
                CurrentAmount = 0,
                IsCompleted = false
            });
            
            quests.Add(QuestIds.RadioRepair, radioRepairQuest);
            
            // Explore Forest Quest
            var exploreForestQuest = new Quest
            {
                Id = QuestIds.ExploreForest,
                Title = "Explore the Forest",
                Description = "Explore the forest to find clues.",
                IsDiscovered = false,
                IsActive = false,
                IsCompleted = false
            };
            
            exploreForestQuest.Objectives.Add(new QuestObjective
            {
                Id = "obj_find_forest_path",
                Description = "Find the forest path",
                RequiredAmount = 1,
                CurrentAmount = 0,
                IsCompleted = false
            });
            
            exploreForestQuest.Objectives.Add(new QuestObjective
            {
                Id = "obj_reach_forest_clearing",
                Description = "Reach the forest clearing",
                RequiredAmount = 1,
                CurrentAmount = 0,
                IsCompleted = false
            });
            
            quests.Add(QuestIds.ExploreForest, exploreForestQuest);
            
            // Survivor Message Quest
            var survivorMessageQuest = new Quest
            {
                Id = QuestIds.SurvivorMessage,
                Title = "Survivor Message",
                Description = "Decode the survivor message.",
                IsDiscovered = false,
                IsActive = false,
                IsCompleted = false
            };
            
            survivorMessageQuest.Objectives.Add(new QuestObjective
            {
                Id = "obj_find_survivor_frequency",
                Description = "Find the survivor frequency",
                RequiredAmount = 1,
                CurrentAmount = 0,
                IsCompleted = false
            });
            
            survivorMessageQuest.Objectives.Add(new QuestObjective
            {
                Id = "obj_decode_survivor_message",
                Description = "Decode the survivor message",
                RequiredAmount = 1,
                CurrentAmount = 0,
                IsCompleted = false
            });
            
            quests.Add(QuestIds.SurvivorMessage, survivorMessageQuest);
            
            // Final Transmission Quest
            var finalTransmissionQuest = new Quest
            {
                Id = QuestIds.FinalTransmission,
                Title = "Final Transmission",
                Description = "Send the final transmission.",
                IsDiscovered = false,
                IsActive = false,
                IsCompleted = false
            };
            
            finalTransmissionQuest.Objectives.Add(new QuestObjective
            {
                Id = "obj_reach_radio_tower",
                Description = "Reach the radio tower",
                RequiredAmount = 1,
                CurrentAmount = 0,
                IsCompleted = false
            });
            
            finalTransmissionQuest.Objectives.Add(new QuestObjective
            {
                Id = "obj_send_transmission",
                Description = "Send the transmission",
                RequiredAmount = 1,
                CurrentAmount = 0,
                IsCompleted = false
            });
            
            quests.Add(QuestIds.FinalTransmission, finalTransmissionQuest);
            
            return quests;
        }
        
        /// <summary>
        /// Creates a set of test locations.
        /// </summary>
        /// <returns>A dictionary of test locations</returns>
        public static Dictionary<string, Location> CreateTestLocations()
        {
            var locations = new Dictionary<string, Location>();
            
            locations.Add(LocationIds.Bunker, new Location
            {
                Id = LocationIds.Bunker,
                Name = "Bunker",
                Description = "A small underground bunker.",
                IsDiscovered = true,
                Position = new Vector2(0, 0)
            });
            
            locations.Add(LocationIds.Forest, new Location
            {
                Id = LocationIds.Forest,
                Name = "Forest",
                Description = "A dense forest.",
                IsDiscovered = false,
                Position = new Vector2(-50, -30)
            });
            
            locations.Add(LocationIds.Town, new Location
            {
                Id = LocationIds.Town,
                Name = "Town",
                Description = "An abandoned town.",
                IsDiscovered = false,
                Position = new Vector2(30, -20)
            });
            
            locations.Add(LocationIds.Factory, new Location
            {
                Id = LocationIds.Factory,
                Name = "Factory",
                Description = "An old factory.",
                IsDiscovered = false,
                Position = new Vector2(70, 40)
            });
            
            locations.Add(LocationIds.RadioTower, new Location
            {
                Id = LocationIds.RadioTower,
                Name = "Radio Tower",
                Description = "A tall radio tower.",
                IsDiscovered = false,
                Position = new Vector2(-20, 60)
            });
            
            return locations;
        }
        
        /// <summary>
        /// Creates a set of test inventory items.
        /// </summary>
        /// <returns>A dictionary of test inventory items</returns>
        public static Dictionary<string, InventorySystem.ItemData> CreateTestItems()
        {
            var items = new Dictionary<string, InventorySystem.ItemData>();
            
            items.Add(ItemIds.Flashlight, new InventorySystem.ItemData
            {
                Id = ItemIds.Flashlight,
                Name = "Flashlight",
                Description = "A battery-powered flashlight. Essential for dark areas.",
                Category = "tool",
                IsUsable = true,
                IsConsumable = false,
                Quantity = 1,
                IconPath = "res://assets/images/items/flashlight.png"
            });
            
            items.Add(ItemIds.Battery, new InventorySystem.ItemData
            {
                Id = ItemIds.Battery,
                Name = "Battery",
                Description = "A standard AA battery. Can be used to power various devices.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                Quantity = 1,
                IconPath = "res://assets/images/items/battery.png"
            });
            
            items.Add(ItemIds.Medkit, new InventorySystem.ItemData
            {
                Id = ItemIds.Medkit,
                Name = "First Aid Kit",
                Description = "A basic first aid kit containing bandages and antiseptics.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                Quantity = 1,
                IconPath = "res://assets/images/items/medkit.png"
            });
            
            items.Add(ItemIds.KeyCabin, new InventorySystem.ItemData
            {
                Id = ItemIds.KeyCabin,
                Name = "Cabin Key",
                Description = "A rusty key that might open a cabin door.",
                Category = "key",
                IsUsable = true,
                IsConsumable = false,
                Quantity = 1,
                IconPath = "res://assets/images/items/key.png"
            });
            
            items.Add(ItemIds.KeyFactory, new InventorySystem.ItemData
            {
                Id = ItemIds.KeyFactory,
                Name = "Factory Key",
                Description = "A key that opens the factory gate.",
                Category = "key",
                IsUsable = true,
                IsConsumable = false,
                Quantity = 1,
                IconPath = "res://assets/images/items/key.png"
            });
            
            items.Add(ItemIds.MapFragment, new InventorySystem.ItemData
            {
                Id = ItemIds.MapFragment,
                Name = "Map Fragment",
                Description = "A torn piece of a map showing part of the area.",
                Category = "document",
                IsUsable = true,
                IsConsumable = false,
                Quantity = 1,
                IconPath = "res://assets/images/items/map_fragment.png"
            });
            
            items.Add(ItemIds.RadioPart, new InventorySystem.ItemData
            {
                Id = ItemIds.RadioPart,
                Name = "Radio Component",
                Description = "A component that could be used to repair or upgrade a radio.",
                Category = "tool",
                IsUsable = false,
                IsConsumable = false,
                Quantity = 1,
                IconPath = "res://assets/images/items/radio_part.png"
            });
            
            items.Add(ItemIds.WaterBottle, new InventorySystem.ItemData
            {
                Id = ItemIds.WaterBottle,
                Name = "Water Bottle",
                Description = "A bottle of clean drinking water.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                Quantity = 1,
                IconPath = "res://assets/images/items/water_bottle.png"
            });
            
            items.Add(ItemIds.CannedFood, new InventorySystem.ItemData
            {
                Id = ItemIds.CannedFood,
                Name = "Canned Food",
                Description = "A can of preserved food. Not tasty, but nutritious.",
                Category = "consumable",
                IsUsable = true,
                IsConsumable = true,
                Quantity = 1,
                IconPath = "res://assets/images/items/canned_food.png"
            });
            
            return items;
        }
    }
}

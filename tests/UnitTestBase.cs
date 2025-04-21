using Godot;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SignalLost.Tests
{
    /// <summary>
    /// Base class for unit tests in the Signal Lost game.
    /// Unit tests focus on testing individual components in isolation.
    /// </summary>
    [TestClass]
    public abstract partial class UnitTestBase : TestBase
    {
        /// <summary>
        /// Called before each test. Sets up the basic environment for unit tests.
        /// </summary>
        public override void Before()
        {
            base.Before();
            LogMessage($"Starting unit test: {GetType().Name}");
        }
        
        /// <summary>
        /// Called after each test. Cleans up the environment.
        /// </summary>
        public override void After()
        {
            LogMessage($"Finished unit test: {GetType().Name}");
            base.After();
        }
        
        /// <summary>
        /// Creates a mock GameState for testing.
        /// </summary>
        /// <returns>A new GameState instance</returns>
        protected GameState CreateMockGameState()
        {
            var gameState = new GameState();
            SafeAddChild(gameState);
            gameState._Ready();
            return gameState;
        }
        
        /// <summary>
        /// Creates a mock InventorySystem for testing.
        /// </summary>
        /// <param name="gameState">Optional GameState to connect to the InventorySystem</param>
        /// <returns>A new InventorySystem instance</returns>
        protected InventorySystem CreateMockInventorySystem(GameState gameState = null)
        {
            var inventorySystem = new InventorySystem();
            SafeAddChild(inventorySystem);
            
            if (gameState != null)
            {
                inventorySystem.SetGameState(gameState);
            }
            
            inventorySystem._Ready();
            return inventorySystem;
        }
        
        /// <summary>
        /// Creates a mock QuestSystem for testing.
        /// </summary>
        /// <returns>A new QuestSystem instance</returns>
        protected QuestSystem CreateMockQuestSystem()
        {
            var questSystem = new QuestSystem();
            SafeAddChild(questSystem);
            questSystem._Ready();
            return questSystem;
        }
        
        /// <summary>
        /// Creates a mock MapSystem for testing.
        /// </summary>
        /// <returns>A new MapSystem instance</returns>
        protected MapSystem CreateMockMapSystem()
        {
            var mapSystem = new MapSystem();
            SafeAddChild(mapSystem);
            mapSystem._Ready();
            return mapSystem;
        }
        
        /// <summary>
        /// Creates a mock MessageManager for testing.
        /// </summary>
        /// <returns>A new MessageManager instance</returns>
        protected MessageManager CreateMockMessageManager()
        {
            var messageManager = new MessageManager();
            SafeAddChild(messageManager);
            messageManager._Ready();
            return messageManager;
        }
        
        /// <summary>
        /// Creates a mock AudioManager for testing.
        /// </summary>
        /// <returns>A new AudioManager instance</returns>
        protected AudioManager CreateMockAudioManager()
        {
            var audioManager = new AudioManager();
            SafeAddChild(audioManager);
            
            // Set up audio buses for testing
            SetupAudioBuses();
            
            audioManager._Ready();
            return audioManager;
        }
        
        /// <summary>
        /// Creates a mock GameProgressionManager for testing.
        /// </summary>
        /// <returns>A new GameProgressionManager instance</returns>
        protected GameProgressionManager CreateMockGameProgressionManager()
        {
            var progressionManager = new GameProgressionManager();
            SafeAddChild(progressionManager);
            progressionManager._Ready();
            return progressionManager;
        }
        
        /// <summary>
        /// Sets up audio buses for testing.
        /// </summary>
        protected static void SetupAudioBuses()
        {
            // Make sure we have the Master bus
            if (AudioServer.GetBusCount() == 0)
            {
                AudioServer.AddBus();
                AudioServer.SetBusName(0, "Master");
            }

            // Create Static bus if it doesn't exist
            int staticBusIdx = AudioServer.GetBusIndex("Static");
            if (staticBusIdx == -1)
            {
                AudioServer.AddBus();
                staticBusIdx = AudioServer.GetBusCount() - 1;
                AudioServer.SetBusName(staticBusIdx, "Static");
                AudioServer.SetBusSend(staticBusIdx, "Master");
            }

            // Create Signal bus if it doesn't exist
            int signalBusIdx = AudioServer.GetBusIndex("Signal");
            if (signalBusIdx == -1)
            {
                AudioServer.AddBus();
                signalBusIdx = AudioServer.GetBusCount() - 1;
                AudioServer.SetBusName(signalBusIdx, "Signal");
                AudioServer.SetBusSend(signalBusIdx, "Master");
            }
        }
    }
}

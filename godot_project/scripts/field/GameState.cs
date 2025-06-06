using Godot;

namespace SignalLost.Field
{
    /// <summary>
    /// Stub implementation of the GameState class for the field exploration system.
    /// </summary>
    [GlobalClass]
    public partial class GameState : Node
    {
        // Singleton instance
        private static GameState _instance;

        // Systems
        private RadioSystem _radioSystem;
        private InventorySystem _inventorySystem;
        private QuestSystem _questSystem;

        // Radio state
        public bool IsRadioOn { get; private set; } = false;
        public float CurrentFrequency { get; private set; } = 90.0f;

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            _instance = this;

            // Initialize systems
            _radioSystem = new RadioSystem();
            AddChild(_radioSystem);

            _inventorySystem = new InventorySystem();
            AddChild(_inventorySystem);

            _questSystem = new QuestSystem();
            AddChild(_questSystem);
        }

        /// <summary>
        /// Gets the singleton instance of the GameState.
        /// </summary>
        public static GameState Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameState();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the radio system.
        /// </summary>
        public RadioSystem GetRadioSystem()
        {
            return _radioSystem;
        }

        /// <summary>
        /// Gets the inventory system.
        /// </summary>
        public InventorySystem GetInventorySystem()
        {
            return _inventorySystem;
        }

        /// <summary>
        /// Gets the quest system.
        /// </summary>
        public QuestSystem GetQuestSystem()
        {
            return _questSystem;
        }

        /// <summary>
        /// Toggles the radio power state.
        /// </summary>
        public void ToggleRadio()
        {
            IsRadioOn = !IsRadioOn;
            GD.Print($"GameState: Radio power toggled to {IsRadioOn}");
        }

        /// <summary>
        /// Sets the radio frequency.
        /// </summary>
        /// <param name="frequency">The new frequency</param>
        public void SetFrequency(float frequency)
        {
            CurrentFrequency = Mathf.Clamp(frequency, 88.0f, 108.0f);
            GD.Print($"GameState: Frequency set to {CurrentFrequency} MHz");
        }
    }
}

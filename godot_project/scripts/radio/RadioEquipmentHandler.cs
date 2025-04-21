using Godot;
using System.Collections.Generic;

namespace SignalLost.Radio
{
    /// <summary>
    /// Handles the integration between radio equipment and the signal system.
    /// </summary>
    [GlobalClass]
    public partial class RadioEquipmentHandler : Node
    {
        // Reference to the radio signals manager
        private RadioSignalsManager _radioSignalsManager;
        
        // Reference to the game state
        private GameState _gameState;
        
        // Equipment effects
        private Dictionary<string, EquipmentEffect> _equipmentEffects = new Dictionary<string, EquipmentEffect>
        {
            { "radio", new EquipmentEffect { SignalBoost = 1.0f, CanDetectHiddenSignals = false } },
            { "radio_enhanced", new EquipmentEffect { SignalBoost = 2.0f, CanDetectHiddenSignals = false } },
            { "strange_crystal", new EquipmentEffect { SignalBoost = 1.5f, CanDetectHiddenSignals = true } }
        };
        
        // Called when the node enters the scene tree for the first time
        public override void _Ready()
        {
            // Get references to systems
            _radioSignalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");
            _gameState = GetNode<GameState>("/root/GameState");
            
            if (_radioSignalsManager == null)
            {
                GD.PrintErr("RadioEquipmentHandler: RadioSignalsManager not found");
                return;
            }
            
            if (_gameState == null)
            {
                GD.PrintErr("RadioEquipmentHandler: GameState not found");
                return;
            }
            
            // Connect to GameState signals
            _gameState.InventoryChanged += OnInventoryChanged;
            
            // Initialize
            UpdateEquipmentEffects();
        }
        
        // Called when the node is about to be removed from the scene tree
        public override void _ExitTree()
        {
            // Disconnect signals
            if (_gameState != null)
            {
                _gameState.InventoryChanged -= OnInventoryChanged;
            }
        }
        
        // Handle inventory changed
        private void OnInventoryChanged()
        {
            // Update equipment effects based on inventory
            UpdateEquipmentEffects();
        }
        
        // Update equipment effects based on inventory
        private void UpdateEquipmentEffects()
        {
            float signalBoost = 1.0f;
            bool canDetectHiddenSignals = false;
            
            // Check for radio equipment in inventory
            foreach (var itemId in _gameState.Inventory)
            {
                if (_equipmentEffects.ContainsKey(itemId))
                {
                    // Apply equipment effects
                    signalBoost *= _equipmentEffects[itemId].SignalBoost;
                    canDetectHiddenSignals |= _equipmentEffects[itemId].CanDetectHiddenSignals;
                }
            }
            
            // Update radio signals manager
            _radioSignalsManager.SetSignalBoost(signalBoost);
            _radioSignalsManager.EnableHiddenSignalDetection(canDetectHiddenSignals);
            
            GD.Print($"Equipment effects updated: Boost = {signalBoost}, Hidden Signals = {canDetectHiddenSignals}");
        }
        
        // Register a new equipment effect
        public void RegisterEquipmentEffect(string itemId, float signalBoost, bool canDetectHiddenSignals)
        {
            _equipmentEffects[itemId] = new EquipmentEffect
            {
                SignalBoost = signalBoost,
                CanDetectHiddenSignals = canDetectHiddenSignals
            };
            
            // Update equipment effects
            UpdateEquipmentEffects();
        }
    }
    
    /// <summary>
    /// Equipment effect data.
    /// </summary>
    public class EquipmentEffect
    {
        public float SignalBoost { get; set; } = 1.0f;
        public bool CanDetectHiddenSignals { get; set; } = false;
    }
}

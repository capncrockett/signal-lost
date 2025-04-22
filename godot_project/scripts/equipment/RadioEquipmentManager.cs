using Godot;
using System;
using System.Collections.Generic;
using SignalLost.Equipment;

namespace SignalLost.Radio
{
    /// <summary>
    /// Manages radio equipment and its effects on radio functionality.
    /// </summary>
    [GlobalClass]
    public partial class RadioEquipmentManager : Node
    {
        // Radio equipment types
        public enum RadioType
        {
            Basic,      // Basic radio with limited functionality
            Advanced,   // Advanced radio with better reception and features
            Military,   // Military-grade radio with encryption capabilities
            Custom      // Custom-built radio with unique features
        }

        // Radio component types
        public enum ComponentType
        {
            Antenna,    // Affects signal range and reception
            Tuner,      // Affects frequency precision and tuning speed
            Amplifier,  // Affects signal strength and clarity
            Battery,    // Affects power consumption and operation time
            Decoder     // Affects ability to decode encrypted signals
        }

        // Radio equipment data
        private RadioType _currentRadioType = RadioType.Basic;
        private Dictionary<ComponentType, int> _componentLevels = new Dictionary<ComponentType, int>();
        private Dictionary<string, Dictionary<string, float>> _radioEffects = new Dictionary<string, Dictionary<string, float>>();

        // References to other systems
        private EquipmentSystem _equipmentSystem;
        private RadioSignalsManager _radioSignalsManager;
        private SignalLost.GameState _gameState;

        // Signals
        [Signal] public delegate void RadioEquipmentChangedEventHandler(RadioType radioType);
        [Signal] public delegate void ComponentUpgradedEventHandler(ComponentType componentType, int level);
        [Signal] public delegate void RadioEffectsUpdatedEventHandler();

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references to other systems
            _equipmentSystem = GetNode<EquipmentSystem>("/root/EquipmentSystem");
            _radioSignalsManager = GetNode<RadioSignalsManager>("/root/RadioSignalsManager");
            _gameState = GetNode<SignalLost.GameState>("/root/GameState");

            // Connect to equipment system signals
            if (_equipmentSystem != null)
            {
                _equipmentSystem.ItemEquipped += OnItemEquipped;
                _equipmentSystem.ItemUnequipped += OnItemUnequipped;
                _equipmentSystem.EquipmentEffectsChanged += OnEquipmentEffectsChanged;
            }

            // Initialize component levels
            InitializeComponentLevels();

            // Initialize radio effects
            InitializeRadioEffects();

            GD.Print("RadioEquipmentManager: Initialized");
        }

        /// <summary>
        /// Initializes component levels to default values.
        /// </summary>
        private void InitializeComponentLevels()
        {
            _componentLevels[ComponentType.Antenna] = 1;
            _componentLevels[ComponentType.Tuner] = 1;
            _componentLevels[ComponentType.Amplifier] = 1;
            _componentLevels[ComponentType.Battery] = 1;
            _componentLevels[ComponentType.Decoder] = 0; // Decoder starts at level 0 (not available)
        }

        /// <summary>
        /// Initializes radio effects for different radio types.
        /// </summary>
        private void InitializeRadioEffects()
        {
            // Basic radio effects
            _radioEffects["radio_basic"] = new Dictionary<string, float>
            {
                { "signal_range", 1.0f },
                { "frequency_precision", 1.0f },
                { "signal_clarity", 1.0f },
                { "battery_consumption", 1.0f },
                { "tuning_speed", 1.0f }
            };

            // Advanced radio effects
            _radioEffects["radio_advanced"] = new Dictionary<string, float>
            {
                { "signal_range", 1.5f },
                { "frequency_precision", 1.2f },
                { "signal_clarity", 1.3f },
                { "battery_consumption", 0.8f },
                { "tuning_speed", 1.2f },
                { "signal_boost", 1.2f }
            };

            // Military radio effects
            _radioEffects["radio_military"] = new Dictionary<string, float>
            {
                { "signal_range", 2.0f },
                { "frequency_precision", 1.5f },
                { "signal_clarity", 1.5f },
                { "battery_consumption", 0.6f },
                { "tuning_speed", 1.5f },
                { "signal_boost", 1.5f },
                { "decode_encrypted", 1.0f }
            };

            // Custom radio effects (initialized with basic values, can be upgraded)
            _radioEffects["radio_custom"] = new Dictionary<string, float>
            {
                { "signal_range", 1.0f },
                { "frequency_precision", 1.0f },
                { "signal_clarity", 1.0f },
                { "battery_consumption", 1.0f },
                { "tuning_speed", 1.0f }
            };
        }

        /// <summary>
        /// Updates radio effects based on equipped items and component levels.
        /// </summary>
        private void UpdateRadioEffects()
        {
            // Get the equipped radio
            string radioItemId = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Radio);
            
            // If no radio is equipped, use default values
            if (string.IsNullOrEmpty(radioItemId))
            {
                // Apply default effects
                ApplyRadioEffects(new Dictionary<string, float>
                {
                    { "signal_range", 1.0f },
                    { "frequency_precision", 1.0f },
                    { "signal_clarity", 1.0f },
                    { "battery_consumption", 1.0f },
                    { "tuning_speed", 1.0f }
                });
                return;
            }

            // Determine radio type
            if (radioItemId == "radio_basic")
            {
                _currentRadioType = RadioType.Basic;
            }
            else if (radioItemId == "radio_advanced")
            {
                _currentRadioType = RadioType.Advanced;
            }
            else if (radioItemId == "radio_military")
            {
                _currentRadioType = RadioType.Military;
            }
            else if (radioItemId == "radio_custom")
            {
                _currentRadioType = RadioType.Custom;
            }

            // Get base radio effects
            Dictionary<string, float> baseEffects = new Dictionary<string, float>();
            if (_radioEffects.ContainsKey(radioItemId))
            {
                baseEffects = new Dictionary<string, float>(_radioEffects[radioItemId]);
            }

            // Apply component level modifiers for custom radio
            if (_currentRadioType == RadioType.Custom)
            {
                // Antenna affects signal range
                baseEffects["signal_range"] = 1.0f + (_componentLevels[ComponentType.Antenna] * 0.2f);
                
                // Tuner affects frequency precision and tuning speed
                baseEffects["frequency_precision"] = 1.0f + (_componentLevels[ComponentType.Tuner] * 0.1f);
                baseEffects["tuning_speed"] = 1.0f + (_componentLevels[ComponentType.Tuner] * 0.1f);
                
                // Amplifier affects signal clarity and signal boost
                baseEffects["signal_clarity"] = 1.0f + (_componentLevels[ComponentType.Amplifier] * 0.1f);
                baseEffects["signal_boost"] = 1.0f + (_componentLevels[ComponentType.Amplifier] * 0.1f);
                
                // Battery affects power consumption
                baseEffects["battery_consumption"] = 1.0f - (_componentLevels[ComponentType.Battery] * 0.1f);
                
                // Decoder affects ability to decode encrypted signals
                if (_componentLevels[ComponentType.Decoder] > 0)
                {
                    baseEffects["decode_encrypted"] = _componentLevels[ComponentType.Decoder] * 0.5f;
                }
            }

            // Apply effects from other equipped items
            Dictionary<string, float> totalEffects = new Dictionary<string, float>(baseEffects);
            
            // Check for headphones
            string headphonesItemId = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Head);
            if (!string.IsNullOrEmpty(headphonesItemId))
            {
                Dictionary<string, float> headphonesEffects = _equipmentSystem.GetItemEffects(headphonesItemId);
                foreach (var effect in headphonesEffects)
                {
                    if (totalEffects.ContainsKey(effect.Key))
                    {
                        totalEffects[effect.Key] *= effect.Value;
                    }
                    else
                    {
                        totalEffects[effect.Key] = effect.Value;
                    }
                }
            }
            
            // Check for accessories
            string accessory1ItemId = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Accessory1);
            if (!string.IsNullOrEmpty(accessory1ItemId))
            {
                Dictionary<string, float> accessoryEffects = _equipmentSystem.GetItemEffects(accessory1ItemId);
                foreach (var effect in accessoryEffects)
                {
                    if (totalEffects.ContainsKey(effect.Key))
                    {
                        totalEffects[effect.Key] *= effect.Value;
                    }
                    else
                    {
                        totalEffects[effect.Key] = effect.Value;
                    }
                }
            }
            
            string accessory2ItemId = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Accessory2);
            if (!string.IsNullOrEmpty(accessory2ItemId))
            {
                Dictionary<string, float> accessoryEffects = _equipmentSystem.GetItemEffects(accessory2ItemId);
                foreach (var effect in accessoryEffects)
                {
                    if (totalEffects.ContainsKey(effect.Key))
                    {
                        totalEffects[effect.Key] *= effect.Value;
                    }
                    else
                    {
                        totalEffects[effect.Key] = effect.Value;
                    }
                }
            }

            // Apply the total effects
            ApplyRadioEffects(totalEffects);
        }

        /// <summary>
        /// Applies radio effects to the radio system.
        /// </summary>
        /// <param name="effects">The effects to apply</param>
        private void ApplyRadioEffects(Dictionary<string, float> effects)
        {
            if (_radioSignalsManager == null)
                return;

            // Apply signal range effect
            if (effects.ContainsKey("signal_range"))
            {
                _radioSignalsManager.SetSignalRangeMultiplier(effects["signal_range"]);
            }

            // Apply signal boost effect
            if (effects.ContainsKey("signal_boost"))
            {
                _radioSignalsManager.SetSignalBoost(effects["signal_boost"]);
            }

            // Apply frequency precision effect
            if (effects.ContainsKey("frequency_precision"))
            {
                _radioSignalsManager.SetFrequencyPrecision(effects["frequency_precision"]);
            }

            // Apply decode encrypted effect
            bool canDecodeEncrypted = effects.ContainsKey("decode_encrypted") && effects["decode_encrypted"] > 0.5f;
            _radioSignalsManager.EnableEncryptedSignalDecoding(canDecodeEncrypted);

            // Apply detect hidden signals effect
            bool canDetectHiddenSignals = effects.ContainsKey("detect_hidden_signals") && effects["detect_hidden_signals"] > 0.5f;
            _radioSignalsManager.EnableHiddenSignalDetection(canDetectHiddenSignals);

            // Emit signal
            EmitSignal(SignalName.RadioEffectsUpdated);

            GD.Print("RadioEquipmentManager: Applied radio effects");
        }

        /// <summary>
        /// Handles item equipped event.
        /// </summary>
        /// <param name="itemId">The ID of the equipped item</param>
        /// <param name="slotIndex">The index of the slot the item was equipped to</param>
        private void OnItemEquipped(string itemId, int slotIndex)
        {
            // Convert slot index to EquipmentSlot enum
            EquipmentSystem.EquipmentSlot slot = (EquipmentSystem.EquipmentSlot)slotIndex;

            // If a radio was equipped, update radio type
            if (slot == EquipmentSystem.EquipmentSlot.Radio)
            {
                if (itemId == "radio_basic")
                {
                    _currentRadioType = RadioType.Basic;
                }
                else if (itemId == "radio_advanced")
                {
                    _currentRadioType = RadioType.Advanced;
                }
                else if (itemId == "radio_military")
                {
                    _currentRadioType = RadioType.Military;
                }
                else if (itemId == "radio_custom")
                {
                    _currentRadioType = RadioType.Custom;
                }

                // Emit signal
                EmitSignal(SignalName.RadioEquipmentChanged, (int)_currentRadioType);
            }

            // Update radio effects
            UpdateRadioEffects();
        }

        /// <summary>
        /// Handles item unequipped event.
        /// </summary>
        /// <param name="itemId">The ID of the unequipped item</param>
        /// <param name="slotIndex">The index of the slot the item was unequipped from</param>
        private void OnItemUnequipped(string itemId, int slotIndex)
        {
            // Convert slot index to EquipmentSlot enum
            EquipmentSystem.EquipmentSlot slot = (EquipmentSystem.EquipmentSlot)slotIndex;

            // If a radio was unequipped, reset radio type
            if (slot == EquipmentSystem.EquipmentSlot.Radio)
            {
                _currentRadioType = RadioType.Basic;
                
                // Emit signal
                EmitSignal(SignalName.RadioEquipmentChanged, (int)_currentRadioType);
            }

            // Update radio effects
            UpdateRadioEffects();
        }

        /// <summary>
        /// Handles equipment effects changed event.
        /// </summary>
        private void OnEquipmentEffectsChanged()
        {
            // Update radio effects
            UpdateRadioEffects();
        }

        /// <summary>
        /// Upgrades a radio component to the next level.
        /// </summary>
        /// <param name="componentType">The type of component to upgrade</param>
        /// <returns>True if the component was upgraded successfully, false otherwise</returns>
        public bool UpgradeComponent(ComponentType componentType)
        {
            // Check if the component is already at max level
            int currentLevel = _componentLevels[componentType];
            int maxLevel = GetMaxComponentLevel(componentType);
            
            if (currentLevel >= maxLevel)
            {
                GD.PrintErr($"RadioEquipmentManager: Component {componentType} is already at max level");
                return false;
            }

            // Upgrade the component
            _componentLevels[componentType]++;

            // Update radio effects if using a custom radio
            if (_currentRadioType == RadioType.Custom)
            {
                UpdateRadioEffects();
            }

            // Emit signal
            EmitSignal(SignalName.ComponentUpgraded, (int)componentType, _componentLevels[componentType]);

            GD.Print($"RadioEquipmentManager: Upgraded {componentType} to level {_componentLevels[componentType]}");

            return true;
        }

        /// <summary>
        /// Gets the current level of a radio component.
        /// </summary>
        /// <param name="componentType">The type of component to check</param>
        /// <returns>The current level of the component</returns>
        public int GetComponentLevel(ComponentType componentType)
        {
            return _componentLevels[componentType];
        }

        /// <summary>
        /// Gets the maximum level for a radio component.
        /// </summary>
        /// <param name="componentType">The type of component to check</param>
        /// <returns>The maximum level for the component</returns>
        public int GetMaxComponentLevel(ComponentType componentType)
        {
            switch (componentType)
            {
                case ComponentType.Antenna:
                    return 5;
                case ComponentType.Tuner:
                    return 5;
                case ComponentType.Amplifier:
                    return 5;
                case ComponentType.Battery:
                    return 3;
                case ComponentType.Decoder:
                    return 3;
                default:
                    return 1;
            }
        }

        /// <summary>
        /// Gets the current radio type.
        /// </summary>
        /// <returns>The current radio type</returns>
        public RadioType GetCurrentRadioType()
        {
            return _currentRadioType;
        }

        /// <summary>
        /// Gets the effects of the current radio setup.
        /// </summary>
        /// <returns>A dictionary mapping effect names to values</returns>
        public Dictionary<string, float> GetCurrentRadioEffects()
        {
            // Get the equipped radio
            string radioItemId = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Radio);
            
            // If no radio is equipped, return empty dictionary
            if (string.IsNullOrEmpty(radioItemId))
            {
                return new Dictionary<string, float>();
            }

            // Get base radio effects
            Dictionary<string, float> baseEffects = new Dictionary<string, float>();
            if (_radioEffects.ContainsKey(radioItemId))
            {
                baseEffects = new Dictionary<string, float>(_radioEffects[radioItemId]);
            }

            // Apply component level modifiers for custom radio
            if (_currentRadioType == RadioType.Custom)
            {
                // Antenna affects signal range
                baseEffects["signal_range"] = 1.0f + (_componentLevels[ComponentType.Antenna] * 0.2f);
                
                // Tuner affects frequency precision and tuning speed
                baseEffects["frequency_precision"] = 1.0f + (_componentLevels[ComponentType.Tuner] * 0.1f);
                baseEffects["tuning_speed"] = 1.0f + (_componentLevels[ComponentType.Tuner] * 0.1f);
                
                // Amplifier affects signal clarity and signal boost
                baseEffects["signal_clarity"] = 1.0f + (_componentLevels[ComponentType.Amplifier] * 0.1f);
                baseEffects["signal_boost"] = 1.0f + (_componentLevels[ComponentType.Amplifier] * 0.1f);
                
                // Battery affects power consumption
                baseEffects["battery_consumption"] = 1.0f - (_componentLevels[ComponentType.Battery] * 0.1f);
                
                // Decoder affects ability to decode encrypted signals
                if (_componentLevels[ComponentType.Decoder] > 0)
                {
                    baseEffects["decode_encrypted"] = _componentLevels[ComponentType.Decoder] * 0.5f;
                }
            }

            return baseEffects;
        }

        /// <summary>
        /// Creates a custom radio from components.
        /// </summary>
        /// <returns>True if the custom radio was created successfully, false otherwise</returns>
        public bool CreateCustomRadio()
        {
            // Check if the player has the necessary items
            if (!_gameState.HasItem("radio_broken") || !_gameState.HasItem("radio_part"))
            {
                GD.PrintErr("RadioEquipmentManager: Missing required items to create custom radio");
                return false;
            }

            // Remove the items from inventory
            _gameState.RemoveFromInventory("radio_broken");
            _gameState.RemoveFromInventory("radio_part");

            // Add the custom radio to inventory
            _gameState.AddToInventory("radio_custom");

            GD.Print("RadioEquipmentManager: Created custom radio");

            return true;
        }

        /// <summary>
        /// Checks if the player can create a custom radio.
        /// </summary>
        /// <returns>True if the player can create a custom radio, false otherwise</returns>
        public bool CanCreateCustomRadio()
        {
            return _gameState.HasItem("radio_broken") && _gameState.HasItem("radio_part");
        }
    }
}

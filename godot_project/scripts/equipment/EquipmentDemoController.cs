using Godot;
using System;
using System.Collections.Generic;
using SignalLost.Equipment;
using SignalLost.Inventory;
using SignalLost.Radio;

namespace SignalLost.Equipment
{
    /// <summary>
    /// Controller for the equipment demo scene.
    /// </summary>
    [GlobalClass]
    public partial class EquipmentDemoController : Control
    {
        // References to UI elements
        private Button _addHeadphonesBasicButton;
        private Button _addHeadphonesAdvancedButton;
        private Button _addRadioBasicButton;
        private Button _addRadioAdvancedButton;
        private Button _addRadioMilitaryButton;
        private Button _addRadioBrokenButton;
        private Button _addRadioPartButton;
        private Button _createCustomRadioButton;
        private Button _addSignalAnalyzerButton;
        private Button _addSignalBoosterButton;
        private Button _addCompassButton;
        private Button _addWeatherGearButton;
        private Button _addRadiationSuitButton;
        private Button _addStrangeCrystalButton;
        private Button _showEquipmentButton;
        private RichTextLabel _logText;
        private Button _clearLogButton;
        private Control _equipmentUI;

        // References to systems
        private EquipmentSystem _equipmentSystem;
        private RadioEquipmentManager _radioEquipmentManager;
        private InventorySystemAdapter _inventorySystem;

        /// <summary>
        /// Called when the node enters the scene tree.
        /// </summary>
        public override void _Ready()
        {
            // Get references to UI elements
            _addHeadphonesBasicButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddHeadphonesBasicButton");
            _addHeadphonesAdvancedButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddHeadphonesAdvancedButton");
            _addRadioBasicButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddRadioBasicButton");
            _addRadioAdvancedButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddRadioAdvancedButton");
            _addRadioMilitaryButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddRadioMilitaryButton");
            _addRadioBrokenButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddRadioBrokenButton");
            _addRadioPartButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddRadioPartButton");
            _createCustomRadioButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/CreateCustomRadioButton");
            _addSignalAnalyzerButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddSignalAnalyzerButton");
            _addSignalBoosterButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddSignalBoosterButton");
            _addCompassButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddCompassButton");
            _addWeatherGearButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddWeatherGearButton");
            _addRadiationSuitButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddRadiationSuitButton");
            _addStrangeCrystalButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/AddStrangeCrystalButton");
            _showEquipmentButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/ItemButtons/ShowEquipmentButton");
            _logText = GetNode<RichTextLabel>("MarginContainer/VBoxContainer/HBoxContainer/LogPanel/MarginContainer/VBoxContainer/LogText");
            _clearLogButton = GetNode<Button>("MarginContainer/VBoxContainer/HBoxContainer/LogPanel/MarginContainer/VBoxContainer/ClearLogButton");
            _equipmentUI = GetNode<Control>("EquipmentUI");

            // Get references to systems
            _equipmentSystem = GetNode<EquipmentSystem>("/root/EquipmentSystem");
            _radioEquipmentManager = GetNode<RadioEquipmentManager>("/root/RadioEquipmentManager");
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");

            // Connect signals
            _addHeadphonesBasicButton.Pressed += () => AddItem("headphones_basic");
            _addHeadphonesAdvancedButton.Pressed += () => AddItem("headphones_advanced");
            _addRadioBasicButton.Pressed += () => AddItem("radio_basic");
            _addRadioAdvancedButton.Pressed += () => AddItem("radio_advanced");
            _addRadioMilitaryButton.Pressed += () => AddItem("radio_military");
            _addRadioBrokenButton.Pressed += () => AddItem("radio_broken");
            _addRadioPartButton.Pressed += () => AddItem("radio_part");
            _createCustomRadioButton.Pressed += CreateCustomRadio;
            _addSignalAnalyzerButton.Pressed += () => AddItem("signal_analyzer");
            _addSignalBoosterButton.Pressed += () => AddItem("signal_booster");
            _addCompassButton.Pressed += () => AddItem("compass");
            _addWeatherGearButton.Pressed += () => AddItem("weather_gear");
            _addRadiationSuitButton.Pressed += () => AddItem("radiation_suit");
            _addStrangeCrystalButton.Pressed += () => AddItem("strange_crystal");
            _showEquipmentButton.Pressed += ToggleEquipmentUI;
            _clearLogButton.Pressed += ClearLog;

            // Connect system signals
            if (_equipmentSystem != null)
            {
                _equipmentSystem.ItemEquipped += OnItemEquipped;
                _equipmentSystem.ItemUnequipped += OnItemUnequipped;
                _equipmentSystem.EquipmentEffectsChanged += OnEquipmentEffectsChanged;
            }

            if (_radioEquipmentManager != null)
            {
                _radioEquipmentManager.ComponentUpgraded += OnComponentUpgraded;
                _radioEquipmentManager.RadioEquipmentChanged += OnRadioEquipmentChanged;
                _radioEquipmentManager.RadioEffectsUpdated += OnRadioEffectsUpdated;
            }

            // Initialize
            LogMessage("Equipment Demo initialized");
            LogMessage("Click the buttons to add equipment to your inventory");
            LogMessage("Click 'Show Equipment' to open the equipment UI");

            // Register equipment items in the inventory system
            RegisterEquipmentItems();
        }

        /// <summary>
        /// Called when the node is about to be removed from the scene tree.
        /// </summary>
        public override void _ExitTree()
        {
            // Disconnect system signals
            if (_equipmentSystem != null)
            {
                _equipmentSystem.ItemEquipped -= OnItemEquipped;
                _equipmentSystem.ItemUnequipped -= OnItemUnequipped;
                _equipmentSystem.EquipmentEffectsChanged -= OnEquipmentEffectsChanged;
            }

            if (_radioEquipmentManager != null)
            {
                _radioEquipmentManager.ComponentUpgraded -= OnComponentUpgraded;
                _radioEquipmentManager.RadioEquipmentChanged -= OnRadioEquipmentChanged;
                _radioEquipmentManager.RadioEffectsUpdated -= OnRadioEffectsUpdated;
            }
        }

        /// <summary>
        /// Registers equipment items in the inventory system.
        /// </summary>
        private void RegisterEquipmentItems()
        {
            // Basic Headphones
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "headphones_basic",
                Name = "Basic Headphones",
                Description = "Standard headphones for listening to radio signals.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/headphones_basic.png",
                IconIndex = 10
            });

            // Advanced Headphones
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "headphones_advanced",
                Name = "Advanced Headphones",
                Description = "High-quality headphones with noise cancellation for better signal clarity.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/headphones_advanced.png",
                IconIndex = 11
            });

            // Basic Radio
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "radio_basic",
                Name = "Basic Radio",
                Description = "A standard radio receiver capable of picking up common frequencies.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/radio_basic.png",
                IconIndex = 12
            });

            // Advanced Radio
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "radio_advanced",
                Name = "Advanced Radio",
                Description = "A high-quality radio with better reception and frequency precision.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/radio_advanced.png",
                IconIndex = 13
            });

            // Military Radio
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "radio_military",
                Name = "Military Radio",
                Description = "A military-grade radio capable of decoding encrypted signals.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/radio_military.png",
                IconIndex = 14
            });

            // Broken Radio
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "radio_broken",
                Name = "Broken Radio",
                Description = "A broken radio that could be repaired with the right parts.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/radio_broken.png",
                IconIndex = 15
            });

            // Radio Part
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "radio_part",
                Name = "Radio Part",
                Description = "A component that can be used to repair a broken radio.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = false,
                IconPath = "res://assets/images/items/radio_part.png",
                IconIndex = 16
            });

            // Custom Radio
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "radio_custom",
                Name = "Custom Radio",
                Description = "A custom-built radio that can be upgraded with various components.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/radio_custom.png",
                IconIndex = 17
            });

            // Signal Analyzer
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "signal_analyzer",
                Name = "Signal Analyzer",
                Description = "A device that can analyze radio signals and detect hidden frequencies.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/signal_analyzer.png",
                IconIndex = 18
            });

            // Signal Booster
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "signal_booster",
                Name = "Signal Booster",
                Description = "A device that amplifies radio signals for better reception.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/signal_booster.png",
                IconIndex = 19
            });

            // Compass
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "compass",
                Name = "Compass",
                Description = "A compass that helps with navigation and finding signal sources.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/compass.png",
                IconIndex = 20
            });

            // Weather Gear
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "weather_gear",
                Name = "Weather Gear",
                Description = "Protective clothing that shields against harsh weather conditions.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/weather_gear.png",
                IconIndex = 21
            });

            // Radiation Suit
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "radiation_suit",
                Name = "Radiation Suit",
                Description = "A suit that protects against radiation but reduces mobility.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/radiation_suit.png",
                IconIndex = 22
            });

            // Strange Crystal
            _inventorySystem.AddItemToDatabase(new ItemData
            {
                Id = "strange_crystal",
                Name = "Strange Crystal",
                Description = "A mysterious crystal that resonates with radio frequencies and reveals hidden signals.",
                Category = "equipment",
                IsUsable = false,
                IsConsumable = false,
                IsEquippable = true,
                IconPath = "res://assets/images/items/strange_crystal.png",
                IconIndex = 23
            });
        }

        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
        /// <param name="itemId">The ID of the item to add</param>
        private void AddItem(string itemId)
        {
            bool result = _inventorySystem.AddItemToInventory(itemId);
            if (result)
            {
                var item = _inventorySystem.GetItem(itemId);
                LogMessage($"Added {item.Name} to inventory");
            }
            else
            {
                LogMessage($"Failed to add {itemId} to inventory", Colors.Red);
            }
        }

        /// <summary>
        /// Creates a custom radio from components.
        /// </summary>
        private void CreateCustomRadio()
        {
            if (_radioEquipmentManager.CanCreateCustomRadio())
            {
                bool result = _radioEquipmentManager.CreateCustomRadio();
                if (result)
                {
                    LogMessage("Created custom radio from broken radio and radio part", Colors.Green);
                }
                else
                {
                    LogMessage("Failed to create custom radio", Colors.Red);
                }
            }
            else
            {
                LogMessage("You need a broken radio and a radio part to create a custom radio", Colors.Yellow);
            }
        }

        /// <summary>
        /// Toggles the equipment UI.
        /// </summary>
        private void ToggleEquipmentUI()
        {
            _equipmentUI.Visible = !_equipmentUI.Visible;
            if (_equipmentUI.Visible)
            {
                LogMessage("Opened equipment UI");
            }
            else
            {
                LogMessage("Closed equipment UI");
            }
        }

        /// <summary>
        /// Clears the log.
        /// </summary>
        private void ClearLog()
        {
            _logText.Clear();
            LogMessage("Log cleared");
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="color">The color of the message</param>
        private void LogMessage(string message, Color color = default)
        {
            if (color == default)
            {
                color = Colors.White;
            }

            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string colorHex = color.ToHtml();
            _logText.AppendText($"[{timestamp}] [color=#{colorHex}]{message}[/color]\n");
        }

        /// <summary>
        /// Handles an item being equipped.
        /// </summary>
        /// <param name="itemId">The ID of the equipped item</param>
        /// <param name="slotIndex">The index of the slot the item was equipped to</param>
        private void OnItemEquipped(string itemId, int slotIndex)
        {
            EquipmentSystem.EquipmentSlot slot = (EquipmentSystem.EquipmentSlot)slotIndex;
            var item = _inventorySystem.GetItem(itemId);
            LogMessage($"Equipped {item.Name} to {slot}", Colors.Green);
        }

        /// <summary>
        /// Handles an item being unequipped.
        /// </summary>
        /// <param name="itemId">The ID of the unequipped item</param>
        /// <param name="slotIndex">The index of the slot the item was unequipped from</param>
        private void OnItemUnequipped(string itemId, int slotIndex)
        {
            EquipmentSystem.EquipmentSlot slot = (EquipmentSystem.EquipmentSlot)slotIndex;
            var item = _inventorySystem.GetItem(itemId);
            LogMessage($"Unequipped {item.Name} from {slot}", Colors.Yellow);
        }

        /// <summary>
        /// Handles equipment effects changing.
        /// </summary>
        private void OnEquipmentEffectsChanged()
        {
            LogMessage("Equipment effects changed", Colors.Cyan);
        }

        /// <summary>
        /// Handles a component being upgraded.
        /// </summary>
        /// <param name="componentTypeIndex">The index of the component type that was upgraded</param>
        /// <param name="level">The new level of the component</param>
        private void OnComponentUpgraded(int componentTypeIndex, int level)
        {
            RadioEquipmentManager.ComponentType componentType = (RadioEquipmentManager.ComponentType)componentTypeIndex;
            LogMessage($"Upgraded {componentType} to level {level}", Colors.Green);
        }

        /// <summary>
        /// Handles the radio equipment changing.
        /// </summary>
        /// <param name="radioTypeIndex">The index of the new radio type</param>
        private void OnRadioEquipmentChanged(int radioTypeIndex)
        {
            RadioEquipmentManager.RadioType radioType = (RadioEquipmentManager.RadioType)radioTypeIndex;
            LogMessage($"Radio equipment changed to {radioType}", Colors.Cyan);
        }

        /// <summary>
        /// Handles radio effects being updated.
        /// </summary>
        private void OnRadioEffectsUpdated()
        {
            LogMessage("Radio effects updated", Colors.Cyan);
        }
    }
}

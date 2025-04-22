using Godot;
using System;
using System.Collections.Generic;
using SignalLost.Equipment;
using SignalLost.Inventory;

namespace SignalLost.Tests
{
    /// <summary>
    /// Tests for the equipment system.
    /// </summary>
    [GlobalClass]
    public partial class EquipmentSystemTest : GutTest
    {
        // References to systems
        private EquipmentSystem _equipmentSystem;
        private RadioEquipmentManager _radioEquipmentManager;
        private InventorySystemAdapter _inventorySystem;
        private SignalLost.GameState _gameState;

        // Test data
        private List<string> _testItems = new List<string>
        {
            "headphones_basic",
            "headphones_advanced",
            "radio_basic",
            "radio_advanced",
            "radio_military",
            "radio_custom",
            "signal_analyzer",
            "signal_booster",
            "compass",
            "weather_gear",
            "radiation_suit",
            "strange_crystal"
        };

        /// <summary>
        /// Called before each test.
        /// </summary>
        public override void Before()
        {
            // Get references to systems
            _equipmentSystem = GetNode<EquipmentSystem>("/root/EquipmentSystem");
            _radioEquipmentManager = GetNode<RadioEquipmentManager>("/root/RadioEquipmentManager");
            _inventorySystem = GetNode<InventorySystemAdapter>("/root/InventorySystem");
            _gameState = GetNode<SignalLost.GameState>("/root/GameState");

            // Clear inventory and equipment
            ClearInventoryAndEquipment();

            // Add test items to inventory
            foreach (string itemId in _testItems)
            {
                _inventorySystem.AddItemToInventory(itemId);
            }
        }

        /// <summary>
        /// Called after each test.
        /// </summary>
        public override void After()
        {
            // Clear inventory and equipment
            ClearInventoryAndEquipment();
        }

        /// <summary>
        /// Clears the inventory and equipment.
        /// </summary>
        private void ClearInventoryAndEquipment()
        {
            // Clear equipment
            foreach (EquipmentSystem.EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSystem.EquipmentSlot)))
            {
                _equipmentSystem.UnequipItem(slot);
            }

            // Clear inventory
            _gameState.ClearInventory();
        }

        /// <summary>
        /// Tests equipping an item.
        /// </summary>
        [Test]
        public void TestEquipItem()
        {
            // Equip headphones
            bool result = _equipmentSystem.EquipItem("headphones_basic", EquipmentSystem.EquipmentSlot.Head);
            Assert.IsTrue(result, "Should be able to equip headphones to head slot");

            // Check if the item is equipped
            string equippedItem = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Head);
            Assert.AreEqual("headphones_basic", equippedItem, "Headphones should be equipped to head slot");

            // Check if the item is marked as equipped
            bool isEquipped = _equipmentSystem.IsItemEquipped("headphones_basic");
            Assert.IsTrue(isEquipped, "Headphones should be marked as equipped");
        }

        /// <summary>
        /// Tests unequipping an item.
        /// </summary>
        [Test]
        public void TestUnequipItem()
        {
            // Equip headphones
            _equipmentSystem.EquipItem("headphones_basic", EquipmentSystem.EquipmentSlot.Head);

            // Unequip headphones
            bool result = _equipmentSystem.UnequipItem(EquipmentSystem.EquipmentSlot.Head);
            Assert.IsTrue(result, "Should be able to unequip headphones from head slot");

            // Check if the slot is empty
            string equippedItem = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Head);
            Assert.IsNull(equippedItem, "Head slot should be empty");

            // Check if the item is not marked as equipped
            bool isEquipped = _equipmentSystem.IsItemEquipped("headphones_basic");
            Assert.IsFalse(isEquipped, "Headphones should not be marked as equipped");
        }

        /// <summary>
        /// Tests equipping an item to an invalid slot.
        /// </summary>
        [Test]
        public void TestEquipItemToInvalidSlot()
        {
            // Try to equip headphones to radio slot
            bool result = _equipmentSystem.EquipItem("headphones_basic", EquipmentSystem.EquipmentSlot.Radio);
            Assert.IsFalse(result, "Should not be able to equip headphones to radio slot");

            // Check if the slot is empty
            string equippedItem = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Radio);
            Assert.IsNull(equippedItem, "Radio slot should be empty");
        }

        /// <summary>
        /// Tests equipping multiple items.
        /// </summary>
        [Test]
        public void TestEquipMultipleItems()
        {
            // Equip headphones
            bool result1 = _equipmentSystem.EquipItem("headphones_basic", EquipmentSystem.EquipmentSlot.Head);
            Assert.IsTrue(result1, "Should be able to equip headphones to head slot");

            // Equip radio
            bool result2 = _equipmentSystem.EquipItem("radio_basic", EquipmentSystem.EquipmentSlot.Radio);
            Assert.IsTrue(result2, "Should be able to equip radio to radio slot");

            // Equip compass
            bool result3 = _equipmentSystem.EquipItem("compass", EquipmentSystem.EquipmentSlot.Accessory1);
            Assert.IsTrue(result3, "Should be able to equip compass to accessory1 slot");

            // Check if all items are equipped
            string equippedHead = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Head);
            string equippedRadio = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Radio);
            string equippedAccessory1 = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Accessory1);

            Assert.AreEqual("headphones_basic", equippedHead, "Headphones should be equipped to head slot");
            Assert.AreEqual("radio_basic", equippedRadio, "Radio should be equipped to radio slot");
            Assert.AreEqual("compass", equippedAccessory1, "Compass should be equipped to accessory1 slot");
        }

        /// <summary>
        /// Tests replacing an equipped item.
        /// </summary>
        [Test]
        public void TestReplaceEquippedItem()
        {
            // Equip basic headphones
            _equipmentSystem.EquipItem("headphones_basic", EquipmentSystem.EquipmentSlot.Head);

            // Replace with advanced headphones
            bool result = _equipmentSystem.EquipItem("headphones_advanced", EquipmentSystem.EquipmentSlot.Head);
            Assert.IsTrue(result, "Should be able to replace basic headphones with advanced headphones");

            // Check if advanced headphones are equipped
            string equippedItem = _equipmentSystem.GetEquippedItem(EquipmentSystem.EquipmentSlot.Head);
            Assert.AreEqual("headphones_advanced", equippedItem, "Advanced headphones should be equipped to head slot");

            // Check if basic headphones are not marked as equipped
            bool isBasicEquipped = _equipmentSystem.IsItemEquipped("headphones_basic");
            Assert.IsFalse(isBasicEquipped, "Basic headphones should not be marked as equipped");
        }

        /// <summary>
        /// Tests getting item effects.
        /// </summary>
        [Test]
        public void TestGetItemEffects()
        {
            // Get effects for headphones
            Dictionary<string, float> effects = _equipmentSystem.GetItemEffects("headphones_basic");
            Assert.IsNotNull(effects, "Should get effects for headphones");
            Assert.IsTrue(effects.Count > 0, "Headphones should have effects");

            // Check specific effects
            Assert.IsTrue(effects.ContainsKey("signal_clarity"), "Headphones should have signal clarity effect");
            Assert.IsTrue(effects.ContainsKey("audio_quality"), "Headphones should have audio quality effect");
        }

        /// <summary>
        /// Tests getting total effect value.
        /// </summary>
        [Test]
        public void TestGetTotalEffectValue()
        {
            // Equip headphones and radio
            _equipmentSystem.EquipItem("headphones_basic", EquipmentSystem.EquipmentSlot.Head);
            _equipmentSystem.EquipItem("radio_basic", EquipmentSystem.EquipmentSlot.Radio);

            // Get total effect value for signal clarity
            float totalValue = _equipmentSystem.GetTotalEffectValue("signal_clarity");
            Assert.IsGreaterThan(totalValue, 1.0f, "Total signal clarity effect should be greater than 1.0");
        }

        /// <summary>
        /// Tests getting all active effects.
        /// </summary>
        [Test]
        public void TestGetAllActiveEffects()
        {
            // Equip headphones and radio
            _equipmentSystem.EquipItem("headphones_basic", EquipmentSystem.EquipmentSlot.Head);
            _equipmentSystem.EquipItem("radio_basic", EquipmentSystem.EquipmentSlot.Radio);

            // Get all active effects
            Dictionary<string, float> activeEffects = _equipmentSystem.GetAllActiveEffects();
            Assert.IsNotNull(activeEffects, "Should get active effects");
            Assert.IsTrue(activeEffects.Count > 0, "Should have active effects");

            // Check specific effects
            Assert.IsTrue(activeEffects.ContainsKey("signal_clarity"), "Should have signal clarity effect");
            Assert.IsTrue(activeEffects.ContainsKey("audio_quality"), "Should have audio quality effect");
            Assert.IsTrue(activeEffects.ContainsKey("signal_range"), "Should have signal range effect");
            Assert.IsTrue(activeEffects.ContainsKey("frequency_precision"), "Should have frequency precision effect");
        }

        /// <summary>
        /// Tests upgrading a radio component.
        /// </summary>
        [Test]
        public void TestUpgradeRadioComponent()
        {
            // Equip custom radio
            _equipmentSystem.EquipItem("radio_custom", EquipmentSystem.EquipmentSlot.Radio);

            // Get initial component level
            int initialLevel = _radioEquipmentManager.GetComponentLevel(RadioEquipmentManager.ComponentType.Antenna);

            // Upgrade antenna
            bool result = _radioEquipmentManager.UpgradeComponent(RadioEquipmentManager.ComponentType.Antenna);
            Assert.IsTrue(result, "Should be able to upgrade antenna");

            // Check if level increased
            int newLevel = _radioEquipmentManager.GetComponentLevel(RadioEquipmentManager.ComponentType.Antenna);
            Assert.AreEqual(initialLevel + 1, newLevel, "Antenna level should increase by 1");
        }

        /// <summary>
        /// Tests creating a custom radio.
        /// </summary>
        [Test]
        public void TestCreateCustomRadio()
        {
            // Add broken radio and radio part to inventory
            _inventorySystem.AddItemToInventory("radio_broken");
            _inventorySystem.AddItemToInventory("radio_part");

            // Check if can create custom radio
            bool canCreate = _radioEquipmentManager.CanCreateCustomRadio();
            Assert.IsTrue(canCreate, "Should be able to create custom radio");

            // Create custom radio
            bool result = _radioEquipmentManager.CreateCustomRadio();
            Assert.IsTrue(result, "Should be able to create custom radio");

            // Check if custom radio was added to inventory
            bool hasCustomRadio = _inventorySystem.HasItem("radio_custom");
            Assert.IsTrue(hasCustomRadio, "Should have custom radio in inventory");

            // Check if broken radio and radio part were removed from inventory
            bool hasBrokenRadio = _inventorySystem.HasItem("radio_broken");
            bool hasRadioPart = _inventorySystem.HasItem("radio_part");
            Assert.IsFalse(hasBrokenRadio, "Should not have broken radio in inventory");
            Assert.IsFalse(hasRadioPart, "Should not have radio part in inventory");
        }
    }
}

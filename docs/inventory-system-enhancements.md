# Inventory System Enhancements

## Overview

The inventory system has been enhanced to provide a more immersive and interactive gameplay experience. These enhancements include item placement in the game world, improved UI for item details, and better integration with other game systems.

## Key Enhancements

### 1. Item Placement System

The item placement system allows for items to be placed in the game world and discovered by the player. This creates a more immersive exploration experience and rewards players for thoroughly exploring the environment.

```csharp
public partial class ItemPlacement : Node
{
    // Dictionary of items placed in the world
    private Dictionary<string, PlacedItem> _placedItems = new Dictionary<string, PlacedItem>();
    
    // Methods for managing placed items
    // ...
}
```

#### Key Features:

- **Location-Based Item Placement**: Items are placed in specific locations and containers within those locations.
- **Item Discovery**: Players can discover items by interacting with objects in the environment.
- **Quest Integration**: Discovering items can advance quest objectives.
- **Persistent State**: The state of placed items (collected or not) is saved and loaded with the game state.

### 2. Enhanced Item UI

The inventory UI has been enhanced to provide more detailed information about items and a more intuitive user experience.

```csharp
public partial class InventorySlotUI : Control
{
    // UI elements for displaying item details
    private TextureRect _iconRect;
    private Label _quantityLabel;
    private Panel _selectionPanel;
    private Panel _equippedPanel;
    private Control _tooltipControl;
    private Label _tooltipNameLabel;
    private Label _tooltipDescriptionLabel;
    private Label _tooltipCategoryLabel;
    
    // Methods for updating the UI
    // ...
}
```

#### Key Features:

- **Item Tooltips**: Hovering over an item shows a tooltip with the item's name, category, and description.
- **Visual Feedback**: Selected and equipped items are visually distinguished.
- **Item Categories**: Items are categorized (e.g., Tool, Consumable, Key Item) for better organization.
- **Item Properties**: Item properties (e.g., Usable, Equippable, Combineable) are clearly displayed.

### 3. Interactive Object Integration

Interactive objects in the game world can now contain placed items, creating a more immersive and interactive environment.

```csharp
public partial class InteractiveObject : Node3D
{
    // Item placement properties
    [Export] public bool ContainsPlacedItem { get; set; } = false;
    [Export] public string PlacedItemId { get; set; } = "";
    [Export] public string ContainerName { get; set; } = "";
    
    // Methods for interacting with placed items
    // ...
}
```

#### Key Features:

- **Container Objects**: Interactive objects can act as containers for items.
- **Contextual Interactions**: Different interactions are available based on the object and its contents.
- **Visual Feedback**: Players receive visual feedback when interacting with objects that contain items.
- **Quest Integration**: Interacting with objects can advance quest objectives.

### 4. Item Functionality

Items now have more functionality and can be used in various ways to affect gameplay.

```csharp
public partial class ItemEffectsSystem : Node
{
    // Dictionary of item effects
    private Dictionary<string, ItemEffect> _itemEffects = new Dictionary<string, ItemEffect>();
    
    // Methods for applying item effects
    // ...
}
```

#### Key Features:

- **Item Effects**: Items can have various effects when used (e.g., healing, providing light, revealing information).
- **Item Combinations**: Items can be combined to create new items.
- **Equipment Effects**: Equipped items can provide passive benefits.
- **Contextual Usage**: Items can be used in specific contexts for different effects.

## Item Types

The inventory system supports several types of items:

1. **Tools**: Items that can be equipped and used for specific purposes (e.g., flashlight, radio, map).
2. **Consumables**: Items that are consumed when used (e.g., battery, medkit, water, food).
3. **Key Items**: Items that are used to unlock or access specific areas or features (e.g., keys, keycards, badges).
4. **Documents**: Items that provide information or story elements (e.g., map fragments, research notes).
5. **Components**: Items that are used to repair or upgrade other items (e.g., radio parts, signal amplifier).
6. **Special Items**: Unique items with special properties or effects (e.g., strange crystal).

## Item Placement

Items are placed in various locations throughout the game world:

### Bunker

- **Flashlight**: On the desk in the bunker.
- **Battery**: In a drawer in the bunker.
- **Broken Radio**: On a shelf in the bunker.
- **Map Fragment**: Pinned to the wall in the bunker.

### Forest

- **Medkit**: Near a fallen tree in the forest.
- **Water Bottle**: By a stream in the forest.
- **Cabin Key**: Hidden in a hollow tree stump in the forest.

### Cabin

- **Radio Part**: On the workbench in the cabin.
- **Canned Food**: In a cupboard in the cabin.
- **Research Notes**: On the desk in the cabin.
- **Research Keycard**: In a safe in the cabin.

### Research Facility

- **Signal Amplifier**: On a lab table in the research facility.
- **Military Badge**: In a locker in the research facility.
- **Strange Crystal**: In a containment unit in the research facility.

## Integration with Other Systems

The enhanced inventory system integrates with several other game systems:

### Field Exploration System

- Items can be placed in the game world and discovered during exploration.
- Interactive objects can contain items.
- Discovering items can reveal new locations or information.

### Quest System

- Discovering items can advance quest objectives.
- Items can be required to complete quests.
- Items can provide clues or information for quests.

### Radio System

- Radio parts can be collected to repair the radio.
- Signal amplifiers can improve radio reception.
- Special items can unlock hidden radio frequencies.

### Game Progression

- Key items can unlock new areas or features.
- Collecting certain items can trigger story events.
- Items can provide information about the game world and story.

## Future Enhancements

1. **Item Crafting**: Allow players to craft new items from collected resources.
2. **Item Degradation**: Implement a system where items can degrade with use and need to be repaired.
3. **Item Sets**: Create sets of items that provide bonuses when collected.
4. **Item Rarity**: Implement a rarity system for items.
5. **Item Trading**: Allow players to trade items with NPCs.
6. **Item Customization**: Allow players to customize or upgrade items.
7. **Item Quests**: Create quests specifically focused on finding or using certain items.
8. **Item Lore**: Add more lore and background information for items.
9. **Item Animations**: Add animations for using or equipping items.
10. **Item Sound Effects**: Add sound effects for using or equipping items.

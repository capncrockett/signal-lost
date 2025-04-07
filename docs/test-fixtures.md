# Test Fixtures

This document describes the test fixtures system for the Signal Lost game.

## Overview

Test fixtures provide consistent, reusable test data for unit, integration, and E2E tests. They help ensure that tests are reliable, maintainable, and focused on testing behavior rather than setting up test data.

## Fixture Structure

The test fixtures system is organized into collections of related fixtures:

- **Game State**: Game state and save data fixtures
- **Scenes**: Scene-specific fixtures for MainScene and FieldScene
- **Audio**: Audio and radio fixtures
- **Narrative**: Narrative engine and event fixtures
- **Inventory**: Inventory and item fixtures

Each fixture has:
- **ID**: Unique identifier
- **Data**: The actual fixture data
- **Metadata**: Optional metadata like description, tags, and dependencies

## Using Fixtures

### Basic Usage

```typescript
import { FixtureLoader } from '../tests/fixtures';

// Load a fixture
const gameState = FixtureLoader.load('gameState', 'midGame');

// Use the fixture in tests
test('should update game state correctly', () => {
  const result = updateGameState(gameState, { flag: 'discovered_bunker1', value: true });
  expect(result.flags.discovered_bunker1).toBe(true);
});
```

### Loading Multiple Fixtures

```typescript
// Load multiple fixtures
const items = FixtureLoader.loadMultiple('items', ['radio', 'map', 'battery']);

// Load all fixtures from a collection
const allEvents = FixtureLoader.loadAll('events');
```

### Fixture Options

```typescript
// Load a fixture with options
const gameState = FixtureLoader.load('gameState', 'midGame', {
  deepClone: true, // Clone the fixture data (default: true)
  validate: true, // Validate the fixture (default: true)
  resolveDependencies: true, // Resolve dependencies (default: true)
});
```

## Available Fixtures

### Game State Fixtures

Game state fixtures represent different states of game progress:

- **newGame**: New game state with no progress
- **midGame**: Mid-game state with some progress
- **lateGame**: Late-game state with significant progress
- **completedGame**: Completed game state

### Save Data Fixtures

Save data fixtures represent different save file states:

- **emptySave**: Empty save data
- **midGameSave**: Mid-game save data
- **corruptedSave**: Corrupted save data for testing error handling
- **incompatibleVersionSave**: Save data with incompatible version
- **multiSlotSave**: Save data with multiple slots

### Scene Fixtures

#### Main Scene Fixtures

- **initialState**: Initial state of the main scene
- **signalDetected**: Main scene with a detected signal
- **weakSignal**: Main scene with a weak signal
- **noSignal**: Main scene with no signal
- **enhancedRadio**: Main scene with enhanced radio capabilities
- **saveMenuOpen**: Main scene with save menu open

#### Field Scene Fixtures

- **initialState**: Initial state of the field scene
- **nearTower**: Field scene with player near a tower
- **narrativeActive**: Field scene with active narrative dialog
- **inventoryOpen**: Field scene with inventory open
- **itemPickup**: Field scene with player near an item pickup

### Audio Fixtures

#### Audio Settings Fixtures

- **defaultSettings**: Default audio settings
- **mutedSettings**: Muted audio settings
- **lowVolume**: Low volume audio settings
- **noMusic**: Audio settings with no music

#### Soundscape Fixtures

- **soundscapeDefault**: Default soundscape settings
- **soundscapeHighInterference**: Soundscape settings with high interference
- **soundscapeClear**: Soundscape settings with clear signal

### Radio Fixtures

- **defaultRadio**: Default radio settings
- **enhancedRadio**: Enhanced radio with better range
- **filteredRadio**: Filtered radio with reduced static
- **fullyUpgradedRadio**: Fully upgraded radio with all enhancements
- **tunedToSignal**: Radio tuned to a signal
- **weakSignal**: Radio with weak signal reception

### Narrative Fixtures

#### Narrative Engine Fixtures

- **emptyEngine**: Empty narrative engine state
- **basicEngine**: Basic narrative engine state with intro and tutorial
- **midGameEngine**: Mid-game narrative engine state
- **conditionalEvents**: Narrative engine with conditional events
- **complexBranching**: Narrative engine with complex branching choices

#### Event Fixtures

- **signalEvents**: Signal discovery events
- **locationEvents**: Location discovery events
- **itemEvents**: Item discovery events
- **endgameEvents**: Endgame events
- **tutorialEvents**: Tutorial events

### Inventory Fixtures

#### Inventory State Fixtures

- **emptyInventory**: Empty inventory
- **startingInventory**: Starting inventory with just a radio
- **midGameInventory**: Mid-game inventory with several items
- **lateGameInventory**: Late-game inventory with many items
- **fullInventory**: Full inventory at capacity
- **stackedInventory**: Inventory with stacked items
- **upgradedInventory**: Inventory with upgraded items

#### Item Fixtures

- **allItems**: All item definitions
- **toolItems**: Tool items
- **consumableItems**: Consumable items
- **documentItems**: Document items
- **keyItems**: Key items
- **componentItems**: Component items

## Creating New Fixtures

To create a new fixture:

1. Identify the appropriate collection for your fixture
2. Add your fixture to the collection file
3. Provide a unique ID, data, and metadata
4. Use the fixture in your tests

Example:

```typescript
// Add to tests/fixtures/state/GameStateFixtures.ts
{
  id: 'customGameState',
  data: {
    flags: {
      custom_flag: true,
    },
    data: {
      customValue: 42,
    },
  },
  metadata: {
    description: 'Custom game state for specific test',
    tags: ['custom', 'test'],
  },
}
```

## Best Practices

1. **Use fixtures for common test scenarios** to avoid duplicating test setup code
2. **Keep fixtures focused** on specific test scenarios
3. **Use metadata** to document the purpose and usage of fixtures
4. **Use tags** to categorize fixtures for easier discovery
5. **Use dependencies** to compose complex fixtures from simpler ones
6. **Clone fixtures** when modifying them to avoid affecting other tests
7. **Validate fixtures** to ensure they meet expected requirements
8. **Document fixtures** to help other developers understand their purpose and usage

## Extending the Fixture System

The fixture system can be extended in several ways:

- **Add new collections** for new types of fixtures
- **Add new fixtures** to existing collections
- **Add new metadata** to provide more information about fixtures
- **Add new validation rules** to ensure fixtures meet specific requirements
- **Add new dependency resolution** to compose fixtures in more complex ways

## Troubleshooting

### Common Issues

- **Fixture not found**: Check that the collection and fixture IDs are correct
- **Fixture data not as expected**: Check that the fixture data is correct and that you're not modifying a shared fixture
- **Dependency resolution failed**: Check that the dependency exists and is correctly formatted

### Debugging

- Use `FixtureLoader.getFixture()` to inspect a fixture without loading it
- Use `FixtureLoader.getAllFixtures()` to inspect all fixtures in a collection
- Use `FixtureLoader.getCollection()` to inspect a collection
- Use `FixtureLoader.getAllCollections()` to inspect all collections

# Field Exploration System

## Overview

The Field Exploration System is a new feature in Signal Lost that allows players to navigate and interact with the game world using a grid-based movement system. This document outlines the design, implementation, and integration of this system with existing game mechanics.

## Core Components

### 1. Grid-Based Movement

- **Grid System**: The game world is divided into a grid of cells
- **Player Movement**: Players can move in four directions (up, down, left, right)
- **Movement Constraints**: Some cells may be impassable (obstacles, boundaries)
- **Visual Feedback**: Movement is accompanied by visual feedback

### 2. Player Character

- **Pixel-Based Rendering**: The player character is rendered using pixel art techniques
- **Animation**: Simple animations for movement and interactions
- **State Management**: Tracking player position, orientation, and current action

### 3. Interactable Objects

- **Object Types**: Various objects that can be examined, collected, or activated
- **Interaction System**: Context-sensitive interactions based on object type
- **Visual Indicators**: Visual cues to show interactable objects
- **Inventory Integration**: Collected objects are added to the player's inventory

### 4. Radio Signal Integration

- **Signal Strength**: Radio signal strength varies based on player location
- **Signal Sources**: Specific locations emit radio signals at certain frequencies
- **Environmental Effects**: Environment affects signal reception and quality
- **Gameplay Mechanics**: Using the radio to locate signal sources and decode messages

## Implementation Details

### Grid System

```csharp
public class GridSystem : Node
{
    // Grid dimensions
    private int _width;
    private int _height;
    
    // Cell data
    private Cell[,] _grid;
    
    // Initialize grid
    public void Initialize(int width, int height)
    {
        _width = width;
        _height = height;
        _grid = new Cell[width, height];
        
        // Initialize cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _grid[x, y] = new Cell(x, y);
            }
        }
    }
    
    // Check if position is valid
    public bool IsValidPosition(Vector2I position)
    {
        return position.X >= 0 && position.X < _width &&
               position.Y >= 0 && position.Y < _height &&
               !_grid[position.X, position.Y].IsBlocked;
    }
    
    // Get cell at position
    public Cell GetCell(Vector2I position)
    {
        if (IsValidPosition(position))
        {
            return _grid[position.X, position.Y];
        }
        return null;
    }
}
```

### Player Controller

```csharp
public class PlayerController : Node2D
{
    // Current position on grid
    private Vector2I _gridPosition;
    
    // Reference to grid system
    private GridSystem _gridSystem;
    
    // Movement speed
    [Export]
    private float _moveSpeed = 100.0f;
    
    // Try to move in direction
    public bool TryMove(Vector2I direction)
    {
        Vector2I newPosition = _gridPosition + direction;
        
        if (_gridSystem.IsValidPosition(newPosition))
        {
            _gridPosition = newPosition;
            Position = new Vector2(_gridPosition.X * 16, _gridPosition.Y * 16);
            return true;
        }
        
        return false;
    }
    
    // Interact with current cell or adjacent cell
    public void Interact()
    {
        Cell currentCell = _gridSystem.GetCell(_gridPosition);
        if (currentCell.HasInteractable)
        {
            currentCell.Interact();
        }
    }
}
```

## Integration with Existing Systems

### Radio System Integration

The field exploration system integrates with the existing radio system:

1. **Signal Strength Calculation**:
   ```csharp
   // Calculate signal strength based on distance to signal source
   public float CalculateSignalStrength(Vector2I playerPosition, Vector2I signalSource)
   {
       float distance = playerPosition.DistanceTo(signalSource);
       return Mathf.Max(0, 1.0f - (distance / _maxDistance));
   }
   ```

2. **Environmental Effects**:
   ```csharp
   // Apply environmental effects to signal
   public float ApplyEnvironmentalEffects(float baseStrength, Cell currentCell)
   {
       if (currentCell.HasInterference)
       {
           return baseStrength * currentCell.InterferenceMultiplier;
       }
       return baseStrength;
   }
   ```

### Inventory System Integration

Objects found during exploration can be added to the player's inventory:

```csharp
// Add item to inventory when collected
public void CollectItem(Item item)
{
    GameState.Instance.InventorySystem.AddItem(item);
    // Show collection message
    MessageManager.Instance.ShowMessage($"Collected: {item.Name}");
}
```

## User Interface

### Map Display

- **Pixel-Based Map**: Shows explored areas and current position
- **Fog of War**: Unexplored areas are hidden
- **Points of Interest**: Special locations are marked once discovered

### Interaction Prompts

- **Context-Sensitive Prompts**: Show available actions for nearby objects
- **Visual Indicators**: Highlight interactable objects
- **Feedback Messages**: Display results of interactions

## Development Roadmap

### Phase 1: Basic Movement ⬜

- Implement grid system
- Create player character with basic movement
- Set up camera and viewport

### Phase 2: Interactions ⬜

- Implement interactable objects
- Create interaction system
- Add visual feedback for interactions

### Phase 3: Radio Integration ⬜

- Connect player position to radio signal strength
- Implement signal sources
- Add environmental effects on signal reception

### Phase 4: Polish ⬜

- Add animations and visual effects
- Implement sound effects
- Optimize performance
- Add quality-of-life features

## Testing

### Unit Tests

- Test grid system functionality
- Verify player movement constraints
- Test interaction system

### Integration Tests

- Test radio system integration
- Verify inventory system integration
- Test save/load functionality with exploration state

### Manual Tests

- Verify visual feedback
- Test player movement feel
- Ensure proper scaling on different resolutions

## Conclusion

The Field Exploration System adds a new dimension to Signal Lost, allowing players to physically explore the game world while using the radio to locate signal sources. This system integrates with existing game mechanics to create a cohesive gameplay experience.

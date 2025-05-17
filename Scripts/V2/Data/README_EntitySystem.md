# Entity System

## Overview
The Entity System forms the foundation of all game objects in the simulation. It provides a base class with common properties and behaviors that all entities in the game world share, such as position, rotation, and unique identification.

## Core Components

### Entity
The `Entity` class is the base class for all objects in the simulation world.

**Key Properties:**
- `ID`: A unique identifier for each entity, automatically assigned upon creation.
- `LocalPosition`: The grid-based position of the entity in the world.
- `Rotation`: The rotation of the entity in degrees (0, 90, 180, 270).

**Key Methods:**
- `SetPosition(Vector2Int)`: Sets the entity's position using grid coordinates.
- `SetPosition(Vector2)`: Sets the entity's position using world coordinates, snapping to the grid.
- `Rotate()`: Rotates the entity by 90 degrees clockwise.
- `Tick(float dt)`: Virtual method called each simulation update, with delta time as parameter.

## Usage Examples

### Creating a Basic Entity
```csharp
// Create a new entity at grid position (5, 3)
Entity entity = new Entity(new Vector2Int(5, 3));

// Rotate the entity 90 degrees clockwise
entity.Rotate();

// Move the entity to a new position
entity.SetPosition(new Vector2Int(7, 4));
```

### Extending the Entity Class
```csharp
// Create a custom entity type
public class CustomEntity : Entity
{
    public CustomEntity(Vector2Int position) : base(position)
    {
        // Custom initialization
    }
    
    public override void Tick(float dt)
    {
        base.Tick(dt);
        // Custom update logic
    }
}
```

## Integration with Other Systems
The Entity system integrates with:
- `GridUtility`: For position snapping and world-to-grid conversions
- `ChunkData`: For managing collections of entities in the world
- Derived classes like `Machine`, `Connector`, and `BeltData`

## Best Practices
- Always use the `SetPosition` methods rather than directly modifying `LocalPosition`
- Override the `Tick` method for custom behavior, but call the base implementation
- Use the `ID` property for entity tracking and reference rather than object references when appropriate
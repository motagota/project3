# Connector System

## Overview
The Connector System manages the flow of items between machines and conveyor belts in the simulation. Connectors act as transfer points that pick up items from one entity and deliver them to another, forming the backbone of the factory automation system.

## Core Components

### Connector
The `Connector` class extends the base `Entity` class and handles the logic for transferring items between connected entities.

**Key Properties:**
- `_inputConnector`: Reference to the entity connected to the input side of this connector.
- `_outputConnector`: Reference to the entity connected to the output side of this connector.
- `_inputHeldItem`: The item currently being held by the connector for transfer.
- `HasInputItem`: Boolean indicating if the connector is currently holding an item.
- `CanDropItem`: Boolean indicating if the connector can drop its held item.

**Key Methods:**
- `GetFrontPosition()`: Returns the grid position in front of the connector based on its rotation.
- `GetBackPosition()`: Returns the grid position behind the connector based on its rotation.
- `Rotate(ChunkData)`: Rotates the connector and updates its connections.
- `CheckForConnection(ChunkData)`: Checks and updates connections to adjacent entities.
- `Tick(float dt)`: Handles the item transfer logic during simulation updates.
- `TryPickUpItem()`: Attempts to pick up an item from the input connection.
- `TryDropItem()`: Attempts to drop the held item to the output connection.

**Events:**
- `OnConnectionChanged`: Triggered when a connection to another entity changes.
- `OnItemPickedUp`: Triggered when an item is picked up from an input entity.
- `OnItemDropped`: Triggered when an item is dropped to an output entity.

## Usage Examples

### Creating and Configuring a Connector
```csharp
// Create a new connector at grid position (5, 3)
Connector connector = new Connector(new Vector2Int(5, 3));

// Rotate the connector to face a specific direction
connector.Rotate(chunkData);

// Check for connections to adjacent entities
connector.CheckForConnection(chunkData);
```

### Handling Connector Events
```csharp
// Subscribe to connector events
connector.OnConnectionChanged += (connector, entity) => {
    Debug.Log($"Connector {connector.ID} connection changed to {entity?.ID}");
};

connector.OnItemPickedUp += (connector, item) => {
    Debug.Log($"Connector {connector.ID} picked up {item}");
};

connector.OnItemDropped += (connector, item) => {
    Debug.Log($"Connector {connector.ID} dropped {item}");
};
```

## Integration with Other Systems
The Connector system integrates with:
- `Entity`: Inherits base functionality for position and rotation
- `Machine`: Connects to machines to pick up or deliver items
- `BeltData`: Connects to conveyor belts to pick up or deliver items
- `ChunkData`: For accessing entities in the world and managing connections
- `SimulationItem`: Handles the items being transferred

## Best Practices
- Always use `CheckForConnection` after placing or rotating a connector
- Subscribe to the connector events to visualize item transfers in the UI
- Consider the rotation of connectors when designing factory layouts
- Use `GetFrontPosition` and `GetBackPosition` to determine the connector's orientation
# Item System

## Overview
The Item System represents the resources and products that flow through the factory simulation. Items are created, processed, and transported between machines and conveyor belts, forming the core resources of the production chain.

## Core Components

### SimulationItem
The `SimulationItem` class represents a discrete item in the simulation world.

**Key Properties:**
- `ItemType`: String identifier for the type of item (e.g., "IronOre", "CopperPlate").
- `id`: Unique identifier for the specific item instance.

**Key Methods:**
- `ToString()`: Returns the item type as a string representation of the item.

## Usage Examples

### Creating Items
```csharp
// Create a new iron ore item
SimulationItem ironOre = new SimulationItem("item1", "IronOre");

// Create a new copper plate item
SimulationItem copperPlate = new SimulationItem("item2", "CopperPlate");
```

### Using Items in Machines
```csharp
// Check if a machine can accept a specific item type
if (machine.CanAcceptItem(ironOre))
{
    // Give the item to the machine for processing
    machine.GiveItem(ironOre);
}
```

## Integration with Other Systems
The Item system integrates with:
- `Machine`: Items are consumed and produced by machines
- `Connector`: Items are transferred between entities by connectors
- `BeltData`: Items are transported along conveyor belts
- `Recipe`: Defines what items are required and produced in manufacturing processes
- `ItemDatabase`: Stores definitions of all available item types

## Best Practices
- Use consistent item type strings throughout the codebase
- Consider using the ItemDatabase for centralized item type management
- Generate unique IDs for items to track them through the production chain
- Use the item's ID rather than object references when tracking items across systems
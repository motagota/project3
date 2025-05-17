# Machine System

## Overview
The Machine System forms the core production facilities in the factory simulation. Machines process input items according to recipes and produce output items, representing the manufacturing processes in the game world.

## Core Components

### Machine
The `Machine` class extends the base `Entity` class and handles the logic for processing items according to recipes.

**Key Properties:**
- `CurrentRecipe`: The recipe that defines what inputs are required and what output will be produced.
- `Progress`: Float value tracking the progress of the current recipe (0 to Recipe.Duration).
- `CompletedRecipes`: Counter for the number of recipes completed by this machine.
- `_outputItem`: The item that has been produced and is ready for collection.
- `_inputItems`: Dictionary storing input items by their type.
- `HasItem`: Boolean indicating if the machine has a completed output item ready.
- `IsEnabled`: Boolean controlling whether the machine is operational.

**Key Methods:**
- `Tick(float dt)`: Handles the recipe processing logic during simulation updates.
- `HasRequiredInputItems()`: Checks if the machine has all required input items for the recipe.
- `ConsumeItems()`: Consumes input items when processing a recipe.
- `TakeItem()`: Removes and returns the output item if available.
- `CanAcceptItem(SimulationItem)`: Checks if the machine can accept a specific item type.
- `GiveItem(SimulationItem)`: Adds an input item to the machine's inventory.

**Events:**
- `OnRecipeCompleted`: Triggered when a recipe is completed and an output item is produced.
- `OnItemConsumed`: Triggered when an input item is consumed during processing.
- `OnEnabledStateChanged`: Triggered when the machine's enabled state changes.

## Usage Examples

### Creating and Configuring a Machine
```csharp
// Create a new machine at grid position (5, 3)
Machine machine = new Machine(new Vector2Int(5, 3));

// Set a recipe for the machine
Recipe ironPlateRecipe = new Recipe(2.0f, "IronPlate", new List<string> { "IronOre" }, 1);
machine.CurrentRecipe = ironPlateRecipe;

// Enable or disable the machine
machine.IsEnabled = true;
```

### Handling Machine Events
```csharp
// Subscribe to machine events
machine.OnRecipeCompleted += (machine) => {
    Debug.Log($"Machine {machine.ID} completed a recipe");
};

machine.OnItemConsumed += (machine, item) => {
    Debug.Log($"Machine {machine.ID} consumed {item}");
};

machine.OnEnabledStateChanged += (machine) => {
    Debug.Log($"Machine {machine.ID} enabled state changed to {machine.IsEnabled}");
};
```

### Processing Items
```csharp
// Give input items to the machine
SimulationItem ironOre = new SimulationItem("ore1", "IronOre");
if (machine.CanAcceptItem(ironOre))
{
    machine.GiveItem(ironOre);
}

// Take output items from the machine
if (machine.HasItem)
{
    SimulationItem ironPlate = machine.TakeItem();
    // Use the produced item
}
```

## Integration with Other Systems
The Machine system integrates with:
- `Entity`: Inherits base functionality for position and rotation
- `Recipe`: Defines the processing requirements and outputs
- `SimulationItem`: Processes items as inputs and outputs
- `Connector`: Interfaces with machines to transfer items
- `ChunkData`: For managing machines in the world

## Best Practices
- Always check `CanAcceptItem()` before calling `GiveItem()`
- Subscribe to the machine events to visualize processing in the UI
- Use the `IsEnabled` property to control machine operation
- Check `HasItem` before attempting to take an output item
- Consider the machine's recipe when designing factory layouts
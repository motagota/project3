# Recipe System Documentation

## Overview
This recipe system provides a flexible way to configure and manage recipes for your simulation. It allows you to define various recipes with different inputs, outputs, and processing times, and makes them available throughout your simulation.

## Core Components

### Recipe Class
The base class that defines what a recipe is. Each recipe has:
- **Duration**: How long it takes to process the recipe
- **OutputItemType**: The type of item produced
- **InputItemTypes**: List of item types required as inputs
- **InputItemCount**: Number of each input item required

### RecipeDatabase
A singleton class that stores and manages all recipes. It provides methods to:
- Add recipes
- Retrieve recipes by ID
- Find recipes by input or output item types
- Get all available recipes

### RecipeFactory
A utility class with static methods to create common recipe types:
- Basic processing recipes (1 input → 1 output)
- Multi-input recipes (multiple inputs → 1 output)
- Generator recipes (no inputs → 1 output)
- Recycling recipes (placeholder for future expansion)

### RecipeDatabaseManager
Handles saving and loading recipes to/from JSON files for persistence between game sessions.

### RecipeDatabaseEditor
A Unity Editor window that allows you to manage recipes directly in the Unity Editor.

## Usage Examples

### Setting Up a Machine with a Recipe
```csharp
// Get a recipe from the database
Recipe ironPlateRecipe = RecipeDatabase.Instance.GetRecipe("IronOre_to_IronPlate");

// Assign it to a machine
machine.CurrentRecipe = ironPlateRecipe;
```

### Creating a Custom Recipe
```csharp
// Create a new recipe
Recipe customRecipe = new Recipe(
    duration: 5.0f,
    outputItemType: "AdvancedComponent",
    inputItemTypes: new List<string> { "BasicComponent", "Electronics" },
    inputItemCount: 2
);

// Add it to the database
RecipeDatabase.Instance.AddRecipe("Advanced_Component_Recipe", customRecipe);
```

### Using the RecipeFactory
```csharp
// Create a basic processing recipe
Recipe basicRecipe = RecipeFactory.CreateBasicProcessingRecipe(
    inputType: "RawMaterial",
    outputType: "ProcessedMaterial",
    duration: 3.0f,
    inputCount: 1
);

// Create a generator recipe
Recipe generatorRecipe = RecipeFactory.CreateGeneratorRecipe(
    outputType: "Energy",
    duration: 1.0f
);
```

### Finding Recipes
```csharp
// Find all recipes that produce "Circuit"
List<Recipe> circuitRecipes = RecipeDatabase.Instance.GetRecipesByOutput("Circuit");

// Find all recipes that require "IronPlate" as input
List<Recipe> ironPlateRecipes = RecipeDatabase.Instance.GetRecipesByInput("IronPlate");
```

## Integration with Machines
The Machine class is already set up to work with recipes. Each machine has a CurrentRecipe property that determines what it produces and what inputs it requires.

## Saving and Loading Recipes
To save or load recipes, add the RecipeDatabaseManager component to a GameObject in your scene:

```csharp
// Get the manager component
RecipeDatabaseManager manager = GetComponent<RecipeDatabaseManager>();

// Save recipes to file
manager.SaveRecipesToFile();

// Load recipes from file
manager.LoadRecipesFromFile();
```

## Editor Tools
To access the Recipe Database Editor, go to Tools > Recipe Database Editor in the Unity menu. This provides a user-friendly interface to manage your recipes without writing code.

## Future Expansion
The system is designed to be easily expandable. Some potential future enhancements:
- Support for multiple outputs from a single recipe
- Recipe categories or tags for better organization
- Recipe prerequisites or tech tree integration
- Recipe efficiency modifiers based on machine types
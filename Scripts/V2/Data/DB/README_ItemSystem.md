# Item System Documentation

## Overview
This item system provides a flexible way to configure and manage items for your simulation. It allows you to define various item types with different properties and makes them available throughout your simulation.

## Core Components

### ItemDefinition Class
The base class that defines what an item is. Each item has:
- **Basic Properties**: DisplayName, Description, Category
- **Inventory Properties**: Stackable, MaxStackSize
- **Visual Properties**: IconPath, IconColor
- **Physical Properties**: Mass, Size
- **Gameplay Properties**: Value, Consumable, ConsumptionEffect
- **Custom Properties**: A dictionary for any additional properties

### ItemDatabase
A singleton class that stores and manages all item definitions. It provides methods to:
- Add items
- Retrieve items by ID
- Find items by category
- Get all available items

### ItemFactory
A utility class with static methods to create common item types:
- Raw materials
- Processed materials
- Components
- Tools
- Consumables
- Fuels

### ItemDatabaseManager
Handles saving and loading item definitions to/from JSON files for persistence between game sessions.

### ItemDatabaseEditor
A Unity Editor window that allows you to manage items directly in the Unity Editor.

## Usage Examples

### Accessing the Database
```csharp
// Get the item database singleton
ItemDatabase itemDB = ItemDatabase.Instance;

// Get an item definition
ItemDefinition ironOre = itemDB.GetItem("IronOre");

// Check item properties
Debug.Log($"Item: {ironOre.DisplayName}, Category: {ironOre.Category}");
```

### Creating Custom Items
```csharp
// Create a new item definition
ItemDefinition customItem = new ItemDefinition(
    displayName: "Advanced Component",
    description: "A complex component used in high-tech machinery.",
    category: "Component",
    stackable: true,
    maxStackSize: 20
);

// Set additional properties
customItem.SetVisualProperties("Icons/advanced_component", Color.blue);
customItem.SetPhysicalProperties(2.5f, new Vector3(0.5f, 0.5f, 0.2f));
customItem.SetGameplayProperties(25.0f, false);

// Add custom properties if needed
customItem.SetCustomProperty("RequiresPower", true);
customItem.SetCustomProperty("TechLevel", 3);

// Add to the database
ItemDatabase.Instance.AddItem("AdvancedComponent", customItem);
```

### Using the ItemFactory
```csharp
// Create a raw material
ItemDefinition ironOre = ItemFactory.CreateRawMaterial(
    displayName: "Iron Ore",
    description: "Raw iron ore extracted from the ground.",
    value: 1.0f,
    mass: 2.0f
);

// Create a tool
ItemDefinition pickaxe = ItemFactory.CreateTool(
    displayName: "Pickaxe",
    description: "A tool for mining resources.",
    durability: 200.0f,
    value: 50.0f
);

// Add to the database
ItemDatabase.Instance.AddItem("IronOre", ironOre);
ItemDatabase.Instance.AddItem("Pickaxe", pickaxe);
```

### Working with Categories
```csharp
// Get all raw materials
var rawMaterials = ItemDatabase.Instance.GetItemsByCategory("RawMaterial");

// Display all raw materials
foreach (var pair in rawMaterials)
{
    Debug.Log($"ID: {pair.Key}, Name: {pair.Value.DisplayName}");
}
```

## Saving and Loading Items
To save or load items, add the ItemDatabaseManager component to a GameObject in your scene:

```csharp
// Get the manager component
ItemDatabaseManager manager = GetComponent<ItemDatabaseManager>();

// Save items to file
manager.SaveItemsToFile();

// Load items from file
manager.LoadItemsFromFile();
```

## Editor Tools
To access the Item Database Editor, go to Tools > Item Database Editor in the Unity menu. This provides a user-friendly interface to manage your items without writing code.

## Integration with Recipes
This Item System works seamlessly with the Recipe System. The recipe system references items by their string IDs, which correspond to the item IDs in this database.

## Future Expansion
The system is designed to be easily expandable. Some potential future enhancements:
- Item quality levels or rarities
- Item crafting requirements
- Item usage effects and animations
- Item equipment slots and attachment points
- Item decay or expiration mechanics
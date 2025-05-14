using UnityEngine;

namespace V2.Data
{
    /// <summary>
    /// Utility class for creating common item types.
    /// </summary>
    public static class ItemFactory
    {
        /// <summary>
        /// Create a raw material item.
        /// </summary>
        /// <param name="displayName">Human-readable name of the item</param>
        /// <param name="description">Description of the item</param>
        /// <param name="value">Base value of the item</param>
        /// <param name="mass">Mass of the item in kg</param>
        /// <returns>A new ItemDefinition configured as a raw material</returns>
        public static ItemDefinition CreateRawMaterial(string displayName, string description, float value = 1.0f, float mass = 1.0f)
        {
            ItemDefinition item = new ItemDefinition(
                id: displayName,
                displayName: displayName,
                description: description,
                category: "RawMaterial",
                stackable: true,
                maxStackSize: 50
            );
       
            
            return item;
        }
        
        /// <summary>
        /// Create a processed material item.
        /// </summary>
        /// <param name="displayName">Human-readable name of the item</param>
        /// <param name="description">Description of the item</param>
        /// <param name="value">Base value of the item</param>
        /// <param name="mass">Mass of the item in kg</param>
        /// <returns>A new ItemDefinition configured as a processed material</returns>
        public static ItemDefinition CreateProcessedMaterial(string displayName, string description, float value = 2.0f, float mass = 0.8f)
        {
            ItemDefinition item = new ItemDefinition(
                id: displayName,
                displayName: displayName,
                description: description,
                category: "ProcessedMaterial",
                stackable: true,
                maxStackSize: 100
            );
     
            return item;
        }
        
        /// <summary>
        /// Create a component item.
        /// </summary>
        /// <param name="displayName">Human-readable name of the item</param>
        /// <param name="description">Description of the item</param>
        /// <param name="value">Base value of the item</param>
        /// <param name="mass">Mass of the item in kg</param>
        /// <returns>A new ItemDefinition configured as a component</returns>
        public static ItemDefinition CreateComponent(string displayName, string description, float value = 5.0f, float mass = 0.5f)
        {
            ItemDefinition item = new ItemDefinition(
                id: displayName,
                displayName: displayName,
                description: description,
                category: "Component",
                stackable: true,
                maxStackSize: 50
            );
            
    
            return item;
        }
        
        /// <summary>
        /// Create a tool item.
        /// </summary>
        /// <param name="displayName">Human-readable name of the item</param>
        /// <param name="description">Description of the item</param>
        /// <param name="durability">Durability of the tool</param>
        /// <param name="value">Base value of the item</param>
        /// <returns>A new ItemDefinition configured as a tool</returns>
        public static ItemDefinition CreateTool(string displayName, string description, float durability = 100.0f, float value = 20.0f)
        {
            ItemDefinition item = new ItemDefinition(
                id: displayName,
                displayName: displayName,
                description: description,
                category: "Tool",
                stackable: false,
                maxStackSize: 1
            );
            
     
            return item;
        }
        
        /// <summary>
        /// Create a consumable item.
        /// </summary>
        /// <param name="displayName">Human-readable name of the item</param>
        /// <param name="description">Description of the item</param>
        /// <param name="effect">Effect value when consumed</param>
        /// <param name="value">Base value of the item</param>
        /// <returns>A new ItemDefinition configured as a consumable</returns>
        public static ItemDefinition CreateConsumable(string displayName, string description, float effect = 10.0f, float value = 5.0f)
        {
            ItemDefinition item = new ItemDefinition(
                id: displayName,
                displayName: displayName,
                description: description,
                category: "Consumable",
                stackable: true,
                maxStackSize: 10
            );
            
            return item;
        }
        
        /// <summary>
        /// Create a fuel item.
        /// </summary>
        /// <param name="displayName">Human-readable name of the item</param>
        /// <param name="description">Description of the item</param>
        /// <param name="energyValue">Energy value of the fuel</param>
        /// <param name="value">Base value of the item</param>
        /// <returns>A new ItemDefinition configured as a fuel</returns>
        public static ItemDefinition CreateFuel(string displayName, string description, float energyValue = 50.0f, float value = 3.0f)
        {
            ItemDefinition item = new ItemDefinition(
                id: displayName,
                displayName: displayName,
                description: description,
                category: "Fuel",
                stackable: true,
                maxStackSize: 50
            );
            
            return item;
        }
    }
}
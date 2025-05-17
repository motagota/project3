using UnityEngine;

namespace V2.Data
{
    public static class ItemFactory
    {
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
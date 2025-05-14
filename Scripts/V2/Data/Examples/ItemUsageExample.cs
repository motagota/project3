using System.Collections.Generic;
using UnityEngine;

namespace V2.Data.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the Item Database system.
    /// </summary>
    public class ItemUsageExample : MonoBehaviour
    {
        // Reference to the item database
        private ItemDatabase _itemDB;
        
        // Start is called before the first frame update
        void Start()
        {
            // Get the item database singleton
            _itemDB = ItemDatabase.Instance;
            
            // Log all available items
            LogAvailableItems();
            
            // Create and add a custom item
            CreateCustomItem();
            
            // Find items by category
            FindItemsByCategory("RawMaterial");
            
            // Use the ItemFactory to create items
            UseItemFactory();
           
        }
        
        /// <summary>
        /// Log all available items in the database.
        /// </summary>
        private void LogAvailableItems()
        {
            Debug.Log("=== Available Items ===");
            
            Dictionary<string, ItemDefinition> allItems = _itemDB.GetAllItems();
            
            foreach (var pair in allItems)
            {
                Debug.Log($"ID: {pair.Key}, Name: {pair.Value.DisplayName}, Category: {pair.Value.Category}");
            }
            
            Debug.Log("=== End of Items ===");
        }
        
        /// <summary>
        /// Create and add a custom item to the database.
        /// </summary>
        private void CreateCustomItem()
        {
            // Create a new item definition
            ItemDefinition customItem = new ItemDefinition(
                id: "AdvancedCircuit",
                displayName: "Advanced Circuit",
                description: "A complex electronic circuit used in advanced machinery.",
                category: "Component",
                stackable: true,
                maxStackSize: 20
            );
         
            // Add to the database
            _itemDB.AddItem("AdvancedCircuit", customItem);
            Debug.Log("Added custom item: AdvancedCircuit");
        }
        
        /// <summary>
        /// Find and log items by category.
        /// </summary>
        /// <param name="category">The category to search for</param>
        private void FindItemsByCategory(string category)
        {
            Debug.Log($"=== Items in Category: {category} ===");
            
            var items = _itemDB.GetItemsByCategory(category);
            
            foreach (var pair in items)
            {
                Debug.Log($"ID: {pair.Key}, Name: {pair.Value.DisplayName}");
            }
            
            Debug.Log($"=== End of {category} Items ===");
        }
        
        /// <summary>
        /// Demonstrate using the ItemFactory.
        /// </summary>
        private void UseItemFactory()
        {
            Debug.Log("=== Using ItemFactory ===");
            
            // Create a tool
            ItemDefinition drill = ItemFactory.CreateTool(
                displayName: "Mining Drill",
                description: "An advanced tool for extracting resources.",
                durability: 500.0f,
                value: 100.0f
            );
            
            // Create a fuel
            ItemDefinition coal = ItemFactory.CreateFuel(
                displayName: "Coal",
                description: "A combustible black rock used as fuel.",
                energyValue: 50.0f,
                value: 2.0f
            );
            
            // Add to the database
            _itemDB.AddItem("MiningDrill", drill);
            _itemDB.AddItem("Coal", coal);
            
            
            Debug.Log("=== End of ItemFactory Demo ===");
        }
        
    }
}
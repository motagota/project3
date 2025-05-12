using System.Collections.Generic;
using UnityEngine;

namespace V2.Data
{
    /// <summary>
    /// A database class that stores and manages item definitions for the simulation.
    /// </summary>
    public class ItemDatabase
    {
        private static ItemDatabase _instance;
        private Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();
        
        // Singleton pattern to ensure only one item database exists
        public static ItemDatabase Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ItemDatabase();
                    _instance.InitializeDefaultItems();
                }
                return _instance;
            }
        }
        
        private ItemDatabase()
        {
            // Private constructor to enforce singleton pattern
        }
        
        /// <summary>
        /// Initialize the database with default items.
        /// </summary>
        private void InitializeDefaultItems()
        {
            // Raw materials
            AddItem("IronOre", new ItemDefinition(
                id: "1",
                displayName: "Iron Ore",
                description: "Raw iron ore extracted from the ground.",
                category: "RawMaterial",
                stackable: true,
                maxStackSize: 50
            ));
            
            AddItem("CopperOre", new ItemDefinition(
                id: "2",
                displayName: "Copper Ore",
                description: "Raw copper ore extracted from the ground.",
                category: "RawMaterial",
                stackable: true,
                maxStackSize: 50
            ));
            
            // Processed materials
            AddItem("IronPlate", new ItemDefinition(
                id: "3",
                displayName: "Iron Plate",
                description: "Processed iron plate used in manufacturing.",
                category: "ProcessedMaterial",
                stackable: true,
                maxStackSize: 100
            ));
            
            AddItem("CopperWire", new ItemDefinition(
                id: "4",
                displayName: "Copper Wire",
                description: "Processed copper wire used in electronics.",
                category: "ProcessedMaterial",
                stackable: true,
                maxStackSize: 200
            ));
            
            // Components
            AddItem("Circuit", new ItemDefinition(
                id: "5",
                displayName: "Basic Circuit",
                description: "A basic electronic circuit.",
                category: "Component",
                stackable: true,
                maxStackSize: 50
            ));
            
            AddItem("IronGear", new ItemDefinition(
                id: "6",
                displayName: "Iron Gear",
                description: "A mechanical gear made of iron.",
                category: "Component",
                stackable: true,
                maxStackSize: 50
            ));
            
        }
        
        /// <summary>
        /// Add an item to the database.
        /// </summary>
        /// <param name="itemId">Unique identifier for the item</param>
        /// <param name="itemDefinition">The item definition to add</param>
        public void AddItem(string itemId, ItemDefinition itemDefinition)
        {
            if (_items.ContainsKey(itemId))
            {
                Debug.LogWarning($"Item with ID {itemId} already exists and will be overwritten.");
            }
            
            _items[itemId] = itemDefinition;
        }
        
        /// <summary>
        /// Get an item definition by its ID.
        /// </summary>
        /// <param name="itemId">The ID of the item to retrieve</param>
        /// <returns>The item definition if found, null otherwise</returns>
        public ItemDefinition GetItem(string itemId)
        {
            if (_items.TryGetValue(itemId, out ItemDefinition item))
            {
                return item;
            }
            
            Debug.LogWarning($"Item with ID {itemId} not found.");
            return null;
        }
        
        /// <summary>
        /// Get all items in a specific category.
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <returns>A list of items in the specified category</returns>
        public List<KeyValuePair<string, ItemDefinition>> GetItemsByCategory(string category)
        {
            List<KeyValuePair<string, ItemDefinition>> result = new List<KeyValuePair<string, ItemDefinition>>();
            
            foreach (var pair in _items)
            {
                if (pair.Value.Category == category)
                {
                    result.Add(pair);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Check if an item with the specified ID exists in the database.
        /// </summary>
        /// <param name="itemId">The ID to check</param>
        /// <returns>True if the item exists, false otherwise</returns>
        public bool HasItem(string itemId)
        {
            return _items.ContainsKey(itemId);
        }
        
        /// <summary>
        /// Get all item IDs in the database.
        /// </summary>
        /// <returns>A list of all item IDs</returns>
        public List<string> GetAllItemIds()
        {
            return new List<string>(_items.Keys);
        }
        
        /// <summary>
        /// Get all items in the database.
        /// </summary>
        /// <returns>A dictionary of all items</returns>
        public Dictionary<string, ItemDefinition> GetAllItems()
        {
            return new Dictionary<string, ItemDefinition>(_items);
        }
        
        /// <summary>
        /// Clear all items from the database.
        /// </summary>
        public void ClearItems()
        {
            _items.Clear();
        }
    }
}